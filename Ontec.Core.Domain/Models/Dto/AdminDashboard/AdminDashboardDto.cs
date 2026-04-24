using Ontec.Core.Domain.Models.Dto.Charts;

namespace Ontec.Core.Domain.Models.Dto.AdminDashboard
{
    public class AdminDashboardDto
    {
        public List<UtilityWiseCount> UtilityCount { get; set; }
        public List<TopUpCount> TopUp { get; set; }
        public List<TopUpAmount> TopUpTotalAmount { get; set; }
        public List<PaymentMethodAmount> MonthlyAmount { get; set; }

       public bool IsBarGraph { get; set; }

    }

    public class UtilityWiseCount
    {
        public string UtilityType { get; set; }
        public int Count { get; set; }
    }
    public class TopUpCount
    {
        public decimal Percentage { get; set; }
        public double ThisMonthTopUpCount { get; set; }
        public double LastMonthTopUpCount { get; set; }
        public string TopUpCountPercentageFlag { get; set; }

    }
    public class TopUpAmount
    {
        public decimal Percentage { get; set; }
        public double ThisMonthTopUpAmount { get; set; }
        public double LastMonthTopUpAmount { get; set; }
        public string TopUpAmountPercentageFlag { get; set; }

    }

    public class PaymentMethodAmount
    {
        public string PaymentMethod { get; set; }
        public LineChartDto MonthlWiseAmount { get; set; }
    }
    

    public class PaymenthMethodSummary
    {
        public string PaymentMethod { get; set;}
        public string Month { get; set; }
        public double Amount { get; set; }
    }
}

