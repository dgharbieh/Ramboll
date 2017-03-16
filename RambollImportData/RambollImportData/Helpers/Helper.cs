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

    }

}