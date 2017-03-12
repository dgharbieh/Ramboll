using RambollImportData.Base;
using RambollImportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RambollImportData.sitecore.admin
{
    public partial class ImportAliases : System.Web.UI.Page
    {
        public Result AliasesFolders;
        public Result Aliases;

        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref AliasesFolders, "AliasesFolders");
            Helper.ParseMappingFile(ref Aliases, "Aliases");
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                ImportDataTable(Helper.GetDataTable(AliasesFolders), AliasesFolders);
                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(Aliases), Aliases);

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }

        protected void ImportDataTable(DataTable data, Result AliasesFolders)
        {
            using (new SecurityDisabler())
            {
                Database masterDb = Helper.GetDatabase();
                TemplateItem template = masterDb.GetItem(AliasesFolders.TemplateName);
                Item parent;
                Item folder;
                foreach (DataRow row in data.Rows)
                {

                    if (row["Path"].ToString().ToLower() == (AliasesFolders.ExportPath + "/" + row["Name"].ToString()).ToLower())
                    {
                        parent = masterDb.GetItem(AliasesFolders.StartPath.Trim());
                    }
                    else
                    {
                        parent = masterDb.GetItem(row["Path"].ToString().ToLower().Replace(AliasesFolders.ExportPath.ToLower(), AliasesFolders.StartPath.ToLower()).Replace("/" + row["Name"].ToString().ToLower(), string.Empty));
                    }

                    folder = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                    if (folder == null)
                    {
                        Item newAliases = parent.Add(row["Name"].ToString(), template);
                        newAliases.Editing.BeginEdit();
                        newAliases["Old Id"] = row["ID"].ToString();
                        newAliases.Editing.EndEdit();
                        AliasesFolders.InsertedNewRecords = AliasesFolders.InsertedNewRecords + 1;
                    }
                    else
                    {
                        folder.Editing.BeginEdit();
                        folder["Old Id"] = row["ID"].ToString();
                        folder.Editing.EndEdit();
                        AliasesFolders.UpdatedRecords = AliasesFolders.UpdatedRecords + 1;
                    }



                }
            }
        }

        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result Aliases)
        {
            UpdatedRecords = InsertedVersionsRecords = 0;
            Database masterDb = Helper.GetDatabase();
            TemplateItem template = masterDb.GetItem(Aliases.TemplateName);
            foreach (var lang in Helper.GetDatabase().Languages)
            {
                DataTable data = dataTables[lang.ToString()];

                UpdatedRecords = InsertedVersionsRecords = 0;

                Item parent;
                Item alias;
                foreach (DataRow row in data.Rows)
                {
                    int versionNumber = 0;
                    int.TryParse(row["Version"].ToString(), out versionNumber);

                    if (row["Path"].ToString().ToLower() == (Aliases.ExportPath + "/" + row["Name"].ToString()).ToLower())
                    {
                        parent = masterDb.GetItem(Aliases.StartPath.Trim(),lang);
                    }
                    else
                    {
                        string url = row["Path"].ToString().ToLower().Replace(Aliases.ExportPath.ToLower(), Aliases.StartPath.ToLower());
                        parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                    }

                    alias = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                    if (alias == null)
                    {
                        Item newAliases = parent.Add(row["Name"].ToString(), template);
                        newAliases.Editing.BeginEdit();
                        newAliases["Old Id"] = row["ID"].ToString();
                        newAliases["Linked item"] = row["LinkTo"].ToString();
                        newAliases.Editing.EndEdit();
                        Aliases.InsertedNewRecords = Aliases.InsertedNewRecords + 1;
                    }
                    else
                    {
                        if (alias.Versions.Count > 0)
                        {
                            if (versionNumber <= alias.Versions.Count)
                            {
                                Item[] versions = alias.Versions.GetVersions();
                                versions[versionNumber - 1].Editing.BeginEdit();
                                versions[versionNumber - 1]["Old Id"] = row["ID"].ToString();
                                versions[versionNumber - 1]["Linked item"] = row["LinkTo"].ToString();
                                versions[versionNumber - 1].Editing.EndEdit();
                                UpdatedRecords = UpdatedRecords + 1;
                            }
                            else
                            {
                                Item newVersion = alias.Versions.AddVersion();
                                newVersion.Editing.BeginEdit();
                                newVersion["Old Id"] = row["ID"].ToString();
                                newVersion["Linked item"] = row["LinkTo"].ToString();
                                newVersion.Editing.EndEdit();
                                InsertedVersionsRecords = InsertedVersionsRecords + 1;
                            }

                        }
                        else
                        {
                            Item newVersion = alias.Versions.AddVersion();
                            newVersion.Editing.BeginEdit();
                            newVersion["Old Id"] = row["ID"].ToString();
                            newVersion["Linked item"] = row["LinkTo"].ToString();
                            newVersion.Editing.EndEdit();
                            InsertedVersionsRecords = InsertedVersionsRecords + 1;
                        }
                    }


                 

                }

                Aliases.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                Aliases.InsertedVersionsTotals.Add(lang.Name, InsertedVersionsRecords.ToString());
                Aliases.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

        }
    }
}