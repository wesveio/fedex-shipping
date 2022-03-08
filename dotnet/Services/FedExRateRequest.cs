namespace ShippingUtilities.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using FedExRateServiceReference;
    using Newtonsoft.Json;
    using ShippingUtilities.Data;
    using ShippingUtilities.Models;

    public class FedExRateRequest : IFedExRateRequest
    {
        private const string CARRIER = "FedEx";
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private MerchantSettings _merchantSettings;

        public FedExRateRequest(IMerchantSettingsRepository merchantSettingsRepository)
        {
            this._merchantSettingsRepository = merchantSettingsRepository ??
                                            throw new ArgumentNullException(nameof(merchantSettingsRepository));
        }

        public async Task<GetRatesResponseWrapper> GetRates(GetRatesRequest getRatesRequest)
        {
            this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings(CARRIER);
            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
            RateRequest request = await CreateRateRequest(getRatesRequest);
            //RateService service = new RateService();
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

            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                //RateReply reply = await service.getRates(request);
                //string jsonrequest = JsonConvert.SerializeObject(request);
                //Console.WriteLine(jsonrequest);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                // Console.WriteLine(JsonConvert.SerializeObject(getRatesRequest));
                // Console.WriteLine(JsonConvert.SerializeObject(request));
                getRatesResponse ratesResponse = await client.getRatesAsync(request);
                stopWatch.Stop();
                // Console.Write(JsonConvert.SerializeObject(ratesResponse));
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;
                getRatesResponseWrapper.timeSpan = ts;
                Console.WriteLine($"{{\"__VTEX_IO_LOG\":true, \"service\":\"fedex\",  \"ttl\":{ts.TotalMilliseconds}}}");
                Console.WriteLine($"Elapsed = {ts.TotalMilliseconds} = {ts.Seconds}(seconds)");
                RateReply reply = ratesResponse.RateReply;
                getRatesResponseWrapper.HighestSeverity = reply.HighestSeverity.ToString();
                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowRateReply(reply);
                    //string replyjson = JsonConvert.SerializeObject(reply);
                    int totalQuantity = 0;
                    foreach (Item item in getRatesRequest.items) {
                        totalQuantity += item.quantity;
                    }
                    foreach(RateReplyDetail detail in reply.RateReplyDetails)
                    {
                        //Item matchingItem = getRatesRequest.items.Select(x => x.id.Equals())
                        //{
                        //    quantity = getRatesRequest.items[0].quantity,
                        //    time = $"{this.TransitDays(detail.ServiceType, detail.TransitTime.ToString(), detail.DeliveryTimestamp.ToString())}.00:00:00"
                        //};
                        TimeSpan transitArrival = detail.DeliveryTimestamp - getRatesRequest.shippingDateUTC;
                        string transitString = new TimeSpan(transitArrival.Days, transitArrival.Hours, transitArrival.Minutes, transitArrival.Seconds).ToString();
                        GetRatesResponse rateResponse = new GetRatesResponse
                        {
                            carrierId = "FEDEX",
                            //dockId = getRatesRequest.items[0].availability[0].dockId,
                            itemId = getRatesRequest.items[0].id,
                            price = detail.RatedShipmentDetails[0].ShipmentRateDetail.TotalNetCharge.Amount,
                            numberOfPackages = totalQuantity,
                            estimateDate = detail.DeliveryTimestamp,
                            shippingMethod = detail.ServiceDescription.Description,
                            transitTime = transitString,
                            carrierSchedule = new List<Schedule>(),
                            deliveryChannel = "delivery",
                            weekendAndHolidays = new WeekendAndHolidays(),
                            pickupAddress = null,
                        };

                        rateResponse.carrierBusinessHours = new BusinessHour[7];
                        for (int day = 0; day < 7; day++) {
                            rateResponse.carrierBusinessHours[day] = new BusinessHour((DayOfWeek) day, new TimeSpan(0, 0, 0).ToString(), new TimeSpan(23, 59, 59).ToString());
                        }

                        getRatesResponseWrapper.GetRatesResponses.Add(rateResponse);
                    }

                    getRatesResponseWrapper.Success = true;
                }

                ShowNotifications(reply);
                //getRatesResponseWrapper.Notifications = new List<Notification>();
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
                getRatesResponseWrapper.Error = e.Message;
            }

            return getRatesResponseWrapper;
        }

        public async Task<GetRatesResponseWrapper> GetRatesSeparate(GetRatesRequest getRatesRequest)
        {
            this._merchantSettings = await _merchantSettingsRepository.GetMerchantSettings(CARRIER);
            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
            for (int ratesCount = 0; ratesCount < getRatesRequest.items.Count; ratesCount++)
            {
                GetRatesRequest tempRequest = new GetRatesRequest
                {
                    destination = getRatesRequest.destination,
                    origin = getRatesRequest.origin,
                    items = new List<Item> { getRatesRequest.items[ratesCount] }
                };

                RateRequest request = await CreateRateRequest(tempRequest);
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

                try
                {
                    // Call the web service passing in a RateRequest and returning a RateReply
                    getRatesResponse ratesResponse = await client.getRatesAsync(request);
                    RateReply reply = ratesResponse.RateReply;
                    getRatesResponseWrapper.HighestSeverity = reply.HighestSeverity.ToString();
                    if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                    {
                        ShowRateReply(reply);
                        foreach (RateReplyDetail detail in reply.RateReplyDetails)
                        {
                            //Item matchingItem = getRatesRequest.items.Select(x => x.id.Equals())
                            GetRatesResponse rateResponse = new GetRatesResponse
                            {
                                carrierId = detail.ServiceDescription.Code,
                                //deliveryOnWeekends = false,
                                //dockId = tempRequest.items[0].availability[0].dockId,
                                //wareHouseId = tempRequest.items[0].availability[0].warehouseId,
                                //itemId = tempRequest.items[0].id,
                                price = detail.RatedShipmentDetails[0].ShipmentRateDetail.TotalNetCharge.Amount,
                                //quantity = tempRequest.items[0].quantity,
                                // time = $"{this.TransitDays(detail.ServiceType, detail.TransitTime.ToString(), detail.DeliveryTimestamp.ToString())}.00:00:00",
                                shippingMethod = detail.ServiceType
                            };

                            getRatesResponseWrapper.GetRatesResponses.Add(rateResponse);
                        }

                        getRatesResponseWrapper.Success = true;
                    }

                    ShowNotifications(reply);
                    //getRatesResponseWrapper.Notifications = new List<Notification>();
                    foreach (Notification notification in reply.Notifications)
                    {
                        getRatesResponseWrapper.Notifications.Add(notification);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception: {e.Message}");
                    Console.WriteLine($"Exception: {e.InnerException}");
                    Console.WriteLine($"Exception: {e.StackTrace}");
                    getRatesResponseWrapper.Error = e.Message;
                }
            }

            return getRatesResponseWrapper;
        }

        private async Task<RateRequest> CreateRateRequest(GetRatesRequest getRatesRequest)
        {
            // Build the RateRequest
            RateRequest request = new RateRequest();
            //
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
            
            request.TransactionDetail = new TransactionDetail();
            request.TransactionDetail.CustomerTransactionId = "***Rate Available Services Request ***"; // This is a reference field for the customer.  Any value can be used and will be provided in the response.
            
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
            request.RequestedShipment.Shipper.Address.PostalCode = getRatesRequest.origin.zipCode;
            request.RequestedShipment.Shipper.Address.CountryCode = getRatesRequest.origin.country;
            request.RequestedShipment.Shipper.Address.City = getRatesRequest.origin.city;
            request.RequestedShipment.Shipper.Address.StateOrProvinceCode = getRatesRequest.origin.state;
            request.RequestedShipment.Shipper.Address.StreetLines = new string[] { getRatesRequest.origin.street };
            request.RequestedShipment.Shipper.Address.ResidentialSpecified = getRatesRequest.origin.residential;
            request.RequestedShipment.Shipper.Address.Residential = getRatesRequest.origin.residential;
            // request.RequestedShipment.Shipper.Address.GeographicCoordinates = getRatesRequest.origin.coordinates.latitude.ToString("+#.###;-#.###;0") + getRatesRequest.origin.coordinates.longitude.ToString("+#.###;-#.###;0");
        }

        private void SetDestination(RateRequest request, GetRatesRequest getRatesRequest)
        {
            request.RequestedShipment.Recipient = new Party();
            request.RequestedShipment.Recipient.Address = new Address();
            request.RequestedShipment.Recipient.Address.PostalCode = getRatesRequest.destination.zipCode;
            request.RequestedShipment.Recipient.Address.CountryCode = getRatesRequest.destination.country;
            request.RequestedShipment.Recipient.Address.City = getRatesRequest.destination.city;
            request.RequestedShipment.Recipient.Address.StateOrProvinceCode = getRatesRequest.destination.state;
            request.RequestedShipment.Recipient.Address.StreetLines = new string[] { getRatesRequest.destination.street };
            request.RequestedShipment.Recipient.Address.ResidentialSpecified = getRatesRequest.destination.residential;
            request.RequestedShipment.Recipient.Address.Residential = getRatesRequest.destination.residential;
            // request.RequestedShipment.Recipient.Address.GeographicCoordinates = getRatesRequest.destination.coordinates.latitude.ToString("+#.###;-#.###;0") + getRatesRequest.destination.coordinates.longitude.ToString("+#.###;-#.###;0");
        }

        private void SetPackageLineItems(RateRequest request, GetRatesRequest getRatesRequest)
        {
            request.RequestedShipment.RequestedPackageLineItems = new RequestedPackageLineItem[getRatesRequest.items.Count];
            for(int cnt = 0; cnt < getRatesRequest.items.Count; cnt++ )
            {
                request.RequestedShipment.RequestedPackageLineItems[cnt] = new RequestedPackageLineItem();
                request.RequestedShipment.RequestedPackageLineItems[cnt].SequenceNumber = getRatesRequest.items[cnt].id;
                request.RequestedShipment.RequestedPackageLineItems[cnt].GroupPackageCount = getRatesRequest.items[cnt].quantity.ToString();
                //request.RequestedShipment.RequestedPackageLineItems[cnt].GroupPackageCount = "1";
                // package weight
                request.RequestedShipment.RequestedPackageLineItems[cnt].Weight = new Weight();
                request.RequestedShipment.RequestedPackageLineItems[cnt].Weight.Units = WeightUnits.LB;
                request.RequestedShipment.RequestedPackageLineItems[cnt].Weight.UnitsSpecified = true;
                //request.RequestedShipment.RequestedPackageLineItems[cnt].Weight.Value = getRatesRequest.items[cnt].unitDimension.weight * getRatesRequest.items[cnt].quantity;
                request.RequestedShipment.RequestedPackageLineItems[cnt].Weight.Value = Convert.ToDecimal(getRatesRequest.items[cnt].unitDimension.weight);
                request.RequestedShipment.RequestedPackageLineItems[cnt].Weight.ValueSpecified = true;
                // package dimensions
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions = new Dimensions();
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions.Length = Math.Ceiling(getRatesRequest.items[cnt].unitDimension.length).ToString();
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions.Width = Math.Ceiling(getRatesRequest.items[cnt].unitDimension.width).ToString();
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions.Height = Math.Ceiling(getRatesRequest.items[cnt].unitDimension.height).ToString();
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions.Units = LinearUnits.IN;
                request.RequestedShipment.RequestedPackageLineItems[cnt].Dimensions.UnitsSpecified = true;
                
                //Special Handling goods
                if (!String.IsNullOrEmpty(getRatesRequest.items[cnt].modal)) {
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested = new PackageSpecialServicesRequested();
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested.SpecialServiceTypes = new String[] {"DANGEROUS_GOODS"};
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested.DangerousGoodsDetail = new DangerousGoodsDetail();
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested.DangerousGoodsDetail.Offeror = "TEST OFFEROR";
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested.DangerousGoodsDetail.EmergencyContactNumber = "3268545905";
                    request.RequestedShipment.RequestedPackageLineItems[cnt].SpecialServicesRequested.DangerousGoodsDetail.Options = new HazardousCommodityOptionType[] { HazardousCommodityOptionType.HAZARDOUS_MATERIALS };
                }
            }
        }

        private void ShowRateReply(RateReply reply)
        {
            Console.WriteLine("RateReply details:");
            for (int i = 0; i < reply.RateReplyDetails.Length; i++)
            {
                RateReplyDetail rateReplyDetail = reply.RateReplyDetails[i];
                Console.WriteLine("Rate Reply Detail for Service {0} ", i + 1);
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
                Console.WriteLine("**********************************************************");
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
            Console.WriteLine("Notifications");
            for (int i = 0; i < reply.Notifications.Length; i++)
            {
                Notification notification = reply.Notifications[i];
                Console.WriteLine("Notification no. {0}", i);
                Console.WriteLine(" Severity: {0}", notification.Severity);
                Console.WriteLine(" Code: {0}", notification.Code);
                Console.WriteLine(" Message: {0}", notification.Message);
                Console.WriteLine(" Source: {0}", notification.Source);
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

        private TimeSpan CalculateTransitTime(DateTime deliveryTimestamp)
        {
            return deliveryTimestamp.Date - DateTime.Today;
        }

        public async Task<RateReplyWrapper> GetRawRates(RateRequest rateRequest)
        {
            RatePortTypeClient client;
            RateReplyWrapper rateReply = new RateReplyWrapper();
            RateReply reply = new RateReply();
            //if (this._merchantSettings.IsLive)
            //{
            //    string remoteAddress = "https://ws.fedex.com:443/web-services";
            //    client = new RatePortTypeClient(RatePortTypeClient.EndpointConfiguration.RateServicePort, remoteAddress);
            //}
            //else
            {
                client = new RatePortTypeClient();
            }

            try
            {
                // Call the web service passing in a RateRequest and returning a RateReply
                //RateReply reply = await service.getRates(request);
                //string jsonrequest = JsonConvert.SerializeObject(request);
                //Console.WriteLine(jsonrequest);
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                getRatesResponse ratesResponse = await client.getRatesAsync(rateRequest);
                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;
                reply = ratesResponse.RateReply;
                rateReply.rateReply = reply;
                rateReply.timeSpan = ts;

                Console.WriteLine($"{{\"__VTEX_IO_LOG\":true, \"service\":\"fedex\",  \"ttl\":{ts.TotalMilliseconds}}}");

                if (reply.HighestSeverity == NotificationSeverityType.SUCCESS || reply.HighestSeverity == NotificationSeverityType.NOTE || reply.HighestSeverity == NotificationSeverityType.WARNING)
                {
                    ShowRateReply(reply);
                }

                ShowNotifications(reply);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                rateReply.message = e.Message;
            }

            return rateReply;
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
