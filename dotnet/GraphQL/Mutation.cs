using GraphQL;
using GraphQL.Types;
using FedexShipping.Data;
using FedexShipping.Models;
using FedexShipping.GraphQL.Types;
using System;

namespace FedexShipping.GraphQL
{
  [GraphQLMetadata("Mutation")]
  public class Mutation : ObjectGraphType<object>
  {
    public Mutation(IMerchantSettingsRepository _merchantSettingsRepository)
    {
        Name = "Mutation";

        FieldAsync<BooleanGraphType>(
            "saveAppSetting",
            arguments: new QueryArguments(
                new QueryArgument<AppSettingsType> { Name = "AppSettings" }
            ),
            resolve: async context =>
            {
                Console.WriteLine("yes");
                var appSettings = context.GetArgument<MerchantSettings>("AppSettings");
                return await _merchantSettingsRepository.SetMerchantSettings("fedex", appSettings);
            }
        );
    }
  }
}