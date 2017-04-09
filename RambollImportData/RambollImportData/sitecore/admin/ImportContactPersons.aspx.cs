using RambollImportData.Base;
using RambollImportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
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
    public partial class ImportContactPersons : System.Web.UI.Page
    {
        public Result ContactPersonsFolders;
        public Result ContactPersons;
        DataTable ProjectsIds;

        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        public Database masterDb;


        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref ContactPersonsFolders, "ContactPersonsFolders");
            Helper.ParseMappingFile(ref ContactPersons, "ContactPersons", true);
        }
        protected void ImportData(object sender, EventArgs e)
        {
            ProjectsIds = Helper.GetIdsMatchDataTable("Project");
            try
            {
                masterDb = Helper.GetDatabase();

                ImportDataTable(Helper.GetDataTable(ContactPersonsFolders), ContactPersonsFolders);
                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(ContactPersons), ContactPersons);

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }
        protected void ImportDataTable(DataTable data, Result ContactPersonsFolders)
        {
            using (new SecurityDisabler())
            {

                TemplateItem template = masterDb.GetItem(ContactPersonsFolders.TemplateName);
                Item parent;
                Item folder;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        if (row["Path"].ToString().ToLower() == (ContactPersonsFolders.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(ContactPersonsFolders.StartPath.Trim());
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(ContactPersonsFolders.ExportPath.ToLower(), ContactPersonsFolders.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')));
                        }

                        folder = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                        if (folder == null)
                        {
                            Item newContactPersons = parent.Add(row["Name"].ToString(), template);
                            newContactPersons.Editing.BeginEdit();
                            newContactPersons["Old Id"] = row["ID"].ToString();
                            if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                            {
                           newContactPersons["__Masters"] = row["Insert options"].ToString().Replace("{08527C66-B6AE-4B31-B9C0-A63439566686}", "{62ACDD72-FF4C-485C-85B7-DA6EB028E63A}");
                            }
                            newContactPersons.Editing.EndEdit();
                            ContactPersonsFolders.InsertedNewRecords = ContactPersonsFolders.InsertedNewRecords + 1;
                        }
                        else
                        {
                            folder.Editing.BeginEdit();
                            folder["Old Id"] = row["ID"].ToString();
                            if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                            {
                                folder["__Masters"] = row["Insert options"].ToString().Replace("{08527C66-B6AE-4B31-B9C0-A63439566686}", "{62ACDD72-FF4C-485C-85B7-DA6EB028E63A}");
                            }
                            folder.Editing.EndEdit();
                            ContactPersonsFolders.UpdatedRecords = ContactPersonsFolders.UpdatedRecords + 1;
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
        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result ContactPersons)
        {
            UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;
            TemplateItem template = masterDb.GetItem(ContactPersons.TemplateName);
            DataTable Ids = Helper.GetIdsMatchDataTable("ContactPersons");
            foreach (var lang in Helper.GetDatabase().Languages)
            {
                DataTable data = dataTables[lang.ToString()];

                UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;

                Item parent;
                Item ContactPerson;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        int versionNumber = 0;
                        int.TryParse(row["Version"].ToString(), out versionNumber);

                        if (row["Path"].ToString().ToLower() == (ContactPersons.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(ContactPersons.StartPath.Trim(), lang);
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(ContactPersons.ExportPath.ToLower(), ContactPersons.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                        }

                        ContactPerson = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                        if (ContactPerson == null)
                        {
                            Item newContactPersons = parent.Add(row["Name"].ToString(), template);

                            this.UpdateItem(ref newContactPersons, row, ref Ids);
                            InsertedNewRecords = InsertedNewRecords + 1;
                        }
                        else
                        {
                            if (ContactPerson.Versions.Count > 0)
                            {
                                if (versionNumber <= ContactPerson.Versions.Count)
                                {
                                    Item[] versions = ContactPerson.Versions.GetVersions();

                                    this.UpdateItem(ref  versions[versionNumber - 1], row, ref Ids);
                                    UpdatedRecords = UpdatedRecords + 1;
                                }
                                else
                                {
                                    Item newVersion = ContactPerson.Versions.AddVersion();
                                    this.UpdateItem(ref newVersion, row, ref Ids);
                                    InsertedNewRecords = InsertedNewRecords + 1;

                                }

                            }
                            else
                            {
                                Item newVersion = ContactPerson.Versions.AddVersion();
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

                ContactPersons.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                ContactPersons.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

            Helper.FromDataTableToExcel(Ids, "ContactPersons");

        }     
        private void UpdateItem(ref Item item, DataRow row, ref DataTable Ids)
        {

            item.Editing.BeginEdit();
            item["Old Id"] = row["ID"].ToString();

            for (var i = 0; i < ContactPersons.ImportedFields.Count; i++)
            {

                try
                {
                    string importField = ContactPersons.ImportedFields[i].ToString();
                    if (!string.IsNullOrEmpty(importField))
                    {
                        string exportField = ContactPersons.ExportedFields[i].ToString();
                        //clear the data 
                        if (exportField.ToLower() == "recentproject1")
                        {
                            item[importField.Trim()] = "";
                        }
 
                        if (importField.ToLower() == "recentprojects")
                        {
                            if (!string.IsNullOrEmpty(row[exportField].ToString()))
                            {
                                if (string.IsNullOrEmpty(item[importField.Trim()]))
                                {
                                    item[importField.Trim()] = Helper.MapItemId(ProjectsIds,row[exportField].ToString());
                                }
                                else
                                {
                                    item[importField.Trim()] = item[importField.Trim()] + "|" + Helper.MapItemId(ProjectsIds, row[exportField].ToString());
                                }

                            }
                          
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