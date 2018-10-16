using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using ServiceData;
using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SkinSelfie.WebAPI.Controllers
{
    [Authorize]
    public class PhotoController : ApiController
    {
        private readonly IReadWriteRepository<ServiceData.Models.Photo> _photoRepository;
        private readonly IReadWriteRepository<ServiceData.Models.UserCondition> _conditionRepository;
        private readonly IReadWriteRepository<ServiceData.Models.User> _userRepository;
        private readonly IReadWriteRepository<ServiceData.Models.Share> _shareRepository;
        private readonly IReadWriteRepository<ServiceData.Models.EventLog> _logRepository;

        public PhotoController(
            IReadWriteRepository<ServiceData.Models.Photo> photoRepository, 
            IReadWriteRepository<ServiceData.Models.UserCondition> conditionRepository, 
            IReadWriteRepository<ServiceData.Models.EventLog> logRepository,
            IReadWriteRepository<ServiceData.Models.Share> shareRepository,
            IReadWriteRepository<ServiceData.Models.User> userRepository)
        {
            _photoRepository = photoRepository;
            _conditionRepository = conditionRepository;
            _userRepository = userRepository;
            _shareRepository = shareRepository;
            _logRepository = logRepository;
        }

        private void PostLog(string eventType, int? userId = null)
        {
            if (userId == null)
            {
                var user = _userRepository.Search(u => u.Email == User.Identity.Name).FirstOrDefault();
                if (user == null) return;
                userId = user.Id;
            }

            _logRepository.Insert(new ServiceData.Models.EventLog
            {
                Action = eventType,
                CreatedAt = DateTime.Now,
                UserId = (int)userId
            });
        }

        private void UpdateShares(ServiceData.Models.Photo photo)
        {
            List<ServiceData.Models.Share> shares = _shareRepository.Search(sh =>
                                                        sh.UserCondition.Id == photo.UserCondition.Id).ToList();
            foreach(var sh in shares)
            {
                if(!sh.Updated)
                {
                    sh.Updated = true;
                    _shareRepository.Update(sh);
                }
            }
        }

        public async Task<HttpResponseMessage> Post([FromBody]Models.Photo newPhoto)
        {
            try
            {
                ServiceData.Models.Photo returned = _photoRepository.Insert(Models.Photo.ToServiceModel(newPhoto, true));
                UpdateShares(returned);

                ServerUtils.LogTelemetryEvent(User.Identity.Name, "AddPhoto");

                PostLog("Photos_Create");

                return Request.CreateResponse(HttpStatusCode.OK, Models.Photo.ToAppModel(returned, false));
            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        struct AboutUser
        {
            public int Id;
            public string Name;
            public string Email;
            public DateTime Dob;
        }

        struct AboutCondition
        {
            public int Id;
            public string Name;
            public string SkinRegion;
            public int NumPhotos;
            public DateTime StartDate;
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

        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                string[] testUsers = new string[]
                {
                    "n.correia@ncl.ac.uk",
                    "nataliejordandickens@gmail.com",
                    "j.atkinson7@newcastle.ac.uk",
                    "j.baldwin1@newcastle.ac.uk",
                    "l.pearson5@newcastle.ac.uk",
                    "c.w.mak1@ncl.ac.uk",
                    "lizlucyrobinson@gmail.com",
                    "a.j.drummond@newcastle.ac.uk",
                    "hannah.janes@hotmail.co.uk"
                };

                string baseFileLoc = "C:/Users/tgs03_000/Downloads/SkinSelfies";

                //if (Directory.Exists(baseFileLoc))
                //{
                //    Directory.Delete(baseFileLoc, true);
                //}

                if (!Directory.Exists(baseFileLoc))
                {
                    Directory.CreateDirectory(baseFileLoc);
                }

                List<Task> allTasks = new List<Task>();
                List<string> nullEmails = new List<string>();

                foreach(string userEmail in testUsers)
                {
                    ServiceData.Models.User user = _userRepository.Search(u => u.Email == userEmail).FirstOrDefault();

                    DirectoryInfo userDir = Directory.CreateDirectory(Path.Combine(baseFileLoc, userEmail));

                    if(user == null)
                    {
                        nullEmails.Add(userEmail);
                        continue;
                    }

                    AboutUser userDetails = new AboutUser
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Name = user.Name,
                        Dob = user.BirthDate
                    };

                    File.WriteAllText(Path.Combine(userDir.FullName, "AboutUser.txt"), JsonConvert.SerializeObject(userDetails, Formatting.Indented));

                    foreach (ServiceData.Models.UserCondition cond in user.Conditions)
                    {
                        ServiceData.Models.UserCondition fullCond = _conditionRepository.GetById(cond.Id);

                        string condPath = Path.Combine(userDir.FullName, cond.Id.ToString());

                        DirectoryInfo condDir = Directory.Exists(condPath)? new DirectoryInfo(condPath) : Directory.CreateDirectory(condPath);

                        AboutCondition condDetails = new AboutCondition
                        {
                            Id = user.Id,
                            Name = cond.Condition,
                            SkinRegion = fullCond.SkinRegion.BodyPart.Name + " - " + fullCond.SkinRegion.Name,
                            StartDate = cond.StartDate,
                            NumPhotos = fullCond.Photos.Count()
                        };

                        File.WriteAllText(Path.Combine(condDir.FullName, "AboutCondition.txt"), JsonConvert.SerializeObject(condDetails, Formatting.Indented));

                        foreach (ServiceData.Models.Photo photo in fullCond.Photos)
                        {
                            string filename = Path.Combine(condDir.FullName, photo.CreatedAt.ToString("yyyy-MM-dd-HH-mm-ss.") + Path.GetExtension(photo.Url));

                            if(File.Exists(filename))
                            {
                                continue;
                            }

                            CloudBlobContainer container = await GetBlobContainer();
                            Stream blobStream = new MemoryStream();
                            CloudBlob photoBlob = container.GetBlobReference(photo.Url.Replace(ConfidentialData.BlobStorageUrl, ""));

                            KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(ServerUtils.GetToken);
                            IKey rsa = await cloudResolver.ResolveKeyAsync(ConfidentialData.KeyLocation, CancellationToken.None);
                            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(null, cloudResolver);
                            BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

                            await photoBlob.DownloadToStreamAsync(blobStream, null, options, null);
                            blobStream.Position = 0;

                            using (var fileStream = File.Create(filename))
                            {
                                await blobStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }

                string nullString = "";

                foreach(string nEmail in nullEmails)
                {
                    nullString += nEmail + ", ";
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Files located at " + baseFileLoc + " Null emails: " + nullString);
            }
            catch(Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
            }
        }

        public HttpResponseMessage Get(int id)
        {
            ServiceData.Models.Photo found = _photoRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!IsSameUser(found)) return Request.CreateResponse(HttpStatusCode.Forbidden);

            Models.Photo toRet = Models.Photo.ToAppModel(found, false);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "GetPhoto");
            PostLog("Photos_GetSingle");
            return Request.CreateResponse(HttpStatusCode.OK, toRet);
        }

        public async Task<HttpResponseMessage> Delete(int id)
        {
            ServiceData.Models.Photo found = _photoRepository.GetById(id);

            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);
            if (!IsSameUser(found)) return Request.CreateResponse(HttpStatusCode.Forbidden);

            await Delete(_photoRepository, id);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "DeletePhoto");
            PostLog("Photos_Delete");
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private bool IsSameUser(ServiceData.Models.Photo foundPhoto)
        {
            ServiceData.Models.UserCondition foundCond = _conditionRepository.GetById(foundPhoto.UserCondition.Id);

            return (foundCond.Owner.Email == User.Identity.Name);
        }

        public HttpResponseMessage Put(int id, [FromBody]Models.Photo updatedPhoto)
        {
            updatedPhoto.Id = id;

            ServiceData.Models.Photo found = _photoRepository.GetById(id);
            if (found == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            if (!IsSameUser(found)) return Request.CreateResponse(HttpStatusCode.Forbidden);

            ServiceData.Models.Photo final = _photoRepository.Update(Models.Photo.ToServiceModel(updatedPhoto, true));

            if (final == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            UpdateShares(final);

            ServerUtils.LogTelemetryEvent(User.Identity.Name, "UpdatePhoto");
            PostLog("Photos_Update");
            return Request.CreateResponse(HttpStatusCode.OK, Models.Photo.ToAppModel(final, true));
        }

        public static async Task Delete(IReadWriteRepository<ServiceData.Models.Photo> photoRep, int id)
        {
            ServiceData.Models.Photo found = photoRep.GetById(id);

            CloudBlobContainer container = await UploadController.GetBlobContainer();

            try
            {
                string url = UploadController.GetFilePathFromUrl(found.Url);
                var mainBlob = container.GetBlockBlobReference(url);
                mainBlob.Delete();
            }
            catch { }

            try
            {
                string thumbUrl = UploadController.GetFilePathFromUrl(found.ThumbUrl);
                var thumbBlob = container.GetBlockBlobReference(thumbUrl);
                thumbBlob.Delete();
            }
            catch { }

            await photoRep.Delete(id);
        }
    }
}
