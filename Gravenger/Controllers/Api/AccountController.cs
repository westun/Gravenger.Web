using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;
using Gravenger.Domain.Core.Models;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/account")]
    public class AccountController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }

        public int CurrentAccountID
        {
            get
            {
                return this.User.Identity.GetAccountID();
            }
        }
        
        [HttpGet]
        public IHttpActionResult Get(int id)
        {
            var accountID = id;
            if (accountID <= 0)
            {
                return this.BadRequest("Account id required");
            }

            var account = this._unitOfWork.Accounts.GetWithPostcardsAndFollowing(accountID);
            if (account == null)
            {
                return this.BadRequest("A valid account id is required");
            }

            //TODO: Update to use automapper
            var accountDTO = new AccountDTO
            {
                AccountID = account.AccountID,
                Name = account.Name,
                Username = account.Username,
                ProfileImageFileName = account.ProfileImageFileName,
                ProfileImageFullPath = account.ProfileImageFullPath,
                Followers = account.Followers.Select(f => new AccountDTO
                {
                    AccountID = f.Follower.AccountID,
                    Name = f.Follower.Name,
                    Username = f.Follower.Username,
                    ProfileImageFullPath = f.Follower.ProfileImageFullPath,
                }),
                Followees = account.Followees.Select(f => new AccountDTO
                {
                    AccountID = f.Followee.AccountID,
                    Name = f.Followee.Name,
                    Username = f.Followee.Username,
                    ProfileImageFullPath = f.Followee.ProfileImageFullPath,
                }),
                Postcards = account.Postcards.OrderBy(pc => pc.Card.Title).Select(pc => new PostcardDTO
                {
                    PostcardID = pc.PostcardID,
                    AccountID = pc.AccountID,
                    CardID = pc.CardID,
                    Title = pc.Card.Title,
                    Likes = pc.PostcardLikes.Count(),
                    IsMarkedCompleted = pc.IsMarkedCompleted,
                    IsLikedByCurrentUser = pc.PostcardLikes.Any(pcl => pcl.AccountID == this.CurrentAccountID),
                    Tiles = pc.Card.Tiles.Select(t => new AccountTileDTO
                    {
                        AccountTileID = pc.AccountTiles.Where(at => at.TileID == t.TileID).Select(at => at.AccountTileID).FirstOrDefault(),
                        TileID = t.TileID,
                        CardID = pc.CardID,
                        Title = t.Title,
                        ImageFileName = pc.AccountTiles.Where(at => at.TileID == t.TileID).Select(at => at.ImageFileName).FirstOrDefault(),
                        ImageFullPath = pc.AccountTiles.Where(at => at.TileID == t.TileID).Select(at => at.ImageFullPath).FirstOrDefault()
                    }),
                })
            };

            return this.Ok(accountDTO);
        }

        [HttpPost]
        [Route("addpostcard/{cardID}")]
        public IHttpActionResult AddPostcard(int cardID)
        {
            if (cardID <= 0)
            {
                return this.BadRequest("card id required");
            }

            var card = this._unitOfWork.Cards.Get(cardID);
            if (card == null)
            {
                return this.BadRequest("A valid card id is required");
            }

            var postcard = this._unitOfWork.Postcards.Get(this.CurrentAccountID, cardID);
            if (postcard != null)
            {
                return this.BadRequest("Account already has postcard for given CardID");
            }

            this._unitOfWork.Postcards.Add(this.CurrentAccountID, cardID);
            this._unitOfWork.Complete();

            //TODO: To be considered "restful" this POST method should return the "Postcard" object that was created
            return this.Ok();
        }

        [HttpDelete]
        [Route("removePostcard/{cardID}")]
        public IHttpActionResult RemovePostcard(int cardID)
        {
            if (cardID <= 0)
            {
                return this.BadRequest("Card id required");
            }

            var card = this._unitOfWork.Cards.Get(cardID);
            if (card == null)
            {
                return this.BadRequest("A valid card id is required");
            }

            var postcard = this._unitOfWork.Postcards.GetWithAccountTiles(this.CurrentAccountID, cardID);
            if (postcard == null)
            {
                return this.BadRequest("Account does not have a postcard for the given cardID");
            }

            var postcardLikes = this._unitOfWork.PostcardLikes.GetAllByPostcardID(postcard.PostcardID);

            //TODO: Consider marking a PostCard as "Removed" by adding a bit value to the table instead of deleting it and all related data
            this._unitOfWork.PostcardLikes.RemoveRange(postcardLikes);
            this._unitOfWork.AccountTiles.RemoveRange(postcard.AccountTiles);
            this._unitOfWork.Postcards.Remove(postcard);
            this._unitOfWork.Complete();

            return this.Ok();
        }

        [HttpPost]
        [Route("likepostcard/{postcardID}")]
        public IHttpActionResult LikePostcard(int postcardID)
        {
            if (postcardID <= 0)
            {
                return this.BadRequest("Postcard Id required");
            }

            var postcard = this._unitOfWork.Postcards.GetWithAccountAndPostcardLikes(postcardID);
            if (postcard == null)
            {
                return this.BadRequest("A valid Postcard id is required");
            }

            if (postcard.PostcardLikes.Any(pcl => pcl.AccountID == this.CurrentAccountID))
            {
                return this.BadRequest("Account has already liked the postcard for the given postcardID");
            }

            if (postcard.AccountID == this.CurrentAccountID)
            {
                return this.BadRequest("Account cannot like their own postcard");
            }
            
            //TODO: encapsulate liking a postcard in the domain model 
            this._unitOfWork.PostcardLikes.Add(this.CurrentAccountID, postcardID);

            var currentAccount = this._unitOfWork.Accounts.Get(this.CurrentAccountID);
            var notification = new Notification(NotificationType.FolloweeLikedYourCard, currentAccount, postcard.Card);
            postcard.Account.Notify(notification);
            
            this._unitOfWork.Complete();

            return this.Ok();
        }

        [HttpDelete]
        [Route("removePostcardlike/{postcardID}")]
        public IHttpActionResult RemovePostcardLike(int postcardID)
        {
            if (postcardID <= 0)
            {
                return this.BadRequest("Postcard id required");
            }

            var postcard = this._unitOfWork.Postcards.GetWithAccountAndPostcardLikes(postcardID);
            if (postcard == null)
            {
                return this.BadRequest("A valid postcard id is required");
            }

            var postcardLike = postcard.PostcardLikes.FirstOrDefault(pcl => pcl.AccountID == this.CurrentAccountID);
            if (postcardLike == null)
            {
                return this.BadRequest("Account does not have a postcard like for the given postcardID");
            }
            
            this._unitOfWork.PostcardLikes.Remove(postcardLike);
            this._unitOfWork.Complete();

            return this.Ok();
        }

        [HttpPost]
        [Route("likeAccountTile/{accountTileID}")]
        public IHttpActionResult LikeAccountTile(int accountTileID)
        {
            if (accountTileID <= 0)
            {
                return this.BadRequest("Account Tile Id required");
            }

            var accountTile = this._unitOfWork.AccountTiles.GetWithAccountAndAccountTileLikes(accountTileID);
            if (accountTile == null)
            {
                return this.BadRequest("A valid Account Tile id is required");
            }

            if (accountTile.AccountTileLikes.Any(atl => atl.AccountID == this.CurrentAccountID))
            {
                return this.BadRequest("Account has already liked the account tile for the given accountTileID");
            }

            if (accountTile.AccountID == this.CurrentAccountID)
            {
                return this.BadRequest("Account cannot like their own account tile");
            }

            //TODO: encapsulate liking an account tile in the domain model 
            this._unitOfWork.AccountTileLikes.Add(this.CurrentAccountID, accountTileID);

            //TODO: create a notification when a user likes an account tile?
            //var currentAccount = this._unitOfWork.Accounts.Get(this.CurrentAccountID);
            //var notification = new Notification(NotificationType.FolloweeLikedYourCard, currentAccount, accountTile.Card);
            //accountTile.Account.Notify(notification);

            this._unitOfWork.Complete();

            return this.Ok();
        }

        [HttpDelete]
        [Route("removeAccountTilelike/{accountTileID}")]
        public IHttpActionResult RemoveAccountTilelike(int accountTileID)
        {
            if (accountTileID <= 0)
            {
                return this.BadRequest("Account tile id required");
            }

            var accountTile = this._unitOfWork.AccountTiles.GetWithAccountAndAccountTileLikes(accountTileID);
            if (accountTile == null)
            {
                return this.BadRequest("A valid account tile id is required");
            }

            var accountTileLike = accountTile.AccountTileLikes.FirstOrDefault(atl => atl.AccountID == this.CurrentAccountID);
            if (accountTileLike == null)
            {
                return this.BadRequest("Account does not have a postcard like for the given postcardID");
            }

            this._unitOfWork.AccountTileLikes.Remove(accountTileLike);
            this._unitOfWork.Complete();

            return this.Ok();
        }
    }
}
