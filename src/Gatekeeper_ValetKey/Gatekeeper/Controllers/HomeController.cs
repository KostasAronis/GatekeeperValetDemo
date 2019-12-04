using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Gatekeeper.Controllers
{
    public class HomeController : Controller
    {
        private Dictionary<string, string> credentialDict = new Dictionary<string, string>();
        public ActionResult Index()
        {
            ViewBag.Title = "Gatekeeper";

            return View();
        }

        public async Task<string> AddBlob(string text)
        {
            var tokenServiceEndpoint = ConfigurationManager.AppSettings["newBlobEndpointUrl"];
            try
            {
                // Get the uri to the data managing service
                var uri = new Uri(tokenServiceEndpoint);
                var request = HttpWebRequest.Create(uri);
                // http request between the services provides a safety layer
                var response = await request.GetResponseAsync();
                var responseString = string.Empty;
                var serializer = new DataContractJsonSerializer(typeof(StorageEntitySas));
                var blobSas = (StorageEntitySas)serializer.ReadObject(response.GetResponseStream());
                // create storage credentials object based on SAS
                var credentials = new StorageCredentials(blobSas.Credentials);
                // using the returned SAS credentials and BLOB Uri create a block blob instance to upload
                var blob = new CloudBlockBlob(blobSas.BlobUri, credentials);
                await blob.UploadTextAsync(text);
                Console.WriteLine("Blob upload successful: {0}", blobSas.Name);
                return blob.Name;
            }
            catch
            {
                throw;
            }
        }
        public async Task<string> GetBlob(string id)
        {
            var tokenServiceEndpoint = ConfigurationManager.AppSettings["readBlobEndpointUrl"];
            var uri = new Uri(tokenServiceEndpoint+id);
            WebClient webClient = new WebClient();
            var res = webClient.DownloadString(uri);
            return res;
        }


        public async Task<string> AddNewBlob(string text)
        {
            var tokenServiceEndpoint = ConfigurationManager.AppSettings["newBlobEndpointUrl"];
            try
            {
                var blobSas = await GetBlobSas(new Uri(tokenServiceEndpoint));
                // Create storage credentials object based on SAS
                var credentials = new StorageCredentials(blobSas.Credentials);
                // Using the returned SAS credentials and BLOB Uri create a block blob instance to upload
                var blob = new CloudBlockBlob(blobSas.BlobUri, credentials);
                await blob.UploadTextAsync(text);
                Console.WriteLine("Blob upload successful: {0}", blobSas.Name);
                return blob.Name;
            }
            catch
            {
                throw;
            }
        }
        private static async Task<StorageEntitySas> GetBlobSas(Uri blobUri)
        {
            var request = HttpWebRequest.Create(blobUri);
            var response = await request.GetResponseAsync();
            var responseString = string.Empty;
            var serializer = new DataContractJsonSerializer(typeof(StorageEntitySas));
            var blobSas = (StorageEntitySas)serializer.ReadObject(response.GetResponseStream());
            return blobSas;
        }

        public struct StorageEntitySas
        {
            public string Credentials;
            public Uri BlobUri;
            public string Name;
        }
    }
}
