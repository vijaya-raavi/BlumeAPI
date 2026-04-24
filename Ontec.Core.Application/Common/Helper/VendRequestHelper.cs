using System;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ontec.Core.Domain.Common.Helper;
using Ontec.Core.Domain.Interface.Common;
using Ontec.Core.Domain.Interface.Meter;
using Ontec.Core.Domain.Models.Dto.Common;
using Ontec.Core.Domain.Models.Dto.Document;
using Ontec.Core.Domain.Models.Dto.TopUp;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Requests.VendRequest.Commands;
using static Ontec.Core.Domain.Models.Dto.STSPurchase.STSPurchasesDto;

namespace Ontec.Core.Application.Common.Helper
{
    public interface IVendRequestHelper
    {
        Task<VendRequestResponse> Vend(SendVendRequestModel request);
        Task<VendRequestResponse> ProcessRecharge(GetTopUpTransaction transaction, string vendPayMethod);
        Task<TrailVendResponse> ProcessVendTrail(GetTopUpTransaction transaction, string vendPayMethod);
        Task<TrailVendResponse> VendTrail(SendVendRequestModel request);
        Task<IEnumerable<VendRequestResponse>> ProcessSTSRequest(SendVendSTSRequestCommand request);
        Task<IEnumerable<VendRequestResponse>> STSVend(SendSTSRequestModel request);
        Task<VendReverseRequestResponse> VendReverse(SendVendReverseRequestModel request);
    }
    public class VendRequestHelper : IVendRequestHelper
    {
        private readonly IMeterRepository _meterRepository;
        private readonly ICompanyHelper _companyHelper;
        private readonly IWorkContext _workContext;
        private readonly ILogger<VendRequestHelper> _logger;
        public VendRequestHelper(IMeterRepository meterRepository, ICompanyHelper companyHelper,
            IWorkContext workContext, ILogger<VendRequestHelper> logger)
        {
            _meterRepository = meterRepository;
            _companyHelper = companyHelper;
            _workContext = workContext;
            _logger = logger;
        }
        public async Task<VendRequestResponse> ProcessRecharge(GetTopUpTransaction transaction, string vendPayMethod)
        {
            _logger.LogInformation("In Process Recharge");
            var result = new VendRequestResponse()
            {
                StatusCode = 500,
                Message = "Something went wrong."
            };
            var meters = await _meterRepository.GetMeterById(transaction.MeterId).ConfigureAwait(false);
            try
            {
                if (meters != null)
                {
                    //  var meter = meters.FirstOrDefault().MeterNumber;

                    var amount = transaction.RechargeAmount;
                    //TODO: Check min max value 
                    var vendRequest = new SendVendRequestModel
                    {
                        Amount = amount,
                        Meter = meters.MeterNumber.ToUpper(),
                        NumTokens = 1,
                        PayType = vendPayMethod,
                        TransactionNumber = transaction.TransactionID
                    };
                    result = await Vend(vendRequest).ConfigureAwait(false);
                    _logger.LogInformation("In Vend Complete");
                }
                else
                {
                    result = new VendRequestResponse()
                    {
                        StatusCode = 400,
                        Message = "No meter found."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("In process recharge:" + ex.Message);
                _logger.LogError(ex.Message);
            }
            return result;
        }
        public async Task<TrailVendResponse> ProcessVendTrail(GetTopUpTransaction transaction, string vendPayMethod)
        {
            var result = new TrailVendResponse()
            {
                StatusCode = 500,
                Message = "Something went wrong."
            };
            var meter = await _meterRepository.GetMetersById(transaction.MeterId).ConfigureAwait(false);
            try
            {
                if (meter != null)
                {
                    var vendRequest = new SendVendRequestModel
                    {
                        Amount = transaction.RechargeAmount,
                        Meter = meter.MeterNumber,
                        NumTokens = 1,
                        PayType = vendPayMethod,
                        TransactionNumber = transaction.TransactionID
                    };
                    result = await VendTrail(vendRequest).ConfigureAwait(false);
                    _logger.LogInformation("In trai vend Complete");
                }
                else
                {
                    result = new TrailVendResponse()
                    {
                        StatusCode = 400,
                        Message = "No meter found."
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("In process vend trail catch:" + ex.Message);
                _logger.LogError(ex.Message);
            }

            return result;
        }


        public async Task<VendRequestResponse> Vend(SendVendRequestModel request)
        {
            _logger.LogInformation("In Process Vend Trail Recharge");
            var result = new VendRequestResponse()
            {
                StatusCode = 500,
                Message = "Something went wrong while sending request to recharge vend."
            };
            var reverseResult = new VendReverseRequestResponse();
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;
            formattedTime = formattedTime.Replace('.', ':');
            HttpResponseMessage httpsresponse;
            Random generator = new Random();
            var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            int count = 100;
            var xmlData = "";
            var responseMsg = "";
            int rptCount = 0;
            int vendReqCount = 0;
            var orignalRefNo = rndNumber + count.ToString();
            var companyDetails = await _companyHelper.GetCompany(2).ConfigureAwait(false);
            try
            {
                LogToFile(companyDetails.WWWPath, "Befor Preapare xmData");
            vendrequest:
                if (vendReqCount < 5)
                {
                    LogToFile(companyDetails.WWWPath, "Entered in Do loop");
                    xmlData = "<ipayMsg client=\"OntecHome\" term=\"00001\" seqNum=\"2\" time=\"" + formattedTime + "\">"
                                        + "<elecMsg ver=\"2.53\">"
                                        + "<vendReq>"
                                        + "<ref>" + orignalRefNo + "</ref>"
                                        + "<amt cur=\"ZAR\">" + (request.Amount * 100) + "</amt>"
                                        + "<numTokens>" + request.NumTokens + "</numTokens>"
                                        + "<meter>" + request.Meter.ToUpper() + "</meter>"
                                        + "<payType>" + request.PayType + "</payType>"
                                        + "</vendReq>"
                                        + "</elecMsg>"
                                        + "</ipayMsg>";

                    LogToFile(companyDetails.WWWPath, "After formed xmlData :" + xmlData);
                    httpsresponse = await OverHttpAsync(xmlData).ConfigureAwait(false);

                    LogToFile(companyDetails.WWWPath, "After hit vend API");
                    LogToFile(companyDetails.WWWPath, httpsresponse.ToString());
                    if (httpsresponse.StatusCode == HttpStatusCode.OK)
                    {
                        LogToFile(companyDetails.WWWPath, "OK then in if loop");
                        // Check if the request was successful (status code 200-299)
                        if (httpsresponse.IsSuccessStatusCode)
                        {
                            LogToFile(companyDetails.WWWPath, "if loop of :" + httpsresponse.IsSuccessStatusCode.ToString());
                            // Read response content
                            string responseContent = await httpsresponse.Content.ReadAsStringAsync();
                            Console.WriteLine("Response:");
                            Console.WriteLine(responseContent);
                            responseMsg = responseContent;
                            LogToFile(companyDetails.WWWPath, "if loop of :" + responseMsg);
                        }
                        else
                        {
                            result.Message = httpsresponse.ReasonPhrase.ToString();
                            LogToFile(companyDetails.WWWPath, "else loop :" + result.Message);
                        }
                        if (!string.IsNullOrEmpty(responseMsg))
                        {
                            VendResponseSuccess(ref result, xmlData, ref responseMsg, companyDetails, _logger);
                        }
                        else
                        {
                            result.Message = httpsresponse.ReasonPhrase.ToString();
                            LogToFile(companyDetails.WWWPath, "else loop :" + result.Message);
                        }
                    }

                    else if (httpsresponse.StatusCode == HttpStatusCode.GatewayTimeout || httpsresponse.StatusCode == HttpStatusCode.RequestTimeout) // max request count here
                    {
                        count++;
                        LogToFile(companyDetails.WWWPath, "RequestTimeOut");
                        var reverseReq = new SendVendReverseRequestModel
                        {
                            OriginRef = orignalRefNo,
                            OriginTime = formattedTime,
                            TransactionNumber = request.TransactionNumber,
                            RepeatCount = rptCount

                        };
                        LogToFile(companyDetails.WWWPath, "Reverse Request Created");
                        string logstring = " Reverse Request OriginRef : " + reverseReq.OriginRef;
                        logstring += "  ||  Reverse Request OriginTime " + reverseReq.OriginTime;
                        logstring += "  ||  Reverse Request TransactionNumber " + reverseReq.TransactionNumber;
                        logstring += "  ||  Reverse Request RepeatCount " + reverseReq.RepeatCount;
                        LogToFile(companyDetails.WWWPath, logstring);

                        LogToFile(companyDetails.WWWPath, "Before hit reverse request");

                        reverseResult = await VendReverse(reverseReq).ConfigureAwait(false);

                        LogToFile(companyDetails.WWWPath, "After hit reverse request ,code: " + reverseResult.Code + " text : " + reverseResult.Text + " Message " + reverseResult.Message);

                        switch (reverseResult.StatusCode)
                        {
                            case "elec000":
                            case "elec003":
                                vendReqCount++;
                                goto vendrequest;

                            case "elec004":
                            default:
                                break;
                        }

                        if (!string.IsNullOrEmpty(reverseResult.Response) && reverseResult.Text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                        {
                            result = new VendRequestResponse()
                            {
                                StatusCode = 200,
                                Message = "Recharge done successfully!",
                                Response = reverseResult.Response,
                                Token = reverseResult.Token,
                                keyChangeToken = reverseResult.keyChangeToken,
                                bsstToken = reverseResult.bsstToken,
                                mrktMsg = reverseResult.mrktMsg,
                                customerMsg = reverseResult.customerMsg,
                                ReceiptNumber = reverseResult.ReceiptNumber,
                                Tarrif = reverseResult.Tarrif

                            };
                            if (!string.IsNullOrEmpty(reverseResult.Token) && reverseResult.Token != "00000000000000000000")
                            {
                                result.Message = " Your token is " + reverseResult.Token; //token for STS meter
                            }
                        }
                        else
                        {
                            result = new VendRequestResponse()
                            {
                                StatusCode = 500,
                                Message = reverseResult.Text,
                                Response = reverseResult.Text
                            };
                        }
                    }

                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Vend catch : " + ex.Message);
                result.Message = ex.Message;
            }
            return result;
        }

        private static void VendResponseSuccess(ref VendRequestResponse result, string xmlData, ref string responseMsg, CompanyDetailsDto companyDetails, ILogger _logger)
        {
            _logger.LogInformation("In Vend response success");
            LogToFile(companyDetails.WWWPath, "if loop responseMsg not null :" + responseMsg);
            try
            {
                XDocument xmlDoc = XDocument.Parse(responseMsg);

                string jsonString = JsonConvert.SerializeXNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.LoadXml(responseMsg);
                //string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);
                LogToFile(companyDetails.WWWPath, "before parse jsonstring :" + jsonString);
                JObject data = JObject.Parse(jsonString);

                LogToFile(companyDetails.WWWPath, "after parse jsonstring :" + data);
                string vendReference = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["ref"];
                string code = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["res"]["@code"];
                string text = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["res"]["#text"];
                string stdToken = "";
                string keyChangeToken = "";
                string bsstToken = "";
                string mrktMsg = "";
                string customerMsg = "";
                string RCTNum = "";
                string tarrif = "";
                if (jsonString.Contains("stdToken"))
                {
                    stdToken = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["stdToken"]["#text"];
                    tarrif = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["stdToken"]["@tariff"];
                    RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["stdToken"]["@rctNum"];
                }
                if (jsonString.Contains("debt"))
                {
                    int debtCount = (Int32)data["ipayMsg"]["elecMsg"]["vendRes"]["debt"].Count();
                    if (debtCount != 8)
                    {
                        for (int i = 0; i < debtCount; i++)
                        {
                            RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["debt"][i]["@rctNum"];
                        }
                    }
                    else
                    {
                        RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["debt"]["@rctNum"];
                    }
                }
                if (jsonString.Contains("fixed"))
                {
                    int fixedCount = (Int32)data["ipayMsg"]["elecMsg"]["vendRes"]["fixed"].Count();
                    if (fixedCount != 5)
                    {
                        for (int i = 0; i < fixedCount; i++)
                        {
                            RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["fixed"][i]["@rctNum"];
                        }
                    }
                    else
                    {
                        RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@rctNum"];
                    }
                }
                //if (jsonString.Contains("fixed"))
                //{
                //    RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@rctNum"];
                //}
                if (jsonString.Contains("bsstToken"))
                {
                    RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["bsstToken"]["@rctNum"];
                }
                if (jsonString.Contains("keyChangeToken"))

                {
                    var keyChangeCodes = data["ipayMsg"]["elecMsg"]["vendRes"]["keyChangeToken"]["code"].Select(c => (string)c);
                    keyChangeToken = string.Join(",", keyChangeCodes); // Concatenate tokens with a space or another delimiter
                }
                if (jsonString.Contains("bsstToken"))
                {
                    bsstToken = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["bsstToken"]["#text"];
                }
                if (jsonString.Contains("mrktMsg"))
                {
                    mrktMsg = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["mrktMsg"];
                }
                if (jsonString.Contains("customerMsg"))
                {
                    customerMsg = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["customerMsg"];
                }
                responseMsg = jsonString;

                if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                {
                    result = new VendRequestResponse()
                    {
                        StatusCode = 200,
                        Message = "Recharge done successfully!",
                        Response = responseMsg,
                        Token = stdToken,
                        keyChangeToken = keyChangeToken,
                        bsstToken = bsstToken,
                        mrktMsg = mrktMsg,
                        customerMsg = customerMsg,
                        ReceiptNumber = RCTNum,
                        Tarrif = tarrif,
                        VendReference = vendReference

                    };
                    if (!string.IsNullOrEmpty(stdToken) && stdToken != "00000000000000000000")
                    {
                        result.Message = " Your token is " + stdToken; //token for STS meter
                    }
                }
                else
                {
                    result = new VendRequestResponse()
                    {
                        StatusCode = 500,
                        Message = text,
                        Response = text
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("vend success catch : " + ex.Message);
                result = new VendRequestResponse()
                {
                    StatusCode = 500,
                    Message = "Exception occured " + ex.Message,
                    Response = responseMsg

                };
                LogToFile(companyDetails.WWWPath, "In catch :" + responseMsg);
                var applicationLogger = new ApplicationLogger
                {
                    Request = JsonConvert.SerializeObject(xmlData),
                    Method = "VendResponse",
                    Error = ex.Message
                };
            }

        }

        public async Task<TrailVendResponse> VendTrail(SendVendRequestModel request)
        {
            _logger.LogInformation("In Vend trail");
            var result = new TrailVendResponse()
            {
                StatusCode = 500,
                Message = "Server not responding, please try again later."
            };
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;
            formattedTime = formattedTime.Replace('.', ':');
            HttpResponseMessage httpsresponse;
            Random generator = new Random();
            var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            int count = 100;
            try
            {
                var xmlData = "<ipayMsg client=\"OntecHome\" term=\"00001\" seqNum=\"2\" time=\"" + formattedTime + "\">"
                                             + "<elecMsg ver=\"2.53\">"
                                             + "<trialVendReq>"
                                             + "<ref>" + rndNumber + count.ToString() + "</ref>"
                                             + "<amt cur=\"ZAR\">" + (request.Amount * 100) + "</amt>"
                                             + "<numTokens>" + request.NumTokens + "</numTokens>"
                                             + "<meter>" + request.Meter.ToUpper() + "</meter>"
                                             + "<payType>" + request.PayType + "</payType>"
                                             + "</trialVendReq>"
                                             + "</elecMsg>"
                                             + "</ipayMsg>";
                httpsresponse = await OverHttpAsync(xmlData).ConfigureAwait(false);

                var responseMsg = "";
                // Check if the request was successful (status code 200-299)
                if (httpsresponse.IsSuccessStatusCode)
                {
                    // Read response content
                    string responseContent = await httpsresponse.Content.ReadAsStringAsync();
                    Console.WriteLine("Response:");
                    Console.WriteLine(responseContent);
                    responseMsg = responseContent;
                }

                if (!string.IsNullOrEmpty(responseMsg))
                {
                    try
                    {
                        XDocument xmlDoc = XDocument.Parse(responseMsg);

                        string jsonString = JsonConvert.SerializeXNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

                        JObject data = JObject.Parse(jsonString);
                        string code = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["res"]["@code"];
                        string text = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["res"]["#text"];
                        var stdToken = "";
                        var keyChangeToken = "";
                        var bsstToken = "";
                        string units = "";
                        string amt = "";
                        string tax = "";
                        string tariff = "";
                        string debtDesc = "";
                        double debtTotal = 0.0;
                        double debt = 0.0;

                        if (jsonString.Contains("stdToken"))
                        {
                            stdToken = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["stdToken"]["#text"];
                            units = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["stdToken"]["@units"];
                            amt = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["stdToken"]["@amt"];
                            tax = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["stdToken"]["@tax"];
                            tariff = data["ipayMsg"]["elecMsg"]["trialVendRes"]["stdToken"]["@tariff"]?.ToString();
                        }
                        if (jsonString.Contains("keyChangeToken"))
                        {
                            var keyChangeCodes = data["ipayMsg"]["elecMsg"]["trialVendRes"]["keyChangeToken"]["code"].Select(c => (string)c);
                            keyChangeToken = string.Join(" ", keyChangeCodes); // Concatenate tokens with a space or another delimiter
                        }
                        if (jsonString.Contains("bsstToken"))
                        {
                            bsstToken = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["bsstToken"]["#text"];
                        }
                        if (jsonString.Contains("debt"))
                        {
                            int debtCount = (Int32)data["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"].Count();

                            if (debtCount != 8)
                            {

                                for (int i = 0; i < debtCount; i++)
                                {
                                    debt = (double)data["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"][i]["@amt"];
                                    if (debt > 0)
                                    {
                                        debtTotal += debt;
                                    }

                                    debtDesc = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"][i]["#text"];

                                }
                                debtTotal /= 100;
                                debt = debtTotal;
                            }
                            else
                            {
                                debt = (double)data["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["@amt"];
                                if (debt > 0)
                                {

                                    debt /= 100;
                                }
                                debtDesc = (string)data["ipayMsg"]["elecMsg"]["trialVendRes"]["debt"]["#text"];


                            }
                        }

                        responseMsg = jsonString;

                        if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                        {
                            result = new TrailVendResponse()
                            {
                                StatusCode = 200,
                                Message = "Recharge done successfully!",
                                Response = responseMsg,
                                Debt = debt,
                                DebtDescription = debtDesc
                            };
                            if (!string.IsNullOrEmpty(stdToken))
                            {
                                result.Units = units;
                                result.Amount = Convert.ToDouble(amt);
                                result.Tax = Convert.ToDouble(tax);
                                result.Tariff = tariff;
                            }
                        }
                        else
                        {
                            result = new TrailVendResponse()
                            {
                                StatusCode = 500,
                                Message = text,
                                Response = text
                            };
                        }

                    }
                    catch (Exception ex)
                    {
                        result = new TrailVendResponse()
                        {
                            StatusCode = 500,
                            Message = "Exception occured " + ex.Message,
                            Response = responseMsg,
                        };
                        var applicationLogger = new ApplicationLogger
                        {
                            Request = JsonConvert.SerializeObject(xmlData),
                            Method = "VendTrial",
                            Error = ex.Message
                        };
                    }
                }
                _logger.LogInformation("In trail vend Complete");
            }
            catch (Exception ex)
            {
                _logger.LogError("In trail vend catch:" + ex.Message);
                result.Message = ex.Message;
            }
            return result;
        }
        private async Task<HttpResponseMessage> OverHttpAsync(string xmlData)
        {
            _logger.LogInformation("In master api hit method");
            ServicePointManager.Expect100Continue = false;
            using (HttpClient httpClient = new HttpClient())
            {

                // Define the endpoint URL
                //string url = "http://bizswitch.net:3530";
                //string url = "http://pyxisuat2.bizswitch.net:3530";
                string url = "http://meerkatuat.bizswitch.net:3680";
                // Add Accept-Encoding header
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                //httpClient.DefaultRequestHeaders.Add("Content-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.Add("connection", "alive");
                // Create StringContent with XML data
                var content = new StringContent(xmlData, Encoding.UTF8, "application/xml");
                string responseMsg = string.Empty;
                try
                {
                    _logger.LogInformation("Master api url : " + url);
                    _logger.LogInformation("xml : " + xmlData);
                    // Send POST request
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);
                    _logger.LogInformation("Master api hit Complete");
                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError("In master api hit method catch : " + ex.Message);
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    var applicationLogger = new ApplicationLogger
                    {
                        Request = JsonConvert.SerializeObject(xmlData),
                        Method = "VendResponse",
                        Error = ex.Message
                    };
                }
            }
            return new HttpResponseMessage() { };
        }



        public async Task<IEnumerable<VendRequestResponse>> ProcessSTSRequest(SendVendSTSRequestCommand request)
        {
            var resList = new List<VendRequestResponse>();
            resList.Add(new VendRequestResponse
            {
                StatusCode = 500,
                Message = "Something went wrong."
            });
            var meters = await _meterRepository.GetMeterByMeterNumber(request.Meter.ToUpper()).ConfigureAwait(false);
            if (meters != null)
            {
                //  var meter = meters.FirstOrDefault().MeterNumber;

                //var amount = transaction.RechargeAmount;
                //TODO: Check min max value 
                var stsRequest = new SendSTSRequestModel
                {
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    Meter = meters.MeterNumber.ToUpper()

                };
                resList = (List<VendRequestResponse>)await STSVend(stsRequest).ConfigureAwait(false);
            }
            else
            {
                resList.Add(new VendRequestResponse
                {
                    StatusCode = 400,
                    Message = "No meter found."
                });
            }

            return resList;
        }


        public async Task<IEnumerable<VendRequestResponse>> STSVend(SendSTSRequestModel request)
        {
            _logger.LogInformation("In STS Vend");
            var resList = new List<VendRequestResponse>();
            resList.Add(new VendRequestResponse
            {
                StatusCode = 500,
                Message = "Something went wrong while sending request to recharge vend."
            });

            string fromtimeZoneOffset = request.FromDate.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedFromTime = request.FromDate.ToString("yyyy-MM-dd 00:00:00") + " " + "+0200";
            formattedFromTime = formattedFromTime.Replace('.', ':');

            string totimeZoneOffset = request.ToDate.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedToTime = request.ToDate.ToString("yyyy-MM-dd 23:59:59") + " " + "+0200";
            formattedToTime = formattedToTime.Replace('.', ':');

            HttpResponseMessage httpsresponse;
            Random generator = new Random();
            var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            int count = 100;
            try
            {
                do
                {
                    var xmlData = "<ipayMsg client = \"OntecHome\" term = \"00001\" seqNum = \"1\" time = \"{{RequestTs}}\">"
                                              + "<elecMsg ver=\"2.6a\">"
                                              + "<reprintManyReq>"
                                              + "<ref>" + rndNumber + count.ToString() + "</ref>"
                                              + "<meter>" + request.Meter.ToUpper() + "</meter>"
                                              + "<searchCriteria>"
                                              + "<fromDate>" + formattedFromTime + "</fromDate>"
                                              + "<toDate>" + formattedToTime + "</toDate>"
                                              + "</searchCriteria>"
                                              + "</reprintManyReq>"
                                              + "</elecMsg>"
                                              + "</ipayMsg>";
                    httpsresponse = await OverHttpAsync(xmlData).ConfigureAwait(false);
                    count++;
                } while (httpsresponse.StatusCode == HttpStatusCode.RequestTimeout);

                var responseMsg = "";
                // Check if the request was successful (status code 200-299)
                if (httpsresponse.IsSuccessStatusCode)
                {
                    // Read response content
                    string responseContent = await httpsresponse.Content.ReadAsStringAsync();
                    Console.WriteLine("Response:");
                    Console.WriteLine(responseContent);
                    responseMsg = responseContent;
                }
                else
                {
                    resList.Add(new VendRequestResponse
                    {
                        Message = httpsresponse.ReasonPhrase.ToString()
                    });
                }

                if (!string.IsNullOrEmpty(responseMsg))
                {
                    try
                    {
                        XDocument xmlDoc = XDocument.Parse(responseMsg);
                        var serializer = new XmlSerializer(typeof(List<VendRe>));

                        string jsonString = JsonConvert.SerializeXNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

                        //XmlDocument xmlDoc = new XmlDocument();
                        //xmlDoc.LoadXml(responseMsg);
                        //string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);

                        JObject data = JObject.Parse(jsonString);
                        var dataAll = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(jsonString);
                        string code = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["res"]["@code"];
                        string text = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["res"]["#text"];
                        string stdToken = "";
                        string keyChangeToken = "";
                        string bsstToken = "";
                        string mrktMsg = "";
                        string customerMsg = "";
                        string RCTNum = "";
                        string ipay = "";
                        string TxnDate = "";
                        string tarrif = "";
                        var vendResponses = "";
                        string resultJson = "";
                        if (dataAll != null)
                        {
                            if (jsonString.Contains("vendRes"))
                            {
                                //int vendResponseCount = (Int32)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"].Count();
                                // int index = 0;
                                var vendResArray = data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"];

                                // List to hold individual JSON strings
                                var vendResToken = data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"];
                                List<string> vendres = new List<string>();
                                //List<JToken> vendResList = new();
                                if (vendResToken != null && vendResToken.Type == JTokenType.Array)
                                {
                                    // It's an array, iterate over it
                                    foreach (var item in vendResToken)
                                    {
                                        var stdTokenNode = item["stdToken"];
                                        if (stdTokenNode != null)
                                        {

                                            stdToken = (string)stdTokenNode["#text"];
                                            tarrif = (string)stdTokenNode["@tariff"];
                                            RCTNum = (string)stdTokenNode["@rctNum"];

                                        }
                                        TxnDate = (string)item["@transDate"];


                                        if (item["debt"] != null)
                                        {
                                            var debtToken = item["debt"];
                                            if (debtToken != null && debtToken.Type == JTokenType.Array)
                                            {
                                                foreach (var debt in debtToken)
                                                {
                                                    RCTNum = (string)debt["@rctNum"];
                                                }
                                            }
                                            else
                                            {
                                                RCTNum = (string)debtToken["@rctNum"];
                                            }
                                        }

                                        // fixed
                                        if (item["fixed"] != null)
                                        {
                                            var fixedToken = item["fixed"];
                                            if (fixedToken != null && fixedToken.Type == JTokenType.Array)
                                            {
                                                foreach (var fix in fixedToken)
                                                {
                                                    RCTNum = (string)fix["@rctNum"];
                                                }
                                            }
                                            else
                                            {
                                                RCTNum = (string)fixedToken["@rctNum"];
                                            }
                                        }

                                        // bsstToken
                                        if (item["bsstToken"] != null)
                                        {
                                            RCTNum = (string)item["bsstToken"]["@rctNum"];
                                            bsstToken = (string)item["bsstToken"]["#text"];
                                        }

                                        // keyChangeToken
                                        if (item["keyChangeToken"] != null && item["keyChangeToken"]["code"] != null)
                                        {
                                            var keyChangeCodes = item["keyChangeToken"]["code"]
                                                .Select(c => (string)c);
                                            keyChangeToken = string.Join(",", keyChangeCodes);
                                        }

                                        // mrktMsg
                                        if (item["mrktMsg"] != null)
                                        {
                                            mrktMsg = (string)item["mrktMsg"];
                                        }

                                        // customerMsg
                                        if (item["customerMsg"] != null)
                                        {
                                            customerMsg = (string)item["customerMsg"];


                                        }
                                        string vendResJson = JsonConvert.SerializeObject(item);

                                        var singleVendResJson = new
                                        {
                                            ipayMsg = new
                                            {
                                                client = dataAll["ipayMsg"]["@client"],
                                                term = dataAll["ipayMsg"]["@term"],
                                                seqnum = dataAll["ipayMsg"]["@seqNum"],
                                                time = dataAll["ipayMsg"]["@time"],
                                                elecMsg = new
                                                {
                                                    ver = dataAll["ipayMsg"]["elecMsg"]["@ver"],
                                                    service = dataAll["ipayMsg"]["elecMsg"]["@service"],
                                                    reprintManyRes = new
                                                    {
                                                        refr = dataAll["ipayMsg"]["elecMsg"]["reprintManyRes"]["ref"],
                                                        res = dataAll["ipayMsg"]["elecMsg"]["reprintManyRes"]["res"],
                                                        vendRes = item // Single vendRes
                                                    }
                                                }
                                            }
                                        };
                                        resultJson = JsonConvert.SerializeObject(singleVendResJson, Formatting.Indented);
                                        responseMsg = resultJson;

                                        /// TxnDate = ["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["@transDate"];
                                        if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            resList.Add(new VendRequestResponse
                                            {
                                                StatusCode = 200,
                                                Message = "Recharge done successfully!",
                                                Response = responseMsg,
                                                Token = stdToken,
                                                keyChangeToken = keyChangeToken,
                                                bsstToken = bsstToken,
                                                mrktMsg = mrktMsg,
                                                customerMsg = customerMsg,
                                                ReceiptNumber = RCTNum,
                                                TxnDatetime = TxnDate
                                            });
                                            if (!string.IsNullOrEmpty(stdToken) && stdToken != "00000000000000000000")
                                            {
                                                resList.Add(new VendRequestResponse
                                                {
                                                    Message = " Your token is " + stdToken //token for STS meter
                                                });
                                            }
                                        }
                                        else
                                        {
                                            resList.Add(new VendRequestResponse
                                            {
                                                StatusCode = 500,
                                                Message = text,
                                                Response = text
                                            });
                                        }

                                    }
                                }
                                else if (vendResToken is JObject)
                                {
                                    string vendResJson = JsonConvert.SerializeObject(vendResToken);

                                    var singleVendResJson = new
                                    {
                                        ipayMsg = new
                                        {
                                            client = dataAll["ipayMsg"]["@client"],
                                            term = dataAll["ipayMsg"]["@term"],
                                            seqnum = dataAll["ipayMsg"]["@seqNum"],
                                            time = dataAll["ipayMsg"]["@time"],
                                            elecMsg = new
                                            {
                                                ver = dataAll["ipayMsg"]["elecMsg"]["@ver"],
                                                service = dataAll["ipayMsg"]["elecMsg"]["@service"],
                                                reprintManyRes = new
                                                {
                                                    refr = dataAll["ipayMsg"]["elecMsg"]["reprintManyRes"]["ref"],
                                                    res = dataAll["ipayMsg"]["elecMsg"]["reprintManyRes"]["res"],
                                                    vendRes = vendResToken // Single vendRes
                                                }
                                            }
                                        }
                                    };
                                    resultJson = JsonConvert.SerializeObject(vendResToken, Formatting.Indented);

                                    JObject jsonObj = JObject.Parse(resultJson);

                                    var stdTokenNode = jsonObj["stdToken"];
                                    if (stdTokenNode != null)
                                    {

                                        stdToken = (string)stdTokenNode["#text"];
                                        tarrif = (string)stdTokenNode["@tariff"];
                                        RCTNum = (string)stdTokenNode["@rctNum"];

                                    }


                                    if (jsonObj["debt"] != null)
                                    {
                                        var debtToken = jsonObj["debt"];
                                        if (debtToken != null && debtToken.Type == JTokenType.Array)
                                        {
                                            foreach (var debt in debtToken)
                                            {
                                                RCTNum = (string)debt["@rctNum"];
                                            }
                                        }
                                        else
                                        {
                                            RCTNum = (string)debtToken["@rctNum"];
                                        }
                                    }

                                    // fixed
                                    if (jsonObj["fixed"] != null)
                                    {
                                        var fixedToken = jsonObj["fixed"];
                                        if (fixedToken != null && fixedToken.Type == JTokenType.Array)
                                        {
                                            foreach (var fix in fixedToken)
                                            {
                                                RCTNum = (string)fix["@rctNum"];
                                            }
                                        }
                                        else
                                        {
                                            RCTNum = (string)fixedToken["@rctNum"];
                                        }
                                    }

                                    // bsstToken
                                    if (jsonObj["bsstToken"] != null)
                                    {
                                        RCTNum = (string)jsonObj["bsstToken"]["@rctNum"];
                                        bsstToken = (string)jsonObj["bsstToken"]["#text"];
                                    }

                                    // keyChangeToken
                                    if (jsonObj["keyChangeToken"] != null && jsonObj["keyChangeToken"]["code"] != null)
                                    {
                                        var keyChangeCodes = jsonObj["keyChangeToken"]["code"]
                                            .Select(c => (string)c);
                                        keyChangeToken = string.Join(",", keyChangeCodes);
                                    }

                                    // mrktMsg
                                    if (jsonObj["mrktMsg"] != null)
                                    {
                                        mrktMsg = (string)jsonObj["mrktMsg"];
                                    }

                                    // customerMsg
                                    if (jsonObj["customerMsg"] != null)
                                    {
                                        customerMsg = (string)jsonObj["customerMsg"];


                                    }
                                    // Access @transDate
                                    TxnDate = (string)jsonObj["@transDate"];

                                    if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        resList.Add(new VendRequestResponse
                                        {
                                            StatusCode = 200,
                                            Message = "Recharge done successfully!",
                                            Response = JsonConvert.SerializeObject(singleVendResJson, Formatting.Indented),
                                            Token = stdToken,
                                            keyChangeToken = keyChangeToken,
                                            bsstToken = bsstToken,
                                            mrktMsg = mrktMsg,
                                            customerMsg = customerMsg,
                                            ReceiptNumber = RCTNum,
                                            TxnDatetime = TxnDate
                                        });
                                        if (!string.IsNullOrEmpty(stdToken) && stdToken != "00000000000000000000")
                                        {
                                            resList.Add(new VendRequestResponse
                                            {
                                                Message = " Your token is " + stdToken //token for STS meter
                                            });
                                        }
                                    }
                                    else
                                    {
                                        resList.Add(new VendRequestResponse
                                        {
                                            StatusCode = 500,
                                            Message = text,
                                            Response = text
                                        });
                                    }

                                }
                            }
                            //else
                            //{
                            //    TxnDate = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["@transDate"];
                            //    if (jsonString.Contains("stdToken"))
                            //    {
                            //        stdToken = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["stdToken"]["#text"];
                            //        tarrif = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["stdToken"]["@tariff"];
                            //        RCTNum = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["stdToken"]["@rctNum"];
                            //    }
                            //    if (jsonString.Contains("debt"))
                            //    {
                            //        int debtCount = (Int32)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["debt"].Count();
                            //        if (debtCount != 8)
                            //        {
                            //            for (int i = 0; i < debtCount; i++)
                            //            {
                            //                RCTNum = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["debt"][i]["@rctNum"];
                            //            }
                            //        }
                            //        else
                            //        {
                            //            RCTNum = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["debt"]["@rctNum"];
                            //        }
                            //    }
                            //    if (jsonString.Contains("fixed"))
                            //    {
                            //        RCTNum = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["fixed"]["@rctNum"];
                            //    }
                            //    if (jsonString.Contains("bsstToken"))
                            //    {
                            //        RCTNum = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["bsstToken"]["@rctNum"];
                            //    }
                            //    if (jsonString.Contains("keyChangeToken"))

                            //    {
                            //        var keyChangeCodes = data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["keyChangeToken"]["code"].Select(c => (string)c);
                            //        keyChangeToken = string.Join(",", keyChangeCodes); // Concatenate tokens with a space or another delimiter
                            //    }
                            //    if (jsonString.Contains("bsstToken"))
                            //    {
                            //        bsstToken = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["bsstToken"]["#text"];
                            //    }
                            //    if (jsonString.Contains("mrktMsg"))
                            //    {
                            //        mrktMsg = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["mrktMsg"];
                            //    }
                            //    if (jsonString.Contains("customerMsg"))
                            //    {
                            //        customerMsg = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["customerMsg"];
                            //    }
                            //    responseMsg = jsonString;

                            //    if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                            //    {
                            //        resList.Add(new VendRequestResponse
                            //        {
                            //            StatusCode = 200,
                            //            Message = "Recharge done successfully!",
                            //            Response = responseMsg,
                            //            Token = stdToken,
                            //            keyChangeToken = keyChangeToken,
                            //            bsstToken = bsstToken,
                            //            mrktMsg = mrktMsg,
                            //            customerMsg = customerMsg,
                            //            ReceiptNumber = RCTNum,
                            //            TxnDatetime = TxnDate
                            //        });
                            //        if (!string.IsNullOrEmpty(stdToken) && stdToken != "00000000000000000000")
                            //        {
                            //            resList.Add(new VendRequestResponse
                            //            {
                            //                Message = " Your token is " + stdToken //token for STS meter
                            //            });
                            //        }
                            //    }
                            //    else
                            //    {
                            //        resList.Add(new VendRequestResponse
                            //        {
                            //            StatusCode = 500,
                            //            Message = text,
                            //            Response = text
                            //        });
                            //    }
                            //}
                        }
                    }

                    catch (Exception ex)
                    {
                        resList.Add(new VendRequestResponse
                        {
                            StatusCode = 500,
                            Message = "Exception occured " + ex.Message,
                            Response = responseMsg
                        }); ;
                    }
                }
            }
            catch (Exception ex)
            {
                resList.Add(new VendRequestResponse
                {
                    Message = ex.Message
                });
            }
            return resList;
        }
        public async Task<VendReverseRequestResponse> VendReverse(SendVendReverseRequestModel request)
        {
            _logger.LogError("In Vend Reverse Method");
            var companyDetails = await _companyHelper.GetCompany(2).ConfigureAwait(false);
            LogToFile(companyDetails.WWWPath, "In  VendReverse function ");
            var result = new VendReverseRequestResponse()
            {
                StatusCode = "500",
                Message = "Something went wrong while sending reverse request to vend."
            };
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;
            formattedTime = formattedTime.Replace('.', ':');
            HttpResponseMessage httpsresponse;
            Random generator = new Random();
            var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            int count = 100;
            var xmlData = "";
            var responseMsg = "";
            int rptCount = 0;
            string code = "";
            string text = "";
            try
            {
                LogToFile(companyDetails.WWWPath, "Before XmData formation ");
                do
                {
                    xmlData = "<ipayMsg client=\"OntecHome\" term=\"00001\" seqNum=\"2\" time=\"" + formattedTime + "\">"
                                            + "<elecMsg ver=\"2.53\">"
                                            + "<vendRevReq repCount=\"" + rptCount + "\" origTime=\"" + request.OriginTime + "\">"
                                            + "<ref>" + rndNumber + count.ToString() + "</ref>"
                                            + "<origRef>" + request.OriginRef + "</origRef>"
                                            + "</vendRevReq></elecMsg></ipayMsg>";

                    LogToFile(companyDetails.WWWPath, "after XmData formation :" + xmlData);
                    LogToFile(companyDetails.WWWPath, "repeat count :" + rptCount);
                    httpsresponse = await OverHttpAsync(xmlData).ConfigureAwait(false);

                    LogToFile(companyDetails.WWWPath, "after hit api :" + httpsresponse);
                } while (httpsresponse.StatusCode == HttpStatusCode.GatewayTimeout || httpsresponse.StatusCode == HttpStatusCode.RequestTimeout);
                rptCount++;

                LogToFile(companyDetails.WWWPath, "repeat count :" + rptCount);
                // Check if the request was successful (status code 200-299)
                if (httpsresponse.IsSuccessStatusCode)
                {
                    // Read response content
                    string responseContent = await httpsresponse.Content.ReadAsStringAsync();
                    Console.WriteLine("Response:");
                    Console.WriteLine(responseContent);
                    responseMsg = responseContent;
                    LogToFile(companyDetails.WWWPath, "if httpsresponse.IsSuccessStatusCode loop :" + responseMsg);
                }
                else
                {
                    result.Message = httpsresponse.ReasonPhrase.ToString();
                }

                if (!string.IsNullOrEmpty(responseMsg))
                {

                    try
                    {
                        VendReverseResponseSuccess(ref result, ref responseMsg, companyDetails,_logger);


                        //LogToFile(companyDetails.WWWPath, "if responseMsg not null loop :" + responseMsg);
                        //XDocument xmlDoc = XDocument.Parse(responseMsg);

                        //string jsonString = JsonConvert.SerializeXNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);
                        //LogToFile(companyDetails.WWWPath, "before json parse :" + jsonString);
                        //JObject data = JObject.Parse(jsonString);
                        //LogToFile(companyDetails.WWWPath, "after json parse :" + data);
                        //code = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["res"]["@code"];
                        //text = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["res"]["#text"];
                    }
                    catch (Exception ex)
                    {
                        result = new VendReverseRequestResponse()
                        {
                            StatusCode = "500",
                            Message = "Exception occured " + ex.Message,
                            Response = responseMsg,
                            Code = code,
                            Text = text
                        };
                        LogToFile(companyDetails.WWWPath, "In catch :" + responseMsg);
                        var applicationLogger = new ApplicationLogger
                        {
                            Request = JsonConvert.SerializeObject(xmlData),
                            Method = "VendReverseResponse",
                            Error = ex.Message
                        };
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("In Vend Reverse catch : " + ex.Message); ;
                result.Message = ex.Message;
            }
            return result;
        }
        private static void VendReverseResponseSuccess(ref VendReverseRequestResponse result, ref string responseMsg, CompanyDetailsDto companyDetails, ILogger _logger)
        {
            _logger.LogError("In Vend Reverse Success");
            LogToFile(companyDetails.WWWPath, "if loop responseMsg not null :" + responseMsg);
            try
            {
                XDocument xmlDoc = XDocument.Parse(responseMsg);

                string jsonString = JsonConvert.SerializeXNode(xmlDoc, Newtonsoft.Json.Formatting.Indented);

                //XmlDocument xmlDoc = new XmlDocument();
                //xmlDoc.LoadXml(responseMsg);
                //string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);
                LogToFile(companyDetails.WWWPath, "before parse jsonstring :" + jsonString);
                JObject data = JObject.Parse(jsonString);
                LogToFile(companyDetails.WWWPath, "after parse jsonstring :" + data);
                string revcode = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["res"]["@code"];
                string revtext = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["res"]["#text"];
                if (revcode == "elec004")
                {
                    string code = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["res"]["@code"];
                    string text = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["res"]["#text"];
                    string stdToken = "";
                    string keyChangeToken = "";
                    string bsstToken = "";
                    string mrktMsg = "";
                    string customerMsg = "";
                    string RCTNum = "";
                    string tarrif = "";
                    if (jsonString.Contains("stdToken"))
                    {
                        stdToken = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["#text"];
                        tarrif = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["@tariff"];
                        RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["@rctNum"];
                    }
                    if (jsonString.Contains("debt"))
                    {
                        int debtCount = (Int32)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["debt"].Count();
                        if (debtCount != 8)
                        {
                            for (int i = 0; i < debtCount; i++)
                            {
                                RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["debt"][i]["@rctNum"];
                            }
                        }
                        else
                        {
                            RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["debt"]["@rctNum"];
                        }
                    }
                    if (jsonString.Contains("fixed"))
                    {
                        int fixedCount = (Int32)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["fixed"].Count();
                        if (fixedCount != 5)
                        {
                            for (int i = 0; i < fixedCount; i++)
                            {
                                RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["fixed"][i]["@rctNum"];
                            }
                        }
                        else
                        {
                            RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["fixed"]["@rctNum"];
                        }
                    }
                    //if (jsonString.Contains("fixed"))
                    //{
                    //    RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["fixed"]["@rctNum"];
                    //}
                    if (jsonString.Contains("bsstToken"))
                    {
                        RCTNum = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["bsstToken"]["@rctNum"];
                    }
                    if (jsonString.Contains("keyChangeToken"))

                    {
                        var keyChangeCodes = data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["keyChangeToken"]["code"].Select(c => (string)c);
                        keyChangeToken = string.Join(",", keyChangeCodes); // Concatenate tokens with a space or another delimiter
                    }
                    if (jsonString.Contains("bsstToken"))
                    {
                        bsstToken = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["bsstToken"]["#text"];
                    }
                    if (jsonString.Contains("mrktMsg"))
                    {
                        mrktMsg = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["mrktMsg"];
                    }
                    if (jsonString.Contains("customerMsg"))
                    {
                        customerMsg = (string)data["ipayMsg"]["elecMsg"]["vendRevRes"]["vendRes"]["customerMsg"];
                    }
                    responseMsg = jsonString;

                    if (!string.IsNullOrEmpty(revtext) && revtext.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = new VendReverseRequestResponse()
                        {
                            StatusCode = "200",
                            Message = "Recharge done successfully!",
                            Response = responseMsg,
                            Token = stdToken,
                            keyChangeToken = keyChangeToken,
                            bsstToken = bsstToken,
                            mrktMsg = mrktMsg,
                            customerMsg = customerMsg,
                            ReceiptNumber = RCTNum,
                            Tarrif = tarrif,
                            Text = text,

                        };
                        if (!string.IsNullOrEmpty(stdToken) && stdToken != "00000000000000000000")
                        {
                            result.Message = " Your token is " + stdToken; //token for STS meter
                        }
                    }
                    else if (!string.IsNullOrEmpty(revtext) && revtext.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = new VendReverseRequestResponse()
                        {
                            StatusCode = revcode,
                            Message = revtext,
                            Response = responseMsg
                        };
                    }
                }
                else if (!string.IsNullOrEmpty(revcode) && !string.IsNullOrEmpty(revtext))
                {
                    result = new VendReverseRequestResponse()
                    {
                        StatusCode = revcode,
                        Message = revtext,
                        Response = responseMsg
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("In Vend Reverse Success catch : " + ex.Message);
                result = new VendReverseRequestResponse()
                {
                    StatusCode = "500",
                    Message = "Exception occured " + ex.Message,
                    Response = responseMsg

                };
                LogToFile(companyDetails.WWWPath, "In catch :" + responseMsg);
                //var applicationLogger = new ApplicationLogger
                //{
                //    Request = JsonConvert.SerializeObject(xmlData),
                //    Method = "VendReverseResponse",
                //    Error = ex.Message
                //};
            }

        }

        static void LogToFile(string filePath, string message)
        {

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            string fileName = "topUpError.txt";
            filePath = Path.Combine(filePath, fileName);
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
    }
}
