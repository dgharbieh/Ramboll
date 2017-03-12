<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ImportCounties.aspx.cs" Inherits="RambollImportData.sitecore.admin.ImportCounties" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Import - Countries</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Import Countries - Update</h2>
                    </div>
                </div>

                <hr />

                <form class="form-inline" id="form2" runat="server">

                    <fieldset>
                        <strong>Start Path :</strong> <%=Countries.StartPath %>
                    </fieldset>
                    <fieldset>
                        <strong>Output Name :</strong> <%=Countries.OutputName %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Language :</strong> <%=Countries.IncludeLanguage.ToString() %>
                    </fieldset>
                    <fieldset>
                        <strong>Include Versions :</strong> <%=Countries.IncludeVersions.ToString()%>
                    </fieldset>
                    <fieldset>
                        <strong>Exported Fields :</strong>
                        <% foreach (var field in Countries.ExportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <fieldset>
                        <strong>Imported Fields :</strong>
                        <% foreach (var field in Countries.ImportedFields)
                           {%>
                        <%=field.ToString()%> |
                         <%}%>
                    </fieldset>
                    <fieldset>
                        <asp:Button ID="btnImport" runat="server" Text="Import from CSV" OnClick="ImportData" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the import completed successfully.
                          <br />
                        <strong>Updated Countries:</strong>
                        <br />
                        <% foreach (var total in this.UpdateTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                        <%}%>

                        <br />
                        <strong>Inserted Versions Countries:</strong>
                        <br />
                        <% foreach (var total in this.InsertedVersionsTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                        <%}%>

                        <br />
                        <strong>Inserted New Countries:</strong>
                        <br />
                        <% foreach (var total in this.InsertedNewTotals)
                            {%>
                        <strong>language (<%=total.Key%>)   </strong>:<%=total.Value%> records.<br />
                        <%}%>
                        <% if (!string.IsNullOrEmpty(NotMatchCountries))
                            {%>
                        <br />
                        <strong>Countries not match:</strong>
                        <br />
                        <%=NotMatchCountries %>

                        <%}%>
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
