# Use JWT with Qlik Sense SaaS and .NET#

## Introduction ##
This article describes how to generate a JSON Web Token (JWT) using .Net code (.Net Framework 4.6.1) in order to authenticate a user with Qlik Sense SaaS. 
Due to the .Net version, the solution makes use of 3rd party NuGet packages (BouncyCastle, jose-jwt).

![image](https://user-images.githubusercontent.com/72072893/185344360-09e1376a-651e-45a4-90bc-23f79b7623cc.png)



## Prerequsites ##
* Microsoft Visual Studio is installed on your machine to build and run the solution
* Install OpenSSL on the developer machine

## Installation ##

1. Use one of the options available when clicking on the "Code" button to clone the repository, open it directly with Visual Studio (easiest option) or download it as a ZIP file <br>

![image](https://user-images.githubusercontent.com/72072893/185239567-e80887b7-69ae-4be0-8101-0426c035c776.png)

2. Open a Command Prompt and change to the "certs" Directory of the solution, e.g.:

```
cd c:\temp\JWT-with-.Net-on-Qlik-Sense-SaaS\jwt\certs
```

3. Generate a signing certificate keypair (public and private key) by running the following commands from the Command Prompt. The commands can also be found in the file `createCerts.txt`

```
openssl genrsa -out private-key.pem 4096
openssl rsa -in private-key.pem -pubout -out public-key.pem
```

4. After running both commands you will have two certificate files inside the "certs" folder. The `private-key.pem` file is used in the .Net code to sign the JWT, the   `public-key.pem` is used in the configuration of a JWT identity provider (IdP) in Qlik Sense SaaS.

   1. Login to Qlik Sense SaaS.
   2. In the Management Console, open the section **Indentity provider**.
   3. Click **Create new**.
   4. Select IdP type **JWT** in the dropdown.
   5. Optionally, enter a description.
   6. Copy the content from the `public-key.pem` file into the **Certificate** field.
   7. Optionally specify an **Issuer** and a **Key ID**. If you leave the fields empty, some random values will be automatically assigned. It is IMPORTANT to remember both the Issuer and the Key ID. You will have to update the corresponding settings in the web.config later with those values later.
   8. Click **Create**.

   ![image](https://user-images.githubusercontent.com/6170297/169548503-30d14e7f-a1fa-4dc4-a70b-081ccdc0fa8f.png)

   9. In the Management Console, open the section **Web**.
   10. Click **Create new** to create a new web integration.
   11. Enter a value in the **Name** field, eg. `.Net`.
   12. Enter `http://localhost:55444` in the **Add an origin** field and Click **Add**. This will add you local web application to the list of trusted origins. 55444 is the default port of your web application when you run it in debug mode in Visual Studio. If you are using a different port, please amend as needed.
   13. Click **Create** to finish this step <br>
   

  ![Web1](https://user-images.githubusercontent.com/6170297/171605462-16c3d750-9908-4173-abd4-7a2fbfddb5de.GIF)
<br>
   14. Remeber the auto-generated **ID** of the list of web integrations for later use. You will have to insert it into the web.config of your web application. <br>

![Web2](https://user-images.githubusercontent.com/6170297/171605631-9f4b9a1b-d1d1-47fc-8369-b35c80bd9a95.GIF) <br>

   15. In the Management Console, open the section **Settings**.
   16. Make sure that the options **_"Enable dynamic assignment of professional users and/or analyzer users depending on the use case"_** and **_"Creation of groups_"**  are toogled on.

   ![image](https://user-images.githubusercontent.com/6170297/169549600-d4337cc6-966d-48e4-9a3d-94f799903eb0.png) ![image](https://user-images.githubusercontent.com/6170297/169549817-d530945d-92fa-4b53-b929-65e207d7f6e2.png)

17. Open the web solution in Visual Studio by clicking on the file `jwt.sln` in case you haven't opened the github repository directly in Visual Studio.
19. In the Solution Explorer of Visual Studio open the `web.config` file.
20. Change the values of the following appSettings to match your environment/configuration:
      - **PrivateCertificateFile**: Enter the full path to the private certificate `private-key.pem` you created in step 3.
      - **Issuer**: Enter the value of the `Issuer` from your IdP configuration in Qlik Sense SaaS.
      - **KeyID**: Enter the value of the `Key ID` from your IdP configuration in Qlik Sense SaaS.
      - **TenantUrl**: Enter the url of your Qlik Cloud tenant, e.g. `https://mytenant.eu.qlikcloud.com`.
      - **IntegrationID**: Enter the `ID` that was generated for your web integration configuration in Qlik Sense SaaS. If you forgot the ID, you can still go back to the `Web` section in the Management Console and copy it from your web integration configuration.
      - _(Optional)_: If you want, you can specify your own values for **ClaimName**, **ClaimEmail** and **ClaimGroups** in the web.config to match your environment/users. In a production system these values will most likely be dynamically assigned based on the user accessing the web application.
21. In the Solution Explorer of Visual Studio select the file 'jwt.aspx' and in the contexte menu of that file (right click on it), select the option **Set As Start Page**. That way `jwt.aspx` will automatically be loaded when you run the application from Visual Studio.

## Explanation of the code ##
The C# function GetJWT is called through JavaScript in the HTML code of the web application when it is loaded and handles the creation of the signed JSON Web Token (JWT). For simplicity of this example, most of the values required for the creation of the JWT are specified in the web.config. In a production environment, the following 4 claims of the payload will most likely be dynamically set:
- `sub`: This will in most case be a static value identical for all users.
- `name`: Assign the name of the user you are generating the JWT for.
- `email`: Assign the email of the user you are generating the JWT for.
- `groups`: Groups can be applied dynamically based on the access level the user needs in Qlik SaaS.
