﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.Web.Http;
using Newtonsoft.Json.Linq;
using Swashbuckle.Swagger.Annotations;
using PerkinElmer.COE.Registration.Server.Code;

namespace PerkinElmer.COE.Registration.Server.Controllers
{
    [ApiVersion(Consts.apiVersion)]
    [AllowAnonymous]
    public class AuthController : RegControllerBase
    {
        private string CryptData(string txt, string key)
        {
            txt = ShiftChr(txt, -1);
            int keyLength = key.Length;
            int keyPtr = 0;
            string hold = string.Empty;
            string cryptKey = string.Empty;
            for (int i = 0; i < txt.Length; i++)
            {
                keyPtr = (keyPtr + 1) % keyLength;
                cryptKey = ((char)((short)txt[i] ^ (short)key[keyPtr])).ToString();
                hold += cryptKey;
            }
            return ShiftChr(hold, 1);
        }

        private string ShiftChr(string txt, int offset)
        {
            string hold = string.Empty;
            for (int i = 0; i < txt.Length; i++)
                hold += ((char)((short)txt[i] + offset)).ToString();
            return hold;
        }

        [HttpPost]
        [Route(Consts.apiPrefix + "auth/login")]
        [SwaggerOperation("Login")]
        [SwaggerResponse(200, type: typeof(JObject))]
        [SwaggerResponse(500, type: typeof(string))]
        public async Task<IHttpActionResult> Login([FromBody]JObject userData)
        {
            HttpResponseMessage responseMessage;
            var message = new StringBuilder();
            try
            {
                var webClient = new WebClient();
                var userName = userData["username"].ToString();
                var password = userData["password"].ToString();
                var loginUrl = string.Format("/COESingleSignOn/SingleSignOn.asmx/GetAuthenticationTicket?userName={0}&password=", userName);
                loginUrl = GetAbsoluteUrl(loginUrl, true);
                string token;
                message.AppendLine(string.Format("Opening {0}...", loginUrl));
                using (var loginResponse = new StreamReader(await webClient.OpenReadTaskAsync(loginUrl + password)))
                    token = loginResponse.ReadToEnd();
                var xml = new XmlDocument();
                xml.LoadXml(token);
                token = xml.DocumentElement.FirstChild.InnerText.Trim();
                message.AppendLine(string.Format("Token: {0}", token));
                responseMessage = Request.CreateResponse(HttpStatusCode.OK, new JObject(
                    new JProperty("data", new JObject(new JProperty("msg", "LOGIN SUCCESSFUL"))),
                    new JProperty("meta",
                        new JObject(
                            new JProperty("token", token),
                            new JProperty("profile", new JObject(new JProperty("fullName", userName)))
                        )
                    )
                ));
                var ssoCookie = new CookieHeaderValue(Consts.ssoCookieName, token);
                ssoCookie.Path = "/";
                var inactivityCookie = new CookieHeaderValue("DisableInactivity", "true");
                inactivityCookie.Path = "/";
                inactivityCookie.Expires = DateTime.Now.AddMinutes(25);
                var userNameCookie = new CookieHeaderValue("CS%5FSEC%5FUserName", HttpUtility.UrlEncode(userName));
                userNameCookie.Path = "/";
                var userIdCookie = new CookieHeaderValue("CS%5FSEC%5FUserID", CryptData(password, userName));
                userIdCookie.Path = "/";
                responseMessage.Headers.AddCookies(new CookieHeaderValue[] { ssoCookie, inactivityCookie, userNameCookie, userIdCookie });
            }
            catch (Exception ex)
            {
                message.AppendLine("Login failed!");
                responseMessage = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message.ToString(), ex);
            }

            return await Task.FromResult<IHttpActionResult>(ResponseMessage(responseMessage));
        }

        [HttpGet]
        [Route(Consts.apiPrefix + "auth/logout")]
        [SwaggerOperation("Logout")]
        [SwaggerResponse(200, type: typeof(string))]
        public async Task<IHttpActionResult> Logout()
        {
            var ssoCookie = new CookieHeaderValue(Consts.ssoCookieName, string.Empty);
            ssoCookie.Path = "/";
            ssoCookie.Expires = DateTime.Now.AddMinutes(-25);
            var responseMessage = Request.CreateResponse(HttpStatusCode.OK, "Logged out successfully!");
            responseMessage.Headers.AddCookies(new CookieHeaderValue[] { ssoCookie });
            return await Task.FromResult<IHttpActionResult>(ResponseMessage(responseMessage));
        }

