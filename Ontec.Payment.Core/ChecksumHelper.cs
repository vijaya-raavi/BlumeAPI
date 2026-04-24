using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ontec.Payment.Core
{
    public class ChecksumHelper
    {
        public static string PopulateChecksum(string checkSumString) {
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
    }
}
