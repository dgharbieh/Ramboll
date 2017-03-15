<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportAliases.aspx.cs" Inherits="RambollExportData.sitecore.admin.ExportAliases" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Export - Aliases</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Export Aliases</h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">
                    <fieldset>
                       <strong> Aliases Folders</strong>
                    </fieldset>
                   <fieldset>
                       <strong> Start Path :</strong> <%=Folders.StartPath %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=Folders.OutputName %>
                    </fieldset>
                     <fieldset>
                        <strong>Template Name :</strong> <%=Folders.TemplateName  %>
                    </fieldset>
                    <fieldset>
                       <strong> Include Language :</strong> <%=Folders.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=Folders.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                     <strong>Exported Fields :</strong>
                          <% foreach (var field in Folders.Fields)
                             {%>              
                             <%=field.ToString()%> |
                         <%}%>    
                    </fieldset>

                    <hr />
                       <fieldset>
                       <strong> Aliases Items</strong>
                    </fieldset>
                   <fieldset>
                       <strong> Start Path :</strong> <%=Aliases.StartPath %>
                    </fieldset>
                     <fieldset>
                        <strong>Template Name :</strong> <%=Aliases.TemplateName  %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=Aliases.OutputName %>
                    </fieldset>
                    <fieldset>
                       <strong> Include Language :</strong> <%=Aliases.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=Aliases.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                     <strong>Exported Fields :</strong>
                          <% foreach (var field in Aliases.Fields)
                             {%>              
                             <%=field.ToString()%> |
                         <%}%>    
                    </fieldset>

                    <hr />


                    <fieldset>
                        <asp:Button ID="btnExport" runat="server" Text="Export to CSV" OnClick="ExportData" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the export completed successfully 
                        <br />
                        <strong>Folders :</strong>  <%=Folders.RecourdNumber  %> records.
                           <br />
                        <strong>Aliases :</strong> 
                          <br />
                           <% foreach (var total in Aliases.Totals)
                              {%>              
                            <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                         <%}%> 
                    
                    </asp:Panel>
                    <!-- success message end -->

                    <!-- error message begin -->
                    <asp:Panel ID="pnFailure" Visible="false" CssClass="alert alert-error" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Error</strong> there has been a problem with extract data, please try again.
                    </asp:Panel>
                    <!-- error message end -->
                </form>
            </div>

        </div>
        <script src="/includes/scripts/jquery-1.7.2.min.js"></script>
        <script src="/includes/scripts/bootstrap.min.js"></script>
    </div>
</body>
</html>

