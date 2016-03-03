using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace istracker_asp.net
{
    public partial class scrape : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        private byte[] queryBin(string key, string rawurl)
        {
            try {
                string[] tmp = Request.RawUrl.Split('?');
                int idx_bgn = tmp[1].IndexOf(key) + key.Length;
                int inx_end = tmp[1].IndexOf("&", idx_bgn);
                string raw_string;
                if (inx_end != -1)
                    raw_string = tmp[1].Substring(idx_bgn, tmp[1].IndexOf("&", idx_bgn) - idx_bgn);
                else
                    raw_string = tmp[1].Substring(idx_bgn);
                return HttpUtility.UrlDecodeToBytes(raw_string);
            }
            catch
            {
                return null;
            }
        }
        private byte[] hex2bin(string hex)
        {
            if (hex != null)
                return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
            else
                return null;
        }
        Dictionary<BENObject, BENObject> handleSha(string sha)
        {
            Dictionary<BENObject, BENObject> sha_dict = new Dictionary<BENObject, BENObject>();
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt = "SELECT count(*) FROM peers WHERE dl_left=0 AND sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    sha_dict[new BENObject("complete")] = new BENObject((int)cmdCount.ExecuteScalar());
                }
                stmt = "SELECT count(*) FROM peers WHERE dl_left!=0 AND sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    sha_dict[new BENObject("incomplete")] = new BENObject((int)cmdCount.ExecuteScalar());
                }
                stmt = "SELECT downloaded FROM torrents WHERE sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    sha_dict[new BENObject("downloaded")] = new BENObject((int)cmdCount.ExecuteScalar());
                }
            }

            return sha_dict;
        }
        private string bin2hex(byte[] bin)
        {
            if (bin == null)
                return null;
            string ret = "";
            foreach (byte ch in bin)
            {
                ret += ch.ToString("X2");
            }
            return ret;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            Dictionary <BENObject, BENObject> dict = new Dictionary<BENObject, BENObject>();
            BENObject obj = new BENObject(dict);

            Dictionary<BENObject, BENObject> files = new Dictionary<BENObject, BENObject>();
            dict[new BENObject("files")] = new BENObject(files);

            string info_hash = bin2hex(queryBin("info_hash=", Request.RawUrl));
            if (info_hash != null && info_hash.Length == 40)
            {
                try {
                    files[new BENObject(hex2bin(info_hash))] = new BENObject(handleSha(info_hash));
                }
                catch
                {
                }
            }
            else
            {
                try {
                    using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                    {
                        myConnection.Open();
                        string stmt = "SELECT sha FROM torrents;";
                        using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                        {
                            using (SqlDataReader reader = cmdCount.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string sha = reader.GetString(0);
                                    try {
                                        files[new BENObject(hex2bin(info_hash))] = new BENObject(handleSha(info_hash));
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            Response.BinaryWrite(obj.ToBytes());
        }
    }
}