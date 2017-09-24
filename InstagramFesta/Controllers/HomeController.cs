using InstaSharp.Models.Responses;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Mvc;
using InstaSharp;
using System.Collections.Generic;
using RestSharp;

namespace InstagramFesta.Controllers
{
    public class HomeController : Controller
    {
        private string clientId = "90e2c431169a4c6ca3166869d1e21d11";
        private string clientSecret = "59f3029811fd4a1788d72d3e9db04ecf ";
        private string accessToken = "204741759.90e2c43.d08709b8aab04860aaae29da923abf23";
        private InstagramConfig _config;
        public InstagramConfig config
        {
            get
            {
                if (_config == null)
                {
                    _config = new InstagramConfig(clientId, clientSecret, "http://localhost:58949/Home/OAuth", string.Empty);
                }
                return _config;
            }
        }

        // GET: Home
        public async Task<ActionResult> Index()
        {

            //var teste = await GetRecentTaggedMediaAsync("#vaiserteta");
            //if (teste != null)
            //{
            //    return View();
            //}
            Test();
            return View();
        }

        public ActionResult Login()
        {
            var scopes = new List<OAuth.Scope>();
            scopes.Add(InstaSharp.OAuth.Scope.Likes);
            scopes.Add(InstaSharp.OAuth.Scope.Comments);

            var link = InstaSharp.OAuth.AuthLink(config.OAuthUri + "authorize", config.ClientId, config.RedirectUri, scopes, InstaSharp.OAuth.ResponseType.Code);

            return Redirect(link);
        }

        public async Task<ActionResult> OAuth(string code)
        {
            // add this code to the auth object
            var auth = new OAuth(config);

            // now we have to call back to instagram and include the code they gave us
            // along with our client secret
            var oauthResponse = await auth.RequestToken(code);

            // both the client secret and the token are considered sensitive data, so we won't be
            // sending them back to the browser. we'll only store them temporarily.  If a user's session times
            // out, they will have to click on the authenticate button again - sorry bout yer luck.
            Session.Add("InstaSharp.AuthInfo", oauthResponse);

            // all done, lets redirect to the home controller which will send some intial data to the app
            return RedirectToAction("Index");
        }

        public async Task<object> LoadInstagramPosts(string hashTagTerm)
        {
            var oAuthResponse = Session["InstaSharp.AuthInfo"] as OAuthResponse;

            if (oAuthResponse == null)
            {
                return RedirectToAction("Login");
            }

            var tagApi = new InstaSharp.Endpoints.Tags(config, oAuthResponse);
            var result =  await tagApi.Get("vaiserteta");

            return result.Data;
        }

        public async Task<ActionResult> MyFeed()
        {
            var oAuthResponse = Session["InstaSharp.AuthInfo"] as OAuthResponse;

            if (oAuthResponse == null)
            {
                return RedirectToAction("Login");
            }

            var users = new InstaSharp.Endpoints.Users(config, oAuthResponse);

            var feed = await users.Search("vaiserteta", null);

            return View(feed.Data);
        }

        public List<object> Test()
        {
            var Client = new RestClient("https://api.instagram.com/");
            var request = new RestRequest("/v1/tags/teste/media/recent?access_token=204741759.90e2c43.d08709b8aab04860aaae29da923abf23", Method.GET);

            var response = Client.Execute<List<object>>(request);

            var teste = JsonConvert.DeserializeObject<List<object>>(response.Content);
            return teste;
        }

        //public async Task<object> GetRecentTaggedMediaAsync(string tag)
        //{
        //    string clientId = "90e2c431169a4c6ca3166869d1e21d11";
        //    string clientSecret = "59f3029811fd4a1788d72d3e9db04ecf ";
        //    string accessToken = "204741759.90e2c43.d08709b8aab04860aaae29da923abf23";

        //    InstagramClient client = new InstagramClient(clientId, clientSecret, accessToken);

        //    var recentTaggedMedia = await client.TagEndpoints.GetRecentTaggedMediaAsync(tag, 10);

        //    //I use Json.NET for parsing the result
        //    var recentTaggedMediaJson = JsonConvert.DeserializeObject(recentTaggedMedia);

        //    //You can deserialize json result to one of the models in InstagramCSharp or to your custom model
        //    //var recentTaggedMediaJson = JsonConvert.DeserializeObject<Envelope<List<Media>>>(recentTaggedMedia);

        //    return recentTaggedMediaJson;
        //}
    }
}