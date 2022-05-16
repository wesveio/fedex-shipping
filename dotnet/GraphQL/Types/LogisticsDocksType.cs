using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;
using FedexShipping.Data;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("LogisticsDocks")]
    public class LogisticsDocksType : ObjectGraphType<LogisticsDockWrapper>
    {
        public LogisticsDocksType(ILogisticsRepository _logisticsRepository)
        {
            Name = "LogisticsDocks";

            Field(b => b.Id).Description("Id");
            Field(b => b.Name).Description("Name");
            Field(b => b.ShippingRatesProviders).Description("Shipping Rates Provider List");
        }
    }
}