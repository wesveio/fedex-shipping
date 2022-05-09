using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("SlaSettingsInput")]
    public class SlaSettingsInput : InputObjectGraphType<SlaSettings>
    {
        public SlaSettingsInput()
        {
            Name = "SlaSettingsInput";

            Field(x => x.Sla).Description("SLA");
            Field(x => x.Hidden).Description("Hides SLA");
            Field(x => x.SurchargeFlatRate).Description("Surcharge Flat Rate");
            Field(x => x.SurchargePercent).Description("Surcharge Percent");
        }
    }
}