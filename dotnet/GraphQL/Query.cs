using GraphQL;
using GraphQL.Types;
using FedexShipping.GraphQL.Types;
using FedexShipping.Data;
using System;
namespace FedexShipping.GraphQL
{
    [GraphQLMetadata("Query")]
    public class Query : ObjectGraphType<object>
    {
        public Query(IMerchantSettingsRepository _merchantSettingsRepository)
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
        }
    }
}