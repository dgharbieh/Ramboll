﻿using RambollImportData.Base;
using RambollImportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace RambollImportData.sitecore.admin
{
    public partial class ImportWebsites : System.Web.UI.Page
    {


        DataTable CountriesIds;
        public List<Result> FullWebsites = new List<Result>();
        public string[] Templates = { "Websites", "ServiceFocusPage" };
        public List<Result> FullWebsitesSubTree = new List<Result>();
        public string[] TemplatesSubtree = { "RichPageReference", "FeaturePageReference" };
        public string[] ServicesTemplates = { "ServiceFocusPage" };

        public int Counter = 0;
        public int UpdatedRecords = 0;
        public int InsertedVersionsRecords = 0;
        public int InsertedNewRecords = 0;
        public string ParentNotFound = "";
        public string MoveData = "";
        private string OldWebsiteFolder = "/sitecore/content/Home/Websites";
        private string NewWebsiteFolder = "/sitecore/content/Ramboll/Ramboll Countries";


        public string[] ServicesBaseTemplates = { "Com5PicturesAndTextBasic", "Com22PublicationsLinksOrNewsBasic", "Com2ProjectsBasic", "Com6ProjectsBasic" };
        public static Database masterDb = Helper.GetDatabase();
        public static TemplateItem Folderstemplate = Helper.GetDatabase().GetItem("/sitecore/templates/Common/Folder");
        public static TemplateItem PictureAndTextTemplate = masterDb.GetItem("{24CF86AE-37CE-4A72-A18B-DD30FF9515BD}");
        public static TemplateItem PublicationLinksOrNewsTemplate = masterDb.GetItem("{BDC80C68-8123-4079-B007-C211B2FFA43D}");
        public static TemplateItem PublicationHeaderTemplate = masterDb.GetItem("{CFCD9E3B-7E77-4994-9517-FDE19965286F}");

        public static Item OldWebsiteFolderItem = masterDb.GetItem("/sitecore/content/Home/Websites");


        protected void Page_Load(object sender, EventArgs e)
        {
            foreach (string temp in Templates)
            {
                Result result = new Result();

                Helper.ParseMappingFile(ref result, temp, true);

                FullWebsites.Add(result);
            }
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                CountriesIds = Helper.GetIdsMatchDataTable("Countries");
                // foreach (var result in FullWebsites)
                for (var i = 0; i < FullWebsites.Count; i++)
                {
                    ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(FullWebsites[i]), FullWebsites[i], Templates[i]);
                }
                if (!string.IsNullOrEmpty(ParentNotFound))
                {
                    pnParentNotFound.Visible = true;
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

        protected void Move_Click(object sender, EventArgs e)
        {
            try
            {
            ParentNotFound = string.Empty;
            Sitecore.Configuration.Settings.Indexing.Enabled = false;
            using (new Sitecore.Data.DatabaseCacheDisabler())
            {
                foreach (Item item in OldWebsiteFolderItem.Children)
                {
                    MoveAndUpdate(item);

                }

                pnMove.Visible = true;

                ImportSubtree();

                if (!string.IsNullOrEmpty(ParentNotFound))
                {
                    pnParentNotFound.Visible = true;
                }

            }
                pnSuccessSubtree.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccessSubtree.Visible = false;
                pnFailure.Visible = true;
            }
        }

     
        protected void ImportMultiLanguageDataTable(Dictionary<string, DataTable> dataTables, Result result, string name, bool withMove= false)
        {
            UpdatedRecords = InsertedVersionsRecords = InsertedNewRecords = 0;
            Database masterDb = Helper.GetDatabase();
            TemplateItem template = masterDb.GetItem(result.TemplateName);

            DataTable Ids = Helper.GetIdsMatchDataTable(name);

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

                        if (row["Path"].ToString().ToLower() == (result.ExportPath + "/" + row["Name"].ToString()).ToLower())
                        {
                            parent = masterDb.GetItem(result.StartPath.Trim(), lang);
                        }
                        else
                        {
                            string url = row["Path"].ToString().ToLower().Replace(result.ExportPath.ToLower(), result.StartPath.ToLower());
                            parent = masterDb.GetItem(url.Substring(0, url.LastIndexOf('/')), lang);
                        }
                        if (parent != null)
                        {

                            Item old = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower()).FirstOrDefault();

                            project = parent.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower() && x.TemplateName== template.Name).FirstOrDefault();


                            if (project == null)
                            {
                                Item newWebsites = parent.Add(row["Name"].ToString(), template);

                                this.UpdateItem(ref newWebsites, row, ref Ids, ref result);

                                if (withMove)
                                {
                                    if (old != null && old.TemplateName != newWebsites.TemplateName)
                                    {
                                        foreach (Item temp in old.Children)
                                        {
                                            temp.MoveTo(newWebsites);

                                        }
                                        old.Delete();
                                    }

                                }
                                InsertedNewRecords = InsertedNewRecords + 1;
                            }
                            else
                            {
                                if (project.Versions.Count > 0)
                                {
                                    if (versionNumber <= project.Versions.Count)
                                    {
                                        Item[] versions = project.Versions.GetVersions();

                                        this.UpdateItem(ref versions[versionNumber - 1], row, ref Ids, ref result);

                                        if (withMove)
                                        {
                                            if (old != null && old.TemplateName != versions[versionNumber - 1].TemplateName)
                                            {
                                                foreach (Item temp in old.Children)
                                                {
                                                    temp.MoveTo(versions[versionNumber - 1]);

                                                }
                                                old.Delete();
                                            }

                                        }


                                        UpdatedRecords = UpdatedRecords + 1;
                                    }
                                    else
                                    {
                                        Item newVersion = project.Versions.AddVersion();
                                        this.UpdateItem(ref newVersion, row, ref Ids, ref result);



                                        if (withMove)
                                        {
                                            if (old != null && old.TemplateName != newVersion.TemplateName)
                                            {
                                                foreach (Item temp in old.Children)
                                                {
                                                    temp.MoveTo(newVersion);

                                                }
                                                old.Delete();
                                            }
                                          
                                        }

                                        InsertedNewRecords = InsertedNewRecords + 1;

                                    }

                                }
                                else
                                {
                                    Item newVersion = project.Versions.AddVersion();
                                    this.UpdateItem(ref newVersion, row, ref Ids, ref result);


                                    if (withMove)
                                    {
                                        if (old != null && old.TemplateName != newVersion.TemplateName)
                                        {
                                            foreach (Item temp in old.Children)
                                            {
                                                temp.MoveTo(newVersion);

                                            }
                                            old.Delete();
                                        }
                                
                                    }
                                    InsertedNewRecords = InsertedNewRecords + 1;

                                }
                            }
                        }
                        else
                        {
                            ParentNotFound = ParentNotFound + row["Path"].ToString() + "</br>";
                        }

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }

                }

                result.InsertedNewTotals.Add(lang.Name, InsertedNewRecords.ToString());
                result.UpdateTotals.Add(lang.Name, UpdatedRecords.ToString());
            }

            Helper.FromDataTableToExcel(Ids, name);
        }

        private void UpdateItem(ref Item item, DataRow row, ref DataTable Ids, ref Result result)
        {

            item.Editing.BeginEdit();
            item["Old Id"] = row["ID"].ToString();

            for (var i = 0; i < result.ImportedFields.Count; i++)
            {

                try
                {
                    string importField = result.ImportedFields[i].ToString();
                    if (!string.IsNullOrEmpty(importField))
                    {
                        string exportField = result.ExportedFields[i].ToString();


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

        protected void MoveAndUpdate(Item OldItem)
        {

            //move the childs
            foreach (Item temp in OldItem.Children)
            {
                if (!Templates.Contains(temp.TemplateName))
                {
                    string parentPath = temp.Parent.Paths.Path.Replace(OldWebsiteFolder, NewWebsiteFolder);
                    Item parentItem = masterDb.GetItem(parentPath);
                    temp.MoveTo(parentItem);
                    UpdatedItemAndChild(temp);

                }
                else

                {
                    MoveAndUpdate(temp);
                }
            }


        }

        protected void ImportSubtree()
        {
            //if (TemplatesSubtree.Contains(item.TemplateName))
            //{
            //    item.Delete();

            //}


            foreach (string temp in TemplatesSubtree)
            {
                Result result = new Result();

                Helper.ParseMappingFile(ref result, temp, true);

                FullWebsitesSubTree.Add(result);
            }

            CountriesIds = Helper.GetIdsMatchDataTable("Countries");
            for (var i = 0; i < FullWebsitesSubTree.Count; i++)
            {
                ImportMultiLanguageDataTable(Helper.GetLanguagesDataTable(FullWebsitesSubTree[i]), FullWebsitesSubTree[i], TemplatesSubtree[i],true);
            }
            if (!string.IsNullOrEmpty(ParentNotFound))
            {
                pnParentNotFound.Visible = true;
            }

        }

        protected void UpdatedItemAndChild(Item item)
        {
            Sitecore.Configuration.Settings.Indexing.Enabled = false;
            using (new Sitecore.Data.DatabaseCacheDisabler())
            {
                try
                {

                    var templates = item.Template.BaseTemplates.Where(x => ServicesBaseTemplates.Contains(x.Name)).ToList();


                    if (templates != null && templates.Count > 0)
                    {

                        foreach (var temp in templates)
                        {
                            if (ServicesBaseTemplates.Contains(temp.Name))
                            {
                                Counter = Counter + 1;
                                switch (temp.Name)
                                {


                                    case "Com5PicturesAndTextBasic": { UpdatePictureAndText(ref item); break; }
                                    case "Com22PublicationsLinksOrNewsBasic": { UpdatePuplications(ref item); break; }
                                        // case "Com2ProjectsBasic": { UpdateProjectsBasic(ref item, 20, "Project");  break; }
                                        // case "Com6ProjectsBasic": { UpdateProjectsBasic(ref item, 6, "Project_"); break; }
                                }
                            }
                        }
                    }


                    foreach (Item child in item.Children)
                    {
                        UpdatedItemAndChild(child);
                    }
                }
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
        }


        private static void UpdateProjectsBasic(ref Item item, int number, string field)
        {
            try
            {
                item.Editing.BeginEdit();
                item.Fields["Projects"].Value = "";

                var projects = "";
                for (var i = 1; i <= number; i++)
                {

                    string project = item.Fields[field + i].Value = "";
                    if (!string.IsNullOrEmpty(project))
                    {
                        if (string.IsNullOrEmpty(projects))
                        {
                            projects = projects + project;
                        }
                        else
                        {
                            projects = projects + "|" + project;
                        }

                    }

                }
                item.Fields["Projects"].Value = projects;
                item.Editing.EndEdit();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static void UpdatePictureAndText(ref Item item)
        {
            using (new LanguageSwitcher(item.Language))
            {
                ///PictureAndText
                try
                {
                    Item PictureAndTextFolder = item.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Pictures-and-Texts".ToLower()).FirstOrDefault();
                    for (var i = 1; i <= 5; i++)
                    {
                        string texti = item["Text" + i].ToString();
                        string PictureiSize225x133Pix = item["Picture" + i + "Size225x133Pix"].ToString();
                        string PictureiSize722x318Pix = item["Picture" + i + "Size722x318Pix"].ToString();
                        string ClickOnPicturei = item["ClickOnPicture" + i].ToString();
                        string TViFullWidth = item["TV" + i + "FullWidth"].ToString();
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
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
        }

        private static void UpdatePuplications(ref Item item)
        {
            using (new LanguageSwitcher(item.Language))
            {
                try
                {
                    Item PublicationsFolder = item.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Publications".ToLower()).FirstOrDefault();


                    for (var i = 1; i < 22; i = i + 2)
                    {
                        int next = i + 1;

                        string HeaderSubject = item["HeaderSubject" + i + "And" + next].ToString();

                        string HeaderForSubjectOne = item["HeaderForSubject" + i].ToString();
                        string TeaserForSubjectOne = item["TeaserForSubject" + i].ToString();
                        string LinkToForSubjectOne = item["LinkToForSubject" + i].ToString();
                        string Picture105x98PixForSubjectOne = item["Picture105x98PixForSubject" + i].ToString();
                        string LinkToFromPicture105x98PixForSubjectOne = item["LinkToFromPicture105x98PixForSubject" + i].ToString();

                        string Item1 = (HeaderForSubjectOne + TeaserForSubjectOne + LinkToForSubjectOne + Picture105x98PixForSubjectOne + LinkToFromPicture105x98PixForSubjectOne).Trim();
                        string HeaderForSubjectTwo = item["HeaderForSubject" + next].ToString();
                        string TeaserForSubjectTwo = item["TeaserForSubject" + next].ToString();
                        string LinkToForSubjectTwo = item["LinkToForSubject" + next].ToString();
                        string Picture105x98PixForSubjectTwo = item["Picture105x98PixForSubject" + next].ToString();
                        string LinkToFromPicture105x98PixForSubjectTwo = item["LinkToFromPicture105x98PixForSubject" + next].ToString();

                        string Item2 = (HeaderForSubjectTwo + TeaserForSubjectTwo + LinkToForSubjectTwo + Picture105x98PixForSubjectTwo + LinkToFromPicture105x98PixForSubjectTwo).Trim();


                        if (!string.IsNullOrEmpty(HeaderSubject + Item1 + Item2))
                        {

                            if (PublicationsFolder == null)
                            {
                                PublicationsFolder = item.Add("Publications", Folderstemplate);
                                using (new EditContext(PublicationsFolder))
                                {

                                    // PublicationsFolder.Editing.BeginEdit();
                                    PublicationsFolder["__Masters"] = PublicationLinksOrNewsTemplate.ID.ToString();
                                    //   PublicationsFolder.Editing.EndEdit();
                                }
                            }

                            Item HeaderSubjectItem = PublicationsFolder.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("HeaderSubject" + i + "And" + next).ToLower()).FirstOrDefault();
                            if (HeaderSubjectItem == null)
                            {
                                HeaderSubjectItem = PublicationsFolder.Add("HeaderSubject" + i + "And" + next, PublicationHeaderTemplate);


                                using (new EditContext(HeaderSubjectItem))
                                {


                                    // HeaderSubjectItem.Editing.BeginEdit();
                                    HeaderSubjectItem["HeaderSubject"] = HeaderSubject;
                                    HeaderSubjectItem["Old Id"] = +i + "And" + next;
                                    //  HeaderSubjectItem.Editing.EndEdit();
                                }
                            }


                            if (!string.IsNullOrEmpty(Item1))
                            {
                                Item PublicationItemOne = HeaderSubjectItem.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == ("Publication-Links-Or-News" + i).ToLower()).FirstOrDefault();
                                if (PublicationItemOne == null)
                                {
                                    PublicationItemOne = HeaderSubjectItem.Add("Publication-Links-Or-News" + i, PublicationLinksOrNewsTemplate);
                                }


                                using (new EditContext(PublicationItemOne))
                                {
                                    // PublicationItemOne.Editing.BeginEdit();
                                    PublicationItemOne["HeaderForSubject"] = HeaderForSubjectOne;
                                    PublicationItemOne["LinkToFromPicture105x98PixForSubject"] = LinkToFromPicture105x98PixForSubjectOne;
                                    ParseTeaserForSubject(ref PublicationItemOne, TeaserForSubjectOne, LinkToFromPicture105x98PixForSubjectOne);
                                    PublicationItemOne["LinkToForSubject"] = LinkToForSubjectOne;
                                    PublicationItemOne["Picture105x98PixForSubject"] = Picture105x98PixForSubjectOne;
                                    PublicationItemOne["Old Id"] = i.ToString();
                                    //  PublicationItemOne.Editing.EndEdit();
                                }

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
                catch (Exception ex)
                {
                    throw (ex);
                }
            }
        }

        private static void ParseTeaserForSubject(ref Item PublicationItemOne, string TeaserForSubject, string LinkToFromPicture105x98PixForSubject)
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

                            Internallink = Internallink.Replace("@id", lnkField.TargetID.ToString().Replace("{", "").Replace("}", "").Replace("-", "")).Replace("@text", lnkField.Text);
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