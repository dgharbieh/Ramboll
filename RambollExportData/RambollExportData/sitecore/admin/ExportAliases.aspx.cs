﻿using RambollExportData.Base;
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
    public partial class ExportAliases :   System.Web.UI.Page
    {
        public Result Folders;
        public Result Aliases;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Folders, "AliasesFolders");
            Helper.ParseMappingFile(ref Aliases, "Aliases");

        }


        protected void ExportData(object sender, EventArgs e)
        {
            try
            {
                Folders.CSV.AppendLine(Helper.GetHeader(Folders.Fields));
              
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(Folders.StartPath.Trim());
                  
                    if (parent != null)
                    {
                        GeFolderstData(parent);

                        Helper.CreateFile(Folders.CSV.ToString(), Folders.OutputName);

                        foreach (var lang in parent.Languages)
                        {
                            Aliases.CSV.AppendLine(Helper.GetHeader(Aliases.Fields));
                            Aliases.Totals.Add(lang.ToString(), 0);
                            GetMultiLanguageVersionData(Aliases,parent,lang );
                            Helper.CreateFile(Aliases.CSV.ToString(), Aliases.OutputName +"_" +lang );
                            Aliases.CSV.Clear();
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
                    string line = Helper.GetFieldsLineWithVersion(sub,ref resultItem,lang.ToString());
                    if (!string.IsNullOrEmpty(line))
                    {
                        resultItem.CSV.AppendLine(line);
                    }
                }

                GetMultiLanguageVersionData(resultItem,item, lang);
            }
        }

        private void GeFolderstData(Item parent)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (var item in parent.Children.AsEnumerable())
            {

                if (item.TemplateName.ToLower() == Folders.TemplateName.Trim().ToLower())
                {
                  Folders.CSV.AppendLine(Helper.GetFieldsLine(item, Folders.Fields));
                 Folders.RecourdNumber = Folders.RecourdNumber + 1;
                }
                GeFolderstData(item);
            }
        }



    }
}