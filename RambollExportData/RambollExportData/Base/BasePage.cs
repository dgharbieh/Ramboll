using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RambollExportData
{
    public class BasePage : System.Web.UI.Page
    {
        public StringBuilder CSV = new StringBuilder();
        public int RecourdNumber;
        public string StartPath = "", OutputName = "";
        public bool IncludeLanguage = false;
        public bool IncludeVersions = false;
        public ArrayList Fields = new ArrayList();
        public Dictionary<string, string> Totals = new Dictionary<string, string>();

    }
}