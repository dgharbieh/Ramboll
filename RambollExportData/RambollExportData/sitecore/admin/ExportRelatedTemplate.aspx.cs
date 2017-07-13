using RambollExportData.Base;
using RambollExportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RambollExportData.sitecore.admin
{
    public partial class ExportRelatedTemplate : System.Web.UI.Page
    {
        //public Result ALLresultItem;

        public List<Result> ALLresultItem = new List<Result>();
        public Result Countries;
        public Result Projects;
        public Result ContactPersons;
        public Result News;
        public Result Aliases;
  
        public string[] Templates = { "Websites", "ServiceFocusPage", "RichPageReference", "FeaturePageReference" };

        public Dictionary<string, int> ReferrersTotals = new Dictionary<string, int>();
        public Dictionary<string, Data> ReferrersTemplateField = new Dictionary<string, Data>();
        StringBuilder ReferrersCSV = new StringBuilder();
        public string Current = "";


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
        protected void ExportDataref(object sender, EventArgs e)
        {
            try
            {
                ReferrersCSV.AppendLine("Mapping, RelatedTemplateName,RelatedTemplateId,Fields,Sample");
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();


                    foreach (var result in ALLresultItem)
                    {
                        Item parent = masterDb.GetItem(result.StartPath.Trim());
                        GetReferrersData(result, parent);
                    }
                    foreach (var item in ReferrersTemplateField)
                    {
                        ReferrersCSV.AppendLine(item.Value.Mapping + "," + item.Value.RelatedTemplateName + "," + item.Value.RelatedTemplateId + "," + string.Join("|", item.Value.Fields.ToArray()) + "," + string.Join("|", item.Value.Items.ToArray()));
                    }
                    Helper.CreateFile(ReferrersCSV.ToString(), "Referrers");

                }

                pnReferrersSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnReferrersSuccess.Visible = false;
                pnFailure.Visible = true;
            }

        }



        private void GetReferrersData(Result resultItem, Item parent)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (Item sub in parent.Children.AsEnumerable())
            {
                // Item sub = Helper.GetDatabase().GetItem(item.ID, lang);

                if (sub.TemplateName.ToLower() == resultItem.TemplateName.Trim().ToLower())
                {

                    GetReferrers(sub);

                }

                GetReferrersData(resultItem, sub);
            }
        }


        public void GetReferrers(Item item)
        {
            Item[] referrers = GetReferrersAsItems(ref item);

            if (referrers != null)
            {

                foreach (Item referr in referrers)
                {
                    var Fields = referr.Fields.Where(x => x.Value.Contains(item.ID.ToString()));

                    var FieldsNames = string.Join("|", Fields.Where(x => x.Value.Contains(item.ID.ToString()))
                                     .Select(p => p.Name.ToString()));

                    if (!string.IsNullOrEmpty(FieldsNames))
                    {
                        if (ReferrersTemplateField.ContainsKey(referr.TemplateName))
                        {
                            try
                            {
                                foreach (var f in Fields)
                                {
                                    if (!ReferrersTemplateField[referr.TemplateName].Fields.Contains(f.Name.ToString()))
                                    {
                                        ReferrersTemplateField[referr.TemplateName].Fields.Add(f.Name.ToString());
                                    }
                                    ReferrersTemplateField[referr.TemplateName].Items.Add(referr.ID.ToString());
                                }

                            }
                            catch (Exception ex)
                            {
                                throw ex;
                            }

                        }
                        else
                        {
                            Data data = new Data();
                            data.Mapping = item.TemplateName;
                            data.RelatedTemplateName = referr.TemplateName;
                            data.RelatedTemplateId = referr.TemplateID.ToString();
                            data.Items.Add(referr.ID.ToString());

                            foreach (var f in Fields)
                            {
                                data.Fields.Add(f.Name.ToString());
                            }

                            ReferrersTemplateField.Add(referr.TemplateName, data);
                            //  ReferrersCSV.AppendLine(item.TemplateName + "," + referr.TemplateName + "," + referr.TemplateID.ToString() + "," + FieldsNames + "," + referr.ID.ToString());
                        }
                    }


                }
            }
        }

        public static Item[] GetReferrersAsItems(ref Item item, bool includeStandardValues = false)
        {
            var links = Sitecore.Globals.LinkDatabase.GetReferrers(item);
            if (links == null)
                return new Item[0];
            var linkedItems = links.Select(i => i.GetSourceItem()).Where(i => i != null);

            linkedItems = linkedItems.Where(i => i.Paths.FullPath.StartsWith("/sitecore/content/Home", StringComparison.InvariantCultureIgnoreCase));

            if (!includeStandardValues)
                linkedItems = linkedItems.Where(i => !i.Name.Equals("__standard values", StringComparison.InvariantCultureIgnoreCase));
            return linkedItems.ToArray();
        }
    }




}