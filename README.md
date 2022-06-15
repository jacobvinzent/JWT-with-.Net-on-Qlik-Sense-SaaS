# Use JWT with Qlik Sensee SaaS and .NET#

## Introduction ##
This article describes how to use JWT in Qlik Sense SaaS using .Net code.

## Prerequsites ##
1. Microsoft Visual Studio is installed 



## Installation ##
1. Clone this repositorty or donwload and unzip it
2. Open the directory <solutionDir>/certs in a command line
3. Run the two commands specified in the createsCerts.txt
4. After running both commands you have two certificate files in the folder. The privatekey.pem file is used in the .Net code when creating the JWT. The   publickey.cer is used in a JWT IDP configuration in Qlik Sense SaaS.
   1. Login to Qlik Sense SaaS and navigate to the Management Console.
   2. Select Indentity Provider in the menu.
   3. Click create new.
   4. Select JWT in the Type dropdown.
   5. Copy the certificate from the publickey.cer file into the Certificate field.
   6. You can either specify an Issuer and a Key ID, if you don't enter values, some random values will be automatically assigned. It is IMPORTANT to remember both the Issuer and the Key ID.
   7. Click Create <br>
   ![image](https://user-images.githubusercontent.com/6170297/169548503-30d14e7f-a1fa-4dc4-a70b-081ccdc0fa8f.png)

   8. Click on Web in the menu
   9.  Create new
   10. Enter a name in Name field, eg. .Net App
   11. Enter http://localhost:55444 (55444 is the port your web app will run on when published, change to correct port in your setup) in Add an Origin and Click Add
   12. Click Create <br>
  ![Web1](https://user-images.githubusercontent.com/6170297/171605462-16c3d750-9908-4173-abd4-7a2fbfddb5de.GIF)

   13. You need the auto-generated ID from the list for later use <br>
![Web2](https://user-images.githubusercontent.com/6170297/171605631-9f4b9a1b-d1d1-47fc-8369-b35c80bd9a95.GIF)

   14. Click on Settings in the menu
   15. Make sure that "Enable dynamic assignment of professional users and/or analyzer users depending on the use case" and "Creation of groups" both are toogled on.
   ![image](https://user-images.githubusercontent.com/6170297/169549600-d4337cc6-966d-48e4-9a3d-94f799903eb0.png) ![image](https://user-images.githubusercontent.com/6170297/169549817-d530945d-92fa-4b53-b929-65e207d7f6e2.png)


5. Open the code from the git repository in Microsoft Visual Studio
6. Open the web.config file
7. Change the appSettings values 
   1. certsPath should point to the directory where the certificates are stored
   2. issuer is the Issuer you saved when created the IDP in Qlik Sense SaaS
   3. keyID is the Key ID you saved when created the IDP in Qlik Sense SaaS
   4. QlikSaaSInstance is the SaaS instance URL (eg mytenant.eu.qlikcloud.com)
   5. QlikIntegrationID is the ID found in the Management Console after the Webintegration form was created. You can still go back to the Management Console and click on Web to find the ID.
8. Test it on an IIS or an IIS express server, navigate to the jwt.aspx page
## Explanation of the code ##
The getJWT function is the one creating the signed jwt, most of the values are taken from static variables defined with values specified in the web.config <br>
There are 4 more values you most likely will change<br>
1. claims.put("sub", "SomeSampleSeedValue1"); this will in most case be a static value identical for all users.
2. claims.put("name", "John Doe"); here the name of the user you are generating the JWT for should be specified.
3. claims.put("email", "JohnD@john.com"); here the email of the user you are generating the JWT for should be specified.
4. The first line in the getJWT function (string[] groups = { "Administrators", "Sales", "Marketing" };) groups can be applied dynamically based on the access level the user needs in Qlik SaaS
   
   [![Run in Postman](https://run.pstmn.io/button.svg)](https://app.getpostman.com/run-collection/13762341-4b37d4fd-d515-4b27-9e7b-664c7930ae78?action=collection%2Ffork&collection-url=entityId%3D13762341-4b37d4fd-d515-4b27-9e7b-664c7930ae78%26entityType%3Dcollection%26workspaceId%3D291f8476-5f4d-4bad-ae87-071012d07349)
   
   [![Run in Postman](https://run.pstmn.io/button.svg)](https://app.getpostman.com/run-collection/13762341-4b37d4fd-d515-4b27-9e7b-664c7930ae78?action=collection%2Ffork&collection-url=entityId%3D13762341-4b37d4fd-d515-4b27-9e7b-664c7930ae78%26entityType%3Dcollection%26workspaceId%3D291f8476-5f4d-4bad-ae87-071012d07349)
