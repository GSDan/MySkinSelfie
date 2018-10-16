using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using Microsoft.WindowsAzure.Storage.Blob;
using ServiceData;
using SkinSelfie.Repository;
using SkinSelfie.ServiceData.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SkinSelfie.WebAPI.Controllers.Site
{
    [Authorize]
    public class ConditionsController : SiteBaseController
    {
        private bool IsSharedOrOwned(ServiceData.Models.UserCondition cond)
        {
            if (cond.Owner.Email == User.Identity.Name) return true;
            IReadWriteRepository<ServiceData.Models.Share> _shareRepository = new ShareRepository();
            return _shareRepository.Search(s => s.UserCondition.Id == cond.Id &&
                                                        s.SharedEmail == User.Identity.Name &&
                                                        s.ExpireDate > DateTime.UtcNow).Any();
        }

        // GET: Conditions
        public async Task<ActionResult> Index(int id)
        {
            await LoadViewBag();

            IReadWriteRepository<ServiceData.Models.UserCondition> _condRepository = new UserConditionsRepository();
            ServiceData.Models.UserCondition found = _condRepository.GetById(id);
            
            if(found == null)
            {
                return new HttpNotFoundResult();
            }

            IReadWriteRepository<ServiceData.Models.Share> _shareRepository = new ShareRepository();

            ServiceData.Models.Share sh = _shareRepository.Search(s => s.UserCondition.Id == id &&
                                                        s.SharedEmail == User.Identity.Name &&
                                                        s.ExpireDate > DateTime.UtcNow).FirstOrDefault();

            if (found.Owner.Email != User.Identity.Name && sh == null)
            {
                return new HttpUnauthorizedResult();
            }

            // Has been shared with the user (potentially themself but meh)
            if(sh != null)
            {
                sh.Updated = false;
                _shareRepository.Update(sh);

                ViewData["Title"] = string.Format("{0}'s {1}", found.Owner.Name, found.Condition);
            }
            else
            {
                ViewData["Title"] = found.Condition;
            }

            Models.UserCondition cond = Models.UserCondition.ToAppModel(found, true);

            ViewData["Condition"] = cond;

            return View(cond.Photos);
        }

        [HttpGet]
        public async Task<ActionResult> Download(string imageId, bool thumb = false)
        {
            int id;

            if (string.IsNullOrEmpty(imageId) || !Int32.TryParse(imageId, out id))
            {
                return new HttpUnauthorizedResult();
            }

            IReadWriteRepository<ServiceData.Models.Photo> _photoRepository = new PhotoRepository();
            IReadWriteRepository<ServiceData.Models.UserCondition> _condRepository = new UserConditionsRepository();

            ServiceData.Models.Photo found = _photoRepository.GetById(id);
            if (found == null) return new HttpNotFoundResult();

            ServiceData.Models.UserCondition foundCond = _condRepository.GetById(found.UserCondition.Id);
            if (!IsSharedOrOwned(foundCond)) return new HttpUnauthorizedResult();

            string target = (thumb) ? found.ThumbUrl : found.Url;

            CloudBlobContainer container = await UploadController.GetBlobContainer();
            Stream blobStream = new MemoryStream();
            CloudBlob photoBlob = container.GetBlobReference(target.Replace(ConfidentialData.BlobStorageUrl, ""));

            KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(ServerUtils.GetToken);
            IKey rsa = await cloudResolver.ResolveKeyAsync(ConfidentialData.KeyLocation, CancellationToken.None);
            BlobEncryptionPolicy policy = new BlobEncryptionPolicy(null, cloudResolver);
            BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

            await photoBlob.DownloadToStreamAsync(blobStream, null, options, null);
            blobStream.Position = 0;

            return File(blobStream, "image/jpeg");
        }
    }
}