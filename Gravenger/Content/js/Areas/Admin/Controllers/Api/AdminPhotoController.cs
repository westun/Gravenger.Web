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
using System.Threading.Tasks;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Manage Account Photos")]
    public partial class AdminPhotoController : ApiController
    {
        private readonly CloudBlobClient blobClient;
        private const string PhotoStorageContainerName = "photos";
        private IUnitOfWork _unitOfWork;

        public AdminPhotoController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            this.blobClient = storageAccount.CreateCloudBlobClient();
            this._unitOfWork = unitOfWork;
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

            var blobContainer = this.GetBlobStorageContainer("photos");
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(dto.FileName);
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
    }
}
