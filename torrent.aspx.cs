using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace istracker_asp.net
{
    public partial class torrent : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        private void deleteAction(string sha)
        {
            if (Session["login"] == null)
            {
                throw new InvalidOperationException("user is not logged in");
            }

            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt = "SELECT username FROM torrents WHERE sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    using (SqlDataReader reader = cmdCount.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if(reader.GetString(0) != (String)Session["username"])
                                throw new InvalidOperationException("this user can delete this file");
                        }

                    }
                }
                stmt = "DELETE FROM torrents WHERE sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    if (cmdCount.ExecuteNonQuery() != 0)
                    {
                        try
                        {
                            string file_path = String.Format("{0}/{1}.torrent", ConfigurationManager.AppSettings["config_upload_dir"], sha);
                            FileInfo file = new FileInfo(Server.MapPath(file_path));
                            if (file.Exists)
                                File.Delete(file.FullName);
                        }
                        catch
                        {
                            throw new InvalidOperationException("unable to delete file");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("record not found");
                    }
                }
            }
        }
        private int getRows()
        {
            int rows;
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt = "SELECT COUNT(*) FROM torrents;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    rows = (int)cmdCount.ExecuteScalar();
                }
            }
            return rows;
        }
        private void handlePeers(string sha, DataRow row)
        {
            string stmt;
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                stmt = "SELECT COUNT(*) FROM peers WHERE sha=@sha and dl_left=0;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    row["S"] = String.Format("<a href=\"peers.aspx?torrent={0}\">{1}</a>", sha, cmdCount.ExecuteScalar());
                }
                stmt = "SELECT COUNT(*) FROM peers WHERE sha=@sha and dl_left!=0;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    row["P"] = String.Format("<a href=\"peers.aspx?torrent={0}\">{1}</a>", sha, cmdCount.ExecuteScalar());
                }
                stmt = "SELECT downloaded FROM torrents WHERE sha=@sha;";
                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("sha", sha);
                    row["C"] = cmdCount.ExecuteScalar();
                }
            }
        }
        private void createTorrentRows(int offset, int max_pages, DataTable torrentDataTable)
        {
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt;
                if (max_pages > 0)
                    stmt = "SELECT sha, name, date, username FROM torrents ORDER BY id DESC OFFSET @offset ROWS FETCH NEXT @max_pages ROWS ONLY;";
                else
                    stmt = "SELECT sha, name, date, username FROM torrents ORDER BY id DESC";

                using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                {
                    cmdCount.Parameters.AddWithValue("offset", offset);
                    cmdCount.Parameters.AddWithValue("max_pages", max_pages);
                    using (SqlDataReader reader = cmdCount.ExecuteReader())
                    {
                        DataRow row = null;
                        while (reader.Read())
                        {
                            String user = reader.GetString(3);
                            row = torrentDataTable.NewRow();
                            row["Torrent"] = String.Format("<a href=\"show.aspx?torrent={0}\">{1}</a>", reader.GetString(0), reader.GetString(1));
                            row["User"] = user;
                            row["Date"] = reader.GetDateTime(2);
                            row["Download"] = String.Format("<a href=\"{0}/{1}.torrent\" download=\"{2}.torrent\">[D]</a>", ConfigurationManager.AppSettings["config_upload_dir"], reader.GetString(0), reader.GetString(1));
                            if (Session["login"] != null)
                            {
                                if (user == (String)Session["username"])
                                {
                                    row["Delete"] = String.Format("<a href=\"torrent.aspx?action=delete&sha={0}\">[X]</a>", reader.GetString(0));
                                }
                                else
                                {
                                    row["Delete"] = "";
                                }
                            }
                            handlePeers(reader.GetString(0), row);

                            torrentDataTable.Rows.Add(row);
                        }
                    }
                }
            }
        }
        private DataTable createTorrentDataTable()
        {
            DataTable ret = new DataTable("Torrent");
            DataColumn col = null;

            col = new DataColumn("Torrent");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);


            col = new DataColumn("User");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Date");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Download");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Delete");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("S");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("P");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("C");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);
            return ret;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            int page = 1;

            if (Request.QueryString["page"] != null)
            {
                Int32.TryParse(Request.QueryString["page"], out page);
                if (page < 1)
                    page = 1;
            }

            if (Request.QueryString["action"] != null)
            {
                if (Request.QueryString["action"].Equals("delete") && Request.QueryString["sha"] != null)
                {
                    try {
                        deleteAction(Request.QueryString["sha"]);
                        TopLabel.Text = "Record deleted successfully<br/>";
                    }
                    catch (Exception ex)
                    {
                        TopLabel.Text = String.Format("Error deleting record: {0}<br/>", ex.Message);
                        return;
                    }
                }
            }
            
            int rows = 0;
            try
            {
                rows = getRows();
            }
            catch (Exception ex)
            {
                TopLabel.Text = "Error: " + ex.Message;
                return;
            }
            if (rows > 0)
            {
                DataTable torrentDataTable = createTorrentDataTable();

                int max_pages;
                Int32.TryParse(ConfigurationManager.AppSettings["config_max_torrent_page"], out max_pages);

                int pages;
                if (max_pages > 0)
                    pages = (int)Math.Ceiling((double)rows / max_pages);
                else
                    pages = 1;

                if (page > pages)
                    page = pages;
                int offset = (page-1) * max_pages;

                try {
                    createTorrentRows(offset, max_pages, torrentDataTable);
                }
                catch (Exception ex)
                {
                    TopLabel.Text = String.Format("Error: {0}", ex.Message);
                    return;
                }
                TorrentList.DataSource = torrentDataTable;
                TorrentList.DataBind();
                BotLabel.Text = "Page";
                for (int i = 1; i <= pages; ++i)
                {
                    BotLabel.Text += String.Format(" <a href=\"torrent.aspx?page={0}\">{0}</a>", i);
                }
            }
            else
            {
                TopLabel.Text += "0 results";
            }
        }
    }
}