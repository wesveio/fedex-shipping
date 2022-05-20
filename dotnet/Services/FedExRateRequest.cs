namespace FedexShipping.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using FedExRateServiceReference;
    using Newtonsoft.Json;
    using FedexShipping.Data;
    using FedexShipping.Models;
    using Vtex.Api.Context;

    public class FedExRateRequest : IFedExRateRequest
    {
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExCacheRepository _fedExCacheRespository;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private MerchantSettings _merchantSettings;

        private Dictionary<string, string> iso2CodeMap = new Dictionary<string, string>(){
            {"USA", "US"}
        };

        private Dictionary<string, string> modalOptionsMap = new Dictionary<string, string>()
        {
            {"CHEMICALS", "HAZARDOUS_MATERIALS"},
            {"ELECTRONICS", "BATTERY"},
            {"FURNITURE", "NONE"},
            {"GLASS", "NONE"},
            {"LIQUID", "HAZARDOUS_MATERIALS"},
            {"MATTRESSES", "NONE"},
            {"REFRIGERATED", "NONE"},
            {"TIRES", "NONE"},
            {"WHITE_GOODS", "NONE"},
            {"FIREARMS", "ORM_D"},
        };

        public FedExRateRequest(IVtexEnvironmentVariableProvider environmentVariableProvider, IMerchantSettingsRepository merchantSettingsRepository, IIOServiceContext context, IFedExCacheRepository fedExCacheRepository)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                throw new ArgumentNullException(nameof(environmentVariableProvider));
            this._merchantSettingsRepository = merchantSettingsRepository ??
                throw new ArgumentNullException(nameof(merchantSettingsRepository));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._fedExCacheRespository = fedExCacheRepository ?? throw new ArgumentNullException(nameof(fedExCacheRepository));
        }

        public async Task<GetRatesResponseWrapper> GetRates(GetRatesRequest getRatesRequest)
        {
            this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings();
            GetRatesResponseWrapper getRatesResponseWrapperParent = new GetRatesResponseWrapper();
            getRatesResponseWrapperParent.Success = true;
            
            GetRatesResponseWrapper fedexCachedResponse;
            int cacheKey = $@"
                {_context.Vtex.App.Version}
                {JsonConvert.SerializeObject(this._merchantSettings)}
                {getRatesRequest.origin.zipCode}
                {getRatesRequest.destination.zipCode}
                {JsonConvert.SerializeObject(getRatesRequest.items)}"
                .GetHashCode();

            if (_fedExCacheRespository.TryGetCache(cacheKey, out fedexCachedResponse)) {
                getRatesResponseWrapperParent = fedexCachedResponse;
                _context.Vtex.Logger.Info("GetRates", "Cache Used", 
                    "Cached Result", 
                    new[]
                    {
                        ( "entireObject", JsonConvert.SerializeObject(fedexCachedResponse)),
                    }
                );
            }
            else
            {
                Dictionary<String, List<Item>> splitItems = SplitRequestItemsByModal(getRatesRequest);
                
                RatePortTypeClient client;
                if (this._merchantSettings.IsLive)
                {
                    string remoteAddress = "https://ws.fedex.com:443/web-services";
                    client = new RatePortTypeClient(RatePortTypeClient.EndpointConfiguration.RateServicePort, remoteAddress);
                }
                else
                {
                    client = new RatePortTypeClient();
                }

                Dictionary<string, SlaSettings> slaMapping = new Dictionary<string, SlaSettings>();

                foreach (SlaSettings slaSettings in this._merchantSettings.SlaSettings) {
                    slaMapping.Add(slaSettings.Sla, slaSettings);
                }

                foreach (KeyValuePair<string, List<Item>> entry in splitItems) {
                    if (entry.Value.Count > 0) {
                        GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
                        getRatesRequest.items = entry.Value;
                        RateRequest request = await CreateRateRequest(getRatesRequest);

                        try
                        {
                            _context.Vtex.Logger.Info("GetRates", "FedEx RatesRequest", 
                                "Mapped RequestedShipment of RateRequest To FedEx", 
                                new[]
                                {
                                    ( "entireObject", JsonConvert.SerializeObject(request.RequestedShipment)),
                                }
                            );
                            Stopwatch stopWatch = new Stopwatch();
                            stopWatch.Start();
                            getRatesResponse ratesResponse = await client.getRatesAsync(request);
                            stopWatch.Stop();
                            TimeSpan ts = stopWatch.Elapsed;
                            _context.Vtex.Logger.Info("GetRates", "FedEx RatesResponse Time", "Time Spent in MS",
                                new[]
                                {
                                    ( "timeLapsed", ts.TotalMilliseconds.ToString()),
                                }
                            );

                            getRatesResponseWrapper.timeSpan = ts;
                            RateReply reply = ratesResponse.RateReply;
                            getRatesResponseWrapperParent.HighestSeverity.Add(reply.HighestSeverity.ToString());
                            if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                            {
                                if (!string.Equals(this._environmentVariableProvider.Workspace, "master")) {
                                    ShowRateReply(reply);
                                }

                                Dictionary<string, double> ratesRatio = CalculateRatesRatio(getRatesRequest.items);
                                foreach(RateReplyDetail detail in reply.RateReplyDetails)
                                {
                                    if (!slaMapping[detail.ServiceDescription.Description].Hidden) {
                                        TimeSpan transitArrival = detail.DeliveryTimestamp - getRatesRequest.shippingDateUTC;
                                        string transitString = new TimeSpan(transitArrival.Days, transitArrival.Hours, transitArrival.Minutes, transitArrival.Seconds).ToString();
                                        foreach (Item item in getRatesRequest.items) {
                                            GetRatesResponse rateResponse = new GetRatesResponse
                                            {
                                                carrierId = "FEDEX",
                                                itemId = item.id,
                                                price = detail.RatedShipmentDetails[0].ShipmentRateDetail.TotalNetCharge.Amount * Convert.ToDecimal(ratesRatio[item.id]),
                                                numberOfPackages = item.quantity,
                                                estimateDate = detail.DeliveryTimestamp,
                                                shippingMethod = detail.ServiceDescription.Description,
                                                transitTime = transitString,
                                                carrierSchedule = new List<Schedule>(),
                                                deliveryChannel = "delivery",
                                                weekendAndHolidays = new WeekendAndHolidays(),
                                                pickupAddress = null,
                                            };

                                            rateResponse.price += Convert.ToDecimal(slaMapping[rateResponse.shippingMethod].SurchargePercent / 100) * rateResponse.price + Convert.ToDecimal(slaMapping[rateResponse.shippingMethod].SurchargeFlatRate);

                                            rateResponse.carrierBusinessHours = new BusinessHour[7];
                                            for (int day = 0; day < 7; day++) {
                                                rateResponse.carrierBusinessHours[day] = new BusinessHour((DayOfWeek) day, new TimeSpan(0, 0, 0).ToString(), new TimeSpan(23, 59, 59).ToString());
                                            }

                                            getRatesResponseWrapper.GetRatesResponses.Add(rateResponse);
                                        }
                                    }
                                }

                                getRatesResponseWrapper.Success = true;
                            }

                            if (!string.Equals(this._environmentVariableProvider.Workspace, "master")) {
                                ShowNotifications(reply);
                            }

                            foreach(Notification notification in reply.Notifications)
                            {
                                getRatesResponseWrapper.Notifications.Add(notification);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Exception: {e.Message}");
                            Console.WriteLine($"Exception: {e.InnerException}");
                            Console.WriteLine($"Exception: {e.StackTrace}");
                            getRatesResponseWrapperParent.Error.Add(e.Message);
                        }

                        getRatesResponseWrapperParent.GetRatesResponses.AddRange(getRatesResponseWrapper.GetRatesResponses);
                        getRatesResponseWrapperParent.Notifications.AddRange(getRatesResponseWrapper.Notifications);
                        getRatesResponseWrapperParent.timeSpan = getRatesResponseWrapper.timeSpan;
                        getRatesResponseWrapperParent.Success = getRatesResponseWrapperParent.Success && getRatesResponseWrapper.Success;
                    }
                }

                // Only cache if the response is successful
                if (getRatesResponseWrapperParent.Success) {
                    await _fedExCacheRespository.SetCache(cacheKey, getRatesResponseWrapperParent);
                }
            }
            return getRatesResponseWrapperParent;
        }

        // Splits the request with respect to FedEx handling types
        private Dictionary<String, List<Item>> SplitRequestItemsByModal(GetRatesRequest getRatesRequest)
        {
            if (this._merchantSettings.ItemModals.Count > 0)
            {
                foreach (ModalMap modalMap in this._merchantSettings.ItemModals)
                {
                    modalOptionsMap[modalMap.Modal] = modalMap.FedexHandling;
                }
            }

            Dictionary<String, List<Item>> handlingItemList = new Dictionary<String, List<Item>>();
            handlingItemList["NONE"] = new List<Item>();
            
            foreach (ModalMap itemModal in this._merchantSettings.ItemModals) {
                if (!itemModal.FedexHandling.Equals("NONE")) {
                    handlingItemList[itemModal.FedexHandling] = new List<Item>();
                }
            }

            foreach (Item item in getRatesRequest.items) {
                if (!string.IsNullOrEmpty(item.modal) && handlingItemList.ContainsKey(modalOptionsMap[item.modal])) {
                    handlingItemList[modalOptionsMap[item.modal]].Add(item);
                } else {
                    handlingItemList["NONE"].Add(item);
                }
            }

            return handlingItemList;
        }
        private async Task<RateRequest> CreateRateRequest(GetRatesRequest getRatesRequest)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();
            request.WebAuthenticationDetail = new WebAuthenticationDetail();
            request.WebAuthenticationDetail.UserCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.UserCredential.Key = this._merchantSettings.UserCredentialKey;
            request.WebAuthenticationDetail.UserCredential.Password = this._merchantSettings.UserCredentialPassword;
            request.WebAuthenticationDetail.ParentCredential = new WebAuthenticationCredential();
            request.WebAuthenticationDetail.ParentCredential.Key = this._merchantSettings.ParentCredentialKey;
            request.WebAuthenticationDetail.ParentCredential.Password = this._merchantSettings.ParentCredentialPassword;
            
            request.ClientDetail = new ClientDetail();
            request.ClientDetail.AccountNumber = this._merchantSettings.ClientDetailAccountNumber;
            request.ClientDetail.MeterNumber = this._merchantSettings.ClientDetailMeterNumber;
            
            // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Rate Available Services Request ***";
            
            request.Version = new VersionId();
            
            request.ReturnTransitAndCommit = true;
            request.ReturnTransitAndCommitSpecified = true;
            SetShipmentDetails(request, getRatesRequest);
            return request;
        }

        private void SetShipmentDetails(RateRequest request, GetRatesRequest getRatesRequest)
        {
            request.RequestedShipment = new RequestedShipment();
            SetOrigin(request, getRatesRequest);
            SetDestination(request, getRatesRequest);
            SetPackageLineItems(request, getRatesRequest);
            request.RequestedShipment.PackageCount = getRatesRequest.items.Sum(x => x.quantity).ToString();
            request.RequestedShipment.PreferredCurrency = getRatesRequest.currency;
            request.RequestedShipment.ShipTimestampSpecified = true;
            request.RequestedShipment.ShipTimestamp = getRatesRequest.shippingDateUTC;
        }

        private void SetOrigin(RateRequest request, GetRatesRequest getRatesRequest)
        {
            request.RequestedShipment.Shipper = new Party();
            request.RequestedShipment.Shipper.Address = new Address();
            request.RequestedShipment.Shipper.Address.PostalCode = getRatesRequest.origin.zipCode.Substring(getRatesRequest.origin.zipCode.Length - 5, 5);
            request.RequestedShipment.Shipper.Address.CountryCode = iso2CodeMap[getRatesRequest.origin.country];
            request.RequestedShipment.Shipper.Address.City = getRatesRequest.origin.city;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = getRatesRequest.origin.state;
            request.RequestedShipment.Shipper.Address.StreetLines = new string[] { getRatesRequest.origin.street };
            request.RequestedShipment.Shipper.Address.ResidentialSpecified = getRatesRequest.origin.residential;
            request.RequestedShipment.Shipper.Address.Residential = getRatesRequest.origin.residential;
        }

        private void SetDestination(RateRequest request, GetRatesRequest getRatesRequest)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.PostalCode = getRatesRequest.destination.zipCode.Substring(getRatesRequest.destination.zipCode.Length - 5, 5);
            request.RequestedShipment.Recipient.Address.CountryCode = iso2CodeMap[getRatesRequest.destination.country];
            request.RequestedShipment.Recipient.Address.City = getRatesRequest.destination.city;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = getRatesRequest.destination.state;
            request.RequestedShipment.Recipient.Address.StreetLines = new string[] { getRatesRequest.destination.street };
            request.RequestedShipment.Recipient.Address.ResidentialSpecified = this._merchantSettings.Residential;
            request.RequestedShipment.Recipient.Address.Residential = this._merchantSettings.Residential;
        }

        // Creates a set for modals with shipAlone
        public HashSet<string> GetShipAlone() {
            HashSet<string> shipAlone = new HashSet<string>();
            foreach (ModalMap itemModal in this._merchantSettings.ItemModals) {
                if (itemModal.ShipAlone) {
                    shipAlone.Add(itemModal.Modal);
                }
            }
            return shipAlone;
        }

        private void SetPackageLineItems(RateRequest request, GetRatesRequest getRatesRequest)
        {
            // Combines all the items into one box
            if (this._merchantSettings.OptimizeShipping) {
                HashSet<string> shipAlone = GetShipAlone();
                int mergedPackageIndex = -1;
                double maxVolume = 0;
                List<RequestedPackageLineItem> packageLines = new List<RequestedPackageLineItem>();
                // Iterates through the list
                // Either combines the items into 1 package
                // Or adds them separately as ship alone
                for (int cnt = 0; cnt < getRatesRequest.items.Count; cnt++) {
                    if (shipAlone.Contains(getRatesRequest.items[cnt].modal)) {
                        packageLines.Add(new RequestedPackageLineItem());
                        packageLines.Last().SequenceNumber = getRatesRequest.items[cnt].id;
                        packageLines.Last().GroupPackageCount = getRatesRequest.items[cnt].quantity.ToString();

                        packageLines.Last().Weight = new Weight();
                        setWeight(packageLines.Last().Weight, getRatesRequest.items[cnt].unitDimension.weight);

                        packageLines.Last().Dimensions = new Dimensions();
                        setDimensions(packageLines.Last().Dimensions, getRatesRequest.items[cnt].unitDimension.length, getRatesRequest.items[cnt].unitDimension.width, getRatesRequest.items[cnt].unitDimension.height);
                        setDimensionUnits(packageLines.Last().Dimensions);

                        // Special Handling goods
                        // Checks if the modal is in the options and there is available mapping
                        if (!string.IsNullOrEmpty(getRatesRequest.items[cnt].modal) && !modalOptionsMap[getRatesRequest.items[cnt].modal].Equals("NONE")) {
                            packageLines.Last().SpecialServicesRequested = new PackageSpecialServicesRequested();
                            setDangerousGoodsDetail(packageLines.Last().SpecialServicesRequested, modalOptionsMap[getRatesRequest.items[cnt].modal]);
                        }
                    } else {
                        // Sets up the item if can be combined
                        if (mergedPackageIndex == -1) {
                            mergedPackageIndex = cnt;
                            packageLines.Add(new RequestedPackageLineItem());

                            packageLines.Last().GroupPackageCount = "1";
                            packageLines.Last().Weight = new Weight();
                            packageLines.Last().Dimensions = new Dimensions();

                            setWeight(packageLines.Last().Weight, 0);
                            setDimensionUnits(packageLines.Last().Dimensions);
                        }

                        packageLines[mergedPackageIndex].Weight.Value += Convert.ToDecimal(getRatesRequest.items[cnt].unitDimension.weight * getRatesRequest.items[cnt].quantity);
                                                
                        double currentItemVolume = Math.Ceiling(getRatesRequest.items[cnt].unitDimension.length) * Math.Ceiling(getRatesRequest.items[cnt].unitDimension.width) * Math.Ceiling(getRatesRequest.items[cnt].unitDimension.height);

                        if (currentItemVolume > maxVolume) {
                            packageLines[mergedPackageIndex].SequenceNumber = getRatesRequest.items[cnt].id;
                            setDimensions(packageLines[mergedPackageIndex].Dimensions, getRatesRequest.items[cnt].unitDimension.length, getRatesRequest.items[cnt].unitDimension.width, getRatesRequest.items[cnt].unitDimension.height);
                            packageLines[mergedPackageIndex].GroupPackageCount = getRatesRequest.items[cnt].quantity.ToString();
                        }

                        // Special Handling goods
                        // Checks if the modal is in the options and there is available mapping
                        if (!string.IsNullOrEmpty(getRatesRequest.items[cnt].modal) && !modalOptionsMap[getRatesRequest.items[cnt].modal].Equals("NONE")) {
                            packageLines[mergedPackageIndex].SpecialServicesRequested = new PackageSpecialServicesRequested();
                            setDangerousGoodsDetail(packageLines[mergedPackageIndex].SpecialServicesRequested, modalOptionsMap[getRatesRequest.items[cnt].modal]);
                        }
                    
                    }
                }
                if (mergedPackageIndex != -1) {
                    packageLines[mergedPackageIndex].Weight.Value /= Convert.ToDecimal(packageLines[mergedPackageIndex].GroupPackageCount);
                }
                request.RequestedShipment.RequestedPackageLineItems = packageLines.ToArray();

            } else {
                request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[getRatesRequest.items.Count];
            
                for(int cnt = 0; cnt < getRatesRequest.items.Count; cnt++ )
                {
                    request.RequestedShipment.RequestedPackageLineItems[cnt] = new RequestedPackageLineItem();
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SequenceNumber = getRatesRequest.items[cnt].id;
                    request.RequestedShipment.RequestedPackageLineItems[cnt].GroupPackageCount = getRatesRequest.items[cnt].quantity.ToString();
                    request.RequestedShipment.RequestedPackageLineItems[cnt].Weight = new Weight();
                    setWeight(request.RequestedShipment.RequestedPackageLineItems[cnt].Weight, getRatesRequest.items[cnt].unitDimension.weight);

                    request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions = new Dimensions();
                    setDimensions(request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions, getRatesRequest.items[cnt].unitDimension.length, getRatesRequest.items[cnt].unitDimension.width, getRatesRequest.items[cnt].unitDimension.height);
                    setDimensionUnits(request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions);
                    
                    // Special Handling goods
                    // Checks if the modal is in the options and there is available mapping
                    if (!string.IsNullOrEmpty(getRatesRequest.items[cnt].modal) && !modalOptionsMap[getRatesRequest.items[cnt].modal].Equals("NONE")) {
                        request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested = new PackageSpecialServicesRequested();
                        setDangerousGoodsDetail(request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested, modalOptionsMap[getRatesRequest.items[cnt].modal]);
                    }
                }
            }
        }

        public void setWeight(Weight weight, double weightAmount) {
            weight.Value = Convert.ToDecimal(weightAmount);
            weight.ValueSpecified = true;
            if (this._merchantSettings.UnitWeight.Equals("G")) {
                this._merchantSettings.UnitWeight = "KG";
                weight.Value /= 1000;
            }
            WeightUnits weightUnits;
            Enum.TryParse<WeightUnits>(this._merchantSettings.UnitWeight, out weightUnits);
            weight.Units = weightUnits;
            weight.UnitsSpecified = true;
        }

        public void setDimensions(Dimensions dimensions, double length, double width, double height) {
            dimensions.Length = Math.Ceiling(length).ToString();
            dimensions.Width = Math.Ceiling(width).ToString();
            dimensions.Height = Math.Ceiling(height).ToString();
        }

        public void setDimensionUnits(Dimensions dimensions) {
            LinearUnits linearUnits;
            Enum.TryParse<LinearUnits>(this._merchantSettings.UnitDimension, out linearUnits);
            dimensions.Units = linearUnits;
            dimensions.UnitsSpecified = true;
        }

        public void setDangerousGoodsDetail(PackageSpecialServicesRequested packageSpecialServicesRequested, string modal) {
            string specialHandlingTypes = "DANGEROUS_GOODS";
            packageSpecialServicesRequested.SpecialServiceTypes = new String[] { specialHandlingTypes };
            packageSpecialServicesRequested.DangerousGoodsDetail = new DangerousGoodsDetail();
            HazardousCommodityOptionType hazOptionType;
            Enum.TryParse<HazardousCommodityOptionType>(modal, out hazOptionType);
            packageSpecialServicesRequested.DangerousGoodsDetail.Offeror = "TEST OFFEROR";
            packageSpecialServicesRequested.DangerousGoodsDetail.EmergencyContactNumber = "3268545905";
            packageSpecialServicesRequested.DangerousGoodsDetail.Options = new HazardousCommodityOptionType[] { hazOptionType };
        }

        private void ShowRateReply(RateReply reply)
        {
            Console.WriteLine("--- RateReply details ---");
            for (int i = 0; i < reply.RateReplyDetails.Length; i++)
            {
                RateReplyDetail rateReplyDetail = reply.RateReplyDetails[i];
                Console.WriteLine("Service Type: {0}", rateReplyDetail.ServiceType);
                Console.WriteLine("Packaging Type: {0}", rateReplyDetail.PackagingType);

                for (int j = 0; j < rateReplyDetail.RatedShipmentDetails.Length; j++)
                {
                    RatedShipmentDetail shipmentDetail = rateReplyDetail.RatedShipmentDetails[j];
                    Console.WriteLine("---Rated Shipment Detail for Rate Type {0}---", j + 1);
                    ShowShipmentRateDetails(shipmentDetail);
                    ShowPackageRateDetails(shipmentDetail.RatedPackages);
                }
                ShowDeliveryDetails(rateReplyDetail);
            }
        }

        private void ShowShipmentRateDetails(RatedShipmentDetail shipmentDetail)
        {
            if (shipmentDetail == null) return;
            if (shipmentDetail.ShipmentRateDetail == null) return;
            ShipmentRateDetail rateDetail = shipmentDetail.ShipmentRateDetail;
            Console.WriteLine("--- Shipment Rate Detail ---");
            //
            Console.WriteLine("RateType: {0} ", rateDetail.RateType);
            if (rateDetail.TotalBillingWeight != null) Console.WriteLine("Total Billing Weight: {0} {1}", rateDetail.TotalBillingWeight.Value, shipmentDetail.ShipmentRateDetail.TotalBillingWeight.Units);
            if (rateDetail.TotalBaseCharge != null) Console.WriteLine("Total Base Charge: {0} {1}", rateDetail.TotalBaseCharge.Amount, rateDetail.TotalBaseCharge.Currency);
            if (rateDetail.TotalFreightDiscounts != null) Console.WriteLine("Total Freight Discounts: {0} {1}", rateDetail.TotalFreightDiscounts.Amount, rateDetail.TotalFreightDiscounts.Currency);
            if (rateDetail.TotalSurcharges != null) Console.WriteLine("Total Surcharges: {0} {1}", rateDetail.TotalSurcharges.Amount, rateDetail.TotalSurcharges.Currency);
            if (rateDetail.Surcharges != null)
            {
                // Individual surcharge for each package
                foreach (Surcharge surcharge in rateDetail.Surcharges)
                    Console.WriteLine(" {0} surcharge {1} {2}", surcharge.SurchargeType, surcharge.Amount.Amount, surcharge.Amount.Currency);
            }
            if (rateDetail.TotalNetCharge != null) Console.WriteLine("Total Net Charge: {0} {1}", rateDetail.TotalNetCharge.Amount, rateDetail.TotalNetCharge.Currency);
        }

        private void ShowPackageRateDetails(RatedPackageDetail[] ratedPackages)
        {
            if (ratedPackages == null) return;
            Console.WriteLine("--- Rated Package Detail ---");
            for (int i = 0; i < ratedPackages.Length; i++)
            {
                RatedPackageDetail ratedPackage = ratedPackages[i];
                Console.WriteLine("Package {0}", i + 1);
                if (ratedPackage.PackageRateDetail != null)
                {
                    Console.WriteLine("Billing weight {0} {1}", ratedPackage.PackageRateDetail.BillingWeight.Value, ratedPackage.PackageRateDetail.BillingWeight.Units);
                    Console.WriteLine("Base charge {0} {1}", ratedPackage.PackageRateDetail.BaseCharge.Amount, ratedPackage.PackageRateDetail.BaseCharge.Currency);
                    if (ratedPackage.PackageRateDetail.TotalSurcharges != null) Console.WriteLine("Total Surcharges: {0} {1}", ratedPackage.PackageRateDetail.TotalSurcharges.Amount, ratedPackage.PackageRateDetail.TotalSurcharges.Currency);
                    if (ratedPackage.PackageRateDetail.Surcharges != null)
                    {
                        foreach (Surcharge surcharge in ratedPackage.PackageRateDetail.Surcharges)
                        {
                            Console.WriteLine(" {0} surcharge {1} {2}", surcharge.SurchargeType, surcharge.Amount.Amount, surcharge.Amount.Currency);
                        }
                    }
                    Console.WriteLine("Net charge {0} {1}", ratedPackage.PackageRateDetail.NetCharge.Amount, ratedPackage.PackageRateDetail.NetCharge.Currency);
                }
            }
        }

        private void ShowDeliveryDetails(RateReplyDetail rateReplyDetail)
        {
            if (rateReplyDetail.DeliveryTimestampSpecified)
                Console.WriteLine("Delivery timestamp: " + rateReplyDetail.DeliveryTimestamp.ToString());
            if (rateReplyDetail.TransitTimeSpecified)
                Console.WriteLine("Transit time: " + rateReplyDetail.TransitTime);
        }

        private void ShowNotifications(RateReply reply)
        {
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Severity: {0}", notification.Severity);
                Console.WriteLine("Code: {0}", notification.Code);
                Console.WriteLine("Message: {0}", notification.Message);
            }
        }

        private string TransitDays(string service, string transitTime, string deliveryTimestamp)
        {
            int transitDays = 30;

            Service serviceEnum;
            if (Enum.TryParse(service, true, out serviceEnum))
            {
                if (Enum.IsDefined(typeof(Service), serviceEnum))
                {
                    switch (serviceEnum)
                    {
                        case Service.FIRST_OVERNIGHT:
                        case Service.PRIORITY_OVERNIGHT:
                        case Service.STANDARD_OVERNIGHT:
                            transitDays = 1;
                            break;
                        case Service.FEDEX_2_DAY_AM:
                        case Service.FEDEX_2_DAY:
                            transitDays = 2;
                            break;
                        case Service.FEDEX_EXPRESS_SAVER:
                            transitDays = 3;
                            break;
                        case Service.FEDEX_GROUND:
                            GroundDays groundDays = (GroundDays)Enum.Parse(typeof(GroundDays), transitTime);
                            transitDays = (int)groundDays;
                            break;
                        case Service.INTERNATIONAL_ECONOMY:
                        case Service.INTERNATIONAL_FIRST:
                        case Service.INTERNATIONAL_PRIORITY:
                            transitDays = CalculateTransitTime(deliveryTimestamp);
                            break;
                        default:
                            Console.WriteLine($"Transit days not set for {service}.");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine($"{service} is not a value of the enum.");
                }
            }
            else
            {
                Console.WriteLine($"{service} is not a member of the enum.");
            }

            return transitDays.ToString();
        }

        private int CalculateTransitTime(string deliveryTimestamp)
        {
            int transitDays = 30;
            DateTime deliveryTimestampParsed;
            if (DateTime.TryParse(deliveryTimestamp, out deliveryTimestampParsed))
            {
                TimeSpan duration = deliveryTimestampParsed.Date - DateTime.Today;
                transitDays = duration.Days;
            }
            else
            {
                Console.WriteLine($"Could not Parse {deliveryTimestamp}.  Expected DateTime.");
            }

            return transitDays;
        }

        private Dictionary<string, double> CalculateRatesRatio(List<Item> items) {
            Dictionary<string, double> itemRatesRatio = new Dictionary<string, double>();

            double totalWeights = 0;
            double totalVolume = 0;
            foreach (Item item in items) {
                totalWeights += item.unitDimension.weight * item.quantity;
                totalVolume += item.unitDimension.length * item.unitDimension.width * item.unitDimension.height * item.quantity;
            }

            foreach (Item item in items) {
                double itemPriceRatio = item.unitDimension.weight / totalWeights;
                double itemVolumeRatio = item.unitDimension.length * item.unitDimension.width * item.unitDimension.height / totalVolume;
                itemRatesRatio.Add(item.id, ((itemPriceRatio + itemVolumeRatio)/2) * item.quantity);
            }

            return itemRatesRatio;
        }

        private TimeSpan CalculateTransitTime(DateTime deliveryTimestamp)
        {
            return deliveryTimestamp.Date - DateTime.Today;
        }

        private enum Service
        {
            UNKNOWN,
            FIRST_OVERNIGHT,
            PRIORITY_OVERNIGHT,
            STANDARD_OVERNIGHT,
            FEDEX_2_DAY_AM,
            FEDEX_2_DAY,
            FEDEX_EXPRESS_SAVER,
            FEDEX_GROUND,
            INTERNATIONAL_FIRST,
            INTERNATIONAL_PRIORITY,
            INTERNATIONAL_ECONOMY
        }

        private enum GroundDays
        {
            ONE_DAY = 1,
            TWO_DAYS = 2,
            THREE_DAYS= 3,
            FOUR_DAYS = 4,
            FIVE_DAYS = 5,
            SIX_DAYS = 6,
            SEVEN_DAYS = 7,
            EIGHT_DAYS = 8,
            NINE_DAYS = 9,
            TEN_DAYS = 10,
            ELEVEN_DAYS = 11,
            TWELVE_DAYS = 12,
            THIRTEEN_DAYS = 13,
            FOURTEEN_DAYS = 14,
            FIFTEEN_DAYS = 15,
            SIXTEEN_DAYS = 16,
            SEVENTEEN_DAYS = 17,
            EIGHTEEN_DAYS = 18,
            NINETEEN_DAYS = 19,
            TWENTY_DAYS = 20,

            UNKNOWN = 30,
        }
    }
}
