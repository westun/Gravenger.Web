using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Gravenger.Extensions
{
    public class AzureStorageMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private readonly CloudBlobContainer _blobContainer;
        private readonly string[] _supportedMimeTypes = { "image/png", "image/jpeg", "image/jpg", "image/bmp" };

        public AzureStorageMultipartFormDataStreamProvider(CloudBlobContainer blobContainer) : base("azure")
        {
            _blobContainer = blobContainer;
        }

        public override Stream GetStream(HttpContent httpContent, HttpContentHeaders headers)
        {
            if (httpContent == null) throw new ArgumentNullException(nameof(httpContent));
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            if (!_supportedMimeTypes.Contains(headers.ContentType.ToString().ToLower()))
            {
                throw new NotSupportedException("Only jpeg, png, or bitmap images are supported");
            }

            var fileExtension = Path.GetExtension(headers.ContentDisposition.FileName.Replace("\"", string.Empty));
            var fileName = $"{Guid.NewGuid()}{fileExtension}";

            CloudBlockBlob blob = _blobContainer.GetBlockBlobReference(fileName);

            if (headers.ContentType != null)
            {
                blob.Properties.ContentType = headers.ContentType.MediaType;
            }
            
            this.FileData.Add(new MultipartFileData(headers, blob.Uri.ToString()));

            return blob.OpenWrite();
        }
    }
}