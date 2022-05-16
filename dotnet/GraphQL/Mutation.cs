using GraphQL;
using GraphQL.Types;
using FedexShipping.Data;
using FedexShipping.Models;
using FedexShipping.GraphQL.Types;

namespace FedexShipping.GraphQL
{
  [GraphQLMetadata("Mutation")]
  public class Mutation : ObjectGraphType<object>
  {
    public Mutation(IMerchantSettingsRepository _merchantSettingsRepository, ILogisticsRepository _logisticsRepository)
    {
        Name = "Mutation";

        FieldAsync<BooleanGraphType>(
            "saveAppSetting",
            arguments: new QueryArguments(
                new QueryArgument<AppSettingsInput> { Name = "appSetting" }
            ),
            resolve: async context =>
            {
                var appSettings = context.GetArgument<MerchantSettings>("appSetting");
                return await _merchantSettingsRepository.SetMerchantSettings("fedex", appSettings);
            }
        );

        FieldAsync<BooleanGraphType>(
            "updateDockConnection",
            arguments: new QueryArguments(
                new QueryArgument<UpdateDockInput> { Name = "updateDock" }
            ),
            resolve: async context =>
            {
                var updateDockRequest = context.GetArgument<UpdateDockRequest>("updateDock");
                return await _logisticsRepository.SetDocks(updateDockRequest);
            }
        );
    }
  }
}