using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Core;
using System.Threading;
using ServiceData;

namespace SkinSelfie.WebAPI.Providers
{
    public class BlobStorageProvider : MultipartFileStreamProvider
    {
        private readonly CloudBlobContainer _container;
        public IList<string> Urls { get; private set; }
        public string currentFilePath;

        public BlobStorageProvider(CloudBlobContainer container, string filepath)
            : base(Path.GetTempPath())
        {
            _container = container;
            currentFilePath = filepath;
            Urls = new List<string>();
        }

        public async override Task ExecutePostProcessingAsync()
        {
            KeyVaultKeyResolver cloudResolver = new KeyVaultKeyResolver(ServerUtils.GetToken);
            IKey rsa = await cloudResolver.ResolveKeyAsync(ConfidentialData.KeyLocation, CancellationToken.None);

            foreach (var file in FileData)
            {
                string fileName = Path.GetFileName(file.Headers.ContentDisposition.FileName.Trim('"'));
                var blob = _container.GetBlockBlobReference(Path.Combine(currentFilePath, fileName));

                BlobEncryptionPolicy policy = new BlobEncryptionPolicy(rsa, null);
                BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

                using (var stream = File.OpenRead(file.LocalFileName))
                {
                    await blob.UploadFromStreamAsync(stream, stream.Length, null, options, null);
                }

                Urls.Add(blob.Uri.AbsoluteUri);

                string ext = Path.GetExtension(fileName);

                if (ext == ".jpeg" || ext == ".jpg")
                {
                    Image original = Image.FromFile(file.LocalFileName);
                    float ratio = (float)original.Width / (float)original.Height;
                    SizeF newSize = new SizeF(350 * ratio, 350);

                    Image thumb = original.GetThumbnailImage((int)newSize.Width, (int)newSize.Height, null, IntPtr.Zero);

                    string thumbName = Path.GetFileNameWithoutExtension(fileName) + "_thumb.jpg";

                    var thumbBlob = _container.GetBlockBlobReference(Path.Combine(currentFilePath, thumbName));

                    MemoryStream thumbStream = new MemoryStream();

                    thumb.Save(thumbStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    thumbStream.Position = 0;

                    await thumbBlob.UploadFromStreamAsync(thumbStream, thumbStream.Length, null, options, null);

                    Urls.Add(thumbBlob.Uri.AbsoluteUri);
                }

                try
                {
                    File.Delete(file.LocalFileName);
                }
                catch (Exception e)
                { }
            }
        }
    }
}