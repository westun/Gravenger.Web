using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public class NotificationController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationController(IUnitOfWork unitOfWork)
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
        public IHttpActionResult Get(int count = 0)
        {
            var accountNotificaitons =  this._unitOfWork.AccountNotifications.GetAllWithNotifications(this.CurrentAccountID)
                .OrderByDescending(an => an.Notification.CreatedDate);

            var notificationDTOs = accountNotificaitons.Select(an => new NotificationDTO
            {
                ActorAccountUsername = an.Notification.ActorAccount.Username,
                ActorAccountProfileImageFullPath = an.Notification.ActorAccount.ProfileImageFullPath,
                CardID = an.Notification.CardID,
                CardTitle = an.Notification.CardTitle,
                Type = an.Notification.Type,
                IsRead = an.IsRead,
                DateTimeStamp = an.Notification.CreatedDate,
            });

            //TODO: update repository to accept a count parameter so that 
            //      all notifications for a user are not returned from database
            if (count > 0)
            {
                return this.Ok(notificationDTOs.Take(count));
            }

            return this.Ok(notificationDTOs);
        }

        [HttpPut]
        public IHttpActionResult MarkAllAsRead()
        {
            var accountNotificaitons = this._unitOfWork.AccountNotifications.GetAllUnreadNotifications(this.CurrentAccountID);

            if (accountNotificaitons.Any())
            {
                foreach (var accountNotification in accountNotificaitons)
                {
                    accountNotification.IsRead = true;
                }
                this._unitOfWork.Complete();
            }

            return Ok("success");
        }
    }
}
