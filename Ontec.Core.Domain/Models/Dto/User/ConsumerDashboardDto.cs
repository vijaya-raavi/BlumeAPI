namespace Ontec.Core.Domain.Models.Dto.User
{
    public class ConsumerDashboardDto
    {
        public int TotalPropertyCount { get; set; }
        public List<PropertiesCount> Properties {  get; set; }
        public List<Utility> AllConsumerUtilities { get; set; }
        public List<Utility> ActiveConsumerUtilities { get; set; }
        public List<TotalUsersCount> TotalUsers { get; set; }
        public List<NewUsersCount> NewUsers { get; set; }
        public List<TotalActiveMeterCount> TotalActiveMeters{ get; set; }
        public List<AllMeterCount> AllMeters { get; set; }

    }
    public class PropertiesCount
    {
        public decimal TotalPropertyPercentage { get; set; }
        public string TotalPropertyPercentageflag { get; set; }
    }
    public class Utility
    {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    public class TotalUsersCount
    {
        public int TotalUserCount { get; set; }
        public int ThisMonthCount { get; set; }
        public int TillLastMonthCount { get; set; }
        public decimal TotalPercentage { get; set; }
        public string TotalUserPercentageFlag { get; set; }

    }
    public class NewUsersCount
    {

        public int NewUserCount { get; set; }
        public int LastMonthCount { get; set; }
        public string NewUserPercentage { get; set; }
        public string NewUserPercentageFlag { get; set; }

    }

    public class TotalActiveMeterCount
    {
        public int TotalMeterCount { get; set; }
        public int ThisMonthCount { get; set; }
        public int TillLastMonthCount { get; set; }
        public decimal TotalPercentage { get; set; }
        public string TotalMeterPercentageFlag { get; set; }

    }
    public class AllMeterCount
    {
        public int TotalMeterCount { get; set; }
        public int ThisMonthCount { get; set; }
        public int TillLastMonthCount { get; set; }
        public decimal TotalPercentage { get; set; }
        public string TotalMeterPercentageFlag { get; set; }

    }
}
