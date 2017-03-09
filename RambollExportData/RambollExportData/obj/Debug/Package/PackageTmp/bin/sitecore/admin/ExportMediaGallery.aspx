<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportMediaGallery.aspx.cs" Inherits="RambollExportData.sitecore.admin.ExportMediaGallery" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">
    <title>Export- Delete - Media Gallary</title>
    <style>

        .customimage{
            width:20px;height:20px;
        }
    </style>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Media Gallary</h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">
                    <fieldset>
                        Width&nbsp; :  
                        <asp:TextBox ID="txtWidth" Text="270" runat="server"></asp:TextBox>
                    </fieldset>
                    <br />
                    <fieldset>
                        Height :  
                        <asp:TextBox ID="txtHeight" Text="270" runat="server"></asp:TextBox>
                    </fieldset>
                   <br />
                     <fieldset>
                        <asp:Button ID="btnExport" runat="server" Text="Export to CSV" OnClick="ExportData" />
                         <asp:Button ID="btnDelete" runat="server" Text="Delete" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the export completed for <%=RecourdNumber %> records.
                    </asp:Panel>
                    <!-- success message end -->

                    <!-- error message begin -->
                    <asp:Panel ID="pnFailure" Visible="false" CssClass="alert alert-error" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Error</strong> there has been a problem with extract data, please try again.
                    </asp:Panel>
                    <!-- error message end -->
                    <asp:GridView 
                        ID="GridItems" 
                        runat="server" 
                        DataKeyNames="ID" 
                        EnablePersistedSelection="true"
                        SelectedRowStyle-BackColor="Yellow" 
                        AllowPaging="true" 
                        AllowSorting="true"
                        PageSize = "5" 
                        AutoGenerateColumns="false" 
                        OnRowDeleting="GridItems_RowDeleting"
                     OnPageIndexChanging="GridItems_PageIndexChanging"
                      
                        >
                        <Columns>
                          <asp:CommandField  ControlStyle-CssClass="customimage"  ShowDeleteButton="true" ButtonType="Image"  DeleteImageUrl="~/includes/images/delete.png"   /> 
                            <asp:BoundField DataField="ID" HeaderText="ID"  ReadOnly="True" />
                            <asp:BoundField DataField="Name" HeaderText="Name"  ReadOnly="True" />
                            <asp:BoundField DataField="Path" HeaderText="Path"  ReadOnly="True" />
                             <asp:BoundField DataField="Height" HeaderText="Height"  ReadOnly="True" />
                             <asp:BoundField DataField="Width" HeaderText="Width"  ReadOnly="True" />
                            
                        </Columns>
                    </asp:GridView>



                </form>
            </div>

        </div>
        <script src="/includes/scripts/jquery-1.7.2.min.js"></script>
        <script src="/includes/scripts/bootstrap.min.js"></script>
    </div>
</body>
</html>
