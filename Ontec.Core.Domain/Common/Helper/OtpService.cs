using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Models;
using Ontec.Core.Domain.Models.Dto.Common;


namespace Ontec.Core.Domain.Common.Helper
{

    public interface IOtpService
    {

        Task<string> SendMobileOtp(string otp, string body, string mobile);
        Task<string> SendEmailOtp(string otp, string body, string email);
        Task<string> SendRegisterOtp(string otp, string body, string email, int communicationId);
        Task<string> SendForgotPasswordOtp(string otp, string body, string email);//, int communicationId);
        Task<string> SendNotificationOtp(string otp, string body, string email);
        Task<string> SendEventMail(EmailModelClass obj);
        Task<string> SendTransactionStatement(EmailModelClass obj);
        Task<string> SendPurchaseReceipt(EmailModelClass obj);

    }
    public class OtpService(ICompanyHelper companyHelper, ILogger<OtpService> logger) : IOtpService
    {
        private readonly ILogger<OtpService> _logger = logger;
        private readonly ICompanyHelper _companyHelper = companyHelper;


        private async Task<CompanyDetailsDto> GetCompmanydetails(int companyId)
        {
            return await _companyHelper.GetCompany(companyId).ConfigureAwait(false);
        }
        private async Task<string> getMailContent(string propertyUser, string body, string email, string mobile, string evt, string companyName, string subtitle)
        {

            string mailContent = "";
            if (evt == "newPropertyUser")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>You have been granted access to " + companyName + @" account " + body;
                mailContent += @"</td></tr><tr><td align='left' valign='top' style='font-size: 17px; font-weight: 400; line-height: 160%; border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 25px; color: #000000; font-family: sans-serif;' class='paragraph'>
                                <b style='color: #333333;'>Sign up with email-id or mobile: </b><br>" + email;
                mailContent += @" </b><br>" + mobile;
                mailContent += @"</td></tr>";
            }
            if (evt == "RejectUser")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";

                mailContent += @"</td></tr><tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>
                                <br>" + body + "<br> Reason: " + subtitle + @"<br>Kindly contact the Administratior for more details.</td></tr>";
            }
            if (evt == "MeterStatusUpdated")
            {

                string reasonPart = "";
                string mainBody = "";
                if (body.Contains("/"))
                {
                    string[] parts = body.Split("/");

                    // Extracting both parts
                    mainBody = parts[0].Trim(); // "Meter Number: XXX : Approved" or "Meter Number: XXX : Rejected"
                    reasonPart = parts[1].Trim();
                }
                else
                {
                    mainBody = body;
                }
                mailContent = @"<tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + subtitle;
                mailContent += @"</td></tr><br>";


                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + "," + @"</td></tr>";
                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + mainBody + @".</td></tr>";
                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + reasonPart + @"</td></tr>";
            }

