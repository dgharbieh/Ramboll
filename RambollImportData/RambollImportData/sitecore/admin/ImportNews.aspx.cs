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
    public partial class ImportNews : System.Web.UI.Page
    {
        public Result NewsFolders;
        public Result News;

        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;

        public Database masterDb;
        public TemplateItem Folderstemplate;
        public TemplateItem PictureAndTextTemplate;
        public TemplateItem PublicationLinksOrNewsTemplate;
        public TemplateItem PublicationHeaderTemplate;
        DataTable ContactPersonsIds;
        DataTable ProjectsIds;

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref NewsFolders, "NewsFolders");
            Helper.ParseMappingFile(ref News, "News", true);
        }
        protected void ImportData(object sender, EventArgs e)
        {


            try
            {
                  masterDb = Helper.GetDatabase();
                  Folderstemplate = Helper.GetDatabase().GetItem("/sitecore/templates/Common/Folder");
                  PictureAndTextTemplate = masterDb.GetItem("{24CF86AE-37CE-4A72-A18B-DD30FF9515BD}");
                  PublicationLinksOrNewsTemplate = masterDb.GetItem("{BDC80C68-8123-4079-B007-C211B2FFA43D}");
                  PublicationHeaderTemplate = masterDb.GetItem("{CFCD9E3B-7E77-4994-9517-FDE19965286F}");
                 ContactPersonsIds = Helper.GetIdsMatchDataTable("ContactPersons");
                 ProjectsIds = Helper.GetIdsMatchDataTable("Project");



                ImportDataTable(Helper.GetDataTable(NewsFolders), NewsFolders);
                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(News), News);

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }
        protected void ImportDataTable(DataTable data, Result NewsFolders)
        {
            using (new SecurityDisabler())
            {
                TemplateItem template = masterDb.GetItem(NewsFolders.TemplateName);
                Item parent;
                Item folder;
                foreach (DataRow row in data.Rows)
                {
                    try
                    {
                        if (row["Path"].ToString().ToLower() == (NewsFolders.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(NewsFolders.StartPath.Trim());
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(NewsFolders.ExportPath.ToLower(), NewsFolders.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')));
                        }

                        folder = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                        if (folder == null)
                        {
                            Item newNews = parent.Add(row["Name"].ToString(), template);
                            newNews.Editing.BeginEdit();
                            newNews["Old Id"] = row["ID"].ToString();
                            if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                            {
                                newNews["__Masters"] = row["Insert options"].ToString().Replace("{C1033422-59DC-40C4-90A8-555CCDD619BB}", "{2419C8C4-3C47-4106-A8B1-6363DC94CC21}");
                            }
                            newNews.Editing.EndEdit();
                            NewsFolders.InsertedNewRecords = NewsFolders.InsertedNewRecords + 1;
                        }
                        else
                        {
                            folder.Editing.BeginEdit();
                            folder["Old Id"] = row["ID"].ToString();
                            if (!string.IsNullOrEmpty(row["Insert options"].ToString()))
                            {
                                folder["__Masters"] = row["Insert options"].ToString().Replace("{C1033422-59DC-40C4-90A8-555CCDD619BB}", "{2419C8C4-3C47-4106-A8B1-6363DC94CC21}");
                            }
                            folder.Editing.EndEdit();
                            NewsFolders.UpdatedRecords = NewsFolders.UpdatedRecords + 1;
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
        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result News)
        {
            UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;
            TemplateItem template = masterDb.GetItem(News.TemplateName);
            DataTable Ids = Helper.GetIdsMatchDataTable("News");

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

                        if (row["Path"].ToString().ToLower() == (News.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(News.StartPath.Trim(), lang);
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(News.ExportPath.ToLower(), News.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                        }

                        project = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();
                        if (project == null)
                        {
                            Item newNews = parent.Add(row["Name"].ToString(), template);

                            this.UpdateItem(ref newNews, row, ref Ids);
                            InsertedNewRecords = InsertedNewRecords + 1;
                        }
                        else
                        {
                            if (project.Versions.Count > 0)
                            {
                                if (versionNumber <= project.Versions.Count)
                                {
                                    Item[] versions = project.Versions.GetVersions();

                                    this.UpdateItem(ref  versions[versionNumber - 1], row, ref Ids);
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

                News.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                News.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

            Helper.FromDataTableToExcel(Ids, "News");
        }

       
        private void UpdateItem(ref Item item, DataRow row, ref DataTable Ids)
        {

            item.Editing.BeginEdit();
            item["Old Id"] = row["ID"].ToString();

            for (var i = 0; i < News.ImportedFields.Count; i++)
            {

                try
                {
                    string importField = News.ImportedFields[i].ToString();
                    if (!string.IsNullOrEmpty(importField))
                    {
                        string exportField = News.ExportedFields[i].ToString();
                        //clear the data 
                        if (exportField.ToLower() == "project1")
                        {
                            item[importField.Trim()] = "";
                        }

                        if (importField.ToLower()=="projects")
                        {

                            if (!string.IsNullOrEmpty(row[exportField].ToString()))
                            {

                                if (string.IsNullOrEmpty(item[importField.Trim()]))
                                {
                                    item[importField.Trim()] = Helper.MapItemId(ProjectsIds, row[exportField].ToString());
                                }
                                else
                                {
                                    item[importField.Trim()] = item[importField.Trim()] + "|" + Helper.MapItemId(ProjectsIds, row[exportField].ToString());
                                }

                            }

                        }
                        else
                        {

                         if (importField.ToLower() == "contactpersons")
                            {
                                if (!string.IsNullOrEmpty(row[exportField].ToString().Trim()))
                                {
                                    string[] PersonsIds = row[exportField].ToString().Split('|');
                                    if (PersonsIds != null && PersonsIds.Count() > 0)
                                    {
                                        foreach (string id in PersonsIds)
                                        {
                                            row[exportField] = row[exportField].ToString().Replace(id, Helper.MapItemId(ContactPersonsIds, id));
                                        }
                                    }
                                }
                           
                                item[importField.Trim()] = row[exportField].ToString();

                            }
                            else
                            {
                                item[importField.Trim()] = row[exportField].ToString();
                            }
                              
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
            //Children Data
            UpdatePictureAndText(ref  item, row);
            UpdatePuplications(ref  item, row);
        }
        private void UpdatePictureAndText(ref Item item, DataRow row)
        {

            ///PictureAndText

            Item PictureAndTextFolder = item.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Pictures-and-Texts".ToLower()).FirstOrDefault();
            for (var i = 1; i <= 5; i++)
            {
                string texti = row["Text" + i].ToString();
                string PictureiSize225x133Pix = row["Picture" + i + "Size225x133Pix"].ToString();
                string PictureiSize722x318Pix = row["Picture" + i + "Size722x318Pix"].ToString();
                string ClickOnPicturei = row["ClickOnPicture" + i].ToString();
                string TViFullWidth = row["TV" + i + "FullWidth"].ToString();
                if (!string.IsNullOrEmpty((texti + PictureiSize225x133Pix + PictureiSize722x318Pix + ClickOnPicturei + TViFullWidth).Trim()))
                {
                    if (PictureAndTextFolder == null)
                    {
                        PictureAndTextFolder = item.Add("Pictures-and-Texts", Folderstemplate);
                        PictureAndTextFolder.Editing.BeginEdit();
                        PictureAndTextFolder["__Masters"] = PictureAndTextTemplate.ID.ToString();
                        PictureAndTextFolder.Editing.EndEdit();

                    }
                    Item picturesandtextItem = PictureAndTextFolder.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("Pictures-and-Text" + i).ToLower()).FirstOrDefault();


                    if (picturesandtextItem == null)
                    {

                        picturesandtextItem = PictureAndTextFolder.Add("Pictures-and-Text" + i, PictureAndTextTemplate);

                    }
                    picturesandtextItem.Editing.BeginEdit();
                    picturesandtextItem["Text"] = texti;
                    picturesandtextItem["PictureSize1280x720Pix-SideImage"] = PictureiSize225x133Pix;
                    picturesandtextItem["PictureSize1280x720Pix-BigImage"] = PictureiSize722x318Pix;
                    picturesandtextItem["ClickOnPicture"] = ClickOnPicturei;
                    picturesandtextItem["VideoFullWidth"] = TViFullWidth;
                    picturesandtextItem["Old Id"] = i.ToString();
                    picturesandtextItem.Editing.EndEdit();

                }

            }
        }
        private void UpdatePuplications(ref Item item, DataRow row)
        {

            Item PublicationsFolder = item.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Publications".ToLower()).FirstOrDefault();


            for (var i = 1; i < 22; i = i + 2)
            {
                int next = i + 1;

                string HeaderSubject = row["HeaderSubject" + i + "And" + next].ToString();

                string HeaderForSubjectOne = row["HeaderForSubject" + i].ToString();
                string TeaserForSubjectOne = row["TeaserForSubject" + i].ToString();
                string LinkToForSubjectOne = row["LinkToForSubject" + i].ToString();
                string Picture105x98PixForSubjectOne = row["Picture105x98PixForSubject" + i].ToString();
                string LinkToFromPicture105x98PixForSubjectOne = row["LinkToFromPicture105x98PixForSubject" + i].ToString();

                string Item1 = (HeaderForSubjectOne + TeaserForSubjectOne + LinkToForSubjectOne + Picture105x98PixForSubjectOne + LinkToFromPicture105x98PixForSubjectOne).Trim();



                string HeaderForSubjectTwo = row["HeaderForSubject" + next].ToString();
                string TeaserForSubjectTwo = row["TeaserForSubject" + next].ToString();
                string LinkToForSubjectTwo = row["LinkToForSubject" + next].ToString();
                string Picture105x98PixForSubjectTwo = row["Picture105x98PixForSubject" + next].ToString();
                string LinkToFromPicture105x98PixForSubjectTwo = row["LinkToFromPicture105x98PixForSubject" + next].ToString();

                string Item2 = (HeaderForSubjectTwo + TeaserForSubjectTwo + LinkToForSubjectTwo + Picture105x98PixForSubjectTwo + LinkToFromPicture105x98PixForSubjectTwo).Trim();


                if (!string.IsNullOrEmpty(HeaderSubject + Item1 + Item2))
                {

                    if (PublicationsFolder == null)
                    {
                        PublicationsFolder = item.Add("Publications", Folderstemplate);
                        PublicationsFolder.Editing.BeginEdit();
                        PublicationsFolder["__Masters"] = PublicationLinksOrNewsTemplate.ID.ToString();
                        PublicationsFolder.Editing.EndEdit();
                    }

                    Item HeaderSubjectItem = PublicationsFolder.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("HeaderSubject" + i + "And" + next).ToLower()).FirstOrDefault();
                    if (HeaderSubjectItem == null)
                    {
                        HeaderSubjectItem = PublicationsFolder.Add("HeaderSubject" + i + "And" + next, PublicationHeaderTemplate);

                        HeaderSubjectItem.Editing.BeginEdit();
                        HeaderSubjectItem["HeaderSubject"] = HeaderSubject;
                        HeaderSubjectItem["Old Id"] = +i + "And" + next;
                        HeaderSubjectItem.Editing.EndEdit();
                    }


                    if (!string.IsNullOrEmpty(Item1))
                    {
                        Item PublicationItemOne = HeaderSubjectItem.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("Publication-Links-Or-News" + i).ToLower()).FirstOrDefault();
                        if (PublicationItemOne == null)
                        {
                            PublicationItemOne = HeaderSubjectItem.Add("Publication-Links-Or-News" + i, PublicationLinksOrNewsTemplate);
                        }

                        PublicationItemOne.Editing.BeginEdit();
                        PublicationItemOne["HeaderForSubject"] = HeaderForSubjectOne;
                        PublicationItemOne["LinkToFromPicture105x98PixForSubject"] = LinkToFromPicture105x98PixForSubjectOne;
                        ParseTeaserForSubject(ref PublicationItemOne, TeaserForSubjectOne, LinkToFromPicture105x98PixForSubjectOne);
                        PublicationItemOne["LinkToForSubject"] = LinkToForSubjectOne;
                        PublicationItemOne["Picture105x98PixForSubject"] = Picture105x98PixForSubjectOne;           
                        PublicationItemOne["Old Id"] = i.ToString();
                        PublicationItemOne.Editing.EndEdit();

                    }



                    if (!string.IsNullOrEmpty(Item2))
                    {
                        Item PublicationItemTwo = HeaderSubjectItem.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("Publication-Links-Or-News" + next).ToLower()).FirstOrDefault();

                        if (PublicationItemTwo == null)
                        {
                            PublicationItemTwo = HeaderSubjectItem.Add("Publication-Links-Or-News" + next, PublicationLinksOrNewsTemplate);
                        }
                        PublicationItemTwo.Editing.BeginEdit();
                        PublicationItemTwo["HeaderForSubject"] = HeaderForSubjectTwo;
                        PublicationItemTwo["LinkToFromPicture105x98PixForSubject"] = LinkToFromPicture105x98PixForSubjectTwo;
                        ParseTeaserForSubject(ref PublicationItemTwo, TeaserForSubjectTwo, LinkToFromPicture105x98PixForSubjectTwo);
                        PublicationItemTwo["LinkToForSubject"] = LinkToForSubjectTwo;
                        PublicationItemTwo["Picture105x98PixForSubject"] = Picture105x98PixForSubjectTwo;
                        PublicationItemTwo["Old Id"] = next.ToString();
                        PublicationItemTwo.Editing.EndEdit();
                    }


                }
            }
        }

        private void ParseTeaserForSubject(ref Item PublicationItemOne, string TeaserForSubject, string LinkToFromPicture105x98PixForSubject)
        {
            if (!string.IsNullOrEmpty(TeaserForSubject.Trim()))
            {
                PublicationItemOne["TeaserForSubject"] = TeaserForSubject;
            }
            else
            {
                if (!string.IsNullOrEmpty(LinkToFromPicture105x98PixForSubject.Trim()))
                {
                    string Internallink = "<p><a href='~/link.aspx?_id=@id&amp;_z=z'>@text</a></p>";

                    string Externallink = "<p><a href='@href'>@text</a></p>";

                    string MediaLink = "<p><a href='-/media/@id.ashx'>cover</a></p>";


                    Sitecore.Data.Fields.LinkField lnkField = PublicationItemOne.Fields["LinkToFromPicture105x98PixForSubject"];
                    if (lnkField != null)
                    {
                        if (lnkField.LinkType.ToLower() == "external")
                        {
                            Externallink = Externallink.Replace("@href", lnkField.Url).Replace("@text", lnkField.Text);
                            PublicationItemOne["TeaserForSubject"] = Externallink;
                        }
                        else if (lnkField.LinkType == "internal")
                        {

                            Internallink = Internallink.Replace("@id", lnkField.TargetID.ToString().Replace("{","").Replace("}","").Replace("-","")).Replace("@text", lnkField.Text);
                            PublicationItemOne["TeaserForSubject"] = Internallink;
                        }
                        else if (lnkField.LinkType == "media")
                        {
                            MediaLink = MediaLink.Replace("@id", lnkField.TargetID.ToString().Replace("{", "").Replace("}", "").Replace("-", "")).Replace("@text", lnkField.Text);
                            PublicationItemOne["TeaserForSubject"] = MediaLink;
                        }
                    }

                }
            }
        }
    }
}