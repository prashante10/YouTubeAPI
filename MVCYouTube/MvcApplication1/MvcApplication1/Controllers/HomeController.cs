using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Web;
using System.Web.Mvc;
using Google.Apis.Auth.OAuth2.Mvc;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;

namespace MvcApplication1.Controllers
{
    public class HomeResultViewModel
    {
        public bool Connected { get; set; }
    }
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            // YOUR CODE SHOULD BE HERE..
            return View(new HomeResultViewModel { Connected = System.IO.File.Exists("D://json.txt") });

        }


        public async Task<ActionResult> upload()
        {
            var youtubeService = await GetYouTubeService();

            var channels = youtubeService.Channels.List("");
            var video = new Video();
            // video.Snippet.
            video.Snippet = new VideoSnippet();
            video.Snippet.ChannelId = channels.Id;
            video.Snippet.Title = "Monica Video";
            video.Snippet.Description = "Monica Video Description";
            video.Snippet.Tags = new string[] { "monica", "vidzapper", "The Assetry" };
            video.Snippet.CategoryId = "22"; // See https://developers.google.com/youtube/v3/docs/videoCategories/list
            video.Status = new VideoStatus();
            video.Status.PrivacyStatus = "unlisted"; // or "private" or "public"
            var filePath = Server.MapPath("~/App_Data/monica.mp4");// @"REPLACE_ME.mp4"; // Replace with path to actual movie file.

            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, "video/*");
                videosInsertRequest.ProgressChanged += videosInsertRequest_ProgressChanged;
                videosInsertRequest.ResponseReceived += videosInsertRequest_ResponseReceived;

                var tmp = await videosInsertRequest.UploadAsync();
                Console.Write(tmp.BytesSent);
            }

            return View();
        }

        void videosInsertRequest_ProgressChanged(Google.Apis.Upload.IUploadProgress progress)
        {
            switch (progress.Status)
            {
                case UploadStatus.Uploading:
                    Console.WriteLine("{0} bytes sent.", progress.BytesSent);
                    break;

                case UploadStatus.Failed:
                    Console.WriteLine("An error prevented the upload from completing.\n{0}", progress.Exception);
                    break;
            }
        }

        void videosInsertRequest_ResponseReceived(Video video)
        {
            Console.WriteLine("Video id '{0}' was successfully uploaded.", video.Id);
        }

        protected IAuthorizationCodeFlow Flow
        {
            get {
                return new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).Flow;
            }
        }


        public async Task<YouTubeService> GetYouTubeService()
        {
            string tokenstring = System.IO.File.ReadAllText("D://json.txt");

            TokenResponse tokenResult = (TokenResponse)new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(tokenstring, typeof(TokenResponse));
            var tk = await Flow.RefreshTokenAsync(new AppFlowMetadata().GetUserId(this), tokenResult.RefreshToken, new CancellationToken(false));

            string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(tk);
            System.IO.File.WriteAllText("D://json.txt", json);

            return new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = new UserCredential(Flow,Guid.NewGuid().ToString(), tk),
                ApplicationName = "VidZapper"
            });
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> Connect()
        {
            if (!System.IO.File.Exists("D://json.txt"))
            {
                var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata()).AuthorizeAsync(new CancellationToken());
                if (result.Credential == null) return Redirect(result.RedirectUri + "&approval_prompt=force");
            }
            return RedirectToAction("Index", "Home");
        }

    }
}
