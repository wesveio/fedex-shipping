using System.Collections.Generic;

namespace FedexShipping.Models
{
    public class PackingResponse
    {
        public string ContainerId { get; set; }
        public List<AlgoPackResults> AlgorithmPackingResults { get; set; } = new List<AlgoPackResults>();
    }

    public class Container {
        public string Id { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Volume { get; set; }
    }

    public class AlgoPackResults {
        public List<PackedItem> PackedItems { get; set; } = new List<PackedItem>();
        public float PercentContainerVolumePacked { get; set; }
        public float PercentItemVolumePacked { get; set; }
    }

    public class PackedItem {
        public int id { get; set; }
        public int dim1 { get; set; }
        public int dim2 { get; set; }
        public int dim3 { get; set; }
        public int packDimX { get; set; }
        public int packDimY { get; set; }
        public int packDimZ { get; set; }
    }

    public class PackingResponseWrapper
    {
        public List<PackingResponse> PackedResults { get; set; } = new List<PackingResponse>();
        public List<Container> Containers { get; set; } = new List<Container>();


        public PackingResponseWrapper() {}
    }
}
