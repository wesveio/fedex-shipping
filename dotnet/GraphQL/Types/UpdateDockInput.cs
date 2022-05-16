using GraphQL;
using GraphQL.Types;
using FedexShipping.Models;

namespace FedexShipping.GraphQL.Types
{
    [GraphQLMetadata("UpdateDockInput")]
    public class UpdateDockInput : InputObjectGraphType<UpdateDockRequest>
    {
        public UpdateDockInput()
        {
            Name = "UpdateDockInput";

            Field(b => b.DockId).Description("Dock ID");
            Field(b => b.ToRemove).Description("Boolean whether to remove connection or not");
        }
    }
}