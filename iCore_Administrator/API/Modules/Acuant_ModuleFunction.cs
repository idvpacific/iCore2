using AssureTec.AssureID.Web.SDK;
using iCore_Administrator.Modules;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace iCore_Administrator.API.Modules
{
    public class Acuant_ModuleFunction
    {
        //====================================================================================================================
        SQL_Tranceiver SQ = new SQL_Tranceiver();
        PublicFunctions PB = new PublicFunctions();
        //====================================================================================================================
        string Username = "";
        string Password = "";
        string AssureIdConnectEndpoint = "";
        Guid _subscriptionId;
        private AssureIDServiceClient _assureIdServiceClient;
        private const string Manufacturer = "xxx";
        private const string Model = "xxx";
        private const string SerialNumber = "xxx";
        private const bool HasMagneticStripeReader = false;
        private const bool HasContactlessChipReader = false;
        private DeviceType _deviceType;
        private DeviceInfo _deviceInfo;
        private DocumentSettings _documentSettings;
        private SensorType? _sensorType = null;
        private CroppingExpectedSize? _croppingExpectedSize = null;
        private CroppingMode? _croppingMode = null;
        //====================================================================================================================
        public AssureIDServiceClient AssureIdServiceClient(string endpoint, string username, string password)
        {
            try
            {
                var serverAddress = new Uri(endpoint);
                var networkCredential = new NetworkCredential(username, password);
                return new AssureIDServiceClient(serverAddress, networkCredential);
            }
            catch (Exception)
            {
                return null;
            }
        }
        //====================================================================================================================
        private bool CheckSubscription()
        {
            try
            {
                var subscriptions = _assureIdServiceClient.GetSubscriptions(false);
                if (subscriptions.Length > 0)
                {
                    var isSubscriptionActive = subscriptions.Any(s => s.Id == _subscriptionId);

                    if (isSubscriptionActive)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        //====================================================================================================================
        public void GetData(string TransactionID)
        {
            try
            {
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Setting_Basic_03_AssureID");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        DataTable DT_Tran = new DataTable();
                        DT_Tran = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Users_15_API_Transaction Where (ID = '" + TransactionID + "') And (Removed = '0')");
                        if (DT_Tran.Rows != null)
                        {
                            if (DT_Tran.Rows.Count == 1)
                            {
                                string Tra_ID = DT_Tran.Rows[0][0].ToString().Trim();
                                string FrontImageName = DT_Tran.Rows[0][52].ToString().Trim() + "." + DT_Tran.Rows[0][50].ToString().Trim();
                                string BackImageName = DT_Tran.Rows[0][62].ToString().Trim() + "." + DT_Tran.Rows[0][60].ToString().Trim();
                                Username = DT.Rows[0][0].ToString().Trim();
                                Password = DT.Rows[0][1].ToString().Trim();
                                AssureIdConnectEndpoint = DT.Rows[0][2].ToString().Trim();
                                _subscriptionId = new Guid(DT.Rows[0][3].ToString().Trim());
                                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(AssureIdConnectEndpoint) && _subscriptionId != Guid.Empty) { _assureIdServiceClient = AssureIdServiceClient(AssureIdConnectEndpoint, Username, Password); if (!CheckSubscription()) { return; } } else { return; }
                                switch(DT_Tran.Rows[0][16].ToString().Trim())
                                {
                                    case "0": { _sensorType = SensorType.Unknown; break; }
                                    case "1": { _sensorType = SensorType.Camera; break; }
                                    case "2": { _sensorType = SensorType.Scanner; break; }
                                    case "3": { _sensorType = SensorType.Mobile; break; }
                                }
                                switch (DT_Tran.Rows[0][18].ToString().Trim())
                                {
                                    case "0": { _croppingMode = CroppingMode.None; break; }
                                    case "1": { _croppingMode = CroppingMode.Automatic; break; }
                                    case "2": { _croppingMode = CroppingMode.Interactive; break; }
                                    case "3": { _croppingMode = CroppingMode.Always; break; }
                                }
                                switch (DT_Tran.Rows[0][5].ToString().Trim())
                                {
                                    case "1": { _croppingExpectedSize = CroppingExpectedSize.ID1; break; }
                                    case "2": { _croppingExpectedSize = CroppingExpectedSize.ID1; break; }
                                    case "3": { _croppingExpectedSize = CroppingExpectedSize.ID3; break; }
                                    case "4": { _croppingExpectedSize = CroppingExpectedSize.ID3; break; }
                                }
                                _deviceType = new DeviceType(Manufacturer, Model, (SensorType)_sensorType);
                                _deviceInfo = new DeviceInfo(_deviceType, SerialNumber, HasMagneticStripeReader, HasContactlessChipReader);
                                _documentSettings = new DocumentSettings(_deviceInfo);
                                _documentSettings.ImageCroppingExpectedSize = (CroppingExpectedSize)_croppingExpectedSize;
                                _documentSettings.ImageCroppingMode = (CroppingMode)_croppingMode;
                                _documentSettings.SubscriptionId = _subscriptionId;
                                switch (DT_Tran.Rows[0][5].ToString().Trim())
                                {
                                    case "1": { _documentSettings.ProcessMode = DocumentProcessMode.CaptureData; break; }
                                    case "2": { _documentSettings.ProcessMode = DocumentProcessMode.Authenticate; break; }
                                    case "3": { _documentSettings.ProcessMode = DocumentProcessMode.CaptureData; break; }
                                    case "4": { _documentSettings.ProcessMode = DocumentProcessMode.Authenticate; break; }
                                }
                                var documentInstanceId = _assureIdServiceClient.PostDocumentInstance(_documentSettings);
                                DocumentSide documentSide;
                                LightSource lightSource;
                                // Ready Image :
                                string BaseFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID);
                                if (Directory.Exists(BaseFolder) == false) { Directory.CreateDirectory(BaseFolder); }
                                if (Directory.Exists(BaseFolder + "\\" + "Upload") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Upload"); }
                                if (Directory.Exists(BaseFolder + "\\" + "Result") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Result"); }
                                var filePathfront = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Upload/" + FrontImageName);
                                var filePathback = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Upload/" + BackImageName);
                                bool WI_F_F = false; string WI_F_A = "";
                                bool WI_B_F = false; string WI_B_A = "";
                                var filePath1 = filePathfront;
                                if (File.Exists(filePath1) == true) { WI_F_F = true; WI_F_A = filePath1; }
                                var filePath2 = filePathback;
                                if (File.Exists(filePath2) == true) { WI_B_F = true; WI_B_A = filePath2; }
                                if ((WI_F_F == false) && (WI_B_F == false)) { return; }
                                if (WI_F_F) { documentSide = DocumentSide.Front; lightSource = LightSource.White; var bitmap = new Bitmap(WI_F_A); { _assureIdServiceClient.PostDocumentImage(documentInstanceId, documentSide, lightSource, bitmap); } }
                                if (WI_B_F) { documentSide = DocumentSide.Back; lightSource = LightSource.White; using (var bitmap = new Bitmap(WI_B_A)) { _assureIdServiceClient.PostDocumentImage(documentInstanceId, documentSide, lightSource, bitmap); } }
                                var document = _assureIdServiceClient.GetDocument(documentInstanceId);
                                var result = document.Result;
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_16_API_Acuant_Result Where (Transaction_ID = '" + Tra_ID + "')");
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_17_API_Acuant_Alert Where (Transaction_ID = '" + Tra_ID + "')");
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_18_API_Acuant_Authentication Where (Transaction_ID = '" + Tra_ID + "')");
                                if (document.Alerts.Length > 0)
                                {
                                    foreach (var alert in document.Alerts)
                                    {
                                        if (alert.Result == result)
                                        {
                                            try
                                            {
                                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_17_API_Acuant_Alert Values ('" + Tra_ID + "','" + alert.Key.Trim() + "')");
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                }
                                int DocCounter = 0;
                                if (document.Fields.Length > 0)
                                {
                                    foreach (var field in document.Fields)
                                    {
                                        if (field.Name.ToLower() == "photo" || field.Name.ToLower() == "signature")
                                        {
                                            switch (field.Name.ToLower())
                                            {
                                                case "photo":
                                                    {
                                                        try
                                                        {
                                                            string Image3_Name = Tra_ID + "I3" + PB.Make_Security_Code(20);
                                                            Bitmap FIMG = _assureIdServiceClient.GetDocumentFieldImage(document.InstanceId, "Photo");
                                                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + Image3_Name + ".jpg");
                                                            FIMG.Save(filePath, ImageFormat.Jpeg);
                                                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_3_Title] = 'Face photo',[Image_3_File_Name] = '" + Image3_Name + "',[Image_3_File_Format] = 'jpg',[Image_3_File_Type] = 'image/jpeg',[Image_3_Download_ID] = '" + Image3_Name + "',[Image_3_Download_Count] = '0' Where (ID = '" + Tra_ID + "')");
                                                        }
                                                        catch (Exception) { }
                                                        break;
                                                    }
                                                case "signature":
                                                    {
                                                        try
                                                        {
                                                            string Image4_Name = Tra_ID + "I4" + PB.Make_Security_Code(20);
                                                            Bitmap SIMG = _assureIdServiceClient.GetDocumentFieldImage(document.InstanceId, "Signature");
                                                            var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + Image4_Name + ".jpg");
                                                            SIMG.Save(filePath, ImageFormat.Jpeg);
                                                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Image_4_Title] = 'Signature',[Image_4_File_Name] = '" + Image4_Name + "',[Image_4_File_Format] = 'jpg',[Image_4_File_Type] = 'image/jpeg',[Image_4_Download_ID] = '" + Image4_Name + "',[Image_4_Download_Count] = '0' Where (ID = '" + Tra_ID + "')");
                                                        }
                                                        catch (Exception) { } 
                                                        break;
                                                    }
                                            }
                                        }
                                        else
                                        {
                                            DocCounter++;
                                            try
                                            {
                                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_16_API_Acuant_Result Values ('" + Tra_ID + "','" + DocCounter.ToString() + "','" + field.Name.Trim() + "','" + field.Value.ToString().Trim() + "')");
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    try
                                    {
                                        if (document.Classification.Type.FullName.ToString().Trim() != "")
                                        {
                                            DocCounter++;
                                            try
                                            {
                                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_16_API_Acuant_Result Values ('" + Tra_ID + "','" + DocCounter.ToString() + "','Document Type','" + document.Classification.Type.FullName.ToString().Trim() + "')");
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    catch (Exception) { }
                                }
                                // Save Last Image to Result Folder :
                                bool VF = false; bool VB = false;
                                Image VFI = null; Image VBI = null;
                                string FileNameIMG = "";
                                FileNameIMG = filePathfront;
                                if (File.Exists(FileNameIMG) == true) { VF = true; VFI = new Bitmap(FileNameIMG); }
                                FileNameIMG = filePathback;
                                if (File.Exists(FileNameIMG) == true) { VB = true; VBI = new Bitmap(FileNameIMG); }
                                if (document.Classification.OrientationChanged == true)
                                {
                                    try { VFI.RotateFlip(RotateFlipType.Rotate180FlipNone); } catch (Exception) { }
                                    try { VBI.RotateFlip(RotateFlipType.Rotate180FlipNone); } catch (Exception) { }
                                }
                                string SIMGP = "";
                                if (document.Classification.PresentationChanged == false)
                                {
                                    if (VF == true)
                                    {
                                        SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + FrontImageName);
                                        VFI.Save(SIMGP, ImageFormat.Jpeg);
                                    }
                                    if (VB == true)
                                    {
                                        SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" +BackImageName);
                                        VBI.Save(SIMGP, ImageFormat.Jpeg);
                                    }
                                }
                                else
                                {
                                    if (VF == true)
                                    {
                                        SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + BackImageName);
                                        VFI.Save(SIMGP, ImageFormat.Jpeg);
                                    }
                                    if (VB == true)
                                    {
                                        SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Users/API/" + Tra_ID + "/Result/" + FrontImageName);
                                        VBI.Save(SIMGP, ImageFormat.Jpeg);
                                    }
                                }
                                // Set Application Status :
                                if (result == AuthenticationResult.Unknown)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','0','Unknown')");
                                }
                                if (result == AuthenticationResult.Passed)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','1','Passed')");
                                }
                                if (result == AuthenticationResult.Failed)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','2','Failed')");
                                }
                                if (result == AuthenticationResult.Skipped)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','3','Skipped')");
                                }
                                if (result == AuthenticationResult.Caution)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','4','Caution')");
                                }
                                if (result == AuthenticationResult.Attention)
                                {
                                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_18_API_Acuant_Authentication Values ('" + Tra_ID + "','5','Attention')");
                                }
                                return;
                            }
                            else
                            {
                                SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Transaction data information not founded' Where (ID = '" + TransactionID  + "') And (Removed = '0')");
                                return;
                            }
                        }
                        else
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Transaction data information not founded' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                            return;
                        }
                    }
                    else
                    {
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Acuant IDV account configuration not valid' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                        return;
                    }
                }
                else
                {
                    SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = 'Acuant IDV account configuration not valid' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                    return;
                }
            }
            catch (Exception e) 
            {
                SQ.Execute_TSql(DataBase_Selector.Administrator, "Update Users_15_API_Transaction Set [Status_Code] = '3',[Status_Text] = 'Failed',[Error_Message] = '" + e.Message.Trim().Replace(",","").Replace(";","").Replace("'","") + "' Where (ID = '" + TransactionID + "') And (Removed = '0')");
                return; 
            }
        }
        //====================================================================================================================
    }
}