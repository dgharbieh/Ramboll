using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RambollImportData.Base
{
    public class Result
    {
        public StringBuilder CSV = new StringBuilder();
        public int RecourdNumber;
        public string StartPath = "", OutputName = "", TemplateName="";
        public string ExportPath = ""; 
        public bool IncludeLanguage = false;
        public bool IncludeVersions = false;
        public ArrayList ExportedFields = new ArrayList();
        public ArrayList ImportedFields = new ArrayList();
        public Dictionary<string, int> Totals = new Dictionary<string, int>();
        public int UpdatedRecords=0, InsertedNewRecords = 0;

        public Dictionary<string, string> InsertedVersionsTotals = new Dictionary<string, string>();
        public Dictionary<string, string> InsertedNewTotals = new Dictionary<string, string>();
        public Dictionary<string, string> UpdateTotals = new Dictionary<string, string>();

    }
}