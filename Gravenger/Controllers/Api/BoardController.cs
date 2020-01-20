using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    [RoutePrefix("api/board")]
    public class BoardController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public BoardController(IUnitOfWork unitOfWork)
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
        [Route("MyTopPostcards")]
        public IHttpActionResult MyTopPostcards()
        {
            var topPostcards = this._unitOfWork.Postcards.GetTopLiked(this.CurrentAccountID);

            var postCardDto = topPostcards.Select(pc => new PostcardDTO
            {
                PostcardID = pc.PostcardID,
                CardID = pc.CardID,
                Title = pc.Card.Title,
                Likes = pc.PostcardLikes.Count(),
            });

            return this.Ok(postCardDto.Where(pc => pc.Likes > 0));
        }

        [HttpGet]
        [Route("TopLikedUsers")]
        public IHttpActionResult TopLikedUsers()
        {
            var topLikedUsers = this._unitOfWork.Reports.GetTopLikedUsers();

            return this.Ok(topLikedUsers);
        }

        [HttpGet]
        [Route("TopActiveCards")]
        public IHttpActionResult TopActiveCards()
        {
            var topActiveCards = this._unitOfWork.Reports.GetTopActiveCards();

            return this.Ok(topActiveCards);
        }

        [HttpGet]
        [Route("TopLikedFollowers")]
        public IHttpActionResult TopLikedFollowers()
        {
            var topLikedFollowers = this._unitOfWork.Reports.GetTopLikedFollowers(this.CurrentAccountID);

            return this.Ok(topLikedFollowers);
        }

        [HttpGet]
        [Route("TopLikedFollowees")]
        public IHttpActionResult TopLikedFollowees()
        {
            var topLikedFollowees = this._unitOfWork.Reports.GetTopLikedFollowees(this.CurrentAccountID);

            return this.Ok(topLikedFollowees);
        }

        [HttpGet]
        [Route("TopLikedCards")]
        public IHttpActionResult TopLikedCards()
        {
            var topLikedCards = this._unitOfWork.Reports.GetTopLikedCards();

            return this.Ok(topLikedCards);
        }
    }
}
