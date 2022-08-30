<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="jwt.aspx.cs" Inherits="jwt_test.jwt_test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Logging into Qlik Sense SaaS with a .NET generated JWT token</title> 
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
        </asp:ScriptManager>
        <div>           
        </div>
    </form>       
</body>
    <script type="text/javascript">

        getJWT();

        function getJWT() {
            PageMethods.GetJWT(onSuccess);
        }

        function onSuccess(response, userContext, methodName) {
            var url = JSON.parse(response).Url;
            var jwt = JSON.parse(response).JWT;
            var webIntegrationID = JSON.parse(response).WebIntegrationID;
            var raw = "";

            var myHeaders = new Headers();
            myHeaders.append("qlik-web-integration-id", webIntegrationID);
            myHeaders.append("content-type", "application/json");
            myHeaders.append("Authorization", "Bearer " + jwt);

           var requestOptions = {
                method: 'POST',
                headers: myHeaders,
                body: raw,
                credentials: "include",
                mode: "cors",
                rejectunAuthorized: false
            };
            
            fetch(url + "/login/jwt-session?qlik-web-integration-id=" + webIntegrationID, requestOptions)
                .then(response => response.text())
                .then(result => {
                    console.log(result);
                    window.location.replace(url);
                })
                .catch(error => console.log('error', error));

    }
    </script>
</html>
