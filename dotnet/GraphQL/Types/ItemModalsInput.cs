using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("ItemModalsInput")]
    public class ItemModalsInput : InputObjectGraphType<ModalMap>
    {
        public ItemModalsInput()
        {
            Name = "ItemModalsInput";

            Field(x => x.Modal).Description("modal");
            Field(x => x.FedexHandling).Description("fedexHandling");
            Field(x => x.ShipAlone).Description("Determines if a modal is ship alone");
        }
    }
}