using RambollExportData.Base;
using RambollExportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
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
    public partial class ExportCountries : System.Web.UI.Page
    {
        public Result Countries;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Countries, "Countries");

        }

        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
                Countries.CSV.AppendLine(Helper.GetHeader(Countries.Fields));
                using (new SecurityDisabler())
                {
                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(Countries.StartPath);
                    Helper.GenerateLanguagesFiles(parent, Countries.Fields, Countries.OutputName, Countries.Totals);
                    Countries.RecourdNumber = parent.Children.Count();
                }

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
