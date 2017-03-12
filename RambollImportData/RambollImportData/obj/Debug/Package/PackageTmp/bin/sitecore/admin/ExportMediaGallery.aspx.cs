using RambollImportData.Base;
using RambollImportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.SecurityModel;
using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace RambollImportData.sitecore.admin
{
    public partial class ExportMediaGallery : System.Web.UI.Page
    {

        public int Height=0, Width=0;
        public DataTable data;
        public Result MediaGalleryItems;

        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(ref MediaGalleryItems, "MediaGallery");
            if (!Page.IsPostBack)
            {
                txtStartPath.Text = MediaGalleryItems.StartPath;
            }

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
                Helper.SetDataTableColums(data, MediaGalleryItems.ExportedFields);


                MediaGalleryItems.CSV.AppendLine(Helper.GetHeader(MediaGalleryItems.ExportedFields));
                using (new SecurityDisabler())
                {

                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(txtStartPath.Text.Trim());
                    if (parent != null)
                    {
                        GetData(parent);
                    }
                    Cache["data"] = data;
                    GridItems.DataSource = data;
                    GridItems.DataBind();
                    Helper.CreateFile(MediaGalleryItems.CSV.ToString(), MediaGalleryItems.OutputName);
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

                        Helper.GetDataRowFields(data, item, MediaGalleryItems.ExportedFields);
                        MediaGalleryItems.CSV.AppendLine(Helper.GetFieldsLine(item, MediaGalleryItems.ExportedFields));
                        MediaGalleryItems.RecourdNumber = MediaGalleryItems.RecourdNumber + 1;
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
            MediaGalleryItems.RecourdNumber = data.Rows.Count;
            GridItems.DataSource = data;
            GridItems.DataBind();

        }
        protected void GridItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataTable data = (DataTable)Cache["data"];
            MediaGalleryItems.RecourdNumber = data.Rows.Count;
            GridItems.DataSource = data;
            GridItems.PageIndex = e.NewPageIndex;
            GridItems.DataBind();
        }

 }
}