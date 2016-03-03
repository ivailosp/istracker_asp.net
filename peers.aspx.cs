using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace istracker_asp.net
{
    public partial class peers : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        private string get_format_size(double size)
        {
            int step = 0;
            string[] arr = new string[] { " B", " KB", " MB", " GB" };
            for (step = 0; step < arr.Length - 1 && size >= 1000; step++)
            {
                size /= 1024;
            }

            return size.ToString("0.00") + arr[step];
        }
        private void handlePeers(string sha, DataTable peersDataTable)
        {
            using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
            {
                myConnection.Open();
                string stmt = "SELECT ip, port, uploaded, downloaded, dl_left FROM peers WHERE sha=@sha";

                using (SqlCommand command = new SqlCommand(stmt, myConnection))
                {
                    command.Parameters.AddWithValue("sha", sha);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataRow row = null;
                        while (reader.Read())
                        {
                            row = peersDataTable.NewRow();
                            row["IP"] = reader.GetString(0);
                            row["Port"] = reader.GetInt32(1).ToString();
                            row["Uploaded"] = get_format_size(reader.GetInt64(2));
                            row["Downloaded"] = get_format_size(reader.GetInt64(3));
                            row["Left"] = get_format_size(reader.GetInt64(4)); ;
                            peersDataTable.Rows.Add(row);
                        }
                    }
                }
            }
        }
        private DataTable createPeersDataTable()
        {
            DataTable ret = new DataTable("Peers");
            DataColumn col = null;

            col = new DataColumn("IP");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Port");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Uploaded");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Downloaded");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);

            col = new DataColumn("Left");
            col.DataType = System.Type.GetType("System.String");
            ret.Columns.Add(col);


            return ret;
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["torrent"] == null || Request.QueryString["torrent"].Length != 40)
            {
                Msg.Text = "Torrent not found";
                return;
            }
            
            try {
                DataTable peersDataTable = createPeersDataTable();
                string sha = Request.QueryString["torrent"];
                handlePeers(sha, peersDataTable);
                PeerList.DataSource = peersDataTable;
                PeerList.DataBind();
            }
            catch (Exception ex)
            {
                Msg.Text = String.Format("Error: {0}", ex.Message);
            }
        }
    }
}