        [HttpGet]
        [Route(Consts.apiPrefix + "auth/validate/{userName}")]
        public HttpResponseMessage Validate(string userName)
        {
            var message = new StringBuilder();
            try
            {
                bool isValid = false;
                const string SSO_URL = "/COESingleSignOn/SingleSignOn.asmx/";
                var ssoCookies = Request.Headers.GetCookies();
                var tokenCookie = ssoCookies == null || ssoCookies.FirstOrDefault() == null ? null : ssoCookies.First().Cookies.FirstOrDefault(c => c.Name == Consts.ssoCookieName);
                var token = tokenCookie == null ? null : tokenCookie.Value;
                if (!string.IsNullOrEmpty(token))
                {
                    message.AppendLine(string.Format("Validating token, {0}...", token));
                    var webClient = new WebClient();
                    var validateUrl = SSO_URL + "ValidateTicket?encryptedTicket=";
                    validateUrl = GetAbsoluteUrl(validateUrl, true);
                    string response;
                    message.AppendLine(string.Format("Opening {0}...", validateUrl));
                    using (var validateResponse = new StreamReader(webClient.OpenRead(validateUrl + token)))
                        response = validateResponse.ReadToEnd();
                    var xml = new XmlDocument();
                    xml.LoadXml(response);
                    isValid = bool.Parse(xml.DocumentElement.FirstChild.InnerText.Trim());
                }

                message.AppendLine(string.Format("Valid: {0}", isValid));
                var responseObject = new JObject(new JProperty("isValid", isValid));

                if (isValid)
                {
                    responseObject.Add(
                        new JProperty("meta",
                            new JObject(
                                new JProperty("token", token),
                                new JProperty("profile", new JObject(new JProperty("fullName", userName)))
                            )
                        )
                    );
                }

                var responseJobject = Request.CreateResponse(HttpStatusCode.OK, responseObject);
                return responseJobject;
            }
            catch (Exception ex)
            {
                message.AppendLine("Validation failed!");
                throw new InvalidOperationException(message.ToString(), ex);
            }
        }

        [Route(Consts.apiPrefix + "auth/renew/{token}")]
        public HttpResponseMessage Renew(string token)
        {
            var message = new StringBuilder();
            try
            {
                var userData = Request.Content.ReadAsAsync<JObject>().Result;
                message.AppendLine(string.Format("Logging in user, {0}...", userData["username"]));
                var webClient = new WebClient();
                var loginUrl = string.Format("/COESingleSignOn/SingleSignOn.asmx/GetAuthenticationTicket?userName={0}&password=", userData["username"]);
                loginUrl = GetAbsoluteUrl(loginUrl, true);
                string newToken;
                message.AppendLine(string.Format("Opening {0}...", loginUrl));
                using (var loginResponse = new StreamReader(webClient.OpenRead(loginUrl + userData["password"])))
                    newToken = loginResponse.ReadToEnd();
                var xml = new XmlDocument();
                xml.LoadXml(newToken);
                newToken = xml.DocumentElement.FirstChild.InnerText.Trim();
                message.AppendLine(string.Format("Token: {0}", newToken));
                var response = Request.CreateResponse(HttpStatusCode.OK, new JObject(
                    new JProperty("data", new JObject(new JProperty("msg", "LOGIN SUCCESSFUL"))),
                    new JProperty("meta",
                        new JObject(
                            new JProperty("token", newToken),
                            new JProperty("profile", new JObject(new JProperty("fullName", userData["username"])))
                        )
                    )
                ));
                var cookie = new CookieHeaderValue(Consts.ssoCookieName, newToken);
                cookie.Expires = DateTime.Now.AddMinutes(60);
                cookie.Domain = Request.RequestUri.Host;
                cookie.Path = "/";
                response.Headers.AddCookies(new CookieHeaderValue[] { cookie });
                return response;
            }
            catch (Exception ex)
            {
                message.AppendLine("Login failed!");
                throw new InvalidOperationException(message.ToString(), ex);
            }
        }
    }
}
