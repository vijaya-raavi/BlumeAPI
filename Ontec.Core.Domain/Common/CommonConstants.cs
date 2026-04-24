namespace Ontec.Core.Domain.Common
{
    public static class CommonConstants
    {
        public const string AlreadyExist = "{0} already registered.";
        public const string InActiveUserExist = "Your account is currently inactive. Please reach out to our support team for assistance.";
        public const string InRejectedUserExist = "Your registration request is rejected. Please reach out to our support team for assistance.";
        public const string NotExist = "{0} does not exist.";
        public const string MeterIsNotPending = "{0} is not pending.";
        public const string UserIsNotPending = "User {0} is not pending.";
        public const string InValid = "{0} is invalid.";
        public const string IsRequired = "{0} is required.";
        public const string AreRequired = "{0} are required.";
        public const string MeterNotInPendingStatus = "{0} is not in pending status.";
        public const string Unauthorized = "You are not authorized.";
        public const string MeterNotExist = "Invalid meter.";
        public const string NotActive = "{0} is not active.";
        public const string MeterVerifyied = "Meter verified successfully!";
        public const string PropertyTenantsNotEditable = "Property tenants/associate are not editable.";
        public const string NotAllowed = "The old and new password must be different.";
        public const string NotMatch = "The new and confirm password must be same.";
        public const string InValidPeriod = "From date and to date must contain maximum 90 days period only.";
        public const string InValidFromDate = "From date should be less than to date.";
        public const string InValidToDate = "To date should be greater than from date.";
        public const string DailyTargetMinLimit = "Target consumption is less than minimum limit.";
        public const string DailyTargetMaxLimit = "Target consumption is more than maximum limit.";
        public const string TopUpMinLimit = "TopUp amount below minimum limit.";
        public const string TopUpMaxLimit = "TopUp amount above maximum limit.";
        public const string DeActivatedAccount = "Your account has been locked, please contact to admin.";
        public const string PermissionNotExist = "Permission not exists for this operator.";
    }
}
