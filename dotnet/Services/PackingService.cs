namespace FedexShipping.Services
{
    using System;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using FedexShipping.Models;
    using FedexShipping.Data;
    public class PackingService : IPackingService
    {
        private readonly IPackingRepository _packingRepository;

        public PackingService(IPackingRepository packingRepository)
        {
            this._packingRepository = packingRepository ?? throw new ArgumentException(nameof(packingRepository));
        }

        public async Task<List<Item>> packingMap(List<Item> items) {
            PackingRequest packingRequest = new PackingRequest();

            // All different modals here are handled the same way
            string itemsListModal = null;
            Dictionary<string, Item> itemMap = new Dictionary<string, Item>();
            foreach (Item item in items)
            {
                itemsListModal = item.modal;
                int itemId = item.id.GetHashCode();
                Int32.TryParse(item.id, out itemId);
                packingRequest.ItemsToPack.Add(new RequestItems(
                    itemId,
                    (int)Math.Ceiling(item.unitDimension.length),
                    (int)Math.Ceiling(item.unitDimension.width),
                    (int)Math.Ceiling(item.unitDimension.height),
                    item.quantity
                ));

                itemMap.Add(itemId.ToString(), item);
            }

            PackingResponseWrapper packingResponseWrapper = await this._packingRepository.PackItems(packingRequest);

            Dictionary<string, Container> containerMap = new Dictionary<string, Container>();

            foreach (Container container in packingResponseWrapper.Containers)
            {
                containerMap.Add(container.Id, container);
            }

            List<Item> containerizedItems = new List<Item>();
            foreach (PackingResponse packingResponse in packingResponseWrapper.PackedResults)
            {

                double weightTotal = 0;

                // Go thru every packed item and get the weight
                foreach (PackedItem packedItem in packingResponse.AlgorithmPackingResults[0].PackedItems)
                {
                    weightTotal += itemMap[packedItem.id.ToString()].unitDimension.weight;
                }

                Item newBox = new Item {
                    id = packingResponse.ContainerId.ToString(),
                    groupId = null,
                    modal = itemsListModal,
                    quantity = 1,
                    unitDimension = new UnitDimension
                    {
                        length = Double.Parse(containerMap[packingResponse.ContainerId].Length),
                        width = Double.Parse(containerMap[packingResponse.ContainerId].Width),
                        height = Double.Parse(containerMap[packingResponse.ContainerId].Height),
                        weight = weightTotal
                    }
                };

                containerizedItems.Add(newBox);
            }
            
            return containerizedItems;
        }
    }
}