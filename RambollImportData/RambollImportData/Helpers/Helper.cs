using Sitecore.ApplicationCenter.Applications;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
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
        public static void ParseMappingFile(BasePage page, string fileName)
        {
 
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
                        case "exported fields":
                            for (var i = 1; i < Fields.Count(); i++)
                            {
                                page.ExportedFields.Add(Fields[i]);
                            }
                            break;

                        case "imported fields":
                            for (var i = 1; i < Fields.Count(); i++)
                            {
                                page.ImportedFields.Add(Fields[i]);
                            }
                            break;
                    }
                }
            }
        }


        public static string ReplaceComma(string data)
        {
            return data.Replace("#;#", ", ");
        }
        public static Database GetDatabase()
        {
            return Sitecore.Configuration.Factory.GetDatabase("master");
        }


        public static DataTable GetDataTable(BasePage page)
        {
            DataTable dt = new DataTable();
             string   fileName = Settings.GetSetting("FolderPath") + "\\" + page.OutputName + ".csv";
            if (File.Exists(fileName))
            {
                bool isheader = true;
                foreach (var line in File.ReadAllLines(fileName))
                {
                    string[] Fields = line.Split(new char[] { ',' });
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



        //public static void CreateFile(string data, string fileName)
        //{

        //    try
        //    {

        //        fileName = Settings.GetSetting("FolderPath") + "\\" + fileName + ".csv";
        //        // Delete the file if it exists.
        //        if (File.Exists(fileName))
        //        {
        //            File.Delete(fileName);
        //        }

        //        // Create the file.
        //        using (FileStream fs = File.Create(fileName))
        //        {
        //            Byte[] info = new UTF8Encoding(true).GetBytes(data);
        //            // Add some information to the file.
        //            fs.Write(info, 0, info.Length);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteToFile("{0} :Exception" + ex.Message);
        //        throw ex;
        //    }

        //}

    }

}