using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;
using FedexShipping.Data;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("AppSettings")]
    public class AppSettingsType : ObjectGraphType<MerchantSettings>
    {
        public AppSettingsType(IMerchantSettingsRepository _merchantSettingsRepository)
        {
            Name = "AppSettings";

            Field(b => b.UserCredentialKey).Description("userCredentialKey");
            Field(b => b.UserCredentialPassword).Description("userCredentialPassword");
            Field(b => b.ParentCredentialKey).Description("parentCredentialKey");
            Field(b => b.ParentCredentialPassword).Description("parentCredentialPassword");
            Field(b => b.ClientDetailAccountNumber).Description("clientDetailAccountNumber");
            Field(b => b.ClientDetailMeterNumber).Description("clientDetailMeterNumber");
            Field(b => b.IsLive).Description("isLive");
            Field(b => b.Residential).Description("Shipping to Residential");
            Field(b => b.OptimizeShipping).Description("Optimizes Shipping");
            Field(b => b.UnitWeight).Description("unitWeight");
            Field(b => b.UnitDimension).Description("unitDimension");
            Field(b => b.ItemModals, type: typeof(ListGraphType<ItemModalsType>)).Description("Item Modals Mapping");
            Field(b => b.SlaSettings, type: typeof(ListGraphType<SlaSettingsType>)).Description("SLA Settings");
        }
    }
}