namespace Gravenger.Controllers
{
    using Domain.Security;
    using Gravenger.Domain.Core;
    using Gravenger.Domain.Core.Models;
    using System.Web;
    using System.Web.Mvc;

    public class BaseController : Controller
    {
        private Account _currentAccount;
        private readonly IUnitOfWork _unitOfWork;

        public BaseController()
        {
            this._unitOfWork = DependencyResolver.Current.GetService<IUnitOfWork>();
        }
                
        public int AccountID
        {
            get { return (User.Identity as AccountIdentity).AccountID; }
        }

        public Account CurrentAcount
        {
            get
            {
                if (_currentAccount == null)
                {
                    _currentAccount = this._unitOfWork.Accounts.Get(this.AccountID);
                }
                return _currentAccount;
            }
        }

        public bool SignedIn
        {
            get
            {
                return this.User.Identity.IsAuthenticated;
            }
        }
    }
}
