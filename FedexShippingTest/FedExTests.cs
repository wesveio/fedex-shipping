using FedExAvailabilityServiceReference;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using FedexShipping.Data;
using FedexShipping.Models;
using FedexShipping.Services;
using System;
using System.Collections.Generic;
using TrackServiceReference;
using Vtex.Api.Context;

namespace FedexShippingTest
{
    [TestClass]
    public class FedExTests
    {
        InMemoryMerchantSettingsRepository merchantSettingsRepository = new InMemoryMerchantSettingsRepository();
        private readonly IIOServiceContext _context;
        private readonly IMerchantSettingsRepository _merchantSettingsRepository;
        private readonly IFedExCacheRepository _fedExCacheRespository;
        private readonly IPackingService _packingService;
        private readonly IVtexEnvironmentVariableProvider _environmentVariableProvider;
        [TestMethod]
        public void TestMethodGetRates()
        {
            FedExRateRequest fedExRateRequest = new FedExRateRequest(_environmentVariableProvider, merchantSettingsRepository, _context, _fedExCacheRespository, _packingService);

            GetRatesRequest getRatesRequest = BuildRequest();

            Console.WriteLine(JsonConvert.SerializeObject(getRatesRequest));

            GetRatesResponseWrapper response = new GetRatesResponseWrapper();
            for (int i = 0; i < 5; i++)
            {
                if (!response.Success)
                {
                    System.Threading.Thread.Sleep(1000 * i * i);
                    Console.WriteLine($"{i}:[{response.HighestSeverity}] Sleeping {i * i} seconds.........");
                    response = fedExRateRequest.GetRates(getRatesRequest).Result;
                }
                else
                {
                    break;
                }
            }

            List<GetRatesResponse> responses = response.GetRatesResponses;
            string debug = JsonConvert.SerializeObject(responses);
            Console.WriteLine(debug);
        }

        [TestMethod]
        public void TestMethodAvailable()
        {
            FedExAvailabilityRequest fedExAvailabilityRequest = new FedExAvailabilityRequest(merchantSettingsRepository);

            serviceAvailabilityResponse response = new serviceAvailabilityResponse();

            //for (int i = 0; i < 5; i++)
            //{
            //        System.Threading.Thread.Sleep(1000 * i * i);
            //        Console.WriteLine($"{i}: Sleeping {i * i} seconds.........");
            response = fedExAvailabilityRequest.GetAvailability().Result;
            //}
        }

        [TestMethod]
        public void TestTracking()
        {
            FedExTrackRequest fedExTrackRequest = new FedExTrackRequest(merchantSettingsRepository);

            TrackReply reply = new TrackReply();

            string[] trackingNumbers = new string[] { "039813852990618", "231300687629630", "039813852990618", "123456789012" };

            foreach(string trackingNumber in trackingNumbers)
            {
                Console.WriteLine($"Requesting {trackingNumber} tracking...");
                reply = fedExTrackRequest.Track(trackingNumber).Result;
            }
        }

        public GetRatesRequest BuildRequest()
        {
            Item item1 = new Item
            {
                groupId = null,
                id = "1",
                modal = null,
                quantity = 1,
                unitDimension = new UnitDimension
                {
                    height = 1,
                    length = 1,
                    weight = 1,
                    width = 1
                },
                unitPrice = 9.99
            };

            Item item2 = new Item
            {
                groupId = null,
                id = "2",
                modal = null,
                quantity = 3,
                unitDimension = new UnitDimension
                {
                    height = 1,
                    length = 1,
                    weight = 1,
                    width = 1
                },
                unitPrice = 29.99
            };

            GetRatesRequest getRatesRequest = new GetRatesRequest
            {
                destination = new Destination
                {
                    city = "Williamson",
                    state = "NY",
                    street = "7533 Bear Swamp Road",
                    country = "US",
                    zipCode = "14589"
                },
                items = new List<Item> { item1, item2 },
                origin = new Origin
                {
                    city = "Beverly Hills",
                    state = "CA",
                    street = "1000 Main Street",
                    country = "US",
                    zipCode = "90210"
                }
            };

            return getRatesRequest;
        }
    }
}
