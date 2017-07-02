using RambollExportData;
using RambollExportData.Base;
using Sitecore.ApplicationCenter.Applications;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Sites;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RambollExportData.Helpers
{
    public class Helper
    {


        public static void ParseMappingFile(ref Result page, string fileName, bool vertical = false)
        {
            page = new Result();

            fileName = HttpContext.Current.Server.MapPath("/MappingFiles/") + fileName + ".csv";
            if (File.Exists(fileName))
            {
                foreach (var line in File.ReadAllLines(fileName))
                {
                    string[] Fields = line.Split(new char[] { ',' });
                    switch (Fields[0].ToLower().Trim())
                    {
                        case "start path":
                            page.StartPath = Fields[1];
                            break;
                        case "include language":
                            page.IncludeLanguage = (Fields[1].ToLower() == "true" ? true : false);
                            break;
                        case "include versions":
                            page.IncludeVersions = (Fields[1].ToLower() == "true" ? true : false);
                            break;
                        case "output name":
                            page.OutputName = Fields[1];
                            break;
                        case "template name":
                            page.TemplateName = Fields[1];
                            break;
                        case "exported fields":
                            if (!vertical)
                            {
                                for (var i = 1; i < Fields.Count(); i++)
                                {
                                    page.Fields.Add(Fields[i]);
                                }
                            }
                            break;
                        default:
                            if (vertical) { page.Fields.Add(Fields[0]); }
                            break;

                    }
                }
            }
        }

        public static void CreateFile(string data, string fileName)
        {

            try
            {

                fileName = Settings.GetSetting("FolderPath") + "\\" + fileName + ".csv";
                // Delete the file if it exists.
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                // Create the file.
                using (FileStream fs = File.Create(fileName))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(data);
                    // Add some information to the file.
                    fs.Write(info, 0, info.Length);
                }
            }
            catch (Exception ex)
            {
                Log.WriteToFile("{0} :Exception" + ex.Message);
                throw ex;
            }

        }

        public static string ReplaceComma(string data)
        {
            return data.Replace(",", "#;#").Replace("\n", "$;$").Replace("\r", "*;*");
        }

        public static Database GetDatabase()
        {
            return Sitecore.Configuration.Factory.GetDatabase("master");
        }

        public static LanguageCollection GetLanguages()
        {
            return LanguageManager.GetLanguages(GetDatabase());
        }
        public static string GetHeader(ArrayList fields)
        {
            string fieldsNames = string.Empty;

            for (var i = 0; i < fields.Count; i++)
            {
                fieldsNames = fieldsNames + fields[i];
                if (i < fields.Count - 1)
                {
                    fieldsNames = fieldsNames + ",";
                }
            }

            return fieldsNames;
        }

        public static void GenerateLanguagesFiles(Item parent, Result resultItem, string outputName, Dictionary<string, int> totals)
        {
 

            foreach (var lang in GetLanguages())
            {
                int total = 0;
                ArrayList fields = resultItem.Fields;
                StringBuilder CSV = new StringBuilder();
              
                CSV.AppendLine(Helper.GetHeader(fields));
               
                foreach (var item in parent.Children.AsEnumerable())
                {
                    Item sub = GetDatabase().GetItem(item.ID, lang);

                    string line = Helper.GetFieldsLineWithVersion(sub, ref resultItem, string.Empty);

                    if (!string.IsNullOrEmpty(line))
                    {
                        CSV.AppendLine(line);
                        total = total + item.Versions.Count;
                    } 
                }
                Helper.CreateFile(CSV.ToString(), outputName + "_" + lang.Name);          
                totals.Add(lang.Name.ToString(), total);

            }
        }

   
        public static string GetFieldsLineWithVersion(Item item, ref Result resultItem, string lang)
        {
            string fieldsValues = string.Empty;

            var count = 0;
            Item[] versions = item.Versions.GetVersions();
            ArrayList fields = resultItem.Fields;

            if (item.Versions.Count > 0)
            {
                foreach (Item version in versions)
                {
                    count = count + 1;
                    for (var i = 0; i < fields.Count; i++)
                    {
                        try
                        {

                            switch (fields[i].ToString().Trim().ToLower())
                            {
                                case "id":
                                    fieldsValues = fieldsValues + version.ID.ToString();
                                    break;
                                case "name":
                                    fieldsValues = fieldsValues + version.Name;
                                    break;
                                case "name*":// Field Name
                                    fieldsValues = fieldsValues + ReplaceComma(version.Fields[fields[i].ToString().Trim().Replace("*", "")].Value);
                                    break;
                                case "path":
                                    fieldsValues = fieldsValues + version.Paths.FullPath;
                                    break;
                                case "version":
                                    fieldsValues = fieldsValues + count.ToString();
                                    break;
                                case "parentid":
                                    fieldsValues = fieldsValues + version.ParentID.ToString();
                                    break;
                                case "templatename":
                                    fieldsValues = fieldsValues + version.TemplateName.ToString();
                                    break;
                                case "linktoitem":
                                    LinkField link = version.Fields["LinkTo"];
                                    if (link != null && link.IsInternal && link.TargetItem != null)
                                    {
                                        fieldsValues = fieldsValues + link.TargetItem.ID;
                                    }
                                    break;

                                case "linktotemplate":
                                    LinkField link2 = version.Fields["LinkTo"];
                                    if (link2 != null && link2.IsInternal && link2.TargetItem != null)
                                    {
                                        fieldsValues = fieldsValues + link2.TargetItem.TemplateName;
                                    }
                                    break;
                                default:
                                    fieldsValues = fieldsValues + ReplaceComma(version.Fields[fields[i].ToString().Trim()].Value);
                                    break;
                            }

                            if (i < fields.Count - 1)
                            {
                                fieldsValues = fieldsValues + ",";
                            }

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }


                    }

                    if (!string.IsNullOrEmpty(lang))
                    {
                        resultItem.Totals[lang.ToString()] = resultItem.Totals[lang.ToString()] + 1;
                    }

                    if (count < item.Versions.Count)
                    { fieldsValues = fieldsValues + "\n"; }


                }
            }
            return fieldsValues;
        }


        public static string GetFieldsLine(Item item, ArrayList fields)
        {
            string fieldsValues = string.Empty;

            for (var i = 0; i < fields.Count; i++)
            {

                switch (fields[i].ToString().Trim().ToLower())
                {
                    case "id":
                        fieldsValues = fieldsValues + item.ID.ToString();
                        break;
                    case "name":
                        fieldsValues = fieldsValues + item.Name;
                        break;
                    case "path":
                        fieldsValues = fieldsValues + item.Paths.FullPath;
                        break;
                    case "insert options":
                        fieldsValues = fieldsValues + ReplaceComma(item.Fields["__Masters"].Value);
                        break;

                    default:
                        fieldsValues = fieldsValues + ReplaceComma(item.Fields[fields[i].ToString()].Value);
                        break;
                }

                if (i < fields.Count - 1)
                {
                    fieldsValues = fieldsValues + ",";
                }
            }

            return fieldsValues;
        }


        public static void GetDataRowFields(DataTable data, Item item, ArrayList fields)
        {
            DataRow row = data.NewRow();

            for (var i = 0; i < fields.Count; i++)
            {

                switch (fields[i].ToString().Trim().ToLower())
                {
                    case "id":
                        row[fields[i].ToString()] = item.ID.ToString();
                        break;
                    case "name":
                        row[fields[i].ToString()] = item.Name;
                        break;
                    case "path":
                        row[fields[i].ToString()] = item.Paths.FullPath;
                        break;
                    default:
                        row[fields[i].ToString()] = ReplaceComma(item.Fields[fields[i].ToString()].Value);
                        break;
                }
            }
            data.Rows.Add(row);
        }

        public static void SetDataTableColums(DataTable data, ArrayList fields)
        {
            foreach (var field in fields)
            {
                data.Columns.Add(field.ToString());
            }
        }


        public static void GenerateReferrersFiles(Item parent, Result resultItem, string outputName)
        {
            resultItem.ReferrersCSV.AppendLine("TemplateName,TemplateId,Fields");

            foreach (var lang in GetLanguages())
            {

                foreach (var item in parent.Children.AsEnumerable())
                {
                    Item sub = GetDatabase().GetItem(item.ID, lang);


                    Helper.GetReferrers(sub, ref resultItem);

                }

                Helper.CreateFile(resultItem.ReferrersCSV.ToString(), outputName + "Referrers");
            }
        }
     

        public static void GetReferrers(Item item, ref Result resultItem)
        {
            Item[] referrers = GetReferrersAsItems(ref item);

            if (referrers != null)
            {

                foreach (Item referr in referrers)
                {

                    var FieldsNames = string.Join("|", referr.Fields.Where(x => x.Value.Contains(item.ID.ToString()))
                                     .Select(p => p.Name.ToString()));

                    if (!resultItem.ReferrersTemplateField.ContainsKey(referr.TemplateName))
                    {


                        resultItem.ReferrersTemplateField.Add(referr.TemplateName, FieldsNames);

                        resultItem.ReferrersCSV.AppendLine(referr.TemplateName + "," + referr.TemplateID.ToString() + "," + FieldsNames);


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