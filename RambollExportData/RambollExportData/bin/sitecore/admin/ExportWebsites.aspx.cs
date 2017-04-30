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
    public partial class ExportWebsites : System.Web.UI.Page
    {

        public Result Websites;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Websites, "Websites",true);
        }


        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
           
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(Websites.StartPath.Trim());
                  
                    if (parent != null)
                    {
                     
                       foreach (var lang in parent.Languages)
                       {
                           Websites.CSV.AppendLine(Helper.GetHeader(Websites.Fields));
                           Websites.Totals.Add(lang.ToString(), 0);
                           GetMultiLanguageVersionData(Websites, parent, lang);
                           Helper.CreateFile(Websites.CSV.ToString(), Websites.OutputName + "_" + lang);
                           Websites.CSV.Clear();
                       }
                    }           
                
                 
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

        private void GetMultiLanguageVersionData(Result resultItem, Item parent,Language lang)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (var item in parent.Children.AsEnumerable())
            {
                Item sub = Helper.GetDatabase().GetItem(item.ID, lang);

                if (sub.TemplateName.ToLower() == resultItem.TemplateName.Trim().ToLower())
                {

                    string line = Helper.GetFieldsLineWithVersion(sub,ref resultItem, lang.ToString());
                  
                    if (!string.IsNullOrEmpty(line))
                    {
                        resultItem.CSV.AppendLine(line);
                    }
                    
                }

                //GetMultiLanguageVersionData(resultItem,item, lang);
            }
        }

      

    }
}