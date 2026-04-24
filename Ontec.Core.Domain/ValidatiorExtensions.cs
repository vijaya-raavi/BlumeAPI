using FluentValidation;
using Microsoft.AspNetCore.Http;
using Ontec.Core.Domain.Common.Helper;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Ontec.Core.Domain
{
    public static class ValidatiorExtensions
    {

        private static DateTime parsedDate;
        public static IRuleBuilderOptions<T, string> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }

        public static IRuleBuilderOptions<T, string> NotNullAndEmptyAsyncOTP<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmptyOTP(m)).WithMessage($"OTP is required.");
        }
        public static IRuleBuilderOptions<T, string> NotNullAndEmptyAsyncForProperty<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmptyProperty(m)).WithMessage($"Property name is required.");
        }
        public static IRuleBuilderOptions<T, string> NotNullAndEmptyAsyncForUnitNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmptyProperty(m)).WithMessage($"Unit number is required.");
        }
        public static IRuleBuilderOptions<T, string> IsDateFormatValid<T>(this IRuleBuilder<T, string> ruleBuilder, string propertyName)
        {
            return ruleBuilder.MustAsync((m, cancellation) => IsDateFormatValid(m)).WithMessage($"{propertyName} is accepted in 'yyyy-MM-dd' format.");
        }
        public static IRuleBuilderOptions<T, string> IsValidContractEndDate<T>(this IRuleBuilder<T, string> ruleBuilder, string propertyName)
        {
            return ruleBuilder.MustAsync((m, cancellation) => IsValidContractEndDate(m)).WithMessage($"{propertyName} shoud less than or equal to todays date.");
        }
        public static IRuleBuilderOptions<T, long> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, long> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }
        public static IRuleBuilderOptions<T, long?> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, long?> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }
        public static IRuleBuilderOptions<T, int?> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, int?> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }
        public static IRuleBuilderOptions<T, int> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, int> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }
        public static IRuleBuilderOptions<T, DateTime> NotNullAndEmptyAsync<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckNotNullAndEmpty(m)).WithMessage($"This field is required.");
        }
        public static IRuleBuilderOptions<T, int> GreaterThanOrEqualToAsync<T>(this IRuleBuilder<T, int> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => GreaterThanOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be greater than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, int?> GreaterThanOrEqualToAsync<T>(this IRuleBuilder<T, int?> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => GreaterThanOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be greater than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, float?> GreaterThanOrEqualToAsync<T>(this IRuleBuilder<T, float?> ruleBuilder, string propertyName, float valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => GreaterThanOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be greater than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string?> LengthShouldBeLessOrEqualToAsync<T>(this IRuleBuilder<T, string?> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => LengthShouldBeLessOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be less than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string?> LengthShouldBeLessOrEqualToAsyncUnitNumber<T>(this IRuleBuilder<T, string?> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => LengthShouldBeLessOrEqualToAsyncUnitNumber(m, valueToCompare)).WithMessage($"{propertyName} length should be less than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string?> LengthShouldBeLessOrEqualToAsyncMeterNumber<T>(this IRuleBuilder<T, string?> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => LengthShouldBeLessOrEqualToAsyncMeterNumber(m, valueToCompare)).WithMessage($"Meter number length should be less than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, int> LengthShouldBeLessOrEqualToAsync<T>(this IRuleBuilder<T, int> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => LengthShouldBeLessOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be less than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string> IsValidEmailId<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidEmail(m)).WithMessage($"Invalid email address");
        }
        public static IRuleBuilderOptions<T, string> IsValidEmailMobile<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidEmailMobile(m)).WithMessage($"Invalid email or mobile");
        }
        public static IRuleBuilderOptions<T, string> GreaterThanOrEqualToAsync<T>(this IRuleBuilder<T, string> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => GreaterThanOrEqualToAsync(m, valueToCompare)).WithMessage($"{propertyName} should be greater than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string> GreaterThanOrEqualToAsyncOldPassword<T>(this IRuleBuilder<T, string> ruleBuilder, string propertyName, int valueToCompare)
        {
            return ruleBuilder.MustAsync((m, cancellation) => GreaterThanOrEqualToAsyncOldPassword(m, valueToCompare)).WithMessage($"Old Password should be greater than or equal to {valueToCompare}.");
        }
        public static IRuleBuilderOptions<T, string> IsValidMobile<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidMobile(m)).WithMessage($"Invalid mobile number");
        }
        public static IRuleBuilderOptions<T, int?> LengthShouldBeEqualAsync<T>(this IRuleBuilder<T, int?> ruleBuilder, string propertyName, int length)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckLengthShouldBeEqual(m, length)).WithMessage($"{propertyName} length should be equal to {length}");
        }
        public static IRuleBuilderOptions<T, string?> LengthShouldBeEqualAsync<T>(this IRuleBuilder<T, string?> ruleBuilder, string propertyName, int length)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckLengthShouldBeEqual(m, length)).WithMessage($"{propertyName} length should be equal to {length}");
        }
        public static IRuleBuilderOptions<T, string?> LengthShouldBeEqualAsyncMobileNumber<T>(this IRuleBuilder<T, string?> ruleBuilder, string propertyName, int length)
        {
            return ruleBuilder.MustAsync((m, cancellation) => CheckLengthShouldBeEqualMobileNumber(m, length)).WithMessage($"Mobile number length should be equal to {length}");
        }
        public static IRuleBuilderOptions<T, IFormFile?> IsFileValid<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, string propertyName, int length)
        {
            return ruleBuilder.MustAsync((m, cancellation) => IsFileValid(m, length)).WithMessage($"Allowed file formats (jpg, jpeg, png, pdf), {propertyName} length should be less or equal to {length} mb");
        }
        public static IRuleBuilderOptions<T, IFormFile?> IsImageValid<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, string propertyName, int length)
        {
            return ruleBuilder.MustAsync((m, cancellation) => IsImageValid(m, length)).WithMessage($"Allowed file formats (jpg, jpeg, png), {propertyName} length should be less or equal to {length} mb");
        }
        public static IRuleBuilderOptions<T, string> IsValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsPasswordValid(m)).WithMessage($"Invalid password");
        }
        public static IRuleBuilderOptions<T, string> IsValidName<T>( this IRuleBuilder<T, string> ruleBuilder,string propertyName)
        {
            return ruleBuilder.Must((m) => IsValidName(m)).WithMessage($"Invalid {propertyName}");
        }
    
        public static IRuleBuilderOptions<T, string> IsValidGroupName<T>(this IRuleBuilder<T, string> ruleBuilder, string propertyName)
        {
            return ruleBuilder.Must((m) => IsValidGroupName(m)).WithMessage($"Invalid {propertyName}");
        }
        public static IRuleBuilderOptions<T, string> IsValidFullName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidFullName(m)).WithMessage($"Invalid name");
        }
        public static IRuleBuilderOptions<T, string> IsValidExpiryCardMonthYear<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidExpiryCardMonthYear(m)).WithMessage($"Invalid name");
        }
        public static IRuleBuilderOptions<T, string> IsValidNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidumber(m)).WithMessage($"Enter only numbers");
        }

        public static IRuleBuilder<T, string> IsValidMeterNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidMeterNumber(m)).WithMessage($"Invalid meter number");
        }

        public static IRuleBuilder<T, double> IsValidTopUpAmount<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidTopUpAmount(m)).WithMessage($"Invalid top up amount");
        }

        public static IRuleBuilderOptions<T, string> IsValidInput<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidInput(m)).WithMessage($"Invalid entered name");
        }
        //public static IRuleBuilderOptions<T, double> IsValidTargetConsumption<T>(this IRuleBuilder<T, double> ruleBuilder)
        //{
        //    return ruleBuilder.Must((m) => IsValidTargetConsumption(m)).WithMessage($"Invalid target consumption");
        //}
        public static IRuleBuilderOptions<T, double> IsValidTargetConsumption<T>(this IRuleBuilder<T, double> ruleBuilder)
        {
            return ruleBuilder.Must((m) => IsValidTargetConsumption(m)).WithMessage($"No decimal numbers are allowed.");
        }
        #region 
        private static async Task<bool> LengthShouldBeLessOrEqualToAsync(string? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value?.Trim().Length <= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> LengthShouldBeLessOrEqualToAsyncMeterNumber(string? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value?.Trim().Length <= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> LengthShouldBeLessOrEqualToAsyncUnitNumber(string? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value?.Trim().Length <= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> LengthShouldBeLessOrEqualToAsync(int? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value.ToString()?.Trim().Length <= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> GreaterThanOrEqualToAsync(int? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value >= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> GreaterThanOrEqualToAsync(string? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value.Length >= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> GreaterThanOrEqualToAsyncOldPassword(string? value, int valueToCompare)
        {
            if (value == null)
                return true;

            var result = value.Length >= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> GreaterThanOrEqualToAsync(float? value, float? valueToCompare)
        {
            if (value == null)
                return true;

            var result = value >= valueToCompare;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckNotNullAndEmpty(string value)
        {
            var result = !string.IsNullOrEmpty(value?.Trim());
            return await Task.FromResult(result).ConfigureAwait(false);

        }
        private static async Task<bool> CheckNotNullAndEmptyProperty(string value)
        {
            var result = !string.IsNullOrEmpty(value?.Trim());
            return await Task.FromResult(result).ConfigureAwait(false);

        }
        private static async Task<bool> CheckNotNullAndEmptyOTP(string value)
        {
            var result = !string.IsNullOrEmpty(value?.Trim());
            return await Task.FromResult(result).ConfigureAwait(false);

        }
        private static async Task<bool> CheckNotNullAndEmpty(DateTime value)
        {
            var result = false;
            if (value > DateTime.MinValue)
                result = true;

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckNotNullAndEmpty(long? value)
        {
            if (!value.HasValue)
            {
                return false;
            }

            var result = !string.IsNullOrEmpty(value.ToString().Trim());

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckNotNullAndEmpty(long value)
        {
            var result = !string.IsNullOrEmpty(value.ToString().Trim());

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckNotNullAndEmpty(int value)
        {
            var result = !string.IsNullOrEmpty(value.ToString().Trim());

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckNotNullAndEmpty(int? value)
        {
            if (!value.HasValue)
            {
                return false;
            }

            var result = !string.IsNullOrEmpty(value.ToString().Trim());

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckLengthShouldBeEqual(string value, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var result = value?.Trim().Length == length;

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> CheckLengthShouldBeEqualMobileNumber(string value, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            var result = value?.Trim().Length == length;

            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static bool IsValidEmail(string email)
        {
            return RegexUtilities.IsValidEmail(email);
        }
        private static bool IsValidMobile(string mobile)
        {
            return RegexUtilities.IsMobileValid(mobile);
        }
        private static bool IsValidMeterNumber(string MeterNumber)
        {
            if (string.IsNullOrEmpty(MeterNumber))
            {
                return false;
            }
            if (MeterNumber == "string")
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(MeterNumber, "^[0-9A-Z]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static async Task<bool> CheckLengthShouldBeEqual(int? value, int length)
        {
            if (!value.HasValue)
            {
                return false;
            }
            var result = value.ToString().Trim().Length == length;
            return await Task.FromResult(result).ConfigureAwait(false);
        }
        private static async Task<bool> IsDateFormatValid(string value)
        {
            bool result = true;
            if (!string.IsNullOrEmpty(value))
            {
                string[] dateFormats = { "yyyy-MM-dd", "yyyy-MM-d" };
                bool validDate = DateTime.TryParseExact(value, dateFormats, new CultureInfo("en-US"), DateTimeStyles.None, out parsedDate);
                if (validDate)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            return await Task.FromResult(result).ConfigureAwait(false);
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

        private static async Task<bool> IsFileValid(IFormFile file, int filesize)
        {
            if (file.Length < 1)
                return false;
            string[] supportedTypes = new[] { "jpg", "jpeg", "png", "pdf" };
            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
            if (!supportedTypes.Contains(fileExt.ToLower()))
                return false;
            if (file.Length > (filesize * 1024 * 1024))
                return false;

            return true;
        }
        private static async Task<bool> IsImageValid(IFormFile file, int filesize)
        {
            if (file.Length < 1)
                return false;
            string[] supportedTypes = new[] { "jpg", "jpeg", "png" };
            var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
            if (!supportedTypes.Contains(fileExt.ToLower()))
                return false;
            if (file.Length > (filesize * 1024 * 1024))
                return false;

            return true;
        }

        private static bool IsPasswordValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;
            if (password == "string")
                return false;
            try
            {
                //return Regex.IsMatch(password, @"^[\w-]+(?:\.[\w-]+)*@(?:[\w-]+\.-)+[a-zA-Z]{2,7}$");
                //return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[$@!%*?&#^&():;=+\|{}[/>()-_])[A-Za-z\d$@!%*?&#^&():;=+\|{}[/>()-_]");
                return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~])[A-Za-z\d!@#$%^&*()_+\-=[\]{};':""\\|,.<>/?`~]{8,15}$");

            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (name == "string")
                return false;
            try
            {
                return Regex.IsMatch(name, "^[a-zA-Z '-]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
      
        private static bool IsValidGroupName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (name == "string")
                return false;
            try
            {
                return Regex.IsMatch(name, "^[a-zA-Z0-9- _.~%]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return false;
            }
            if (fullName == "string")
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(fullName, "^[a-zA-Z ]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidNumber(string Number)
        {
            if (string.IsNullOrEmpty(Number))
            {
                return false;
            }
            if (Number == "string")
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(Number, "^[0-9]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidTopUpAmount(double amount)
        {

            try
            {
                return Regex.IsMatch(amount.ToString(), "^\\d+(?:\\.\\d+)?$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidExpiryCardMonthYear(string validThru)
        {
            if (string.IsNullOrEmpty(validThru))
            {
                return false;
            }
            if (validThru == "string")
            {
                return false;
            }
            try
            {
                return Regex.IsMatch(validThru, "^(0[1-9]|1[0-2])\\/[0-9]{4}$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidInput(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            if (name == "string")
                return false;
            try
            {
                return Regex.IsMatch(name, @"^[a-zA-Z0-9-.,& ]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        //private static bool IsValidTargetConsumption(double Number)
        //{
        //    try
        //    {
        //        return Regex.IsMatch(Number.ToString(), @"^[0-9]+[.]?[0-9]+|[0-9]$");
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        private static bool IsValidTargetConsumption(double Number)
        {
            try
            {
                return Regex.IsMatch(Number.ToString(), @"^\d+(\.\d+)?$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidContractEndDate(double Number)
        {
            try
            {
                return Regex.IsMatch(Number.ToString(), @"^[0-9]+[.]?[0-9]+|[0-9]$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidExpiryDate(string Number)
        {
            try
            {
                return Regex.IsMatch(Number.ToString(), @"^[0-9]+[.]?[0-9]+|[0-9]$");
            }
            catch (Exception)
            {
                return false;
            }
        }


        private static bool IsValidumber(string Number)
        {
            if (string.IsNullOrWhiteSpace(Number))
                return false;
            if (Number == "string")
                return false;
            try
            {
                return Regex.IsMatch(Number.ToString(), @"^[0-9]+$");
            }
            catch (Exception)
            {
                return false;
            }
        }
        private static bool IsValidEmailMobile(string email)
        {
            return RegexUtilities.IsValidMobileEmail(email);
        }
        #endregion
    }
}
