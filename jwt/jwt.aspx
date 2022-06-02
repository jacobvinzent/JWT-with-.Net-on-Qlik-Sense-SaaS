<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="jwt.aspx.cs" Inherits="jwt_test.jwt_test" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script type="text/javascript">
    function ShowCurrentTime() {
        PageMethods.getJWT(OnSuccess);
    }
    function OnSuccess(response, userContext, methodName) {
      var url = JSON.parse(response).url;
      var jwt = JSON.parse(response).jwt;
        var integrationID = JSON.parse(response).integrationID;
        console.log(jwt);

        var raw = ""; 
          var myHeaders = new Headers();
            myHeaders.append("qlik-web-integration-id", integrationID);
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
            fetch("https://" +  url + "/login/jwt-session?qlik-web-integration-id=" + integrationID, requestOptions)
                .then(response => response.text())
                .then(result => {
                    console.log(result);
                   window.location.replace("https://" +  url );
                })
                .catch(error => console.log('error', error));

    }
</script>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true">
        </asp:ScriptManager>
        <div>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Button" />
            <asp:TextBox ID="TextBox1" runat="server" Height="503px" Width="767px"></asp:TextBox>
            <asp:TextBox ID="txtUserName" runat="server" ></asp:TextBox>
            <input id="btnGetTime" type="button" value="Show Current Time" onclick="ShowCurrentTime()"/>

            <br />
        </div>
    </form>
</body>
</html>
