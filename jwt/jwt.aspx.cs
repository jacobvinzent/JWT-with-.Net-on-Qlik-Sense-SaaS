using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace jwt_test
{
    public partial class jwt_test : System.Web.UI.Page
    {
        static readonly string privateCertificateFile = System.Configuration.ConfigurationManager.AppSettings["PrivateCertificateFile"];
        static readonly string issuer = System.Configuration.ConfigurationManager.AppSettings["Issuer"];
        static readonly string keyID = System.Configuration.ConfigurationManager.AppSettings["KeyID"];
        static readonly string tenantUrl = System.Configuration.ConfigurationManager.AppSettings["TenantUrl"];
        static readonly string webIntegrationID = System.Configuration.ConfigurationManager.AppSettings["WebIntegrationID"];
        static readonly string claimName = System.Configuration.ConfigurationManager.AppSettings["ClaimName"];
        static readonly string claimEmail = System.Configuration.ConfigurationManager.AppSettings["ClaimEmail"];
        static readonly string[] claimGroups = System.Configuration.ConfigurationManager.AppSettings["ClaimGroups"].ToString().Split(';');
        
        [System.Web.Services.WebMethod]
        public static string GetJWT()
        {
            string jwt_out = CreateJWT(claimName, claimEmail, claimGroups);
            JwtResponse resp = new JwtResponse
            {
                JWT = jwt_out,
                Url = tenantUrl,
                WebIntegrationID = webIntegrationID
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(resp);
        }

        public  static string CreateJWT(string name, string email, string[] groups)
        {
            try
            {
                //Read the content of the private certificate file
                if (!File.Exists(privateCertificateFile))
                {
                    throw new FileNotFoundException(string.Format("The file {0} could not be found", privateCertificateFile));
                }
                string privateKey = File.ReadAllText(privateCertificateFile);

                //Prepare the unix time values required for the signing process. All time values must be of type integer
                var dtOffsetNow = DateTimeOffset.UtcNow;
                var creationTime = (int)dtOffsetNow.ToUnixTimeSeconds();
                var expTime = (int)dtOffsetNow.AddSeconds(500).ToUnixTimeSeconds();
                var nbfTime = (int)dtOffsetNow.ToUnixTimeSeconds(); 

                //Prepare the header
                Dictionary<string, object> headers = new Dictionary<string, object>
                {
                    { "typ", "JWT"},
                    { "alg", "RS256" },
                    { "kid", keyID }
                };

                //Prepare the payload
                var payload = new Dictionary<string, object>
                {
                    { "sub", Guid.NewGuid().ToString() },
                    { "subType", "user" },
                    { "name", name },
                    { "email", email },
                    { "email_verified", "true"},
                    { "iss", issuer },
                    { "iat", creationTime }, //value must be of type integer
                    { "nbf", nbfTime }, //value must be of type integer
                    { "exp", expTime }, //value must be of type integer
                    { "jti", Guid.NewGuid().ToString() },
                    { "aud", "qlik.api/login/jwt-session" },
                    { "groups", groups }
                };

                //Create and return the JWT token               
                var token = CreateToken(payload, privateKey, headers);

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        public static string CreateToken(Dictionary<string, object> payload, string privateRsaKey, Dictionary<string, object> headers)
        {
            RSAParameters rsaParams = new RSAParameters();

            try
            {
                using (var tr = new StringReader(privateRsaKey))
                {
                    var pemReader = new PemReader(tr);

                    //We need to check the first line of the certificate file for reading the RSA keys/parameters 
                    if (privateRsaKey.Contains("-----BEGIN RSA PRIVATE KEY-----"))
                    {
                        if (!(pemReader.ReadObject() is AsymmetricCipherKeyPair keyPair))
                        {
                            throw new Exception("Could not read RSA private key");
                        }
                        var privateRsaParams = keyPair.Private as RsaPrivateCrtKeyParameters;
                        rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                    }
                    else if (privateRsaKey.Contains("-----BEGIN PRIVATE KEY-----"))
                    {
                        if (!(pemReader.ReadObject() is AsymmetricKeyParameter keyParam))
                        {
                            throw new Exception("Could not read RSA private key");
                        }
                        var privateRsaParams = keyParam as RsaPrivateCrtKeyParameters;
                        rsaParams = DotNetUtilities.ToRSAParameters(privateRsaParams);
                    }
                    else
                    {
                        //Handle extra cases if required
                        throw new Exception("Unknown key format. This key is not supported yet");
                    }
                }

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(rsaParams);
                    return Jose.JWT.Encode(payload, rsa, Jose.JwsAlgorithm.RS256, extraHeaders: headers);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }            
        }
    }
}