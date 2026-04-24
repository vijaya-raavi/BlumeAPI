namespace Ontec.Core.Domain.Models.Dto.AdminDashboard
{
    public class PaymentDashboardDto
    {

        public int TotalProperties { get; set; }
        public int TotalConsumers { get; set; }
        public int TotalMeters { get; set; }
        public int TotalActiveMeters { get; set; }

        public int TotalTopUpCount { get; set; }

        public double TotalTopUpAmount { get; set; }
        public List<UtilityWiseTopUpAmount> UtilityTopUpAmount { get; set; }
      
    }

    public class UtilityWiseTopUpAmount
    {
        public string MeterType { get; set; }
        public decimal LastWeekAmount { get; set; }
        public decimal LastMonthAmount { get; set; }
        public decimal LastYearAmount { get; set; }
    }


}
