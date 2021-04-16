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

namespace iCore_Administrator.API.Modules
{
    public class Word
    {
        public string WordText { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
    }

    public class Line
    {
        public string LineText { get; set; }
        public IList<Word> Words { get; set; }
        public double MaxHeight { get; set; }
        public double MinTop { get; set; }
    }

    public class TextOverlay
    {
        public IList<Line> Lines { get; set; }
        public bool HasOverlay { get; set; }
        public string Message { get; set; }
    }

    public class ParsedResult
    {
        public TextOverlay TextOverlay { get; set; }
        public string TextOrientation { get; set; }
        public int FileParseExitCode { get; set; }
        public string ParsedText { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetails { get; set; }
    }

    public class OCR_Result
    {
        public IList<ParsedResult> ParsedResults { get; set; }
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public string ProcessingTimeInMilliseconds { get; set; }
        public string SearchablePDFURL { get; set; }
    }

    public class TemplateList
    {
        public string TemplateID { get; set; }
        public double FrontImage_Key_Similarity { get; set; }
        public double FrontImage_ColorPicker_Similarity { get; set; }
        public double FrontImage_X_Coeficient { get; set; }
        public double FrontImage_Y_Coeficient { get; set; }
        public double FrontImage_X_TopLeft_Refrence { get; set; }
        public double FrontImage_Y_TopLeft_Refrence { get; set; }
        public double BackImage_Key_Similarity { get; set; }
        public double BackImage_ColorPicker_Similarity { get; set; }
        public double BackImage_X_Coeficient { get; set; }
        public double BackImage_Y_Coeficient { get; set; }
        public double BackImage_X_TopLeft_Refrence { get; set; }
        public double BackImage_Y_TopLeft_Refrence { get; set; }
        public double OCR_Line_Rank { get; set; }
        public double TemplateAverageRank { get; set; }
        public int TemplateAverageRank_Step2 { get; set; }
    }

    public class OCR_Relut_Elements
    {
        public string Element_Key { get; set; }
        public string Element_Value { get; set; }
    }

