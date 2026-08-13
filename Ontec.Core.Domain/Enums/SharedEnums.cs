using System.ComponentModel.DataAnnotations;

namespace Ontec.Core.Domain.Enums
{
    public enum SharedEnums
    {
    }
    public enum StatusEnum : short
    {
        Active = 1,
        InComplete = 2,
        NotVerified = 3,
        Pending = 4,
        Inactive = 5,
        Rejected = 6,
        Sent = 7,
        Read = 8,
        InProcess = 9,
        Deactive = 10,
        All = 11
    }
    public enum PaymentStatus : short
    {

        Complete = 1,
        Refunded = 2,
        Revoked = 3,
        Pending = 4,
        Failed = 5,
        OnHold = 6,
        Abandoned = 7,
        Preapproved = 8,
        Cancelled = 9,
        InProcess = 10
    }
    public enum Title : short
    {
        Mr = 2,
        Mrs = 3,
        Ms = 4

    }

    public enum TopUpStatusEnum : short
    {
        PayFastSuccess = 1,
        DebitechSuccess = 2,
        VendSuccess = 3,
        VendFailed = 4,
        DebitechValidationFailed = 5,
        TrailVendSuccess = 6,
        TrailVendFailed = 7,
    }

    public enum NotificationType : short
    {
        Register = 1,
        Notification = 2,
        Invitation = 3,
        Deleted = 4,
        Updated = 5,
        MeterBust = 6,
        MeterLeak = 7,
        LowBalance = 8,
        Usage = 9,
        firebase=10,
    }
    public enum Note_status
    {
        Open = 1,
        Close = 2
    }
    public enum PermissionEnum : short
    {
        IsPermitted = 1,
        NotPermitted = 0
    }
    public enum CommunicationTypeEnum : short
    {
        Mobile = 1,
        Email = 2
    }
    public enum RoleMasterEnum : short
    {
        Customer = 1,
        Admin = 2,
        SuperAdmin = 3,
        Temporary = 4,
        Operator = 5
    }

    public enum PropertyUserRelationEnum : short
    {
        Tenant = 1,
        Associate = 2
    }

    public enum DocumentTypeEnum
    {
        ContractProof = 1,
        DrivingLicense = 3,
        Passport = 4,
        SouthAfricanID = 5
    }
    public enum ConsumptionCylceEnum
    {

        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Yearly = 4
    }
    public enum LoginStatus
    {
        Success = 1,
        Failed = 0,
        AccountLocked = 5
    }
    public enum TransactionPeriod
    {
        [Display(Name = "30 Days")]
        ThirtyDays = 1,

        [Display(Name = "60 Days")]
        SixtyDays = 2,

        [Display(Name = "90 Days")]
        NinetyDays = 3,

        Custom = 4

    }
    public enum STSTransactionPeriod
    {
        [Display(Name = "This Month")]
        ThisMonth = 1,

        [Display(Name = "Last Month")]
        LastMonth = 2,

        Custom = 3

    }
    public enum TransactionType
    {
        Credit = 1,
        Debit = 2,
        Refunded = 3
    }
    public enum DashboardPeriod
    {

        Today = 1,

        Yesterday = 2,

        [Display(Name = "7D")]
        SevenDays = 3,

        [Display(Name = "30D")]
        ThirtyDays = 4,

        [Display(Name = "3M")]
        ThreeMonths = 5,

        [Display(Name = "6M")]
        SixMonths = 6,

        [Display(Name = "12M")]
        TwelveMonths = 7,

        Custom = 8

    }
    public enum ImageType
    {
        Small = 1,
        Regular = 2,
        Favicon = 3
    }
    public enum UpdateStatusEnum
    {
        User = 1,
        Meter = 2,
        Reason = 3,
        Operator = 4,
        TermConditionsVersion = 5,
        GroupCategory = 6,
        Property = 7,
        TopUp = 8,

    }
    public enum PaymentMethodsEnum
    {
        [Display(Name = "Electronic Fund Transfer")]
        EFT = 1,
        [Display(Name = "Debit Card")]
        DebitCard = 2,
        [Display(Name = "Credit Card")]
        CreditCard = 3,
        [Display(Name = "Wallet")]
        Wallet = 4,
        [Display(Name = "bank-transfer")]
        DebitecDeposits = 4,

    }
    public enum PaymentGatewaysEnum
    {
        Payfast = 1,
        LekkaPay = 2

    }
}
