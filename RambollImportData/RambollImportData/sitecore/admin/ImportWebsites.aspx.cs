using RambollImportData.Base;
using RambollImportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RambollImportData.sitecore.admin
{
    public partial class ImportWebsites : System.Web.UI.Page
    {

        public Result Websites;
        DataTable CountriesIds;

        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        protected void Page_Load(object sender, EventArgs e)
        {

            Helper.ParseMappingFile(ref Websites, "Websites", true);
        }
        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                CountriesIds = Helper.GetIdsMatchDataTable("Countries");

                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(Websites), Websites);

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }

        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result Websites)
        {
            UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;
            Database masterDb = Helper.GetDatabase();
            TemplateItem template = masterDb.GetItem(Websites.TemplateName);
            DataTable Ids = Helper.GetIdsMatchDataTable("Websites");

            foreach (var lang in Helper.GetDatabase().Languages)
            {
                DataTable data = dataTables[lang.ToString()];

                UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;

                Item parent;
                Item project;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        int versionNumber = 0;
                        int.TryParse(row["Version"].ToString(), out versionNumber);

                        if (row["Path"].ToString().ToLower() == (Websites.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(Websites.StartPath.Trim(), lang);
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(Websites.ExportPath.ToLower(), Websites.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                        }

                        project = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();


                        if (project == null)
                        {
                            Item newWebsites = parent.Add(row["Name"].ToString(), template);

                            this.UpdateItem(ref newWebsites, row ,ref Ids);
                            InsertedNewRecords = InsertedNewRecords + 1;
                        }
                        else
                        {
                            if (project.Versions.Count > 0)
                            {
                                if (versionNumber <= project.Versions.Count)
                                {
                                    Item[] versions = project.Versions.GetVersions();

                                    this.UpdateItem(ref versions[versionNumber - 1], row, ref Ids);
                                    UpdatedRecords = UpdatedRecords + 1;
                                }
                                else
                                {
                                    Item newVersion = project.Versions.AddVersion();
                                    this.UpdateItem(ref newVersion, row, ref Ids);
                                    InsertedNewRecords = InsertedNewRecords + 1;

                                }

                            }
                            else
                            {
                                Item newVersion = project.Versions.AddVersion();
                                this.UpdateItem(ref newVersion, row, ref Ids);
                                InsertedNewRecords = InsertedNewRecords + 1;

                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }

                }

                Websites.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                Websites.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

            Helper.FromDataTableToExcel(Ids, "Websites");
        }

        private void UpdateItem(ref Item item, DataRow row, ref DataTable Ids)
        {

            item.Editing.BeginEdit();
            item["Old Id"] = row["ID"].ToString();

            for (var i = 0; i < Websites.ImportedFields.Count; i++)
            {

                try
                {
                    string importField = Websites.ImportedFields[i].ToString();
                    if (!string.IsNullOrEmpty(importField))
                    {
                        string exportField = Websites.ExportedFields[i].ToString();


                        if (importField.ToLower() == "country")
                        {
                            if (!string.IsNullOrEmpty(row[exportField].ToString().Trim()))
                            {
                                row[exportField] = row[exportField].ToString().Replace(row[exportField].ToString(), Helper.MapItemId(CountriesIds, row[exportField].ToString()));
                            }

                            item[importField.Trim()] = row[exportField].ToString();

                        }
                        else
                        {
                            item[importField.Trim()] = row[exportField].ToString();
                        }
     
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
            item.Editing.EndEdit();
            Helper.UpdateIds(ref Ids, item);
        }


    }
}