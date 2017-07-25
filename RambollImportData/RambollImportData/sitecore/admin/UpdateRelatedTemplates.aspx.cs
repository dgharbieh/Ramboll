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
    public partial class UpdateRelatedTemplates : System.Web.UI.Page
    {
        public List<Result> ALLresultItem = new List<Result>();
        public Result Countries;
        public Result Projects;
        public Result ContactPersons;
        public Result News;
        public Result Aliases;
        public Dictionary<string, DataTable> Ids = new Dictionary<string, DataTable>();

        public Dictionary<string, Result> Results = new Dictionary<string, Result>();


        public string[] Templates = { "Websites", "ServiceFocusPage", "RichPageReference", "FeaturePageReference" };


        public DataTable DetailsRelatedCountries;

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref Countries, "Countries");

            Helper.ParseMappingFile(ref Projects, "Projects", true);
            Helper.ParseMappingFile(ref ContactPersons, "ContactPersons", true);
            Helper.ParseMappingFile(ref News, "News", true);
            Helper.ParseMappingFile(ref Aliases, "Aliases");


             ALLresultItem.Add(Countries);
             ALLresultItem.Add(Projects);
             ALLresultItem.Add(ContactPersons);
             ALLresultItem.Add(News);
             ALLresultItem.Add(Aliases);

             foreach (string temp in Templates)
             {
                 Result result = new Result();

                 Helper.ParseMappingFile(ref result, temp, true);

                 ALLresultItem.Add(result);
             }
        }

        protected void UpdateData(object sender, EventArgs e)
        {

            try
            {

                Database masterDb = Helper.GetDatabase();

                foreach (var result in ALLresultItem)
                {
                    Ids.Add(result.OldTemplateName, Helper.GetIdsMatchDataTable(result.FileName));
                    Results.Add(result.OldTemplateName, result);
                }
                foreach (var result in ALLresultItem)
                {



                    DataTable DetailsRelated = Helper.GetDataTableFromCSV("DetailsRelated" + result.FileName);
                    foreach (DataRow row in DetailsRelated.Rows)
                    {

                        try
                        {
                            string RelatedTemplateName = row["RelatedTemplateName"].ToString();
                            string relatedId = row["RelatedId"].ToString();

                            if (Ids.Keys.Contains(RelatedTemplateName))
                            {
                                try
                                {
                                    relatedId = Ids[RelatedTemplateName].AsEnumerable()
                                                       .Where(r => (r.Field<string>("OldID")) == relatedId).Select(x => x.Field<string>("NewID")).First();
                                }
                                catch (Exception ex)
                                {
                                    //Check with Tamara
                                }
                            }

                            string Fields = row["Fields"].ToString();
                            Item RelatedItem = masterDb.GetItem(relatedId);
                            if (RelatedItem != null)
                            {
                                foreach (var itemLanguage in RelatedItem.Languages)
                                {
                                    bool CheckChilds = false;

                                    using (new LanguageSwitcher(itemLanguage))
                                    {
                                        var item = RelatedItem.Database.GetItem(RelatedItem.ID, itemLanguage);
                                        if (item.Versions.Count > 0)
                                        {
                                            Item[] versions = item.Versions.GetVersions();
                                            foreach (Item version in versions)
                                            {
                                                version.Editing.BeginEdit();

                                                foreach (string Field in Fields.Split('|').ToList())
                                                {
                                                    var fieldValue = Field;

                                                    if (Results.Keys.Contains(RelatedTemplateName))
                                                    {

                                                        //For Aliases
                                                        if (RelatedTemplateName == "ComAlias" && Field == "LinkTo")
                                                        {
                                                            fieldValue = "Linked item";
                                                        }
                                                        else
                                                        {
                                                            int index = Results[RelatedTemplateName].ExportedFields.IndexOf(fieldValue);
                                                            if (index != -1)
                                                            { fieldValue = Results[RelatedTemplateName].ImportedFields[index].ToString(); }

                                                        }
                                                    }

                                                    if (version.Fields[fieldValue] != null)
                                                    {
                                                        string oldValue = version.Fields[fieldValue].Value;
                                                        List<DataRow> dr = Ids[result.OldTemplateName].AsEnumerable()
                                                        .Where(r => oldValue.Contains(r.Field<string>("OldID"))).ToList();
                                                        foreach (DataRow pep in dr)
                                                        {
                                                            oldValue = oldValue.Replace(pep["OldID"].ToString(), pep["NewID"].ToString());
                                                        }
                                                        version.Fields[fieldValue].Value = oldValue;

                                                    }
                                                    else
                                                    {
                                                        CheckChilds = true;
                                                    }

                                                }
                                                version.Editing.EndEdit();


                                                if (version.HasChildren && CheckChilds)
                                                {

                                                    Item PictureAndTextFolder = version.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Pictures-and-Texts".ToLower()).FirstOrDefault();
                                                    if (PictureAndTextFolder != null)
                                                    {
                                                        UpdatedItemAndChild(PictureAndTextFolder, Fields, result);
                                                    }

                                                    Item PublicationsFolder = item.Children.AsEnumerable().ToList().Where(x => x.Name.ToLower() == "Publications".ToLower()).FirstOrDefault();
                                                    if (PublicationsFolder != null)
                                                    {
                                                        UpdatedItemAndChild(PublicationsFolder, Fields, result);
                                                    }
                                                }


                                                result.LanguageVersionRelatedRecords += 1;
                                            }
                                        }
                                    }
                                }
                                result.UpdatedRelatedRecords += 1;


                            }
                            else
                            {
                                result.NotFoundRelatedRecords += 1;
                            }

                        }
                        catch (Exception ex)
                        {
                            throw (ex);
                        }
                        //hh
                    }

                    pnSuccess.Visible = true;
                    pnFailure.Visible = false;


                }
            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }
        }



        protected void UpdatedItemAndChild(Item item, string Fields, Result result)
        {

            try
            {

                foreach (Item version in item.Children)
                {
                    version.Editing.BeginEdit();


                    foreach (var field in version.Fields.ToList())
                    {
                        string oldValue = field.Value;
                        List<DataRow> dr = Ids[result.OldTemplateName].AsEnumerable()
                        .Where(r => oldValue.Contains(r.Field<string>("OldID"))).ToList();
                        foreach (DataRow pep in dr)
                        {
                            version.Fields[field.Name].Value = oldValue.Replace(pep["OldID"].ToString(), pep["NewID"].ToString());
                        }

                    }
                    version.Editing.EndEdit();

                }



                foreach (Item child in item.Children)
                {
                    UpdatedItemAndChild(child, Fields, result);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }


}



