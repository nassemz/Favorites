using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Diagnostics;

namespace Favorites._CLASS
{
    public class Event
    {
        public Event()
        {
            try
            {
                if (!EventLog.SourceExists(MyApplicationName))
                {
                    //An event log source should not be created and immediately used.
                    //There is a latency time to enable the source, it should be created
                    //prior to executing the application that uses the source.
                    //Execute this sample a second time to use the new source.
                    EventLog.CreateEventSource(MyApplicationName, MyApplicationName);
                    objEventLog.Source = MyApplicationName;
                    objEventLog.MaximumKilobytes = 1024;
                    objEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7);
                }
                else
                    objEventLog.Source = MyApplicationName;
            }
            catch (Exception ex) { }

        }

        public bool SaveToLOG(string sEvent, EventLogEntryType evnt)
        {
            try
            {
                objEventLog.WriteEntry(sEvent, evnt);
                return true;
            }
            catch (Exception) { return false; }
        }

        EventLog objEventLog = new EventLog();
        string MyApplicationName = "FavoritesServer";


    }
}
