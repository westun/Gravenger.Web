using AutoMapper;
using Gravenger.Azure;
using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Security.Extensions;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/feed/v2")]
    public class FeedVer2Controller : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CloudTable _activityFeedCloudTable;
        private const string StorageConnectionStringV2 = "StorageConnectionStringV2";
        private const string ActivityFeedTableName = "ActivityFeed";

        public FeedVer2Controller(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;

            var storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(StorageConnectionStringV2));

            var tableClient = storageAccount.CreateCloudTableClient();
            this._activityFeedCloudTable = tableClient.GetTableReference(ActivityFeedTableName);
        }
                
        [HttpGet]
        [Route("items")]
        public IHttpActionResult Items()
        {
            int currentAccountID = this.User.Identity.GetAccountID();

            TableQuery<ActivityFeedEntity> query = new TableQuery<ActivityFeedEntity>()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, currentAccountID.ToString()));

            var results = this._activityFeedCloudTable.ExecuteQuery(query);

            var feedItemDTOs = results
                .OrderByDescending(r => r.Timestamp)
                .Select(Mapper.Map<FeedItemDTOVer2>);

            return Ok(feedItemDTOs);
        }
    }
}
