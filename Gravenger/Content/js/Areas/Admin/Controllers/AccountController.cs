using Gravenger.Areas.Admin.ViewModels;
using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Gravenger.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountController : Controller
    {
        private IUnitOfWork _unitOfWork;

        public AccountController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
    }
}