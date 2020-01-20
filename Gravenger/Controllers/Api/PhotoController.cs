using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Security.Extensions;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public partial class PhotoController : ApiController
    {
        private readonly CloudBlobClient blobClient;
        private const string PhotoStorageContainerName = "photos";
        private IUnitOfWork _unitOfWork;

        public PhotoController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            this.blobClient = storageAccount.CreateCloudBlobClient();
            this._unitOfWork = unitOfWork;
        }

        //TODO: determine if uploading a photo using multipart content is necessary and remove this method if it is not
        //[HttpPost]
        //public async Task<ImageDTO> Post()
        //{
        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    var blobStoragePhotoContainer = this.GetBlobStorageContainer("photos");
        //    var provider = new AzureStorageMultipartFormDataStreamProvider(blobStoragePhotoContainer);

        //    await Request.Content.ReadAsMultipartAsync<MultipartFormDataStreamProvider>(provider);

        //    return provider.FileData.Select(fileData =>
        //    {
        //        return new ImageDTO
        //        {
        //            FileName = System.IO.Path.GetFileName(new Uri(fileData.LocalFileName).LocalPath),
        //            FullPath = fileData.LocalFileName
        //        };
        //    }).FirstOrDefault();
        //}   

        [HttpPost]
        public IHttpActionResult Post(ImageBase64DTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload cannot be null");
            }

            if (dto.Base64 == null || dto.MimeType == null)
            {
                return this.BadRequest("Image base64 string and image mime type must not be null");
            }

            //TODO: do additional validation check to securely validate appropriate file

            var fileExtension = this.GetFileExtensionFromMimeType(dto.MimeType);
            var folderName = $"{this.User.Identity.GetAccountID()}";
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var blobContainer = this.GetBlobStorageContainer("photos");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference($"{folderName}/{fileName}");

            if (dto.MimeType != null)
            {
                blob.Properties.ContentType = dto.MimeType;
            }

            //TODO: Clean up how base64 string is parsed.  Should it be parsed before or after sent to api?
            byte[] imageBytes = Convert.FromBase64String(dto.Base64.Split(',')[1]);
            using (var stream = new System.IO.MemoryStream(imageBytes, writable: false))
            {
                blob.UploadFromStream(stream);
            }

            return this.Ok(new ImageDTO { FileName = fileName, FullPath = blob.Uri.ToString() });
        }

        [HttpDelete]
        public IHttpActionResult Delete(DeleteImageDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload cannot be null");
            }

            if (dto.FileName == null)
            {
                return this.BadRequest("Image file name cannot be null");
            }

            //TODO: Generally speaking, should AccountTile records be removed before actually removing photos from azure blob storage?  
            //      If a photo is removed, then removing the AccountTile record fails, the AccountTile record's image no longer exists.
            //      If a tile is removed first, and the photo deletion fails, then you simply have an orphaned photo in azure storage.
            //      If tile is removed first, the below check will no longer work.
            var currentAccountID = this.User.Identity.GetAccountID();
            var currentAccount = this._unitOfWork.Accounts.Get(currentAccountID);
            var isProfilePhotoFoundInDatabase = dto.FileName == currentAccount.ProfileImageFileName;
            var isFileFromTileFoundInDatabase = this._unitOfWork.AccountTiles
                .Find(t => t.ImageFileName == dto.FileName && t.AccountID == currentAccountID).Any();
            if (!isFileFromTileFoundInDatabase && !isProfilePhotoFoundInDatabase)
            {
                return this.BadRequest("File not found");
            }

            var blobContainer = this.GetBlobStorageContainer("photos");
            var folderName = $"{currentAccountID}";
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference($"{folderName}/{dto.FileName}");
            var fileDeleted = blob.DeleteIfExists();

            if (!fileDeleted)
            {
                return this.BadRequest("An occurred attempted to remove the photo");
            }

            return this.Ok();
        }

        private CloudBlobContainer GetBlobStorageContainer(string containerName)
        {
            CloudBlobContainer container = this.blobClient.GetContainerReference(containerName);
            container.CreateIfNotExists();
            return container;
        }
        
        private string GetFileExtensionFromMimeType(string mimeType)
        {
            switch (mimeType)
            {
                case "image/png":
                    return ".png";
                case "image/jpeg":
                    return ".jpg";
                default:
                    return null;
            }
        }
    }
}
