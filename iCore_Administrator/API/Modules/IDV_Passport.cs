using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Accord.Vision.Detection;
using Accord.Vision.Detection.Cascades;
using System.Drawing;
using System.Drawing.Imaging;
using RestSharp;
using System.Data;
using Spire.Barcode;
using QRCodeDecoderLibrary;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Accord.Imaging.Filters;
using Accord;
using System.Drawing.Drawing2D;
using Accord.Statistics.Models.Markov.Topology;
using IronPython;
using IronPython.Hosting;
using System.Diagnostics;
using System.Globalization;

namespace iCore_Administrator.API.Modules
{
    public class IDV_Passport
    {
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        iCore_Administrator.Modules.SQL_Tranceiver SQ = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions PB = new iCore_Administrator.Modules.PublicFunctions();
        SimilarityFunction SF = new SimilarityFunction();
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        private string RequestOCR_DetectOrientation(Bitmap IMG, string FileType)
        {
            try
            {
                FileType = FileType.Trim();
                if (FileType == "") { FileType = "JPG"; }
                if (IMG == null) { return ""; }
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Setting_Basic_05_IDVOCR");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        var client = new RestClient(DT.Rows[0][0].ToString().Trim());
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("apikey", DT.Rows[0][1].ToString().Trim());
                        ImageConverter converter = new ImageConverter();
                        byte[] PIMG = (byte[])converter.ConvertTo(IMG, typeof(byte[]));
                        request.AddFile("file", PIMG, "IDV.JPG");
                        request.AddParameter("filetype", FileType);
                        request.AddParameter("isOverlayRequired", "False");
                        request.AddParameter("detectOrientation", "True");
                        request.AddParameter("scale", "False");
                        request.AddParameter("isTable", "False");
                        request.AddParameter("OCREngine", DT.Rows[0][6].ToString().Trim());
                        IRestResponse response = client.Execute(request);
                        return response.Content.Trim();
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        private string RequestOCR_Image(Bitmap IMG, string FileType, string isOverlayRequired, string scale)
        {
            try
            {
                FileType = FileType.Trim();
                isOverlayRequired = isOverlayRequired.Trim();
                scale = scale.Trim();
                if (FileType == "") { FileType = "JPG"; }
                if (isOverlayRequired == "1") { isOverlayRequired = "True"; }
                if (isOverlayRequired == "0") { isOverlayRequired = "False"; }
                if (isOverlayRequired == "") { isOverlayRequired = "False"; } else { if (isOverlayRequired.ToLower() == "true") { isOverlayRequired = "True"; } else { if (isOverlayRequired.ToLower() == "false") { isOverlayRequired = "False"; } else { isOverlayRequired = "False"; } } }
                if (scale == "1") { scale = "True"; }
                if (scale == "0") { scale = "False"; }
                if (scale == "") { scale = "False"; } else { if (scale.ToLower() == "true") { scale = "True"; } else { if (scale.ToLower() == "false") { scale = "False"; } else { scale = "False"; } } }
                if (IMG == null) { return ""; }
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Setting_Basic_05_IDVOCR");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        var client = new RestClient(DT.Rows[0][0].ToString().Trim());
                        client.Timeout = -1;
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("apikey", DT.Rows[0][1].ToString().Trim());
                        ImageConverter converter = new ImageConverter();
                        byte[] PIMG = (byte[])converter.ConvertTo(IMG, typeof(byte[]));
                        request.AddFile("file", PIMG, "IDV.JPG");
                        request.AddParameter("filetype", FileType);
                        request.AddParameter("isOverlayRequired", isOverlayRequired);
                        request.AddParameter("detectOrientation", "False");
                        request.AddParameter("scale", scale);
                        request.AddParameter("isTable", "False");
                        request.AddParameter("OCREngine", DT.Rows[0][6].ToString().Trim());
                        IRestResponse response = client.Execute(request);
                        return response.Content.Trim();
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        public Bitmap ImageRotation(Bitmap IMG, string Degree)
        {
            try
            {
                Image IMGL = null;
                IMGL = new Bitmap(IMG);
                switch (Degree.Trim())
                {
                    case "0":
                        {
                            try { IMGL.RotateFlip(RotateFlipType.RotateNoneFlipNone); } catch (Exception) { }
                            break;
                        }
                    case "90":
                        {
                            try { IMGL.RotateFlip(RotateFlipType.Rotate90FlipNone); } catch (Exception) { }
                            break;
                        }
                    case "180":
                        {

                            try { IMGL.RotateFlip(RotateFlipType.Rotate180FlipNone); } catch (Exception) { }
                            break;
                        }
                    case "270":
                        {
                            try { IMGL.RotateFlip(RotateFlipType.Rotate270FlipNone); } catch (Exception) { }
                            break;
                        }
                    case "360":
                        {
                            try { IMGL.RotateFlip(RotateFlipType.RotateNoneFlipNone); } catch (Exception) { }
                            break;
                        }
                }

                return new Bitmap(IMGL);
            }
            catch (Exception)
            {
                return null;
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        public void GetData(string TransactionID)
        {
            try
            {
                TransactionID = TransactionID.Trim();
                DataTable DT_Tran = new DataTable();
                DT_Tran = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (ID = '" + TransactionID + "') And (Removed = '0')");
                if (DT_Tran.Rows != null)
                {
                    if (DT_Tran.Rows.Count == 1)
                    {
                        DataTable DT_Tran_Argument = new DataTable();
                        DT_Tran_Argument = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction_RequestArgument Where (Transaction_ID = '" + TransactionID + "')");
                        if (DT_Tran.Rows != null)
                        {
                            if (DT_Tran.Rows.Count == 1)
                            {
                                // Request arguments defines :
                                string Req_Date_Format = "dd/MM/yyyy";
                                string Req_Processing_Type = "1";
                                string Req_Document_Validation = "0";
                                string Req_Overlay_Required = "0";
                                string Req_Detect_Orientation = "0";
                                string Req_Image_Scale = "0";
                                string Req_Cropping_Mode = "0";
                                try { Req_Date_Format = DT_Tran_Argument.Rows[0][3].ToString().Trim(); } catch (Exception) { Req_Date_Format = "dd/MM/yyyy"; }
                                try { Req_Processing_Type = DT_Tran_Argument.Rows[0][4].ToString().Trim(); } catch (Exception) { Req_Processing_Type = "1"; }
                                try { Req_Document_Validation = DT_Tran_Argument.Rows[0][5].ToString().Trim(); } catch (Exception) { Req_Document_Validation = "0"; }
                                try { Req_Overlay_Required = DT_Tran_Argument.Rows[0][6].ToString().Trim(); } catch (Exception) { Req_Overlay_Required = "0"; }
                                try { Req_Detect_Orientation = DT_Tran_Argument.Rows[0][7].ToString().Trim(); } catch (Exception) { Req_Detect_Orientation = "0"; }
                                try { Req_Image_Scale = DT_Tran_Argument.Rows[0][8].ToString().Trim(); } catch (Exception) { Req_Image_Scale = "0"; }
                                try { Req_Cropping_Mode = DT_Tran_Argument.Rows[0][9].ToString().Trim(); } catch (Exception) { Req_Cropping_Mode = "0"; }
                                if (Req_Date_Format == "") { Req_Date_Format = "dd/MM/yyyy"; }
                                if (Req_Processing_Type == "") { Req_Processing_Type = "1"; }
                                if ((Req_Processing_Type != "1") && (Req_Processing_Type != "2") && (Req_Processing_Type != "3") && (Req_Processing_Type != "4") && (Req_Processing_Type != "5") && (Req_Processing_Type != "6")) { Req_Processing_Type = "1"; }
                                if (Req_Document_Validation == "") { Req_Document_Validation = "0"; }
                                if (Req_Document_Validation.ToLower() == "true") { Req_Document_Validation = "1"; }
                                if (Req_Document_Validation.ToLower() == "false") { Req_Document_Validation = "0"; }
                                if (Req_Overlay_Required == "") { Req_Overlay_Required = "0"; }
                                if (Req_Overlay_Required.ToLower() == "true") { Req_Overlay_Required = "1"; }
                                if (Req_Overlay_Required.ToLower() == "false") { Req_Overlay_Required = "0"; }
                                if (Req_Detect_Orientation == "") { Req_Detect_Orientation = "0"; }
                                if (Req_Detect_Orientation.ToLower() == "true") { Req_Detect_Orientation = "1"; }
                                if (Req_Detect_Orientation.ToLower() == "false") { Req_Detect_Orientation = "0"; }
                                if (Req_Image_Scale == "") { Req_Image_Scale = "0"; }
                                if (Req_Image_Scale.ToLower() == "true") { Req_Image_Scale = "1"; }
                                if (Req_Image_Scale.ToLower() == "false") { Req_Image_Scale = "0"; }
                                if (Req_Cropping_Mode == "") { Req_Cropping_Mode = "0"; }
                                if (Req_Cropping_Mode.ToLower() == "true") { Req_Cropping_Mode = "1"; }
                                if (Req_Cropping_Mode.ToLower() == "false") { Req_Cropping_Mode = "0"; }
                                // Create result folder and clear :
                                string BaseFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID);
                                if (Directory.Exists(BaseFolder + "\\" + "Result") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Result"); }
                                string FilePath_Upload = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID + "/Upload");
                                string FilePath_Last = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID + "/Result/" + DT_Tran.Rows[0][52].ToString().Trim() + "." + DT_Tran.Rows[0][50].ToString().Trim());
                                string Image_Name_PassportUpload = DT_Tran.Rows[0][52].ToString().Trim() + "." + DT_Tran.Rows[0][50].ToString().Trim();
                                string Image_Path_PassportUpload = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID + "/Upload/" + Image_Name_PassportUpload);
                                string Image_Name_Passport_Last = "";
                                string Image_Path_Passport_Last = "";
                                string Image_Name_MRZ_Last = "";
                                string Image_Path_MRZ_Last = "";
                                // Detect Orientation :
                                if (Req_Detect_Orientation == "1")
                                {
                                    Image IMG_Con_Pass = new Bitmap(Image_Path_PassportUpload);
                                    string IMG_Con_Pass_Format = DT_Tran.Rows[0][50].ToString().Trim();
                                    string OCR_Orientation = RequestOCR_DetectOrientation((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format);
                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                    if (OCR_Orientation_Res.ParsedResults[0].TextOrientation.ToString().Trim() != "0")
                                    {
                                        Image Img_Rotated = null;
                                        Img_Rotated = ImageRotation((Bitmap)IMG_Con_Pass, OCR_Orientation_Res.ParsedResults[0].TextOrientation.ToString().Trim());
                                        Image_Name_Passport_Last = Image_Name_PassportUpload;
                                        Image_Path_Passport_Last = FilePath_Last;
                                        Img_Rotated.Save(Image_Path_Passport_Last);
                                    }
                                    else
                                    {
                                        System.IO.File.Copy(Image_Path_PassportUpload, FilePath_Last, true);
                                        Image_Name_Passport_Last = Image_Name_PassportUpload;
                                        Image_Path_Passport_Last = FilePath_Last;
                                    }
                                }
                                else
                                {
                                    System.IO.File.Copy(Image_Path_PassportUpload, FilePath_Last, true);
                                    Image_Name_Passport_Last = Image_Name_PassportUpload;
                                    Image_Path_Passport_Last = FilePath_Last;
                                }
                                // Cropping Mode :
                                if (Req_Cropping_Mode == "1")
                                {
                                    try
                                    {
                                        string Image2_Name = TransactionID + "I2" + PB.Make_Security_Code(20);
                                        var MrzPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID + "/Result/" + Image2_Name + ".jpg");
                                        try
                                        {
                                            string BaseFolderAddress = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID);
                                            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
                                            start.FileName = System.Configuration.ConfigurationManager.AppSettings["PythonExe_FileAddress"].ToString().Trim();
                                            start.Arguments = "\"" + System.Web.Hosting.HostingEnvironment.MapPath("~/Python/AM_MRZCropping.py") + "\"" + " \"" + BaseFolderAddress + "\" " + Image_Name_Passport_Last + " " + (Image2_Name + ".jpg").ToString();
                                            start.UseShellExecute = false;// Do not use OS shell
                                            start.CreateNoWindow = true; // We don't need new window
                                            start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
                                            start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
                                            start.LoadUserProfile = true;
                                            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
                                            {
                                                using (StreamReader reader = process.StandardOutput)
                                                {
                                                    string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                                                    string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                                                }
                                            }
                                        }
                                        catch (Exception) { }
                                        if (System.IO.File.Exists(MrzPath) == false) { System.IO.File.Copy(Image_Path_Passport_Last, MrzPath, true); }
                                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_2_Title] = 'MRZ image',[Image_2_File_Name] = '" + Image2_Name + "',[Image_2_File_Format] = 'jpg',[Image_2_File_Type] = 'image/jpeg',[Image_2_Download_ID] = '" + Image2_Name + "',[Image_2_Download_Count] = '0' Where (ID = '" + TransactionID + "')");
                                        Image_Name_MRZ_Last = Image2_Name + ".jpg";
                                        Image_Path_MRZ_Last = MrzPath;
                                    }
                                    catch (Exception) { }
                                }
                                else
                                {
                                    try
                                    {
                                        string Image2_Name = TransactionID + "I2" + PB.Make_Security_Code(20);
                                        var MrzPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + TransactionID + "/Result/" + Image2_Name + ".jpg");
                                        System.IO.File.Copy(Image_Path_Passport_Last, MrzPath, true);
                                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_2_Title] = 'MRZ image',[Image_2_File_Name] = '" + Image2_Name + "',[Image_2_File_Format] = 'jpg',[Image_2_File_Type] = 'image/jpeg',[Image_2_Download_ID] = '" + Image2_Name + "',[Image_2_Download_Count] = '0' Where (ID = '" + TransactionID + "')");
                                        Image_Name_MRZ_Last = Image2_Name + ".jpg";
                                        Image_Path_MRZ_Last = MrzPath;
                                    }
                                    catch (Exception)
                                    { }
                                }
                                //-------------------------------------------------------------------------------------------------
                                // Main Proccessing :
                                //-------------------------------------------------------------------------------------------------
                                // Clear Database :
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_19_API_Passport_MRZ Where (Transaction_ID = '" + TransactionID + "')");
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_20_API_Passport_MRZ_Decoded Where (Transaction_ID = '" + TransactionID + "')");
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_21_API_Passport_OCR Where (Transaction_ID = '" + TransactionID + "')");
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_22_API_Passport_OCR_Decoded Where (Transaction_ID = '" + TransactionID + "')");
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_23_API_Passport_Validation Where (Transaction_ID = '" + TransactionID + "')");
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_24_API_Passport_Validation_Result Where (Transaction_ID = '" + TransactionID + "')");
                                //-------------------------------------------------------------------------------------------------
                                // Document Processing :
                                switch (Req_Processing_Type)
                                {
                                    case "1": // MRZ OCR :
                                        {
                                            try
                                            {
                                                string MRZ_Line1 = ""; string MRZ_Line2 = ""; string MRZ_Message = ""; string IMG_LST_Path = "";
                                                if (File.Exists(Image_Path_MRZ_Last) == true) { IMG_LST_Path = Image_Path_MRZ_Last; } else { if (File.Exists(Image_Path_Passport_Last) == true) { IMG_LST_Path = Image_Path_Passport_Last; } else { IMG_LST_Path = ""; } }
                                                if (IMG_LST_Path.Trim() != "")
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(IMG_LST_Path);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(IMG_LST_Path);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    string LastOCR_Result = "";
                                                    try { LastOCR_Result = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim(); } catch (Exception) { }
                                                    LastOCR_Result = LastOCR_Result.Trim();
                                                    if (LastOCR_Result != "")
                                                    {
                                                        LastOCR_Result = LastOCR_Result.Replace(" ", "").Replace("\n", "$").Replace("\r", "$");
                                                        LastOCR_Result = LastOCR_Result.Replace("$$", "$");
                                                        LastOCR_Result = LastOCR_Result.Trim().ToUpper();
                                                        int StartCap = LastOCR_Result.IndexOf("P<");
                                                        if (StartCap >= 0)
                                                        {
                                                            try
                                                            {
                                                                string MRZ_Detect = LastOCR_Result.Substring(StartCap, LastOCR_Result.Length - StartCap);
                                                                string[] MRZ_DetectSPL = MRZ_Detect.Split('$');
                                                                MRZ_Line1 = MRZ_DetectSPL[0].Trim();
                                                                MRZ_Line2 = MRZ_DetectSPL[1].Trim();
                                                            }
                                                            catch (Exception)
                                                            {
                                                                MRZ_Line1 = "";
                                                                MRZ_Line2 = "";
                                                                MRZ_Message = "Passport MRZ code not identified";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MRZ_Line1 = "";
                                                            MRZ_Line2 = "";
                                                            MRZ_Message = "Passport MRZ code not identified";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MRZ_Line1 = "";
                                                        MRZ_Line2 = "";
                                                        MRZ_Message = "Passport MRZ code not identified";
                                                    }
                                                }
                                                else
                                                {
                                                    MRZ_Line1 = "";
                                                    MRZ_Line2 = "";
                                                    MRZ_Message = "Passport image file not detected";
                                                }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_19_API_Passport_MRZ Values ('" + TransactionID + "','" + MRZ_Line1 + "','" + MRZ_Line2 + "','" + MRZ_Message + "')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                    case "2": // MRZ OCR And Classification :
                                        {
                                            try
                                            {
                                                string MRZ_Line1 = ""; string MRZ_Line2 = ""; string MRZ_Message = ""; string IMG_LST_Path = "";
                                                if (File.Exists(Image_Path_MRZ_Last) == true) { IMG_LST_Path = Image_Path_MRZ_Last; } else { if (File.Exists(Image_Path_Passport_Last) == true) { IMG_LST_Path = Image_Path_Passport_Last; } else { IMG_LST_Path = ""; } }
                                                if (IMG_LST_Path.Trim() != "")
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(IMG_LST_Path);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(IMG_LST_Path);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    string LastOCR_Result = "";
                                                    try { LastOCR_Result = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim(); } catch (Exception) { }
                                                    LastOCR_Result = LastOCR_Result.Trim();
                                                    if (LastOCR_Result != "")
                                                    {
                                                        LastOCR_Result = LastOCR_Result.Replace(" ", "").Replace("\n", "$").Replace("\r", "$");
                                                        LastOCR_Result = LastOCR_Result.Replace("$$", "$");
                                                        LastOCR_Result = LastOCR_Result.Trim().ToUpper();
                                                        int StartCap = LastOCR_Result.IndexOf("P<");
                                                        if (StartCap >= 0)
                                                        {
                                                            try
                                                            {
                                                                string MRZ_Detect = LastOCR_Result.Substring(StartCap, LastOCR_Result.Length - StartCap);
                                                                string[] MRZ_DetectSPL = MRZ_Detect.Split('$');
                                                                MRZ_Line1 = MRZ_DetectSPL[0].Trim();
                                                                MRZ_Line2 = MRZ_DetectSPL[1].Trim();
                                                            }
                                                            catch (Exception)
                                                            {
                                                                MRZ_Line1 = "";
                                                                MRZ_Line2 = "";
                                                                MRZ_Message = "Passport MRZ code not identified";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MRZ_Line1 = "";
                                                            MRZ_Line2 = "";
                                                            MRZ_Message = "Passport MRZ code not identified";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MRZ_Line1 = "";
                                                        MRZ_Line2 = "";
                                                        MRZ_Message = "Passport MRZ code not identified";
                                                    }
                                                }
                                                else
                                                {
                                                    MRZ_Line1 = "";
                                                    MRZ_Line2 = "";
                                                    MRZ_Message = "Passport image file not detected";
                                                }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_19_API_Passport_MRZ Values ('" + TransactionID + "','" + MRZ_Line1 + "','" + MRZ_Line2 + "','" + MRZ_Message + "')");
                                                MRZ_Line1 = MRZ_Line1.Trim();
                                                MRZ_Line2 = MRZ_Line2.Trim();
                                                string DC_Document_Type = "";
                                                string DC_Document_IssuingState = "";
                                                string DC_Document_Lastname = "";
                                                string DC_Document_Firstname = "";
                                                string DC_Document_Middlename = "";
                                                string DC_Document_Line1_Message = "";
                                                // Decode Line 1 :
                                                if (MRZ_Line1 != "")
                                                {
                                                    if (MRZ_Line1.Length == 44)
                                                    {
                                                        try { DC_Document_Type = MRZ_Line1.Substring(0, 1); } catch (Exception) { }
                                                        try { DC_Document_IssuingState = MRZ_Line1.Substring(2, 3); } catch (Exception) { }
                                                        try
                                                        {
                                                            string FullnameDetector = MRZ_Line1.Substring(5, MRZ_Line1.Length - 5);
                                                            FullnameDetector = FullnameDetector.Replace("<", " ");
                                                            FullnameDetector = FullnameDetector.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                                                            FullnameDetector = FullnameDetector.Trim();
                                                            string[] FullnameDetector_Sub = FullnameDetector.Split(' ');
                                                            try { DC_Document_Lastname = FullnameDetector_Sub[0].Trim(); } catch (Exception) { }
                                                            try { DC_Document_Firstname = FullnameDetector_Sub[1].Trim(); } catch (Exception) { }
                                                            try { for (int i = 2; i <= FullnameDetector_Sub.Count(); i++) { DC_Document_Middlename += FullnameDetector_Sub[i] + " "; } DC_Document_Middlename = DC_Document_Middlename.Trim(); } catch (Exception) { }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                    else
                                                    {
                                                        DC_Document_Line1_Message = "The structure of MRZ (Line1) is incorrect";
                                                    }
                                                }
                                                else
                                                {
                                                    DC_Document_Line1_Message = "The structure of MRZ (Line1) is incorrect";
                                                }
                                                string DC_Document_PassportNumber = "";
                                                string DC_Document_Number_Checksum = "";
                                                string DC_Document_NationalityCode = "";
                                                string DC_Document_BirthDate = "";
                                                string DC_Document_BirthDate_Checksum = "";
                                                string DC_Document_Gender = "";
                                                string DC_Document_ExpiryDate = "";
                                                string DC_Document_ExpiryDate_Checksum = "";
                                                string DC_Document_Personal_Number = "";
                                                string DC_Document_Personal_Number_Checksum = "";
                                                string DC_Document_Checksum = "";
                                                string DC_Document_Line2_Message = "";
                                                // Decode Line 1 :
                                                if (MRZ_Line2 != "")
                                                {
                                                    if (MRZ_Line2.Length == 44)
                                                    {
                                                        try { DC_Document_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 1, 1); } catch (Exception) { }
                                                        try { DC_Document_Personal_Number_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 2, 1); } catch (Exception) { }
                                                        try { DC_Document_Personal_Number = MRZ_Line2.Substring(MRZ_Line2.Length - 16, 14); } catch (Exception) { }
                                                        try { DC_Document_ExpiryDate_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 17, 1); } catch (Exception) { }
                                                        try { DC_Document_ExpiryDate = MRZ_Line2.Substring(MRZ_Line2.Length - 23, 6); } catch (Exception) { }
                                                        try { DC_Document_Gender = MRZ_Line2.Substring(MRZ_Line2.Length - 24, 1); } catch (Exception) { }
                                                        try { DC_Document_BirthDate_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 25, 1); } catch (Exception) { }
                                                        try { DC_Document_BirthDate = MRZ_Line2.Substring(MRZ_Line2.Length - 31, 6); } catch (Exception) { }
                                                        try { DC_Document_NationalityCode = MRZ_Line2.Substring(MRZ_Line2.Length - 34, 3); } catch (Exception) { }
                                                        try { DC_Document_Number_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 35, 1); } catch (Exception) { }
                                                        try { DC_Document_PassportNumber = MRZ_Line2.Substring(MRZ_Line2.Length - 44, 9); } catch (Exception) { }
                                                    }
                                                    else
                                                    {
                                                        DC_Document_Line2_Message = "The structure of MRZ (Line2) is incorrect";
                                                    }
                                                }
                                                else
                                                {
                                                    DC_Document_Line2_Message = "The structure of MRZ (Line2) is incorrect";
                                                }
                                                DC_Document_Type = DC_Document_Type.Replace("<", "").Trim();
                                                DC_Document_IssuingState = DC_Document_IssuingState.Replace("<", "").Trim();
                                                DC_Document_Lastname = DC_Document_Lastname.Replace("<", "").Trim();
                                                DC_Document_Firstname = DC_Document_Firstname.Replace("<", "").Trim();
                                                DC_Document_Middlename = DC_Document_Middlename.Replace("<", "").Trim();
                                                DC_Document_Line1_Message = DC_Document_Line1_Message.Replace("<", "").Trim();
                                                DC_Document_PassportNumber = DC_Document_PassportNumber.Replace("<", "").Trim();
                                                DC_Document_Number_Checksum = DC_Document_Number_Checksum.Replace("<", "").Trim();
                                                DC_Document_NationalityCode = DC_Document_NationalityCode.Replace("<", "").Trim();
                                                DC_Document_BirthDate = DC_Document_BirthDate.Replace("<", "").Trim();
                                                DC_Document_BirthDate_Checksum = DC_Document_BirthDate_Checksum.Replace("<", "").Trim();
                                                DC_Document_Gender = DC_Document_Gender.Replace("<", "").Trim();
                                                DC_Document_ExpiryDate = DC_Document_ExpiryDate.Replace("<", "").Trim();
                                                DC_Document_ExpiryDate_Checksum = DC_Document_ExpiryDate_Checksum.Replace("<", "").Trim();
                                                DC_Document_Personal_Number = DC_Document_Personal_Number.Replace("<", "").Trim();
                                                DC_Document_Personal_Number_Checksum = DC_Document_Personal_Number_Checksum.Replace("<", "").Trim();
                                                DC_Document_Checksum = DC_Document_Checksum.Replace("<", "").Trim();
                                                DC_Document_Line2_Message = DC_Document_Line2_Message.Replace("<", "").Trim();
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','1','Document_Type_Code','" + DC_Document_Type + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','2','Issuing_State_Code','" + DC_Document_IssuingState + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','3','Last_Name','" + DC_Document_Lastname + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','4','First_Name','" + DC_Document_Firstname + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','5','Middle_Name','" + DC_Document_Middlename + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','6','Line1_Message','" + DC_Document_Line1_Message + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','7','Passport_Number','" + DC_Document_PassportNumber + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','8','Passport_Number_Checkdigit','" + DC_Document_Number_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','9','Nationality_Code','" + DC_Document_NationalityCode + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','10','BirthDate','" + DC_Document_BirthDate + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','11','BirthDate_Checkdigit','" + DC_Document_BirthDate_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','12','Gender','" + DC_Document_Gender + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','13','Expiry_Date','" + DC_Document_ExpiryDate + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','14','Expiry_Date_Checkdigit','" + DC_Document_ExpiryDate_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','15','Personal_Number','" + DC_Document_Personal_Number + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','16','Personal_Number_Checkdigit','" + DC_Document_Personal_Number_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','17','Document_Checkdigit','" + DC_Document_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','18','Line2_Message','" + DC_Document_Line2_Message + "')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                    case "3": // Passport OCR :
                                        {
                                            try
                                            {
                                                string PassportJson_Result_TextOverlay = "";
                                                string PassportJson_Result_ParsedText = "";
                                                string PassportJson_Message = "";
                                                if (File.Exists(Image_Path_Passport_Last) == true)
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(Image_Path_Passport_Last);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(Image_Path_Passport_Last);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    PassportJson_Result_TextOverlay = OCR_Orientation.Trim();
                                                    PassportJson_Result_ParsedText = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim();
                                                }
                                                else
                                                {
                                                    PassportJson_Message = "Passport image file not detected";
                                                }
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace(',', '#');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\'', '$');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\"', '%');

                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace(',', '#');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\'', '$');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\"', '%');
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_21_API_Passport_OCR Values ('" + TransactionID + "','" + PassportJson_Result_TextOverlay + "','" + PassportJson_Result_ParsedText + "','" + PassportJson_Message + "')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                    case "4": // Passport OCR And Classification :
                                        {
                                            try
                                            {
                                                string PassportJson_Result_TextOverlay = "";
                                                string PassportJson_Result_ParsedText = "";
                                                string PassportJson_Message = "";
                                                if (File.Exists(Image_Path_Passport_Last) == true)
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(Image_Path_Passport_Last);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(Image_Path_Passport_Last);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    PassportJson_Result_TextOverlay = OCR_Orientation.Trim();
                                                    PassportJson_Result_ParsedText = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim();
                                                }
                                                else
                                                {
                                                    PassportJson_Message = "Passport image file not detected";
                                                }
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace(',', '#');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\'', '$');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\"', '%');

                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace(',', '#');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\'', '$');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\"', '%');
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_21_API_Passport_OCR Values ('" + TransactionID + "','" + PassportJson_Result_TextOverlay + "','" + PassportJson_Result_ParsedText + "','" + PassportJson_Message + "')");
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_22_API_Passport_OCR_Decoded Values ('" + TransactionID + "','0','Message','We are updating this section, at the moment it is not possible to classification passport information, for more information please contact the support team')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                    case "5": // Passport And MRZ OCR :
                                        {
                                            try
                                            {
                                                string MRZ_Line1 = ""; string MRZ_Line2 = ""; string MRZ_Message = ""; string IMG_LST_Path = "";
                                                if (File.Exists(Image_Path_MRZ_Last) == true) { IMG_LST_Path = Image_Path_MRZ_Last; } else { if (File.Exists(Image_Path_Passport_Last) == true) { IMG_LST_Path = Image_Path_Passport_Last; } else { IMG_LST_Path = ""; } }
                                                if (IMG_LST_Path.Trim() != "")
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(IMG_LST_Path);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(IMG_LST_Path);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    string LastOCR_Result = "";
                                                    try { LastOCR_Result = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim(); } catch (Exception) { }
                                                    LastOCR_Result = LastOCR_Result.Trim();
                                                    if (LastOCR_Result != "")
                                                    {
                                                        LastOCR_Result = LastOCR_Result.Replace(" ", "").Replace("\n", "$").Replace("\r", "$");
                                                        LastOCR_Result = LastOCR_Result.Replace("$$", "$");
                                                        LastOCR_Result = LastOCR_Result.Trim().ToUpper();
                                                        int StartCap = LastOCR_Result.IndexOf("P<");
                                                        if (StartCap >= 0)
                                                        {
                                                            try
                                                            {
                                                                string MRZ_Detect = LastOCR_Result.Substring(StartCap, LastOCR_Result.Length - StartCap);
                                                                string[] MRZ_DetectSPL = MRZ_Detect.Split('$');
                                                                MRZ_Line1 = MRZ_DetectSPL[0].Trim();
                                                                MRZ_Line2 = MRZ_DetectSPL[1].Trim();
                                                            }
                                                            catch (Exception)
                                                            {
                                                                MRZ_Line1 = "";
                                                                MRZ_Line2 = "";
                                                                MRZ_Message = "Passport MRZ code not identified";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MRZ_Line1 = "";
                                                            MRZ_Line2 = "";
                                                            MRZ_Message = "Passport MRZ code not identified";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MRZ_Line1 = "";
                                                        MRZ_Line2 = "";
                                                        MRZ_Message = "Passport MRZ code not identified";
                                                    }
                                                }
                                                else
                                                {
                                                    MRZ_Line1 = "";
                                                    MRZ_Line2 = "";
                                                    MRZ_Message = "Passport image file not detected";
                                                }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_19_API_Passport_MRZ Values ('" + TransactionID + "','" + MRZ_Line1 + "','" + MRZ_Line2 + "','" + MRZ_Message + "')");
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                string PassportJson_Result_TextOverlay = "";
                                                string PassportJson_Result_ParsedText = "";
                                                string PassportJson_Message = "";
                                                if (File.Exists(Image_Path_Passport_Last) == true)
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(Image_Path_Passport_Last);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(Image_Path_Passport_Last);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    PassportJson_Result_TextOverlay = OCR_Orientation.Trim();
                                                    PassportJson_Result_ParsedText = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim();
                                                }
                                                else
                                                {
                                                    PassportJson_Message = "Passport image file not detected";
                                                }
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace(',', '#');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\'', '$');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\"', '%');

                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace(',', '#');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\'', '$');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\"', '%');
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_21_API_Passport_OCR Values ('" + TransactionID + "','" + PassportJson_Result_TextOverlay + "','" + PassportJson_Result_ParsedText + "','" + PassportJson_Message + "')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                    case "6": // Passport And MRZ OCR And Classification :
                                        {
                                            try
                                            {
                                                string MRZ_Line1 = ""; string MRZ_Line2 = ""; string MRZ_Message = ""; string IMG_LST_Path = "";
                                                if (File.Exists(Image_Path_MRZ_Last) == true) { IMG_LST_Path = Image_Path_MRZ_Last; } else { if (File.Exists(Image_Path_Passport_Last) == true) { IMG_LST_Path = Image_Path_Passport_Last; } else { IMG_LST_Path = ""; } }
                                                if (IMG_LST_Path.Trim() != "")
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(IMG_LST_Path);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(IMG_LST_Path);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    string LastOCR_Result = "";
                                                    try { LastOCR_Result = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim(); } catch (Exception) { }
                                                    LastOCR_Result = LastOCR_Result.Trim();
                                                    if (LastOCR_Result != "")
                                                    {
                                                        LastOCR_Result = LastOCR_Result.Replace(" ", "").Replace("\n", "$").Replace("\r", "$");
                                                        LastOCR_Result = LastOCR_Result.Replace("$$", "$");
                                                        LastOCR_Result = LastOCR_Result.Trim().ToUpper();
                                                        int StartCap = LastOCR_Result.IndexOf("P<");
                                                        if (StartCap >= 0)
                                                        {
                                                            try
                                                            {
                                                                string MRZ_Detect = LastOCR_Result.Substring(StartCap, LastOCR_Result.Length - StartCap);
                                                                string[] MRZ_DetectSPL = MRZ_Detect.Split('$');
                                                                MRZ_Line1 = MRZ_DetectSPL[0].Trim();
                                                                MRZ_Line2 = MRZ_DetectSPL[1].Trim();
                                                            }
                                                            catch (Exception)
                                                            {
                                                                MRZ_Line1 = "";
                                                                MRZ_Line2 = "";
                                                                MRZ_Message = "Passport MRZ code not identified";
                                                            }
                                                        }
                                                        else
                                                        {
                                                            MRZ_Line1 = "";
                                                            MRZ_Line2 = "";
                                                            MRZ_Message = "Passport MRZ code not identified";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        MRZ_Line1 = "";
                                                        MRZ_Line2 = "";
                                                        MRZ_Message = "Passport MRZ code not identified";
                                                    }
                                                }
                                                else
                                                {
                                                    MRZ_Line1 = "";
                                                    MRZ_Line2 = "";
                                                    MRZ_Message = "Passport image file not detected";
                                                }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_19_API_Passport_MRZ Values ('" + TransactionID + "','" + MRZ_Line1 + "','" + MRZ_Line2 + "','" + MRZ_Message + "')");
                                                MRZ_Line1 = MRZ_Line1.Trim();
                                                MRZ_Line2 = MRZ_Line2.Trim();
                                                string DC_Document_Type = "";
                                                string DC_Document_IssuingState = "";
                                                string DC_Document_Lastname = "";
                                                string DC_Document_Firstname = "";
                                                string DC_Document_Middlename = "";
                                                string DC_Document_Line1_Message = "";
                                                // Decode Line 1 :
                                                if (MRZ_Line1 != "")
                                                {
                                                    if (MRZ_Line1.Length == 44)
                                                    {
                                                        try { DC_Document_Type = MRZ_Line1.Substring(0, 1); } catch (Exception) { }
                                                        try { DC_Document_IssuingState = MRZ_Line1.Substring(2, 3); } catch (Exception) { }
                                                        try
                                                        {
                                                            string FullnameDetector = MRZ_Line1.Substring(5, MRZ_Line1.Length - 5);
                                                            FullnameDetector = FullnameDetector.Replace("<", " ");
                                                            FullnameDetector = FullnameDetector.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                                                            FullnameDetector = FullnameDetector.Trim();
                                                            string[] FullnameDetector_Sub = FullnameDetector.Split(' ');
                                                            try { DC_Document_Lastname = FullnameDetector_Sub[0].Trim(); } catch (Exception) { }
                                                            try { DC_Document_Firstname = FullnameDetector_Sub[1].Trim(); } catch (Exception) { }
                                                            try { for (int i = 2; i <= FullnameDetector_Sub.Count(); i++) { DC_Document_Middlename += FullnameDetector_Sub[i] + " "; } DC_Document_Middlename = DC_Document_Middlename.Trim(); } catch (Exception) { }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                    else
                                                    {
                                                        DC_Document_Line1_Message = "The structure of MRZ (Line1) is incorrect";
                                                    }
                                                }
                                                else
                                                {
                                                    DC_Document_Line1_Message = "The structure of MRZ (Line1) is incorrect";
                                                }
                                                string DC_Document_PassportNumber = "";
                                                string DC_Document_Number_Checksum = "";
                                                string DC_Document_NationalityCode = "";
                                                string DC_Document_BirthDate = "";
                                                string DC_Document_BirthDate_Checksum = "";
                                                string DC_Document_Gender = "";
                                                string DC_Document_ExpiryDate = "";
                                                string DC_Document_ExpiryDate_Checksum = "";
                                                string DC_Document_Personal_Number = "";
                                                string DC_Document_Personal_Number_Checksum = "";
                                                string DC_Document_Checksum = "";
                                                string DC_Document_Line2_Message = "";
                                                // Decode Line 1 :
                                                if (MRZ_Line2 != "")
                                                {
                                                    if (MRZ_Line2.Length == 44)
                                                    {
                                                        try { DC_Document_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 1, 1); } catch (Exception) { }
                                                        try { DC_Document_Personal_Number_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 2, 1); } catch (Exception) { }
                                                        try { DC_Document_Personal_Number = MRZ_Line2.Substring(MRZ_Line2.Length - 16, 14); } catch (Exception) { }
                                                        try { DC_Document_ExpiryDate_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 17, 1); } catch (Exception) { }
                                                        try { DC_Document_ExpiryDate = MRZ_Line2.Substring(MRZ_Line2.Length - 23, 6); } catch (Exception) { }
                                                        try { DC_Document_Gender = MRZ_Line2.Substring(MRZ_Line2.Length - 24, 1); } catch (Exception) { }
                                                        try { DC_Document_BirthDate_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 25, 1); } catch (Exception) { }
                                                        try { DC_Document_BirthDate = MRZ_Line2.Substring(MRZ_Line2.Length - 31, 6); } catch (Exception) { }
                                                        try { DC_Document_NationalityCode = MRZ_Line2.Substring(MRZ_Line2.Length - 34, 3); } catch (Exception) { }
                                                        try { DC_Document_Number_Checksum = MRZ_Line2.Substring(MRZ_Line2.Length - 35, 1); } catch (Exception) { }
                                                        try { DC_Document_PassportNumber = MRZ_Line2.Substring(MRZ_Line2.Length - 44, 9); } catch (Exception) { }
                                                    }
                                                    else
                                                    {
                                                        DC_Document_Line2_Message = "The structure of MRZ (Line2) is incorrect";
                                                    }
                                                }
                                                else
                                                {
                                                    DC_Document_Line2_Message = "The structure of MRZ (Line2) is incorrect";
                                                }
                                                DC_Document_Type = DC_Document_Type.Replace("<", "").Trim();
                                                DC_Document_IssuingState = DC_Document_IssuingState.Replace("<", "").Trim();
                                                DC_Document_Lastname = DC_Document_Lastname.Replace("<", "").Trim();
                                                DC_Document_Firstname = DC_Document_Firstname.Replace("<", "").Trim();
                                                DC_Document_Middlename = DC_Document_Middlename.Replace("<", "").Trim();
                                                DC_Document_Line1_Message = DC_Document_Line1_Message.Replace("<", "").Trim();
                                                DC_Document_PassportNumber = DC_Document_PassportNumber.Replace("<", "").Trim();
                                                DC_Document_Number_Checksum = DC_Document_Number_Checksum.Replace("<", "").Trim();
                                                DC_Document_NationalityCode = DC_Document_NationalityCode.Replace("<", "").Trim();
                                                DC_Document_BirthDate = DC_Document_BirthDate.Replace("<", "").Trim();
                                                DC_Document_BirthDate_Checksum = DC_Document_BirthDate_Checksum.Replace("<", "").Trim();
                                                DC_Document_Gender = DC_Document_Gender.Replace("<", "").Trim();
                                                DC_Document_ExpiryDate = DC_Document_ExpiryDate.Replace("<", "").Trim();
                                                DC_Document_ExpiryDate_Checksum = DC_Document_ExpiryDate_Checksum.Replace("<", "").Trim();
                                                DC_Document_Personal_Number = DC_Document_Personal_Number.Replace("<", "").Trim();
                                                DC_Document_Personal_Number_Checksum = DC_Document_Personal_Number_Checksum.Replace("<", "").Trim();
                                                DC_Document_Checksum = DC_Document_Checksum.Replace("<", "").Trim();
                                                DC_Document_Line2_Message = DC_Document_Line2_Message.Replace("<", "").Trim();
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','1','Document_Type_Code','" + DC_Document_Type + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','2','Issuing_State_Code','" + DC_Document_IssuingState + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','3','Last_Name','" + DC_Document_Lastname + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','4','First_Name','" + DC_Document_Firstname + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','5','Middle_Name','" + DC_Document_Middlename + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','6','Line1_Message','" + DC_Document_Line1_Message + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','7','Passport_Number','" + DC_Document_PassportNumber + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','8','Passport_Number_Checkdigit','" + DC_Document_Number_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','9','Nationality_Code','" + DC_Document_NationalityCode + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','10','BirthDate','" + DC_Document_BirthDate + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','11','BirthDate_Checkdigit','" + DC_Document_BirthDate_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','12','Gender','" + DC_Document_Gender + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','13','Expiry_Date','" + DC_Document_ExpiryDate + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','14','Expiry_Date_Checkdigit','" + DC_Document_ExpiryDate_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','15','Personal_Number','" + DC_Document_Personal_Number + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','16','Personal_Number_Checkdigit','" + DC_Document_Personal_Number_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','17','Document_Checkdigit','" + DC_Document_Checksum + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_20_API_Passport_MRZ_Decoded Values ('" + TransactionID + "','18','Line2_Message','" + DC_Document_Line2_Message + "')");
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                string PassportJson_Result_TextOverlay = "";
                                                string PassportJson_Result_ParsedText = "";
                                                string PassportJson_Message = "";
                                                if (File.Exists(Image_Path_Passport_Last) == true)
                                                {
                                                    Image IMG_Con_Pass = new Bitmap(Image_Path_Passport_Last);
                                                    string IMG_Con_Pass_Format = PB.GetFile_Type(Image_Path_Passport_Last);
                                                    string OCR_Orientation = RequestOCR_Image((Bitmap)IMG_Con_Pass, IMG_Con_Pass_Format, Req_Overlay_Required, Req_Image_Scale);
                                                    OCR_Result OCR_Orientation_Res = JsonConvert.DeserializeObject<OCR_Result>(OCR_Orientation);
                                                    PassportJson_Result_TextOverlay = OCR_Orientation.Trim();
                                                    PassportJson_Result_ParsedText = OCR_Orientation_Res.ParsedResults[0].ParsedText.ToString().Trim();
                                                }
                                                else
                                                {
                                                    PassportJson_Message = "Passport image file not detected";
                                                }
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace(',', '#');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\'', '$');
                                                PassportJson_Result_TextOverlay = PassportJson_Result_TextOverlay.Replace('\"', '%');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace(',', '#');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\'', '$');
                                                PassportJson_Result_ParsedText = PassportJson_Result_ParsedText.Replace('\"', '%');
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_21_API_Passport_OCR Values ('" + TransactionID + "','" + PassportJson_Result_TextOverlay + "','" + PassportJson_Result_ParsedText + "','" + PassportJson_Message + "')");
                                            }
                                            catch (Exception) { }
                                            try
                                            {
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_22_API_Passport_OCR_Decoded Values ('" + TransactionID + "','0','Message','We are updating this section, at the moment it is not possible to classification passport information, for more information please contact the support team')");
                                            }
                                            catch (Exception) { }
                                            break;
                                        }
                                }
                                //-------------------------------------------------------------------------------------------------
                                // Document Validation :
                                // 1 : Failed
                                // 2 : Passed
                                // 3 : Refered
                                if (Req_Document_Validation == "1")
                                {
                                    if ((Req_Processing_Type == "1") || (Req_Processing_Type == "2"))
                                    {
                                        string Val_Line1_Length = "Failed";
                                        string Val_Line2_Length = "Failed";
                                        string Val_Document_Type = "Failed";
                                        string Val_Passportnumber_SumCheck = "Failed";
                                        string Val_Birthdate_SumCheck = "Failed";
                                        string Val_Expirydate_SumCheck = "Failed";
                                        string Val_Expirydate_Expiry = "Failed";
                                        string Val_Document_SumCheck = "Failed";
                                        string Val_Document_SumCheck_Iseven = "Failed";
                                        string Val_Gender_Check = "Failed";
                                        string Val_Personal_Number_Check = "Failed";
                                        string Val_IssuingState = "Failed";
                                        string Val_NationalituCode = "Failed";
                                        DataTable DT_MRZ = new DataTable();
                                        DT_MRZ = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_19_API_Passport_MRZ Where (Transaction_ID = '" + TransactionID + "')");
                                        if (DT_MRZ.Rows != null)
                                        {
                                            if (DT_MRZ.Rows.Count == 1)
                                            {
                                                string MRZLN_1 = ""; string MRZLN_2 = "";
                                                try { MRZLN_1 = DT_MRZ.Rows[0][1].ToString().Trim(); } catch (Exception) { }
                                                try { MRZLN_2 = DT_MRZ.Rows[0][2].ToString().Trim(); } catch (Exception) { }
                                                MRZLN_1 = MRZLN_1.Trim(); MRZLN_2 = MRZLN_2.Trim();
                                                string Val_DC_Document_Type = "";
                                                string Val_DC_Document_IssuingState = "";
                                                string Val_DC_Document_Lastname = "";
                                                string Val_DC_Document_Firstname = "";
                                                string Val_DC_Document_Middlename = "";
                                                if (MRZLN_1 != "")
                                                {
                                                    if (MRZLN_1.Length == 44)
                                                    {
                                                        try { Val_DC_Document_Type = MRZLN_1.Substring(0, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_IssuingState = MRZLN_1.Substring(2, 3); } catch (Exception) { }
                                                        try
                                                        {
                                                            string FullnameDetector = MRZLN_1.Substring(5, MRZLN_1.Length - 5);
                                                            FullnameDetector = FullnameDetector.Replace("<", " ");
                                                            FullnameDetector = FullnameDetector.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                                                            FullnameDetector = FullnameDetector.Trim();
                                                            string[] FullnameDetector_Sub = FullnameDetector.Split(' ');
                                                            try { Val_DC_Document_Lastname = FullnameDetector_Sub[0].Trim(); } catch (Exception) { }
                                                            try { Val_DC_Document_Firstname = FullnameDetector_Sub[1].Trim(); } catch (Exception) { }
                                                            try { for (int i = 2; i <= FullnameDetector_Sub.Count(); i++) { Val_DC_Document_Middlename += FullnameDetector_Sub[i] + " "; } Val_DC_Document_Middlename = Val_DC_Document_Middlename.Trim(); } catch (Exception) { }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                }
                                                string Val_DC_Document_PassportNumber = "";
                                                string Val_DC_Document_Number_Checksum = "";
                                                string Val_DC_Document_NationalityCode = "";
                                                string Val_DC_Document_BirthDate = "";
                                                string Val_DC_Document_BirthDate_Checksum = "";
                                                string Val_DC_Document_Gender = "";
                                                string Val_DC_Document_ExpiryDate = "";
                                                string Val_DC_Document_ExpiryDate_Checksum = "";
                                                string Val_DC_Document_ExtraInfo = "";
                                                string Val_DC_Document_ExtraInfo_Checksum = "";
                                                string Val_DC_Document_Checksum = "";
                                                if (MRZLN_2 != "")
                                                {
                                                    if (MRZLN_2.Length == 44)
                                                    {
                                                        try { Val_DC_Document_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 1, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExtraInfo_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 2, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExtraInfo = MRZLN_2.Substring(MRZLN_2.Length - 16, 14); } catch (Exception) { }
                                                        try { Val_DC_Document_ExpiryDate_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 17, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExpiryDate = MRZLN_2.Substring(MRZLN_2.Length - 23, 6); } catch (Exception) { }
                                                        try { Val_DC_Document_Gender = MRZLN_2.Substring(MRZLN_2.Length - 24, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_BirthDate_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 25, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_BirthDate = MRZLN_2.Substring(MRZLN_2.Length - 31, 6); } catch (Exception) { }
                                                        try { Val_DC_Document_NationalityCode = MRZLN_2.Substring(MRZLN_2.Length - 34, 3); } catch (Exception) { }
                                                        try { Val_DC_Document_Number_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 35, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_PassportNumber = MRZLN_2.Substring(MRZLN_2.Length - 44, 9); } catch (Exception) { }
                                                    }
                                                }
                                                Val_DC_Document_Type = Val_DC_Document_Type.Trim().ToUpper();
                                                Val_DC_Document_IssuingState = Val_DC_Document_IssuingState.Trim().ToUpper();
                                                Val_DC_Document_Lastname = Val_DC_Document_Lastname.Trim().ToUpper();
                                                Val_DC_Document_Firstname = Val_DC_Document_Firstname.Trim().ToUpper();
                                                Val_DC_Document_Middlename = Val_DC_Document_Middlename.Trim().ToUpper();
                                                Val_DC_Document_PassportNumber = Val_DC_Document_PassportNumber.Trim();
                                                Val_DC_Document_Number_Checksum = Val_DC_Document_Number_Checksum.Trim().ToUpper();
                                                Val_DC_Document_NationalityCode = Val_DC_Document_NationalityCode.Trim().ToUpper();
                                                Val_DC_Document_BirthDate = Val_DC_Document_BirthDate.Trim().ToUpper();
                                                Val_DC_Document_BirthDate_Checksum = Val_DC_Document_BirthDate_Checksum.Trim().ToUpper();
                                                Val_DC_Document_Gender = Val_DC_Document_Gender.Trim().ToUpper();
                                                Val_DC_Document_ExpiryDate = Val_DC_Document_ExpiryDate.Trim().ToUpper();
                                                Val_DC_Document_ExpiryDate_Checksum = Val_DC_Document_ExpiryDate_Checksum.Trim().ToUpper();
                                                Val_DC_Document_ExtraInfo = Val_DC_Document_ExtraInfo.Trim().ToUpper();
                                                Val_DC_Document_ExtraInfo_Checksum = Val_DC_Document_ExtraInfo_Checksum.Trim().ToUpper();
                                                Val_DC_Document_Checksum = Val_DC_Document_Checksum.Trim().ToUpper();
                                                MRZLN_1 = MRZLN_1.Trim().ToUpper();
                                                MRZLN_2 = MRZLN_2.Trim().ToUpper();
                                                // Test Last Validation :
                                                if (MRZLN_1.Length == 44) { Val_Line1_Length = "Passed"; }
                                                if (MRZLN_2.Length == 44) { Val_Line2_Length = "Passed"; }
                                                if (Val_DC_Document_Type == "P") { Val_Document_Type = "Passed"; }
                                                if ((Val_DC_Document_Gender == "M") || (Val_DC_Document_Gender == "F")) { Val_Gender_Check = "Passed"; }
                                                try
                                                {
                                                    int[] EXINF_D = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                                    int EXINF_D_Sum = 0;
                                                    for (int i = 0; i < 14; i++)
                                                    {
                                                        int LSTEICVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_ExtraInfo[i])
                                                            {
                                                                case '0': { LSTEICVal = 0; break; }
                                                                case '1': { LSTEICVal = 1; break; }
                                                                case '2': { LSTEICVal = 2; break; }
                                                                case '3': { LSTEICVal = 3; break; }
                                                                case '4': { LSTEICVal = 4; break; }
                                                                case '5': { LSTEICVal = 5; break; }
                                                                case '6': { LSTEICVal = 6; break; }
                                                                case '7': { LSTEICVal = 7; break; }
                                                                case '8': { LSTEICVal = 8; break; }
                                                                case '9': { LSTEICVal = 9; break; }
                                                                case '<': { LSTEICVal = 0; break; }
                                                                case 'A': { LSTEICVal = 10; break; }
                                                                case 'B': { LSTEICVal = 11; break; }
                                                                case 'C': { LSTEICVal = 12; break; }
                                                                case 'D': { LSTEICVal = 13; break; }
                                                                case 'E': { LSTEICVal = 14; break; }
                                                                case 'F': { LSTEICVal = 15; break; }
                                                                case 'G': { LSTEICVal = 16; break; }
                                                                case 'H': { LSTEICVal = 17; break; }
                                                                case 'I': { LSTEICVal = 18; break; }
                                                                case 'J': { LSTEICVal = 19; break; }
                                                                case 'K': { LSTEICVal = 20; break; }
                                                                case 'L': { LSTEICVal = 21; break; }
                                                                case 'M': { LSTEICVal = 22; break; }
                                                                case 'N': { LSTEICVal = 23; break; }
                                                                case 'O': { LSTEICVal = 24; break; }
                                                                case 'P': { LSTEICVal = 25; break; }
                                                                case 'Q': { LSTEICVal = 26; break; }
                                                                case 'R': { LSTEICVal = 27; break; }
                                                                case 'S': { LSTEICVal = 28; break; }
                                                                case 'T': { LSTEICVal = 29; break; }
                                                                case 'U': { LSTEICVal = 30; break; }
                                                                case 'V': { LSTEICVal = 31; break; }
                                                                case 'W': { LSTEICVal = 32; break; }
                                                                case 'X': { LSTEICVal = 33; break; }
                                                                case 'Y': { LSTEICVal = 34; break; }
                                                                case 'Z': { LSTEICVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        EXINF_D[i] = LSTEICVal;
                                                    }
                                                    EXINF_D_Sum = (EXINF_D[0] * 7) + (EXINF_D[1] * 3) + (EXINF_D[2] * 1) + (EXINF_D[3] * 7) + (EXINF_D[4] * 3) + (EXINF_D[5] * 1) + (EXINF_D[6] * 7) + (EXINF_D[7] * 3) + (EXINF_D[8] * 1) + (EXINF_D[9] * 7) + (EXINF_D[10] * 3) + (EXINF_D[11] * 1) + (EXINF_D[12] * 7) + (EXINF_D[13] * 3);
                                                    if ((EXINF_D_Sum % 10).ToString() == Val_DC_Document_ExtraInfo_Checksum) { Val_Personal_Number_Check = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] PassNum_D = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                                    int PassNum_D_Sum = 0;
                                                    for (int i = 0; i < 9; i++)
                                                    {
                                                        int LSTPNCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_PassportNumber[i])
                                                            {
                                                                case '0': { LSTPNCVal = 0; break; }
                                                                case '1': { LSTPNCVal = 1; break; }
                                                                case '2': { LSTPNCVal = 2; break; }
                                                                case '3': { LSTPNCVal = 3; break; }
                                                                case '4': { LSTPNCVal = 4; break; }
                                                                case '5': { LSTPNCVal = 5; break; }
                                                                case '6': { LSTPNCVal = 6; break; }
                                                                case '7': { LSTPNCVal = 7; break; }
                                                                case '8': { LSTPNCVal = 8; break; }
                                                                case '9': { LSTPNCVal = 9; break; }
                                                                case '<': { LSTPNCVal = 0; break; }
                                                                case 'A': { LSTPNCVal = 10; break; }
                                                                case 'B': { LSTPNCVal = 11; break; }
                                                                case 'C': { LSTPNCVal = 12; break; }
                                                                case 'D': { LSTPNCVal = 13; break; }
                                                                case 'E': { LSTPNCVal = 14; break; }
                                                                case 'F': { LSTPNCVal = 15; break; }
                                                                case 'G': { LSTPNCVal = 16; break; }
                                                                case 'H': { LSTPNCVal = 17; break; }
                                                                case 'I': { LSTPNCVal = 18; break; }
                                                                case 'J': { LSTPNCVal = 19; break; }
                                                                case 'K': { LSTPNCVal = 20; break; }
                                                                case 'L': { LSTPNCVal = 21; break; }
                                                                case 'M': { LSTPNCVal = 22; break; }
                                                                case 'N': { LSTPNCVal = 23; break; }
                                                                case 'O': { LSTPNCVal = 24; break; }
                                                                case 'P': { LSTPNCVal = 25; break; }
                                                                case 'Q': { LSTPNCVal = 26; break; }
                                                                case 'R': { LSTPNCVal = 27; break; }
                                                                case 'S': { LSTPNCVal = 28; break; }
                                                                case 'T': { LSTPNCVal = 29; break; }
                                                                case 'U': { LSTPNCVal = 30; break; }
                                                                case 'V': { LSTPNCVal = 31; break; }
                                                                case 'W': { LSTPNCVal = 32; break; }
                                                                case 'X': { LSTPNCVal = 33; break; }
                                                                case 'Y': { LSTPNCVal = 34; break; }
                                                                case 'Z': { LSTPNCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        PassNum_D[i] = LSTPNCVal;
                                                    }
                                                    PassNum_D_Sum = (PassNum_D[0] * 7) + (PassNum_D[1] * 3) + (PassNum_D[2] * 1) + (PassNum_D[3] * 7) + (PassNum_D[4] * 3) + (PassNum_D[5] * 1) + (PassNum_D[6] * 7) + (PassNum_D[7] * 3) + (PassNum_D[8] * 1);
                                                    if ((PassNum_D_Sum % 10).ToString() == Val_DC_Document_Number_Checksum) { Val_Passportnumber_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] BDate_D = { 0, 0, 0, 0, 0, 0 };
                                                    int BDate_D_Sum = 0;
                                                    for (int i = 0; i < 6; i++)
                                                    {
                                                        int LSTBDCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_BirthDate[i])
                                                            {
                                                                case '0': { LSTBDCVal = 0; break; }
                                                                case '1': { LSTBDCVal = 1; break; }
                                                                case '2': { LSTBDCVal = 2; break; }
                                                                case '3': { LSTBDCVal = 3; break; }
                                                                case '4': { LSTBDCVal = 4; break; }
                                                                case '5': { LSTBDCVal = 5; break; }
                                                                case '6': { LSTBDCVal = 6; break; }
                                                                case '7': { LSTBDCVal = 7; break; }
                                                                case '8': { LSTBDCVal = 8; break; }
                                                                case '9': { LSTBDCVal = 9; break; }
                                                                case '<': { LSTBDCVal = 0; break; }
                                                                case 'A': { LSTBDCVal = 10; break; }
                                                                case 'B': { LSTBDCVal = 11; break; }
                                                                case 'C': { LSTBDCVal = 12; break; }
                                                                case 'D': { LSTBDCVal = 13; break; }
                                                                case 'E': { LSTBDCVal = 14; break; }
                                                                case 'F': { LSTBDCVal = 15; break; }
                                                                case 'G': { LSTBDCVal = 16; break; }
                                                                case 'H': { LSTBDCVal = 17; break; }
                                                                case 'I': { LSTBDCVal = 18; break; }
                                                                case 'J': { LSTBDCVal = 19; break; }
                                                                case 'K': { LSTBDCVal = 20; break; }
                                                                case 'L': { LSTBDCVal = 21; break; }
                                                                case 'M': { LSTBDCVal = 22; break; }
                                                                case 'N': { LSTBDCVal = 23; break; }
                                                                case 'O': { LSTBDCVal = 24; break; }
                                                                case 'P': { LSTBDCVal = 25; break; }
                                                                case 'Q': { LSTBDCVal = 26; break; }
                                                                case 'R': { LSTBDCVal = 27; break; }
                                                                case 'S': { LSTBDCVal = 28; break; }
                                                                case 'T': { LSTBDCVal = 29; break; }
                                                                case 'U': { LSTBDCVal = 30; break; }
                                                                case 'V': { LSTBDCVal = 31; break; }
                                                                case 'W': { LSTBDCVal = 32; break; }
                                                                case 'X': { LSTBDCVal = 33; break; }
                                                                case 'Y': { LSTBDCVal = 34; break; }
                                                                case 'Z': { LSTBDCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        BDate_D[i] = LSTBDCVal;
                                                    }
                                                    BDate_D_Sum = (BDate_D[0] * 7) + (BDate_D[1] * 3) + (BDate_D[2] * 1) + (BDate_D[3] * 7) + (BDate_D[4] * 3) + (BDate_D[5] * 1);
                                                    if ((BDate_D_Sum % 10).ToString() == Val_DC_Document_BirthDate_Checksum) { Val_Birthdate_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] EDate_D = { 0, 0, 0, 0, 0, 0 };
                                                    int EDate_D_Sum = 0;
                                                    for (int i = 0; i < 6; i++)
                                                    {
                                                        int LSTEDCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_ExpiryDate[i])
                                                            {
                                                                case '0': { LSTEDCVal = 0; break; }
                                                                case '1': { LSTEDCVal = 1; break; }
                                                                case '2': { LSTEDCVal = 2; break; }
                                                                case '3': { LSTEDCVal = 3; break; }
                                                                case '4': { LSTEDCVal = 4; break; }
                                                                case '5': { LSTEDCVal = 5; break; }
                                                                case '6': { LSTEDCVal = 6; break; }
                                                                case '7': { LSTEDCVal = 7; break; }
                                                                case '8': { LSTEDCVal = 8; break; }
                                                                case '9': { LSTEDCVal = 9; break; }
                                                                case '<': { LSTEDCVal = 0; break; }
                                                                case 'A': { LSTEDCVal = 10; break; }
                                                                case 'B': { LSTEDCVal = 11; break; }
                                                                case 'C': { LSTEDCVal = 12; break; }
                                                                case 'D': { LSTEDCVal = 13; break; }
                                                                case 'E': { LSTEDCVal = 14; break; }
                                                                case 'F': { LSTEDCVal = 15; break; }
                                                                case 'G': { LSTEDCVal = 16; break; }
                                                                case 'H': { LSTEDCVal = 17; break; }
                                                                case 'I': { LSTEDCVal = 18; break; }
                                                                case 'J': { LSTEDCVal = 19; break; }
                                                                case 'K': { LSTEDCVal = 20; break; }
                                                                case 'L': { LSTEDCVal = 21; break; }
                                                                case 'M': { LSTEDCVal = 22; break; }
                                                                case 'N': { LSTEDCVal = 23; break; }
                                                                case 'O': { LSTEDCVal = 24; break; }
                                                                case 'P': { LSTEDCVal = 25; break; }
                                                                case 'Q': { LSTEDCVal = 26; break; }
                                                                case 'R': { LSTEDCVal = 27; break; }
                                                                case 'S': { LSTEDCVal = 28; break; }
                                                                case 'T': { LSTEDCVal = 29; break; }
                                                                case 'U': { LSTEDCVal = 30; break; }
                                                                case 'V': { LSTEDCVal = 31; break; }
                                                                case 'W': { LSTEDCVal = 32; break; }
                                                                case 'X': { LSTEDCVal = 33; break; }
                                                                case 'Y': { LSTEDCVal = 34; break; }
                                                                case 'Z': { LSTEDCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        EDate_D[i] = LSTEDCVal;
                                                    }
                                                    EDate_D_Sum = (EDate_D[0] * 7) + (EDate_D[1] * 3) + (EDate_D[2] * 1) + (EDate_D[3] * 7) + (EDate_D[4] * 3) + (EDate_D[5] * 1);
                                                    if ((EDate_D_Sum % 10).ToString() == Val_DC_Document_ExpiryDate_Checksum) { Val_Expirydate_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                                                    DateTime CVT_Val_DC_Document_ExpiryDate = DateTime.ParseExact(Val_DC_Document_ExpiryDate, "yyMMdd", new CultureInfo("en-AU"));
                                                    if (CVT_Val_DC_Document_ExpiryDate.Date >= DateTime.Now.Date) { Val_Expirydate_Expiry = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    string MRZFull = MRZLN_2.Substring(0, 10) + MRZLN_2.Substring(13, 7) + MRZLN_2.Substring(21, 22);
                                                    int SumAll = 0;
                                                    int SumStep = 0;
                                                    int ChartDig = 0;
                                                    for (int i = 0; i < MRZFull.Length; i++)
                                                    {
                                                        ChartDig = 0;
                                                        if (SumStep >= 3) { SumStep = 0; }
                                                        SumStep++;
                                                        try
                                                        {
                                                            switch (MRZFull[i])
                                                            {
                                                                case '0': { ChartDig = 0; break; }
                                                                case '1': { ChartDig = 1; break; }
                                                                case '2': { ChartDig = 2; break; }
                                                                case '3': { ChartDig = 3; break; }
                                                                case '4': { ChartDig = 4; break; }
                                                                case '5': { ChartDig = 5; break; }
                                                                case '6': { ChartDig = 6; break; }
                                                                case '7': { ChartDig = 7; break; }
                                                                case '8': { ChartDig = 8; break; }
                                                                case '9': { ChartDig = 9; break; }
                                                                case '<': { ChartDig = 0; break; }
                                                                case 'A': { ChartDig = 10; break; }
                                                                case 'B': { ChartDig = 11; break; }
                                                                case 'C': { ChartDig = 12; break; }
                                                                case 'D': { ChartDig = 13; break; }
                                                                case 'E': { ChartDig = 14; break; }
                                                                case 'F': { ChartDig = 15; break; }
                                                                case 'G': { ChartDig = 16; break; }
                                                                case 'H': { ChartDig = 17; break; }
                                                                case 'I': { ChartDig = 18; break; }
                                                                case 'J': { ChartDig = 19; break; }
                                                                case 'K': { ChartDig = 20; break; }
                                                                case 'L': { ChartDig = 21; break; }
                                                                case 'M': { ChartDig = 22; break; }
                                                                case 'N': { ChartDig = 23; break; }
                                                                case 'O': { ChartDig = 24; break; }
                                                                case 'P': { ChartDig = 25; break; }
                                                                case 'Q': { ChartDig = 26; break; }
                                                                case 'R': { ChartDig = 27; break; }
                                                                case 'S': { ChartDig = 28; break; }
                                                                case 'T': { ChartDig = 29; break; }
                                                                case 'U': { ChartDig = 30; break; }
                                                                case 'V': { ChartDig = 31; break; }
                                                                case 'W': { ChartDig = 32; break; }
                                                                case 'X': { ChartDig = 33; break; }
                                                                case 'Y': { ChartDig = 34; break; }
                                                                case 'Z': { ChartDig = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        if (SumStep == 1) { SumAll += (ChartDig * 7); }
                                                        if (SumStep == 2) { SumAll += (ChartDig * 3); }
                                                        if (SumStep == 3) { SumAll += (ChartDig * 1); }
                                                    }
                                                    if ((SumAll % 10).ToString() == Val_DC_Document_Checksum)
                                                    {
                                                        Val_Document_SumCheck = "Passed";
                                                        try
                                                        {
                                                            int ModDocSC = (SumAll % 10);
                                                            if ((ModDocSC % 2) == 0) { Val_Document_SumCheck_Iseven = "Passed"; }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    DataTable DTCC1 = new DataTable();
                                                    DTCC1 = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Country_Name From Setting_Basic_08_CountryCode Where (Alpha3 = '" + Val_DC_Document_IssuingState + "')");
                                                    if (DTCC1.Rows != null)
                                                    {
                                                        if (DTCC1.Rows.Count == 1)
                                                        {
                                                            Val_IssuingState = "Passed";
                                                        }
                                                    }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    DataTable DTCC2 = new DataTable();
                                                    DTCC2 = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Country_Name From Setting_Basic_08_CountryCode Where (Alpha3 = '" + Val_DC_Document_NationalityCode + "')");
                                                    if (DTCC2.Rows != null)
                                                    {
                                                        if (DTCC2.Rows.Count == 1)
                                                        {
                                                            Val_NationalituCode = "Passed";
                                                        }
                                                    }
                                                }
                                                catch (Exception) { }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','1','Length_Line1','" + Val_Line1_Length + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','2','Length_Line2','" + Val_Line2_Length + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','3','Document_Type','" + Val_Document_Type + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','4','Passport_Number_Check','" + Val_Passportnumber_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','5','Birthdate_Check','" + Val_Birthdate_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','6','Expiry_Date','" + Val_Expirydate_Expiry + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','7','Expiry_Date_Check','" + Val_Expirydate_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','8','MRZ_Check','" + Val_Document_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','9','MRZ_Check_Even','" + Val_Document_SumCheck_Iseven + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','10','Gender_Type','" + Val_Gender_Check + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','11','Personal_Number_Check','" + Val_Personal_Number_Check + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','12','Issuing_State','" + Val_IssuingState + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','13','Nationality','" + Val_NationalituCode + "')");
                                                int LastValidation_Code = 2;
                                                if (Val_Line1_Length.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Line2_Length.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_Type.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Passportnumber_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Birthdate_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Expirydate_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Expirydate_Expiry.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_SumCheck_Iseven.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Gender_Check.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Personal_Number_Check.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_IssuingState.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_NationalituCode.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (MRZLN_1.Length != 44) { LastValidation_Code = 3; }
                                                if (MRZLN_2.Length != 44) { LastValidation_Code = 3; }
                                                if (MRZLN_1.Trim() == "") { LastValidation_Code = 3; }
                                                if (MRZLN_2.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Type.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_IssuingState.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Lastname.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Firstname.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_PassportNumber.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Number_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_NationalityCode.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_BirthDate.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_BirthDate_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Gender.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExpiryDate.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExpiryDate_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExtraInfo.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExtraInfo_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_IssuingState.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_NationalityCode.Trim() == "") { LastValidation_Code = 3; }
                                                if (LastValidation_Code == 1) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','1','Failed')"); }
                                                if (LastValidation_Code == 2) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','2','Passed')"); }
                                                if (LastValidation_Code == 3) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','3','Refered')"); }
                                            }
                                        }
                                    }
                                    if ((Req_Processing_Type == "3") || (Req_Processing_Type == "4"))
                                    {
                                        try
                                        {
                                            SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','0','Message','In this type of processing, validation is not possible')");
                                            SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','1','Failed')");
                                        }
                                        catch (Exception) { }
                                    }
                                    if ((Req_Processing_Type == "5") || (Req_Processing_Type == "6"))
                                    {
                                        string Val_Line1_Length = "Failed";
                                        string Val_Line2_Length = "Failed";
                                        string Val_Document_Type = "Failed";
                                        string Val_Passportnumber_SumCheck = "Failed";
                                        string Val_Birthdate_SumCheck = "Failed";
                                        string Val_Expirydate_SumCheck = "Failed";
                                        string Val_Expirydate_Expiry = "Failed";
                                        string Val_Document_SumCheck = "Failed";
                                        string Val_Document_SumCheck_Iseven = "Failed";
                                        string Val_Gender_Check = "Failed";
                                        string Val_Personal_Number_Check = "Failed";
                                        string Val_IssuingState = "Failed";
                                        string Val_NationalituCode = "Failed";
                                        string Match_Fistname = "Failed";
                                        string Match_Lastname = "Failed";
                                        string Match_Middlename = "Failed";
                                        string Match_Passportnumber = "Failed";
                                        string Match_Birthdate = "Failed";
                                        string Match_Expiredate = "Failed";
                                        DataTable DT_MRZ = new DataTable();
                                        DT_MRZ = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_19_API_Passport_MRZ Where (Transaction_ID = '" + TransactionID + "')");
                                        if (DT_MRZ.Rows != null)
                                        {
                                            if (DT_MRZ.Rows.Count == 1)
                                            {
                                                string MRZLN_1 = ""; string MRZLN_2 = "";
                                                try { MRZLN_1 = DT_MRZ.Rows[0][1].ToString().Trim(); } catch (Exception) { }
                                                try { MRZLN_2 = DT_MRZ.Rows[0][2].ToString().Trim(); } catch (Exception) { }
                                                MRZLN_1 = MRZLN_1.Trim(); MRZLN_2 = MRZLN_2.Trim();
                                                string Val_DC_Document_Type = "";
                                                string Val_DC_Document_IssuingState = "";
                                                string Val_DC_Document_Lastname = "";
                                                string Val_DC_Document_Firstname = "";
                                                string Val_DC_Document_Middlename = "";
                                                if (MRZLN_1 != "")
                                                {
                                                    if (MRZLN_1.Length == 44)
                                                    {
                                                        try { Val_DC_Document_Type = MRZLN_1.Substring(0, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_IssuingState = MRZLN_1.Substring(2, 3); } catch (Exception) { }
                                                        try
                                                        {
                                                            string FullnameDetector = MRZLN_1.Substring(5, MRZLN_1.Length - 5);
                                                            FullnameDetector = FullnameDetector.Replace("<", " ");
                                                            FullnameDetector = FullnameDetector.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                                                            FullnameDetector = FullnameDetector.Trim();
                                                            string[] FullnameDetector_Sub = FullnameDetector.Split(' ');
                                                            try { Val_DC_Document_Lastname = FullnameDetector_Sub[0].Trim(); } catch (Exception) { }
                                                            try { Val_DC_Document_Firstname = FullnameDetector_Sub[1].Trim(); } catch (Exception) { }
                                                            try { for (int i = 2; i <= FullnameDetector_Sub.Count(); i++) { Val_DC_Document_Middlename += FullnameDetector_Sub[i] + " "; } Val_DC_Document_Middlename = Val_DC_Document_Middlename.Trim(); } catch (Exception) { }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                }
                                                string Val_DC_Document_PassportNumber = "";
                                                string Val_DC_Document_Number_Checksum = "";
                                                string Val_DC_Document_NationalityCode = "";
                                                string Val_DC_Document_BirthDate = "";
                                                string Val_DC_Document_BirthDate_Checksum = "";
                                                string Val_DC_Document_Gender = "";
                                                string Val_DC_Document_ExpiryDate = "";
                                                string Val_DC_Document_ExpiryDate_Checksum = "";
                                                string Val_DC_Document_ExtraInfo = "";
                                                string Val_DC_Document_ExtraInfo_Checksum = "";
                                                string Val_DC_Document_Checksum = "";
                                                if (MRZLN_2 != "")
                                                {
                                                    if (MRZLN_2.Length == 44)
                                                    {
                                                        try { Val_DC_Document_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 1, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExtraInfo_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 2, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExtraInfo = MRZLN_2.Substring(MRZLN_2.Length - 16, 14); } catch (Exception) { }
                                                        try { Val_DC_Document_ExpiryDate_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 17, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_ExpiryDate = MRZLN_2.Substring(MRZLN_2.Length - 23, 6); } catch (Exception) { }
                                                        try { Val_DC_Document_Gender = MRZLN_2.Substring(MRZLN_2.Length - 24, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_BirthDate_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 25, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_BirthDate = MRZLN_2.Substring(MRZLN_2.Length - 31, 6); } catch (Exception) { }
                                                        try { Val_DC_Document_NationalityCode = MRZLN_2.Substring(MRZLN_2.Length - 34, 3); } catch (Exception) { }
                                                        try { Val_DC_Document_Number_Checksum = MRZLN_2.Substring(MRZLN_2.Length - 35, 1); } catch (Exception) { }
                                                        try { Val_DC_Document_PassportNumber = MRZLN_2.Substring(MRZLN_2.Length - 44, 9); } catch (Exception) { }
                                                    }
                                                }
                                                Val_DC_Document_Type = Val_DC_Document_Type.Trim().ToUpper();
                                                Val_DC_Document_IssuingState = Val_DC_Document_IssuingState.Trim().ToUpper();
                                                Val_DC_Document_Lastname = Val_DC_Document_Lastname.Trim().ToUpper();
                                                Val_DC_Document_Firstname = Val_DC_Document_Firstname.Trim().ToUpper();
                                                Val_DC_Document_Middlename = Val_DC_Document_Middlename.Trim().ToUpper();
                                                Val_DC_Document_PassportNumber = Val_DC_Document_PassportNumber.Trim();
                                                Val_DC_Document_Number_Checksum = Val_DC_Document_Number_Checksum.Trim().ToUpper();
                                                Val_DC_Document_NationalityCode = Val_DC_Document_NationalityCode.Trim().ToUpper();
                                                Val_DC_Document_BirthDate = Val_DC_Document_BirthDate.Trim().ToUpper();
                                                Val_DC_Document_BirthDate_Checksum = Val_DC_Document_BirthDate_Checksum.Trim().ToUpper();
                                                Val_DC_Document_Gender = Val_DC_Document_Gender.Trim().ToUpper();
                                                Val_DC_Document_ExpiryDate = Val_DC_Document_ExpiryDate.Trim().ToUpper();
                                                Val_DC_Document_ExpiryDate_Checksum = Val_DC_Document_ExpiryDate_Checksum.Trim().ToUpper();
                                                Val_DC_Document_ExtraInfo = Val_DC_Document_ExtraInfo.Trim().ToUpper();
                                                Val_DC_Document_ExtraInfo_Checksum = Val_DC_Document_ExtraInfo_Checksum.Trim().ToUpper();
                                                Val_DC_Document_Checksum = Val_DC_Document_Checksum.Trim().ToUpper();
                                                MRZLN_1 = MRZLN_1.Trim().ToUpper();
                                                MRZLN_2 = MRZLN_2.Trim().ToUpper();
                                                // Test Last Validation :
                                                if (MRZLN_1.Length == 44) { Val_Line1_Length = "Passed"; }
                                                if (MRZLN_2.Length == 44) { Val_Line2_Length = "Passed"; }
                                                if (Val_DC_Document_Type == "P") { Val_Document_Type = "Passed"; }
                                                if ((Val_DC_Document_Gender == "M") || (Val_DC_Document_Gender == "F")) { Val_Gender_Check = "Passed"; }
                                                try
                                                {
                                                    int[] EXINF_D = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                                    int EXINF_D_Sum = 0;
                                                    for (int i = 0; i < 14; i++)
                                                    {
                                                        int LSTEICVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_ExtraInfo[i])
                                                            {
                                                                case '0': { LSTEICVal = 0; break; }
                                                                case '1': { LSTEICVal = 1; break; }
                                                                case '2': { LSTEICVal = 2; break; }
                                                                case '3': { LSTEICVal = 3; break; }
                                                                case '4': { LSTEICVal = 4; break; }
                                                                case '5': { LSTEICVal = 5; break; }
                                                                case '6': { LSTEICVal = 6; break; }
                                                                case '7': { LSTEICVal = 7; break; }
                                                                case '8': { LSTEICVal = 8; break; }
                                                                case '9': { LSTEICVal = 9; break; }
                                                                case '<': { LSTEICVal = 0; break; }
                                                                case 'A': { LSTEICVal = 10; break; }
                                                                case 'B': { LSTEICVal = 11; break; }
                                                                case 'C': { LSTEICVal = 12; break; }
                                                                case 'D': { LSTEICVal = 13; break; }
                                                                case 'E': { LSTEICVal = 14; break; }
                                                                case 'F': { LSTEICVal = 15; break; }
                                                                case 'G': { LSTEICVal = 16; break; }
                                                                case 'H': { LSTEICVal = 17; break; }
                                                                case 'I': { LSTEICVal = 18; break; }
                                                                case 'J': { LSTEICVal = 19; break; }
                                                                case 'K': { LSTEICVal = 20; break; }
                                                                case 'L': { LSTEICVal = 21; break; }
                                                                case 'M': { LSTEICVal = 22; break; }
                                                                case 'N': { LSTEICVal = 23; break; }
                                                                case 'O': { LSTEICVal = 24; break; }
                                                                case 'P': { LSTEICVal = 25; break; }
                                                                case 'Q': { LSTEICVal = 26; break; }
                                                                case 'R': { LSTEICVal = 27; break; }
                                                                case 'S': { LSTEICVal = 28; break; }
                                                                case 'T': { LSTEICVal = 29; break; }
                                                                case 'U': { LSTEICVal = 30; break; }
                                                                case 'V': { LSTEICVal = 31; break; }
                                                                case 'W': { LSTEICVal = 32; break; }
                                                                case 'X': { LSTEICVal = 33; break; }
                                                                case 'Y': { LSTEICVal = 34; break; }
                                                                case 'Z': { LSTEICVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        EXINF_D[i] = LSTEICVal;
                                                    }
                                                    EXINF_D_Sum = (EXINF_D[0] * 7) + (EXINF_D[1] * 3) + (EXINF_D[2] * 1) + (EXINF_D[3] * 7) + (EXINF_D[4] * 3) + (EXINF_D[5] * 1) + (EXINF_D[6] * 7) + (EXINF_D[7] * 3) + (EXINF_D[8] * 1) + (EXINF_D[9] * 7) + (EXINF_D[10] * 3) + (EXINF_D[11] * 1) + (EXINF_D[12] * 7) + (EXINF_D[13] * 3);
                                                    if ((EXINF_D_Sum % 10).ToString() == Val_DC_Document_ExtraInfo_Checksum) { Val_Personal_Number_Check = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] PassNum_D = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                                                    int PassNum_D_Sum = 0;
                                                    for (int i = 0; i < 9; i++)
                                                    {
                                                        int LSTPNCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_PassportNumber[i])
                                                            {
                                                                case '0': { LSTPNCVal = 0; break; }
                                                                case '1': { LSTPNCVal = 1; break; }
                                                                case '2': { LSTPNCVal = 2; break; }
                                                                case '3': { LSTPNCVal = 3; break; }
                                                                case '4': { LSTPNCVal = 4; break; }
                                                                case '5': { LSTPNCVal = 5; break; }
                                                                case '6': { LSTPNCVal = 6; break; }
                                                                case '7': { LSTPNCVal = 7; break; }
                                                                case '8': { LSTPNCVal = 8; break; }
                                                                case '9': { LSTPNCVal = 9; break; }
                                                                case '<': { LSTPNCVal = 0; break; }
                                                                case 'A': { LSTPNCVal = 10; break; }
                                                                case 'B': { LSTPNCVal = 11; break; }
                                                                case 'C': { LSTPNCVal = 12; break; }
                                                                case 'D': { LSTPNCVal = 13; break; }
                                                                case 'E': { LSTPNCVal = 14; break; }
                                                                case 'F': { LSTPNCVal = 15; break; }
                                                                case 'G': { LSTPNCVal = 16; break; }
                                                                case 'H': { LSTPNCVal = 17; break; }
                                                                case 'I': { LSTPNCVal = 18; break; }
                                                                case 'J': { LSTPNCVal = 19; break; }
                                                                case 'K': { LSTPNCVal = 20; break; }
                                                                case 'L': { LSTPNCVal = 21; break; }
                                                                case 'M': { LSTPNCVal = 22; break; }
                                                                case 'N': { LSTPNCVal = 23; break; }
                                                                case 'O': { LSTPNCVal = 24; break; }
                                                                case 'P': { LSTPNCVal = 25; break; }
                                                                case 'Q': { LSTPNCVal = 26; break; }
                                                                case 'R': { LSTPNCVal = 27; break; }
                                                                case 'S': { LSTPNCVal = 28; break; }
                                                                case 'T': { LSTPNCVal = 29; break; }
                                                                case 'U': { LSTPNCVal = 30; break; }
                                                                case 'V': { LSTPNCVal = 31; break; }
                                                                case 'W': { LSTPNCVal = 32; break; }
                                                                case 'X': { LSTPNCVal = 33; break; }
                                                                case 'Y': { LSTPNCVal = 34; break; }
                                                                case 'Z': { LSTPNCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        PassNum_D[i] = LSTPNCVal;
                                                    }
                                                    PassNum_D_Sum = (PassNum_D[0] * 7) + (PassNum_D[1] * 3) + (PassNum_D[2] * 1) + (PassNum_D[3] * 7) + (PassNum_D[4] * 3) + (PassNum_D[5] * 1) + (PassNum_D[6] * 7) + (PassNum_D[7] * 3) + (PassNum_D[8] * 1);
                                                    if ((PassNum_D_Sum % 10).ToString() == Val_DC_Document_Number_Checksum) { Val_Passportnumber_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] BDate_D = { 0, 0, 0, 0, 0, 0 };
                                                    int BDate_D_Sum = 0;
                                                    for (int i = 0; i < 6; i++)
                                                    {
                                                        int LSTBDCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_BirthDate[i])
                                                            {
                                                                case '0': { LSTBDCVal = 0; break; }
                                                                case '1': { LSTBDCVal = 1; break; }
                                                                case '2': { LSTBDCVal = 2; break; }
                                                                case '3': { LSTBDCVal = 3; break; }
                                                                case '4': { LSTBDCVal = 4; break; }
                                                                case '5': { LSTBDCVal = 5; break; }
                                                                case '6': { LSTBDCVal = 6; break; }
                                                                case '7': { LSTBDCVal = 7; break; }
                                                                case '8': { LSTBDCVal = 8; break; }
                                                                case '9': { LSTBDCVal = 9; break; }
                                                                case '<': { LSTBDCVal = 0; break; }
                                                                case 'A': { LSTBDCVal = 10; break; }
                                                                case 'B': { LSTBDCVal = 11; break; }
                                                                case 'C': { LSTBDCVal = 12; break; }
                                                                case 'D': { LSTBDCVal = 13; break; }
                                                                case 'E': { LSTBDCVal = 14; break; }
                                                                case 'F': { LSTBDCVal = 15; break; }
                                                                case 'G': { LSTBDCVal = 16; break; }
                                                                case 'H': { LSTBDCVal = 17; break; }
                                                                case 'I': { LSTBDCVal = 18; break; }
                                                                case 'J': { LSTBDCVal = 19; break; }
                                                                case 'K': { LSTBDCVal = 20; break; }
                                                                case 'L': { LSTBDCVal = 21; break; }
                                                                case 'M': { LSTBDCVal = 22; break; }
                                                                case 'N': { LSTBDCVal = 23; break; }
                                                                case 'O': { LSTBDCVal = 24; break; }
                                                                case 'P': { LSTBDCVal = 25; break; }
                                                                case 'Q': { LSTBDCVal = 26; break; }
                                                                case 'R': { LSTBDCVal = 27; break; }
                                                                case 'S': { LSTBDCVal = 28; break; }
                                                                case 'T': { LSTBDCVal = 29; break; }
                                                                case 'U': { LSTBDCVal = 30; break; }
                                                                case 'V': { LSTBDCVal = 31; break; }
                                                                case 'W': { LSTBDCVal = 32; break; }
                                                                case 'X': { LSTBDCVal = 33; break; }
                                                                case 'Y': { LSTBDCVal = 34; break; }
                                                                case 'Z': { LSTBDCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        BDate_D[i] = LSTBDCVal;
                                                    }
                                                    BDate_D_Sum = (BDate_D[0] * 7) + (BDate_D[1] * 3) + (BDate_D[2] * 1) + (BDate_D[3] * 7) + (BDate_D[4] * 3) + (BDate_D[5] * 1);
                                                    if ((BDate_D_Sum % 10).ToString() == Val_DC_Document_BirthDate_Checksum) { Val_Birthdate_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    int[] EDate_D = { 0, 0, 0, 0, 0, 0 };
                                                    int EDate_D_Sum = 0;
                                                    for (int i = 0; i < 6; i++)
                                                    {
                                                        int LSTEDCVal = 0;
                                                        try
                                                        {
                                                            switch (Val_DC_Document_ExpiryDate[i])
                                                            {
                                                                case '0': { LSTEDCVal = 0; break; }
                                                                case '1': { LSTEDCVal = 1; break; }
                                                                case '2': { LSTEDCVal = 2; break; }
                                                                case '3': { LSTEDCVal = 3; break; }
                                                                case '4': { LSTEDCVal = 4; break; }
                                                                case '5': { LSTEDCVal = 5; break; }
                                                                case '6': { LSTEDCVal = 6; break; }
                                                                case '7': { LSTEDCVal = 7; break; }
                                                                case '8': { LSTEDCVal = 8; break; }
                                                                case '9': { LSTEDCVal = 9; break; }
                                                                case '<': { LSTEDCVal = 0; break; }
                                                                case 'A': { LSTEDCVal = 10; break; }
                                                                case 'B': { LSTEDCVal = 11; break; }
                                                                case 'C': { LSTEDCVal = 12; break; }
                                                                case 'D': { LSTEDCVal = 13; break; }
                                                                case 'E': { LSTEDCVal = 14; break; }
                                                                case 'F': { LSTEDCVal = 15; break; }
                                                                case 'G': { LSTEDCVal = 16; break; }
                                                                case 'H': { LSTEDCVal = 17; break; }
                                                                case 'I': { LSTEDCVal = 18; break; }
                                                                case 'J': { LSTEDCVal = 19; break; }
                                                                case 'K': { LSTEDCVal = 20; break; }
                                                                case 'L': { LSTEDCVal = 21; break; }
                                                                case 'M': { LSTEDCVal = 22; break; }
                                                                case 'N': { LSTEDCVal = 23; break; }
                                                                case 'O': { LSTEDCVal = 24; break; }
                                                                case 'P': { LSTEDCVal = 25; break; }
                                                                case 'Q': { LSTEDCVal = 26; break; }
                                                                case 'R': { LSTEDCVal = 27; break; }
                                                                case 'S': { LSTEDCVal = 28; break; }
                                                                case 'T': { LSTEDCVal = 29; break; }
                                                                case 'U': { LSTEDCVal = 30; break; }
                                                                case 'V': { LSTEDCVal = 31; break; }
                                                                case 'W': { LSTEDCVal = 32; break; }
                                                                case 'X': { LSTEDCVal = 33; break; }
                                                                case 'Y': { LSTEDCVal = 34; break; }
                                                                case 'Z': { LSTEDCVal = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        EDate_D[i] = LSTEDCVal;
                                                    }
                                                    EDate_D_Sum = (EDate_D[0] * 7) + (EDate_D[1] * 3) + (EDate_D[2] * 1) + (EDate_D[3] * 7) + (EDate_D[4] * 3) + (EDate_D[5] * 1);
                                                    if ((EDate_D_Sum % 10).ToString() == Val_DC_Document_ExpiryDate_Checksum) { Val_Expirydate_SumCheck = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-AU");
                                                    DateTime CVT_Val_DC_Document_ExpiryDate = DateTime.ParseExact(Val_DC_Document_ExpiryDate, "yyMMdd", new CultureInfo("en-AU"));
                                                    if (CVT_Val_DC_Document_ExpiryDate.Date >= DateTime.Now.Date) { Val_Expirydate_Expiry = "Passed"; }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    string MRZFull = MRZLN_2.Substring(0, 10) + MRZLN_2.Substring(13, 7) + MRZLN_2.Substring(21, 22);
                                                    int SumAll = 0;
                                                    int SumStep = 0;
                                                    int ChartDig = 0;
                                                    for (int i = 0; i < MRZFull.Length; i++)
                                                    {
                                                        ChartDig = 0;
                                                        if (SumStep >= 3) { SumStep = 0; }
                                                        SumStep++;
                                                        try
                                                        {
                                                            switch (MRZFull[i])
                                                            {
                                                                case '0': { ChartDig = 0; break; }
                                                                case '1': { ChartDig = 1; break; }
                                                                case '2': { ChartDig = 2; break; }
                                                                case '3': { ChartDig = 3; break; }
                                                                case '4': { ChartDig = 4; break; }
                                                                case '5': { ChartDig = 5; break; }
                                                                case '6': { ChartDig = 6; break; }
                                                                case '7': { ChartDig = 7; break; }
                                                                case '8': { ChartDig = 8; break; }
                                                                case '9': { ChartDig = 9; break; }
                                                                case '<': { ChartDig = 0; break; }
                                                                case 'A': { ChartDig = 10; break; }
                                                                case 'B': { ChartDig = 11; break; }
                                                                case 'C': { ChartDig = 12; break; }
                                                                case 'D': { ChartDig = 13; break; }
                                                                case 'E': { ChartDig = 14; break; }
                                                                case 'F': { ChartDig = 15; break; }
                                                                case 'G': { ChartDig = 16; break; }
                                                                case 'H': { ChartDig = 17; break; }
                                                                case 'I': { ChartDig = 18; break; }
                                                                case 'J': { ChartDig = 19; break; }
                                                                case 'K': { ChartDig = 20; break; }
                                                                case 'L': { ChartDig = 21; break; }
                                                                case 'M': { ChartDig = 22; break; }
                                                                case 'N': { ChartDig = 23; break; }
                                                                case 'O': { ChartDig = 24; break; }
                                                                case 'P': { ChartDig = 25; break; }
                                                                case 'Q': { ChartDig = 26; break; }
                                                                case 'R': { ChartDig = 27; break; }
                                                                case 'S': { ChartDig = 28; break; }
                                                                case 'T': { ChartDig = 29; break; }
                                                                case 'U': { ChartDig = 30; break; }
                                                                case 'V': { ChartDig = 31; break; }
                                                                case 'W': { ChartDig = 32; break; }
                                                                case 'X': { ChartDig = 33; break; }
                                                                case 'Y': { ChartDig = 34; break; }
                                                                case 'Z': { ChartDig = 35; break; }
                                                            }
                                                        }
                                                        catch (Exception) { }
                                                        if (SumStep == 1) { SumAll += (ChartDig * 7); }
                                                        if (SumStep == 2) { SumAll += (ChartDig * 3); }
                                                        if (SumStep == 3) { SumAll += (ChartDig * 1); }
                                                    }
                                                    if ((SumAll % 10).ToString() == Val_DC_Document_Checksum)
                                                    {
                                                        Val_Document_SumCheck = "Passed";
                                                        try
                                                        {
                                                            int ModDocSC = (SumAll % 10);
                                                            if ((ModDocSC % 2) == 0) { Val_Document_SumCheck_Iseven = "Passed"; }
                                                        }
                                                        catch (Exception) { }
                                                    }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    DataTable DTCC1 = new DataTable();
                                                    DTCC1 = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Country_Name From Setting_Basic_08_CountryCode Where (Alpha3 = '" + Val_DC_Document_IssuingState + "')");
                                                    if (DTCC1.Rows != null)
                                                    {
                                                        if (DTCC1.Rows.Count == 1)
                                                        {
                                                            Val_IssuingState = "Passed";
                                                        }
                                                    }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    DataTable DTCC2 = new DataTable();
                                                    DTCC2 = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Country_Name From Setting_Basic_08_CountryCode Where (Alpha3 = '" + Val_DC_Document_NationalityCode + "')");
                                                    if (DTCC2.Rows != null)
                                                    {
                                                        if (DTCC2.Rows.Count == 1)
                                                        {
                                                            Val_NationalituCode = "Passed";
                                                        }
                                                    }
                                                }
                                                catch (Exception) { }
                                                try
                                                {
                                                    DataTable DT_FullTextOCR = new DataTable();
                                                    DT_FullTextOCR = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Passport_ParsedText From Users_21_API_Passport_OCR Where (Transaction_ID = '" + TransactionID + "')");
                                                    if (DT_FullTextOCR.Rows != null)
                                                    {
                                                        if (DT_FullTextOCR.Rows.Count == 1)
                                                        {
                                                            string FullTextOCR = DT_FullTextOCR.Rows[0][0].ToString().Trim();
                                                            FullTextOCR = FullTextOCR.Replace("  ", " ").Replace("/", "").Replace("\\", "").Replace("<", " ").Replace("\n", " ").Replace("\r", " ").Replace("#", " ").Replace("$", " ").Replace("%", " ").Trim();
                                                            if (FullTextOCR.IndexOf(Val_DC_Document_Firstname) > 0)
                                                            {
                                                                Match_Fistname = "Passed";
                                                            }
                                                            else
                                                            {
                                                                if (SF.GetWordsSimilarityInString(Val_DC_Document_Firstname, FullTextOCR) >= 90) { Match_Fistname = "Passed"; }
                                                            }
                                                            if (FullTextOCR.IndexOf(Val_DC_Document_Lastname) > 0)
                                                            {
                                                                Match_Lastname = "Passed";
                                                            }
                                                            else
                                                            {
                                                                if (SF.GetWordsSimilarityInString(Val_DC_Document_Lastname, FullTextOCR) >= 90) { Match_Lastname = "Passed"; }
                                                            }
                                                            if (FullTextOCR.IndexOf(Val_DC_Document_PassportNumber) > 0)
                                                            {
                                                                Match_Passportnumber = "Passed";
                                                            }
                                                            else
                                                            {
                                                                if (SF.GetWordsSimilarityInString(Val_DC_Document_PassportNumber, FullTextOCR) >= 90) { Match_Passportnumber = "Passed"; }
                                                            }
                                                            if (FullTextOCR.IndexOf(PB.ConvertDate_Format(Val_DC_Document_BirthDate, "yyMMdd", "ddMMyyyy")) > 0)
                                                            {
                                                                Match_Birthdate = "Passed";
                                                            }
                                                            else
                                                            {
                                                                if (SF.GetWordsSimilarityInString(PB.ConvertDate_Format(Val_DC_Document_BirthDate, "yyMMdd", "ddMMyyyy"), FullTextOCR) >= 90) { Match_Birthdate = "Passed"; }
                                                            }
                                                            if (FullTextOCR.IndexOf(PB.ConvertDate_Format(Val_DC_Document_ExpiryDate, "yyMMdd", "ddMMyyyy")) > 0)
                                                            {
                                                                Match_Expiredate = "Passed";
                                                            }
                                                            else
                                                            {
                                                                if (SF.GetWordsSimilarityInString(PB.ConvertDate_Format(Val_DC_Document_ExpiryDate, "yyMMdd", "ddMMyyyy"), FullTextOCR) >= 90) { Match_Expiredate = "Passed"; }
                                                            }
                                                            if (Val_DC_Document_Middlename.Trim() != "")
                                                            {
                                                                if (Val_DC_Document_Middlename.IndexOf(" ") >= 0)
                                                                {
                                                                    string[] MDLNSEP = Val_DC_Document_Middlename.Split(' ');
                                                                    int SumSim = 0;
                                                                    for (int i = 0; i < MDLNSEP.Count(); i++)
                                                                    {
                                                                        if (FullTextOCR.IndexOf(MDLNSEP[i]) > 0)
                                                                        {
                                                                            SumSim +=100;
                                                                        }
                                                                        else
                                                                        {
                                                                            SumSim += SF.GetWordsSimilarityInString(MDLNSEP[i], FullTextOCR);
                                                                        }
                                                                    }
                                                                    if ((SumSim / MDLNSEP.Count()) >= 90) { Match_Middlename = "Passed"; }
                                                                }
                                                                else
                                                                {
                                                                    if (FullTextOCR.IndexOf(Val_DC_Document_Middlename) > 0)
                                                                    {
                                                                        Match_Middlename = "Passed";
                                                                    }
                                                                    else
                                                                    {
                                                                        if (SF.GetWordsSimilarityInString(Val_DC_Document_Middlename, FullTextOCR) >= 90) { Match_Middlename = "Passed"; }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception) { }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','1','Length_Line1','" + Val_Line1_Length + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','2','Length_Line2','" + Val_Line2_Length + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','3','Document_Type','" + Val_Document_Type + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','4','Passport_Number_Checkdigit','" + Val_Passportnumber_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','5','Birthdate_Checkdigit','" + Val_Birthdate_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','6','Expiry_Date','" + Val_Expirydate_Expiry + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','7','Expiry_Date_Checkdigit','" + Val_Expirydate_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','8','MRZ_Checkdigit','" + Val_Document_SumCheck + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','9','MRZ_Check_Even','" + Val_Document_SumCheck_Iseven + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','10','Gender_Type','" + Val_Gender_Check + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','11','Personal_Number_Checkdigit','" + Val_Personal_Number_Check + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','12','Issuing_State','" + Val_IssuingState + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','13','Nationality','" + Val_NationalituCode + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','14','Similarity_First_Name','" + Match_Fistname + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','15','Similarity_Last_Name','" + Match_Lastname + "')");
                                                if (Val_DC_Document_Middlename.Trim() != "")
                                                {
                                                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','16','Similarity_Middle_Name','" + Match_Middlename + "')");
                                                }
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','17','Similarity_Passport_Number','" + Match_Passportnumber + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','18','Similarity_Birthdate','" + Match_Birthdate + "')");
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_23_API_Passport_Validation Values ('" + TransactionID + "','19','Similarity_Expirydate','" + Match_Expiredate + "')");
                                                int LastValidation_Code = 2;
                                                if (Val_Line1_Length.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Line2_Length.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_Type.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Passportnumber_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Birthdate_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Expirydate_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Expirydate_Expiry.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_SumCheck.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Document_SumCheck_Iseven.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Gender_Check.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_Personal_Number_Check.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_IssuingState.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_NationalituCode.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Match_Fistname.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Match_Lastname.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Val_DC_Document_Middlename.Trim() != "")
                                                {
                                                    if (Match_Middlename.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                }
                                                if (Match_Passportnumber.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Match_Birthdate.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (Match_Expiredate.Trim().ToLower() == "failed") { LastValidation_Code = 1; }
                                                if (MRZLN_1.Length != 44) { LastValidation_Code = 3; }
                                                if (MRZLN_2.Length != 44) { LastValidation_Code = 3; }
                                                if (MRZLN_1.Trim() == "") { LastValidation_Code = 3; }
                                                if (MRZLN_2.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Type.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_IssuingState.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Lastname.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Firstname.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_PassportNumber.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Number_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_NationalityCode.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_BirthDate.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_BirthDate_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Gender.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExpiryDate.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExpiryDate_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExtraInfo.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_ExtraInfo_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_Checksum.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_IssuingState.Trim() == "") { LastValidation_Code = 3; }
                                                if (Val_DC_Document_NationalityCode.Trim() == "") { LastValidation_Code = 3; }
                                                if (LastValidation_Code == 1) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','1','Failed')"); }
                                                if (LastValidation_Code == 2) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','2','Passed')"); }
                                                if (LastValidation_Code == 3) { SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_24_API_Passport_Validation_Result Values ('" + TransactionID + "','3','Refered')"); }
                                            }
                                        }
                                    }
                                }
                                //-------------------------------------------------------------------------------------------------
                            }
                            else
                            {   // DataTable Return Multy or Zero Row.
                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return ununic row application argument' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                            }
                        }
                        else
                        {   // DataTable Return Null Rows.
                            SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return null application argument' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                        }
                    }
                    else
                    {   // DataTable Return Multy or Zero Row.
                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return ununic row application' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                    }
                }
                else
                {   // DataTable Return Null Rows.
                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return null application' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                }
            }
            catch (Exception e)
            {   // Structure Error.
                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + TransactionID + "') And (Removed = '0')");
            }
        }
    }
    //-----------------------------------------------------------------------------------------------------
    //-----------------------------------------------------------------------------------------------------
}