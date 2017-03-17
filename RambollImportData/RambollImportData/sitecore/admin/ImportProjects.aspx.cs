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
            Helper.ParseMappingFile(ref Projects, "Projects",true);
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                ImportDataTable(Helper.GetDataTable(ProjectsFolders), ProjectsFolders);
                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(Projects), Projects);

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
            UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;
            Database masterDb = Helper.GetDatabase();
            TemplateItem template = masterDb.GetItem(Projects.TemplateName);
            foreach (var lang in Helper.GetDatabase().Languages)
            {
                DataTable data = dataTables[lang.ToString()];

                UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords=0;

                Item parent;
                Item project;
                foreach (DataRow row in data.Rows)
                {
                  try
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

                    project = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                    if (project == null)
                    {
                        Item newProjects = parent.Add(row["Name"].ToString(), template);

                        this.UpdateItem(ref newProjects, row);
                        InsertedNewRecords = InsertedNewRecords + 1;
                    }
                    else
                    {
                        if (project.Versions.Count > 0)
                        {
                            if (versionNumber <= project.Versions.Count)
                            {
                                Item[] versions = project.Versions.GetVersions();

                                this.UpdateItem(ref  versions[versionNumber - 1],row);
                                UpdatedRecords = UpdatedRecords + 1;
                            }
                            else
                            {
                                Item newVersion = project.Versions.AddVersion();
                                this.UpdateItem(ref newVersion,row);
                                InsertedNewRecords = InsertedNewRecords + 1;

                            }

                        }
                        else
                        {
                            Item newVersion = project.Versions.AddVersion();
                            this.UpdateItem(ref newVersion,row);
                            InsertedNewRecords = InsertedNewRecords + 1;

                        }
                    }


                      }
                      catch (Exception ex)
                      {
                          throw (ex);
                      }

                }

                Projects.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                Projects.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

        }

        private void UpdateItem(ref Item item, DataRow row)
        {

            item.Editing.BeginEdit();
            item["Old Id"] = row["ID"].ToString();

            for (var i = 0; i < Projects.ImportedFields.Count - 1; i++)
            {
                try
                {
                    string importField = Projects.ImportedFields[i].ToString();
                    if (!string.IsNullOrEmpty(importField))
                    {
                        string exportField = Projects.ExportedFields[i].ToString();
                        if (exportField == "PersonsOnProject")
                        {

                            item[importField.Trim()] = (string.IsNullOrEmpty(item[importField.Trim()]) ? row[exportField].ToString() : item[importField.Trim()] + "|" + row[exportField].ToString());
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
        }
        
    }
}