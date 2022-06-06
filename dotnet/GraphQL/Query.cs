using GraphQL;
using GraphQL.Types;
using FedexShipping.GraphQL.Types;
using FedexShipping.Data;
using FedexShipping.Models;

namespace FedexShipping.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(IMerchantSettingsRepository _merchantSettingsRepository, ILogisticsRepository _logisticsRepository, IPackingRepository _packingRepository)
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
        }
    }
}