using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ontec.Core.Application.Common.Helper
{
    public class ChecksumHelper
    {
        public static string PopulateChecksum(string checkSumString)
        {
            var checkSum = string.Empty;
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(checkSumString));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                checkSum = builder.ToString();
            }

            return checkSum;
        }
        public static string GenerateTransactionNumber()
        {
            Random TransactionNumner = new Random();
            int number = TransactionNumner.Next(10000, 99999);
            string digits = number.ToString();
            string strTransactionNumber = "";
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString("ddMMyy");
            strTransactionNumber = "TXN" + date + digits;
            return strTransactionNumber;
        }
        public static string GenerateOTP()
        {
            Random OTP = new Random();
            int number = OTP.Next(100000, 999999);
            string digits = number.ToString();
            string strOTP = "";

            strOTP = digits;
            return strOTP;
        }
        public static string GenerateVerifiedKey()
        {
            Random TransactionNumner = new Random();
            int number = TransactionNumner.Next(10000, 99999);
            string digits = number.ToString();
            string strTransactionNumber = "";
            DateTime currentDate = DateTime.Now;
            string date = currentDate.ToString("ddMMyyhhmmss");
            strTransactionNumber = "VER"+ date + digits;
            return strTransactionNumber;
        }
        public static string FormatAmount(decimal amount)
        {
            amount = Math.Abs(amount);  // Remove negative sign  
            return "R " + amount.ToString("N2", CultureInfo.GetCultureInfo("en-ZA"));
        }
    }
}
