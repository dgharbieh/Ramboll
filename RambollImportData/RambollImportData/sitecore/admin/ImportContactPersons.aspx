﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportContactPersons.aspx.cs" Inherits="RambollImportData.sitecore.admin.ImportContactPersons" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Import - ContactPersons</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Import ContactPersons </h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">
                      <fieldset>
                        <strong>ContactPersons Folder :</strong> 
                    </fieldset>
                    <fieldset>
                        <strong>Start Path :</strong> <%=ContactPersonsFolders.StartPath %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=ContactPersonsFolders.OutputName %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Language :</strong> <%=ContactPersonsFolders.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=ContactPersonsFolders.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                        <strong>Exported Fields :</strong>
                        <% foreach (var field in ContactPersonsFolders.ExportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <fieldset>
                        <strong>Imported Fields :</strong>
                        <% foreach (var field in ContactPersonsFolders.ImportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <hr />

                       <fieldset>
                        <strong>ContactPersons Items :</strong> 
                    </fieldset>
                    <fieldset>
                        <strong>Start Path :</strong> <%=ContactPersons.StartPath %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=ContactPersons.OutputName %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Language :</strong> <%=ContactPersons.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=ContactPersons.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                        <strong>Exported Fields :</strong>
                        <% foreach (var field in ContactPersons.ExportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <fieldset>
                        <strong>Imported Fields :</strong>
                        <% foreach (var field in ContactPersons.ImportedFields)
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
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the import completed successfully.
                         <br />
                        <strong>Inserted Folders:</strong><%=ContactPersonsFolders.InsertedNewRecords  %>
                          <br />
                        <strong>Updated Folders:</strong><%=ContactPersonsFolders.UpdatedRecords %>
                          <br />
                        <hr />
                        <br />
                       <strong>Updated ContactPersons:</strong>
                        <br />
                        <% foreach (var total in ContactPersons.UpdateTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                        <%}%>

                        <br />
                     
                        <strong>Inserted ContactPersons:</strong>
                        <br />
                        <% foreach (var total in ContactPersons.InsertedNewTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                        <%}%>
             
                        <br />
                    </asp:Panel>
                    <!-- success message end -->

                    <!-- error message begin -->
                    <asp:Panel ID="pnFailure" Visible="false" CssClass="alert alert-error" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Error</strong> there has been a problem with import data, please try again.
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
