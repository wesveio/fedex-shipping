namespace service.Controllers
{
    using System;
    using System.Threading.Tasks;
    using FedExRateServiceReference;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using FedexShipping.Data;
    using FedexShipping.Models;
    using FedexShipping.Services;
    using TrackServiceReference;

    public class RoutesController : Controller
    {
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExRateRequest _fedExRateRequest;
        private readonly IFedExAvailabilityRequest _fedExAvailabilityRequest;
        private readonly IFedExTrackRequest _fedExTrackRequest;
        private readonly IFedExEstimateDeliveryRequest _fedExEstimateDeliveryRequest;
        private const string FEDEX = "FEDEX";

        public RoutesController(IMerchantSettingsRepository merchantSettingsRepository, IFedExRateRequest fedExRateRequest, IFedExAvailabilityRequest fedExAvailabilityRequest, IFedExTrackRequest fedExTrackRequest, IFedExEstimateDeliveryRequest fedExEstimateDeliveryRequest)
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
            GetRatesResponseWrapper getRatesResponseWrapper = new GetRatesResponseWrapper();
            var bodyAsText = await new System.IO.StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            GetRatesRequest getRatesRequest = JsonConvert.DeserializeObject<GetRatesRequest>(bodyAsText);

            getRatesResponseWrapper = await this._fedExRateRequest.GetRates(getRatesRequest);            

            Response.Headers.Add("Cache-Control", "private");
            Response.Headers.Add("Timespan", getRatesResponseWrapper.timeSpan.TotalSeconds.ToString());

            //return Json(getRatesResponseWrapper.GetRatesResponses);
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
        public async Task<IActionResult> GetMerchantSettings(string carrier)
        {
            var authenticationResponse = await this._merchantSettingsRepository.GetMerchantSettings(carrier);
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
