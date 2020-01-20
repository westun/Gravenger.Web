using Gravenger.Domain.Core;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "Can Manage Cards")]
    [RoutePrefix("api/admin/categories")]
    public class AdminCategoriesController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminCategoriesController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult Get()
        {
            var categoryNames = this._unitOfWork.Categories.GetAll()
                .OrderBy(c => c.Title)
                .Select(c => c.Title);

            return this.Ok(categoryNames);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post(CategoryDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Title))
            {
                return this.BadRequest("Category title is required");
            }
            
            var categoryNames = this._unitOfWork.Categories.GetAll()
                .OrderBy(c => c.Title)
                .Select(c => c.Title);

            var categoryTitle = dto.Title.Trim();

            if (categoryNames.Any(c => c.ToLower() == categoryTitle.ToLower()))
            {
                return this.BadRequest($"Category with title {dto.Title} already exists");
            }

            var newCategory = new Category
            {
                Title = categoryTitle,
            };

            this._unitOfWork.Categories.Add(newCategory);

            this._unitOfWork.Complete();

            return this.Ok(newCategory);
        }
    }
}
