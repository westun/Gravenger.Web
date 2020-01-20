using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;
using AutoMapper;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public class AccountListController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountListController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            var currentAccountID = this.User.Identity.GetAccountID(); 
            if (currentAccountID <= 0)
            {
                return this.BadRequest("Account id required");
            }

            var currentAccount = this._unitOfWork.Accounts.GetWithFollowing(currentAccountID);
            var allAccounts = this._unitOfWork.Accounts.GetAll().Where(a => a.AccountID != currentAccountID);
            var accountListDTO = allAccounts.Select(Mapper.Map<AccountListDTO>).Select(a =>
            {
                a.CurrentUserIsFollowing = currentAccount.Followees.Any(f => f.Followee.AccountID == a.AccountID);
                a.CurrentUserIsFollowed = currentAccount.Followers.Any(f => f.Follower.AccountID == a.AccountID);

                return a;
            });
            
            return Ok(accountListDTO.OrderBy(al => al.Name));
        }
        
    }
}
