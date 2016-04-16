using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace istracker_asp.net
{
    public partial class logout : System.Web.UI.Page
    {
        static string prevPage = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Page.IsPostBack == false)
            {
                if (Request.UrlReferrer != null)
                {
                    prevPage = Request.UrlReferrer.ToString();
                }
                if (Session["login"] == null)
                {
                    Response.Redirect("login.aspx");
                }
            }
 
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            Session.RemoveAll();
            if (prevPage != null)
            {
                Response.Redirect(prevPage);
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            if (prevPage != null)
            {
                Response.Redirect(prevPage);
            }
        }
    }
}