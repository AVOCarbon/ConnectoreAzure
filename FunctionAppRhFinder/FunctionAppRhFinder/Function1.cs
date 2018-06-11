
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Web;
using System.Net.Http;
using DotNetOpenAuth.OAuth2;
using System;
using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace FunctionApp1
{

    public static class Function1
    {
        private static WebServerClient _webServerClient;
        private static string _accessToken;
        // Client ID (as obtained from Azure AD portal)
        private static string clientId = "a4441ffb-d987-4d05-948b-4d120ca8a21b";
        // Client Secret (as obtained from Azure AD portal)
        private static string appKey = "mdsDO82IuZDeMdhKCo6Qa/2BPSMppV9PsYyPITfrFHo=";
        private static string aadInstance = "https://login.microsoftonline.com/";
        private static string tenant = "AVOCarbon";
        private static string authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        // Azure AD resource I am trying to access
        public static string serviceResourceId;
        [FunctionName("Function1")]
        public static async System.Threading.Tasks.Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];


            /*string url = "https://hrfinder.azurewebsites.net/.auth/login/aad/callback";
            string client_id = "a4441ffb-d987-4d05-948b-4d120ca8a21b";
            string client_secret = "mdsDO82IuZDeMdhKCo6Qa/2BPSMppV9PsYyPITfrFHo=";*/

            InitializeWebServerClient();
            RequestToken();



            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            var client = new HttpClient();

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["api-version"] = "1.6";

            serviceResourceId = "https://graph.windows.net/users/" + name + "?" + queryString;
            //log.Info(token);
            var response = await client.GetAsync(serviceResourceId);


            var responseString = await response.Content.ReadAsStringAsync();

            return name != null
                ? (ActionResult)new OkObjectResult($"{responseString}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        private static void InitializeWebServerClient()
        {
            var authorizationServer = new AuthorizationServerDescription
            {
                AuthorizationEndpoint = new Uri("https://login.windows.net/e27023d0-bf6d-47f1-94be-a75cae8debcd/oauth2/authorize"/* WHAT TO PUT HERE */),
                TokenEndpoint = new Uri("https://login.windows.net/e27023d0-bf6d-47f1-94be-a75cae8debcd/oauth2/token"/* WHAT TO PUT HERE */)
            };
            _webServerClient = new WebServerClient(authorizationServer, clientId, appKey);
        }



        private static void RequestToken()
        {
            var state = _webServerClient.GetClientAccessToken();
            _accessToken = state.AccessToken;
        }
    }
}