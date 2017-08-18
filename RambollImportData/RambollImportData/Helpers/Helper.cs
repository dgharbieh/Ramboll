using RambollImportData.Base;
using Sitecore.ApplicationCenter.Applications;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
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

namespace RambollImportData.Helpers
{
    public class Helper
    {
        public static void ParseMappingFile(ref Result page, string fileName, bool vertical = false)
        {
            page = new Result();
            page.FileName = fileName;
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
                        case "export path":
                            page.ExportPath = Fields[1];
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
                        case "old template name":
                            page.OldTemplateName = Fields[1];
                            break;
                        case "exported fields":
                            if (!vertical)
                            {
                                for (var i = 1; i < Fields.Count(); i++)
                                {
                                    page.ExportedFields.Add(Fields[i]);
                                }
                            }
                            break;
                       
                        case "imported fields":
                            if (!vertical)
                            {
                                for (var i = 1; i < Fields.Count(); i++)
                                {
                                    page.ImportedFields.Add(Fields[i]);
                                }
                            }
                            break;
                        default:
                            if (vertical) { page.ExportedFields.Add(Fields[0]); page.ImportedFields.Add(Fields[1]); }
                            break;
                    }
                }
            }
        }


        public static string ReplaceComma(string data)
        {
            return data.Replace("#;#", ", ").Replace( "$;$","\n").Replace( "*;*","\r");
        }
        public static Database GetDatabase()
        {
            return Sitecore.Configuration.Factory.GetDatabase("master");
        }

        public static Dictionary<string, DataTable> GetLanguagesDataTable(Result page)
        {
            Dictionary<string, DataTable> dataTables = new Dictionary<string, DataTable>();

             foreach( var lang in GetDatabase().Languages)
            {
                dataTables.Add(lang.Name, GetDataTable(page, lang));
            }
            return dataTables;

        }


        public static DataTable GetDataTable(Result page, Language lang = null)
        {
            DataTable dt = new DataTable();
             string   fileName = Settings.GetSetting("FolderPath") + "\\" + page.OutputName + (lang==null?string .Empty: "_"+lang.Name)+ ".csv";
            if (File.Exists(fileName))
            {
                bool isheader = true;
                foreach (var line in File.ReadAllLines(fileName))
                {

                 
                    string[] Fields = line.Split(new char[] { ',' });
                    try
                    {
                    if(isheader)
                    {
                        isheader = false;
                        foreach(var field in Fields)
                        {
                            dt.Columns.Add(field);
                        }

                    }else
                    {
                        DataRow row = dt.NewRow();
                        for(var i= 0 ; i < Fields.Count();i++)
                        {
                            row[i] = ReplaceComma(Fields[i]);             
                        }
                        dt.Rows.Add(row);
                    }

                  }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }
            return dt;
            }

        public static DataTable GetIdsMatchDataTable(string name)
        {
            DataTable dt = new DataTable();
            string fileName = Settings.GetSetting("FolderPath") + "\\" + name + "IdsMatch" + ".csv";
            if (File.Exists(fileName))
            {
                bool isheader = true;
                foreach (var line in File.ReadAllLines(fileName))
                {
                    string[] Fields = line.Split(new char[] { ',' });
                    try
                    {
                        if (isheader)
                        {
                            isheader = false;
                            foreach (var field in Fields)
                            {
                                dt.Columns.Add(field);
                            }

                        }
                        else
                        {
                            DataRow row = dt.NewRow();
                            for (var i = 0; i < Fields.Count(); i++)
                            {
                                row[i] = ReplaceComma(Fields[i]);
                            }
                            dt.Rows.Add(row);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }else
            {
                dt.Columns.Add("OldID");
                dt.Columns.Add("NewID");

            }

            return dt;
        }

        public static DataTable GetDataTableFromCSV(string name)
        {
            DataTable dt = new DataTable();
            string fileName = Settings.GetSetting("FolderPath") + "\\" + name + ".csv";
            if (File.Exists(fileName))
            {
                bool isheader = true;
                foreach (var line in File.ReadAllLines(fileName))
                {
                    string[] Fields = line.Split(new char[] { ',' });
                    try
                    {
                        if (isheader)
                        {
                            isheader = false;
                            foreach (var field in Fields)
                            {
                                dt.Columns.Add(field);
                            }

                        }
                        else
                        {
                            DataRow row = dt.NewRow();
                            for (var i = 0; i < Fields.Count(); i++)
                            {
                                row[i] = ReplaceComma(Fields[i]);
                            }
                            dt.Rows.Add(row);
                        }

                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
            }

            return dt;
        }
        public static void FromDataTableToExcel(DataTable dt,string name)
        {
            StringBuilder csv = new StringBuilder();
            string colums = "";
            for(int i=0; i <= dt.Columns.Count -1;i++)
            {
                colums = colums + dt.Columns[i].ColumnName;
                if(i < dt.Columns.Count - 1)
                {
                    colums = colums + ",";
                }
            }
            csv.AppendLine(colums);


         
            for (int i = 0; i <= dt.Rows.Count - 1; i++)
            {
                string row = "";
                for (int j = 0; j <= dt.Columns.Count - 1; j++)
                {
                    row = row + dt.Rows[i][j];
                    if (j < dt.Columns.Count - 1)
                    {
                        row = row + ",";
                    }
                }
                csv.AppendLine(row);
            }

            CreateFile(csv.ToString(), name + "IdsMatch");

        }

        public static string GetHeader(ArrayList fields)
        {
            string fieldsNames = string.Empty;

            for (var i=0; i<fields.Count;i++)
            {
                fieldsNames = fieldsNames + fields[i];
                if (i< fields.Count-1)
                {
                    fieldsNames = fieldsNames + ",";
                }
            }

            return fieldsNames;
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
                    default:
                         fieldsValues = fieldsValues + item.Fields[fields[i].ToString()].Value;
                          break; 
                }
      
                if (i < fields.Count - 1)
                {
                    fieldsValues = fieldsValues + ",";
                }
            }

            return fieldsValues;
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


        public static void UpdateIds(ref DataTable Ids, Item item)
        {

            string oldID = item["Old Id"];
            var filtered = Ids.AsEnumerable()
            .Where(r => r.Field<String>("OldID").Contains(oldID));

            if (filtered == null || filtered.Count() == 0)
            {
                DataRow row2 = Ids.NewRow();
                row2["NewID"] = item.ID.ToString();
                row2["OldID"] = item["Old Id"];
                Ids.Rows.Add(row2);
            }
        }

        public static string MapItemId(DataTable Ids, string OldId)
        {
            try
            {
                var filtered = Ids.AsEnumerable()
                 .Where(r => r.Field<String>("OldID").Contains(OldId));

                if (filtered != null && filtered.Count()>0)
                {
                    return filtered.FirstOrDefault()["NewID"].ToString();
                }else
                {
                    return OldId;
                }

            } catch(Exception ex)
            {
                throw ex;

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