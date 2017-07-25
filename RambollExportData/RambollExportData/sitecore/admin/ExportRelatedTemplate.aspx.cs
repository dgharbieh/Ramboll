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
        public Dictionary<string, Data2> ReferrersDetailsTemplateField = new Dictionary<string, Data2>();
        List<Data2> DataCollection = new List<Data2>();
        StringBuilder ReferrersCSV = new StringBuilder();
        StringBuilder DetailsReferrersCSV = new StringBuilder();
        public string Current = "";

        public List<Result> ALLWebsite = new List<Result>();

        protected void Page_Load(object sender, EventArgs e)
        {



           // Helper.ParseMappingFile(ref Countries, "Countries");
           // Countries.ExcludedRelatedTemplates.Add("ComProject");

           //Helper.ParseMappingFile(ref Projects, "Projects", true);
           //Helper.ParseMappingFile(ref ContactPersons, "ContactPersons", true);
       //   Helper.ParseMappingFile(ref News, "News", true);
           //Helper.ParseMappingFile(ref Aliases, "Aliases");

           // ALLresultItem.Add(Countries);
           // ALLresultItem.Add(Projects);
         // ALLresultItem.Add(ContactPersons);
          //  ALLresultItem.Add(News);
           // ALLresultItem.Add(Aliases);

            foreach (string temp in Templates)
            {
                Result result = new Result();

                Helper.ParseMappingFile(ref result, temp, true);

                ALLWebsite.Add(result);
            }
        }
        protected void ExportDataref(object sender, EventArgs e)
        {
            try
            {
             
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();


                    foreach (var result in ALLresultItem)
                    {
                        ReferrersCSV.AppendLine("ItemTemplateName, RelatedTemplateName,Fields,Sample");
                        DetailsReferrersCSV.AppendLine("ItemId,ItemTemplateName,RelatedId,RelatedTemplateName,RelatedTemplateId,Fields");
                        Item parent = masterDb.GetItem(result.StartPath.Trim());
                        GetReferrersData(result, parent);


                         foreach (var item in DataCollection.Select(item =>item.ItemId + "," + item.ItemTemplateName + "," + item.RelatedId + "," + item.RelatedTemplateName + "," + item.RelatedTemplateId + "," +  item.Fields).Distinct())          
                        {
                            DetailsReferrersCSV.AppendLine(item);
                        }


                         foreach (var item in DataCollection.Select(x => new
                         {
                             x.ItemTemplateName,
                             x.RelatedTemplateName,
                             Fields = String.Join("|", DataCollection.Where(z => z.ItemTemplateName == x.ItemTemplateName && z.RelatedTemplateName == x.RelatedTemplateName && z.Fields != "").Select(m => m.Fields).Distinct()),
                             Sample = String.Join("|", DataCollection.Where(z => z.ItemTemplateName == x.ItemTemplateName && z.RelatedTemplateName == x.RelatedTemplateName && z.Fields != "").Select(m => m.RelatedId).Distinct())
                         }).Distinct())
                         {
                             ReferrersCSV.AppendLine( item.ItemTemplateName +"," + item.RelatedTemplateName + "," + item.Fields + ","+ item.Sample);
                         }




                        Helper.CreateFile(ReferrersCSV.ToString(), "Related"+result.fileName );
                        Helper.CreateFile(DetailsReferrersCSV.ToString(), "DetailsRelated" + result.fileName);
                        ReferrersTemplateField.Clear();
                        ReferrersCSV.Clear();
                        DetailsReferrersCSV.Clear();
                    }


                   /////////////////////
                    if (ALLWebsite.Count>0)
                    {

                        ReferrersCSV.AppendLine("ItemTemplateName, RelatedTemplateName,Fields,Sample");
                        DetailsReferrersCSV.AppendLine("ItemId,ItemTemplateName,RelatedId,RelatedTemplateName,RelatedTemplateId,Fields");
                        Item parent = masterDb.GetItem(ALLWebsite[0].StartPath.Trim());

                        GetReferrersData(ALLWebsite, parent);

                        foreach (var item in DataCollection.Select(item => item.ItemId + "," + item.ItemTemplateName + "," + item.RelatedId + "," + item.RelatedTemplateName + "," + item.RelatedTemplateId + "," + item.Fields).Distinct())
                        {
                            DetailsReferrersCSV.AppendLine(item);
                        }


                        foreach (var item in DataCollection.Select(x => new
                        {
                            x.ItemTemplateName,
                            x.RelatedTemplateName,
                            Fields = String.Join("|", DataCollection.Where(z => z.ItemTemplateName == x.ItemTemplateName && z.RelatedTemplateName == x.RelatedTemplateName && z.Fields != "").Select(m => m.Fields).Distinct()),
                            Sample = String.Join("|", DataCollection.Where(z => z.ItemTemplateName == x.ItemTemplateName && z.RelatedTemplateName == x.RelatedTemplateName && z.Fields != "").Select(m => m.RelatedId).Distinct())
                        }).Distinct())
                        {
                            ReferrersCSV.AppendLine(item.ItemTemplateName + "," + item.RelatedTemplateName + "," + item.Fields + "," + item.Sample);
                        }




                        Helper.CreateFile(ReferrersCSV.ToString(), "Related" + ALLWebsite[0].fileName);
                        Helper.CreateFile(DetailsReferrersCSV.ToString(), "DetailsRelated" + ALLWebsite[0].fileName);
                        ReferrersTemplateField.Clear();
                        ReferrersCSV.Clear();
                        DetailsReferrersCSV.Clear();

                    }
                 



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

                if (sub.TemplateName.ToLower() == resultItem.TemplateName.Trim().ToLower())
                {

                    GetReferrers( resultItem,sub);

                }

                GetReferrersData(resultItem, sub);
            }
        }



        private void GetReferrersData(List<Result> resultItems, Item parent)
        {

            if (parent.Children.Count == 0)
                return;

            foreach (Item sub in parent.Children.AsEnumerable())
            {
                var result = resultItems.Where(x => x.TemplateName.Trim().ToLower() == sub.TemplateName.ToLower()).FirstOrDefault();
                if (result != null)
                {
                    GetReferrers(result, sub);
                }

                GetReferrersData(resultItems, sub);
            }
        }

        public void GetReferrers(Result resultItem,Item item)
        {
            Item[] referrers = GetReferrersAsItems(ref item);

            if (referrers != null)
            {

                foreach (Item referr in referrers)
                {

                    if (!resultItem.ExcludedRelatedTemplates.Contains(referr.TemplateName))
                    {

                        var Fields = referr.Fields.Where(x => x.Value.Contains(item.ID.ToString()));
                        if (Fields != null && Fields.Count() > 0)
                        {
                            Data2 data = new Data2();
                            data.ItemId = item.ID.ToString(); ;
                            data.ItemTemplateName = item.TemplateName;
                            data.RelatedId = referr.ID.ToString();
                            data.RelatedTemplateName = referr.TemplateName;
                            data.RelatedTemplateId = referr.TemplateID.ToString();
                            data.Fields = string.Join("|", Fields.Where(x => x.Value.Contains(item.ID.ToString()))
                                        .Select(p => p.Name.ToString()));

                            DataCollection.Add(data);

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