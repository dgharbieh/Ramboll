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

        public List<Result> FullWebsites = new List<Result>();
        // "NewsReference" , "EventsReference","RichPageReferenceCollection"
        public string[] Templates = { "Websites", "ServiceFocusPage", "RichPageReference", "FeaturePageReference" };


        protected void Page_Load(object sender, EventArgs e)
        {

            foreach (string temp in Templates)
            {
                Result result = new Result();

                Helper.ParseMappingFile(ref result, temp, true);

                FullWebsites.Add(result);
            }
        }

        protected void ExportData(object sender, EventArgs e)
        {
            try
            {

                using (new SecurityDisabler())
                {
                    var Websites = FullWebsites[0];

                    Database masterDb = Helper.GetDatabase();
                   Item parent = masterDb.GetItem(Websites.StartPath);
                    //TestData
         

                    if (parent != null)
                    {

                        foreach (var lang in Helper.GetLanguages())
                        {
                            try
                            {
                                foreach (var result in FullWebsites)
                                {

                                    result.CSV.AppendLine(Helper.GetHeader(result.Fields));
                                    result.Totals.Add(lang.ToString(), 0);
                                }

                               // Item child = masterDb.GetItem("{EBB0FBC2-F6F9-49A1-AE4C-01D284A759DC}");

                                foreach (Item child in parent.Children)
                                {
                                    if (child.TemplateName == "StandardWebsite")
                                    {
                                        string line = Helper.GetFieldsLineWithVersion(child, ref Websites, lang.ToString());                     
                     
                                        if (!string.IsNullOrEmpty(line))
                                        {
                                            Websites.CSV.AppendLine(line);
                                        }


                                        GetMultiLanguageVersionData(child, lang);
                                    }
                                }

                                foreach (var result in FullWebsites)
                                {

                                    Helper.CreateFile(result.CSV.ToString(), result.OutputName + "_" + lang);
                                    result.CSV.Clear();
                                }


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

        private void GetMultiLanguageVersionData(Item parent, Language lang)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (var item in parent.Children.AsEnumerable())
            {
                try
                {
                    Item sub = Helper.GetDatabase().GetItem(item.ID, lang);


                    for (var i = 0; i < FullWebsites.Count(); i++)
                    {
                        var result = FullWebsites[i];

                        if (sub.TemplateName.ToLower() == result.TemplateName.Trim().ToLower())
                        {
                            string line = Helper.GetFieldsLineWithVersion(sub, ref result, lang.ToString());
                            if (!string.IsNullOrEmpty(line))
                            {
                                result.CSV.AppendLine(line);
                            }
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