<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SampleHealthChecker.AspNet.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Health Checks Sample</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous" />
</head>
<body>
    <div class="container body-content">
        <h1>Overall status: <em><%: CheckResult.CheckStatus %></em></h1>

        <br />

        <table class="table">
            <thead>
                <tr><td>Name</td><td>Status</td><td>Description</td>
            </thead>
            <tbody>
                <% foreach (var kvp in CheckResult.Results) { %>
                    <tr><td><%: kvp.Key %></td><td><%: kvp.Value.CheckStatus %></td><td><pre><%: kvp.Value.Description %></pre></td></tr>
                <% } %>
            </tbody>
        </table>

        <br />

        <a href="health">API endpoint</a>

    </div>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" integrity="sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa" crossorigin="anonymous"></script>
</body>
</html>
