using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public class FollowingController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public FollowingController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }
                
        [HttpPost]
        public IHttpActionResult Follow(FolloweeDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload cannot be null");
            }

            if (dto.FolloweeID <= 0)
            {
                return this.BadRequest("Follower id required");
            }

            var followee = this._unitOfWork.Accounts.Get(dto.FolloweeID);
            if (followee == null)
            {
                return this.BadRequest("A valid follower id is required");
            }

            var currentAccount = this._unitOfWork.Accounts.GetWithFollowing(this.User.Identity.GetAccountID());
            bool alreadyFollowing = currentAccount.Followees.Any(f => f.FolloweeID == followee.AccountID);
            if(alreadyFollowing)
            {
                return this.BadRequest("Current user is already following user with provided follower id");
            }

            this._unitOfWork.Followings.Add(dto.FolloweeID, currentAccount.AccountID);

            followee.Notify(new Notification(NotificationType.UserIsFollowingYou, currentAccount));

            this._unitOfWork.Complete();

            return Ok("success");
        }

        [HttpDelete]
        public IHttpActionResult Unfollow(FolloweeDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload cannot be null");
            }

            if (dto.FolloweeID <= 0)
            {
                return this.BadRequest("Follower id required");
            }

            var followee = this._unitOfWork.Accounts.Get(dto.FolloweeID);
            if (followee == null)
            {
                return this.BadRequest("A valid follower id is required");
            }

            var currentAccount = this._unitOfWork.Accounts.GetWithFollowing(this.User.Identity.GetAccountID());
            bool alreadyFollowing = currentAccount.Followees.Any(f => f.FolloweeID == followee.AccountID);
            if (!alreadyFollowing)
            {
                return this.BadRequest("Current user is not following user with provided follower id");
            }

            this._unitOfWork.Followings.Remove(dto.FolloweeID, currentAccount.AccountID);
            this._unitOfWork.Complete();

            return Ok("success");
        }
    }
}
