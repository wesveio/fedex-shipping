using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;
using FedexShipping.Data;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("LogisticsDocksList")]
    public class LogisticsDocksListType : ObjectGraphType<LogisticsDocksListWrapper>
    {
        public LogisticsDocksListType(ILogisticsRepository _logisticsRepository)
        {
            Name = "LogisticsDocksList";

            Field(b => b.DocksList, type: typeof(ListGraphType<LogisticsDocksType>)).Description("Dock List");

        }
    }
}