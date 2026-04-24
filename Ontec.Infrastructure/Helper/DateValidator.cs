using System.Globalization;
using System.Reflection;

namespace Ontec.Infrastructure.Helper
{
    public class DateValidator
    {
        public static bool IsValidDate(string value, out DateTime? date)
        {
            DateTime parsedDate;
            string[] dateFormats = { "yyyy-MM-dd", "yyyy-MM-d" };
            bool validDate = DateTime.TryParseExact(value, dateFormats, new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate);
            date = parsedDate;
            if (validDate)
                return true;
            else
                return false;
        }
        public static async Task<bool> IsValidContractEndDate(string endDateString)
        {
            if (DateTime.TryParseExact(endDateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime contractDate))
            {
                DateTime today = DateTime.UtcNow.Date;
                bool isValid = contractDate >= today;
                return await Task.FromResult(isValid).ConfigureAwait(false);
            }
            else
            {
                throw new ArgumentException("Invalid date format. Please provide the date in yyyy-MM-dd format.");
            }
        }
    }
}