            if (evt == "ApproveUser")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"<br>" + body + "<br>Your request was approved.</td></tr>";
            }
            if (evt == "MeterStatus")
            {
                //string reasonPart = "";
                //string mainBody = "";
                //if (body.Contains("/"))
                //{
                //    string[] parts = body.Split("/");

                //    // Extracting both parts
                //    mainBody = parts[0].Trim(); // "Meter Number: XXX : Approved" or "Meter Number: XXX : Rejected"
                //    reasonPart = parts[1].Trim();
                //}
                //else
                //{
                //    mainBody = body;
                //}
                mailContent = @"<tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + subtitle;
                mailContent += @"</td></tr><br>";


                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'></td></tr>";
                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + body + @".</td></tr>";
                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'></td></tr>";
            }


            if (evt == "newUser")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>You are successfully registered into  " + companyName + " .";
                mailContent += @"<br>Kindly update your profile to proceed.</td></tr>";

                //mailContent += body;
            }

            if (evt == "ResetPassword")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>You are about to reset your " + companyName + @" password.<br> Confirmation OTP: " + body;
                mailContent += @"</td></tr>";
            }
            if (evt == "ContactUpdate")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + body;
                mailContent += @"</td></tr>";
            }
            if (evt == "PasswordChanged")
            {
                //mailContent += body;
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser;
                mailContent += @", </td></tr>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>"+body;
                mailContent += @"</td></tr>";
            }
            if (evt == "Deregister")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi,";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>You have not logged into your  " + companyName + @" Profile for more than year , as a consequence, your profile is  deactivated.
                                <br>Please contact the Administrator if access is still required.</td></tr>";
            }
            if (evt == "AccountDelete")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser;
                mailContent += @",</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Your account is deleted from  " + companyName + @"
                                Please report suspicious activity to the Administrator if it is not you that logged in.</td></tr>";
            }
            if (evt == "ConsumerDelete")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser;
                mailContent += @",</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Your account is deleted from  " + companyName + @"
                               </td></tr>";
            }
            if (evt == "SuccessFullLogin")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>You have logged into your " + companyName + @" Profile.
                                   <br>Please report suspicious activity to the Administrator if it is not you that logged in.</td></tr>";
            }
            if (evt == "FailedLoginAttempt")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'> " + companyName + @" access locked for 20 minutes due to 5 failed login attempts.
                                Please report to the Administrator if this activity is suspicious.</td></tr>";
            }
            if (evt == "Deactivate")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Your rental contract has expired, and therefore,  your " + companyName + @" profile has been deactivated.
                                Please report to the Administrator if this activity is suspicious</td></tr>";
            }
            if (evt == "SigUpOTP")
            {

                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi,</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Your OTP is " + body;
                mailContent += @"</td></tr>";
            }

            if (evt == "UpdateContactsOTP")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Your OTP to update email is " + body;
                mailContent += @"</td></tr>";
            }
            if (evt == "DownloadStatement")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'> Kindly check the transaction statement attached in this mail";
                mailContent += @"</td></tr><tr><td align='left' valign='top' style='font-size: 17px; font-weight: 400; line-height: 160%; border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-top: 25px; color: #000000; font-family: sans-serif;' class='paragraph'></td></tr>";
            }
            if (evt == "PurchaseReceipt")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";

                mailContent += body;
            }
            if (evt == "TopUpFailed")
            {
                mailContent = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;' class='list-item'>
                                    <table align='center' border='0' cellspacing='0' cellpadding='0' style='width: inherit; margin: 0; padding: 0; border-collapse: collapse; border-spacing: 0;'>
                                    <tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Hi " + propertyUser + ",";
                mailContent += @"</td></tr><br>";
                mailContent += @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>" + body;
                mailContent += @"</td></tr>";
            }
            return mailContent;
        }


        private async Task<string> getMailSubject(string evt, string companyName)
        {
            string subject = "";
            if (evt == "ApproveUser")
            {
                subject = "Registration update ";
            }
            if (evt == "RejectUser")
            {
                subject = "Registration update ";
            }
            if (evt == "newUser")
            {
                subject = "Welcome to " + companyName;
            }
            if (evt == "newPropertyUser")
            {
                subject = companyName + " - User Invite";
            }
            if (evt == "ConsumerDelete")
            {
                subject = companyName + " - User account deleted";
            }

            if (evt == "ResetPassword")
            {
                subject = "Password Reset";
            }
            if (evt == "PasswordChanged")
            {
                subject = "Password Changed";
            }
            if (evt == "Deregister")
            {
                subject = companyName + " -User Deregistration";
            }
            if (evt == "AccountDelete")
            {
                subject = companyName + " -User account deleted";
            }
            if (evt == "SigUpOTP")
            {
                subject = companyName + " -Notification of sign up OTP";
            }
            if (evt == "UpdateContactsOTP")
            {
                subject = companyName + " -Notification of OTP to update contacts";
            }
            if (evt == "SuccessFullLogin")
            {
                subject = companyName + " -Notification of Login";
            }
            if (evt == "FailedLoginAttempt")
            {
                subject = companyName + " -Notification of more than 5 failed Login attempts";
            }
            if (evt == "Deactivation")
            {
                subject = companyName + " - Notification of Profile deactivation";
            }
            if (evt == "MeterStatusUpdated")
            {
                subject = "Meter status update ";
            }
            if (evt == "DownloadStatement")
            {
                subject = companyName + " - Find Transaction Statement";
            }
            if (evt == "PurchaseReceipt")
            {
                subject = companyName + " - Find Purchase Receipt";
            }
            if (evt == "MeterStatus")
            {
                subject = "Meter status update ";
            }
            if (evt == "TopUpFailed")
            {
                subject = companyName + " - Top Up Failed";
            }
            if (evt == "ContactUpdate")
            {
                subject = companyName + " - Update Contacts";
            }
            return subject;

        }



        public async Task<string> SendEmailOtp(string otp, string body, string email)
        {
            await SendEmail(otp, body, email).ConfigureAwait(false);

            return "OTP sent successfully!";
        }

        public async Task<string> SendMobileOtp(string otp, string body, string mobile)
        {
            await SendSMS(otp, body, mobile);

            return "OTP sent successfully!";
        }
        public async Task<string> SendRegisterOtp(string otp, string body, string email, int communicationId)
        {
            if (communicationId == 2)
            {
                await SendEmail(otp, body, email).ConfigureAwait(false);
            }
            return " OTP sent successfully!";
        }
        public async Task<string> SendForgotPasswordOtp(string otp, string body, string email)//, int communicationId)
        {
            //if (communicationId == 2)
            //{
            await SendEmail(otp, body, email).ConfigureAwait(false);
            //}
            return "OTP sent successfully!";
        }
        public async Task<string> SendNotificationOtp(string otp, string body, string email)
        {
            await SendEmail(otp, body, email).ConfigureAwait(false);
            return "OTP sent successfully!";
        }
        public async Task<string> SendEventMail(EmailModelClass obj)
        {
            await SendMailByevents(obj).ConfigureAwait(false);
            return "Mail sent successfully!";
        }
        public async Task<string> SendTransactionStatement(EmailModelClass obj)
        {
            await SendDocument(obj).ConfigureAwait(false);
            return "Transaction statement sent successfully!";
        }
        public async Task<string> SendPurchaseReceipt(EmailModelClass obj)
        {
            await SendPurchaseReceiptDocument(obj).ConfigureAwait(false);
            return "Purchase receipt sent successfully!";
        }
        public async Task<bool> SendMailByevents(EmailModelClass obj)
        {
            var companyDto = new CompanyDetailsDto();
            companyDto = await GetCompmanydetails(obj.companyId).ConfigureAwait(false);
            string html = obj.body;

            // Extract base64 images and replace them with cid references
            var images = HtmlBase64Extractor.ExtractBase64Images(ref html);

            try
            {
                string smtpAddress = "za-smtp-outbound-1.mimecast.co.za";
                int portNumber = 587;
                bool enableSSL = true;
                string emailFromAddress = "ontecportal@ontec.co.za"; //Sender Email Address  
                string password = "yE2fwfC5t"; //Sender Password  
                string emailToAddress = obj.email; //Receiver Email Address  

                string mailContent = await getMailContent(obj.propertyUser, obj.body, obj.email, obj.mobile, obj.forEvent, companyDto.Name, obj.subtitle).ConfigureAwait(false);
                string subject = await getMailSubject(obj.forEvent, companyDto.Name).ConfigureAwait(false);


                string head = @"<html xmlns='http://www.w3.org/1999/xhtml'>
                                    <head><meta http-equiv='content-type' content='text/html; charset=utf-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0;'>
                                    <meta name='format-detection' content='telephone=no'/>";


                string style = @"<style>body { margin: 0; padding: 0; min-width: 100%; width: 100% !important; height: 100% !important;}
                                     body, table, td, div, p, a { -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; }
                                    table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse !important; border-spacing: 0; }
                                    img { border: 0; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
                                     #outlook a { padding: 0; }
                                    .ReadMsgBody { width: 100%; } .ExternalClass { width: 100%; }
                                    .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; }
                                     @media all and (min-width: 560px) {
                                    .container { border-radius: 8px; -webkit-border-radius: 8px; -moz-border-radius: 8px; -khtml-border-radius: 8px;}
                                        }
                                     a, a:hover { color: #127DB3; }
                                    .footer a, .footer a:hover { color: #999999; }</style></head>";
                //if (obj.forEvent == "ResetPassword" || obj.forEvent == "SigUpOTP" || obj.forEvent == "ApproveUser")
                //{
                    string logo = @"<body topmargin='0' rightmargin='0' bottommargin='0' leftmargin='0' marginwidth='0' marginheight='0' width='100%' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;padding-bottom:20px; width: 100%; height: 100%; -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; background-color: #f0f0f0; color: #000000;' bgcolor='#f0f0f0' text='#000000'>
                                     <table width='100%' align='center' border='0' cellpadding='0' cellspacing='0' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; width: 100%;' class='background'><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;' bgcolor='#f0f0f0'>
                                        <table border='0' cellpadding='0' cellspacing='0' align='center' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='wrapper'>
                                            <tr>
                                                <td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 20px;'>
                                                    <img src='" + companyDto.RelativeUrl + @"' border='0' vspace='0' hspace='0' height='50' alt='logo' title='logo' style='color: #000000; font-size: 10px; margin: 0; padding: 0; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; border: none; display: block;' /></a>
                                                </td> </tr> </table>";

                    string titlesubt = @"<table border='0' cellpadding='0' cellspacing='0' align='center' bgcolor='#ffffff' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='container'>
                                  <tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 24px; font-weight: bold; line-height: 130%; padding-top: 25px; color: #000000; font-family: sans-serif;' class='header'>" + obj.title;
                    titlesubt += @"</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-bottom: 3px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 18px; font-weight: 300; line-height: 150%; padding-top: 5px; color: #000000; font-family: sans-serif;' class='subheader'>" + obj.subtitle;
                    titlesubt += @"</td></tr>";

                    string footer = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>kind regards,<br>" + companyDto.Name + @"</td></tr>";

                    //if (forevent == "new invite")
                    //{
                    //    footer += @"<tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 30px; padding-bottom: 35px;' class='button'>
                    //                    <a href='https://blumefe.ontec.co.za/' target='_blank' style='text-decoration: underline;'>
                    //                    <table border='0' cellpadding='0' cellspacing='0' align='center' style='max-width: 240px; min-width: 120px; border-collapse: collapse; border-spacing: 0; padding: 0;'>
                    //                     <tr><td align='center' valign='middle' style='padding: 12px 24px; margin: 0; text-decoration: underline; border-collapse: collapse; border-spacing: 0; border-radius: 4px; -webkit-border-radius: 4px; -moz-border-radius: 4px; -khtml-border-radius: 4px;' bgcolor='#127db3'>
                    //                     <a target='_blank' style='text-decoration: underline;color: #ffffff; font-family: sans-serif; font-size: 17px; font-weight: 400; line-height: 120%;' href='https://blumefe.ontec.co.za/'>sign up</a>
                    //                     </td></tr></table></a></td></tr></table></td></tr><tr>";
                    //}

                    footer += @"<td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%;  width: 87.5%; font-size: 17px; font-weight: 400; line-height: 160%; padding-top: 20px; padding-bottom: 25px; color: #000000; font-family: sans-serif;' class='paragraph'>
                                     for more details visit: <a href='https://blumefe.ontec.co.za/' target='_blank' style='color: #127db3; font-family: sans-serif; font-size: 17px; font-weight: 400; line-height: 160%;'>our website</a>
                                     </td></tr></table></td></tr></table></body></html>";
                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress(emailFromAddress);
                        mail.To.Add(emailToAddress);
                        mail.Subject = subject;
                        if (obj.forEvent == "RejectUser" || obj.forEvent == "ApproveUser"|| obj.forEvent == "ContactUpdate" || obj.forEvent=="AccountDelete"|| obj.forEvent== "FailedLoginAttempt")
                        {
                            mail.Body = head + style + logo + mailContent + footer;
                        }
                        else
                        {
                            mail.Body = head + style + logo + titlesubt + mailContent + footer;
                        }
                        mail.IsBodyHtml = true;
                        using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                        {
                            smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                            smtp.EnableSsl = enableSSL;
                            smtp.UseDefaultCredentials = false;
                            smtp.Send(mail);
                        }
                    }
               // }
                //else
                //{
                //    string htmlFull = head + style + mailContent;

                //    // Step 1️⃣: Extract base64 images and replace with cid:
                //    var imgs = HtmlBase64Extractor.ExtractBase64Images(ref htmlFull);
                //    using (MailMessage mail = new MailMessage())
                //    {
                //        mail.From = new MailAddress(emailFromAddress);
                //        mail.To.Add(emailToAddress);
                //        mail.Subject = subject;
                //        if (obj.forEvent == "RejectUser" || obj.forEvent == "ApproveUser")
                //        {
                //            mail.Body = head + style + mailContent;
                //        }
                //        else
                //        {
                //            mail.Body = head + style + mailContent;
                //        }
                //        mail.IsBodyHtml = true;

                //        var htmlView = AlternateView.CreateAlternateViewFromString(htmlFull, null, MediaTypeNames.Text.Html);

                //        // Step 3️⃣: Add all images as linked resources
                //        foreach (var img in images)
                //        {
                //            byte[] bytes = Convert.FromBase64String(img.Base64Data);
                //            var ms = new MemoryStream(bytes); // No "using" here!
                //            var lr = new LinkedResource(ms, img.MimeType)
                //            {
                //                ContentId = img.ContentId,
                //                TransferEncoding = TransferEncoding.Base64
                //            };
                //            htmlView.LinkedResources.Add(lr);
                //        }

                //        mail.AlternateViews.Add(htmlView);

                //        using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                //        {
                //            smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                //            smtp.EnableSsl = enableSSL;
                //            smtp.UseDefaultCredentials = false;
                //            smtp.Send(mail);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError("IN email catch otp services");
                return false;
            }
            return true;
        }



        public async Task<bool> SendSMS(string otp, string body, string mobile)
        {

            string myURI = "https://api.bulksms.com/v1/messages";
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;

            formattedTime = formattedTime.Replace('.', ':');
            string timeOnly = formattedTime.Substring(11, 8);

            string Username = "itronenergy";
            string Password = "gl@d1at0r";
            if (body == null)
            {
                body = "Your OTP is:" + otp + " requested on " + DateTime.UtcNow.Date + " at " + timeOnly;
            }


            //string myData = "{to: \"+27634004746\", body:\""+body+"\"}";
            string myData = "{to:\"" + mobile + "\", body:\"" + body + "\"}";

            // build the request based on the supplied settings
            var request = WebRequest.Create(myURI);


            // supply the credentials
            request.Credentials = new NetworkCredential(Username, Password);
            request.PreAuthenticate = true;
            // we want to use HTTP POST
            request.Method = "POST";
            // for this API, the type must always be JSON
            request.ContentType = "application/json";

            // Here we use Unicode encoding, but ASCIIEncoding would also work
            var encoding = new UnicodeEncoding();
            var encodedData = encoding.GetBytes(myData);

            // Write the data to the request stream
            var stream = request.GetRequestStream();
            stream.Write(encodedData, 0, encodedData.Length);
            stream.Close();

            // try ... catch to handle errors nicely
            try
            {
                _logger.LogError("SMS : " + encodedData);
                // make the call to the API
                var response = request.GetResponse();

                // read the response and print it to the console
                var reader = new StreamReader(response.GetResponseStream());
                _logger.LogError("SMS : " + response);
                Console.WriteLine(reader.ReadToEnd());
            }
            catch (WebException ex)
            {
                _logger.LogError("sms catch");
                return false;
            }
            return true;
        }


        public async Task<bool> SendEmail(string otp, string body, string email)
        {
            try
            {
                string smtpAddress = "za-smtp-outbound-1.mimecast.co.za";
                int portNumber = 587;
                bool enableSSL = true;
                string emailFromAddress = "ontecportal@ontec.co.za"; //Sender Email Address  
                string password = "yE2fwfC5t"; //Sender Password  
                string emailToAddress = email; //Receiver Email Address  
                string subject = "OTP Notification";


                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.UseDefaultCredentials = false;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }


        public async Task<bool> SendDocument(EmailModelClass obj)
        {
            try
            {

                string smtpAddress = "za-smtp-outbound-1.mimecast.co.za";
                int portNumber = 587;
                bool enableSSL = true;
                string emailFromAddress = "ontecportal@ontec.co.za"; //Sender Email Address  
                string password = "yE2fwfC5t"; //Sender Password  
                string emailToAddress = obj.email; //Receiver Email Address  

                var companyDto = await _companyHelper.GetCompany(obj.companyId).ConfigureAwait(false);
                string mailContent = await getMailContent(obj.propertyUser, obj.body, obj.email, obj.mobile, obj.forEvent, companyDto.Name, obj.subtitle).ConfigureAwait(false);
                string subject = "Transaction Statement";


                string head = @"<html xmlns='http://www.w3.org/1999/xhtml'>
                                    <head><meta http-equiv='content-type' content='text/html; charset=utf-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0;'>
                                    <meta name='format-detection' content='telephone=no'/>";


                string style = @"<style>body { margin: 0; padding: 0; min-width: 100%; width: 100% !important; height: 100% !important;}
                                     body, table, td, div, p, a { -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; }
                                    table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse !important; border-spacing: 0; }
                                    img { border: 0; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
                                     #outlook a { padding: 0; }
                                    .ReadMsgBody { width: 100%; } .ExternalClass { width: 100%; }
                                    .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; }
                                     @media all and (min-width: 560px) {
                                    .container { border-radius: 8px; -webkit-border-radius: 8px; -moz-border-radius: 8px; -khtml-border-radius: 8px;}
                                        }
                                     a, a:hover { color: #127DB3; }
                                    .footer a, .footer a:hover { color: #999999; }</style></head>";

                string logo = @"<body topmargin='0' rightmargin='0' bottommargin='0' leftmargin='0' marginwidth='0' marginheight='0' width='100%' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;padding-bottom:20px; width: 100%; height: 100%; -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; background-color: #F0F0F0; color: #000000;' bgcolor='#F0F0F0' text='#000000'>
                                     <table width='100%' align='center' border='0' cellpadding='0' cellspacing='0' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; width: 100%;' class='background'><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;' bgcolor='#F0F0F0'>
                                        <table border='0' cellpadding='0' cellspacing='0' align='center' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='wrapper'>
                                            <tr>
                                                <td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 20px;'>
                                                    <img src='" + companyDto.RelativeUrl + @"' border='0' vspace='0' hspace='0' height='50' alt='Logo' title='Logo' style='color: #000000; font-size: 10px; margin: 0; padding: 0; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; border: none; display: block;' /></a>
                                                </td> </tr> </table>";

                string titleSubt = @"<table border='0' cellpadding='0' cellspacing='0' align='center' bgcolor='#FFFFFF' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='container'>
                                  <tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 24px; font-weight: bold; line-height: 130%; padding-top: 25px; color: #000000; font-family: sans-serif;' class='header'>" + obj.title;
                titleSubt += @"</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-bottom: 3px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 18px; font-weight: 300; line-height: 150%; padding-top: 5px; color: #000000; font-family: sans-serif;' class='subheader'>";
                titleSubt += @"</td></tr>";

                string footer = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Kind regards,<br>" + companyDto.Name + @"</td></tr>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    mail.Subject = subject;
                    mail.Body = head + style + logo + titleSubt + mailContent + footer;
                    mail.IsBodyHtml = true;
                    mail.Attachments.Add(new Attachment(obj.documentPath));//--Uncomment this to send any attachment 
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.UseDefaultCredentials = false;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<bool> SendPurchaseReceiptDocument(EmailModelClass obj)
        {
            try
            {

                string smtpAddress = "za-smtp-outbound-1.mimecast.co.za";
                int portNumber = 587;
                bool enableSSL = true;
                string emailFromAddress = "ontecportal@ontec.co.za"; //Sender Email Address  
                string password = "yE2fwfC5t"; //Sender Password  
                string emailToAddress = obj.email; //Receiver Email Address  
                var companyDto = await _companyHelper.GetCompany(obj.companyId).ConfigureAwait(false);
                string mailContent = await getMailContent(obj.propertyUser, obj.body, obj.email, obj.mobile, obj.forEvent, companyDto.Name, obj.subtitle).ConfigureAwait(false);
                string subject = "Purchase Receipt";


                string head = @"<html xmlns='http://www.w3.org/1999/xhtml'>
                                    <head><meta http-equiv='content-type' content='text/html; charset=utf-8'>
                                    <meta name='viewport' content='width=device-width, initial-scale=1.0;'>
                                    <meta name='format-detection' content='telephone=no'/>";


                string style = @"<style>body { margin: 0; padding: 0; min-width: 100%; width: 100% !important; height: 100% !important;}
                                     body, table, td, div, p, a { -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; }
                                    table, td { mso-table-lspace: 0pt; mso-table-rspace: 0pt; border-collapse: collapse !important; border-spacing: 0; }
                                    img { border: 0; line-height: 100%; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; }
                                     #outlook a { padding: 0; }
                                    .ReadMsgBody { width: 100%; } .ExternalClass { width: 100%; }
                                    .ExternalClass, .ExternalClass p, .ExternalClass span, .ExternalClass font, .ExternalClass td, .ExternalClass div { line-height: 100%; }
                                     @media all and (min-width: 560px) {
                                    .container { border-radius: 8px; -webkit-border-radius: 8px; -moz-border-radius: 8px; -khtml-border-radius: 8px;}
                                        }
                                     a, a:hover { color: #127DB3; }
                                    .footer a, .footer a:hover { color: #999999; }</style></head>";
                string logo = @"<body topmargin='0' rightmargin='0' bottommargin='0' leftmargin='0' marginwidth='0' marginheight='0' width='100%' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;padding-bottom:20px; width: 100%; height: 100%; -webkit-font-smoothing: antialiased; text-size-adjust: 100%; -ms-text-size-adjust: 100%; -webkit-text-size-adjust: 100%; line-height: 100%; background-color: #F0F0F0; color: #000000;' bgcolor='#F0F0F0' text='#000000'>
                                     <table width='100%' align='center' border='0' cellpadding='0' cellspacing='0' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; width: 100%;' class='background'><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0;' bgcolor='#F0F0F0'>
                                        <table border='0' cellpadding='0' cellspacing='0' align='center' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='wrapper'>
                                            <tr>
                                                <td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; padding-top: 20px;'>
                                                    <img src='" + companyDto.RelativeUrl + @"' border='0' vspace='0' hspace='0' height='50' alt='Logo' title='Logo' style='color: #000000; font-size: 10px; margin: 0; padding: 0; outline: none; text-decoration: none; -ms-interpolation-mode: bicubic; border: none; display: block;' /></a>
                                                </td> </tr> </table>";

                string titleSubt = @"<table border='0' cellpadding='0' cellspacing='0' align='center' bgcolor='#FFFFFF' width='560' style='border-collapse: collapse; border-spacing: 0; padding: 0; width: inherit; max-width: 560px;' class='container'>
                                  <tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 24px; font-weight: bold; line-height: 130%; padding-top: 25px; color: #000000; font-family: sans-serif;' class='header'>" + obj.title;
                titleSubt += @"</td></tr><tr><td align='center' valign='top' style='border-collapse: collapse; border-spacing: 0; margin: 0; padding: 0; padding-bottom: 3px; padding-left: 6.25%; padding-right: 6.25%; width: 87.5%; font-size: 18px; font-weight: 300; line-height: 150%; padding-top: 5px; color: #000000; font-family: sans-serif;' class='subheader'>";
                titleSubt += @"</td></tr>";

                string footer = @"<tr><td align='left' valign='top' style='border-collapse: collapse; border-spacing: 0; padding: 0; padding-bottom: 3px;padding-top: 5px; padding-right: 20px;'>Kind regards,<br>" + companyDto.Name + @"</td></tr>";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(emailFromAddress);
                    mail.To.Add(emailToAddress);
                    mail.Subject = subject;
                    mail.Body = head + style + logo + titleSubt + mailContent + footer;
                    mail.IsBodyHtml = true;
                    mail.Attachments.Add(new Attachment(obj.documentPath));//--Uncomment this to send any attachment 
                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.UseDefaultCredentials = false;
                        smtp.Send(mail);
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public static class HtmlBase64Extractor
        {
            public static List<Base64ImageInfo> ExtractBase64Images(ref string htmlContent)
            {
                var results = new List<Base64ImageInfo>();

                // Regex to match <img src="data:image/...;base64,...">
                string pattern = @"<img[^>]+src=[""']data:(image\/[^;]+);base64,([^""']+)[""'][^>]*>";

                int index = 1;

                htmlContent = Regex.Replace(htmlContent, pattern, match =>
                {
                    string mimeType = match.Groups[1].Value;
                    string base64 = match.Groups[2].Value;

                    var info = new Base64ImageInfo
                    {
                        MimeType = mimeType,
                        Base64Data = base64,
                        ContentId = $"image{index}"
                    };

                    results.Add(info);

                    // Replace with cid reference for email embedding
                    string newTag = match.Value.Replace(match.Groups[0].Value, $"<img src=\"cid:{info.ContentId}\" alt=\"EmbeddedImage{index}\" />");
                    index++;
                    return newTag;
                }, RegexOptions.IgnoreCase);

                return results;
            }
        }
    }
}
