using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace RambollImportData.Helpers
{
    public class PublicationHelper
    {

        public static Database masterDb=Helper.GetDatabase();
        public static TemplateItem Folderstemplate = Helper.GetDatabase().GetItem("/sitecore/templates/Common/Folder");
        public static TemplateItem PictureAndTextTemplate = masterDb.GetItem("{24CF86AE-37CE-4A72-A18B-DD30FF9515BD}");
        public static TemplateItem PublicationLinksOrNewsTemplate = masterDb.GetItem("{BDC80C68-8123-4079-B007-C211B2FFA43D}");
        public static TemplateItem PublicationHeaderTemplate = masterDb.GetItem("{CFCD9E3B-7E77-4994-9517-FDE19965286F}");

        private static void UpdatePictureAndText(ref Item item, DataRow row)
        {
            using (new LanguageSwitcher(item.Language))
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
        }
        private static void UpdatePuplications(ref Item item, DataRow row)
        {
            using (new LanguageSwitcher(item.Language))
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