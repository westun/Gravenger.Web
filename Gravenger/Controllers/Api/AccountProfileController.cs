using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Core.Repositories;
using Gravenger.Domain.Persistence.Repositories;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public class AccountProfileController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountProfileController(IUnitOfWork unitOfWork)
        {
            if(unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }

        [HttpPut]
        public IHttpActionResult Put(AccountProfileDTO dto)
        {
            if(dto == null)
            {
                return this.BadRequest("Payload cannot be null");
            }

            if(dto.ProfileImageFileName == null || dto.ProfileImageFullPath == null)
            {
                return this.BadRequest("Image file name and image full path must not be null");
            }

            var account = this._unitOfWork.Accounts.Get(this.User.Identity.GetAccountID());
            account.ProfileImageFileName = dto.ProfileImageFileName;
            account.ProfileImageFullPath = dto.ProfileImageFullPath;

            this._unitOfWork.Complete();

            return this.Ok();
        }
    }
}
