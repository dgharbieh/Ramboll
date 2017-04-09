using RambollExportData.Base;
using RambollExportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RambollExportData.sitecore.admin
{
    public partial class ExportRedirectItems : System.Web.UI.Page
    {
    
        protected void Page_Load(object sender, EventArgs e)
        {

        }


        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
                Result Redirects = new Result();
                Database masterDb = Helper.GetDatabase();
                Item redirectItem = masterDb.GetItem("{0A25A514-5FAC-407F-8DE5-4FABA328265F}");

                string Allredirects=  redirectItem.Fields["AllRedirects"].Value;
                Redirects.CSV.AppendLine("Language,From,To");

                foreach (var redirect in Allredirects.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string language = redirect.Substring(0, 2);
                    string from = redirect.Substring(2, 38);
                    string to = redirect.Substring(45, 38);

                    Redirects.CSV.AppendLine(language + ","+ from +"," +to);
                }

                Helper.CreateFile(Redirects.CSV.ToString(), "RedirectItems");
                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }

        }

    }
}