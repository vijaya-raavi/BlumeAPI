namespace Ontec.Core.Domain.Models.Dto.Charts
{
    public class GuageChartDto
    {
        public double ActualValue { get; set; }
        public double TargetValue { get; set; }
    }
    public class WeeklyGuageChartDto
    {
        public double ActualValue { get; set; }
        public double TargetValue { get; set; }
    }
    public class DailyGuageChartDto
    {
        public double ActualValue { get; set; }
        public double TargetValue { get; set; }
    }
}