    public class IDV_OCR
    {
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        iCore_Administrator.Modules.SQL_Tranceiver SQ = new iCore_Administrator.Modules.SQL_Tranceiver();
        iCore_Administrator.Modules.PublicFunctions PB = new iCore_Administrator.Modules.PublicFunctions();
        SimilarityFunction SF = new SimilarityFunction();
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        int Rank_KeySimilarity_FI = 10;
        int Rank_ColorSimilarity_FI = 5;
        int Rank_KeySimilarity_BI = 8;
        int Rank_ColorSimilarity_BI = 4;
        int Rank_OCRFielad = 3;
        int Rank_TemplateAcceptedMinmumRank = 70;
        int NormalOcrLines_threshold = 20;
        bool FrontIMage_Rotated = false;
        bool BackIMage_Rotated = false;
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        // Barcode 128 :
        public string Barcode_128_Read(Bitmap BTM)
        {
            try
            {
                string[] datas = BarcodeScanner.Scan(BTM);
                return datas[0].ToString().Trim();
            }
            catch (Exception)
            {
                return "";
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        // QR Code :
        public string Barcode_QR_Read(Bitmap QRCodeInputImage)
        {
            try
            {
                QRDecoder QRCodeDecoder = new QRDecoder();
                byte[][] DataByteArray = QRCodeDecoder.ImageDecoder(QRCodeInputImage);
                return QRCodeResult(DataByteArray);
            }
            catch (Exception)
            {
                return "";
            }
        }
        private string QRCodeResult(byte[][] DataByteArray)
        {
            try
            {
                if (DataByteArray == null) return string.Empty;
                if (DataByteArray.Length == 1) return ForDisplay(QRDecoder.ByteArrayToStr(DataByteArray[0]));
                StringBuilder Str = new StringBuilder();
                for (int Index = 0; Index < DataByteArray.Length; Index++)
                {
                    if (Index != 0) Str.Append("\r\n");
                    Str.AppendFormat("QR Code {0}\r\n", Index + 1);
                    Str.Append(ForDisplay(QRDecoder.ByteArrayToStr(DataByteArray[Index])));
                }
                return Str.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
        private string ForDisplay(string Result)
        {
            try
            {
                int Index;
                for (Index = 0; Index < Result.Length && (Result[Index] >= ' ' && Result[Index] <= '~' || Result[Index] >= 160); Index++) ;
                if (Index == Result.Length) return Result;
                StringBuilder Display = new StringBuilder(Result.Substring(0, Index));
                for (; Index < Result.Length; Index++)
                {
                    char OneChar = Result[Index];
                    if (OneChar >= ' ' && OneChar <= '~' || OneChar >= 160)
                    {
                        Display.Append(OneChar);
                        continue;
                    }
                    if (OneChar == '\r')
                    {
                        Display.Append("\r\n");
                        if (Index + 1 < Result.Length && Result[Index + 1] == '\n') Index++;
                        continue;
                    }
                    if (OneChar == '\n')
                    {
                        Display.Append("\r\n");
                        continue;
                    }
                    Display.Append('¿');
                }
                return Display.ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        public bool FaceDetector(Image IMGL)
        {
            try
            {
                for (int i = 1; i < 5; i++)
                {
                    Bitmap IMG = null;
                    switch (i)
                    {
                        case 1: { IMG = new Bitmap(IMGL); break; }
                        case 2: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate90FlipNone); break; }
                        case 3: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate180FlipNone); break; }
                        case 4: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate270FlipNone); break; }
                    }
                    if (IMG.Width >= IMG.Height)
                    {
                        for (int j = 4; j < 9; j++)
                        {
                            try
                            {
                                HaarCascade cascade = new FaceHaarCascade();
                                var detector = new HaarObjectDetector(cascade, IMG.Width / j, ObjectDetectorSearchMode.Single);
                                Rectangle[] objects = detector.ProcessFrame(IMG);
                                if (objects.Length == 1)
                                {
                                    Bitmap IMG2 = new Bitmap(IMG);
                                    IMG2 = CropImage(IMG2, objects[0]);
                                    for (int k = 1; k < 4; k++)
                                    {
                                        try
                                        {
                                            HaarCascade cascade2 = new FaceHaarCascade();
                                            var detector2 = new HaarObjectDetector(cascade2, IMG2.Width / k, ObjectDetectorSearchMode.Single);
                                            Rectangle[] objects2 = detector2.ProcessFrame(IMG2);
                                            if (objects2.Length == 1)
                                            {
                                                return true;
                                            }
                                        }
                                        catch (Exception)
                                        { }
                                    }
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public Rectangle FaceDetector_FB(Image IMGL, ref Image FB_IMG)
        {
            try
            {
                for (int i = 1; i < 5; i++)
                {
                    Bitmap IMG = null;
                    switch (i)
                    {
                        case 1: { IMG = new Bitmap(IMGL); break; }
                        case 2: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate90FlipNone); break; }
                        case 3: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate180FlipNone); break; }
                        case 4: { IMG = new Bitmap(IMGL); IMG.RotateFlip(RotateFlipType.Rotate270FlipNone); break; }
                    }
                    if (IMG.Width >= IMG.Height)
                    {
                        for (int j = 4; j < 9; j++)
                        {
                            try
                            {
                                HaarCascade cascade = new FaceHaarCascade();
                                var detector = new HaarObjectDetector(cascade, IMG.Width / j, ObjectDetectorSearchMode.Single);
                                Rectangle[] objects = detector.ProcessFrame(IMG);
                                if (objects.Length == 1)
                                {
                                    Bitmap IMG2 = new Bitmap(IMG);
                                    IMG2 = CropImage(IMG2, objects[0]);
                                    for (int k = 1; k < 4; k++)
                                    {
                                        try
                                        {
                                            HaarCascade cascade2 = new FaceHaarCascade();
                                            var detector2 = new HaarObjectDetector(cascade2, IMG2.Width / k, ObjectDetectorSearchMode.Single);
                                            Rectangle[] objects2 = detector2.ProcessFrame(IMG2);
                                            if (objects2.Length == 1)
                                            {
                                                FB_IMG = IMG;
                                                return objects[0];
                                            }
                                        }
                                        catch (Exception)
                                        { }
                                    }
                                }
                            }
                            catch (Exception)
                            { }
                        }
                    }
                }
                return Rectangle.Empty;
            }
            catch (Exception)
            {
                return Rectangle.Empty;
            }
        }
        public static Bitmap CropImage(Image source, Rectangle crop)
        {
            int LX = crop.X;
            int LY = crop.Y;
            int LW = crop.Width;
            int LH = crop.Height;
            if ((LX - (LW / 3)) >= 0) { crop.X = crop.X - (LW / 3); crop.Width = crop.Width + (LW / 3); }
            if ((LX + LW + (LW / 3)) <= source.Width) { crop.Width = crop.Width + (LW / 3); } else { crop.Width = source.Width - crop.X; }
            if ((LY - (LH / 3)) >= 0) { crop.Y = crop.Y - (LH / 3); crop.Height = crop.Height + (LH / 3); }
            if ((LY + LH + (LH / 3)) <= source.Height) { crop.Height = crop.Height + (LH / 3); }
            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
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
        private string IDVOCR(Bitmap IMG, string FileType)
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
                        request.AddParameter("isOverlayRequired", DT.Rows[0][2].ToString().Trim());
                        request.AddParameter("detectOrientation", DT.Rows[0][3].ToString().Trim());
                        request.AddParameter("scale", DT.Rows[0][4].ToString().Trim());
                        request.AddParameter("isTable", DT.Rows[0][5].ToString().Trim());
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
        private void DataReady(ref List<OCR_Relut_Elements> ORE, string Key, string Value)
        {
            try
            {
                Key = Key.Trim();
                Value = Value.Trim();
                bool NeedAddNew = true;
                foreach (OCR_Relut_Elements OCRR in ORE)
                {
                    if (OCRR.Element_Key == Key)
                    {
                        OCRR.Element_Value = OCRR.Element_Value = OCRR.Element_Value.Trim() + " " + Value;
                        NeedAddNew = false;
                    }
                }
                if (NeedAddNew == true)
                {
                    ORE.Add(new OCR_Relut_Elements()
                    {
                        Element_Key = Key,
                        Element_Value = Value
                    });
                }
            }
            catch (Exception)
            { }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
        public void GetData(string TransactionID)
        {
            try
            {
                TransactionID = TransactionID.Trim();
                string TemplateSearchCode = "0";
                try
                {
                    DataTable DTRank = new DataTable();
                    DTRank = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 * From Setting_Basic_06_IDVOCR_Rank");
                    Rank_KeySimilarity_FI = int.Parse(DTRank.Rows[0][0].ToString().Trim());
                    Rank_ColorSimilarity_FI = int.Parse(DTRank.Rows[0][1].ToString().Trim());
                    Rank_KeySimilarity_BI = int.Parse(DTRank.Rows[0][2].ToString().Trim());
                    Rank_ColorSimilarity_BI = int.Parse(DTRank.Rows[0][3].ToString().Trim());
                    Rank_OCRFielad = int.Parse(DTRank.Rows[0][4].ToString().Trim());
                    Rank_TemplateAcceptedMinmumRank = int.Parse(DTRank.Rows[0][5].ToString().Trim());
                    NormalOcrLines_threshold = int.Parse(DTRank.Rows[0][6].ToString().Trim());
                }
                catch (Exception)
                {
                    Rank_KeySimilarity_FI = 10;
                    Rank_ColorSimilarity_FI = 5;
                    Rank_KeySimilarity_BI = 8;
                    Rank_ColorSimilarity_BI = 4;
                    Rank_OCRFielad = 3;
                    NormalOcrLines_threshold = 20;
                }
                DataTable DT_TemplateCode = new DataTable();
                DT_TemplateCode = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Template_DrivingLicence_Code From Setting_Basic_05_IDVOCR");
                if (DT_TemplateCode.Rows != null)
                {
                    if (DT_TemplateCode.Rows.Count == 1)
                    {
                        TemplateSearchCode = DT_TemplateCode.Rows[0][0].ToString().Trim();
                        DataTable DT_Tran = new DataTable();
                        DT_Tran = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (ID = '" + TransactionID + "') And (Removed = '0')");
                        if (DT_Tran.Rows != null)
                        {
                            if (DT_Tran.Rows.Count == 1)
                            {
                                string Tra_ID = DT_Tran.Rows[0][0].ToString().Trim();
                                string FrontImageName = DT_Tran.Rows[0][52].ToString().Trim() + "." + DT_Tran.Rows[0][50].ToString().Trim();
                                string BackImageName = DT_Tran.Rows[0][62].ToString().Trim() + "." + DT_Tran.Rows[0][60].ToString().Trim();
                                string FrontImageName_Format = DT_Tran.Rows[0][50].ToString().Trim();
                                string BackImageName_Format = DT_Tran.Rows[0][60].ToString().Trim();
                                // Images Variables :
                                bool IMG_Nor_Tag_Front = false; bool IMG_Nor_Tag_Back = false;
                                Image Img_Front_Nor = null; Image Img_Back_Nor = null;
                                // Prepare Images :
                                string BaseFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID);
                                if (Directory.Exists(BaseFolder) == false) { Directory.CreateDirectory(BaseFolder); }
                                if (Directory.Exists(BaseFolder + "\\" + "Upload") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Upload"); }
                                if (Directory.Exists(BaseFolder + "\\" + "Result") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Result"); }
                                var filePathfront = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Upload/" + FrontImageName);
                                var filePathback = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Upload/" + BackImageName);
                                if (File.Exists(filePathfront) == true) { IMG_Nor_Tag_Front = true; Img_Front_Nor = new Bitmap(filePathfront); }
                                if (File.Exists(filePathback) == true) { IMG_Nor_Tag_Back = true; Img_Back_Nor = new Bitmap(filePathback); }
                                // Test Application have Front or Back Image :
                                if ((IMG_Nor_Tag_Front == true) || (IMG_Nor_Tag_Back == true))
                                {
                                    bool IMG_Replacement = false;
                                    bool ContinueProcedure = false;
                                    bool FaceDetected = false;
                                    // Face Detection :
                                    if (IMG_Nor_Tag_Front == true)
                                    {
                                        if (IMG_Nor_Tag_Back == true)
                                        {
                                            if (FaceDetector(Img_Front_Nor) == true)
                                            {
                                                IMG_Replacement = false;
                                                ContinueProcedure = true;
                                                FaceDetected = true;
                                            }
                                            else
                                            {
                                                if (FaceDetector(Img_Back_Nor) == true)
                                                {
                                                    IMG_Replacement = true;
                                                    ContinueProcedure = true;
                                                    FaceDetected = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (FaceDetector(Img_Front_Nor) == true) { IMG_Replacement = false; ContinueProcedure = true; FaceDetected = true; }
                                        }
                                    }
                                    else
                                    {
                                        if (FaceDetector(Img_Back_Nor) == true) { IMG_Replacement = true; FaceDetected = true; }
                                        ContinueProcedure = true;
                                    }
                                    if (ContinueProcedure == true)
                                    {
                                        // Images Replacement :
                                        if (IMG_Replacement == true)
                                        {
                                            IMG_Nor_Tag_Front = false; IMG_Nor_Tag_Back = false;
                                            Img_Front_Nor = null; Img_Back_Nor = null;
                                            if (File.Exists(filePathback) == true) { IMG_Nor_Tag_Front = true; Img_Front_Nor = new Bitmap(filePathback); FrontImageName_Format = DT_Tran.Rows[0][60].ToString().Trim(); }
                                            if (File.Exists(filePathfront) == true) { IMG_Nor_Tag_Back = true; Img_Back_Nor = new Bitmap(filePathfront); BackImageName_Format = DT_Tran.Rows[0][50].ToString().Trim(); }
                                        }
                                        // Select OCR Image :
                                        string Sel_IMG_Format = "JPG";
                                        bool Selected_IMG_Front = true;
                                        Image Selected_IMG = null;
                                        if (IMG_Nor_Tag_Front == true)
                                        {
                                            if (IMG_Nor_Tag_Back == true)
                                            {
                                                if (FaceDetected == true)
                                                {
                                                    Selected_IMG_Front = true;
                                                    Selected_IMG = Img_Front_Nor;
                                                    Sel_IMG_Format = FrontImageName_Format;
                                                }
                                                else
                                                {
                                                    Selected_IMG_Front = false;
                                                    Selected_IMG = Img_Back_Nor;
                                                    Sel_IMG_Format = BackImageName_Format;
                                                }
                                            }
                                            else
                                            {
                                                Selected_IMG_Front = true;
                                                Selected_IMG = Img_Front_Nor;
                                                Sel_IMG_Format = FrontImageName_Format;
                                            }
                                        }
                                        else
                                        {
                                            Selected_IMG_Front = false;
                                            Selected_IMG = Img_Back_Nor;
                                            Sel_IMG_Format = BackImageName_Format;
                                        }
                                        string OCRResultLocal = IDVOCR((Bitmap)Selected_IMG, Sel_IMG_Format);
                                        OCR_Result OCRRes = JsonConvert.DeserializeObject<OCR_Result>(OCRResultLocal);
                                        OCR_Result OCRRes_Back = new OCR_Result();
                                        OCR_Result OCR_Last_Front = null;
                                        OCR_Result OCR_Last_Back = null;
                                        FrontIMage_Rotated = false;
                                        BackIMage_Rotated = false;
                                        if (OCRRes.OCRExitCode == 1)
                                        {
                                            if (OCRRes.IsErroredOnProcessing == false)
                                            {
                                                if (OCRRes.ParsedResults[0].ErrorMessage.Trim() == "")
                                                {
                                                    bool TemplateFounded = false;
                                                    TemplateList LST_Template = new TemplateList();
                                                    if (Selected_IMG_Front == true)
                                                    {
                                                        //=============================================================================================================================================================
                                                        //======================================================================== Front Image ========================================================================
                                                        //=============================================================================================================================================================
                                                        string OCR_TextResult = "";
                                                        string OCR_TextOrientation = "0";
                                                        string OCR_TextResult_Back = "";
                                                        string OCR_TextOrientation_Back = "0";
                                                        OCR_TextResult = OCRRes.ParsedResults[0].ParsedText.Trim();
                                                        OCR_TextResult = OCR_TextResult.Replace("\r", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("\n", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("\t", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("  ", " ").Replace("  ", " "); ;
                                                        OCR_TextOrientation = OCRRes.ParsedResults[0].TextOrientation.Trim();
                                                        OCR_Last_Front = OCRRes;
                                                        if (OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim() != "0")
                                                        {
                                                            Img_Front_Nor = ImageRotation((Bitmap)Img_Front_Nor, OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim());
                                                            FrontIMage_Rotated = true;
                                                        }
                                                        // Searching for Template Start as Front Image :
                                                        DataTable DT_FIE_All = new DataTable();
                                                        DataTable DT_FIEK = new DataTable();
                                                        DT_FIE_All = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select DID,BackImage,OnlyBackImageBarcode,ONOCR,LNOCR,MNOCR,FrontImage From Template_06_BasicConfiguration Where (DTID = '" + TemplateSearchCode + "') And (FrontImage = '1') Order By DID");
                                                        if (DT_FIE_All.Rows != null)
                                                        {
                                                            if (DT_FIE_All.Rows.Count > 0)
                                                            {
                                                                double[,] FIE_TemplateCode = new double[DT_FIE_All.Rows.Count, 21];
                                                                int FIE_TemplateCode_Row = 0;
                                                                foreach (DataRow RW1 in DT_FIE_All.Rows)
                                                                {
                                                                    FIE_TemplateCode_Row++;
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 0] = int.Parse(RW1[0].ToString().Trim()); //Document ID ------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] = 0; // Front Image Key Similarity Average ----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 2] = 0; // Front Image Color Picker Similarity Average -------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] = 0; // Back Image Key Similarity Average -----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 4] = 0; // Back Image Color Picker Similarity Average --------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 5] = 0; // OCR Line Number Average Rank ----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6] = 0; // X Coefficient Front Image -------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7] = 0; // Y Coefficient Front Image -------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8] = 0; // Scan X Top-Left Front Image -----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9] = 0; // Scan Y Top-Left Front Image -----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10] = 0; // X Coefficient Back Image -------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11] = 0; // Y Coefficient Back Image -------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12] = 0; // Base X Top-Left Back Image -----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13] = 0; // Base Y Top-Left Back Image -----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] = 0; // Founded Front Image ------------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 15] = 0; // Founded Front Image Color Picker -----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] = 0; // Founded Back Image -------------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 17] = 0; // Founded Back Image Color Picker ------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 18] = 0; // Calculate Front Image ----------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 19] = 0; // Calculate Back Image -----------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 20] = 0; // Template Last Rank -------------------------------> OK
                                                                    // Search For Key Front Image :
                                                                    DT_FIEK = new DataTable();
                                                                    DT_FIEK = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select KeyCode,Similarity,X1,Y1,Y2,KeyIndex,KeyPosition From Template_08_FrontImage_Elements Where (DID = '" + RW1[0].ToString().Trim() + "') And (KeyActive = '1') And (KeyCode <> '') Order By Y1,X1");
                                                                    if (DT_FIEK.Rows != null)
                                                                    {
                                                                        if (DT_FIEK.Rows.Count > 0)
                                                                        {
                                                                            int[,] FIE_TemplateKey = new int[DT_FIEK.Rows.Count, 2];
                                                                            int FIE_TemplateKey_Row = 0;
                                                                            foreach (DataRow RW2 in DT_FIEK.Rows)
                                                                            {
                                                                                FIE_TemplateKey_Row++;
                                                                                FIE_TemplateKey[FIE_TemplateKey_Row - 1, 0] = SF.GetWordsSimilarityInString(RW2[0].ToString().Trim(), OCR_TextResult);
                                                                                FIE_TemplateKey[FIE_TemplateKey_Row - 1, 1] = int.Parse(RW2[1].ToString().Trim());
                                                                            }
                                                                            double LastRate_Cal = 0;
                                                                            double LastRate_Master = 0;
                                                                            for (int i = 0; i < FIE_TemplateKey_Row; i++)
                                                                            {
                                                                                LastRate_Cal += FIE_TemplateKey[i, 0];
                                                                                LastRate_Master += FIE_TemplateKey[i, 1];
                                                                            }
                                                                            LastRate_Cal /= FIE_TemplateKey_Row;
                                                                            LastRate_Master /= FIE_TemplateKey_Row;
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] = (int)LastRate_Cal;
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] = 1;
                                                                        }
                                                                    }
                                                                    if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] == 1)
                                                                    {
                                                                        // Get Front Image Scale :
                                                                        double FI_X_Coe = 0;
                                                                        double FI_Y_Coe = 0;
                                                                        bool ProContinue = false;
                                                                        if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] > 0)
                                                                        {
                                                                            int M_X1 = 0; int M_Y1 = 0; int M_X2 = 0; int M_Y2 = 0;
                                                                            int S_X1 = 0; int S_Y1 = 0; int S_X2 = 0; int S_Y2 = 0;
                                                                            M_X1 = int.Parse(DT_FIEK.Rows[0][2].ToString().Trim());
                                                                            M_Y1 = int.Parse(DT_FIEK.Rows[0][3].ToString().Trim());
                                                                            M_X2 = int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][2].ToString().Trim());
                                                                            M_Y2 = int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][3].ToString().Trim());
                                                                            if (SF.KeyDetector(ref S_X1, ref S_Y1, DT_FIEK.Rows[0][0].ToString().Trim(), OCRRes.ParsedResults[0].TextOverlay.Lines, DT_FIEK.Rows[0][1].ToString().Trim(), int.Parse(DT_FIEK.Rows[0][5].ToString().Trim()), DT_FIEK.Rows[0][6].ToString().Trim()) == true)
                                                                            {
                                                                                if (SF.KeyDetector(ref S_X2, ref S_Y2, DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][0].ToString().Trim(), OCRRes.ParsedResults[0].TextOverlay.Lines, DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][1].ToString().Trim(), int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][5].ToString().Trim()), DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][6].ToString().Trim()) == true)
                                                                                {
                                                                                    int XSM = Math.Abs(S_X1 - S_X2);
                                                                                    int XMM = Math.Abs(M_X1 - M_X2);
                                                                                    if (XSM == 0) { XSM = 1; }
                                                                                    if (XMM == 0) { XMM = 1; }
                                                                                    FI_X_Coe = (double)((double)XMM / (double)XSM);
                                                                                    int YSM = Math.Abs(S_Y1 - S_Y2);
                                                                                    int YMM = Math.Abs(M_Y1 - M_Y2);
                                                                                    if (YSM == 0) { YSM = 1; }
                                                                                    if (YMM == 0) { YMM = 1; }
                                                                                    FI_Y_Coe = (double)((double)YMM / (double)YSM);
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6] = FI_X_Coe; // X Coefficient Front Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7] = FI_Y_Coe; // Y Coefficient Front Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8] = S_X1 - (M_X1 / FI_X_Coe); // Scan X Top-Left Front Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9] = S_Y1 - (M_Y1 / FI_Y_Coe); ; // Scan Y Top-Left Front Image
                                                                                    ProContinue = true;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (ProContinue == true)
                                                                        {
                                                                            // Front Image Color Picker :
                                                                            DataTable DT_FICP = new DataTable();
                                                                            DT_FICP = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select X,Y,R,G,B,Similarity From Template_10_FrontImage_ColorPicker Where (DID = '" + RW1[0].ToString().Trim() + "') Order By X,Y");
                                                                            if (DT_FICP.Rows != null)
                                                                            {
                                                                                if (DT_FICP.Rows.Count > 0)
                                                                                {
                                                                                    double IMG_IP = 0;
                                                                                    double IMG_BP = 0;
                                                                                    foreach (DataRow RW2 in DT_FICP.Rows)
                                                                                    {
                                                                                        int CPX = (int)((int.Parse(RW2[0].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8]);
                                                                                        int CPY = (int)((int.Parse(RW2[1].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9]);
                                                                                        Color CLS = Color.Black;
                                                                                        try
                                                                                        {
                                                                                            CLS = ((Bitmap)Img_Front_Nor).GetPixel(CPX, CPY);
                                                                                        }
                                                                                        catch (Exception)
                                                                                        { }
                                                                                        IMG_IP += SF.GetColorSimilarity(CLS, Color.FromArgb(int.Parse(RW2[2].ToString().Trim()), int.Parse(RW2[3].ToString().Trim()), int.Parse(RW2[4].ToString().Trim())));
                                                                                        IMG_BP += int.Parse(RW2[5].ToString().Trim());
                                                                                    }
                                                                                    IMG_IP /= DT_FICP.Rows.Count;
                                                                                    IMG_BP /= DT_FICP.Rows.Count;
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 2] = (int)IMG_IP;
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 15] = 1;
                                                                                }
                                                                            }
                                                                            // Back Image Functionality :
                                                                            if (IMG_Nor_Tag_Back == true)
                                                                            {
                                                                                if ((RW1[1].ToString().Trim() == "1") && ((RW1[2].ToString().Trim() == "0")))
                                                                                {
                                                                                    // Get OCR Result Of Back Image :
                                                                                    if (OCRRes_Back.ParsedResults == null)
                                                                                    {
                                                                                        string OCRResultLocalBack = IDVOCR((Bitmap)Img_Back_Nor, BackImageName_Format);
                                                                                        OCRRes_Back = JsonConvert.DeserializeObject<OCR_Result>(OCRResultLocalBack);
                                                                                        OCR_TextResult_Back = OCRRes_Back.ParsedResults[0].ParsedText.Trim();
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\r", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\n", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\t", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("  ", " ").Replace("  ", " "); ;
                                                                                        OCR_TextOrientation_Back = OCRRes_Back.ParsedResults[0].TextOrientation.Trim();
                                                                                        OCR_Last_Back = OCRRes_Back;
                                                                                        if (OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim() != "0")
                                                                                        {
                                                                                            Img_Back_Nor = ImageRotation((Bitmap)Img_Back_Nor, OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim());
                                                                                            BackIMage_Rotated = true;
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        OCR_Last_Back = OCRRes_Back;
                                                                                    }
                                                                                    // Search For Key Back Image :
                                                                                    DataTable DT_FBEK = new DataTable();
                                                                                    DT_FBEK = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select KeyCode,Similarity,X1,Y1,Y2,KeyIndex,KeyPosition From Template_09_BackImage_Elements Where (DID = '" + RW1[0].ToString().Trim() + "') And (KeyActive = '1') And (KeyCode <> '') Order By Y1,X1");
                                                                                    if (DT_FBEK.Rows != null)
                                                                                    {
                                                                                        if (DT_FBEK.Rows.Count > 0)
                                                                                        {
                                                                                            int[,] BIE_TemplateKey = new int[DT_FBEK.Rows.Count, 2];
                                                                                            int BIE_TemplateKey_Row = 0;
                                                                                            foreach (DataRow RW2 in DT_FBEK.Rows)
                                                                                            {
                                                                                                BIE_TemplateKey_Row++;
                                                                                                BIE_TemplateKey[BIE_TemplateKey_Row - 1, 0] = SF.GetWordsSimilarityInString(RW2[0].ToString().Trim(), OCR_TextResult_Back);
                                                                                                BIE_TemplateKey[BIE_TemplateKey_Row - 1, 1] = int.Parse(RW2[1].ToString().Trim());
                                                                                            }
                                                                                            double LastRate_Cal = 0;
                                                                                            double LastRate_Master = 0;
                                                                                            for (int i = 0; i < BIE_TemplateKey_Row; i++)
                                                                                            {
                                                                                                LastRate_Cal += BIE_TemplateKey[i, 0];
                                                                                                LastRate_Master += BIE_TemplateKey[i, 1];
                                                                                            }
                                                                                            LastRate_Cal /= BIE_TemplateKey_Row;
                                                                                            LastRate_Master /= BIE_TemplateKey_Row;
                                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] = (int)LastRate_Cal;
                                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] = 1;
                                                                                        }
                                                                                    }
                                                                                    if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] == 1)
                                                                                    {
                                                                                        // Get Back Image Scale :
                                                                                        double FI_X_CoeB = 0;
                                                                                        double FI_Y_CoeB = 0;
                                                                                        bool ProContinueB = false;
                                                                                        if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] > 0)
                                                                                        {
                                                                                            int M_X1 = 0; int M_Y1 = 0; int M_X2 = 0; int M_Y2 = 0;
                                                                                            int S_X1 = 0; int S_Y1 = 0; int S_X2 = 0; int S_Y2 = 0;
                                                                                            M_X1 = int.Parse(DT_FBEK.Rows[0][2].ToString().Trim());
                                                                                            M_Y1 = (int)((int.Parse(DT_FBEK.Rows[0][3].ToString().Trim()) + int.Parse(DT_FBEK.Rows[0][4].ToString().Trim())) / 2);
                                                                                            M_X2 = int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][2].ToString().Trim());
                                                                                            M_Y2 = (int)((int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][3].ToString().Trim()) + int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][4].ToString().Trim())) / 2);
                                                                                            if (SF.KeyDetector(ref S_X1, ref S_Y1, DT_FBEK.Rows[0][0].ToString().Trim(), OCRRes_Back.ParsedResults[0].TextOverlay.Lines, DT_FBEK.Rows[0][1].ToString().Trim(), int.Parse(DT_FBEK.Rows[0][5].ToString().Trim()), DT_FBEK.Rows[0][6].ToString().Trim()) == true)
                                                                                            {
                                                                                                if (SF.KeyDetector(ref S_X2, ref S_Y2, DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][0].ToString().Trim(), OCRRes_Back.ParsedResults[0].TextOverlay.Lines, DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][1].ToString().Trim(), int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][5].ToString().Trim()), DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][6].ToString().Trim()) == true)
                                                                                                {
                                                                                                    int XSM = Math.Abs(S_X1 - S_X2);
                                                                                                    int XMM = Math.Abs(M_X1 - M_X2);
                                                                                                    if (XSM == 0) { XSM = 1; }
                                                                                                    if (XMM == 0) { XMM = 1; }
                                                                                                    FI_X_CoeB = (double)((double)XMM / (double)XSM);
                                                                                                    int YSM = Math.Abs(S_Y1 - S_Y2);
                                                                                                    int YMM = Math.Abs(M_Y1 - M_Y2);
                                                                                                    if (YSM == 0) { YSM = 1; }
                                                                                                    if (YMM == 0) { YMM = 1; }
                                                                                                    FI_Y_CoeB = (double)((double)YMM / (double)YSM);
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10] = FI_X_CoeB; // X Coefficient Back Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11] = FI_Y_CoeB; // Y Coefficient Back Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12] = S_X1 - (M_X1 / FI_X_CoeB); // Scan X Top-Left Back Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13] = S_Y1 - (M_Y1 / FI_Y_CoeB); ; // Scan Y Top-Left Back Image
                                                                                                    ProContinueB = true;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        if (ProContinueB == true)
                                                                                        {
                                                                                            // Back Image Color Picker :
                                                                                            DataTable DT_FICPB = new DataTable();
                                                                                            DT_FICPB = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select X,Y,R,G,B,Similarity From Template_11_BackImage_ColorPicker Where (DID = '" + RW1[0].ToString().Trim() + "') Order By X,Y");
                                                                                            if (DT_FICPB.Rows != null)
                                                                                            {
                                                                                                if (DT_FICPB.Rows.Count > 0)
                                                                                                {
                                                                                                    double IMG_IP = 0;
                                                                                                    double IMG_BP = 0;
                                                                                                    foreach (DataRow RW2 in DT_FICPB.Rows)
                                                                                                    {
                                                                                                        int CPX = (int)((int.Parse(RW2[0].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12]);
                                                                                                        int CPY = (int)((int.Parse(RW2[1].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13]);
                                                                                                        Color CLS = Color.Black;
                                                                                                        try
                                                                                                        {
                                                                                                            CLS = ((Bitmap)Img_Back_Nor).GetPixel(CPX, CPY);
                                                                                                        }
                                                                                                        catch (Exception)
                                                                                                        { }
                                                                                                        IMG_IP += SF.GetColorSimilarity(CLS, Color.FromArgb(int.Parse(RW2[2].ToString().Trim()), int.Parse(RW2[3].ToString().Trim()), int.Parse(RW2[4].ToString().Trim())));
                                                                                                        IMG_BP += int.Parse(RW2[5].ToString().Trim());
                                                                                                    }
                                                                                                    IMG_IP /= DT_FICPB.Rows.Count;
                                                                                                    IMG_BP /= DT_FICPB.Rows.Count;
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 4] = (int)IMG_IP;
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 17] = 1;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            // Get Optimal , Maximum , Minimum OCR Fields Count :
                                                                            int LastCounterPercent = 0;
                                                                            int OCRNorC = int.Parse(RW1[3].ToString().Trim());
                                                                            int OCRMinC = int.Parse(RW1[4].ToString().Trim());
                                                                            int OCRMaxC = int.Parse(RW1[5].ToString().Trim());
                                                                            int LineCount = OCRRes.ParsedResults[0].TextOverlay.Lines.Count;
                                                                            if (LineCount >= OCRMinC) { LastCounterPercent += 25; }
                                                                            if (LineCount <= OCRMaxC) { LastCounterPercent += 25; }
                                                                            if ((LineCount >= (OCRNorC - (int)((OCRNorC * NormalOcrLines_threshold) / 100))) && (LineCount <= (OCRNorC + (int)((OCRNorC * NormalOcrLines_threshold) / 100)))) { LastCounterPercent += 50; }
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 5] = (int)LastCounterPercent;
                                                                            // Get Front / Back Image Calculation :
                                                                            if (RW1[1].ToString().Trim() == "1") { FIE_TemplateCode[FIE_TemplateCode_Row - 1, 19] = 1; }
                                                                            if (RW1[6].ToString().Trim() == "1") { FIE_TemplateCode[FIE_TemplateCode_Row - 1, 18] = 1; }
                                                                        }
                                                                    }
                                                                }
                                                                // Select Template With Maximum Rank
                                                                if ((FIE_TemplateCode.Length / 21) > 0)
                                                                {
                                                                    int Divider_Count = 0;
                                                                    double SumRank = 0;
                                                                    // Calculate Template Last Rank
                                                                    for (int i = 0; i < (FIE_TemplateCode.Length / 21); i++)
                                                                    {
                                                                        Divider_Count = 0;
                                                                        SumRank = 0;
                                                                        FIE_TemplateCode[i, 20] = 0;
                                                                        // Front Image Rank 
                                                                        if (FIE_TemplateCode[i, 18] == 1)
                                                                        {
                                                                            // Key Similarity
                                                                            if (FIE_TemplateCode[i, 14] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 1] * Rank_KeySimilarity_FI;
                                                                                Divider_Count += Rank_KeySimilarity_FI;
                                                                            }
                                                                            // Color Picker Similarity 
                                                                            if (FIE_TemplateCode[i, 15] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 2] * Rank_ColorSimilarity_FI;
                                                                                Divider_Count += Rank_ColorSimilarity_FI;
                                                                            }
                                                                        }
                                                                        // Back Image Rank 
                                                                        if (FIE_TemplateCode[i, 19] == 1)
                                                                        {
                                                                            // Key Similarity
                                                                            if (FIE_TemplateCode[i, 16] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 3] * Rank_KeySimilarity_BI;
                                                                                Divider_Count += Rank_KeySimilarity_BI;
                                                                            }
                                                                            // Color Picker Similarity 
                                                                            if (FIE_TemplateCode[i, 17] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 4] * Rank_ColorSimilarity_BI;
                                                                                Divider_Count += Rank_ColorSimilarity_BI;
                                                                            }
                                                                        }
                                                                        // OCR Line Number Rank
                                                                        SumRank += FIE_TemplateCode[i, 5] * Rank_OCRFielad;
                                                                        Divider_Count += Rank_OCRFielad;
                                                                        // Calculate Average
                                                                        try
                                                                        {
                                                                            FIE_TemplateCode[i, 20] = (int)Math.Round(SumRank / Divider_Count);
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            FIE_TemplateCode[i, 20] = 0;
                                                                        }
                                                                    }
                                                                    // Select Template With Maximum Rank :
                                                                    var TemList = new List<TemplateList>();
                                                                    TemList.Clear();
                                                                    double MaxTemplateRank = 0;
                                                                    for (int i = 0; i < (FIE_TemplateCode.Length / 21); i++)
                                                                    {
                                                                        if (FIE_TemplateCode[i, 20] > MaxTemplateRank)
                                                                        {
                                                                            TemList.Clear();
                                                                            MaxTemplateRank = FIE_TemplateCode[i, 20];
                                                                            TemList.Add(new TemplateList()
                                                                            {
                                                                                TemplateID = FIE_TemplateCode[i, 0].ToString(),
                                                                                FrontImage_Key_Similarity = FIE_TemplateCode[i, 1],
                                                                                FrontImage_ColorPicker_Similarity = FIE_TemplateCode[i, 2],
                                                                                FrontImage_X_Coeficient = FIE_TemplateCode[i, 6],
                                                                                FrontImage_Y_Coeficient = FIE_TemplateCode[i, 7],
                                                                                FrontImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 8],
                                                                                FrontImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 9],
                                                                                BackImage_Key_Similarity = FIE_TemplateCode[i, 3],
                                                                                BackImage_ColorPicker_Similarity = FIE_TemplateCode[i, 4],
                                                                                BackImage_X_Coeficient = FIE_TemplateCode[i, 10],
                                                                                BackImage_Y_Coeficient = FIE_TemplateCode[i, 11],
                                                                                BackImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 12],
                                                                                BackImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 13],
                                                                                OCR_Line_Rank = FIE_TemplateCode[i, 5],
                                                                                TemplateAverageRank = FIE_TemplateCode[i, 20],
                                                                                TemplateAverageRank_Step2 = 0
                                                                            });
                                                                        }
                                                                        else
                                                                        {
                                                                            if (FIE_TemplateCode[i, 20] == MaxTemplateRank)
                                                                            {
                                                                                MaxTemplateRank = FIE_TemplateCode[i, 20];
                                                                                TemList.Add(new TemplateList()
                                                                                {
                                                                                    TemplateID = FIE_TemplateCode[i, 0].ToString(),
                                                                                    FrontImage_Key_Similarity = FIE_TemplateCode[i, 1],
                                                                                    FrontImage_ColorPicker_Similarity = FIE_TemplateCode[i, 2],
                                                                                    FrontImage_X_Coeficient = FIE_TemplateCode[i, 6],
                                                                                    FrontImage_Y_Coeficient = FIE_TemplateCode[i, 7],
                                                                                    FrontImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 8],
                                                                                    FrontImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 9],
                                                                                    BackImage_Key_Similarity = FIE_TemplateCode[i, 3],
                                                                                    BackImage_ColorPicker_Similarity = FIE_TemplateCode[i, 4],
                                                                                    BackImage_X_Coeficient = FIE_TemplateCode[i, 10],
                                                                                    BackImage_Y_Coeficient = FIE_TemplateCode[i, 11],
                                                                                    BackImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 12],
                                                                                    BackImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 13],
                                                                                    OCR_Line_Rank = FIE_TemplateCode[i, 5],
                                                                                    TemplateAverageRank = FIE_TemplateCode[i, 20],
                                                                                    TemplateAverageRank_Step2 = 0
                                                                                });
                                                                            }
                                                                        }
                                                                    }
                                                                    if (TemList.Count == 1)
                                                                    {
                                                                        LST_Template.TemplateID = TemList[0].TemplateID;
                                                                        LST_Template.FrontImage_Key_Similarity = TemList[0].FrontImage_Key_Similarity;
                                                                        LST_Template.FrontImage_ColorPicker_Similarity = TemList[0].FrontImage_ColorPicker_Similarity;
                                                                        LST_Template.FrontImage_X_Coeficient = TemList[0].FrontImage_X_Coeficient;
                                                                        LST_Template.FrontImage_Y_Coeficient = TemList[0].FrontImage_Y_Coeficient;
                                                                        LST_Template.FrontImage_X_TopLeft_Refrence = TemList[0].FrontImage_X_TopLeft_Refrence;
                                                                        LST_Template.FrontImage_Y_TopLeft_Refrence = TemList[0].FrontImage_Y_TopLeft_Refrence;
                                                                        LST_Template.BackImage_Key_Similarity = TemList[0].BackImage_Key_Similarity;
                                                                        LST_Template.BackImage_ColorPicker_Similarity = TemList[0].BackImage_ColorPicker_Similarity;
                                                                        LST_Template.BackImage_X_Coeficient = TemList[0].BackImage_X_Coeficient;
                                                                        LST_Template.BackImage_Y_Coeficient = TemList[0].BackImage_Y_Coeficient;
                                                                        LST_Template.BackImage_X_TopLeft_Refrence = TemList[0].BackImage_X_TopLeft_Refrence;
                                                                        LST_Template.BackImage_Y_TopLeft_Refrence = TemList[0].BackImage_Y_TopLeft_Refrence;
                                                                        LST_Template.OCR_Line_Rank = TemList[0].OCR_Line_Rank;
                                                                        LST_Template.TemplateAverageRank = TemList[0].TemplateAverageRank;
                                                                        TemplateFounded = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            foreach (TemplateList TL2 in TemList)
                                                                            {
                                                                                if (TL.TemplateID != TL2.TemplateID)
                                                                                {
                                                                                    if (TL.TemplateAverageRank >= TL2.TemplateAverageRank)
                                                                                    {
                                                                                        if (TL.TemplateAverageRank == TL2.TemplateAverageRank)
                                                                                        {
                                                                                            if (TL.FrontImage_Key_Similarity >= TL2.FrontImage_Key_Similarity)
                                                                                            {
                                                                                                if (TL.FrontImage_Key_Similarity == TL2.FrontImage_Key_Similarity)
                                                                                                {
                                                                                                    if (TL.FrontImage_ColorPicker_Similarity >= TL2.FrontImage_ColorPicker_Similarity)
                                                                                                    {
                                                                                                        if (TL.FrontImage_ColorPicker_Similarity == TL2.FrontImage_ColorPicker_Similarity)
                                                                                                        {
                                                                                                            if (TL.BackImage_Key_Similarity >= TL2.BackImage_Key_Similarity)
                                                                                                            {
                                                                                                                if (TL.BackImage_Key_Similarity == TL2.BackImage_Key_Similarity)
                                                                                                                {
                                                                                                                    if (TL.BackImage_ColorPicker_Similarity >= TL2.BackImage_ColorPicker_Similarity)
                                                                                                                    {
                                                                                                                        if (TL.BackImage_ColorPicker_Similarity == TL2.BackImage_ColorPicker_Similarity)
                                                                                                                        {
                                                                                                                            if (TL.OCR_Line_Rank >= TL2.OCR_Line_Rank)
                                                                                                                            {
                                                                                                                                if (TL.OCR_Line_Rank == TL2.OCR_Line_Rank)
                                                                                                                                {
                                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                                    TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
                                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                                }
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                                                        }
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                    }
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                }
                                                                                                            }
                                                                                                            else
                                                                                                            {
                                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                                            }
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                }
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                            }
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        double MaxRank2 = 0;
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            if (TL.TemplateAverageRank_Step2 > MaxRank2)
                                                                            {
                                                                                LST_Template.TemplateID = TL.TemplateID;
                                                                                LST_Template.FrontImage_Key_Similarity = TL.FrontImage_Key_Similarity;
                                                                                LST_Template.FrontImage_ColorPicker_Similarity = TL.FrontImage_ColorPicker_Similarity;
                                                                                LST_Template.FrontImage_X_Coeficient = TL.FrontImage_X_Coeficient;
                                                                                LST_Template.FrontImage_Y_Coeficient = TL.FrontImage_Y_Coeficient;
                                                                                LST_Template.FrontImage_X_TopLeft_Refrence = TL.FrontImage_X_TopLeft_Refrence;
                                                                                LST_Template.FrontImage_Y_TopLeft_Refrence = TL.FrontImage_Y_TopLeft_Refrence;
                                                                                LST_Template.BackImage_Key_Similarity = TL.BackImage_Key_Similarity;
                                                                                LST_Template.BackImage_ColorPicker_Similarity = TL.BackImage_ColorPicker_Similarity;
                                                                                LST_Template.BackImage_X_Coeficient = TL.BackImage_X_Coeficient;
                                                                                LST_Template.BackImage_Y_Coeficient = TL.BackImage_Y_Coeficient;
                                                                                LST_Template.BackImage_X_TopLeft_Refrence = TL.BackImage_X_TopLeft_Refrence;
                                                                                LST_Template.BackImage_Y_TopLeft_Refrence = TL.BackImage_Y_TopLeft_Refrence;
                                                                                LST_Template.OCR_Line_Rank = TL.OCR_Line_Rank;
                                                                                LST_Template.TemplateAverageRank = TL.TemplateAverageRank;
                                                                                LST_Template.TemplateAverageRank_Step2 = TL.TemplateAverageRank_Step2;
                                                                            }
                                                                        }
                                                                        TemplateFounded = true;
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            if (LST_Template.TemplateID != TL.TemplateID)
                                                                            {
                                                                                if (LST_Template.TemplateAverageRank_Step2 == TL.TemplateAverageRank_Step2)
                                                                                {
                                                                                    TemplateFounded = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    TemplateFounded = false;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                TemplateFounded = false;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            TemplateFounded = false;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        //============================================================================================================================================================
                                                        //======================================================================== Back Image ========================================================================
                                                        //============================================================================================================================================================
                                                        string OCR_TextResult = "";
                                                        string OCR_TextOrientation = "0";
                                                        string OCR_TextResult_Back = "";
                                                        string OCR_TextOrientation_Back = "0";
                                                        OCR_TextResult = OCRRes.ParsedResults[0].ParsedText.Trim();
                                                        OCR_TextResult = OCR_TextResult.Replace("\r", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("\n", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("\t", " ");
                                                        OCR_TextResult = OCR_TextResult.Replace("  ", " ").Replace("  ", " "); ;
                                                        OCR_TextOrientation = OCRRes.ParsedResults[0].TextOrientation.Trim();
                                                        OCR_Last_Back = OCRRes;
                                                        if (OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim() != "0")
                                                        {
                                                            Img_Back_Nor = ImageRotation((Bitmap)Img_Back_Nor, OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim());
                                                            BackIMage_Rotated = true;
                                                        }
                                                        // Searching for Template Only Back Image :
                                                        DataTable DT_FIE_All = new DataTable();
                                                        DataTable DT_FIEK = new DataTable();
                                                        DT_FIE_All = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select DID,FrontImage,OnlyBackImageBarcode,ONOCR,LNOCR,MNOCR,BackImage From Template_06_BasicConfiguration Where (DTID = '" + TemplateSearchCode + "') And (BackImage = '1') Order By DID");
                                                        if (DT_FIE_All.Rows != null)
                                                        {
                                                            if (DT_FIE_All.Rows.Count > 0)
                                                            {
                                                                double[,] FIE_TemplateCode = new double[DT_FIE_All.Rows.Count, 21];
                                                                int FIE_TemplateCode_Row = 0;
                                                                foreach (DataRow RW1 in DT_FIE_All.Rows)
                                                                {
                                                                    FIE_TemplateCode_Row++;
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 0] = int.Parse(RW1[0].ToString().Trim()); //Document ID ------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] = 0; // Back Image Key Similarity Average -----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 2] = 0; // Back Image Color Picker Similarity Average --------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] = 0; // Front Image Key Similarity Average ----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 4] = 0; // Front Image Color Picker Similarity Average -------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 5] = 0; // OCR Line Number Average Rank ----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6] = 0; // X Coefficient Back Image --------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7] = 0; // Y Coefficient Back Image --------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8] = 0; // Scan X Top-Left Back Image ------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9] = 0; // Scan Y Top-Left Back Image ------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10] = 0; // X Coefficient Front Image ------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11] = 0; // Y Coefficient Front Image ------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12] = 0; // Base X Top-Left Front Image ----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13] = 0; // Base Y Top-Left Front Image ----------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] = 0; // Founded Back Image -------------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 15] = 0; // Founded Back Image Color Picker ------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] = 0; // Founded Front Image ------------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 17] = 0; // Founded Front Image Color Picker -----------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 18] = 0; // Calculate Back Image -----------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 19] = 0; // Calculate Front Image ----------------------------> OK
                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 20] = 0; // Template Last Rank -------------------------------> OK
                                                                    // Search For Key Back Image :
                                                                    DT_FIEK = new DataTable();
                                                                    DT_FIEK = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select KeyCode,Similarity,X1,Y1,Y2,KeyIndex,KeyPosition From Template_09_BackImage_Elements Where (DID = '" + RW1[0].ToString().Trim() + "') And (KeyActive = '1') And (KeyCode <> '') Order By Y1,X1");
                                                                    if (DT_FIEK.Rows != null)
                                                                    {
                                                                        if (DT_FIEK.Rows.Count > 0)
                                                                        {
                                                                            int[,] FIE_TemplateKey = new int[DT_FIEK.Rows.Count, 2];
                                                                            int FIE_TemplateKey_Row = 0;
                                                                            foreach (DataRow RW2 in DT_FIEK.Rows)
                                                                            {
                                                                                FIE_TemplateKey_Row++;
                                                                                FIE_TemplateKey[FIE_TemplateKey_Row - 1, 0] = SF.GetWordsSimilarityInString(RW2[0].ToString().Trim(), OCR_TextResult);
                                                                                FIE_TemplateKey[FIE_TemplateKey_Row - 1, 1] = int.Parse(RW2[1].ToString().Trim());
                                                                            }
                                                                            double LastRate_Cal = 0;
                                                                            double LastRate_Master = 0;
                                                                            for (int i = 0; i < FIE_TemplateKey_Row; i++)
                                                                            {
                                                                                LastRate_Cal += FIE_TemplateKey[i, 0];
                                                                                LastRate_Master += FIE_TemplateKey[i, 1];
                                                                            }
                                                                            LastRate_Cal /= FIE_TemplateKey_Row;
                                                                            LastRate_Master /= FIE_TemplateKey_Row;
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] = (int)LastRate_Cal;
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] = 1;
                                                                        }
                                                                    }
                                                                    if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 14] == 1)
                                                                    {
                                                                        // Get Back Image Scale :
                                                                        double FI_X_Coe = 0;
                                                                        double FI_Y_Coe = 0;
                                                                        bool ProContinue = false;
                                                                        if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 1] > 0)
                                                                        {
                                                                            int M_X1 = 0; int M_Y1 = 0; int M_X2 = 0; int M_Y2 = 0;
                                                                            int S_X1 = 0; int S_Y1 = 0; int S_X2 = 0; int S_Y2 = 0;
                                                                            M_X1 = int.Parse(DT_FIEK.Rows[0][2].ToString().Trim());
                                                                            M_Y1 = (int)((int.Parse(DT_FIEK.Rows[0][3].ToString().Trim()) + int.Parse(DT_FIEK.Rows[0][4].ToString().Trim())) / 2);
                                                                            M_X2 = int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][2].ToString().Trim());
                                                                            M_Y2 = (int)((int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][3].ToString().Trim()) + int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][4].ToString().Trim())) / 2);
                                                                            if (SF.KeyDetector(ref S_X1, ref S_Y1, DT_FIEK.Rows[0][0].ToString().Trim(), OCRRes.ParsedResults[0].TextOverlay.Lines, DT_FIEK.Rows[0][1].ToString().Trim(), int.Parse(DT_FIEK.Rows[0][5].ToString().Trim()), DT_FIEK.Rows[0][6].ToString().Trim()) == true)
                                                                            {
                                                                                if (SF.KeyDetector(ref S_X2, ref S_Y2, DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][0].ToString().Trim(), OCRRes.ParsedResults[0].TextOverlay.Lines, DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][1].ToString().Trim(), int.Parse(DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][5].ToString().Trim()), DT_FIEK.Rows[DT_FIEK.Rows.Count - 1][6].ToString().Trim()) == true)
                                                                                {
                                                                                    int XSM = Math.Abs(S_X1 - S_X2);
                                                                                    int XMM = Math.Abs(M_X1 - M_X2);
                                                                                    if (XSM == 0) { XSM = 1; }
                                                                                    if (XMM == 0) { XMM = 1; }
                                                                                    FI_X_Coe = (double)((double)XMM / (double)XSM);
                                                                                    int YSM = Math.Abs(S_Y1 - S_Y2);
                                                                                    int YMM = Math.Abs(M_Y1 - M_Y2);
                                                                                    if (YSM == 0) { YSM = 1; }
                                                                                    if (YMM == 0) { YMM = 1; }
                                                                                    FI_Y_Coe = (double)((double)YMM / (double)YSM);
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6] = FI_X_Coe; // X Coefficient Back Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7] = FI_Y_Coe; // Y Coefficient Back Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8] = S_X1 - (M_X1 / FI_X_Coe); // Scan X Top-Left Back Image
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9] = S_Y1 - (M_Y1 / FI_Y_Coe); ; // Scan Y Top-Left Back Image
                                                                                    ProContinue = true;
                                                                                }
                                                                            }
                                                                        }
                                                                        if (ProContinue == true)
                                                                        {
                                                                            // Back Image Color Picker :
                                                                            DataTable DT_FICP = new DataTable();
                                                                            DT_FICP = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select X,Y,R,G,B,Similarity From Template_11_BackImage_ColorPicker Where (DID = '" + RW1[0].ToString().Trim() + "') Order By X,Y");
                                                                            if (DT_FICP.Rows != null)
                                                                            {
                                                                                if (DT_FICP.Rows.Count > 0)
                                                                                {
                                                                                    double IMG_IP = 0;
                                                                                    double IMG_BP = 0;
                                                                                    foreach (DataRow RW2 in DT_FICP.Rows)
                                                                                    {
                                                                                        int CPX = (int)((int.Parse(RW2[0].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 6]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 8]);
                                                                                        int CPY = (int)((int.Parse(RW2[1].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 7]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 9]);
                                                                                        Color CLS = Color.Black;
                                                                                        try
                                                                                        {
                                                                                            CLS = ((Bitmap)Img_Back_Nor).GetPixel(CPX, CPY);
                                                                                        }
                                                                                        catch (Exception)
                                                                                        { }
                                                                                        IMG_IP += SF.GetColorSimilarity(CLS, Color.FromArgb(int.Parse(RW2[2].ToString().Trim()), int.Parse(RW2[3].ToString().Trim()), int.Parse(RW2[4].ToString().Trim())));
                                                                                        IMG_BP += int.Parse(RW2[5].ToString().Trim());
                                                                                    }
                                                                                    IMG_IP /= DT_FICP.Rows.Count;
                                                                                    IMG_BP /= DT_FICP.Rows.Count;
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 2] = (int)IMG_IP;
                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 15] = 1;
                                                                                }
                                                                            }
                                                                            // Front Image Functionality :
                                                                            if (IMG_Nor_Tag_Back == true)
                                                                            {
                                                                                if ((RW1[1].ToString().Trim() == "1") && ((RW1[2].ToString().Trim() == "0")))
                                                                                {
                                                                                    // Get OCR Result Of Front Image :
                                                                                    if (OCRRes_Back.ParsedResults == null)
                                                                                    {
                                                                                        string OCRResultLocalBack = IDVOCR((Bitmap)Img_Front_Nor, FrontImageName_Format);
                                                                                        OCRRes_Back = JsonConvert.DeserializeObject<OCR_Result>(OCRResultLocalBack);
                                                                                        OCR_TextResult_Back = OCRRes_Back.ParsedResults[0].ParsedText.Trim();
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\r", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\n", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("\t", " ");
                                                                                        OCR_TextResult_Back = OCR_TextResult_Back.Replace("  ", " ").Replace("  ", " "); ;
                                                                                        OCR_TextOrientation_Back = OCRRes_Back.ParsedResults[0].TextOrientation.Trim();
                                                                                        OCR_Last_Front = OCRRes_Back;
                                                                                        if (OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim() != "0")
                                                                                        {
                                                                                            Img_Front_Nor = ImageRotation((Bitmap)Img_Front_Nor, OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim());
                                                                                            FrontIMage_Rotated = true;
                                                                                        }
                                                                                    }
                                                                                    // Search For Key Front Image :
                                                                                    DataTable DT_FBEK = new DataTable();
                                                                                    DT_FBEK = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select KeyCode,Similarity,X1,Y1,Y2,KeyIndex,KeyPosition From Template_08_FrontImage_Elements Where (DID = '" + RW1[0].ToString().Trim() + "') And (KeyActive = '1') And (KeyCode <> '') Order By Y1,X1");
                                                                                    if (DT_FBEK.Rows != null)
                                                                                    {
                                                                                        if (DT_FBEK.Rows.Count > 0)
                                                                                        {
                                                                                            int[,] BIE_TemplateKey = new int[DT_FBEK.Rows.Count, 2];
                                                                                            int BIE_TemplateKey_Row = 0;
                                                                                            foreach (DataRow RW2 in DT_FBEK.Rows)
                                                                                            {
                                                                                                BIE_TemplateKey_Row++;
                                                                                                BIE_TemplateKey[BIE_TemplateKey_Row - 1, 0] = SF.GetWordsSimilarityInString(RW2[0].ToString().Trim(), OCR_TextResult_Back);
                                                                                                BIE_TemplateKey[BIE_TemplateKey_Row - 1, 1] = int.Parse(RW2[1].ToString().Trim());
                                                                                            }
                                                                                            double LastRate_Cal = 0;
                                                                                            double LastRate_Master = 0;
                                                                                            for (int i = 0; i < BIE_TemplateKey_Row; i++)
                                                                                            {
                                                                                                LastRate_Cal += BIE_TemplateKey[i, 0];
                                                                                                LastRate_Master += BIE_TemplateKey[i, 1];
                                                                                            }
                                                                                            LastRate_Cal /= BIE_TemplateKey_Row;
                                                                                            LastRate_Master /= BIE_TemplateKey_Row;
                                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] = (int)LastRate_Cal;
                                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] = 1;
                                                                                        }
                                                                                    }
                                                                                    if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 16] == 1)
                                                                                    {
                                                                                        // Get Front Image Scale :
                                                                                        double FI_X_CoeB = 0;
                                                                                        double FI_Y_CoeB = 0;
                                                                                        bool ProContinueB = false;
                                                                                        if (FIE_TemplateCode[FIE_TemplateCode_Row - 1, 3] > 0)
                                                                                        {
                                                                                            int M_X1 = 0; int M_Y1 = 0; int M_X2 = 0; int M_Y2 = 0;
                                                                                            int S_X1 = 0; int S_Y1 = 0; int S_X2 = 0; int S_Y2 = 0;
                                                                                            M_X1 = int.Parse(DT_FBEK.Rows[0][2].ToString().Trim());
                                                                                            M_Y1 = (int)((int.Parse(DT_FBEK.Rows[0][3].ToString().Trim()) + int.Parse(DT_FBEK.Rows[0][4].ToString().Trim())) / 2);
                                                                                            M_X2 = int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][2].ToString().Trim());
                                                                                            M_Y2 = (int)((int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][3].ToString().Trim()) + int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][4].ToString().Trim())) / 2);
                                                                                            if (SF.KeyDetector(ref S_X1, ref S_Y1, DT_FBEK.Rows[0][0].ToString().Trim(), OCRRes_Back.ParsedResults[0].TextOverlay.Lines, DT_FBEK.Rows[0][1].ToString().Trim(), int.Parse(DT_FBEK.Rows[0][5].ToString().Trim()), DT_FBEK.Rows[0][6].ToString().Trim()) == true)
                                                                                            {
                                                                                                if (SF.KeyDetector(ref S_X2, ref S_Y2, DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][0].ToString().Trim(), OCRRes_Back.ParsedResults[0].TextOverlay.Lines, DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][1].ToString().Trim(), int.Parse(DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][5].ToString().Trim()), DT_FBEK.Rows[DT_FBEK.Rows.Count - 1][6].ToString().Trim()) == true)
                                                                                                {
                                                                                                    int XSM = Math.Abs(S_X1 - S_X2);
                                                                                                    int XMM = Math.Abs(M_X1 - M_X2);
                                                                                                    if (XSM == 0) { XSM = 1; }
                                                                                                    if (XMM == 0) { XMM = 1; }
                                                                                                    FI_X_CoeB = (double)((double)XMM / (double)XSM);
                                                                                                    int YSM = Math.Abs(S_Y1 - S_Y2);
                                                                                                    int YMM = Math.Abs(M_Y1 - M_Y2);
                                                                                                    if (YSM == 0) { YSM = 1; }
                                                                                                    if (YMM == 0) { YMM = 1; }
                                                                                                    FI_Y_CoeB = (double)((double)YMM / (double)YSM);
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10] = FI_X_CoeB; // X Coefficient Front Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11] = FI_Y_CoeB; // Y Coefficient Front Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12] = S_X1 - (M_X1 / FI_X_CoeB); // Scan X Top-Left Front Image
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13] = S_Y1 - (M_Y1 / FI_Y_CoeB); ; // Scan Y Top-Left Front Image
                                                                                                    ProContinueB = true;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        if (ProContinueB == true)
                                                                                        {
                                                                                            // Front Image Color Picker :
                                                                                            DataTable DT_FICPB = new DataTable();
                                                                                            DT_FICPB = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select X,Y,R,G,B,Similarity From Template_10_FrontImage_ColorPicker Where (DID = '" + RW1[0].ToString().Trim() + "') Order By X,Y");
                                                                                            if (DT_FICPB.Rows != null)
                                                                                            {
                                                                                                if (DT_FICPB.Rows.Count > 0)
                                                                                                {
                                                                                                    double IMG_IP = 0;
                                                                                                    double IMG_BP = 0;
                                                                                                    foreach (DataRow RW2 in DT_FICPB.Rows)
                                                                                                    {
                                                                                                        int CPX = (int)((int.Parse(RW2[0].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 10]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 12]);
                                                                                                        int CPY = (int)((int.Parse(RW2[1].ToString().Trim()) * FIE_TemplateCode[FIE_TemplateCode_Row - 1, 11]) + FIE_TemplateCode[FIE_TemplateCode_Row - 1, 13]);
                                                                                                        Color CLS = Color.Black;
                                                                                                        try
                                                                                                        {
                                                                                                            CLS = ((Bitmap)Img_Front_Nor).GetPixel(CPX, CPY);
                                                                                                        }
                                                                                                        catch (Exception)
                                                                                                        { }
                                                                                                        IMG_IP += SF.GetColorSimilarity(CLS, Color.FromArgb(int.Parse(RW2[2].ToString().Trim()), int.Parse(RW2[3].ToString().Trim()), int.Parse(RW2[4].ToString().Trim())));
                                                                                                        IMG_BP += int.Parse(RW2[5].ToString().Trim());
                                                                                                    }
                                                                                                    IMG_IP /= DT_FICPB.Rows.Count;
                                                                                                    IMG_BP /= DT_FICPB.Rows.Count;
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 4] = (int)IMG_IP;
                                                                                                    FIE_TemplateCode[FIE_TemplateCode_Row - 1, 17] = 1;
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                            // Get Optimal , Maximum , Minimum OCR Fields Count :
                                                                            int LastCounterPercent = 0;
                                                                            int OCRNorC = int.Parse(RW1[3].ToString().Trim());
                                                                            int OCRMinC = int.Parse(RW1[4].ToString().Trim());
                                                                            int OCRMaxC = int.Parse(RW1[5].ToString().Trim());
                                                                            int LineCount = OCRRes.ParsedResults[0].TextOverlay.Lines.Count;
                                                                            if (LineCount >= OCRMinC) { LastCounterPercent += 25; }
                                                                            if (LineCount <= OCRMaxC) { LastCounterPercent += 25; }
                                                                            if ((LineCount >= (OCRNorC - (int)((OCRNorC * NormalOcrLines_threshold) / 100))) && (LineCount <= (OCRNorC + (int)((OCRNorC * NormalOcrLines_threshold) / 100)))) { LastCounterPercent += 50; }
                                                                            FIE_TemplateCode[FIE_TemplateCode_Row - 1, 5] = (int)LastCounterPercent;
                                                                            // Get Front / Back Image Calculation :
                                                                            if (RW1[1].ToString().Trim() == "1") { FIE_TemplateCode[FIE_TemplateCode_Row - 1, 19] = 1; }
                                                                            if (RW1[6].ToString().Trim() == "1") { FIE_TemplateCode[FIE_TemplateCode_Row - 1, 18] = 1; }
                                                                        }
                                                                    }
                                                                }
                                                                // Select Template With Maximum Rank
                                                                if ((FIE_TemplateCode.Length / 21) > 0)
                                                                {
                                                                    int Divider_Count = 0;
                                                                    double SumRank = 0;
                                                                    // Calculate Template Last Rank
                                                                    for (int i = 0; i < (FIE_TemplateCode.Length / 21); i++)
                                                                    {
                                                                        Divider_Count = 0;
                                                                        SumRank = 0;
                                                                        FIE_TemplateCode[i, 20] = 0;
                                                                        // Back Image Rank 
                                                                        if (FIE_TemplateCode[i, 18] == 1)
                                                                        {
                                                                            // Key Similarity
                                                                            if (FIE_TemplateCode[i, 14] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 1] * Rank_KeySimilarity_FI;
                                                                                Divider_Count += Rank_KeySimilarity_FI;
                                                                            }
                                                                            // Color Picker Similarity 
                                                                            if (FIE_TemplateCode[i, 15] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 2] * Rank_ColorSimilarity_FI;
                                                                                Divider_Count += Rank_ColorSimilarity_FI;
                                                                            }
                                                                        }
                                                                        // Front Image Rank 
                                                                        if (FIE_TemplateCode[i, 19] == 1)
                                                                        {
                                                                            // Key Similarity
                                                                            if (FIE_TemplateCode[i, 16] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 3] * Rank_KeySimilarity_BI;
                                                                                Divider_Count += Rank_KeySimilarity_BI;
                                                                            }
                                                                            // Color Picker Similarity 
                                                                            if (FIE_TemplateCode[i, 17] == 1)
                                                                            {
                                                                                SumRank += FIE_TemplateCode[i, 4] * Rank_ColorSimilarity_BI;
                                                                                Divider_Count += Rank_ColorSimilarity_BI;
                                                                            }
                                                                        }
                                                                        // OCR Line Number Rank
                                                                        SumRank += FIE_TemplateCode[i, 5] * Rank_OCRFielad;
                                                                        Divider_Count += Rank_OCRFielad;
                                                                        // Calculate Average
                                                                        try
                                                                        {
                                                                            FIE_TemplateCode[i, 20] = (int)Math.Round(SumRank / Divider_Count);
                                                                        }
                                                                        catch (Exception)
                                                                        {
                                                                            FIE_TemplateCode[i, 20] = 0;
                                                                        }
                                                                    }
                                                                    // Select Template With Maximum Rank :
                                                                    var TemList = new List<TemplateList>();
                                                                    TemList.Clear();
                                                                    double MaxTemplateRank = 0;
                                                                    for (int i = 0; i < (FIE_TemplateCode.Length / 21); i++)
                                                                    {
                                                                        if (FIE_TemplateCode[i, 20] > MaxTemplateRank)
                                                                        {
                                                                            TemList.Clear();
                                                                            MaxTemplateRank = FIE_TemplateCode[i, 20];
                                                                            TemList.Add(new TemplateList()
                                                                            {
                                                                                TemplateID = FIE_TemplateCode[i, 0].ToString(),
                                                                                BackImage_Key_Similarity = FIE_TemplateCode[i, 1],
                                                                                BackImage_ColorPicker_Similarity = FIE_TemplateCode[i, 2],
                                                                                BackImage_X_Coeficient = FIE_TemplateCode[i, 6],
                                                                                BackImage_Y_Coeficient = FIE_TemplateCode[i, 7],
                                                                                BackImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 8],
                                                                                BackImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 9],
                                                                                FrontImage_Key_Similarity = FIE_TemplateCode[i, 3],
                                                                                FrontImage_ColorPicker_Similarity = FIE_TemplateCode[i, 4],
                                                                                FrontImage_X_Coeficient = FIE_TemplateCode[i, 10],
                                                                                FrontImage_Y_Coeficient = FIE_TemplateCode[i, 11],
                                                                                FrontImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 12],
                                                                                FrontImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 13],
                                                                                OCR_Line_Rank = FIE_TemplateCode[i, 5],
                                                                                TemplateAverageRank = FIE_TemplateCode[i, 20],
                                                                                TemplateAverageRank_Step2 = 0
                                                                            });
                                                                        }
                                                                        else
                                                                        {
                                                                            if (FIE_TemplateCode[i, 20] == MaxTemplateRank)
                                                                            {
                                                                                MaxTemplateRank = FIE_TemplateCode[i, 20];
                                                                                TemList.Add(new TemplateList()
                                                                                {
                                                                                    TemplateID = FIE_TemplateCode[i, 0].ToString(),
                                                                                    BackImage_Key_Similarity = FIE_TemplateCode[i, 1],
                                                                                    BackImage_ColorPicker_Similarity = FIE_TemplateCode[i, 2],
                                                                                    BackImage_X_Coeficient = FIE_TemplateCode[i, 6],
                                                                                    BackImage_Y_Coeficient = FIE_TemplateCode[i, 7],
                                                                                    BackImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 8],
                                                                                    BackImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 9],
                                                                                    FrontImage_Key_Similarity = FIE_TemplateCode[i, 3],
                                                                                    FrontImage_ColorPicker_Similarity = FIE_TemplateCode[i, 4],
                                                                                    FrontImage_X_Coeficient = FIE_TemplateCode[i, 10],
                                                                                    FrontImage_Y_Coeficient = FIE_TemplateCode[i, 11],
                                                                                    FrontImage_X_TopLeft_Refrence = FIE_TemplateCode[i, 12],
                                                                                    FrontImage_Y_TopLeft_Refrence = FIE_TemplateCode[i, 13],
                                                                                    OCR_Line_Rank = FIE_TemplateCode[i, 5],
                                                                                    TemplateAverageRank = FIE_TemplateCode[i, 20],
                                                                                    TemplateAverageRank_Step2 = 0
                                                                                });
                                                                            }
                                                                        }
                                                                    }
                                                                    if (TemList.Count == 1)
                                                                    {
                                                                        LST_Template.TemplateID = TemList[0].TemplateID;
                                                                        LST_Template.FrontImage_Key_Similarity = TemList[0].FrontImage_Key_Similarity;
                                                                        LST_Template.FrontImage_ColorPicker_Similarity = TemList[0].FrontImage_ColorPicker_Similarity;
                                                                        LST_Template.FrontImage_X_Coeficient = TemList[0].FrontImage_X_Coeficient;
                                                                        LST_Template.FrontImage_Y_Coeficient = TemList[0].FrontImage_Y_Coeficient;
                                                                        LST_Template.FrontImage_X_TopLeft_Refrence = TemList[0].FrontImage_X_TopLeft_Refrence;
                                                                        LST_Template.FrontImage_Y_TopLeft_Refrence = TemList[0].FrontImage_Y_TopLeft_Refrence;
                                                                        LST_Template.BackImage_Key_Similarity = TemList[0].BackImage_Key_Similarity;
                                                                        LST_Template.BackImage_ColorPicker_Similarity = TemList[0].BackImage_ColorPicker_Similarity;
                                                                        LST_Template.BackImage_X_Coeficient = TemList[0].BackImage_X_Coeficient;
                                                                        LST_Template.BackImage_Y_Coeficient = TemList[0].BackImage_Y_Coeficient;
                                                                        LST_Template.BackImage_X_TopLeft_Refrence = TemList[0].BackImage_X_TopLeft_Refrence;
                                                                        LST_Template.BackImage_Y_TopLeft_Refrence = TemList[0].BackImage_Y_TopLeft_Refrence;
                                                                        LST_Template.OCR_Line_Rank = TemList[0].OCR_Line_Rank;
                                                                        LST_Template.TemplateAverageRank = TemList[0].TemplateAverageRank;
                                                                        TemplateFounded = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            foreach (TemplateList TL2 in TemList)
                                                                            {
                                                                                if (TL.TemplateID != TL2.TemplateID)
                                                                                {
                                                                                    if (TL.TemplateAverageRank >= TL2.TemplateAverageRank)
                                                                                    {
                                                                                        if (TL.TemplateAverageRank == TL2.TemplateAverageRank)
                                                                                        {
                                                                                            if (TL.BackImage_Key_Similarity >= TL2.BackImage_Key_Similarity)
                                                                                            {
                                                                                                if (TL.BackImage_Key_Similarity == TL2.BackImage_Key_Similarity)
                                                                                                {
                                                                                                    if (TL.BackImage_ColorPicker_Similarity >= TL2.BackImage_ColorPicker_Similarity)
                                                                                                    {
                                                                                                        if (TL.BackImage_ColorPicker_Similarity == TL2.BackImage_ColorPicker_Similarity)
                                                                                                        {
                                                                                                            if (TL.FrontImage_Key_Similarity >= TL2.FrontImage_Key_Similarity)
                                                                                                            {
                                                                                                                if (TL.FrontImage_Key_Similarity == TL2.FrontImage_Key_Similarity)
                                                                                                                {
                                                                                                                    if (TL.FrontImage_ColorPicker_Similarity >= TL2.FrontImage_ColorPicker_Similarity)
                                                                                                                    {
                                                                                                                        if (TL.FrontImage_ColorPicker_Similarity == TL2.FrontImage_ColorPicker_Similarity)
                                                                                                                        {
                                                                                                                            if (TL.OCR_Line_Rank >= TL2.OCR_Line_Rank)
                                                                                                                            {
                                                                                                                                if (TL.OCR_Line_Rank == TL2.OCR_Line_Rank)
                                                                                                                                {
                                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                                    TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                                }
                                                                                                                                else
                                                                                                                                {
                                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                                }
                                                                                                                            }
                                                                                                                            else
                                                                                                                            {
                                                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                            }
                                                                                                                        }
                                                                                                                        else
                                                                                                                        {
                                                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                                                        }
                                                                                                                    }
                                                                                                                    else
                                                                                                                    {
                                                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                                                    }
                                                                                                                }
                                                                                                                else
                                                                                                                {
                                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                                }
                                                                                                            }
                                                                                                            else
                                                                                                            {
                                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                                            }
                                                                                                        }
                                                                                                        else
                                                                                                        {
                                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                                        }
                                                                                                    }
                                                                                                    else
                                                                                                    {
                                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                                    }
                                                                                                }
                                                                                                else
                                                                                                {
                                                                                                    TL.TemplateAverageRank_Step2 += 1;
                                                                                                }
                                                                                            }
                                                                                            else
                                                                                            {
                                                                                                TL2.TemplateAverageRank_Step2 += 1;
                                                                                            }
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            TL.TemplateAverageRank_Step2 += 1;
                                                                                        }
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        TL2.TemplateAverageRank_Step2 += 1;
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                        double MaxRank2 = 0;
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            if (TL.TemplateAverageRank_Step2 > MaxRank2)
                                                                            {
                                                                                LST_Template.TemplateID = TL.TemplateID;
                                                                                LST_Template.FrontImage_Key_Similarity = TL.FrontImage_Key_Similarity;
                                                                                LST_Template.FrontImage_ColorPicker_Similarity = TL.FrontImage_ColorPicker_Similarity;
                                                                                LST_Template.FrontImage_X_Coeficient = TL.FrontImage_X_Coeficient;
                                                                                LST_Template.FrontImage_Y_Coeficient = TL.FrontImage_Y_Coeficient;
                                                                                LST_Template.FrontImage_X_TopLeft_Refrence = TL.FrontImage_X_TopLeft_Refrence;
                                                                                LST_Template.FrontImage_Y_TopLeft_Refrence = TL.FrontImage_Y_TopLeft_Refrence;
                                                                                LST_Template.BackImage_Key_Similarity = TL.BackImage_Key_Similarity;
                                                                                LST_Template.BackImage_ColorPicker_Similarity = TL.BackImage_ColorPicker_Similarity;
                                                                                LST_Template.BackImage_X_Coeficient = TL.BackImage_X_Coeficient;
                                                                                LST_Template.BackImage_Y_Coeficient = TL.BackImage_Y_Coeficient;
                                                                                LST_Template.BackImage_X_TopLeft_Refrence = TL.BackImage_X_TopLeft_Refrence;
                                                                                LST_Template.BackImage_Y_TopLeft_Refrence = TL.BackImage_Y_TopLeft_Refrence;
                                                                                LST_Template.OCR_Line_Rank = TL.OCR_Line_Rank;
                                                                                LST_Template.TemplateAverageRank = TL.TemplateAverageRank;
                                                                                LST_Template.TemplateAverageRank_Step2 = TL.TemplateAverageRank_Step2;
                                                                            }
                                                                        }
                                                                        TemplateFounded = true;
                                                                        foreach (TemplateList TL in TemList)
                                                                        {
                                                                            if (LST_Template.TemplateID != TL.TemplateID)
                                                                            {
                                                                                if (LST_Template.TemplateAverageRank_Step2 == TL.TemplateAverageRank_Step2)
                                                                                {
                                                                                    TemplateFounded = false;
                                                                                    break;
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    TemplateFounded = false;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                TemplateFounded = false;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            TemplateFounded = false;
                                                        }
                                                    }
                                                    //=============================================================================================================================================================
                                                    //================================================================= Element Convert To Values =================================================================
                                                    //=============================================================================================================================================================
                                                    if (TemplateFounded == true)
                                                    {
                                                        if (LST_Template.TemplateAverageRank >= Rank_TemplateAcceptedMinmumRank)
                                                        {
                                                            DataTable DT_BaseConfig = new DataTable();
                                                            DT_BaseConfig = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select FrontImage,FrontImageBarcode,FrontImageBarcodeQR,OnlyFrontImageBarcode,OnlyFrontImageBarcodeQR,FrontImageBarcodeReplcaseField,BackImage,BackImageBarcode,BackImageBarcodeQR,OnlyBackImageBarcode,OnlyBackImageBarcodeQR,BackImageBarcodeReplcaseField From Template_06_BasicConfiguration Where (DID = '" + LST_Template.TemplateID + "')");
                                                            if (DT_BaseConfig.Rows != null)
                                                            {
                                                                if (DT_BaseConfig.Rows.Count == 1)
                                                                {
                                                                    var OREL = new List<OCR_Relut_Elements>();
                                                                    try
                                                                    {
                                                                        OREL.Clear();
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    try
                                                                    {
                                                                        DataTable DTOPF = new DataTable();
                                                                        DTOPF = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select OutputTitle From Template_08_FrontImage_Elements Where (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1')");
                                                                        foreach (DataRow RW in DTOPF.Rows)
                                                                        {
                                                                            DataReady(ref OREL, RW[0].ToString().Trim(), "");
                                                                        }
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    try
                                                                    {
                                                                        DataTable DTOPF = new DataTable();
                                                                        DTOPF = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select OutputTitle From Template_09_BackImage_Elements Where (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1')");
                                                                        foreach (DataRow RW in DTOPF.Rows)
                                                                        {
                                                                            DataReady(ref OREL, RW[0].ToString().Trim(), "");
                                                                        }
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    if (OCR_Last_Front.ParsedResults != null)
                                                                    {
                                                                        if (DT_BaseConfig.Rows[0][0].ToString().Trim() == "1")
                                                                        {
                                                                            if (DT_BaseConfig.Rows[0][3].ToString().Trim() == "1")
                                                                            {
                                                                                if (DT_BaseConfig.Rows[0][5].ToString().Trim() != "")
                                                                                {
                                                                                    // Only Read Barcode :
                                                                                    if (DT_BaseConfig.Rows[0][4].ToString().Trim() == "1")
                                                                                    {
                                                                                        // QR Barcode :
                                                                                        string BRCDR = "";
                                                                                        BRCDR = Barcode_QR_Read((Bitmap)Img_Front_Nor);
                                                                                        DataReady(ref OREL, DT_BaseConfig.Rows[0][5].ToString().Trim(), BRCDR);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        // 128 Normal Barcode :
                                                                                        string BRCDR = "";
                                                                                        BRCDR = Barcode_128_Read((Bitmap)Img_Front_Nor);
                                                                                        DataReady(ref OREL, DT_BaseConfig.Rows[0][5].ToString().Trim(), BRCDR);
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                // Get Front Image Elements :
                                                                                DataTable DTWRD = new DataTable();
                                                                                foreach (Line LN in OCR_Last_Front.ParsedResults[0].TextOverlay.Lines)
                                                                                {
                                                                                    foreach (Word WD in LN.Words)
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            int T = (int)Math.Round((WD.Top - LST_Template.FrontImage_Y_TopLeft_Refrence) * LST_Template.FrontImage_Y_Coeficient);
                                                                                            int B = (int)Math.Round(T + (WD.Height * LST_Template.FrontImage_Y_Coeficient));
                                                                                            int L = (int)Math.Round((WD.Left - LST_Template.FrontImage_X_TopLeft_Refrence) * LST_Template.FrontImage_X_Coeficient);
                                                                                            int R = (int)Math.Round(L + (WD.Width * LST_Template.FrontImage_X_Coeficient));
                                                                                            DTWRD = new DataTable();
                                                                                            DTWRD = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select OutputTitle From Template_08_FrontImage_Elements Where (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1') And (X1 <= '" + L + "') And (Y1 <= '" + T + "') And (X4 >= '" + R + "') And (Y4 >= '" + B + "')");
                                                                                            if (DTWRD.Rows != null)
                                                                                            {
                                                                                                if (DTWRD.Rows.Count == 1)
                                                                                                {
                                                                                                    try
                                                                                                    {
                                                                                                        DataReady(ref OREL, DTWRD.Rows[0][0].ToString().Trim(), WD.WordText.Trim());
                                                                                                    }
                                                                                                    catch (Exception)
                                                                                                    { }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        catch (Exception)
                                                                                        { }
                                                                                    }
                                                                                }
                                                                                // Read Image Barcode :
                                                                                if (DT_BaseConfig.Rows[0][1].ToString().Trim() == "1")
                                                                                {
                                                                                    if (DT_BaseConfig.Rows[0][5].ToString().Trim() != "")
                                                                                    {
                                                                                        if (DT_BaseConfig.Rows[0][2].ToString().Trim() == "1")
                                                                                        {
                                                                                            // QR Barcode :
                                                                                            string BRCDR = "";
                                                                                            BRCDR = Barcode_QR_Read((Bitmap)Img_Front_Nor);
                                                                                            DataReady(ref OREL, DT_BaseConfig.Rows[0][5].ToString().Trim(), BRCDR);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            // 128 Normal Barcode :
                                                                                            string BRCDR = "";
                                                                                            BRCDR = Barcode_128_Read((Bitmap)Img_Front_Nor);
                                                                                            DataReady(ref OREL, DT_BaseConfig.Rows[0][5].ToString().Trim(), BRCDR);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    if (OCR_Last_Back.ParsedResults != null)
                                                                    {
                                                                        if (DT_BaseConfig.Rows[0][6].ToString().Trim() == "1")
                                                                        {
                                                                            if (DT_BaseConfig.Rows[0][9].ToString().Trim() == "1")
                                                                            {
                                                                                if (DT_BaseConfig.Rows[0][11].ToString().Trim() != "")
                                                                                {
                                                                                    // Only Read Barcode :
                                                                                    if (DT_BaseConfig.Rows[0][10].ToString().Trim() == "1")
                                                                                    {
                                                                                        // QR Barcode :
                                                                                        string BRCDR = "";
                                                                                        BRCDR = Barcode_QR_Read((Bitmap)Img_Back_Nor);
                                                                                        DataReady(ref OREL, DT_BaseConfig.Rows[0][11].ToString().Trim(), BRCDR);
                                                                                    }
                                                                                    else
                                                                                    {
                                                                                        // 128 Normal Barcode :
                                                                                        string BRCDR = "";
                                                                                        BRCDR = Barcode_128_Read((Bitmap)Img_Back_Nor);
                                                                                        DataReady(ref OREL, DT_BaseConfig.Rows[0][11].ToString().Trim(), BRCDR);
                                                                                    }
                                                                                }
                                                                            }
                                                                            else
                                                                            {
                                                                                // Get Back Image Elements :
                                                                                DataTable DTWRD = new DataTable();
                                                                                foreach (Line LN in OCR_Last_Back.ParsedResults[0].TextOverlay.Lines)
                                                                                {
                                                                                    foreach (Word WD in LN.Words)
                                                                                    {
                                                                                        try
                                                                                        {
                                                                                            int T = (int)Math.Round((WD.Top - LST_Template.BackImage_Y_TopLeft_Refrence) * LST_Template.BackImage_Y_Coeficient);
                                                                                            int B = (int)Math.Round(T + (WD.Height * LST_Template.BackImage_Y_Coeficient));
                                                                                            int L = (int)Math.Round((WD.Left - LST_Template.BackImage_X_TopLeft_Refrence) * LST_Template.BackImage_X_Coeficient);
                                                                                            int R = (int)Math.Round(L + (WD.Width * LST_Template.BackImage_X_Coeficient));
                                                                                            DTWRD = new DataTable();
                                                                                            DTWRD = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select OutputTitle From Template_09_BackImage_Elements Where (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1') And (X1 <= '" + L + "') And (Y1 <= '" + T + "') And (X4 >= '" + R + "') And (Y4 >= '" + B + "')");
                                                                                            if (DTWRD.Rows != null)
                                                                                            {
                                                                                                if (DTWRD.Rows.Count == 1)
                                                                                                {
                                                                                                    try
                                                                                                    {
                                                                                                        DataReady(ref OREL, DTWRD.Rows[0][0].ToString().Trim(), WD.WordText.Trim());
                                                                                                    }
                                                                                                    catch (Exception)
                                                                                                    { }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                        catch (Exception)
                                                                                        { }
                                                                                    }
                                                                                }
                                                                                // Read Image Barcode :
                                                                                if (DT_BaseConfig.Rows[0][7].ToString().Trim() == "1")
                                                                                {
                                                                                    if (DT_BaseConfig.Rows[0][11].ToString().Trim() != "")
                                                                                    {
                                                                                        if (DT_BaseConfig.Rows[0][8].ToString().Trim() == "1")
                                                                                        {
                                                                                            // QR Barcode :
                                                                                            string BRCDR = "";
                                                                                            BRCDR = Barcode_QR_Read((Bitmap)Img_Back_Nor);
                                                                                            DataReady(ref OREL, DT_BaseConfig.Rows[0][11].ToString().Trim(), BRCDR);
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            // 128 Normal Barcode :
                                                                                            string BRCDR = "";
                                                                                            BRCDR = Barcode_128_Read((Bitmap)Img_Back_Nor);
                                                                                            DataReady(ref OREL, DT_BaseConfig.Rows[0][11].ToString().Trim(), BRCDR);
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    // Save Country And State And Document Type :
                                                                    try
                                                                    {
                                                                        int DT_FND = 0;
                                                                        string Country_ID = "0";
                                                                        string State_ID = "0";
                                                                        string DocType_ID = "0";
                                                                        DataTable DT_Codes = new DataTable();
                                                                        DT_Codes = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 CID,SID,DTID From Template_08_FrontImage_Elements Where (DID = '" + LST_Template.TemplateID + "')");
                                                                        if (DT_Codes.Rows.Count == 1)
                                                                        {
                                                                            DT_FND = 1;
                                                                            Country_ID = DT_Codes.Rows[0][0].ToString().Trim();
                                                                            State_ID = DT_Codes.Rows[0][1].ToString().Trim();
                                                                            DocType_ID = DT_Codes.Rows[0][2].ToString().Trim();
                                                                        }
                                                                        else
                                                                        {
                                                                            DT_Codes = new DataTable();
                                                                            DT_Codes = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 CID,SID,DTID From Template_09_BackImage_Elements Where (DID = '" + LST_Template.TemplateID + "')");
                                                                            if (DT_Codes.Rows.Count == 1)
                                                                            {
                                                                                DT_FND = 1;
                                                                                Country_ID = DT_Codes.Rows[0][0].ToString().Trim();
                                                                                State_ID = DT_Codes.Rows[0][1].ToString().Trim();
                                                                                DocType_ID = DT_Codes.Rows[0][2].ToString().Trim();
                                                                            }
                                                                        }
                                                                        if (DT_FND == 1)
                                                                        {
                                                                            DataTable DT_DTVL = new DataTable();
                                                                            try
                                                                            {
                                                                                DT_DTVL = new DataTable();
                                                                                DT_DTVL = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 Country_Code,Country_Name From Template_01_Country Where (Country_ID = '" + Country_ID + "')");
                                                                                if (DT_DTVL.Rows.Count == 1)
                                                                                {
                                                                                    DataReady(ref OREL, "Country Code", DT_DTVL.Rows[0][0].ToString().Trim());
                                                                                    DataReady(ref OREL, "Country Name", DT_DTVL.Rows[0][1].ToString().Trim());
                                                                                }
                                                                            }
                                                                            catch (Exception) { }
                                                                            try
                                                                            {
                                                                                DT_DTVL = new DataTable();
                                                                                DT_DTVL = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 State_Code,State_Name From Template_02_State Where (Country_ID = '" + Country_ID + "') And (State_ID = '" + State_ID + "')");
                                                                                if (DT_DTVL.Rows.Count == 1)
                                                                                {
                                                                                    DataReady(ref OREL, "State Code", DT_DTVL.Rows[0][0].ToString().Trim());
                                                                                    DataReady(ref OREL, "State Name", DT_DTVL.Rows[0][1].ToString().Trim());
                                                                                }
                                                                            }
                                                                            catch (Exception) { }
                                                                            try
                                                                            {
                                                                                DT_DTVL = new DataTable();
                                                                                DT_DTVL = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Top 1 DocType_Code,DocType_Name From Template_03_DocumentType Where (DocType_ID = '" + DocType_ID + "')");
                                                                                if (DT_DTVL.Rows.Count == 1)
                                                                                {
                                                                                    DataReady(ref OREL, "Document Code", DT_DTVL.Rows[0][0].ToString().Trim());
                                                                                    DataReady(ref OREL, "Document Name", DT_DTVL.Rows[0][1].ToString().Trim());
                                                                                }
                                                                            }
                                                                            catch (Exception) { }
                                                                        }
                                                                    }
                                                                    catch (Exception) { }
                                                                    // Save Data To Sql :
                                                                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Delete From Users_16_API_Acuant_Result Where (Transaction_ID = '" + TransactionID + "')");
                                                                    int RowN = 0;
                                                                    int TemplateFnd = 0;
                                                                    string DP_TypeCode = "";
                                                                    string DP_Sub_Start = "";
                                                                    string DP_Sub_Length = "";
                                                                    string DP_Sub_Left = "";
                                                                    string DP_Input_Format = "";
                                                                    string DP_Input_Format_Sep = "";
                                                                    string DP_Output_Format = "";
                                                                    string DP_Output_Format_Sep = "";
                                                                    foreach (OCR_Relut_Elements ORE in OREL)
                                                                    {
                                                                        TemplateFnd = 0;
                                                                        DP_TypeCode = "0";
                                                                        DP_Sub_Start = "";
                                                                        DP_Sub_Length = "";
                                                                        DP_Sub_Left = "";
                                                                        DP_Input_Format = "";
                                                                        DP_Input_Format_Sep = "";
                                                                        DP_Output_Format = "";
                                                                        DP_Output_Format_Sep = "";
                                                                        try
                                                                        {
                                                                            DataTable DT_EL = new DataTable();
                                                                            DT_EL = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select TypeCode,Sub_Start,Sub_Length,Sub_Left,Input_Format,Input_Format_Sep,Output_Format,Output_Format_Sep From Template_08_FrontImage_Elements Where (Data_Processing = '1') And (OutputTitle = '" + ORE.Element_Key + "') And (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1')");
                                                                            if (DT_EL.Rows.Count == 1)
                                                                            {
                                                                                TemplateFnd = 1;
                                                                                DP_TypeCode = DT_EL.Rows[0][0].ToString();
                                                                                DP_Sub_Start = DT_EL.Rows[0][1].ToString();
                                                                                DP_Sub_Length = DT_EL.Rows[0][2].ToString();
                                                                                DP_Sub_Left = DT_EL.Rows[0][3].ToString();
                                                                                DP_Input_Format = DT_EL.Rows[0][4].ToString();
                                                                                DP_Input_Format_Sep = DT_EL.Rows[0][5].ToString();
                                                                                DP_Output_Format = DT_EL.Rows[0][6].ToString();
                                                                                DP_Output_Format_Sep = DT_EL.Rows[0][7].ToString();
                                                                            }
                                                                            if (TemplateFnd == 0)
                                                                            {
                                                                                DT_EL = new DataTable();
                                                                                DT_EL = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select TypeCode,Sub_Start,Sub_Length,Sub_Left,Input_Format,Input_Format_Sep,Output_Format,Output_Format_Sep From Template_09_BackImage_Elements Where (Data_Processing = '1') And (OutputTitle = '" + ORE.Element_Key + "') And (DID = '" + LST_Template.TemplateID + "') And (OutputTag = '1')");
                                                                                if (DT_EL.Rows.Count == 1)
                                                                                {
                                                                                    TemplateFnd = 1;
                                                                                    DP_TypeCode = DT_EL.Rows[0][0].ToString();
                                                                                    DP_Sub_Start = DT_EL.Rows[0][1].ToString();
                                                                                    DP_Sub_Length = DT_EL.Rows[0][2].ToString();
                                                                                    DP_Sub_Left = DT_EL.Rows[0][3].ToString();
                                                                                    DP_Input_Format = DT_EL.Rows[0][4].ToString();
                                                                                    DP_Input_Format_Sep = DT_EL.Rows[0][5].ToString();
                                                                                    DP_Output_Format = DT_EL.Rows[0][6].ToString();
                                                                                    DP_Output_Format_Sep = DT_EL.Rows[0][7].ToString();
                                                                                }
                                                                            }
                                                                        }
                                                                        catch (Exception) { TemplateFnd = 0; }
                                                                        ORE.Element_Value = ORE.Element_Value.Replace("?", "").Replace(",", "").Replace("'", "").Replace("\"", "").Replace("  ", " ").Trim();
                                                                        if (TemplateFnd == 1)
                                                                        {
                                                                            string LastValueResult = "";
                                                                            string BeforeValue = ORE.Element_Value.Trim();
                                                                            try
                                                                            {
                                                                                // Substring :
                                                                                if ((DP_Sub_Start != "0") && (DP_Sub_Start != ""))
                                                                                {
                                                                                    if ((DP_Sub_Length != "0") && (DP_Sub_Length != ""))
                                                                                    {
                                                                                        if ((DP_Sub_Left == "0") || (DP_Sub_Left == ""))
                                                                                        {
                                                                                            LastValueResult = BeforeValue.Substring(int.Parse(DP_Sub_Start) - 1, int.Parse(DP_Sub_Length));
                                                                                        }
                                                                                        else
                                                                                        {
                                                                                            LastValueResult = BeforeValue.Substring(BeforeValue.Length - int.Parse(DP_Sub_Start), int.Parse(DP_Sub_Length));
                                                                                        }
                                                                                    }
                                                                                }
                                                                                // Type :
                                                                                switch (DP_TypeCode)
                                                                                {
                                                                                    case "1": //Alphabet and number
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                    case "2": //Only alphabet
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                    case "3": //Only number
                                                                                        {
                                                                                            BeforeValue = BeforeValue.Replace("O", "0");
                                                                                            BeforeValue = BeforeValue.Replace("o", "0");
                                                                                            BeforeValue = BeforeValue.Replace("C", "0");
                                                                                            BeforeValue = BeforeValue.Replace("c", "0");
                                                                                            BeforeValue = BeforeValue.Replace("Q", "0");
                                                                                            BeforeValue = BeforeValue.Replace("e", "0");
                                                                                            BeforeValue = BeforeValue.Replace("U", "0");
                                                                                            BeforeValue = BeforeValue.Replace("u", "0");
                                                                                            BeforeValue = BeforeValue.Replace("*", "0");
                                                                                            BeforeValue = BeforeValue.Replace("(", "0");
                                                                                            BeforeValue = BeforeValue.Replace(")", "0");
                                                                                            BeforeValue = BeforeValue.Replace("n", "0");
                                                                                            BeforeValue = BeforeValue.Replace("!", "1");
                                                                                            BeforeValue = BeforeValue.Replace("L", "1");
                                                                                            BeforeValue = BeforeValue.Replace("l", "1");
                                                                                            BeforeValue = BeforeValue.Replace("I", "1");
                                                                                            BeforeValue = BeforeValue.Replace("i", "1");
                                                                                            BeforeValue = BeforeValue.Replace("z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("Z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("?", "3");
                                                                                            BeforeValue = BeforeValue.Replace("Y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("S", "5");
                                                                                            BeforeValue = BeforeValue.Replace("s", "5");
                                                                                            BeforeValue = BeforeValue.Replace("F", "5");
                                                                                            BeforeValue = BeforeValue.Replace("f", "5");
                                                                                            BeforeValue = BeforeValue.Replace("P", "5");
                                                                                            BeforeValue = BeforeValue.Replace("p", "5");
                                                                                            BeforeValue = BeforeValue.Replace("$", "5");
                                                                                            BeforeValue = BeforeValue.Replace("G", "6");
                                                                                            BeforeValue = BeforeValue.Replace("b", "6");
                                                                                            BeforeValue = BeforeValue.Replace("h", "6");
                                                                                            BeforeValue = BeforeValue.Replace("r", "6");
                                                                                            BeforeValue = BeforeValue.Replace("d", "7");
                                                                                            BeforeValue = BeforeValue.Replace("B", "8");
                                                                                            BeforeValue = BeforeValue.Replace("&", "8");
                                                                                            BeforeValue = BeforeValue.Replace("q", "9");
                                                                                            BeforeValue = BeforeValue.Replace("g", "9");
                                                                                            BeforeValue = BeforeValue.Replace(",", "").Replace("\"", "").Replace(" ", "").Trim();
                                                                                            LastValueResult = BeforeValue.Trim();
                                                                                            break;
                                                                                        }
                                                                                    case "4": //Address
                                                                                        {
                                                                                            break;
                                                                                        }
                                                                                    case "5": //Date with full month name ( D:day - NTHN:month name - M:month - Y:year )
                                                                                        {
                                                                                            BeforeValue = BeforeValue.Replace(DP_Input_Format_Sep, "-");
                                                                                            BeforeValue = BeforeValue.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace(DP_Input_Format_Sep, "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("D", "d");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("NTHN", "MMMM");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("Y", "y");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("m", "M");
                                                                                            try
                                                                                            {
                                                                                                string[] BF = BeforeValue.Split('-');
                                                                                                string[] InF = DP_Input_Format.Split('-');
                                                                                                string InFT = "";
                                                                                                for (int i = 0; i < InF.Length; i++)
                                                                                                {
                                                                                                    InFT = "";
                                                                                                    InFT = InF[i].Trim().ToUpper();
                                                                                                    if (InFT == "MMMM")
                                                                                                    {
                                                                                                        int Mn1 = 0; int Mn2 = 0; int Mn3 = 0; int Mn4 = 0; int Mn5 = 0; int Mn6 = 0; int Mn7 = 0; int Mn8 = 0; int Mn9 = 0; int Mn10 = 0; int Mn11 = 0; int Mn12 = 0;
                                                                                                        Mn1 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JANUARY");
                                                                                                        Mn2 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "FEBRUARY");
                                                                                                        Mn3 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "MARCH");
                                                                                                        Mn4 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "APRIL");
                                                                                                        Mn5 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "MAY");
                                                                                                        Mn6 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JUNE");
                                                                                                        Mn7 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JULY");
                                                                                                        Mn8 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "AUGUST");
                                                                                                        Mn9 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "SEPTEMBER");
                                                                                                        Mn10 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "OCTOBER");
                                                                                                        Mn11 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "NOVEMBER");
                                                                                                        Mn12 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "DECEMBER");
                                                                                                        int MaxRank = 0; int MaxRank_ID = 1; MaxRank = Mn1;
                                                                                                        if (MaxRank <= Mn2) { MaxRank_ID = 2; }
                                                                                                        if (MaxRank <= Mn3) { MaxRank_ID = 3; }
                                                                                                        if (MaxRank <= Mn4) { MaxRank_ID = 4; }
                                                                                                        if (MaxRank <= Mn5) { MaxRank_ID = 5; }
                                                                                                        if (MaxRank <= Mn6) { MaxRank_ID = 6; }
                                                                                                        if (MaxRank <= Mn7) { MaxRank_ID = 7; }
                                                                                                        if (MaxRank <= Mn8) { MaxRank_ID = 8; }
                                                                                                        if (MaxRank <= Mn9) { MaxRank_ID = 9; }
                                                                                                        if (MaxRank <= Mn10) { MaxRank_ID = 10; }
                                                                                                        if (MaxRank <= Mn11) { MaxRank_ID = 11; }
                                                                                                        if (MaxRank <= Mn12) { MaxRank_ID = 12; }
                                                                                                        switch (MaxRank_ID)
                                                                                                        {
                                                                                                            case 1: { BF[i] = "January"; break; }
                                                                                                            case 2: { BF[i] = "February"; break; }
                                                                                                            case 3: { BF[i] = "March"; break; }
                                                                                                            case 4: { BF[i] = "April"; break; }
                                                                                                            case 5: { BF[i] = "May"; break; }
                                                                                                            case 6: { BF[i] = "June"; break; }
                                                                                                            case 7: { BF[i] = "July"; break; }
                                                                                                            case 8: { BF[i] = "August"; break; }
                                                                                                            case 9: { BF[i] = "September"; break; }
                                                                                                            case 10: { BF[i] = "October"; break; }
                                                                                                            case 11: { BF[i] = "November"; break; }
                                                                                                            case 12: { BF[i] = "December"; break; }
                                                                                                        }
                                                                                                    }
                                                                                                    if ((InFT == "D") || (InFT == "DD") || (InFT == "M") || (InFT == "MM") || (InFT == "Y") || (InFT == "YY") || (InFT == "YYY") || (InFT == "YYYY"))
                                                                                                    {
                                                                                                        BF[i] = BF[i].Replace("O", "0");
                                                                                                        BF[i] = BF[i].Replace("o", "0");
                                                                                                        BF[i] = BF[i].Replace("C", "0");
                                                                                                        BF[i] = BF[i].Replace("c", "0");
                                                                                                        BF[i] = BF[i].Replace("Q", "0");
                                                                                                        BF[i] = BF[i].Replace("e", "0");
                                                                                                        BF[i] = BF[i].Replace("U", "0");
                                                                                                        BF[i] = BF[i].Replace("u", "0");
                                                                                                        BF[i] = BF[i].Replace("*", "0");
                                                                                                        BF[i] = BF[i].Replace("(", "0");
                                                                                                        BF[i] = BF[i].Replace(")", "0");
                                                                                                        BF[i] = BF[i].Replace("n", "0");
                                                                                                        BF[i] = BF[i].Replace("!", "1");
                                                                                                        BF[i] = BF[i].Replace("L", "1");
                                                                                                        BF[i] = BF[i].Replace("l", "1");
                                                                                                        BF[i] = BF[i].Replace("I", "1");
                                                                                                        BF[i] = BF[i].Replace("i", "1");
                                                                                                        BF[i] = BF[i].Replace("z", "2");
                                                                                                        BF[i] = BF[i].Replace("Z", "2");
                                                                                                        BF[i] = BF[i].Replace("?", "3");
                                                                                                        BF[i] = BF[i].Replace("Y", "4");
                                                                                                        BF[i] = BF[i].Replace("y", "4");
                                                                                                        BF[i] = BF[i].Replace("S", "5");
                                                                                                        BF[i] = BF[i].Replace("s", "5");
                                                                                                        BF[i] = BF[i].Replace("F", "5");
                                                                                                        BF[i] = BF[i].Replace("f", "5");
                                                                                                        BF[i] = BF[i].Replace("P", "5");
                                                                                                        BF[i] = BF[i].Replace("p", "5");
                                                                                                        BF[i] = BF[i].Replace("$", "5");
                                                                                                        BF[i] = BF[i].Replace("G", "6");
                                                                                                        BF[i] = BF[i].Replace("b", "6");
                                                                                                        BF[i] = BF[i].Replace("h", "6");
                                                                                                        BF[i] = BF[i].Replace("r", "6");
                                                                                                        BF[i] = BF[i].Replace("d", "7");
                                                                                                        BF[i] = BF[i].Replace("B", "8");
                                                                                                        BF[i] = BF[i].Replace("&", "8");
                                                                                                        BF[i] = BF[i].Replace("q", "9");
                                                                                                        BF[i] = BF[i].Replace("g", "9");
                                                                                                        BF[i] = BF[i].Replace(",", "").Replace("\"", "").Replace(" ", "").Trim();
                                                                                                        BF[i] = BF[i].Trim();
                                                                                                    }
                                                                                                }
                                                                                                BeforeValue = "";
                                                                                                DP_Input_Format = "";
                                                                                                for (int i = 0; i < InF.Length; i++)
                                                                                                {
                                                                                                    BeforeValue += "-" + BF[i];
                                                                                                    DP_Input_Format += "-" + InF[i];
                                                                                                }
                                                                                                BeforeValue = BeforeValue.Trim();
                                                                                                DP_Input_Format = DP_Input_Format.Trim();
                                                                                                if (BeforeValue.Substring(0, 1) == "-") { BeforeValue = BeforeValue.Substring(1, BeforeValue.Length - 1); }
                                                                                                if (DP_Input_Format.Substring(0, 1) == "-") { DP_Input_Format = DP_Input_Format.Substring(1, DP_Input_Format.Length - 1); }
                                                                                                BeforeValue = BeforeValue.Trim();
                                                                                                DP_Input_Format = DP_Input_Format.Trim();
                                                                                            }
                                                                                            catch (Exception) { }
                                                                                            LastValueResult = PB.ConvertDate_Format(BeforeValue, DP_Input_Format, DP_Output_Format);
                                                                                            break;
                                                                                        }
                                                                                    case "6": //Date with summary month name ( D:day - NTHN:month name - M:month - Y:year )
                                                                                        {

                                                                                            BeforeValue = BeforeValue.Replace(DP_Input_Format_Sep, "-");
                                                                                            BeforeValue = BeforeValue.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace(DP_Input_Format_Sep, "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("D", "d");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("NTHN", "MMM");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("Y", "y");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("m", "M");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("Y", "y");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("m", "M");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("D", "d");
                                                                                            try
                                                                                            {
                                                                                                string[] BF = BeforeValue.Split('-');
                                                                                                string[] InF = DP_Input_Format.Split('-');
                                                                                                string InFT = "";
                                                                                                for (int i = 0; i < InF.Length; i++)
                                                                                                {
                                                                                                    InFT = "";
                                                                                                    InFT = InF[i].Trim().ToUpper();
                                                                                                    if (InFT == "MMM")
                                                                                                    {
                                                                                                        int Mn1 = 0; int Mn2 = 0; int Mn3 = 0; int Mn4 = 0; int Mn5 = 0; int Mn6 = 0; int Mn7 = 0; int Mn8 = 0; int Mn9 = 0; int Mn10 = 0; int Mn11 = 0; int Mn12 = 0;
                                                                                                        Mn1 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JAN");
                                                                                                        Mn2 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "FEB");
                                                                                                        Mn3 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "MAR");
                                                                                                        Mn4 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "APR");
                                                                                                        Mn5 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "MAY");
                                                                                                        Mn6 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JUN");
                                                                                                        Mn7 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "JUL");
                                                                                                        Mn8 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "AUG");
                                                                                                        Mn9 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "SEP");
                                                                                                        Mn10 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "OCT");
                                                                                                        Mn11 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "NOV");
                                                                                                        Mn12 = SF.GetWordsSimilarity(BF[i].ToUpper().Trim(), "DEC");
                                                                                                        int MaxRank = 0; int MaxRank_ID = 1; MaxRank = Mn1;
                                                                                                        if (MaxRank <= Mn2) { MaxRank_ID = 2; }
                                                                                                        if (MaxRank <= Mn3) { MaxRank_ID = 3; }
                                                                                                        if (MaxRank <= Mn4) { MaxRank_ID = 4; }
                                                                                                        if (MaxRank <= Mn5) { MaxRank_ID = 5; }
                                                                                                        if (MaxRank <= Mn6) { MaxRank_ID = 6; }
                                                                                                        if (MaxRank <= Mn7) { MaxRank_ID = 7; }
                                                                                                        if (MaxRank <= Mn8) { MaxRank_ID = 8; }
                                                                                                        if (MaxRank <= Mn9) { MaxRank_ID = 9; }
                                                                                                        if (MaxRank <= Mn10) { MaxRank_ID = 10; }
                                                                                                        if (MaxRank <= Mn11) { MaxRank_ID = 11; }
                                                                                                        if (MaxRank <= Mn12) { MaxRank_ID = 12; }
                                                                                                        switch (MaxRank_ID)
                                                                                                        {
                                                                                                            case 1: { BF[i] = "Jan"; break; }
                                                                                                            case 2: { BF[i] = "Feb"; break; }
                                                                                                            case 3: { BF[i] = "Mar"; break; }
                                                                                                            case 4: { BF[i] = "Apr"; break; }
                                                                                                            case 5: { BF[i] = "May"; break; }
                                                                                                            case 6: { BF[i] = "Jun"; break; }
                                                                                                            case 7: { BF[i] = "Jul"; break; }
                                                                                                            case 8: { BF[i] = "Aug"; break; }
                                                                                                            case 9: { BF[i] = "Sep"; break; }
                                                                                                            case 10: { BF[i] = "Oct"; break; }
                                                                                                            case 11: { BF[i] = "Nov"; break; }
                                                                                                            case 12: { BF[i] = "Dec"; break; }
                                                                                                        }
                                                                                                    }
                                                                                                    if ((InFT == "D") || (InFT == "DD") || (InFT == "M") || (InFT == "MM") || (InFT == "Y") || (InFT == "YY") || (InFT == "YYY") || (InFT == "YYYY"))
                                                                                                    {
                                                                                                        BF[i] = BF[i].Replace("O", "0");
                                                                                                        BF[i] = BF[i].Replace("o", "0");
                                                                                                        BF[i] = BF[i].Replace("C", "0");
                                                                                                        BF[i] = BF[i].Replace("c", "0");
                                                                                                        BF[i] = BF[i].Replace("Q", "0");
                                                                                                        BF[i] = BF[i].Replace("e", "0");
                                                                                                        BF[i] = BF[i].Replace("U", "0");
                                                                                                        BF[i] = BF[i].Replace("u", "0");
                                                                                                        BF[i] = BF[i].Replace("*", "0");
                                                                                                        BF[i] = BF[i].Replace("(", "0");
                                                                                                        BF[i] = BF[i].Replace(")", "0");
                                                                                                        BF[i] = BF[i].Replace("n", "0");
                                                                                                        BF[i] = BF[i].Replace("!", "1");
                                                                                                        BF[i] = BF[i].Replace("L", "1");
                                                                                                        BF[i] = BF[i].Replace("l", "1");
                                                                                                        BF[i] = BF[i].Replace("I", "1");
                                                                                                        BF[i] = BF[i].Replace("i", "1");
                                                                                                        BF[i] = BF[i].Replace("z", "2");
                                                                                                        BF[i] = BF[i].Replace("Z", "2");
                                                                                                        BF[i] = BF[i].Replace("?", "3");
                                                                                                        BF[i] = BF[i].Replace("Y", "4");
                                                                                                        BF[i] = BF[i].Replace("y", "4");
                                                                                                        BF[i] = BF[i].Replace("S", "5");
                                                                                                        BF[i] = BF[i].Replace("s", "5");
                                                                                                        BF[i] = BF[i].Replace("F", "5");
                                                                                                        BF[i] = BF[i].Replace("f", "5");
                                                                                                        BF[i] = BF[i].Replace("P", "5");
                                                                                                        BF[i] = BF[i].Replace("p", "5");
                                                                                                        BF[i] = BF[i].Replace("$", "5");
                                                                                                        BF[i] = BF[i].Replace("G", "6");
                                                                                                        BF[i] = BF[i].Replace("b", "6");
                                                                                                        BF[i] = BF[i].Replace("h", "6");
                                                                                                        BF[i] = BF[i].Replace("r", "6");
                                                                                                        BF[i] = BF[i].Replace("d", "7");
                                                                                                        BF[i] = BF[i].Replace("B", "8");
                                                                                                        BF[i] = BF[i].Replace("&", "8");
                                                                                                        BF[i] = BF[i].Replace("q", "9");
                                                                                                        BF[i] = BF[i].Replace("g", "9");
                                                                                                        BF[i] = BF[i].Replace(",", "").Replace("\"", "").Replace(" ", "").Trim();
                                                                                                        BF[i] = BF[i].Trim();
                                                                                                    }
                                                                                                }
                                                                                                BeforeValue = "";
                                                                                                DP_Input_Format = "";
                                                                                                for (int i = 0; i < InF.Length; i++)
                                                                                                {
                                                                                                    BeforeValue += "-" + BF[i];
                                                                                                    DP_Input_Format += "-" + InF[i];
                                                                                                }
                                                                                                BeforeValue = BeforeValue.Trim();
                                                                                                DP_Input_Format = DP_Input_Format.Trim();
                                                                                                if (BeforeValue.Substring(0, 1) == "-") { BeforeValue = BeforeValue.Substring(1, BeforeValue.Length - 1); }
                                                                                                if (DP_Input_Format.Substring(0, 1) == "-") { DP_Input_Format = DP_Input_Format.Substring(1, DP_Input_Format.Length - 1); }
                                                                                                BeforeValue = BeforeValue.Trim();
                                                                                                DP_Input_Format = DP_Input_Format.Trim();
                                                                                            }
                                                                                            catch (Exception) { }
                                                                                            LastValueResult = PB.ConvertDate_Format(BeforeValue, DP_Input_Format, DP_Output_Format);
                                                                                            break;
                                                                                        }
                                                                                    case "7": //Date - only number ( D:day - M:month - Y:year )
                                                                                        {
                                                                                            BeforeValue = BeforeValue.Replace("O", "0");
                                                                                            BeforeValue = BeforeValue.Replace("o", "0");
                                                                                            BeforeValue = BeforeValue.Replace("C", "0");
                                                                                            BeforeValue = BeforeValue.Replace("c", "0");
                                                                                            BeforeValue = BeforeValue.Replace("Q", "0");
                                                                                            BeforeValue = BeforeValue.Replace("e", "0");
                                                                                            BeforeValue = BeforeValue.Replace("U", "0");
                                                                                            BeforeValue = BeforeValue.Replace("u", "0");
                                                                                            BeforeValue = BeforeValue.Replace("*", "0");
                                                                                            BeforeValue = BeforeValue.Replace("(", "0");
                                                                                            BeforeValue = BeforeValue.Replace(")", "0");
                                                                                            BeforeValue = BeforeValue.Replace("n", "0");
                                                                                            BeforeValue = BeforeValue.Replace("!", "1");
                                                                                            BeforeValue = BeforeValue.Replace("L", "1");
                                                                                            BeforeValue = BeforeValue.Replace("l", "1");
                                                                                            BeforeValue = BeforeValue.Replace("I", "1");
                                                                                            BeforeValue = BeforeValue.Replace("i", "1");
                                                                                            BeforeValue = BeforeValue.Replace("z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("Z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("?", "3");
                                                                                            BeforeValue = BeforeValue.Replace("Y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("S", "5");
                                                                                            BeforeValue = BeforeValue.Replace("s", "5");
                                                                                            BeforeValue = BeforeValue.Replace("F", "5");
                                                                                            BeforeValue = BeforeValue.Replace("f", "5");
                                                                                            BeforeValue = BeforeValue.Replace("P", "5");
                                                                                            BeforeValue = BeforeValue.Replace("p", "5");
                                                                                            BeforeValue = BeforeValue.Replace("$", "5");
                                                                                            BeforeValue = BeforeValue.Replace("G", "6");
                                                                                            BeforeValue = BeforeValue.Replace("b", "6");
                                                                                            BeforeValue = BeforeValue.Replace("h", "6");
                                                                                            BeforeValue = BeforeValue.Replace("r", "6");
                                                                                            BeforeValue = BeforeValue.Replace("d", "7");
                                                                                            BeforeValue = BeforeValue.Replace("B", "8");
                                                                                            BeforeValue = BeforeValue.Replace("&", "8");
                                                                                            BeforeValue = BeforeValue.Replace("q", "9");
                                                                                            BeforeValue = BeforeValue.Replace("g", "9");
                                                                                            BeforeValue = BeforeValue.Replace(DP_Input_Format_Sep, "-");
                                                                                            BeforeValue = BeforeValue.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace(DP_Input_Format_Sep, "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("--", "-");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("D", "d");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("Y", "y");
                                                                                            DP_Input_Format = DP_Input_Format.Replace("m", "M");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("Y", "y");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("m", "M");
                                                                                            DP_Output_Format = DP_Output_Format.Replace("D", "d");
                                                                                            LastValueResult = PB.ConvertDate_Format(BeforeValue, DP_Input_Format, DP_Output_Format);
                                                                                            break;
                                                                                        }
                                                                                    case "8": //Time ( H:hour - M:minutes - S:seconds
                                                                                        {
                                                                                            BeforeValue = BeforeValue.Replace("O", "0");
                                                                                            BeforeValue = BeforeValue.Replace("o", "0");
                                                                                            BeforeValue = BeforeValue.Replace("C", "0");
                                                                                            BeforeValue = BeforeValue.Replace("c", "0");
                                                                                            BeforeValue = BeforeValue.Replace("Q", "0");
                                                                                            BeforeValue = BeforeValue.Replace("e", "0");
                                                                                            BeforeValue = BeforeValue.Replace("U", "0");
                                                                                            BeforeValue = BeforeValue.Replace("u", "0");
                                                                                            BeforeValue = BeforeValue.Replace("*", "0");
                                                                                            BeforeValue = BeforeValue.Replace("(", "0");
                                                                                            BeforeValue = BeforeValue.Replace(")", "0");
                                                                                            BeforeValue = BeforeValue.Replace("n", "0");
                                                                                            BeforeValue = BeforeValue.Replace("!", "1");
                                                                                            BeforeValue = BeforeValue.Replace("L", "1");
                                                                                            BeforeValue = BeforeValue.Replace("l", "1");
                                                                                            BeforeValue = BeforeValue.Replace("I", "1");
                                                                                            BeforeValue = BeforeValue.Replace("i", "1");
                                                                                            BeforeValue = BeforeValue.Replace("z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("Z", "2");
                                                                                            BeforeValue = BeforeValue.Replace("?", "3");
                                                                                            BeforeValue = BeforeValue.Replace("Y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("y", "4");
                                                                                            BeforeValue = BeforeValue.Replace("S", "5");
                                                                                            BeforeValue = BeforeValue.Replace("s", "5");
                                                                                            BeforeValue = BeforeValue.Replace("F", "5");
                                                                                            BeforeValue = BeforeValue.Replace("f", "5");
                                                                                            BeforeValue = BeforeValue.Replace("P", "5");
                                                                                            BeforeValue = BeforeValue.Replace("p", "5");
                                                                                            BeforeValue = BeforeValue.Replace("$", "5");
                                                                                            BeforeValue = BeforeValue.Replace("G", "6");
                                                                                            BeforeValue = BeforeValue.Replace("b", "6");
                                                                                            BeforeValue = BeforeValue.Replace("h", "6");
                                                                                            BeforeValue = BeforeValue.Replace("r", "6");
                                                                                            BeforeValue = BeforeValue.Replace("d", "7");
                                                                                            BeforeValue = BeforeValue.Replace("B", "8");
                                                                                            BeforeValue = BeforeValue.Replace("&", "8");
                                                                                            BeforeValue = BeforeValue.Replace("q", "9");
                                                                                            BeforeValue = BeforeValue.Replace("g", "9");
                                                                                            LastValueResult = PB.ConvertDate_Format(BeforeValue, DP_Input_Format, DP_Output_Format);
                                                                                            break;
                                                                                        }
                                                                                }
                                                                                ORE.Element_Value = LastValueResult.Trim();
                                                                            }
                                                                            catch (Exception) { }
                                                                        }
                                                                        RowN++;
                                                                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Insert Into Users_16_API_Acuant_Result Values ('" + TransactionID + "','" + RowN + "','" + ORE.Element_Key.Trim() + "','" + ORE.Element_Value.Trim() + "')");
                                                                    }
                                                                }
                                                            }
                                                            // Rotation Image :
                                                            string IMG_Rotation_F = "0";
                                                            string IMG_Rotation_B = "0";
                                                            try
                                                            {
                                                                if ((OCR_Last_Front.ParsedResults != null) && (OCR_Last_Back.ParsedResults == null))
                                                                {
                                                                    IMG_Rotation_F = OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                    IMG_Rotation_B = OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                }
                                                            }
                                                            catch (Exception)
                                                            {
                                                                IMG_Rotation_F = "0";
                                                                IMG_Rotation_B = "0";
                                                            }
                                                            try
                                                            {
                                                                if ((OCR_Last_Back.ParsedResults != null) && (OCR_Last_Front.ParsedResults == null))
                                                                {
                                                                    IMG_Rotation_F = OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                    IMG_Rotation_B = OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                }
                                                            }
                                                            catch (Exception)
                                                            {
                                                                IMG_Rotation_F = "0";
                                                                IMG_Rotation_B = "0";
                                                            }
                                                            try
                                                            {
                                                                if ((OCR_Last_Front.ParsedResults != null) && (OCR_Last_Back.ParsedResults != null))
                                                                {
                                                                    IMG_Rotation_F = OCR_Last_Front.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                    IMG_Rotation_B = OCR_Last_Back.ParsedResults[0].TextOrientation.ToString().Trim();
                                                                }
                                                            }
                                                            catch (Exception)
                                                            {
                                                                IMG_Rotation_F = "0";
                                                                IMG_Rotation_B = "0";
                                                            }
                                                            if (IMG_Nor_Tag_Front == true)
                                                            {
                                                                if (FrontIMage_Rotated == false)
                                                                {
                                                                    Img_Front_Nor = ImageRotation((Bitmap)Img_Front_Nor, IMG_Rotation_F);
                                                                }
                                                            }
                                                            if (IMG_Nor_Tag_Back == true)
                                                            {
                                                                if (BackIMage_Rotated == false)
                                                                {
                                                                    Img_Back_Nor = ImageRotation((Bitmap)Img_Back_Nor, IMG_Rotation_B);
                                                                }
                                                            }
                                                            // Crop X, Y, Width And Height Coeficion :
                                                            Rectangle RFI = new Rectangle(0, 0, 0, 0);
                                                            Rectangle RBI = new Rectangle(0, 0, 0, 0);
                                                            try
                                                            {
                                                                if ((OCR_Last_Front.ParsedResults != null) && (OCR_Last_Back.ParsedResults == null))
                                                                {
                                                                    DataTable DTIS = new DataTable();
                                                                    DTIS = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Img_W,Img_H From Template_07_Images Where (DID = '" + LST_Template.TemplateID + "') And (ImageID = '1')");
                                                                    try
                                                                    {
                                                                        double FIW = double.Parse(DTIS.Rows[0][0].ToString().Trim());
                                                                        double FIH = double.Parse(DTIS.Rows[0][1].ToString().Trim());
                                                                        RFI.X = (int)LST_Template.FrontImage_X_TopLeft_Refrence;
                                                                        RFI.Y = (int)LST_Template.FrontImage_Y_TopLeft_Refrence;
                                                                        RFI.Width = (int)(FIW * LST_Template.FrontImage_X_Coeficient);
                                                                        RFI.Height = (int)(FIH * LST_Template.FrontImage_Y_Coeficient);
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    RBI = RFI;
                                                                }
                                                            }
                                                            catch (Exception) { }
                                                            try
                                                            {
                                                                if ((OCR_Last_Back.ParsedResults != null) && (OCR_Last_Front.ParsedResults == null))
                                                                {
                                                                    DataTable DTIS = new DataTable();
                                                                    DTIS = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Img_W,Img_H From Template_07_Images Where (DID = '" + LST_Template.TemplateID + "') And (ImageID = '2')");
                                                                    try
                                                                    {
                                                                        double BIW = double.Parse(DTIS.Rows[0][0].ToString().Trim());
                                                                        double BIH = double.Parse(DTIS.Rows[0][1].ToString().Trim());
                                                                        RBI.X = (int)LST_Template.BackImage_X_TopLeft_Refrence;
                                                                        RBI.Y = (int)LST_Template.BackImage_Y_TopLeft_Refrence;
                                                                        RBI.Width = (int)(BIW * LST_Template.BackImage_X_Coeficient);
                                                                        RBI.Height = (int)(BIH * LST_Template.BackImage_Y_Coeficient);
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    RFI = RBI;
                                                                }
                                                            }
                                                            catch (Exception) { }
                                                            try
                                                            {
                                                                if ((OCR_Last_Front.ParsedResults != null) && (OCR_Last_Back.ParsedResults != null))
                                                                {
                                                                    DataTable DTIS = new DataTable();
                                                                    DTIS = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Img_W,Img_H From Template_07_Images Where (DID = '" + LST_Template.TemplateID + "') And (ImageID = '1')");
                                                                    try
                                                                    {
                                                                        double FIW = double.Parse(DTIS.Rows[0][0].ToString().Trim());
                                                                        double FIH = double.Parse(DTIS.Rows[0][1].ToString().Trim());
                                                                        RFI.X = (int)LST_Template.FrontImage_X_TopLeft_Refrence;
                                                                        RFI.Y = (int)LST_Template.FrontImage_Y_TopLeft_Refrence;
                                                                        RFI.Width = (int)(FIW * LST_Template.FrontImage_X_Coeficient);
                                                                        RFI.Height = (int)(FIH * LST_Template.FrontImage_Y_Coeficient);
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                    DTIS = new DataTable();
                                                                    DTIS = SQ.Get_DTable_TSQL(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Select Img_W,Img_H From Template_07_Images Where (DID = '" + LST_Template.TemplateID + "') And (ImageID = '2')");
                                                                    try
                                                                    {
                                                                        double BIW = double.Parse(DTIS.Rows[0][0].ToString().Trim());
                                                                        double BIH = double.Parse(DTIS.Rows[0][1].ToString().Trim());
                                                                        RBI.X = (int)LST_Template.BackImage_X_TopLeft_Refrence;
                                                                        RBI.Y = (int)LST_Template.BackImage_Y_TopLeft_Refrence;
                                                                        RBI.Width = (int)(BIW * LST_Template.BackImage_X_Coeficient);
                                                                        RBI.Height = (int)(BIH * LST_Template.BackImage_Y_Coeficient);
                                                                    }
                                                                    catch (Exception)
                                                                    { }
                                                                }
                                                            }
                                                            catch (Exception) { }
                                                            // Crop Image :
                                                            try
                                                            {
                                                                if (IMG_Nor_Tag_Front == true) { Img_Front_Nor = CropImage(Img_Front_Nor, RFI); }
                                                            }
                                                            catch (Exception) { }
                                                            try
                                                            {
                                                                if (IMG_Nor_Tag_Back == true) { Img_Back_Nor = CropImage(Img_Back_Nor, RBI); }
                                                            }
                                                            catch (Exception) { }
                                                            // Save Result Images :
                                                            string SIMGP = "";
                                                            if (IMG_Nor_Tag_Front == true)
                                                            {
                                                                try
                                                                {
                                                                    SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + FrontImageName);
                                                                    Img_Front_Nor.Save(SIMGP, ImageFormat.Jpeg);
                                                                }
                                                                catch (Exception) { }
                                                            }
                                                            if (IMG_Nor_Tag_Back == true)
                                                            {
                                                                try
                                                                {
                                                                    SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + BackImageName);
                                                                    Img_Back_Nor.Save(SIMGP, ImageFormat.Jpeg);
                                                                }
                                                                catch (Exception) { }
                                                            }
                                                            // Save Face Image :
                                                            if (FaceDetected == true)
                                                            {
                                                                try
                                                                {
                                                                    string Image3_Name = Tra_ID + "I3" + PB.Make_Security_Code(20);
                                                                    Image Selected_IMG_LC = null;
                                                                    Rectangle rct = FaceDetector_FB(Selected_IMG, ref Selected_IMG_LC);
                                                                    Image FCIMG = CropImage(Selected_IMG_LC, rct);
                                                                    var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + Image3_Name + ".jpg");
                                                                    FCIMG.Save(filePath, ImageFormat.Jpeg);
                                                                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_3_Title] = 'Face photo',[Image_3_File_Name] = '" + Image3_Name + "',[Image_3_File_Format] = 'jpg',[Image_3_File_Type] = 'image/jpeg',[Image_3_Download_ID] = '" + Image3_Name + "',[Image_3_Download_Count] = '0' Where (ID = '" + Tra_ID + "')");
                                                                }
                                                                catch (Exception)
                                                                { }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            // Template Rank no Accept
                                                            SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Template rank failed' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                                        }
                                                    }
                                                    else
                                                    {   // Template not found
                                                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Template not found' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                                    }
                                                }
                                                else
                                                {   // OCR Return Error Message
                                                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = '" + OCRRes.ParsedResults[0].ErrorMessage.Trim() + "$" + OCRRes.ParsedResults[0].ErrorDetails.Trim() + "' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                                }
                                            }
                                            else
                                            {   // Processing Error
                                                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Is errored on processing' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                            }
                                        }
                                        else
                                        {   // Exit Code Error
                                            SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Exit code : " + OCRRes.OCRExitCode.ToString() + "' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                        }
                                    }
                                    else
                                    {   // No Face Detected or Back Image Not Recognized.
                                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Images not valid' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                                    }
                                }
                                else
                                {   // Applicarion Don't Have Front or Back Image For OCR.
                                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'OCR image not found' Where (ID = '" + TransactionID + "') And (Removed = '0')");
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
                    else
                    {   // DataTable Return No 1 Record For Template Searching Code.
                        SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return ununic row setting' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                    }
                }
                else
                {   // DataTable Return null Record For Template Searching Code.
                    SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'DT return null etting' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                }
            }
            catch (Exception e)
            {   // Structure Error.
                SQ.Execute_TSql(iCore_Administrator.Modules.DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = '" + e.Message.Trim().Replace(",", "").Replace(";", "").Replace("'", "") + "' Where (ID = '" + TransactionID + "') And (Removed = '0')");
            }
        }
        //-----------------------------------------------------------------------------------------------------
        //-----------------------------------------------------------------------------------------------------
    }
}