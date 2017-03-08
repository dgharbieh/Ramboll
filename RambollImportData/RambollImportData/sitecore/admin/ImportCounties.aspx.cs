using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RambollImportData.Helpers;
using Sitecore.SecurityModel;
using Sitecore.Data;
using Sitecore.Data.Items;
using System.Data;

namespace RambollImportData.sitecore.admin
{
    public partial class ImportCounties : BasePage
    {
        public  string NotMatchCountries = "";
        public int TotalRecords =0;
        protected void Page_Load(object sender, EventArgs e)
        {
            Helper.ParseMappingFile(this, "Countries");
        }

        protected void ImportData(object sender, EventArgs e)
        {
            try
            {
                
                DataTable data = Helper.GetDataTable(this);

                using (new SecurityDisabler())
                {
                    Database masterDb = Helper.GetDatabase();
                    Item parent = masterDb.GetItem(this.StartPath);
                    List<Item> countries = parent.Children.AsEnumerable().ToList();
                    TotalRecords = data.Rows.Count;
                    foreach (DataRow row in data.Rows)
                    {

                        Item Country= countries.Where(x => x.Name.ToLower() == row["Name"].ToString().ToLower() || x.Name.ToLower() == row["CountryName"].ToString().ToLower()).FirstOrDefault();
                        if (Country ==null)
                        {
                             Country = countries.Where(x => x.Name.ToLower().Contains(row["Name"].ToString().ToLower())).FirstOrDefault();

                            //Manual Mapping
                            if (row["Name"].ToString().ToLower()== "Congo-DR".ToLower() )
                            {
                                Country = countries.Where(x => x.Name.ToLower() == "Congo The Democratic Republic of the".ToLower()).FirstOrDefault();
                            }
                            if (row["Name"].ToString().ToLower() == "USA".ToLower())
                            {
                                Country = countries.Where(x => x.Name.ToLower() == "United States".ToLower()).FirstOrDefault();
                            }


                            if (Country == null)
                            {
                                NotMatchCountries += row["ID"].ToString() + "-" + row["Name"].ToString() + "<br/>";
                            }
                            else
                            {
                                UpdateItem(Country, row["ID"].ToString());
                                
                            }
                        }
                        else
                        {

                            UpdateItem(Country, row["ID"].ToString());
                        }
                    }

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

        private void UpdateItem(Item Country, string OldId)
        {

            Country.Editing.BeginEdit();
            try
            {
                Country["Old Id"] = OldId;
                Country.Editing.EndEdit();
                RecourdNumber += 1;
            }
            catch (Exception ex)
            {
                Country.Editing.CancelEdit();
                throw ex;
            }
        }
    }
}