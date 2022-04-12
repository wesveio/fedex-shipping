using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("ItemModals")]
    public class ItemModalsInput : ObjectGraphType<ModalMap>
    {
        public ItemModalsInput()
        {
            Name = "ItemModals";

            Field(x => x.Modal).Description("modal");
            Field(x => x.FedexHandling).Description("fedexHandling");
        }
    }
}