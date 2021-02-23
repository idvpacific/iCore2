using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace iCore_Administrator.Modules.HSU_Application
{
    public class Application_Guesty
    {
        //====================================================================================================================
        SQL_Tranceiver Sq = new SQL_Tranceiver();
        PublicFunctions Pb = new PublicFunctions();
        //====================================================================================================================
        string Guesty_Key = ""; string Guesty_Secret = ""; string Guesty_API_Address = "";
        string Guesty_Key_Func = ""; string Guesty_Secret_Func = ""; string Guesty_API_Address_Func = "";
        //===================================================================================================================
        public enum BookingStatus
        {
            Approve = 1,
            Decline = 2
        }

        private class Class_Get_ReservationID
        {
            public string type { get; set; }
            public string _id { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public object picture { get; set; }
        }

        private class Class_Get_ListingID
        {
            public string _id { get; set; }
            public string listingId { get; set; }
        }

        private class Address
        {
            public string searchable { get; set; }
            public double lng { get; set; }
            public double lat { get; set; }
            public string apt { get; set; }
            public string street { get; set; }
            public string zipcode { get; set; }
            public string country { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string full { get; set; }
        }

        private class Class_Get_Address
        {
            public string _id { get; set; }
            public Address address { get; set; }
        }

        public class Guesty_Address
        {
            public string searchable { get; set; }
            public double lng { get; set; }
            public double lat { get; set; }
            public string apt { get; set; }
            public string street { get; set; }
            public string zipcode { get; set; }
            public string country { get; set; }
            public string state { get; set; }
            public string city { get; set; }
            public string full { get; set; }
            public int Error_Code { get; set; }
            public string Error_Text { get; set; }
        }

        private class Class_Get_BookingStatus
        {
            public string _id { get; set; }
            public string status { get; set; }
        }

        public class Guesty_BookingStatus
        {
            public string _id { get; set; }
            public string status { get; set; }
            public int Error_Code { get; set; }
            public string Error_Text { get; set; }
        }
        //===================================================================================================================
        public void Set_ConnectionKey(string API_Url, string G_Key, string G_Secret)
        {
            Guesty_Key = ""; Guesty_Secret = ""; Guesty_API_Address = "";
            Guesty_Key_Func = ""; Guesty_Secret_Func = ""; Guesty_API_Address_Func = "";
            API_Url = API_Url.Trim();
            G_Key = G_Key.Trim();
            G_Secret = G_Secret.Trim();
            if(API_Url.Substring(API_Url.Length - 1, 1) == "/") { API_Url = API_Url.Substring(0, API_Url.Length - 1); }
            if(API_Url.Substring(API_Url.Length - 1, 1) == "/") { API_Url = API_Url.Substring(0, API_Url.Length - 1); }
            if(API_Url.Substring(API_Url.Length - 1, 1) == "/") { API_Url = API_Url.Substring(0, API_Url.Length - 1); }
            Guesty_API_Address_Func = API_Url;
            Guesty_Key_Func = G_Key;
            Guesty_Secret_Func = G_Secret; 
        }
        //===================================================================================================================
        private bool Set_Guesty_ConnectionData()
        {
            try
            {
                Guesty_Key = ""; Guesty_Secret = ""; Guesty_API_Address = "";
                Guesty_Key = Guesty_Key_Func.Trim();
                Guesty_Secret = Guesty_Secret_Func.Trim();
                Guesty_API_Address = Guesty_API_Address_Func.Trim();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        //===================================================================================================================
        public string Get_ReservationID(string ConfirmationCode)
        {
            try
            {
                if (Guesty_API_Address.Trim() == "") { if (Set_Guesty_ConnectionData() == false) { return "0"; } }
                ConfirmationCode = ConfirmationCode.Replace(":", "").Replace(",", "").Replace(" ", "").Trim();
                if (ConfirmationCode == "") { return "0"; }
                var client = new RestClient(Guesty_API_Address + "/search?q=" + ConfirmationCode + "&type=bookings");
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                client.Authenticator = new HttpBasicAuthenticator(Guesty_Key, Guesty_Secret);
                IRestResponse response = client.Execute(request);
                Class_Get_ReservationID Result_GRID = JsonConvert.DeserializeObject<Class_Get_ReservationID>(response.Content.Replace("]", "").Replace("[", "").Trim());
                string ReservationID = "0";
                ReservationID = Result_GRID._id;
                return ReservationID;
            }
            catch (Exception)
            {
                return "0";
            }
        }
        //===================================================================================================================
        public string Get_ListingID(string ReservationID)
        {
            try
            {
                if (Guesty_API_Address.Trim() == "") { if (Set_Guesty_ConnectionData() == false) { return "0"; } }
                ReservationID = ReservationID.Replace(":", "").Replace(",", "").Replace(" ", "").Trim();
                if (ReservationID == "") { return "0"; }
                string url = Guesty_API_Address + "/reservations/" + ReservationID;
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                client.Authenticator = new HttpBasicAuthenticator(Guesty_Key, Guesty_Secret);
                IRestResponse response = client.Execute(request);
                string ResCon = response.Content.Trim();
                if (ResCon.Substring(0, 1) == "[") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                if (ResCon.Substring(0, 1) == "]") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                if (ResCon.Substring(ResCon.Length - 1, 1) == "[") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                if (ResCon.Substring(ResCon.Length - 1, 1) == "]") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                Class_Get_ListingID Result_GLID = JsonConvert.DeserializeObject<Class_Get_ListingID>(ResCon);
                string ListingID = "0";
                ListingID = Result_GLID.listingId;
                return ListingID;
            }
            catch (Exception)
            {
                return "0";
            }
        }
        //===================================================================================================================
        public Guesty_Address Get_Address(string ConfirmationCode)
        {
            Guesty_Address Gadd = new Guesty_Address();
            Gadd.searchable = "";
            Gadd.lng = 0;
            Gadd.lat = 0;
            Gadd.apt = "";
            Gadd.street = "";
            Gadd.zipcode = "";
            Gadd.country = "";
            Gadd.state = "";
            Gadd.city = "";
            Gadd.full = "";
            Gadd.Error_Code = 0;
            Gadd.Error_Text = "";
            try
            {
                ConfirmationCode = ConfirmationCode.Replace(":", "").Replace(",", "").Replace(" ", "").Trim();
                if (ConfirmationCode != "")
                {
                    if (Set_Guesty_ConnectionData() == true)
                    {
                        string ReservationID = "";
                        ReservationID = Get_ReservationID(ConfirmationCode).Trim();
                        if ((ReservationID != "") && (ReservationID != "0"))
                        {
                            string ListingID = "";
                            ListingID = Get_ListingID(ReservationID).Trim();
                            if ((ListingID != "") && (ListingID != "0"))
                            {
                                string url = Guesty_API_Address + "/listings/" + ListingID;
                                var client = new RestClient(url);
                                client.Timeout = -1;
                                var request = new RestRequest(Method.GET);
                                client.Authenticator = new HttpBasicAuthenticator(Guesty_Key, Guesty_Secret);
                                IRestResponse response = client.Execute(request);
                                if ((response.Content.ToString().Trim().ToLower().IndexOf("provide an valid listing id") < 0) && (response.Content.ToString().Trim().ToLower().IndexOf("listing not found") < 0))
                                {
                                    string ResCon = response.Content.Trim();
                                    if (ResCon.Substring(0, 1) == "[") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                                    if (ResCon.Substring(0, 1) == "]") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                                    if (ResCon.Substring(ResCon.Length - 1, 1) == "[") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                                    if (ResCon.Substring(ResCon.Length - 1, 1) == "]") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                                    Class_Get_Address Result_Listing = JsonConvert.DeserializeObject<Class_Get_Address>(ResCon);
                                    if (Result_Listing._id.Trim().ToLower() == ListingID.Trim().ToLower())
                                    {
                                        Gadd.searchable = Result_Listing.address.searchable.Trim();
                                        Gadd.lng = Result_Listing.address.lng;
                                        Gadd.lat = Result_Listing.address.lat;
                                        Gadd.apt = Result_Listing.address.apt.Trim();
                                        Gadd.street = Result_Listing.address.street.Trim();
                                        Gadd.zipcode = Result_Listing.address.zipcode.Trim();
                                        Gadd.country = Result_Listing.address.country.Trim();
                                        Gadd.state = Result_Listing.address.state.Trim();
                                        Gadd.city = Result_Listing.address.city.Trim();
                                        Gadd.full = Result_Listing.address.full.Trim();
                                        Gadd.Error_Code = 0;
                                        Gadd.Error_Text = "OK";
                                    }
                                    else
                                    {
                                        Gadd.Error_Code = 7;
                                        Gadd.Error_Text = "Result not found, Check your confirmation or reservation or listing code";
                                    }
                                }
                                else
                                {
                                    Gadd.Error_Code = 6;
                                    Gadd.Error_Text = "Address data not valid, Check your confirmation or reservation code";
                                }
                            }
                            else
                            {
                                Gadd.Error_Code = 5;
                                Gadd.Error_Text = "Listing ID not valid, Check your confirmation and reservation code";
                            }
                        }
                        else
                        {
                            Gadd.Error_Code = 4;
                            Gadd.Error_Text = "Reservation ID not valid, Check your confirmation code";
                        }
                    }
                    else
                    {
                        Gadd.Error_Code = 3;
                        Gadd.Error_Text = "Connection setting not found";
                    }
                }
                else
                {
                    Gadd.Error_Code = 2;
                    Gadd.Error_Text = "Confirmation code not found";
                }
            }
            catch (Exception)
            {
                Gadd.Error_Code = 1;
                Gadd.Error_Text = "System error";
            }
            return Gadd;
        }
        //===================================================================================================================
        public Guesty_BookingStatus Set_Booking_Status(string ConfirmationCode, BookingStatus BStatus)
        {
            Guesty_BookingStatus GBS = new Guesty_BookingStatus();
            GBS._id = "";
            GBS.status = "";
            GBS.Error_Code = 0;
            GBS.Error_Text = "";
            try
            {
                ConfirmationCode = ConfirmationCode.Replace(":", "").Replace(",", "").Replace(" ", "").Trim();
                if (ConfirmationCode != "")
                {
                    if (Set_Guesty_ConnectionData() == true)
                    {
                        string ReservationID = "";
                        ReservationID = Get_ReservationID(ConfirmationCode).Trim();
                        if ((ReservationID != "") && (ReservationID != "0"))
                        {
                            string BSts = "none";
                            if (BStatus == BookingStatus.Approve) { BSts = "approve"; }
                            if (BStatus == BookingStatus.Decline) { BSts = "decline"; }
                            string url = Guesty_API_Address + "/reservations/" + ReservationID + "/" + BSts;
                            var client = new RestClient(url);
                            client.Timeout = -1;
                            var request = new RestRequest(Method.POST);
                            client.Authenticator = new HttpBasicAuthenticator(Guesty_Key, Guesty_Secret);
                            IRestResponse response = client.Execute(request);
                            string ResCon = response.Content.Trim();
                            if (ResCon.Substring(0, 1) == "[") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                            if (ResCon.Substring(0, 1) == "]") { ResCon = ResCon.Substring(1, ResCon.Length - 1); }
                            if (ResCon.Substring(ResCon.Length - 1, 1) == "[") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                            if (ResCon.Substring(ResCon.Length - 1, 1) == "]") { ResCon = ResCon.Substring(0, ResCon.Length - 1); }
                            if (ResCon.IndexOf("servation status i") < 0)
                            {
                                Class_Get_BookingStatus Result_BST = JsonConvert.DeserializeObject<Class_Get_BookingStatus>(ResCon);
                                GBS._id = Result_BST._id;
                                GBS.status = Result_BST.status;
                                GBS.Error_Code = 0;
                                GBS.Error_Text = "OK";
                            }
                            else
                            {
                                GBS.Error_Code = 5;
                                GBS.Error_Text = ResCon;
                            }
                        }
                        else
                        {
                            GBS.Error_Code = 4;
                            GBS.Error_Text = "Reservation ID not valid, Check your confirmation code";
                        }
                    }
                    else
                    {
                        GBS.Error_Code = 3;
                        GBS.Error_Text = "Connection setting not found";
                    }
                }
                else
                {
                    GBS.Error_Code = 2;
                    GBS.Error_Text = "Confirmation code not found";
                }
            }
            catch (Exception)
            {
                GBS.Error_Code = 1;
                GBS.Error_Text = "System error";
            }
            return GBS;
        }
        //===================================================================================================================
    }
}