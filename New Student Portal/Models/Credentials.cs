using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net;
using New_Student_Portal.NAVWS;
using System.IO;

namespace New_Student_Portal.Models
{
    public class Credentials
    {
        public static string fileSourcePath = ConfigurationManager.AppSettings["FILEPATH"];
        public static string fileDownLoads = ConfigurationManager.AppSettings["DOWNLOADLINKS"];
        public static HttpWebResponse GetOdataData(string page)
        {
            HttpWebResponse httpResponse = null;

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(ConfigurationManager.AppSettings["ODATA_URI"] + page);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";
            httpWebRequest.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["W_USER"],
                        ConfigurationManager.AppSettings["W_PWD"], ConfigurationManager.AppSettings["DOMAIN"]);

            httpWebRequest.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

            return httpResponse;
        }
        public static webportal ObjNav
        {
            get
            {
                var ws = new webportal();

                try
                {
                    var credentials = new NetworkCredential(ConfigurationManager.AppSettings["W_USER"],
                        ConfigurationManager.AppSettings["W_PWD"], ConfigurationManager.AppSettings["DOMAIN"]);

                    ws.Credentials = credentials;
                    ws.PreAuthenticate = true;

                }
                catch (Exception ex)
                {
                    ex.Data.Clear();
                }
                return ws;
            }
        }
        public static string GetDocumentAttachmet(int TblID, string DocNo, int Id)
        {
            string PicString = "";
            try
            {
                PicString = ObjNav.GetDocumentAttachment(TblID, DocNo, Id);
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
            }
            return PicString;
        }
        public static void DownloadAttachment(string path, Byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }
        public static string UploadDocumentAttachment(string DocNo, string base64String, string filePath, int TableID)
        {
            string Uploaded = "";
            try
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(base64String));

                ObjNav.UploadAttachedDocument(DocNo, filePath, base64String, TableID);
                Uploaded = "SUCCESS";
            }
            catch (Exception ex)
            {
                Uploaded = ex.Message;
            }
            return Uploaded;
        }
    }
}