using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Net;
using System.Text;
using System.IO;

namespace Favorites._CLASS
{
    public class LinkInfo
    {
        private string link = "";
        private string title = "";
        private string description = "";
        private string keywords = "";

        public string getlink
        {
            get { return link; }
        }
        public string gettitle
        {
            get { return title; }
        }
        public string getdescription
        {
            get { return description; }
        }
        public string getkeywords
        {
            get { return keywords; }
        }

        public LinkInfo(string _link)
        {
            link = _link;
            string temp = connectToServer(link);
            title = GetStringInBetween(temp,"<title>", "</title>");
            description = GetStringInBetween(temp, "<meta name=\"description\" content=\"", "\" >", "\" />", "\"/>", "\">");
            keywords = GetStringInBetween(temp,"<meta name=\"keywords\" content=\"", "\" />", "\" >", "\" />", "\">", "\"/>");    
        }

        public LinkInfo(string _link, string _description, string _keywords, string _title)
        {
            link = _link;
            description = _description;
            keywords = _keywords;
            title = _title;            
        }


        private string GetStringInBetween(string strSource,string strBegin,params string[] strEnd)
        {
            strBegin = strBegin.ToLower();
            for(int i = 0 ; i < strEnd.Length ; i++)
                strEnd[i] = strEnd[i].ToLower();
            string temp = strSource.ToLower();
            string result = "";
            int iIndexOfBegin = temp.IndexOf(strBegin);
            if (iIndexOfBegin != -1)
            {
                // include the Begin string if desired
                temp = temp.Substring(iIndexOfBegin + strBegin.Length);
                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);
                
                int[] iEnd = new int[strEnd.Length];
                int min = -1;
                bool init = false;
                for (int i = 0; i < iEnd.Length; i++)
                {
                    iEnd[i] = temp.IndexOf(strEnd[i]);
                    if (iEnd[i] == -1)
                        continue;
                    else if ((iEnd[i] < min) || !init)
                    {
                        min = iEnd[i];
                        init = true;
                    }
                }
                if (min != -1)
                {
                    result = strSource.Substring(0, min);
                }
            }
            return result;
        }

        private string connectToServer(string link)
        {
            try
            {
                // used to build entire input
                StringBuilder sb = new StringBuilder();
                // used on each read operation
                byte[] buf = new byte[8192];
                // prepare the web page we will be asking for
                //TODO the url dynamicly changed
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);
                request.Timeout = 1 * 60 * 60 * 1000;
                // execute the request
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                // we will read data via the response stream
                Stream resStream = response.GetResponseStream();
                string tempString = null;
                int count = 0;
                do
                {
                    // fill the buffer with data
                    count = resStream.Read(buf, 0, buf.Length);
                    // make sure we read some data
                    if (count != 0)
                    {
                        // translate from bytes to ASCII text
                        tempString = Encoding.UTF8.GetString(buf, 0, count);
                        // continue building the string
                        sb.Append(tempString);
                    }
                }
                while (count > 0); // any more data to read?
                // print out page source
                return sb.ToString();
            }
            catch (Exception er) { return ""; }
        }

    }
}
