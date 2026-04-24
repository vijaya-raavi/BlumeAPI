namespace Ontec.Core.Domain.Models.Dto.Charts
{
    public class LineChartDto
    {
        public List<string> XAxisdata { get; set; }
        public List<double> SeriesLineData { get; set; }
    }
    public class WeeklyLineChartDto
    {
        public List<string> XAxisdata { get; set; }
        public List<double> SeriesLineData { get; set; }
    }
    public class DailyLineChartDto
    {
        public List<string> XAxisdata { get; set; }
        public List<double> SeriesLineData { get; set; }
    }
}
