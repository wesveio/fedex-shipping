using GraphQL;
using GraphQL.Types;
using FedexShipping.Data;
using FedexShipping.Models;
using FedexShipping.GraphQL.Types;
using FedexShipping.Services;
using System;
using System.Net;

namespace FedexShipping.GraphQL
{
  [GraphQLMetadata("Mutation")]
  public class Mutation : ObjectGraphType<object>
  {
    public Mutation(IMerchantSettingsRepository _merchantSettingsRepository, ILogisticsRepository _logisticsRepository, IVtexApiService vtexApiService)
    {
        Name = "Mutation";

        FieldAsync<BooleanGraphType>(
            "saveAppSetting",
            arguments: new QueryArguments(
                new QueryArgument<AppSettingsInput> { Name = "appSetting" }
            ),
            resolve: async context =>
            {
                HttpStatusCode isValidAuthUser = await vtexApiService.IsValidAuthUser();

                if (isValidAuthUser != HttpStatusCode.OK)
                {
                    context.Errors.Add(new ExecutionError(isValidAuthUser.ToString())
                    {
                        Code = isValidAuthUser.ToString()
                    });

                    return default;
                }

                var appSettings = context.GetArgument<MerchantSettings>("appSetting");
                return await _merchantSettingsRepository.SetMerchantSettings(appSettings);
            }
        );

        FieldAsync<BooleanGraphType>(
            "updateDockConnection",
            arguments: new QueryArguments(
                new QueryArgument<UpdateDockInput> { Name = "updateDock" }
            ),
            resolve: async context =>
            {
                HttpStatusCode isValidAuthUser = await vtexApiService.IsValidAuthUser();

                if (isValidAuthUser != HttpStatusCode.OK)
                {
                    context.Errors.Add(new ExecutionError(isValidAuthUser.ToString())
                    {
                        Code = isValidAuthUser.ToString()
                    });

                    return default;
                }

                var updateDockRequest = context.GetArgument<UpdateDockRequest>("updateDock");
                return await _logisticsRepository.SetDocks(updateDockRequest);
            }
        );
    }
  }
}