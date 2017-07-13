using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RambollExportData.sitecore.admin
{
    public class Data
    {

        public string Mapping;
        public string RelatedTemplateName;
        public string RelatedTemplateId;
        public List<string> Fields = new List<string>();
        public List<string> Items = new List<string>();

    }
}