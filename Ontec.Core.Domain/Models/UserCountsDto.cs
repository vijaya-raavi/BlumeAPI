namespace Ontec.Core.Domain.Models
{
    public  class UserCountsDto
    {
        public int Web { get; set; }
        public int Mobile { get; set; }

        public int counts { get; set; }
        public int currentRequests {  get; set; }
    }
}
