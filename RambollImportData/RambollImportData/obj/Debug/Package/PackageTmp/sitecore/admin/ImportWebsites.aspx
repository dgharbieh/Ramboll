<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportWebsites.aspx.cs" Inherits="RambollImportData.sitecore.admin.ImportWebsites" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Import - Websites</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Import Websites </h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">
           
                    <hr />

                       <fieldset>
                        <strong>Websites Items :</strong> 
                    </fieldset>
                    <fieldset>
                        <strong>Start Path :</strong> <%=FullWebsites[0].StartPath %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=FullWebsites[0].OutputName %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Language :</strong> <%=FullWebsites[0].IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=FullWebsites[0].IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                        <strong>Exported Fields :</strong>
                        <% foreach (var field in FullWebsites[0].ExportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <fieldset>
                        <strong>Imported Fields :</strong>
                        <% foreach (var field in FullWebsites[0].ImportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <hr />
                    <fieldset>
                        <asp:Button ID="btnImport" runat="server" Text="Import from CSV" OnClick="ImportData" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                 <% foreach (var result in FullWebsites)
                           {%>
                       <strong>Updated <%=result.TemplateName  %>:</strong>
                        <br />
                        <% foreach (var total in result.UpdateTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%>records.<br />
                        <%}%>

                        <br />
                     
                        <strong>Inserted <%=result.TemplateName  %>:</strong>
                        <br />
                        <% foreach (var total in result.InsertedNewTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%>records.<br />
                        <%}%>
                          <br />
                   <%}%>
                        <br />
                    </asp:Panel>
                    <!-- success message end -->
            
                    <!-- error message begin -->
                    <asp:Panel ID="pnFailure" Visible="false" CssClass="alert alert-error" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Error</strong> there has been a problem with import data, please try again.
                       Parent Not Found for:
                        <br/>
                        <%=ParentNotFound %>
                    </asp:Panel>
                    <!-- error message end -->

                     <asp:Panel ID="pnParentNotFound" Visible="false" CssClass="alert alert-error" runat="server">
                           <button type="button" class="close" data-dismiss="alert">&times;</button>
                            Parent Not Found for:
                        <br/>
                        <%=ParentNotFound %>
                    </asp:Panel>

                     <asp:Panel ID="pnMove" Visible="false" CssClass="alert alert-success" runat="server">
                           <button type="button" class="close" data-dismiss="alert">&times;</button>
                           items:
                        <br/>
                        <%=Counter %>
                    </asp:Panel>

                       <asp:Button ID="btnMove" runat="server" Text="Move" OnClick="Move_Click" />
                    
                </form>
            </div>

        </div>
        <script src="/includes/scripts/jquery-1.7.2.min.js"></script>
        <script src="/includes/scripts/bootstrap.min.js"></script>
    </div>
</body>
</html>
