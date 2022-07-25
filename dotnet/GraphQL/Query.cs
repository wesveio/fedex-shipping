using GraphQL;
using GraphQL.Types;
using FedexShipping.GraphQL.Types;
using FedexShipping.Data;
using FedexShipping.Models;
using FedexShipping.Services;
using Newtonsoft.Json;

namespace FedexShipping.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(IMerchantSettingsRepository _merchantSettingsRepository, ILogisticsRepository _logisticsRepository, IPackingRepository _packingRepository, IFedExRateRequest _fedExRateRequest)
        {
            Name = "Query";

            FieldAsync<AppSettingsType>(
                "getAppSettings",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await _merchantSettingsRepository.GetMerchantSettings());
                }
            );

            FieldAsync<LogisticsDocksListType>(
                "getDocks",
                resolve: async context =>
                {
                return await context.TryAsyncResolve(
                        async c => await _logisticsRepository.GetDocks());
                }
            );

            FieldAsync<BooleanGraphType>(
                "testKey",
                arguments: new QueryArguments(
                    new QueryArgument<StringGraphType> { Name = "packingAccessKey" }
                ),
                resolve: async context =>
                {
                    var packingAccessKey = context.GetArgument<string>("packingAccessKey");

                    PackingRequest pack = new PackingRequest();
                    RequestItems item = new RequestItems(1,1,1,1,1);

                    pack.ItemsToPack.Add(item);

                    PackingResponseWrapper resp = await context.TryAsyncResolve(
                        async c => await _packingRepository.PackItems(pack, packingAccessKey));

                    // If not 0, return True for success
                    return resp.PackedResults.Capacity != 0;
                }
            );

            FieldAsync<BooleanGraphType>(
                "testCredentials",
                resolve: async context =>
                {
                    string bodyAsText = "{\"items\":[{\"id\":\"8\",\"quantity\":4,\"groupId\":null,\"unitPrice\":500,\"modal\":\"\",\"unitDimension\":{\"weight\":10,\"height\":10,\"width\":12,\"length\":10}},{\"id\":\"4\",\"quantity\":1,\"groupId\":null,\"unitPrice\":2,\"modal\":\"ELECTRONICS\",\"unitDimension\":{\"weight\":10,\"height\":11,\"width\":10,\"length\":10}}],\"origin\":{\"zipCode\":\"33020\",\"country\":\"USA\",\"state\":\"FL\",\"city\":\"Hollywood\",\"coordinates\":null,\"residential\":false},\"destination\":{\"zipCode\":\"00010002\",\"country\":\"USA\",\"state\":\"NY\",\"city\":\"New York\",\"coordinates\":null,\"residential\":false},\"shippingDateUTC\":\"2022-05-31T01:02:45.128577+00:00\",\"currency\":null}";

                    GetRatesRequest getRatesRequest = JsonConvert.DeserializeObject<GetRatesRequest>(bodyAsText);

                    GetRatesResponseWrapper getRatesResponseWrapper = await _fedExRateRequest.GetRates(getRatesRequest);

                    // If >0, success
                    return getRatesResponseWrapper.GetRatesResponses.Capacity > 0;
                }
            );
        }
    }
}