using System.Text.Json.Serialization;

namespace Ontec.Core.Domain.Models
{
    public class BasetGetQuery
    {
        [JsonIgnore]
        public bool IsSuperAdmin { get; set; }

        [JsonIgnore]
        public int LoggedInUserId { get; set; }
    }
    public abstract class BaseDatatableQuery<T> : BasetGetQuery
    {
        private int page;
        private int pageSize;
        public string sort { get; set; }
        public string order { get; set; }
        public int Page
        {
            get
            {
                return (page <= 0) ? 0 : page;
            }
            set { page = value; }
        }
        public int PageSize
        {
            get
            {
                return (pageSize <= 0) ? 20 : pageSize;
            }
            set { pageSize = value; }
        }
    }
    public abstract class BaseDatatableQuery : BasetGetQuery
    {
        private int page;
        private int pageSize;
        public string sort { get; set; }
        public string order { get; set; }
        public int Page
        {
            get
            {
                return (page <= 0) ? 0 : page;
            }
            set { page = value; }
        }
        public int PageSize
        {
            get
            {
                return (pageSize <= 0) ? 20 : pageSize;
            }
            set { pageSize = value; }
        }

        public string SearchText { get; set; }
    }
}
