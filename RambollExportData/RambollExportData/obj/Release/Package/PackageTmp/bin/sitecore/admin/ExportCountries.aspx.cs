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
    public partial class ExportCountries : BasePage
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(this, "Countries");

        }

        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
                CSV.AppendLine(Helper.GetHeader(this.Fields));
                using (new SecurityDisabler())
                {
                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(this.StartPath);

                    foreach (var item in parent.Children.AsEnumerable())
                    {
                        CSV.AppendLine(Helper.GetFieldsLine(item, Fields));
                    }
                    Helper.CreateFile(CSV.ToString(),this.OutputName);
                    RecourdNumber = parent.Children.Count();
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
