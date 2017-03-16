using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RambollImportData.Helpers;
using Sitecore.SecurityModel;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Data;
using Sitecore.Data.Managers;
using Sitecore.Collections;
using Sitecore.Globalization;
using RambollImportData.Base;

namespace RambollImportData.sitecore.admin
{
    public partial class ImportCounties : System.Web.UI.Page
    {
        public  string NotMatchCountries = "";
        public int UpdatedRecords =0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        public Dictionary<string, string> InsertedVersionsTotals = new Dictionary<string, string>();
        public Dictionary<string, string> InsertedNewTotals = new Dictionary<string, string>();
        public Dictionary<string, string> UpdateTotals = new Dictionary<string, string>();
        public Result Countries;

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Countries, "Countries");
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {

                Dictionary<string, DataTable> dataTables = Helper.GetLanguagesDataTable(Countries);

                using (new SecurityDisabler())
                {
                    Database masterDb = Helper.GetDatabase();     
                    TemplateItem template = masterDb.GetItem("/sitecore/templates/System/Analytics/Country");
                    foreach (var lang in Helper.GetDatabase().Languages)
                    {
                        UpdatedRecords = InsertedVersionsRecords = 0;

                        Item parent = masterDb.GetItem(Countries.StartPath, lang);
                        List<Item> countries = parent.Children.AsEnumerable().ToList();
                        DataTable data = dataTables[lang.Name];
                        foreach (DataRow row in data.Rows)
                        {

                            Item Country = countries.Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower() || x.Name.ToLower() == row["CountryName"].ToString().ToLower()).FirstOrDefault();


                            if (Country != null)
                            {
                                UpdateItem(Country, row["ID"].ToString(), row["Version"].ToString(), lang);
                               
                            }
                            else
                            {
                                Country = countries.Where(x => x.Name.ToLower().Contains(row["Name"].ToString().ToLower())).FirstOrDefault();

                                //Manual Mapping
                                if (row["Name"].ToString().ToLower() == "Congo-DR".ToLower())
                                {
                                    Country = countries.Where(x => x.Name.ToLower() == "Congo The Democratic Republic of the".ToLower()).FirstOrDefault();
                                }
                                if (row["Name"].ToString().ToLower() == "USA".ToLower())
                                {
                                    Country = countries.Where(x => x.Name.ToLower() == "United States".ToLower()).FirstOrDefault();
                                }


                                if (Country == null)
                                {
                                    Item newCountry = parent.Add(row["Name"].ToString(), template);
                                    newCountry.Editing.BeginEdit();
                                    newCountry["Old Id"] = row["ID"].ToString();
                                    newCountry.Editing.EndEdit();

                                    InsertedNewRecords = InsertedNewRecords + 1;
                                    NotMatchCountries +=  row["Name"].ToString() + "<br/>";
                                }
                                else
                                {
                                    UpdateItem(Country, row["ID"].ToString(), row["Version"].ToString(), lang);

                                }

                            }
                        }
                        InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                        InsertedVersionsTotals.Add(lang.Name, InsertedVersionsRecords.ToString());
                        UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
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

        private void UpdateItem(Item Country, string OldId, string Version, Language lang)
        {

            try
            {

                Item item = Country.Database.GetItem(Country.ID, lang);
                int versionNumber = 0;
                int.TryParse(Version,out versionNumber);
                if (item.Versions.Count > 0)
                {
                    if (versionNumber<= item.Versions.Count)
                    {
                        Item[] versions = item.Versions.GetVersions();
                        versions[versionNumber - 1].Editing.BeginEdit();
                        versions[versionNumber - 1]["Old Id"] = OldId;
                        versions[versionNumber - 1].Editing.EndEdit();
                        UpdatedRecords = UpdatedRecords + 1;
                    }else
                    {
                        Item newVersion = item.Versions.AddVersion();
                        newVersion.Editing.BeginEdit();
                        newVersion["Old Id"] = OldId;
                        newVersion.Editing.EndEdit();
                        InsertedVersionsRecords = InsertedVersionsRecords + 1;
                    }
                }
                else
                {
                    Item newVersion = item.Versions.AddVersion();
                    newVersion.Editing.BeginEdit();
                    newVersion["Old Id"] = OldId;
                    newVersion.Editing.EndEdit();
                    InsertedVersionsRecords = InsertedVersionsRecords + 1;
                }
               
            }
            catch (Exception ex)
            {
                Country.Editing.CancelEdit();
                throw ex;
            }
        }
    }
}