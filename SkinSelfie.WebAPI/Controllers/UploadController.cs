using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ServiceData;
using SkinSelfie.ServiceData.Interfaces;
using SkinSelfie.WebAPI.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SkinSelfie.WebAPI.Controllers
{
    [Authorize]
    public class UploadController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.Photo> _photoRepository;
        private readonly IReadWriteRepository<ServiceData.Models.UserCondition> _conditionRepository;

        public UploadController(IReadWriteRepository<ServiceData.Models.Photo> photoRepository, IReadWriteRepository<ServiceData.Models.UserCondition> conditionRepository)
        {
            _photoRepository = photoRepository;
            _conditionRepository = conditionRepository;
        }

        public static async Task<CloudBlobContainer> GetBlobContainer()
        {
            // Connect to Azure cloud storage, creating uploads container (folder) if necessary
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["Storage"].ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("skinselfies");
            await container.CreateIfNotExistsAsync();
            await container.SetPermissionsAsync(
                new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Off
                });

            return container;
        }

        public static string GetFilePathFromUrl(string url)
        {
            return Regex.Replace(url, ".*skinselfies/", String.Empty, RegexOptions.None);
        }

        public async Task<IList<string>> Put(string filepath)
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                Console.WriteLine(Request.Content.Headers.ContentType.ToString());
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            try
            {
                CloudBlobContainer container = await GetBlobContainer();

                BlobStorageProvider provider = new BlobStorageProvider(container, filepath);
                await Request.Content.ReadAsMultipartAsync(provider);

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "UploadImage");

                return provider.Urls;
            }
            catch(Exception e)
            {
                return new List<string>() { e.Message };
            }
        }

        public async Task<HttpResponseMessage> Get(string imageId, bool thumb = false)
        {
            int id;

            if (string.IsNullOrEmpty(imageId)|| !Int32.TryParse(imageId, out id))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            ServiceData.Models.Photo found = _photoRepository.GetById(id);
            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            ServiceData.Models.UserCondition foundCond = _conditionRepository.GetById(found.UserCondition.Id);
            if (foundCond.Owner.Email != User.Identity.Name) return Request.CreateResponse(HttpStatusCode.Forbidden);

            string target = (thumb) ? found.ThumbUrl : found.Url;

            CloudBlobContainer container = await GetBlobContainer();
            Stream blobStream = new MemoryStream();
            CloudBlob photoBlob = container.GetBlobReference(target.Replace(ConfidentialData.BlobStorageUrl, ""));

            KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(ServerUtils.GetToken);
            IKey rsa = await cloudResolver.ResolveKeyAsync(ConfidentialData.KeyLocation, CancellationToken.None);
            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(null, cloudResolver);
            BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

            await photoBlob.DownloadToStreamAsync(blobStream, null, options, null);
            blobStream.Position = 0;

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(blobStream);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = imageId + Path.GetExtension(target);

            string eventName = thumb ? "DownloadThumb" : "DownloadImage";
            ServerUtils.LogTelemetryEvent(User.Identity.Name, eventName);

            return response;
        } 
    }
}
