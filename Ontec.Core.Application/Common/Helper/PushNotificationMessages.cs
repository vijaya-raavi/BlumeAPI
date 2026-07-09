using System.Text;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface;
using Ontec.Core.Domain.Interface.Document;
using Ontec.Core.Domain.Interface.User;
using Ontec.Core.Domain.Models.Dto;
using Ontec.Core.Domain.Models.Dto.Document;

namespace Ontec.Core.Application.Common.Helper
{
    public interface IPushNotification
    {
        Task<PushNotificationDto> SendMessage(string title, string body, string deviceToken, int userId);
        Task<string> SendMessageToGroupAsync(string notificationKey, string title, string body);
    }
    public class PushNotification : IPushNotification
    {
        private readonly IGenericRepository _genericRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PushNotification> _logger;
        private IHostingEnvironment _environment;
        private readonly ICompanyHelper _companyHelper;
        private readonly IDocumentRepository _documentRepository;

        public PushNotification(IGenericRepository genericRepository,
                                IUserRepository userRepository,
                                ILogger<PushNotification> logger,
                                IHostingEnvironment environment,
                                ICompanyHelper companyHelper,
                                IDocumentRepository documentRepository)
        {
            _genericRepository = genericRepository;
            _userRepository = userRepository;
            _environment = environment;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _companyHelper = companyHelper;
            _documentRepository = documentRepository;
        }

        public async Task<PushNotificationDto> SendMessage(string title, string body, string deviceToken, int userId)
        {
            var res = new PushNotificationDto();
            var user = await _userRepository.GetUserById(userId).ConfigureAwait(false);
            var companyDetails = await _companyHelper.GetCompany(user.CompanyId).ConfigureAwait(false);
            try
            {

                var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";

                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(jsonPath),
                    });
                }

                LogToFile(companyDetails.WWWPath, "before Send notification");
                LogToFile(companyDetails.WWWPath, deviceToken);
                LogToFile(companyDetails.WWWPath, title);
                LogToFile(companyDetails.WWWPath, body);
                var message = new Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body,
                    },
                    Data = new Dictionary<string, string>()
                    {
                        ["FirstName"] = user.FirstName,
                        ["LastName"] = user.LastName
                    },

                    Token = deviceToken
                };

                var messaging = FirebaseMessaging.DefaultInstance;

                var result = await messaging.SendAsync(message);
                LogToFile(companyDetails.WWWPath, "after Send notification");
                if (!string.IsNullOrEmpty(result))
                {
                    res.ResponseMsg = "Messgae sent successfully";
                    LogToFile(companyDetails.WWWPath, res.ResponseMsg);
                    return res;
                }
                else
                {

                    _logger.LogTrace(null, "An exception occurred while sending notification to user {UserId}", userId);
                    res.ResponseMsg = "Messgae not sent";
                    LogToFile(companyDetails.WWWPath, res.ResponseMsg);
                    var applicationLogger = new ApplicationLogger
                    {
                        Request = companyDetails.WWWPath,
                        Method = "Create firebase log file",
                        Error = res.ResponseMsg
                    };
                    await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
                    return res;
                }
            }
            catch (Exception ex)
            {
                LogToFile(companyDetails.WWWPath, ex.ToString());
                _logger.LogTrace(ex, "An exception occurred while sending notification to user {UserId}", userId);
                res.ResponseMsg = "Messgae not sent";
                LogToFile(companyDetails.WWWPath, res.ResponseMsg);

                var applicationLogger = new ApplicationLogger
                {
                    Request = companyDetails.WWWPath,
                    Method = "Create firebase log file",
                    Error = ex.Message
                };
                await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
            }
            return res;


        }

        public async Task<string> SendMessageToGroupAsync(string topic, string title, string body)
        {
            var response = "";
            using var client = new HttpClient();
            var companyDetails = await _companyHelper.GetCompany(3).ConfigureAwait(false);
            var jsonPath = _environment.ContentRootPath + "\\serviceAccountKey.json";
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(jsonPath),

                });
            }
            try
            {
                var message = new Message()
                {
                    Topic = topic,
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>()
                    {
                    }

                };

                LogToFile(companyDetails.WWWPath, "before Send notification");
                LogToFile(companyDetails.WWWPath, title);
                LogToFile(companyDetails.WWWPath, body);
                response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                LogToFile(companyDetails.WWWPath, response);
                return response;
            }
            catch (Exception ex)
            {
                var applicationLogger = new ApplicationLogger
                {
                    Request = topic,
                    Method = "Create firebase log file",
                    Error = ex.Message
                };
                await _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
                return response;
            }
            //return responseContent;
        }


        public void LogToFile(string filePath, string message)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string fileName = "firebaseError.txt";
            filePath = Path.Combine(filePath, fileName);
            try
            {
                // Ensure directory exists

                if (!File.Exists(filePath))
                {
                    // Create the file
                    using (FileStream fs = File.Create(filePath))
                    {
                        // Optionally write something to the file
                        byte[] content = new UTF8Encoding(true).GetBytes("File created successfully.");
                        fs.Write(content, 0, content.Length);
                    }
                    Console.WriteLine("File created.");
                }
                // Append log with timestamp
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
                File.AppendAllText(filePath, logEntry); // Append log to file
            }
            catch (Exception ex)
            {
                var applicationLogger = new ApplicationLogger
                {
                    Request = "FilePath:" + filePath + ", FileName :  " + fileName,
                    Method = "Create firebase log file",
                    Error = ex.Message
                };
                _documentRepository.AddApplicationLogger(applicationLogger).ConfigureAwait(false);
            }
        }
    }

}
