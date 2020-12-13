using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Xml;

namespace iCore_Administrator.Modules
{
    public enum DataBase_Selector
    {
        Administrator = 1
    }
    public class SQL_Tranceiver
    {
        PublicFunctions Pb = new PublicFunctions();

        public string Sql_Date()
        {
            return Pb.Get_Date_Formated("yyyy-MM-dd");
        }

        public string Sql_Time()
        {
            return Pb.Get_Time_Formated("HH:mm:ss");
        }


        private string Get_Connection_String(DataBase_Selector DBS)
        {
            try
            {
                string Feefback = "";
                string DB = "";
                switch (DBS)
                {
                    case DataBase_Selector.Administrator: { DB = System.Configuration.ConfigurationManager.AppSettings["SQL_DBN_Administrator"]; break; }
                }
                string SN = System.Configuration.ConfigurationManager.AppSettings["SQL_ServerName"];
                string UN = System.Configuration.ConfigurationManager.AppSettings["SQL_Username"];
                string PW = System.Configuration.ConfigurationManager.AppSettings["SQL_Password"];
                Feefback = "data source=" + SN.Trim() + ";initial catalog=" + DB.Trim() + ";user id=" + UN.Trim() + ";Password=" + PW.Trim() + ";";
                return Feefback;
            }
            catch (Exception)
            {
                return "";
            }
        }


        public SqlConnection Get_Sql_Connection(DataBase_Selector DBS)
        {
            try
            {
                SqlConnection SC = new SqlConnection(Get_Connection_String(DBS));
                return SC;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataTable Get_DTable_TSQL(DataBase_Selector DBS, string TSQL)
        {
            try
            {
                SqlConnection S_Con = Get_Sql_Connection(DBS);
                DataTable DataT = new DataTable();
                SqlCommand Com = new SqlCommand(TSQL.Trim());
                Com.CommandType = System.Data.CommandType.Text;
                Com.Connection = S_Con;
                S_Con.Open();
                SqlDataAdapter Da = new SqlDataAdapter(Com);
                Da.Fill(DataT);
                S_Con.Close();
                return DataT;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public DataSet Get_DSet_TSQL(DataBase_Selector DBS, string TSQL)
        {
            try
            {
                SqlConnection S_Con = Get_Sql_Connection(DBS);
                DataSet DataT = new DataSet();
                SqlCommand Com = new SqlCommand(TSQL.Trim());
                Com.CommandType = System.Data.CommandType.Text;
                Com.Connection = S_Con;
                S_Con.Open();
                SqlDataAdapter Da = new SqlDataAdapter(Com);
                Da.Fill(DataT);
                S_Con.Close();
                return DataT;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public void Execute_TSql(DataBase_Selector DBS, string TSQL)
        {
            try
            {
                SqlConnection S_Con = Get_Sql_Connection(DBS);
                S_Con.Open();
                SqlCommand SCom = new SqlCommand(TSQL, S_Con);
                SCom.ExecuteNonQuery();
                S_Con.Close();
            }
            catch (Exception)
            { }
        }


        public long Get_New_ID(DataBase_Selector DBS, string Table_Name, string Field)
        {
            try
            {
                long FB = 0;
                DataTable GNC = new DataTable();
                GNC = Get_DTable_TSQL(DBS, "Select Max(" + Field.Trim() + ") From " + Table_Name.Trim());
                string FBB = GNC.Rows[0][0].ToString();
                if (FBB.Trim() != "")
                {
                    FB = Int32.Parse(FBB);
                    if (FB <= 100) { FB = 100; }
                }
                else
                {
                    FB = 100;
                }
                FB++;
                return FB;
            }
            catch (Exception)
            {
                return 0;
            }
        }


        public long Get_New_ID_Refrence(DataBase_Selector DBS, string Table_Name, string Field, string Equal_Field, string Equal_Value)
        {
            try
            {
                long FB = 0;
                DataTable GNC = new DataTable();
                GNC = Get_DTable_TSQL(DBS, "Select Max(" + Field.Trim() + ") From " + Table_Name.Trim() + " Where (" + Equal_Field + " = '" + Equal_Value + "')");
                string FBB = GNC.Rows[0][0].ToString();
                if (FBB.Trim() != "")
                {
                    FB = Int32.Parse(FBB);
                    if (FB < 1) { FB = 0; }
                }
                else
                {
                    FB = 0;
                }
                FB++;
                return FB;
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}