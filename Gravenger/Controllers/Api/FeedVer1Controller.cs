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
    [RoutePrefix("api/feed/v1")]
    public class FeedVer1Controller : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeedVer1Controller(IUnitOfWork unitOfWork)
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

            var followings = this._unitOfWork.Followings.GetFolloweesWithAll(currentAccountID);
            
            var allFeedItems = this.GetAllFeedItemDTOs(followings).OrderByDescending(fi => fi.DateTimeStamp).ToList();

            return Ok(allFeedItems);
        }

        private IEnumerable<FeedItemDTO> GetAllFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItems = new List<FeedItemDTO>();

            feedItems.AddRange(this.GetFolloweeAddedTileFeedItemDTOs(followings));
            feedItems.AddRange(this.GetYouFollowedFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeFollowedFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeJoinedFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeAddedCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeCompletedCardFeedItemDTOs(followings));
            feedItems.AddRange(this.GetFolloweeLikedCardFeedItemDTOs(followings));

            return feedItems;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeAddedTileFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                feedItemDTOList.AddRange(following.Followee.AccountTiles.Select(at => new FeedItemDTO
                {
                    Type = FeedItemType.FolloweeAddedTile.ToString(),
                    Message = $"{following.Followee.Username} added a photo to the postcard {at.Postcard.Card.Title} for tile {at.Tile.Title}",
                    ImageUrls = new string[] { at.ImageFullPath },
                    DateTimeStamp = at.CreatedDate,
                }));
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetYouFollowedFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                feedItemDTOList.Add(new FeedItemDTO
                {
                    Type = FeedItemType.YouFollowed.ToString(),
                    Message = $"You followed {following.Followee.Username}",
                    //ImageUrls = new string[] { following.Followee.ProfileImageFullPath },
                    DateTimeStamp = following.CreatedDate,
                });
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeFollowedFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            var currentAccountID = this.User.Identity.GetAccountID();
            foreach (var following in followings)
            {
                var yourFolloweesFollowings = following.Followee.Followees;
                foreach (var followeeFollowing in yourFolloweesFollowings)
                {
                    var followerUserNameOrYourself = followeeFollowing.FolloweeID == currentAccountID ? "You" : followeeFollowing.Followee.Username;
                    feedItemDTOList.Add(new FeedItemDTO
                    {
                        Type = FeedItemType.FolloweeFollowed.ToString(),
                        Message = $"{followeeFollowing.Follower.Username} followed {followerUserNameOrYourself}",
                        //ImageUrls = new string[] 
                        //{
                        //    following.Follower.ProfileImageFullPath,
                        //    following.Followee.ProfileImageFullPath
                        //},
                        DateTimeStamp = followeeFollowing.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeJoinedFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                feedItemDTOList.Add(new FeedItemDTO
                {
                    Type = FeedItemType.UserJoined.ToString(),
                    Message = $"{following.Followee.Username} joined Seenry",
                    //ImageUrls = new string[] { at.ImageFullPath },
                    DateTimeStamp = following.Followee.CreatedDate,
                });
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeAddedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                var followeePostcards = following.Followee.Postcards;
                foreach (var postcard in followeePostcards)
                {
                    feedItemDTOList.Add(new FeedItemDTO
                    {
                        Type = FeedItemType.FolloweeAddedCard.ToString(),
                        Message = $"{following.Followee.Username} added the postcard {postcard.Card.Title}",
                        //ImageUrls = new string[] { at.ImageFullPath },
                        DateTimeStamp = postcard.CreatedDate,
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeCompletedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                var followeePostcards = following.Followee.Postcards.Where(p => p.CompletedDate.HasValue);
                foreach (var postcard in followeePostcards)
                {
                    feedItemDTOList.Add(new FeedItemDTO
                    {
                        Type = FeedItemType.FolloweeCompletedCard.ToString(),
                        Message = $"{following.Followee.Username} completed the postcard {postcard.Card.Title}",
                        //ImageUrls = new string[] { at.ImageFullPath },
                        DateTimeStamp = postcard.CompletedDate.GetValueOrDefault(),
                    });
                }
            }
            return feedItemDTOList;
        }

        private IEnumerable<FeedItemDTO> GetFolloweeLikedCardFeedItemDTOs(IEnumerable<Following> followings)
        {
            var currentAccountID = this.User.Identity.GetAccountID();
            var feedItemDTOList = new List<FeedItemDTO>();
            foreach (var following in followings)
            {
                var followeeLikedPostcards = following.Followee.PostcardLikes.Select(pcl => pcl.Postcard);
                foreach (var postcard in followeeLikedPostcards)
                {
                    var likedUserText = postcard.AccountID == currentAccountID ? "your" : postcard.Account.Username + "'s";
                    feedItemDTOList.Add(new FeedItemDTO
                    {
                        Type = FeedItemType.FolloweeLikedCard.ToString(),
                        Message = $"{following.Followee.Username} liked {likedUserText} postcard {postcard.Card.Title}",
                        //ImageUrls = new string[] { at.ImageFullPath },
                        DateTimeStamp = following.Followee.PostcardLikes
                            .Where(pcl => pcl.PostcardID == postcard.PostcardID)
                            .FirstOrDefault().CreatedDate
                    });
                }
            }
            return feedItemDTOList;
        }
    }
}
