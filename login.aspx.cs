using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace istracker_asp.net
{
    public partial class login : System.Web.UI.Page
    {
        static string DatabaseConnectionString = ConfigurationManager.ConnectionStrings["dbConStr"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                Session.RemoveAll();
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try {
                using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                {
                    myConnection.Open();
                    string stmt = "SELECT password FROM users WHERE username=@username;";
                    using (SqlCommand cmdCount = new SqlCommand(stmt, myConnection))
                    {
                        cmdCount.Parameters.AddWithValue("username", userName.Text.ToLower());
                        using (SqlDataReader reader = cmdCount.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.GetString(0) == password.Text)
                                {
                                    Session["login"] = true;
                                    Session["username"] = userName.Text.ToLower();
                                    Response.Redirect("torrent.aspx");
                                    return;
                                }
                            }
                        }
                    }
                }
            } catch
            {
                Error.Text = "Unable to connecto db";
            }
            Error.Text = "Wrong username or password";
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (userName.Text.Length == 0 || password.Text.Length == 0)
            {
                Error.Text = "Username too short";
            }
            try {
                using (SqlConnection myConnection = new SqlConnection(DatabaseConnectionString))
                {
                    myConnection.Open();
                    string stmt = "INSERT INTO users (username, password) values(@username, @password);";
                    try
                    {
                        using (SqlCommand myCommand = new SqlCommand(stmt, myConnection))
                        {
                            myCommand.Parameters.AddWithValue("username", userName.Text.ToLower());
                            myCommand.Parameters.AddWithValue("password", password.Text);
                            myCommand.ExecuteNonQuery();
                            Session["login"] = true;
                            Session["username"] = userName.Text.ToLower();
                            Response.Redirect("torrent.aspx");
                        }
                    }
                    catch
                    {
                        Error.Text = "Unable to add user";
                    }
                }
            } catch
            {
                Error.Text = "Unable to connecto db";
            }
        }
    }
}