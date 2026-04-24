using Microsoft.AspNetCore.Http;
using System.Globalization;

namespace Ontec.Core.Application.Common.Helper
{
    public static class OntecExtensions
    {
        public static DateTime ParseDateTime(this string value)
        {
            string[] dateFormats = { "yyyy-MM-dd", "yyyy-MM-d" };
            bool validDate = DateTime.TryParseExact(value, dateFormats, new CultureInfo("en-US"), DateTimeStyles.None, out DateTime parsedDate);
            if (validDate)
            {
               return parsedDate;
            }
            return DateTime.MinValue;
        }

        public static bool IsFileValid(this IFormFile file, int filesize)
        {
            if (file.Length < 1)
                return false;
            string[] supportedTypes = new[] { "jpg", "jpeg", "png", "pdf" };
            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
            if (!supportedTypes.Contains(fileExt))
                return false;
            if (file.Length > (filesize * 1024))
                return false;

            return true;
        }
    }
}
