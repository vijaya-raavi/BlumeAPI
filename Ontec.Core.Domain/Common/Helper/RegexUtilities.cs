using System.Text.RegularExpressions;

namespace Ontec.Core.Domain.Common.Helper
{
    public class RegexUtilities
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                return Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z._]+\.[a-zA-Z.]{2,}$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsValidMobileEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                if (email.Length == 10 && email.Any(char.IsDigit))
                {
                    return IsMobileValid(email);
                }
                else
                {
                    return IsValidEmail(email);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsMobileValid(string mobile)
        {
            if (string.IsNullOrWhiteSpace(mobile))
                return false;
            try
            {
                return Regex.IsMatch(mobile, "^[0-9]{7,14}$");
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
