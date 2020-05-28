using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Favorites._CLASS
{
    public class DataBase
    {
        SQLiteConnection DB = null;
        Event eventlog = null;
        public DataBase(Event eventlog_t)
        {
            try
            {
                eventlog = eventlog_t;
                if (!System.IO.File.Exists(HttpContext.Current.Server.MapPath("Fav.s3db")))
                    throw new Exception("Fav.s3db file dose not exist on : " + HttpContext.Current.Server.MapPath("Fav.s3db"));
                DB = new SQLiteConnection("Data Source=" + HttpContext.Current.Server.MapPath("Fav.s3db;UseUTF16Encoding=True"));
                DB.Open();
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while creating Database connection : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }
        public void closeConnection() { try { DB.Close(); } catch (Exception) { } }

        public bool addFav(string userid, LinkInfo link)
        {
            if (userid.Equals("") || link == null)
                return false;
            try
            {

                SQLiteCommand command = DB.CreateCommand();
                //create a new user
                string create = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                command.CommandText = "INSERT INTO favorite (favoriteLink, favoriteCreate,favoriteSubcriberID,favoriteDescription,favoriteKeywords,favoriteTitle) VALUES ('" + link.getlink + "','" + create + "','" + userid + "','" + link.getdescription + "','" + link.getkeywords + "','" + link.gettitle + "')";
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while saving favorites : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        public string LoginUser(string username, string password) {
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT subcribersid FROM subcribers Where subcribersUserName = '" + username + "' AND subcribersPassword = '" + password + "'";
                SQLiteDataReader reader = command.ExecuteReader();
                string id = null;
                if (reader.HasRows)
                {
                    reader.Read();
                    id = reader.GetInt32(0).ToString();
                }
                reader.Close();
                return id;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<LinkInfo> getFav(string userid)
        {
            List<LinkInfo> ret = new List<LinkInfo>();
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT favoriteLink, favoriteDescription,favoriteKeywords,favoriteTitle FROM favorite Where favoriteSubcriberID = '" + userid + "'";
                SQLiteDataReader reader = command.ExecuteReader();

                while (reader.HasRows)
                {
                    reader.Read();
                    ret.Add(new LinkInfo(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }
                reader.Close();
                return ret;
            }
            catch (Exception e)
            {
                return ret;
            }
        }


          public List<LinkInfo> searchFav(string userid, string title)
          {
              List<LinkInfo> ret = new List<LinkInfo>();
              try
              {
                  SQLiteCommand command = DB.CreateCommand();
                  command.CommandText = "SELECT favoriteLink, favoriteDescription,favoriteKeywords,favoriteTitle FROM favorite Where favoriteSubcriberID = '" + userid + "' AND (favoriteTitle like '%" + title + "%' OR favoriteKeywords like '%" + title + "%' OR favoriteLink like '%" + title + "%')";
                  SQLiteDataReader reader = command.ExecuteReader();

                  while (reader.HasRows)
                  {
                      reader.Read();
                      ret.Add(new LinkInfo(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                  }
                  reader.Close();
                  return ret;
              }
              catch (Exception e)
              {
                  return ret;
              }
          }
        

      

        
        /*
        public bool updateUserNewMeasurement(BridgeInfoPacket user, int devetype)
        {
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created, NumOfHeartBeats , LastHeartBeatTX , LastTxType FROM subcribers Where BTID = '" + user.getBluetoothIDBridge() + "'";
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    //update existing user
                    reader.Read();
                    string btid = reader.GetString(0);
                    int numaccess = reader.GetInt32(1);
                    string pcode = reader.GetString(2);
                    string sorc = reader.GetString(3);
                    string lastaccess = reader.GetString(4);
                    string created = reader.GetString(5);
                    pcode = user.getPatientCode();
                    sorc = user.getSorC(true);
                    string mbqver = user.getBridgeVersion();
                    lastaccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    numaccess++;
                    reader.Close();
                    command.CommandText = "UPDATE subcribers SET NumOfAccess=" + numaccess + " ,PatientCode='" + pcode + "',SorC='" + sorc + "' , LastAccess='" + lastaccess + "' ,  LastTxType = " + devetype + " , MBQver = '" + mbqver + "' Where BTID = '" + user.getBluetoothIDBridge() + "'";

                }
                else
                {
                    //create a new user
                    string btid = user.getBluetoothIDBridge();
                    int numaccess = 0;
                    numaccess++;
                    string lastaccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    string created = lastaccess;
                    reader.Close();
                    command.CommandText = "INSERT INTO subcribers (BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created , NumOfHeartBeats , LastHeartBeatTX , LastTxType , LastExceptionTX , NumExceptionTX , MBQver , Remark) VALUES ('" + user.getBluetoothIDBridge() +
                    "',1,'" + user.getPatientCode() + "','" + user.getSorC(true) + "','" + lastaccess + "','" + created + "' , 0 , '' , " + devetype + ",'',0 , '' , '')";

                }

                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while updating user : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }
        public bool updateUserLastHeartBeatTX(string bt)
        {
            try
            {
                if (DB == null)
                    throw new Exception("Data Base not defiend");
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT NumOfHeartBeats FROM subcribers Where BTID = '" + bt + "'";
                SQLiteDataReader reader = command.ExecuteReader();
                if (!reader.HasRows)
                {
                    //create a new user
                    string btid = bt;
                    string lastaccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    string created = lastaccess;
                    reader.Close();
                    command.CommandText = "INSERT INTO subcribers (BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created , NumOfHeartBeats , LastHeartBeatTX , LastTxType , LastExceptionTX , NumExceptionTX, MBQver , Remark) VALUES " +
                                                                 "('" + bt + "',0,'','','','" + created + "' , 1 , '" + lastaccess + "' , 0 , '',0,'','')";
                }
                else
                {
                    reader.Read();
                    int NumOfHeartBeats = reader.GetInt32(0);
                    NumOfHeartBeats++;
                    string LastHeartBeatTX = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    reader.Close();
                    command.CommandText = "UPDATE subcribers SET LastHeartBeatTX = '" + LastHeartBeatTX + "' , NumOfHeartBeats = " + NumOfHeartBeats + " WHERE (BTID = '" + bt + "')";

                }
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while updating user LastHeartBeatTX: " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }
        public bool updateUserNewException(BridgeInfoPacket user)
        {
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT BTID, LastExceptionTX , NumExceptionTX FROM subcribers Where BTID = '" + user.getBluetoothIDBridge() + "'";
                SQLiteDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    //update existing user
                    reader.Read();
                    string btid = reader.GetString(0);
                    string lastaccess = reader.GetString(1);
                    int numaccess = reader.GetInt32(2);
                    string pcode = user.getPatientCode();
                    string sorc = user.getSorC(true);
                    lastaccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    numaccess++;
                    reader.Close();
                    command.CommandText = "UPDATE subcribers SET NumExceptionTX=" + numaccess + " ,PatientCode='" + pcode + "',SorC='" + sorc + "' , LastExceptionTX='" + lastaccess + "' , MBQver = '" + user.getBridgeVersion() + "' Where BTID = '" + user.getBluetoothIDBridge() + "'";

                }
                else
                {
                    //create a new user
                    string lastaccess = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    reader.Close();
                    command.CommandText = "INSERT INTO subcribers (BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created , NumOfHeartBeats , LastHeartBeatTX , LastTxType , LastExceptionTX , NumExceptionTX, MBQver , Remark) VALUES ('" + user.getBluetoothIDBridge() +
                    "',0,'" + user.getPatientCode() + "','" + user.getSorC(true) + "','','" + lastaccess + "' , 0 , '' , 0 ,'" + lastaccess + "',1,'','')";

                }
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while updating user : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }
        /// <summary>
        /// Adding an event to the DataBase for the reqeusted user
        /// </summary>
        /// <param name="userbt">The user BlueTooth Address (user id)</param>
        /// <param name="type" type="EventType">EventType class include all the illigal types</param>
        /// <param name="datetime">The datetime of the client that the event ouccured</param>
        /// <param name="details">Details if exists</param>
        /// <returns>Bool indicate if the insert function success</returns>
        public bool addEvent(string userbt, int type, string datetime, string details)
        {
            if (userbt.Equals("") || datetime.Equals(""))
                return false;
            try
            {

                SQLiteCommand command = DB.CreateCommand();
                //create a new user
                string lastaccess = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                command.CommandText = "INSERT INTO userevents (BTID, EventType,DateTime,Details,ServerDateTime) VALUES ('" + userbt + "','" + type + "','" + datetime + "','" + details + "','" + lastaccess + "')";
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while saving user event : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return false;
            }
        }

        public List<UserEvent> getEventReportLog(int timestamp, List<int> eventypes)
        {
            try
            {
                if (eventypes.Count == 0)
                    throw new Exception("No event type define.");
                SQLiteCommand command = DB.CreateCommand();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                command.CommandText = "SELECT BTID,DateTime,ServerDateTime,EventType,Details FROM userevents WHERE ((strftime('%s','" + now + "') - strftime('%s',ServerDateTime)) BETWEEN 0 and " + timestamp + ") AND (";
                for (int i = 0; i < eventypes.Count; i++)
                {
                    command.CommandText += "EventType = '" + eventypes[i] + "'";
                    if (i < eventypes.Count - 1)
                        command.CommandText += " OR ";
                }
                command.CommandText += ")";
                SQLiteDataReader reader = command.ExecuteReader();
                List<UserEvent> subcribers = new List<UserEvent>();
                while (reader.HasRows)
                {
                    if (!reader.Read())
                        break;
                    UserEvent currentmail = new UserEvent(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetInt32(3).ToString(), reader.GetString(4));
                    subcribers.Add(currentmail);
                }
                return subcribers;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while reading user events : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        public List<Subcriber> getMissedHBReportLog(int timestamp)
        {
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                command.CommandText = "SELECT BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created, NumOfHeartBeats , LastHeartBeatTX , LastTxType,LastExceptionTX,NumExceptionTX,MBQver  FROM subcribers WHERE ((strftime('%s','" + now + "') - strftime('%s',strftime('%Y-%m-%d %H:%M:%S',substr(LastHeartBeatTX,7,4) || '-' || substr(LastHeartBeatTX,4,2) || '-' || substr(LastHeartBeatTX,1,2)  || ' ' || substr(LastHeartBeatTX,12,8)))) > " + timestamp + ")";
                SQLiteDataReader reader = command.ExecuteReader();
                List<Subcriber> subcribers = new List<Subcriber>();
                while (reader.HasRows)
                {
                    if (!reader.Read())
                        break;
                    Subcriber currentmail = new Subcriber(reader.GetString(0), reader.GetInt32(1).ToString(), reader.GetString(2), reader.GetString(3),
                                            reader.GetString(4), reader.GetString(5), reader.GetInt32(6).ToString(), reader.GetString(7), reader.GetInt32(8).ToString(), reader.GetString(9), reader.GetInt32(10).ToString(), reader.GetString(11));
                    subcribers.Add(currentmail);
                }
                return subcribers;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while reading missed hb events : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }

        public List<Subcriber> getDeviceReportLog()
        {
            try
            {
                SQLiteCommand command = DB.CreateCommand();
                command.CommandText = "SELECT BTID, NumOfAccess,PatientCode,SorC , LastAccess, Created, NumOfHeartBeats , LastHeartBeatTX , LastTxType,LastExceptionTX,NumExceptionTX,MBQver FROM subcribers";
                SQLiteDataReader reader = command.ExecuteReader();
                List<Subcriber> subcribers = new List<Subcriber>();
                while (reader.HasRows)
                {
                    if (!reader.Read())
                        break;
                    Subcriber currentmail = new Subcriber(reader.GetString(0), reader.GetInt32(1).ToString(), reader.GetString(2), reader.GetString(3),
                                            reader.GetString(4), reader.GetString(5), reader.GetInt32(6).ToString(), reader.GetString(7), reader.GetInt32(8).ToString(), reader.GetString(9), reader.GetInt32(10).ToString(), reader.GetString(11));
                    subcribers.Add(currentmail);
                }
                return subcribers;
            }
            catch (Exception e)
            {
                eventlog.SaveToLOG("Error while reading device report : " + e.Message, System.Diagnostics.EventLogEntryType.Error);
                return null;
            }
        }


        */
    }
}
