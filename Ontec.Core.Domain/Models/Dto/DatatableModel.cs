using Newtonsoft.Json;

namespace Ontec.Core.Domain.Models.Dto
{
    public class DatatableModel<T> where T : class
    {
        public DatatableModel()
        {
            Data = new List<T>();
        }

        [JsonProperty(PropertyName = "Page")]
        public int Page { get; set; }

        [JsonProperty(PropertyName = "PageSize")]
        public int PageSize { get; set; }

        [JsonProperty(PropertyName = "Total")]
        public int TotalRecords {  get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<T> Data { get; set; }

        [JsonProperty(propertyName: "SearchText")]
        public string? SearchText { get; set; }  
    }
}
