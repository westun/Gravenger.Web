using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Gravenger.Azure
{
    public class ActivityFeedEntity : TableEntity
    {
        public ActivityFeedEntity(int accountID)
        {
            this.PartitionKey = accountID.ToString();
            this.RowKey = Guid.NewGuid().ToString();
        }

        public ActivityFeedEntity() { }

        public string FeedItemType { get; set; }
        public int ActorAccountID { get; set; }
        public string ActorUserName { get; set; }
        public string ActorProfileImageUrl { get; set; }
        public int FolloweeAccountID { get; set; }
        public string FolloweeUserName { get; set; }
        public string FolloweeProfileImageUrl { get; set; }
        public int FollowerAccountID { get; set; }
        public string FollowerUserName { get; set; }
        public string FollowerProfileImageUrl { get; set; }
        public int CardID { get; set; }
        public string CardTitle { get; set; }
    }
}
