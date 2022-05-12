using GraphQL;
using GraphQL.Types;
using FedexShipping.GraphQL.Types;
using FedexShipping.Data;
namespace FedexShipping.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(IMerchantSettingsRepository _merchantSettingsRepository, ILogisticsRepository _logisticsRepository)
        {
            Name = "Query";

            FieldAsync<AppSettingsType>(
                "getAppSettings",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await _merchantSettingsRepository.GetMerchantSettings("fedex"));
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
        }
    }
}