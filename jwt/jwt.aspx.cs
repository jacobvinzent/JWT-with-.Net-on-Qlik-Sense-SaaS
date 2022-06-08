using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Threading;
using System.Security.Claims;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RestSharp;
using System.Threading.Tasks;

namespace jwt_test
{
    public partial class jwt_test : System.Web.UI.Page
    {
        static String certsPath = System.Configuration.ConfigurationManager.AppSettings["certsPath"];
        static String issuer = System.Configuration.ConfigurationManager.AppSettings["issuer"];
        static String keyID = System.Configuration.ConfigurationManager.AppSettings["keyID"];
        static String expiresIn = System.Configuration.ConfigurationManager.AppSettings["expiresIn"];
        static String notBefore = System.Configuration.ConfigurationManager.AppSettings["notBefore"];
        static String QlikSaaSInstance = System.Configuration.ConfigurationManager.AppSettings["QlikSaaSInstance"];
        static String QlikIntegrationID = System.Configuration.ConfigurationManager.AppSettings["QlikIntegrationID"];
        


        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void Button1_Click(object sender, EventArgs e)
        {


            


        }
        
        [System.Web.Services.WebMethod]
        public static string getJWT()
        {
            string[] groups = { "Administrators", "Sales", "Marketing" };
            string jwt_out = createJWT("jvi", groups, "Jacob Vinzent");
            jwtResponse resp = new jwtResponse();
            resp.jwt = jwt_out;
            resp.url = QlikSaaSInstance;
            resp.integrationID = QlikIntegrationID;
            return Newtonsoft.Json.JsonConvert.SerializeObject(resp);
        }

        static async Task getJWT_task(string jwt)
        {

           
            var client = new RestClient("https://" + QlikSaaSInstance);
            var request = new RestRequest("login/jwt-session?qlik-web-integration-id=" + QlikIntegrationID, Method.Post);

           



            request.AddHeader("qlik-web-integration-id", "iIYLxGEXTpeCYDR1DIoKLgRWoRKK3bp6");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("Authorization", "Bearer " + jwt);
            //var body = @"";
            //request.AddParameter("application/json", body, ParameterType.RequestBody);

            var token = new CancellationToken(true);

            RestResponse resp = await client.ExecuteAsync(request);



            string r = string.Empty;


        }

        public  static string createJWT(string username, string[] groups, string name)
        {
            string privateKey = File.ReadAllText(certsPath + "privatekey.pem");

            Int32 creationTime= (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            Int32 ExpTime= (int)DateTime.UtcNow.AddSeconds(10).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;



            var claims = new List<Claim>();

            claims.Add(new Claim("exp", ExpTime.ToString()));
            claims.Add(new Claim("iat", creationTime.ToString()));
            claims.Add(new Claim("sub", "SomeSampleSeedValue1"));
            claims.Add(new Claim("subType", "user"));
            claims.Add(new Claim("name", "John Doe"));
            claims.Add(new Claim("email", "JohnD@john.com"));
            claims.Add(new Claim("iss", issuer));
            claims.Add(new Claim("aud", "qlik.api/login/jwt-session"));
            claims.Add(new Claim("jti", generateRandomString(32)));
            claims.Add(new Claim("email_verified", "true"));
          

            Dictionary<string, object> headers = new Dictionary<string, object>
            {
                {"typ", "JWT"},
                { "alg", "RS256" },
                {"kid", keyID },
                {"expiresIn", expiresIn },
                {"notBefore", notBefore }

        };


            var token = CreateToken(claims, privateKey, headers, groups);
            
            return token;
        }


        public static string generateRandomString(int length)
        {
            Random random = new Random();
            
            var rString = "";
            for (var i = 0; i < length; i++)
            {
                rString += ((char)(random.Next(1, 26) + 64)).ToString().ToLower();
            }
            return rString;
        }

        public static string CreateToken(List<Claim> claims, string privateRsaKey, Dictionary<string, object> headers, string[] groups)
        {
            RSAParameters rsaParams;
            using (var tr = new StringReader(privateRsaKey))
            {
                var pemReader = new PemReader(tr);
                var keyPair = pemReader.ReadObject() as AsymmetricCipherKeyPair;
                if (keyPair == null)
                {
                    throw new Exception("Could not read RSA private key");
                }
                var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(rsaParams);
                Dictionary<string, object> payload = claims.ToDictionary(k => k.Type, v => (object)v.Value);
                payload.Add("groups", groups);
                var headers_var = headers;
                return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256, extraHeaders: headers);
            }
        }

    }
    }