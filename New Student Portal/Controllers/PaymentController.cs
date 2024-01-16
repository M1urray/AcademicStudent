using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using New_Student_Portal.Models;
using New_Student_Portal.ViewModel;
using Newtonsoft.Json.Linq;

namespace New_Student_Portal.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        private static readonly Encoding encoding = Encoding.UTF8;
        public ActionResult MakePayment(string DocNo, string SID)
        {
            try
            {

                string clientMSISDN = Session["PhoneNumber"].ToString();
                string clientName = Session["Name"].ToString();
                string clientEmail = Session["Email"].ToString();
                string clientIDNumber = Session["ID_No"].ToString();
                string RegNo = Session["Username"].ToString();
                string key = "WajVmUaVU5BdKaAg";
                string secret = "qFOof7TQ+LvyVnSv7iah/FsAXozo87X6";
                string billDesc = getServiceDesc(SID);
                string currency = "KES";
                string clientid = "156";
                string amounttopay = DocNo;
                string refno = Credentials.ObjNav.InsertECTZBill(clientMSISDN, clientName, clientIDNumber, SID, Session["Email"].ToString(), billDesc, RegNo, RegNo);
                string data_string = clientid + DocNo + SID + clientIDNumber + currency + refno + billDesc + clientName + secret;
                string secureHash = GetEncoding(data_string, RegNo, key);
                string callback = "https://portal.chuka.ac.ke/Financial/Receipts";
                string notification = "https://api.chuka.ac.ke:8444/ecitizen/notification";
                string postdata = "apiClientID=" + clientid + "&secureHash=" + secureHash + "&billDesc=" + billDesc + "&billRefNumber=" + refno + "&currency=KES&serviceID=" + SID + "&clientMSISDN=" + clientMSISDN + "&clientName=" + clientName + "&clientIDNumber=" + clientIDNumber + "&clientEmail=" + clientEmail + "&callBackURLOnSuccess=" + callback + "&pictureURL=&notificationURL=" + notification + "&amountExpected=" + amounttopay;

                Payment viewModel = new Payment();
                viewModel.apiClientID = clientid;
                viewModel.secureHash = secureHash;
                viewModel.billDesc = billDesc;
                viewModel.billRefNumber = refno;
                viewModel.currency = currency;
                viewModel.serviceID = SID;
                viewModel.clientMSISDN = clientMSISDN;
                viewModel.clientName = clientName;
                viewModel.clientIDNumber = clientIDNumber;
                viewModel.clientEmail = clientEmail;
                viewModel.callBackURLOnSuccess = callback;
                viewModel.notificationURL = notification;
                viewModel.amountExpected = amounttopay;
                return View(viewModel);
            }
            catch (Exception ex)
            {
                Error error = new Error();
                error.Message = ex.Message.Replace("'", "");
                return View("~/Views/Shared/Partial Views/ErroMessangeView.cshtml", error);
            }
        }

        private string GetEncoding(string datastring, string studno, string key)
        {
            string result = "";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.chuka.ac.ke:8444/ecitizen/hashmac");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"datastring\":\"" + datastring + "\"," +
                               "\"key\":\"" + key + "\"," +
                              "\"StudNo\":\"" + studno + "\"}";

                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        public string getServiceDesc(string ServiceID)
        {
            string billdesc = "";
            try
            {
                string page = "StudentServices?$filter=Service_Code eq '" + ServiceID + "'&$format=json";

                HttpWebResponse httpResponseResC = Credentials.GetOdataData(page);
                using (var streamReader = new StreamReader(httpResponseResC.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    var details = JObject.Parse(result);


                    foreach (JObject config in details["value"])
                    {
                        billdesc = (string)config["Service_Description"];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return billdesc;
        }
    }
}