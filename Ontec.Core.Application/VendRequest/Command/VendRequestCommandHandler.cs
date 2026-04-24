using MediatR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ontec.Core.Domain.Models.Dto.VendRequest;
using Ontec.Core.Domain.Requests.VendRequest.Commands;
using System.Text;
using System.Xml;

namespace Ontec.Core.Application.VendRequest.Command
{
    public class VendRequestCommandHandler : IRequestHandler<SendVendRequestCommand, VendRequestResponse>,
                                             IRequestHandler<SendVendSTSRequestCommand, IEnumerable<STSVendRequestResponse>>
    {
        public async Task<VendRequestResponse> Handle(SendVendRequestCommand request, CancellationToken cancellationToken)
        {
            var result = new VendRequestResponse()
            {
                StatusCode = 500,
                Message = "Something went wrong."
            };
            string timeZoneOffset = DateTime.UtcNow.ToString("zzz").Replace(":", ""); //"+0200";
            string formattedTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + timeZoneOffset;
            formattedTime = formattedTime.Replace('.', ':');
            //Random generator = new Random();
            //var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            var xmlData = "<ipayMsg client=\"OntecHome\" term=\"00001\" seqNum=\"2\" time=\"" + formattedTime + "\">"
                                     + "<elecMsg ver=\"2.53\">"
                                       + "<vendReq>"
                                         + "<ref>" + request.TransactionNumber + "</ref>"
                                         + "<amt cur=\"ZAR\">" + (request.Amount * 100) + "</amt>"
                                         + "<numTokens>" + request.NumTokens + "</numTokens>"
                                         + "<meter>" + request.Meter + "</meter>"
                                         + "<payType>" + request.PayType + "</payType>"
                                       + "</vendReq>"
                                     + "</elecMsg>"
                                   + "</ipayMsg>";
            var response = await OverHttpAsync(xmlData).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response);
                    string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);

                    // using System.Xml.Serialization;
                    //XmlSerializer serializer = new XmlSerializer(typeof(IpayMessage));
                    //using (StringReader reader = new StringReader(response))
                    //{
                    //   var test = (IpayMessage)serializer.Deserialize(reader);
                    //}
                    
                    JObject data = JObject.Parse(jsonString);
                    string code = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["res"]["@code"];
                    string text = (string)data["ipayMsg"]["elecMsg"]["vendRes"]["res"]["#text"];

                    if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                    {
                        result = new VendRequestResponse()
                        {
                            StatusCode = 200,
                            Message = "Recharge done successfully!"
                        };
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return result;
        }


        public async Task<IEnumerable<STSVendRequestResponse>> Handle(SendVendSTSRequestCommand request, CancellationToken cancellationToken)
        {
            var responseList = new List<STSVendRequestResponse>();
            responseList.Add(new STSVendRequestResponse
            {
                StatusCode = 500,
                Message = "Something went wrong."
            });
            Random generator = new Random();
            var rndNumber = generator.Next(0, 1000000000).ToString("D9");
            var xmlData = "<ipayMsg client=\"OntecHome\" term=\"00001\" seqNum=\"1\" time=\" {{RequestTs}} \">"
                                     + "<elecMsg ver=\"2.6a\">"
                                       + "<reprintManyReq>"
                                         + "<ref>" + rndNumber + "</ref>"
                                         + "<meter>" + request.Meter + "</meter>"
                                         + "<searchCriteria><fromDate>"+request.FromDate+ "</fromDate><toDate>"+request.ToDate+ "</toDate></searchCriteria>"
                                       + "</reprintManyReq>"
                                     + "</elecMsg>"
                                   + "</ipayMsg>";
            var response = await OverHttpAsync(xmlData).ConfigureAwait(false);

            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(response);
                    string jsonString = JsonConvert.SerializeXmlNode(xmlDoc);

                    // using System.Xml.Serialization;
                    //XmlSerializer serializer = new XmlSerializer(typeof(IpayMessage));
                    //using (StringReader reader = new StringReader(response))
                    //{
                    //   var test = (IpayMessage)serializer.Deserialize(reader);
                    //}

                    JObject data = JObject.Parse(jsonString);
                    string code = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["@code"];
                    string text = (string)data["ipayMsg"]["elecMsg"]["reprintManyRes"]["vendRes"]["#text"];

                    if (!string.IsNullOrEmpty(text) && text.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
                    {
                        responseList.Add(new STSVendRequestResponse
                        {
                            StatusCode = 200,
                            Message = "Request done successfully!"
                        });
                    }
                }
                catch (Exception ex)
                {

                }
            }
            return responseList;
        }



        private async Task<string> OverHttpAsync(string xmlData)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                // Define the endpoint URL
                //string url = "http://bizswitch.net:3530";
                string url = "http://pyxisuat2.bizswitch.net:3530/";
                //string url = "http://meerkatuat.bizswitch.net:3680/"; 
                // Add Accept-Encoding header
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                //httpClient.DefaultRequestHeaders.Add("Content-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.Add("connection", "alive");
                // Create StringContent with XML data
                var content = new StringContent(xmlData, Encoding.UTF8, "application/xml");
                string responseMsg = string.Empty;
                try
                {
                    // Send POST request
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    // Check if the request was successful (status code 200-299)
                    if (response.IsSuccessStatusCode)
                    {
                        // Read response content
                        string responseContent = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response:");
                        Console.WriteLine(responseContent);
                        responseMsg = responseContent;
                    }
                    else
                    {
                        Console.WriteLine($"Failed with status code {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                return responseMsg;
            }
        }
    }
}