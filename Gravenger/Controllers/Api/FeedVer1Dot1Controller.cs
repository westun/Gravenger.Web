using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/feed/v1.1")]
    public class FeedVer1Dot1Controller : ApiController
    {
        private const int MaxReturnedFeedItems = 300;
        private readonly IUnitOfWork _unitOfWork;

        public FeedVer1Dot1Controller(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }
                
        [HttpGet]
        [Route("items")]
        public IHttpActionResult Items()
        {
            int currentAccountID = this.User.Identity.GetAccountID();
            var currentAccount = this._unitOfWork.Accounts.GetWithPostcardsAndFollowing(currentAccountID);

            var followings = this._unitOfWork.Followings.GetFolloweesWithAll(currentAccountID);

            var allFeedItems = this.GetAllFeedItemDTOs(followings, currentAccount)
                .OrderByDescending(fi => fi.DateTimeStamp)
                .Take(MaxReturnedFeedItems) //Temporary solution to not having an overloaded amount of data/images the user is required to download to view the feed
                .ToList();

            return Ok(allFeedItems);
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetAllFeedItemDTOs(IEnumerable<Following> followings, Account currentAccount)
        {
            var feedItems = new List<FeedItemDTOVer1Dot1>();

            feedItems.AddRange(this.GetFolloweeAddedTileFeedItemDTOs(followings));
            feedItems.AddRange(this.GetYouFollowedFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFollowedYouFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeFollowedFeedItemDTOs(followings));
            //feedItems.AddRange(this.GetFolloweeJoinedFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeAddedCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeCompletedCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeLikedCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeLikedYourCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetYouAddedTileFeedItemDTOs(currentAccount.Postcards));
            feedItems.AddRange(this.YouCompletedCardFeedItemDTOs(currentAccount.Postcards));

            return feedItems;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeAddedTileFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followee = following.Followee;
                feedItemDTOList.AddRange(following.Followee.AccountTiles.Select(at => new FeedItemDTOVer1Dot1
                {
                    FeedItemType = FeedItemType.FolloweeAddedTile.ToString(),
                    FolloweeUserName = followee.Username,
                    FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                    CardID = at.Postcard.Card.CardID,
                    CardTitle = at.Postcard.Card.Title,
                    TileTitle = at.Tile.Title,
                    ImageUrls = new string[] { at.ImageFullPath },
                    DateTimeStamp = at.CreatedDate,
                }));
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetYouFollowedFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followee = following.Followee;
                feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                {
                    FeedItemType = FeedItemType.YouFollowed.ToString(),
                    FolloweeUserName = followee.Username,
                    FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                    DateTimeStamp = following.CreatedDate,
                });
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFollowedYouFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            var currentAccountID = this.User.Identity.GetAccountID();
            foreach (var following in followings)
            {
                var yourFolloweesFollowings = following.Followee.Followees;
                foreach (var followeeFollowing in yourFolloweesFollowings.Where(f => f.FolloweeID == currentAccountID)) //when the current account is the one be followed (current account is followee)
                {
                    var followee = followeeFollowing.Followee;
                    var follower = followeeFollowing.Follower;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FollowedYou.ToString(),
                        FollowerUserName = follower.Username,
                        FollowerProfileImageUrl = follower.ProfileImageFullPath,
                        DateTimeStamp = followeeFollowing.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeFollowedFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            var currentAccountID = this.User.Identity.GetAccountID();
            foreach (var following in followings)
            {
                var yourFolloweesFollowings = following.Followee.Followees;
                foreach (var followeeFollowing in yourFolloweesFollowings.Where(f => f.FolloweeID != currentAccountID && f.FollowerID != currentAccountID)) //do not include current logged in account
                {
                    var followee = followeeFollowing.Followee;
                    var follower = followeeFollowing.Follower;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FolloweeFollowed.ToString(),
                        FolloweeUserName = followee.Username,
                        FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                        FollowerUserName = follower.Username,
                        FollowerProfileImageUrl = follower.ProfileImageFullPath,
                        DateTimeStamp = followeeFollowing.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        //private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeJoinedFeedItemDTOs(IEnumerable<Following> followings)
        //{
        //    var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
        //    foreach (var following in followings)
        //    {
        //        feedItemDTOList.Add(new FeedItemDTOVer1Dot1
        //        {
        //            FeedItemType = FeedItemType.UserJoined.ToString(),
        //            //Message = $"{following.Followee.Username} joined Seenry",
        //            //ImageUrls = new string[] { at.ImageFullPath },
        //            DateTimeStamp = following.Followee.CreatedDate,
        //        });
        //    }
        //    return feedItemDTOList;
        //}

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeAddedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followee = following.Followee;
                var followeePostcards = following.Followee.Postcards;
                foreach (var postcard in followeePostcards)
                {
                    var card = postcard.Card;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FolloweeAddedCard.ToString(),
                        FolloweeUserName = followee.Username,
                        FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                        CardID = card.CardID,
                        CardTitle = card.Title,
                        DateTimeStamp = postcard.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeCompletedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followee = following.Followee;
                var followeePostcards = following.Followee.Postcards.Where(p => p.CompletedDate.HasValue);
                foreach (var postcard in followeePostcards.Where(pc => pc.IsMarkedCompleted))
                {
                    var card = postcard.Card;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FolloweeCompletedCard.ToString(),
                        FolloweeUserName = followee.Username,
                        FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                        CardID = card.CardID,
                        CardTitle = card.Title,
                        DateTimeStamp = postcard.CompletedDate.GetValueOrDefault(),
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeLikedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var currentAccountID = this.User.Identity.GetAccountID();
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followeeLikedPostcards = following.Followee.PostcardLikes.Select(pcl => pcl.Postcard);
                var followee = following.Followee;
                var follower = following.Follower;
                foreach (var postcard in followeeLikedPostcards.Where(p => p.AccountID != currentAccountID)) //not current users card
                {
                    var card = postcard.Card;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FolloweeLikedCard.ToString(),
                        FolloweeUserName = followee.Username,
                        FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                        FollowerUserName = postcard.Account.Username,
                        FollowerProfileImageUrl = postcard.Account.ProfileImageFullPath,
                        CardID = card.CardID,
                        CardTitle = card.Title,
                        DateTimeStamp = following.Followee.PostcardLikes
                            .Where(pcl => pcl.PostcardID == postcard.PostcardID)
                            .FirstOrDefault().CreatedDate
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetFolloweeLikedYourCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var currentAccountID = this.User.Identity.GetAccountID();
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var following in followings)
            {
                var followeeLikedPostcards = following.Followee.PostcardLikes.Select(pcl => pcl.Postcard);
                var followee = following.Followee;
                var follower = following.Follower;
                foreach (var postcard in followeeLikedPostcards.Where(p => p.AccountID == currentAccountID)) //not current users card
                {
                    var card = postcard.Card;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.FolloweeLikedYourCard.ToString(),
                        FolloweeUserName = followee.Username,
                        FolloweeProfileImageUrl = followee.ProfileImageFullPath,
                        FollowerUserName = postcard.Account.Username,
                        FollowerProfileImageUrl = postcard.Account.ProfileImageFullPath,
                        CardID = card.CardID,
                        CardTitle = card.Title,
                        DateTimeStamp = following.Followee.PostcardLikes
                            .Where(pcl => pcl.PostcardID == postcard.PostcardID)
                            .FirstOrDefault().CreatedDate
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> GetYouAddedTileFeedItemDTOs(IEnumerable<Postcard> postcards)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var postcard in postcards)
            {
                var card = postcard.Card;
                foreach (var accountTile in postcard.AccountTiles)
                {
                    var tile = accountTile.Tile;
                    feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                    {
                        FeedItemType = FeedItemType.YouAddedTile.ToString(),
                        CardID = card.CardID,
                        CardTitle = card.Title,
                        TileTitle = tile.Title,
                        ImageUrls = new string[] { accountTile.ImageFullPath },
                        DateTimeStamp = accountTile.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTOVer1Dot1> YouCompletedCardFeedItemDTOs(IEnumerable<Postcard> postcards)
        {
            var feedItemDTOList = new List<FeedItemDTOVer1Dot1>();
            foreach (var postcard in postcards.Where(pc => pc.IsMarkedCompleted))
            {
                var card = postcard.Card;
                feedItemDTOList.Add(new FeedItemDTOVer1Dot1
                {
                    FeedItemType = FeedItemType.YouCompletedCard.ToString(),
                    CardID = card.CardID,
                    CardTitle = card.Title,
                    DateTimeStamp = postcard.CreatedDate,
                });
            }
            return feedItemDTOList;
        }
    }
}
