using Gravenger.Domain.Core;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize]
    public class CategoryController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
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
            var categoryNames = this._unitOfWork.Categories.GetAll()
                .OrderBy(c => c.Title)
                .Select(c => c.Title);

            return this.Ok(categoryNames);
        }
    }
}
