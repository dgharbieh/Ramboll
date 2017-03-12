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
    public partial class ImportProjects : System.Web.UI.Page
    {
        public Result ProjectsFolders;
        public Result Projects;

        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref ProjectsFolders, "ProjectsFolders");
            Helper.ParseMappingFile(ref Projects, "Projects");
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                ImportDataTable(Helper.GetDataTable(ProjectsFolders), ProjectsFolders);
             //   ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(Projects), Projects);

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }

        protected void ImportDataTable(DataTable data, Result ProjectsFolders)
        {
            using (new SecurityDisabler())
            {
              
                Database masterDb = Helper.GetDatabase();
                TemplateItem template = masterDb.GetItem(ProjectsFolders.TemplateName);
                Item parent;
                Item folder;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                    if (row["Path"].ToString().ToLower() == (ProjectsFolders.ExportPath + "/" + row["Name"].ToString()).ToLower())
                    {
                        parent = masterDb.GetItem(ProjectsFolders.StartPath.Trim());
                    }
                    else
                    {                    
                        string url = row["Path"].ToString().ToLower().Replace(ProjectsFolders.ExportPath.ToLower(), ProjectsFolders.StartPath.ToLower());
                        parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')));
                    }

                    folder = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                    if (folder == null)
                    {
                        Item newProjects = parent.Add(row["Name"].ToString(), template);
                        newProjects.Editing.BeginEdit();
                        newProjects["Old Id"] = row["ID"].ToString();
                        if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                        { newProjects["__Masters"] = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}|{ED2FBBC9-26E0-44B1-B60C-17E753E2E819}"; }
                        newProjects.Editing.EndEdit();
                        ProjectsFolders.InsertedNewRecords = ProjectsFolders.InsertedNewRecords + 1;
                    }
                    else
                    {
                        folder.Editing.BeginEdit();
                        folder["Old Id"] = row["ID"].ToString();
                        if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                        {
                            folder["__Masters"] = "{A87A00B1-E6DB-45AB-8B54-636FEC3B5523}|{ED2FBBC9-26E0-44B1-B60C-17E753E2E819}";
                        }
                        folder.Editing.EndEdit();
                        ProjectsFolders.UpdatedRecords = ProjectsFolders.UpdatedRecords + 1;
                    }

                     }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
                throw ex;
            }

                }
            }
        }

        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result Projects)
        {
            UpdatedRecords = InsertedVersionsRecords = 0;
            Database masterDb = Helper.GetDatabase();
            TemplateItem template = masterDb.GetItem(Projects.TemplateName);
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

                    if (row["Path"].ToString().ToLower() == (Projects.ExportPath + "/" + row["Name"].ToString()).ToLower())
                    {
                        parent = masterDb.GetItem(Projects.StartPath.Trim(),lang);
                    }
                    else
                    {
                        string url = row["Path"].ToString().ToLower().Replace(Projects.ExportPath.ToLower(), Projects.StartPath.ToLower());
                        parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                    }

                    alias = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                    if (alias == null)
                    {
                        Item newProjects = parent.Add(row["Name"].ToString(), template);
                        newProjects.Editing.BeginEdit();
                        newProjects["Old Id"] = row["ID"].ToString();
                        newProjects["Linked item"] = row["LinkTo"].ToString();
                        newProjects.Editing.EndEdit();
                        Projects.InsertedNewRecords = Projects.InsertedNewRecords + 1;
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

                Projects.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                Projects.InsertedVersionsTotals.Add(lang.Name, InsertedVersionsRecords.ToString());
                Projects.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

        }
    }
}