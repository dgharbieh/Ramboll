﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportRelatedTemplate.aspx.cs" Inherits="RambollExportData.sitecore.admin.ExportRelatedTemplate" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Export - Related Template</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Export Related Template</h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">

                      <fieldset>
                        <asp:Button ID="btnExportRef" runat="server" Text="Export Referrers Data to CSV" OnClick="ExportDataref" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the export completed successfully.
                        <br />
                           <% foreach (var total in Countries.Totals)
                              {%>              
                            <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                         <%}%> 
                    </asp:Panel>


                    <!-- success message begin -->
                    <asp:Panel ID="pnReferrersSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the export completed successfully.
                        <br />
                          
                          <% foreach (var result in ALLresultItem)
                             {%>     
                        <%=result.fileName %>
                       <%--   <% foreach (var temp in result.ReferrersTemplateField)
                              {%>              
                            <strong>Tempalet Name: <%=temp.Key%>   </strong>Tempalet Field :<%=temp.Value%> .<br />
                         <%}%> --%>
                             <%}%> 

                    </asp:Panel>
                   
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
