using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Favorites._CLASS;
using System.Collections.Generic;

namespace Favorites
{
    public partial class addFav : System.Web.UI.Page
    {
        static Event eventlog = new Event();
        static DataBase DB = null;
        static bool ServerLoaded = false;
        static Object ServerLoading = new Object();
        
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!ServerLoaded)
            {
                lock (ServerLoading)
                {
                    if (!ServerLoaded)
                    {
                        DB = new DataBase(eventlog);
                        eventlog.SaveToLOG("Server Started.", System.Diagnostics.EventLogEntryType.SuccessAudit);
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////
            if (Request.HttpMethod.Equals("GET"))
                doGet();
            else if (Request.HttpMethod.Equals("POST"))
                ;// doPost();
            ////////////////////////////////////////////////////////////////////////////

        }

        private void doGet()
        {
            string userid = (string)Session["userid"];
            string comname = Request.QueryString["comname"];
            if (comname == null)
            {
                return;
            }
            else if (comname.Equals("login"))
            {
                string user = Request.QueryString["user"];
                string password = Request.QueryString["pass"];
                string id = DB.LoginUser(user, password);
                if (id != null && !id.Equals(""))
                {
                    Session["userid"] = id;
                    userid = id;
                    comname = "getFav";
                }
            }
            else if (comname.Equals("logout"))
            {
               Session["userid"] = null;
            }

            if (userid == null)
            {
                Response.Write(LoginPage());
                return;
            }
            
            string local = Request.ServerVariables["HTTP_HOST"];
            if (comname.Equals("addFav"))
            {
                string link = Request.QueryString["link"];
                LinkInfo linkinfo = new LinkInfo(link);
                DB.addFav(userid, linkinfo);
                return;
            }
            else if (comname.Equals("getFav"))
            {
                List<LinkInfo> linkinfo = DB.getFav(userid);
                Response.Write(CreateHTML(linkinfo, ""));
                Response.OutputStream.Close();
                return;
            }
            else if (comname.Equals("serachFav"))
            {
                string title = Request.QueryString["title"];
                List<LinkInfo> linkinfo = DB.searchFav(userid, title);
                Response.Write(CreateHTML(linkinfo, title));
                Response.OutputStream.Close();
                return;
            }
        }



        private string LoginPage()
        {
            string temp = "";
            temp += "<p>My Favorite Links</p><tr><p>User : <input type=\"text\" name=\"user\" id=\"user\" /></p><tr><p>" +
            "Pass : <input type=\"password\" name=\"password\" id=\"password\" /></p><tr><p>" +
            "<input type=\"button\" value=\"Login\" onclick=\"javascript:Login();\"/></p>";
            return temp;
        }

        private string CreateHTML(List<LinkInfo> linkinfo, string titleserach)
        {
            string temp = "";

            temp += "<p>My Favorite Links</p><table><tr><td colspan='2' align='left'>" +
            "<a id='savelinkclick' href='javascript:addLink();' title=''>Save Link</a>,<a id='refreshclick' href='javascript:refreshAll();' title=''>Refresh</a><br> Search :<input type='text' value='" + titleserach + "' name='titleserach' id='titleserach' onkeyup='javascript:searchLink();'/>" +
            "<input type='hidden' name='link' id='link' /></td></tr>";
            
            temp += "<tr class=\"odd\">" +
            "<td scope=\"col\" >Title</td>" +
            "<td scope=\"col\" >Link</td>" +
            "</tr><tbody>"; 

            for(int i = 0 ; i < linkinfo.Count ; i++){
                string odd = "odd";
                if(i % 2 == 0)
                    odd = "";
                temp += "<tr class=\""+odd+"\">";
                temp += "<td title='" + linkinfo[i].getdescription+ "'>" + linkinfo[i].gettitle + "</td>";
                temp += "<td><a href=\"javascript:openTab('" + linkinfo[i].getlink + "')\"  title='" + linkinfo[i].getdescription + "' >" + linkinfo[i].getlink + "</a></td>";
                temp += "</tr>";
                }
            temp += "</tbody></table></div></tr>";
            temp += "";
            return temp;
        }
    }
}
