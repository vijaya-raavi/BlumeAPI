using Newtonsoft.Json;

namespace Ontec.Core.Domain.Models
{
    public class MatTableModel<T> where T : class
    {
        public MatTableModel()
        {
            Data = new List<T>();
        }
        [JsonProperty(PropertyName = "pageSize")]
        public int PageSize { get; set; }

        [JsonProperty(PropertyName = "page")]
        public int Page { get; set; }

        [JsonProperty(PropertyName = "totalRecord")]
        public int TotalRecord { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<T> Data { get; set; }
    }
}
