using System;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace istracker_asp.net
{
    public partial class upload : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
        }
        private bool is_valid_torrent(ref BENObject obj)
        {
            if (obj.getType() == BENObjectType.Dictionary && obj.getDictonary().ContainsKey(new BENObject("info")))
            {
                BENObject info = obj.getDictonary()[new BENObject("info")];
                if (info.getType() == BENObjectType.Dictionary &&
                    info.getDictonary().ContainsKey(new BENObject("piece length")) &&
                    info.getDictonary().ContainsKey(new BENObject("pieces")) &&
                    info.getDictonary().ContainsKey(new BENObject("name")) &&
                    (info.getDictonary().ContainsKey(new BENObject("length")) ||
                    (info.getDictonary().ContainsKey(new BENObject("files")) &&
                     info.getDictonary()[new BENObject("files")].getType() == BENObjectType.List)))
                    return true;
            }
            return false;
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            if (fileToUpload.HasFile)
            {
                Stream fileStream;
                int length = fileToUpload.PostedFile.ContentLength;

                Byte[] Input = new Byte[length];
                fileStream = fileToUpload.FileContent;
                fileStream.Read(Input, 0, length);
                BENObject obj;
                try
                {
                    obj = new BENObject(ref Input);
                }
                catch
                {
                    BotLabel.Text = "Torrent parsing fail";
                    return;
                }
                if (is_valid_torrent(ref obj))
                {
                    obj.getDictonary()[new BENObject("announce")] = new BENObject(ConfigurationManager.AppSettings["config_announce"]);
                    obj.getDictonary().Remove(new BENObject("announce-list"));
                    BENObject info = obj.getDictonary()[new BENObject("info")];

                    SHA1 sha_crypto = new SHA1CryptoServiceProvider();
                    byte[] result = sha_crypto.ComputeHash(info.ToBytes());
                    string sha = BitConverter.ToString(result).Replace("-", string.Empty);
                    BotLabel.Text = sha + "<br />";

                    try {
                        using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                        {
                            myConnection.Open();
                            string stmt = "INSERT INTO torrents (sha, name) values(@sha, @name);";
                            using (SqlCommand myCommand = new SqlCommand(stmt, myConnection))
                            {
                                myCommand.Parameters.AddWithValue("sha", sha);
                                myCommand.Parameters.AddWithValue("name", info.getDictonary()[new BENObject("name")].getString());
                                myCommand.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BotLabel.Text += "Error: " + ex.Message;
                        return;
                    }
                    try {
                        string file_path = String.Format("{0}/{1}.torrent", ConfigurationManager.AppSettings["config_upload_dir"], sha);
                        FileInfo file = new FileInfo(Server.MapPath(file_path));

                        file.Directory.Create();
                        File.WriteAllBytes(file.FullName, obj.ToBytes());

                        BotLabel.Text += "New record created successfully";
                    }
                    catch (Exception ex)
                    {
                        try {
                            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                            {
                                myConnection.Open();
                                string stmt = "DELETE FROM torrents WHERE sha=@sha;";
                                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                                {
                                    cmdCount.Parameters.AddWithValue("sha", sha);
                                    cmdCount.ExecuteNonQuery();
                                }
                            }
                        }
                        catch
                        {
                        }
                        BotLabel.Text += "Error: " + ex.Message;
                    }
                }
                else
                {
                    BotLabel.Text = "Torrent validation fail";
                }
            }
        }
    }
}