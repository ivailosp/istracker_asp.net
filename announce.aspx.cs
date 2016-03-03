using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace istracker_asp.net
{
    public partial class announce : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        protected string GetIPAddress()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
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
        private byte[] queryBin(string key, string rawurl)
        {
            try {
                string[] tmp = Request.RawUrl.Split('?');
                int idx_bgn = tmp[1].IndexOf(key) + key.Length;
                int inx_end = tmp[1].IndexOf("&", idx_bgn);
                string raw_string;
                if (inx_end != -1)
                    raw_string = tmp[1].Substring(idx_bgn, inx_end - idx_bgn);
                else
                    raw_string = tmp[1].Substring(idx_bgn);
                return HttpUtility.UrlDecodeToBytes(raw_string);
            }
            catch
            {
                return null;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            BENObject obj = new BENObject(new Dictionary<BENObject, BENObject>());
            if (Request.QueryString["info_hash"] == null)
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Missing info_hash");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(100);
                Response.Write(obj.ToString());
                return;
            }
            if (Request.QueryString["peer_id"] == null)
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Missing peer_id");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(102);
                Response.Write(obj.ToString());
                return;
            }
            if (Request.QueryString["port"] == null)
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Missing port");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(103);
                Response.Write(obj.ToString());
                return;
            }

            string sha = bin2hex(queryBin("info_hash=", Request.RawUrl));
            string peer_id = bin2hex(queryBin("peer_id=", Request.RawUrl));

            if (sha != null && sha.Length != 40)
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Invalid infohash: infohash is not 20 bytes long");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(150);
                Response.Write(obj.ToString());
                return;
            }

            if (peer_id != null && peer_id.Length != 40)
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Invalid peerid: peerid is not 20 bytes long");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(151);
                Response.Write(obj.ToString());
                return;
            }
            try {
                using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                {
                    string stmt;
                    myConnection.Open();

                    int interval;
                    if (Int32.TryParse(ConfigurationManager.AppSettings["config_interval"], out interval) == false)
                        interval = 300;

                    stmt = "DELETE FROM peers WHERE date < DATEADD(SECOND, -@interval, GETDATE())";
                    using (SqlCommand command = new SqlCommand(stmt, myConnection))
                    {
                        command.Parameters.AddWithValue("interval", (int)(interval * 1.2));
                        command.ExecuteScalar();
                    }

                    string ip;
                    if (Request.QueryString["ip"] != null)
                        ip = Request.QueryString["ip"];
                    else
                        ip = GetIPAddress();

                    int port;
                    Int32.TryParse(Request.QueryString["port"], out port);
         
                    if (Request.QueryString["event"] != null && Request.QueryString["event"].Equals("stopped"))
                    {
                        stmt = "DELETE FROM peers WHERE sha=@sha AND port=@port AND peer_id=@peer_id";
                        using (SqlCommand command = new SqlCommand(stmt, myConnection))
                        {
                            command.Parameters.AddWithValue("sha", sha);
                            command.Parameters.AddWithValue("peer_id", peer_id);
                            command.Parameters.AddWithValue("port", port);
                            command.ExecuteNonQuery();
                            return;
                        }
                    }

                    if (Request.QueryString["event"] != null && Request.QueryString["event"].Equals("completed"))
                    {
                        stmt = "UPDATE torrents SET downloaded = downloaded + 1 WHERE sha=@sha";
                        using (SqlCommand command = new SqlCommand(stmt, myConnection))
                        {
                            command.Parameters.AddWithValue("sha", sha);
                            command.ExecuteNonQuery();
                        }
                    }

                    int numwant;
                    int numwant_max;

                    if (Int32.TryParse(Request.QueryString["numwant"], out numwant) == false)
                        numwant = 50;

                    if (Int32.TryParse(ConfigurationManager.AppSettings["config_max_numwant"], out numwant_max) == false)
                        numwant_max = 200;

                    if (numwant > numwant_max)
                    {
                        obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Invalid numwant.Client requested more peers than allowed by tracker.");
                        obj.getDictonary()[new BENObject("failure code")] = new BENObject(152);
                        Response.Write(obj.ToString());
                        return;
                    }

                    long uploaded;
                    long downloaded;
                    long left;

                    Int64.TryParse(Request.QueryString["uploaded"], out uploaded);
                    Int64.TryParse(Request.QueryString["downloaded"], out downloaded);
                    Int64.TryParse(Request.QueryString["left"], out left);

                    stmt = "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;\n" +
                            "BEGIN TRANSACTION;\n" +
                            "UPDATE peers SET peer_id=@peer_id, date=@date, uploaded=@uploaded, downloaded=@downloaded, dl_left=@dl_left WHERE sha=@sha AND ip=@ip AND port=@port\n" +
                            "IF @@ROWCOUNT = 0\n" +
                            "INSERT INTO peers(sha, ip, port, peer_id, uploaded, downloaded, dl_left) VALUES(@sha, @ip, @port, @peer_id, @uploaded, @downloaded, @dl_left)\n" +
                            "COMMIT TRANSACTION;";
                    try {
                        using (SqlCommand command = new SqlCommand(stmt, myConnection))
                        {
                            command.Parameters.AddWithValue("sha", sha);
                            command.Parameters.AddWithValue("peer_id", peer_id);
                            command.Parameters.AddWithValue("ip", ip);
                            command.Parameters.AddWithValue("port", port);
                            command.Parameters.AddWithValue("date", DateTime.Now);
                            command.Parameters.AddWithValue("uploaded", uploaded);
                            command.Parameters.AddWithValue("downloaded", downloaded);
                            command.Parameters.AddWithValue("dl_left", left);
                            command.ExecuteNonQuery();
                        }
                    }
                    catch
                    {
                        obj.getDictonary()[new BENObject("failure reason")] = new BENObject("info_hash not found in the database");
                        obj.getDictonary()[new BENObject("failure code")] = new BENObject(200);
                        Response.Write(obj.ToString());
                        return;
                    }


                    int no_peer_id;
                    Int32.TryParse(Request.QueryString["no_peer_id"], out no_peer_id);
                    no_peer_id = 0;
                    obj.getDictonary()[new BENObject("interval")] = new BENObject(interval);
                    List<BENObject> peers = new List<BENObject>();
                    obj.getDictonary()[new BENObject("peers")] = new BENObject(peers);

                    if (numwant > 0)
                    {
                        if (no_peer_id == 0)
                            stmt = String.Format("SELECT TOP {0} ip, port, peer_id FROM peers WHERE sha=@sha;", numwant);
                        else
                            stmt = String.Format("SELECT TOP {0} ip, port FROM peers WHERE sha=@sha;", numwant);

                        using (SqlCommand command = new SqlCommand(stmt, myConnection))
                        {
                            command.Parameters.AddWithValue("sha", sha);
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                BENObject ben_peer_id = new BENObject("peer id");
                                BENObject ben_ip = new BENObject("ip");
                                BENObject ben_port = new BENObject("port");
                                while (reader.Read())
                                {
                                    Dictionary<BENObject, BENObject> dic = new Dictionary<BENObject, BENObject>();
                                    if (no_peer_id == 0)
                                        dic.Add(ben_peer_id, new BENObject(hex2bin(reader.GetString(2))));
                                    dic.Add(ben_ip, new BENObject(reader.GetString(0)));
                                    dic.Add(ben_port, new BENObject(reader.GetInt32(1)));

                                    peers.Add(new BENObject(dic));
                                }
                            }
                        }
                    }
                    Response.BinaryWrite(obj.ToBytes());
                }
            }
            catch
            {
                obj.getDictonary()[new BENObject("failure reason")] = new BENObject("Generic error");
                obj.getDictonary()[new BENObject("failure code")] = new BENObject(900);
                Response.Write(obj.ToString());
            }
        }
    }
}