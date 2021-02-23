using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using iCore_Administrator;
using System.IO;
using System.Web.UI;
using System.Globalization;

namespace iCore_Administrator.Modules
{
    public class PublicFunctions
    {
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public bool IsNumberic(string InTxt)
        {
            try
            {
                foreach (char c in InTxt)
                {
                    if (!char.IsDigit(c)) { return false; }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool IsEnglish(string InTXT)
        {
            try
            {
                return Regex.IsMatch(InTXT, "^[a-zA-Z0-9.!@#$%^&*)(_+]+$");
            }
            catch
            {
                return false;
            }
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string Make_Security_Code(int Count)
        {
            string FB = "";
            try
            {
                const string valid = "ABCDEFG9HI8JK7LM6NO5PQ4RS3TU2VW1XY0Z";
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < Count--)
                { res.Append(valid[rnd.Next(valid.Length)]); }
                FB = res.ToString();
            }
            catch
            { FB = ""; }
            return FB;
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string Make_Security_CodeFake(int Count)
        {
            string FB = "";
            try
            {
                const string valid = "3Sd4e5fV6g7h8i9j0kAlBYmCnDoQTpFqGrXHsItJuKvLwMxNyWOzPZRUa1b2c";
                StringBuilder res = new StringBuilder();
                Random rnd = new Random();
                while (0 < Count--)
                { res.Append(valid[rnd.Next(valid.Length)]); }
                FB = res.ToString();
            }
            catch
            { FB = ""; }
            return FB;
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string Get_Date()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                string Txt = DateTime.Now.ToString("dd/MM/yyyy");
                return Txt.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        public string Get_Date_Formated(string TxtFormated)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                string Txt = DateTime.Now.ToString(TxtFormated);
                return Txt.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        public string Get_Date_Formated_Change(string TxtFormated, string TxtDay, bool AddDay)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                DateTime NDT = DateTime.Now;
                DateTime LDT;
                if (AddDay == true)
                {
                    LDT = NDT.AddDays(double.Parse(TxtDay));
                }
                else
                {
                    LDT = NDT.AddDays(-1 * (double.Parse(TxtDay)));
                }
                return LDT.ToString(TxtFormated);
            }
            catch
            {
                return "";
            }
        }

        public string Get_ExpireDate_Formated_Change(string InputDate, string TxtFormated, string TxtDay, bool AddDay)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                DateTime InDate = DateTime.Parse(InputDate);
                DateTime LDT;
                if (AddDay == true)
                {
                    LDT = InDate.AddDays(double.Parse(TxtDay));
                }
                else
                {
                    LDT = InDate.AddDays(-1 * (double.Parse(TxtDay)));
                }
                return LDT.ToString(TxtFormated);
            }
            catch
            {
                return "";
            }
        }

        public bool ExpiredDateCheck(string BaseDate, string MaxDate)
        {
            try
            {
                bool Res = false;
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                DateTime BDate = DateTime.Parse(BaseDate);
                DateTime MDate = DateTime.Parse(MaxDate);
                if (BDate.Date >= MDate.Date)
                {
                    Res = true;
                }
                return Res;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public string ConvertDate_Format(string InputDate,string Input_Format,string Output_Foramt)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                DateTime InDate = DateTime.ParseExact(InputDate, Input_Format, new CultureInfo("en-AU"));
                return InDate.ToString(Output_Foramt);
            }
            catch (Exception)
            {
                return InputDate;
            }
        }

        public string Get_Time()
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                string Txt = DateTime.Now.ToString("HH:mm");
                return Txt.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }

