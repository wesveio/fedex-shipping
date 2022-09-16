namespace service.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using FedexShipping.Data;
    using FedexShipping.Models;
    using FedexShipping.Services;
    using TrackServiceReference;
    using Vtex.Api.Context;
    using System.Collections.Generic;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExRateRequest _fedExRateRequest;
        private readonly IFedExTrackRequest _fedExTrackRequest;
        private readonly IFedExEstimateDeliveryRequest _fedExEstimateDeliveryRequest;
        private readonly IPackingService _packingService;
        private const string FEDEX = "FEDEX";

        public RoutesController(IMerchantSettingsRepository merchantSettingsRepository, IFedExRateRequest fedExRateRequest, IFedExAvailabilityRequest fedExAvailabilityRequest, IFedExTrackRequest fedExTrackRequest, IFedExEstimateDeliveryRequest fedExEstimateDeliveryRequest, IIOServiceContext context, IPackingService packingService)
        {
            this._merchantSettingsRepository = merchantSettingsRepository ??
                                            throw new ArgumentNullException(nameof(merchantSettingsRepository));
            this._fedExRateRequest = fedExRateRequest ??
                                            throw new ArgumentNullException(nameof(fedExRateRequest));
            this._fedExTrackRequest = fedExTrackRequest ??
                                            throw new ArgumentNullException(nameof(fedExTrackRequest));
            this._fedExEstimateDeliveryRequest = fedExEstimateDeliveryRequest ??
                                            throw new ArgumentNullException(nameof(fedExEstimateDeliveryRequest));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
            this._packingService = packingService ?? throw new ArgumentNullException(nameof(packingService));
        }

        public async Task<IActionResult> GetRates()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();

            try
            {
                var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                GetRatesRequest getRatesRequest = JsonConvert.DeserializeObject<GetRatesRequest>(bodyAsText);
                _context.Vtex.Logger.Info("GetRates", "GetRatesRequest", 
                    "Get Rates Request", 
                    new[]
                    {
                        ( "Origin ZipCode", getRatesRequest.origin.zipCode),
                        ( "Destination ZipCode", getRatesRequest.destination.zipCode),
                        ( "Unique Item Count", getRatesRequest.items.Count.ToString()),
                        ( "entireObject", JsonConvert.SerializeObject(getRatesRequest)),
                    }
                );

                getRatesResponseWrapper = await this._fedExRateRequest.GetRates(getRatesRequest);

                if (!getRatesResponseWrapper.IsValidCountry) 
                {
                    Response.StatusCode = 404;
                }

                Response.Headers.Add("Cache-Control", "private");
                Response.Headers.Add("Timespan", getRatesResponseWrapper.timeSpan.TotalSeconds.ToString());

                _context.Vtex.Logger.Info("GetRates", "GetRatesResponse", 
                    "Get Rates Response", 
                    new[]
                    {
                        ( "HighestSeverity", string.Join(",", getRatesResponseWrapper.HighestSeverity)),
                        ( "Success", getRatesResponseWrapper.Success.ToString()),
                        ( "Error", string.Join(",", getRatesResponseWrapper.Error)),
                        ( "entireObject", JsonConvert.SerializeObject(getRatesResponseWrapper)),
                    }
                );

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;
                _context.Vtex.Logger.Info("GetRates", "GetRates Time", "Time Spent in MS",
                    new[]
                    {
                        ( "timeLapsed", ts.TotalMilliseconds.ToString()),
                    }
                );
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetRates", null, "Error:", ex);
            }

            return Json(getRatesResponseWrapper.GetRatesResponses);
        }

        public async Task<IActionResult> TestPack()
        {
            List<Item> response = null;

            try
            {
                var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                List<Item> packingRequest = JsonConvert.DeserializeObject<List<Item>>(bodyAsText);
                response = await this._packingService.packingMap(packingRequest);
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("TestPack", null, "Error:", ex);
            }

            return Json(response);
        }

        public async Task<IActionResult> Track(string carrier, string trackingNumber)
        {
            TrackReply reply = new TrackReply();

            try
            {
                switch (carrier.ToUpper())
                {
                    case FEDEX:
                        reply = await this._fedExTrackRequest.Track(trackingNumber);
                        break;
                }

                Response.Headers.Add("Cache-Control", "private");
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("Track", null, 
                "Error:", ex,
                new[]
                {
                    ( "carrier", carrier ),
                    ( "trackingNumber", trackingNumber )
                });
            }

            return Json(reply);
        }

        public async Task SetMerchantSettings([FromBody] MerchantSettings merchantSettings)
        {
            await this._merchantSettingsRepository.SetMerchantSettings(merchantSettings);
        }

        public async Task<IActionResult> GetMerchantSettings()
        {
            var authenticationResponse = new MerchantSettings();

            try
            {
                authenticationResponse = await this._merchantSettingsRepository.GetMerchantSettings();
                Response.Headers.Add("Cache-Control", "no-cache");
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetMerchantSettings", null, "Error:", ex);
            }

            return Json(authenticationResponse);
        }

        public async Task<IActionResult> GetEstimateDate()
        {
            GetEstimateDeliveryResponse result =  null;

            try
            {
                var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                GetEstimateDeliveryRequest getEstDateReq = JsonConvert.DeserializeObject<GetEstimateDeliveryRequest>(bodyAsText);

                this._fedExEstimateDeliveryRequest.getEstimateDelivery(getEstDateReq);
                result = this._fedExEstimateDeliveryRequest.getEstimateDelivery(getEstDateReq);
                Response.Headers.Add("Cache-Control", "private");
            }
            catch (Exception ex)
            {
                _context.Vtex.Logger.Error("GetEstimateDate", null, "Error:", ex);
            }

            return Json(result);
        }
    }
}
