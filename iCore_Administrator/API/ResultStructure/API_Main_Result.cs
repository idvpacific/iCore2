using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iCore_Administrator.API.ResultStructure
{
    public class API_Main_Result
    {
        public object Result { get; set; }
        public API_Mesaage Message { get; set; }
    }

    public class API_Mesaage
    {
        public string Transaction_ID{ get; set; }
        public int Code { get; set; }
        public bool Error { get; set; }
        public string Description { get; set; }
    }

    public class Transaction_CLS
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public int Status_Code { get; set; }
        public string Status_Result { get; set; }
        public string Username { get; set; }
        public string Date_Format { get; set; }
        public string Request_Date { get; set; }
        public string Request_Time { get; set; }
        public string Request_IP { get; set; }
        public int Attached_File { get; set; }
        public bool Async { get; set; }
        public string Callback_URL { get; set; }
    }

    public class Transaction_CLS_Passport
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public int Status_Code { get; set; }
        public string Status_Result { get; set; }
        public string Username { get; set; }
        public string Date_Format { get; set; }
        public string Request_Date { get; set; }
        public string Request_Time { get; set; }
        public string Request_IP { get; set; }
        public int Attached_File { get; set; }
        public bool Async { get; set; }
        public string Callback_URL { get; set; }
        public string Processing_Type { get; set; }
        public string Document_Validation { get; set; }
        public string Overlay_Required { get; set; }
        public string Detect_Orientation { get; set; }
        public string Image_Scale { get; set; }
        public string Cropping_Mode { get; set; }
    }

    public class Transaction_File
    {
        public object Upload { get; set; }
        public object Processed { get; set; }
    }

    public class TransactionApplication_CLS
    {
        public string ID { get; set; }
        public string Traking_Code { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Email { get; set; }
        public int Status_Code { get; set; }
        public string Status_Result { get; set; }
        public string Username { get; set; }
        public string Date_Format { get; set; }
        public string Request_Date { get; set; }
        public string Request_Time { get; set; }
        public string Request_IP { get; set; }
        public string Request_Browser { get; set; }
    }
}