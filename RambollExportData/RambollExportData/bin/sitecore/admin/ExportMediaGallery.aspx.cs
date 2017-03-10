using RambollExportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RambollExportData.sitecore.admin
{
    public partial class ExportMediaGallery : BasePage
    {

        public int Height=0, Width=0;
        public DataTable data;

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(this, "MediaGallery");

        }

        protected void ExportData(object sender, EventArgs e)
        {
             try
            {
                string h = txtHeight.Text;
                Int32.TryParse(h, out Height);
                string w = txtWidth.Text;
                Int32.TryParse(w, out Width);
                data = new DataTable();
                Helper.SetDataTableColums(data,this.Fields);


                CSV.AppendLine(Helper.GetHeader(this.Fields));
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(this.StartPath);
                    GetData(parent);
                    Cache["data"] = data;
                    GridItems.DataSource = data;
                    GridItems.DataBind();  
                    Helper.CreateFile(CSV.ToString(), this.OutputName);
                }

                pnSuccess.Visible = true;
                pnFailure.Visible = false;

            }
            catch (Exception ex)
            {
                pnSuccess.Visible = false;
                pnFailure.Visible = true;
            }

        }

        private void GetData(Item  parent)
        {

            if (parent.Children.Count == 0 )
                return;

            foreach (var item in parent.Children.AsEnumerable())
            {
                
                if (item.TemplateName.ToLower() == "image" || item.TemplateName.ToLower() == "jpeg")
                {
                    int height = 0, width = 0;
                    string h = item.Fields["Height"].Value;
                    Int32.TryParse(h, out height);
                    string w = item.Fields["Width"].Value;
                    Int32.TryParse(w, out width);
                    //Check the condition
                    if (width <= Width && height <= Height)
                    {
                       
                        Helper.GetDataRowFields(data,item, Fields);          
                        CSV.AppendLine(Helper.GetFieldsLine(item, Fields));
                        RecourdNumber = RecourdNumber + 1;
                    }
                }
                GetData(item);
            }
        }

    
        protected void GridItems_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            DataTable  data = (DataTable) Cache["data"];

            Item item = Helper.GetDatabase().GetItem(data.Rows[e.RowIndex]["ID"].ToString());
            item.Delete();
            data.Rows.RemoveAt(e.RowIndex);
            Cache["data"] = data;
            GridItems.DataSource = data;
            GridItems.DataBind();

        }
        protected void GridItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataTable data = (DataTable)Cache["data"];
            GridItems.DataSource = data;
            GridItems.PageIndex = e.NewPageIndex;
            GridItems.DataBind();
        }

 }
}