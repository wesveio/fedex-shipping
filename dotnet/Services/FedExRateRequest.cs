namespace FedexShipping.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using FedExRateServiceReference;
    using Newtonsoft.Json;
    using FedexShipping.Data;
    using FedexShipping.Models;
    using System.Text.RegularExpressions;
    using Vtex.Api.Context;

    public class FedExRateRequest : IFedExRateRequest
    {
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExCacheRepository _fedExCacheRespository;
        private readonly IPackingService _packingService;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        private MerchantSettings _merchantSettings;

        private readonly Dictionary<string, string> iso2CodeMap = new Dictionary<string, string>(){
            { "ARE", "AE" },
            { "ARG", "AR" },
            { "AUS", "AU" },
            { "AUT", "AT" },
            { "BEL", "BE" },
            { "BGR", "BG" },
            { "BOL", "BO" },
            { "BRA", "BR" },
            { "CAN", "CA" },
            { "CHL", "CL" },
            { "COL", "CO" },
            { "CRI", "CR" },
            { "CZE", "CZ" },
            { "DEU", "DE" },
            { "DNK", "DK" },
            { "ECU", "EC" },
            { "ESP", "ES" },
            { "FIN", "FI" },
            { "FRA", "FR" },
            { "GBR", "GB" },
            { "GTM", "GT" },
            { "HRV", "HR" },
            { "IDN", "ID" },
            { "IND", "IN" },
            { "IRL", "IE" },
            { "ITA", "IT" },
            { "KOR", "KR" },
            { "KWT", "KW" },
            { "MEX", "MX" },
            { "NIC", "NI" },
            { "NLD", "NL" },
            { "NZL", "NZ" },
            { "PER", "PE" },
            { "POL", "PL" },
            { "PRT", "PT" },
            { "PRY", "PY" },
            { "ROU", "RO" },
            { "RUS", "RU" },
            { "SAU", "SA" },
            { "SGP", "SG" },
            { "SLV", "SV" },
            { "SMR", "SM" },
            { "SRB", "RS" },
            { "SVK", "SK" },
            { "SWE", "SE" },
            { "UKR", "UA" },
            { "URY", "UY" },
            { "USA", "US" },
            { "VEN", "VE" },
            { "ZAF", "ZA" }
        };

        private readonly Dictionary<string, string> modalOptionsMap = new Dictionary<string, string>()
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

        public FedExRateRequest(IVtexEnvironmentVariableProvider environmentVariableProvider, IMerchantSettingsRepository merchantSettingsRepository, IIOServiceContext context, IFedExCacheRepository fedExCacheRepository, IPackingService packingService)
        {
            this._environmentVariableProvider = environmentVariableProvider ??
                throw new ArgumentNullException(nameof(environmentVariableProvider));
            this._merchantSettingsRepository = merchantSettingsRepository ??
                throw new ArgumentNullException(nameof(merchantSettingsRepository));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._fedExCacheRespository = fedExCacheRepository ?? throw new ArgumentNullException(nameof(fedExCacheRepository));
            this._packingService = packingService ?? throw new ArgumentNullException(nameof(packingService));
        }

        public async Task<GetRatesResponseWrapper> GetRates(GetRatesRequest getRatesRequest)
        {
            GetRatesResponseWrapper getRatesResponseWrapperParent = new GetRatesResponseWrapper();

            try
            {
                this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings();
                getRatesResponseWrapperParent.Success = true;
                
                getRatesResponseWrapperParent.IsValidCountry = 
                    Constants.POSTAL_CODE_REGEX.ContainsKey(getRatesRequest.destination.country);
                
                GetRatesResponseWrapper fedexCachedResponse;
                int cacheKey = $@"
                    {_context.Vtex.App.Version}
                    {JsonConvert.SerializeObject(this._merchantSettings)}
                    {getRatesRequest.origin.zipCode}
                    {getRatesRequest.destination.zipCode}
                    {JsonConvert.SerializeObject(getRatesRequest.items)}"
                    .GetHashCode();

                if (_fedExCacheRespository.TryGetCache(cacheKey, out fedexCachedResponse))
                {
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
                    Dictionary<string, List<Item>> splitItems = SplitRequestItemsByModal(getRatesRequest);
                    
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

                    foreach (SlaSettings slaSettings in this._merchantSettings.SlaSettings)
                    {
                        slaMapping.Add(slaSettings.Sla, slaSettings);
                    }

                    BusinessHour[] carrierBusinessHours = new BusinessHour[7];
                    for (int day = 0; day < 7; day++)
                    {
                        carrierBusinessHours[day] = new BusinessHour((DayOfWeek) day, new TimeSpan(0, 0, 0).ToString(), new TimeSpan(23, 59, 59).ToString());
                    }

                    // Iterates in parallel through every entry in the different FedEx handling types

                    List<List<Item>> parallelCollection = new List<List<Item>>();

                    foreach (KeyValuePair<string, List<Item>> entry in splitItems)
                    {
                        parallelCollection.Add(entry.Value);
                    }

                    var bag = new ConcurrentBag<GetRatesResponseWrapper>();

                    var tasks = parallelCollection.Select(async itemArr => {
                        if (itemArr.Count > 0)
                        {
                            GetRatesResponseWrapper cell = new GetRatesResponseWrapper();
                            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
                            getRatesRequest.items = itemArr;
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
                                
                                var copyStr = JsonConvert.SerializeObject(getRatesRequest.items);
                                List<Item> copyItemResult = JsonConvert.DeserializeObject<List<Item>>(copyStr);

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

                                _context.Vtex.Logger.Info("reply", "RateReply", "ratesResponse",
                                    new[]
                                    {
                                        ( "reply", JsonConvert.SerializeObject(reply)),
                                    }
                                );

                                cell.HighestSeverity.Add(reply.HighestSeverity.ToString());

                                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                                {
                                    Dictionary<string, double> ratesRatio = CalculateRatesRatio(copyItemResult);

                                    if (reply.RateReplyDetails != null)
                                    {
                                        DateTime defaultDeliveryTimestamp = DateTime.MaxValue;
                                        int defaultDeliveryEstimateInDays = 0;
                                        if (!string.IsNullOrWhiteSpace(this._merchantSettings.DefaultDeliveryEstimateInDays))
                                        {
                                            if (int.TryParse(this._merchantSettings.DefaultDeliveryEstimateInDays, out defaultDeliveryEstimateInDays))
                                            {
                                                defaultDeliveryTimestamp = DateTime.Now.AddDays(defaultDeliveryEstimateInDays).ToUniversalTime();
                                            }
                                        }

                                        foreach (RateReplyDetail detail in reply.RateReplyDetails)
                                        {
                                            if (!slaMapping[detail.ServiceDescription.Description].Hidden)
                                            {
                                                if(!detail.DeliveryTimestampSpecified)
                                                {
                                                    if (defaultDeliveryEstimateInDays > 0)
                                                    {
                                                        detail.DeliveryTimestamp = DateTime.Now.AddDays(defaultDeliveryEstimateInDays).ToUniversalTime();
                                                    }
                                                    else
                                                    {
                                                        break;
                                                    }
                                                }

                                                TimeSpan transitArrival = detail.DeliveryTimestamp - DateTime.Now.ToUniversalTime();
                                                string transitString = new TimeSpan(transitArrival.Days, transitArrival.Hours, transitArrival.Minutes, transitArrival.Seconds).ToString();

                                                foreach (Item item in copyItemResult)
                                                {
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

                                                    rateResponse.carrierBusinessHours = carrierBusinessHours;
                                                    getRatesResponseWrapper.GetRatesResponses.Add(rateResponse);
                                                }
                                            }
                                        }
                                    }

                                    getRatesResponseWrapper.Success = true;
                                }

                                if (!string.Equals(this._environmentVariableProvider.Workspace, "master"))
                                {
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
                                cell.Error.Add(e.Message);

                                _context.Vtex.Logger.Error("GetRates-rateResponse-item", null, "Error:", e);
                            }

                            cell.GetRatesResponses.AddRange(getRatesResponseWrapper.GetRatesResponses);
                            cell.Notifications.AddRange(getRatesResponseWrapper.Notifications);
                            cell.timeSpan = getRatesResponseWrapper.timeSpan;
                            cell.Success = getRatesResponseWrapper.Success;

                            bag.Add(cell);
                        }
                    });

                    await Task.WhenAll(tasks);
                    
                    foreach (var elem in bag)
                    {
                        getRatesResponseWrapperParent.GetRatesResponses.AddRange(elem.GetRatesResponses);
                        getRatesResponseWrapperParent.Notifications.AddRange(elem.Notifications);
                        getRatesResponseWrapperParent.Success = elem.Success && getRatesResponseWrapperParent.Success;
                        getRatesResponseWrapperParent.Error.AddRange(elem.Error);
                        getRatesResponseWrapperParent.HighestSeverity.AddRange(elem.HighestSeverity);
                        getRatesResponseWrapperParent.timeSpan = elem.timeSpan;
                    }
                    // Only cache if the response is successful
                    if (getRatesResponseWrapperParent.Success)
                    {
                        await _fedExCacheRespository.SetCache(cacheKey, getRatesResponseWrapperParent);
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetRates", null, 
                "Error:", ex,
                new[]
                {
                    ( "getRatesRequest", JsonConvert.SerializeObject(getRatesRequest) ),
                });
            }

            return getRatesResponseWrapperParent;
        }

        // Splits the request with respect to FedEx handling types
        private Dictionary<string, List<Item>> SplitRequestItemsByModal(GetRatesRequest getRatesRequest)
        {
            Dictionary<string, List<Item>> handlingItemList = new Dictionary<string, List<Item>>();
            handlingItemList["NONE"] = new List<Item>();
            foreach (ModalMap modalMap in this._merchantSettings.ItemModals)
            {
                modalOptionsMap[modalMap.Modal] = modalMap.FedexHandling;
                if (!modalMap.FedexHandling.Equals("NONE"))
                {
                    handlingItemList[modalMap.FedexHandling] = new List<Item>();
                }
            }

            foreach (Item item in getRatesRequest.items)
            {
                if (!string.IsNullOrEmpty(item.modal) && handlingItemList.ContainsKey(modalOptionsMap[item.modal]))
                {
                    handlingItemList[modalOptionsMap[item.modal]].Add(item);
                }
                else
                {
                    handlingItemList["NONE"].Add(item);
                }
            }

            return handlingItemList;
        }

        private async Task<RateRequest> CreateRateRequest(GetRatesRequest getRatesRequest)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();

            try
            {
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
                await SetShipmentDetails(request, getRatesRequest);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("CreateRateRequest", null, 
                "Error:", ex,
                new[]
                {
                    ( "getRatesRequest", JsonConvert.SerializeObject(getRatesRequest) ),
                });
            }

            return request;
        }

        private async Task SetShipmentDetails(RateRequest request, GetRatesRequest getRatesRequest)
        {
            try
            {
                request.RequestedShipment = new RequestedShipment();
                SetOrigin(request, getRatesRequest);
                SetDestination(request, getRatesRequest);
                await SetPackageLineItems(request, getRatesRequest);
                request.RequestedShipment.PackageCount = getRatesRequest.items.Sum(x => x.quantity).ToString();
                request.RequestedShipment.PreferredCurrency = getRatesRequest.currency;
                request.RequestedShipment.ShipTimestampSpecified = true;
                request.RequestedShipment.ShipTimestamp = getRatesRequest.shippingDateUTC;
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SetShipmentDetails", null, 
                "Error:", ex,
                new[]
                {
                    ( "request", JsonConvert.SerializeObject(request) ),
                    ( "getRatesRequest", JsonConvert.SerializeObject(getRatesRequest) )
                });
            }
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

            Regex rx = new Regex(Constants.POSTAL_CODE_REGEX[getRatesRequest.destination.country], RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Find matches.
            Match match = rx.Match(getRatesRequest.destination.zipCode);

            request.RequestedShipment.Recipient.Address.PostalCode = match.Value;
            request.RequestedShipment.Recipient.Address.CountryCode = iso2CodeMap[getRatesRequest.destination.country];
            request.RequestedShipment.Recipient.Address.City = getRatesRequest.destination.city;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = getRatesRequest.destination.state;
            request.RequestedShipment.Recipient.Address.StreetLines = new string[] { getRatesRequest.destination.street };
            request.RequestedShipment.Recipient.Address.ResidentialSpecified = this._merchantSettings.Residential;
            request.RequestedShipment.Recipient.Address.Residential = this._merchantSettings.Residential;
        }

        // Creates a set for modals with shipAlone
        public HashSet<string> GetShipAlone()
        {
            HashSet<string> shipAlone = new HashSet<string>();
            foreach (ModalMap itemModal in this._merchantSettings.ItemModals)
            {
                if (itemModal.ShipAlone)
                {
                    shipAlone.Add(itemModal.Modal);
                }
            }

            return shipAlone;
        }

        private async Task SetPackageLineItems(RateRequest request, GetRatesRequest getRatesRequest)
        {
            try
            {
                // Combines all the items into one box
                // If 1, then it is pack together in box
                // If 2, then use smart packing
                if (this._merchantSettings.OptimizeShippingType > 0)
                {
                    HashSet<string> shipAlone = GetShipAlone();
                    int mergedPackageIndex = -1;
                    double maxVolume = 0;
                    List<RequestedPackageLineItem> packageLines = new List<RequestedPackageLineItem>();

                    // List of items used for smart packing
                    List<Item> smartPackingList = new List<Item>();

                    // Iterates through the list
                    // Either combines the items into 1 package
                    // Or adds them separately as ship alone
                    for (int cnt = 0; cnt < getRatesRequest.items.Count; cnt++)
                    {
                        if (shipAlone.Contains(getRatesRequest.items[cnt].modal))
                        {
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
                            if (!string.IsNullOrEmpty(getRatesRequest.items[cnt].modal) && !modalOptionsMap[getRatesRequest.items[cnt].modal].Equals("NONE"))
                            {
                                packageLines.Last().SpecialServicesRequested = new PackageSpecialServicesRequested();
                                setDangerousGoodsDetail(packageLines.Last().SpecialServicesRequested, modalOptionsMap[getRatesRequest.items[cnt].modal]);
                            }
                        }
                        else if (this._merchantSettings.OptimizeShippingType == 1)
                        {
                            // Sets up the item if can be combined
                            if (mergedPackageIndex == -1)
                            {
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

                            if (currentItemVolume > maxVolume)
                            {
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
                        } else if (!shipAlone.Contains(getRatesRequest.items[cnt].modal) && this._merchantSettings.OptimizeShippingType == 2) {
                            smartPackingList.Add(getRatesRequest.items[cnt]);
                        }
                    }

                    // For combining items into one box
                    if (mergedPackageIndex != -1)
                    {
                        packageLines[mergedPackageIndex].Weight.Value /= Convert.ToDecimal(packageLines[mergedPackageIndex].GroupPackageCount);
                    }
                    else if (smartPackingList.Capacity > 0)
                    { // For smart packing
                        List<Item> packedResponse = await this._packingService.packingMap(smartPackingList);
                        foreach (Item box in packedResponse)
                        {
                            packageLines.Add(new RequestedPackageLineItem());
                            packageLines.Last().SequenceNumber = box.id;
                            packageLines.Last().GroupPackageCount = box.quantity.ToString();

                            packageLines.Last().Weight = new Weight();
                            setWeight(packageLines.Last().Weight, box.unitDimension.weight);

                            packageLines.Last().Dimensions = new Dimensions();
                            setDimensions(packageLines.Last().Dimensions, box.unitDimension.length, box.unitDimension.width, box.unitDimension.height);
                            setDimensionUnits(packageLines.Last().Dimensions);
                            
                            // Special Handling goods
                            // Checks if the modal is in the options and there is available mapping
                            if (!string.IsNullOrEmpty(box.modal) && !modalOptionsMap[box.modal].Equals("NONE")) {
                                packageLines.Last().SpecialServicesRequested = new PackageSpecialServicesRequested();
                                setDangerousGoodsDetail(packageLines.Last().SpecialServicesRequested, modalOptionsMap[box.modal]);
                            }

                        }
                    }

                    request.RequestedShipment.RequestedPackageLineItems = packageLines.ToArray();
                }
                else
                {
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
                        if (!string.IsNullOrEmpty(getRatesRequest.items[cnt].modal) && !modalOptionsMap[getRatesRequest.items[cnt].modal].Equals("NONE"))
                        {
                            request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested = new PackageSpecialServicesRequested();
                            setDangerousGoodsDetail(request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested, modalOptionsMap[getRatesRequest.items[cnt].modal]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("SetPackageLineItems", null, 
                "Error:", ex,
                new[]
                {
                    ( "request", JsonConvert.SerializeObject(request) ),
                    ( "getRatesRequest", JsonConvert.SerializeObject(getRatesRequest) )
                });
            }
        }

        public void setWeight(Weight weight, double weightAmount)
        {
            weight.Value = Convert.ToDecimal(weightAmount);
            weight.ValueSpecified = true;
            string parseUnit = this._merchantSettings.UnitWeight;
            if (parseUnit.Equals("G"))
            {
                parseUnit = "KG";
                weight.Value /= 1000;
            }
            WeightUnits weightUnits;
            Enum.TryParse<WeightUnits>(parseUnit, out weightUnits);
            weight.Units = weightUnits;
            weight.UnitsSpecified = true;
        }

        public void setDimensions(Dimensions dimensions, double length, double width, double height)
        {
            dimensions.Length = Math.Ceiling(length).ToString();
            dimensions.Width = Math.Ceiling(width).ToString();
            dimensions.Height = Math.Ceiling(height).ToString();
        }

        public void setDimensionUnits(Dimensions dimensions)
        {
            LinearUnits linearUnits;
            Enum.TryParse<LinearUnits>(this._merchantSettings.UnitDimension, out linearUnits);
            dimensions.Units = linearUnits;
            dimensions.UnitsSpecified = true;
        }

        public void setDangerousGoodsDetail(PackageSpecialServicesRequested packageSpecialServicesRequested, string modal)
        {
            string specialHandlingTypes = "DANGEROUS_GOODS";
            packageSpecialServicesRequested.SpecialServiceTypes = new String[] { specialHandlingTypes };
            packageSpecialServicesRequested.DangerousGoodsDetail = new DangerousGoodsDetail();
            HazardousCommodityOptionType hazOptionType;
            Enum.TryParse<HazardousCommodityOptionType>(modal, out hazOptionType);
            packageSpecialServicesRequested.DangerousGoodsDetail.Offeror = "TEST OFFEROR";
            packageSpecialServicesRequested.DangerousGoodsDetail.EmergencyContactNumber = "3268545905";
            packageSpecialServicesRequested.DangerousGoodsDetail.Options = new HazardousCommodityOptionType[] { hazOptionType };
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

        private Dictionary<string, double> CalculateRatesRatio(List<Item> items)
        {
            Dictionary<string, double> itemRatesRatio = new Dictionary<string, double>();

            double totalWeights = 0;
            double totalVolume = 0;
            foreach (Item item in items)
            {
                totalWeights += item.unitDimension.weight * item.quantity;
                totalVolume += item.unitDimension.length * item.unitDimension.width * item.unitDimension.height * item.quantity;
            }

            foreach (Item item in items)
            {
                double itemPriceRatio = item.unitDimension.weight / totalWeights;
                double itemVolumeRatio = item.unitDimension.length * item.unitDimension.width * item.unitDimension.height / totalVolume;
                itemRatesRatio.Add(item.id, ((itemPriceRatio + itemVolumeRatio)/2) * item.quantity);
            }

            return itemRatesRatio;
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
