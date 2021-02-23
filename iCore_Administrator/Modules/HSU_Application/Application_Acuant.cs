using AssureTec.AssureID.Web.SDK;
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

namespace iCore_Administrator.Modules.HSU_Application
{
    public class Application_Acuant
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
        public void GetData(string AppID, string Element_ID, string Front_Image_ID, string Back_Image_ID, bool Authen, CroppingExpectedSize Acuant_DocumentType)
        {
            try
            {
                DataTable DT = new DataTable();
                DT = SQ.Get_DTable_TSQL(DataBase_Selector.Administrator, "Select * From Setting_Basic_03_AssureID");
                if (DT.Rows != null)
                {
                    if (DT.Rows.Count == 1)
                    {
                        Username = DT.Rows[0][0].ToString().Trim();
                        Password = DT.Rows[0][1].ToString().Trim();
                        AssureIdConnectEndpoint = DT.Rows[0][2].ToString().Trim();
                        _subscriptionId = new Guid(DT.Rows[0][3].ToString().Trim());
                        if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(AssureIdConnectEndpoint) && _subscriptionId != Guid.Empty) { _assureIdServiceClient = AssureIdServiceClient(AssureIdConnectEndpoint, Username, Password); if (!CheckSubscription()) { return; } }
                        else { return; }
                        _sensorType = SensorType.Unknown;
                        _croppingMode = CroppingMode.Automatic;
                        _croppingExpectedSize = Acuant_DocumentType;
                        _deviceType = new DeviceType(Manufacturer, Model, (SensorType)_sensorType);
                        _deviceInfo = new DeviceInfo(_deviceType, SerialNumber, HasMagneticStripeReader, HasContactlessChipReader);
                        _documentSettings = new DocumentSettings(_deviceInfo);
                        _documentSettings.ImageCroppingExpectedSize = (CroppingExpectedSize)_croppingExpectedSize;
                        _documentSettings.ImageCroppingMode = (CroppingMode)_croppingMode;
                        _documentSettings.SubscriptionId = _subscriptionId;
                        if (Authen == true) { _documentSettings.ProcessMode = DocumentProcessMode.Authenticate; } else { _documentSettings.ProcessMode = DocumentProcessMode.CaptureData; }
                        var documentInstanceId = _assureIdServiceClient.PostDocumentInstance(_documentSettings);
                        DocumentSide documentSide;
                        LightSource lightSource;
                        // Ready Image :
                        string BaseFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID);
                        if (Directory.Exists(BaseFolder) == false) { Directory.CreateDirectory(BaseFolder); }
                        if (Directory.Exists(BaseFolder + "\\" + "Scanned") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Scanned"); }
                        if (Directory.Exists(BaseFolder + "\\" + "Result") == false) { Directory.CreateDirectory(BaseFolder + "\\" + "Result"); }
                        var filePathfront = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/" + Front_Image_ID + ".IDV");
                        var filePathback = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/" + Back_Image_ID + ".IDV");
                        var filePathfront_Last = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Scanned/" + Front_Image_ID + ".jpg");
                        var filePathback_Last = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Scanned/" + Back_Image_ID + ".jpg");
                        if (File.Exists(filePathfront) == true) { File.Copy(filePathfront, filePathfront_Last); }
                        if (File.Exists(filePathback) == true) { File.Copy(filePathback, filePathback_Last); }
                        bool WI_F_F = false; string WI_F_A = "";
                        bool WI_B_F = false; string WI_B_A = "";
                        var filePath1 = filePathfront_Last;
                        if (File.Exists(filePath1) == true) { WI_F_F = true; WI_F_A = filePath1; }
                        var filePath2 = filePathback_Last;
                        if (File.Exists(filePath2) == true) { WI_B_F = true; WI_B_A = filePath2; }
                        if ((WI_F_F == false) && (WI_B_F == false)) { return; }
                        if (WI_F_F) { documentSide = DocumentSide.Front; lightSource = LightSource.White; var bitmap = new Bitmap(WI_F_A); { _assureIdServiceClient.PostDocumentImage(documentInstanceId, documentSide, lightSource, bitmap); } }
                        if (WI_B_F) { documentSide = DocumentSide.Back; lightSource = LightSource.White; using (var bitmap = new Bitmap(WI_B_A)) { _assureIdServiceClient.PostDocumentImage(documentInstanceId, documentSide, lightSource, bitmap); } }
                        var document = _assureIdServiceClient.GetDocument(documentInstanceId);
                        var result = document.Result;
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_10_Hospitality_SingleUser_Application_Acuant_Alert Where (App_ID = '" + AppID + "') And (Element_ID = '" + Element_ID + "')");
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_11_Hospitality_SingleUser_Application_Acuant_Result Where (App_ID = '" + AppID + "') And (Element_ID = '" + Element_ID + "')");
                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Delete From Users_12_Hospitality_SingleUser_Application_Validation Where (App_ID = '" + AppID + "') And (Element_ID = '" + Element_ID + "')");
                        if (document.Alerts.Length > 0)
                        {
                            foreach (var alert in document.Alerts)
                            {
                                if (alert.Result == result)
                                {
                                    try
                                    {
                                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_10_Hospitality_SingleUser_Application_Acuant_Alert Values ('" + AppID + "','" + Element_ID + "','" + alert.Key.Trim() + "')");
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
                                if (field.Name == "Photo" || field.Name == "Signature")
                                {
                                    switch (field.Name)
                                    {
                                        case "Photo":
                                            {
                                                try
                                                {
                                                    Bitmap FIMG = _assureIdServiceClient.GetDocumentFieldImage(document.InstanceId, "Photo");
                                                    var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Result/" + Front_Image_ID + "_Face" + ".png");
                                                    FIMG.Save(filePath, ImageFormat.Jpeg);
                                                }
                                                catch (Exception) { }
                                                break;
                                            }
                                        case "Signature":
                                            {
                                                Bitmap SIMG = _assureIdServiceClient.GetDocumentFieldImage(document.InstanceId, "Signature");
                                                var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/" + AppID + "/Result/" + Front_Image_ID + "_Signature" + ".png");
                                                SIMG.Save(filePath, ImageFormat.Jpeg);
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    DocCounter++;
                                    try
                                    {
                                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_11_Hospitality_SingleUser_Application_Acuant_Result Values ('" + AppID + "','" + Element_ID + "','" + DocCounter.ToString() + "','" + field.Name.Trim() + "','" + field.Value.ToString().Trim() + "')");
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
                                        SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_11_Hospitality_SingleUser_Application_Acuant_Result Values ('" + AppID + "','" + Element_ID + "','" + DocCounter.ToString() + "','Document Type','" + document.Classification.Type.FullName.ToString().Trim() + "')");
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
                        FileNameIMG = filePathfront_Last;
                        if (File.Exists(FileNameIMG) == true) { VF = true; VFI = new Bitmap(FileNameIMG); }
                        FileNameIMG = filePathback_Last;
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
                                SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Result/" + Front_Image_ID + ".jpg");
                                VFI.Save(SIMGP, ImageFormat.Jpeg);
                            }
                            if (VB == true)
                            {
                                SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Result/" + Back_Image_ID + ".jpg");
                                VBI.Save(SIMGP, ImageFormat.Jpeg);
                            }
                        }
                        else
                        {
                            if (VF == true)
                            {
                                SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Result/"  + Back_Image_ID +  ".jpg");
                                VFI.Save(SIMGP, ImageFormat.Jpeg);
                            }
                            if (VB == true)
                            {
                                SIMGP = System.Web.Hosting.HostingEnvironment.MapPath("~/Drive/Hospitality/CustomerData/" + AppID + "/Result/"  + Front_Image_ID +  ".jpg");
                                VBI.Save(SIMGP, ImageFormat.Jpeg);
                            }
                        }
                        // Set Application Status :
                        if (result == AuthenticationResult.Unknown)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','1','Unknown')");
                        }
                        if (result == AuthenticationResult.Attention)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','2','Attention')");
                        }
                        if (result == AuthenticationResult.Caution)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','3','Caution')");
                        }
                        if (result == AuthenticationResult.Skipped)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','4','Skipped')");
                        }
                        if (result == AuthenticationResult.Failed)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','5','Failed')");
                        }
                        if (result == AuthenticationResult.Passed)
                        {
                            SQ.Execute_TSql(DataBase_Selector.Administrator, "Insert Into Users_12_Hospitality_SingleUser_Application_Validation Values ('" + AppID + "','" + Element_ID + "','6','Passed')");
                        }
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception) { return; }
        }
        //====================================================================================================================
    }
}