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
    [RoutePrefix("api/postcard")]
    public class PostcardController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public PostcardController(IUnitOfWork unitOfWork)
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

        //TODO: does this belong in postcard api controller, or card api controller?
        //      Is it restful/okay to return a different entity in an api controller meant for another entity
        //      (i.e.: CardController api returns accounts, instead of cards)
        [HttpGet]
        [Route("{postcardID}/postcardlikes/accounts")] //TODO: what is the appropriate endpoint for getting all postcards by card id in order to be Restful?
        public IHttpActionResult GetAccountsThatLikePostcard(int postcardID)
        {
            if (postcardID <= 0)
            {
                return this.BadRequest("postcardID required");
            }

            var accounts = this._unitOfWork.Accounts.GetAllThatLikePostcard(postcardID);

            var accountDTOs = accounts.Select(a => new AccountDTO
            {
                AccountID = a.AccountID,
                Name = a.Name,
                Username = a.Username,
                ProfileImageFileName = a.ProfileImageFileName,
                ProfileImageFullPath = a.ProfileImageFullPath,
            });

            return this.Ok(accountDTOs);
        }
    }
}