        public string Get_Time_Formated(string TimeFormat)
        {
            try
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                string Txt = DateTime.Now.ToString(TimeFormat);
                return Txt.ToString().Trim();
            }
            catch
            {
                return "";
            }
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string RenderViewToString(ControllerContext context, string viewName, object model)
        {
            try
            {
                var controller = context.Controller;
                var partialView = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var stringBuilder = new StringBuilder();
                using (var stringWriter = new StringWriter(stringBuilder))
                {
                    using (var htmlWriter = new HtmlTextWriter(stringWriter))
                    {
                        controller.ViewData.Model = model;
                        partialView.View.Render(new ViewContext(controller.ControllerContext, partialView.View, controller.ViewData, new TempDataDictionary(), htmlWriter), htmlWriter);
                    }
                }
                return stringBuilder.ToString();

            }
            catch
            {
                return "";
            }
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string RenderViewToStringFile(string viewPath)
        {
            string FilePath = HttpContext.Current.Server.MapPath(viewPath);
            var file = new FileInfo(FilePath);
            using (StreamReader streamReader = file.OpenText())
            {
                string viewLine = "";
                var result = string.Empty;
                while ((viewLine = streamReader.ReadLine()) != null)
                {
                    result = result + viewLine;
                }
                return result;
            }
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string GetFile_Type(string FileName)
        {
            try
            {
                FileName = FileName.Trim();
                string LFT = "";
                int DotNum = 0;
                for (int i = (FileName.Length - 1); i >= 0; i--) { if (FileName[i] == '.') { DotNum = i; break; } }
                if (DotNum > 0) { LFT = FileName.Substring(DotNum + 1, FileName.Length - (DotNum + 1)); }
                return LFT.Trim();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string GetFile_Name(string FileAddress)
        {
            try
            {
                FileAddress = FileAddress.Trim();
                string FTP = GetFile_Type(FileAddress);
                FileAddress = FileAddress.Substring(0, FileAddress.Length - (FTP.Length + 1));
                return FileAddress.Trim();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public string GetFileSize_String(long bytes)
        {
            string Res = "0 KB";
            double SF = bytes / 1024f;
            Res = Math.Round(SF, 3).ToString() + " KB";
            if (SF > 1024)
            {
                SF = SF / 1024f;
                Res = Math.Round(SF, 3).ToString() + " MB";
            }
            return Res.Trim();
        }

        public double GetFileSize_KB(long bytes)
        {
            return Math.Round(bytes / 1024f, 3);
        }

        public double GetFileSize_MB(long bytes)
        {
            double SF = bytes / 1024f;
            SF = SF / 1024f;
            return Math.Round(SF, 3);
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string SeprateDigit(string Input)
        {
            try
            {
                Input = UnSeprateDigit(Input);
                int num1 = Convert.ToInt32(Input.Trim());
                return String.Format("{0:#,###0}", num1);
            }
            catch (Exception)
            { return ""; }
        }

        public string UnSeprateDigit(string Input)
        {
            try
            { return Input.Replace(",", ""); }
            catch (Exception)
            { return ""; }
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string GetUser_IP()
        {
            try
            {
                string InternetIP = "";
                InternetIP = new WebClient().DownloadString("http://icanhazip.com");
                return InternetIP;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetUser_Browser()
        {
            try
            {
                string BRN = (HttpContext.Current.Request.Browser.Browser + " " + HttpContext.Current.Request.Browser.Version).ToString().Trim();
                return BRN;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetUserIP_HttpRequest(HttpRequest httpReq)
        {
            string ip = httpReq.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(ip))
            {
                ip = httpReq.ServerVariables["REMOTE_ADDR"];
            }
            return ip;
        }
        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
        public string Text_UpperCase_AfterSpase(string InText)
        {
            string ResTxt = "";
            InText = InText.Replace("  ", " ").Trim();
            try
            {
                string[] SepTxt = InText.Split(' ');
                foreach (string Word in SepTxt)
                {
                    if (Word != " ")
                    {
                        if (Word.Length > 1)
                        {
                            ResTxt += " " + Word[0].ToString().ToUpper() + Word.Substring(1, Word.Length - 1).ToLower();
                        }
                        else
                        {
                            ResTxt += " " + Word.ToString().ToUpper();
                        }
                    }
                }
            }
            catch (Exception) { }
            return ResTxt.Trim();
        }

        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================


        //==========================================================================================================================================================================================
        //==========================================================================================================================================================================================
    }
}