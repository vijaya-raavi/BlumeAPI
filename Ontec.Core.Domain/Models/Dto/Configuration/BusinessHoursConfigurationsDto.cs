namespace Ontec.Core.Domain.Models.Dto.Configuration
{
    public  class BusinessHoursConfigurationsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public bool AcceptUserNonBusiness { get; set; }
        public bool AcceptUserBusiness { get; set; }

        public bool AcceptMeterNonBusiness { get; set; }

        public bool AcceptMeterBusiness { get; set; }
    }
}
