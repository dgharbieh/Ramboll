using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RambollExportData.Base
{
    public class Result
    {
        public StringBuilder CSV = new StringBuilder();
        public int RecourdNumber;
        public string StartPath = "", OutputName = "", TemplateName="";
        public bool IncludeLanguage = false;
        public bool IncludeVersions = false;
        public ArrayList Fields = new ArrayList();
        public Dictionary<string, int> Totals = new Dictionary<string, int>();
    }
}