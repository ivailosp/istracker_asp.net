using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace istracker_asp.net
{
    public partial class show : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        private string get_format_size(double size)
        {
            int step = 0;
            string[] arr = new string[] { " B", " KB", " MB", " GB" };
            for(step = 0; step < arr.Length-1 && size >= 1000; step++)
            {
                size /= 1024;
            }

            return size.ToString("0.00") + arr[step];
        }
        private void sendMsg(string msg)
        {
            NameLabel.Text = msg;
            DateLabel.Text = "";
            SizeLabel.Text = "";
            FilesLabel.Text = "";
        }
        private void handleRecord(string sha, SqlDataReader reader)
        {
            StringBuilder sb = new StringBuilder();
            String user = reader.GetString(2);
            sb.AppendFormat("{0}<a href=\"{1}/{2}.torrent\" download=\"{3}.torrent\">{3}</a> ", NameLabel.Text, ConfigurationManager.AppSettings["config_upload_dir"], sha, reader.GetString(0));
            if (Session["login"] != null)
            {
                if (user == (String)Session["username"])
                {
                    sb.AppendFormat("<a href=\"torrent.aspx?action=delete&sha={0}\">[X]</a>", sha);
                }
            }
            NameLabel.Text = sb.ToString();
            sb.Length = 0;

            sb.Append(DateLabel.Text);
            sb.Append(reader.GetDateTime(1));
            DateLabel.Text = sb.ToString();
            sb.Length = 0;

            string file_path = String.Format("{0}/{1}.torrent", ConfigurationManager.AppSettings["config_upload_dir"], sha);
            FileInfo file = new FileInfo(Server.MapPath(file_path));
            if (file.Exists)
            {
                byte[] bytes = File.ReadAllBytes(file.FullName);

                BENObject obj = new BENObject(ref bytes);

                BENObject ben_lenght = new BENObject("length");
                BENObject ben_name = new BENObject("name");
                Dictionary<BENObject, BENObject> info_dict = obj.getDictonary()[new BENObject("info")].getDictonary();

                if (info_dict.ContainsKey(ben_lenght) && info_dict[ben_lenght].getType() == BENObjectType.Integer &&
                    info_dict.ContainsKey(ben_name) && info_dict[ben_name].getType() == BENObjectType.String)
                {
                    sb.Append(SizeLabel.Text);
                    sb.Append(get_format_size(info_dict[ben_lenght].getInt()));
                    SizeLabel.Text = sb.ToString();
                    sb.Length = 0;

                    sb.Append(FilesLabel.Text);
                    sb.Append(info_dict[ben_name].getString());
                    FilesLabel.Text = sb.ToString();
                    sb.Length = 0;
                }
                else
                {
                    long size = 0;
                    List<BENObject> files_list = info_dict[new BENObject("files")].getList();
                    BENObject ben_length = new BENObject("length");
                    BENObject ben_path = new BENObject("path");
                    sb.Append(FilesLabel.Text);
                    foreach (BENObject files in files_list)
                    {
                        Dictionary<BENObject, BENObject> file_dict = files.getDictonary();
                        size += file_dict[ben_length].getInt();
                        List<BENObject> list = file_dict[ben_path].getList();
                        Boolean first = true;
                        foreach (BENObject path in list)
                        {
                            sb.Append(first ? "" : "/");
                            sb.Append(path.getString());
                            first = false;
                        }
                        sb.Append("<br>");
                    }
                    FilesLabel.Text = sb.ToString(); ;
                    sb.Length = 0;

                    sb.Append(SizeLabel.Text);
                    sb.Append(get_format_size(size));
                    SizeLabel.Text = sb.ToString();
                    sb.Length = 0;
                }
            }
        }
        void handleTorrent(string sha)
        {
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt = "SELECT name, date, username FROM torrents WHERE sha=@sha";

                using (SqlCommand command = new SqlCommand(stmt, myConnection))
                {
                    command.Parameters.AddWithValue("sha", sha);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            handleRecord(sha, reader);
                        }
                        else
                        {
                            sendMsg("Torrent not found");
                        }
                    }
                }
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["torrent"] == null || Request.QueryString["torrent"].Length != 40)
            {
                sendMsg("Torrent not found");
                return;
            }
            try {
                string sha = Request.QueryString["torrent"];
                handleTorrent(sha);
            }
            catch (Exception ex)
            {
                sendMsg(String.Format("Error: {0}", ex.Message));
            }
        }
    }
}