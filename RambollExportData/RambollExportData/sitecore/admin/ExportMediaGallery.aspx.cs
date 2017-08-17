using RambollExportData.Base;
using RambollExportData.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Resources.Media;
using Sitecore.SecurityModel;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace RambollExportData.sitecore.admin
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

            pnSuccess.Visible = false;
            pnFailure.Visible = false;
            pnSuccessUpdate.Visible = false;
            pnFailureUpdate.Visible = false;
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
                Helper.SetDataTableColums(data, MediaGalleryItems.Fields);


                MediaGalleryItems.CSV.AppendLine(Helper.GetHeader(MediaGalleryItems.Fields));
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

                        Helper.GetDataRowFields(data, item, MediaGalleryItems.Fields);
                        MediaGalleryItems.CSV.AppendLine(Helper.GetFieldsLine(item, MediaGalleryItems.Fields));
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

        protected void FireRowCommand(object sender, GridViewCommandEventArgs e)
        {

            string command = e.CommandName;
            if (command == "FireRowCommand")
            {
                string autoId = e.CommandArgument.ToString();

                if (FileUploadControl.HasFile)
                {
                    try
                    {
                        Item mediaItem = Helper.GetDatabase().GetItem(autoId);


                        string fileNameWithExtension = Path.GetFileName(FileUploadControl.FileName);
                        string extention = Path.GetExtension(FileUploadControl.FileName);
                        string filename = fileNameWithExtension.Replace(extention, "");

                        MemoryStream memStream = new MemoryStream(FileUploadControl.FileBytes);

                        var options = new MediaCreatorOptions();
                        options.FileBased = false;
                        options.IncludeExtensionInItemName = false;
                        options.KeepExisting = true;
                        options.Versioned = false;
                        options.Destination = mediaItem.Parent.Paths.FullPath + "/" + filename;
                        options.Database = Helper.GetDatabase();


                        var creator = new MediaCreator();

                        //Create a new item
                        var newItem = creator.CreateFromStream(memStream, fileNameWithExtension, options);
                        newItem.Editing.BeginEdit();
                        newItem.Fields["Title"].Value = filename;
                        newItem.Fields["Alt"].Value = mediaItem.Fields["Alt"].Value;
                        newItem.Editing.EndEdit();

                        Item[] referrers = Helper.GetReferrersAsItems(ref mediaItem);

                        if (referrers != null)
                        {
                            Sitecore.Data.Items.MediaItem newMediaItem = new Sitecore.Data.Items.MediaItem(newItem);

                            foreach (Item referr in referrers)
                            {
                                var Fields = referr.Fields.Where(x => x.Value.Contains(mediaItem.ID.ToString()));
                                if (Fields != null && Fields.Count() > 0)
                                {
                                    referr.Editing.BeginEdit();

                                    foreach (var field in Fields)
                                    {
                                        if (field.Type == "Image")
                                        {

                                            Sitecore.Data.Fields.ImageField imageField = referr.Fields[field.Name];

                                            var alt = imageField.Alt;
                                            var sclass = imageField.Class;
                                            imageField.Clear();
                                            imageField.MediaID = newMediaItem.ID;
                                            imageField.Alt = alt;
                                            imageField.Class = sclass;
                                        }
                                        else
                                        {
                                            referr.Fields[field.Name].Value = referr.Fields[field.Name].Value.Replace(mediaItem.ID.ToString(), newItem.ID.ToString());

                                        }
                                    }

                                    referr.Editing.EndEdit();

                                }

                            }
                        }


                        pnSuccessUpdate.Visible = true;

                    }
                    catch (Exception ex)
                    {
                        pnFailureUpdate.Visible = true;

                    }
                }
                else
                {
                    pnFailureUpdate.Visible = true;
                }
            }
        }


        protected void GridItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            DataTable data = (DataTable)Cache["data"];
            GridItems.DataSource = data;
            MediaGalleryItems.RecourdNumber = data.Rows.Count;
            GridItems.PageIndex = e.NewPageIndex;
            GridItems.DataBind();
        }

 }
}