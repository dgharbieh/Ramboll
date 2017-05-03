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
        public Result ServiceFocusPage;
        public Result NewsConfiguration;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Websites, "Websites",true);
            Helper.ParseMappingFile(ref ServiceFocusPage, "ServiceFocusPage", true);
            Helper.ParseMappingFile(ref NewsConfiguration, "NewsConfiguration", true);
        }


        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
           
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem("{8F592CB9-A2C3-4F1F-A8CA-A8E48B2582F3}");


                    if (parent != null)
                    {
                     
                       foreach (var lang in Helper.GetLanguages())
                        {
                            try
                            {
                                Websites.CSV.AppendLine(Helper.GetHeader(Websites.Fields));
                                Websites.Totals.Add(lang.ToString(), 0);
                                //here 
                                ServiceFocusPage.Totals.Add(lang.ToString(), 0);
                                NewsConfiguration.Totals.Add(lang.ToString(), 0);


                                string line = Helper.GetFieldsLineWithVersion(parent, ref Websites, lang.ToString());

                                if (!string.IsNullOrEmpty(line))
                                {
                                    Websites.CSV.AppendLine(line);
                                }

                                //
                                GetMultiLanguageVersionData(parent, lang);
                                Helper.CreateFile(Websites.CSV.ToString(), Websites.OutputName + "_" + lang);
                                Websites.CSV.Clear();
                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }
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

        private void GetMultiLanguageVersionData( Item parent,Language lang)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (var item in parent.Children.AsEnumerable())
            {
                try
                {
                    Item sub = Helper.GetDatabase().GetItem(item.ID, lang);

                if (sub.TemplateName.ToLower() == Websites.TemplateName.Trim().ToLower())
                {
                    string line = Helper.GetFieldsLineWithVersion(sub,ref Websites, lang.ToString()); 
                    if (!string.IsNullOrEmpty(line))
                    {
                        Websites.CSV.AppendLine(line);
                    }            
                }


                if (sub.TemplateName.ToLower() == ServiceFocusPage.TemplateName.Trim().ToLower())
                {
                    string line = Helper.GetFieldsLineWithVersion(sub, ref ServiceFocusPage, lang.ToString());
                    if (!string.IsNullOrEmpty(line))
                    {
                        Websites.CSV.AppendLine(line);
                    }
                }



                if (sub.TemplateName.ToLower() == NewsConfiguration.TemplateName.Trim().ToLower())
                {
                    string line = Helper.GetFieldsLineWithVersion(sub, ref NewsConfiguration, lang.ToString());
                    if (!string.IsNullOrEmpty(line))
                    {
                        Websites.CSV.AppendLine(line);
                    }
                }
                GetMultiLanguageVersionData(item, lang);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

      

    }
}