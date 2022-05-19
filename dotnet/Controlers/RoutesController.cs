namespace service.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using FedExRateServiceReference;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using FedexShipping.Data;
    using FedexShipping.Models;
    using FedexShipping.Services;
    using TrackServiceReference;
    using Vtex.Api.Context;

    public class RoutesController : Controller
    {
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExRateRequest _fedExRateRequest;
        private readonly IFedExAvailabilityRequest _fedExAvailabilityRequest;
        private readonly IFedExTrackRequest _fedExTrackRequest;
        private readonly IFedExEstimateDeliveryRequest _fedExEstimateDeliveryRequest;
        private const string FEDEX = "FEDEX";

        public RoutesController(IMerchantSettingsRepository merchantSettingsRepository, IFedExRateRequest fedExRateRequest, IFedExAvailabilityRequest fedExAvailabilityRequest, IFedExTrackRequest fedExTrackRequest, IFedExEstimateDeliveryRequest fedExEstimateDeliveryRequest, IIOServiceContext context)
        {
            this._merchantSettingsRepository = merchantSettingsRepository ??
                                            throw new ArgumentNullException(nameof(merchantSettingsRepository));
            this._fedExRateRequest = fedExRateRequest ??
                                            throw new ArgumentNullException(nameof(fedExRateRequest));
            this._fedExAvailabilityRequest = fedExAvailabilityRequest ??
                                            throw new ArgumentNullException(nameof(fedExAvailabilityRequest));
            this._fedExTrackRequest = fedExTrackRequest ??
                                            throw new ArgumentNullException(nameof(fedExTrackRequest));
            this._fedExEstimateDeliveryRequest = fedExEstimateDeliveryRequest ??
                                            throw new ArgumentNullException(nameof(fedExEstimateDeliveryRequest));
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> GetRawRates(string carrier)
        {
            RateReply reply = new RateReply();
            RateReplyWrapper rateReply = new RateReplyWrapper();
            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            RateRequest rateRequest = JsonConvert.DeserializeObject<RateRequest>(bodyAsText);
            switch (carrier.ToUpper())
            {
                case FEDEX:
                    rateReply = await this._fedExRateRequest.GetRawRates(rateRequest);
                    break;
            }

            Response.Headers.Add("Cache-Control", "private");
            Response.Headers.Add("Timespan", rateReply.timeSpan.TotalSeconds.ToString());

            //return Json(getRatesResponseWrapper.GetRatesResponses);
            return Json(rateReply.rateReply);
        }

        public async Task<IActionResult> GetRates()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
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

            return Json(getRatesResponseWrapper.GetRatesResponses);
        }

        public async Task<IActionResult> Track(string carrier, string trackingNumber)
        {
            TrackReply reply = new TrackReply();
            switch (carrier.ToUpper())
            {
                case FEDEX:
                    reply = await this._fedExTrackRequest.Track(trackingNumber);
                    break;
            }

            Response.Headers.Add("Cache-Control", "private");

            return Json(reply);
        }

        public async Task SetMerchantSettings(string carrier, [FromBody] MerchantSettings merchantSettings)
        {
            await this._merchantSettingsRepository.SetMerchantSettings(carrier, merchantSettings);
        }

        /// <summary>
        /// Retrieve merchant settings
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> GetMerchantSettings()
        {
            var authenticationResponse = await this._merchantSettingsRepository.GetMerchantSettings();
            Response.Headers.Add("Cache-Control", "no-cache");
            return Json(authenticationResponse);
        }

        public async Task<IActionResult> GetEstimateDate()
        {
            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            GetEstimateDeliveryRequest getEstDateReq = JsonConvert.DeserializeObject<GetEstimateDeliveryRequest>(bodyAsText);

            this._fedExEstimateDeliveryRequest.getEstimateDelivery(getEstDateReq);
            GetEstimateDeliveryResponse result = this._fedExEstimateDeliveryRequest.getEstimateDelivery(getEstDateReq);
            Response.Headers.Add("Cache-Control", "private");

            return Json(result);
        }
    }
}
