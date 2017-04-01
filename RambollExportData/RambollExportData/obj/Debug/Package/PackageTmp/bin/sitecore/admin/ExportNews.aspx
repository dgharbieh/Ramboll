<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportNews.aspx.cs" Inherits="RambollExportData.sitecore.admin.ExportNews" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Export - News</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Export News</h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">
                    <fieldset>
                       <strong> News Folders</strong>
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
                       <strong> News Items</strong>
                    </fieldset>
                   <fieldset>
                       <strong> Start Path :</strong> <%=News.StartPath %>
                    </fieldset>
                     <fieldset>
                        <strong>Template Name :</strong> <%=News.TemplateName  %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=News.OutputName %>
                    </fieldset>
                    <fieldset>
                       <strong> Include Language :</strong> <%=News.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=News.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                     <strong>Exported Fields :</strong>
                          <% foreach (var field in News.Fields)
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
                        <strong>News :</strong> 
                          <br />
                           <% foreach (var total in News.Totals)
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

