<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UpdateRelatedTemplates.aspx.cs" Inherits="RambollImportData.sitecore.admin.UpdateRelatedTemplates" %>


<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="robots" content="noindex,nofollow">

    <link rel="stylesheet" type="text/css" href="/includes/css/bootstrap.css">
    <link rel="stylesheet" type="text/css" href="/includes/css/custom.css">

    <title>Update - Related Templates</title>
</head>
<body>

    <div class="contentContainer container">
        <div class="row">
            <div class="span12">

                <div class="row pageIntro">
                    <div class="span12">
                        <h2>Update Related Templates </h2>
                    </div>
                </div>

                <hr />

              <form class="form-inline" id="form2" runat="server">  
                    <hr />
                    <fieldset>
                        <asp:Button ID="btnUpdate" runat="server" Text="Update Data" OnClick="UpdateData" />
                    </fieldset>

                    <hr />

                    <!-- success message begin -->
                    <asp:Panel ID="pnSuccess" Visible="false" CssClass="alert alert-success" runat="server">
                        <button type="button" class="close" data-dismiss="alert">&times;</button>
                        <strong>Success</strong> the import completed successfully.
                         <br />
                         <% foreach (var result in ALLresultItem )
                           {%>
                        <%=result.FileName %> : <br />
                        NotFoundRelatedRecords: <%=result.NotFoundRelatedRecords %> <br />
                        UpdatedRelatedRecords: <%=result.UpdatedRelatedRecords %> <br />
                        LanguageVersionRelatedRecords: <%=result.LanguageVersionRelatedRecords %> <br />
                         <br /><hr />
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
