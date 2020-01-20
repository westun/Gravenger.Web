using Gravenger.Domain.Core.Models;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core;
using Gravenger.Domain.Security.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Gravenger.Controllers.Api
{
    [Authorize(Roles = "Admin")]
    public class AdminAccountTileController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminAccountTileController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }
        
        [HttpDelete]
        [Authorize(Roles = "Can Manage Account Photos")]
        public IHttpActionResult Delete(AccountTileDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload is null");
            }

            if (dto.TileID <= 0)
            {
                return this.BadRequest("Payload required a valid TileID");
            }

            if (dto.AccountID <= 0)
            {
                return this.BadRequest("Payload required a valid AccountID");
            }

            var accountTileFromDatabase = this._unitOfWork.AccountTiles.Get(dto.AccountID, dto.TileID);
            if (accountTileFromDatabase == null)
            {
                return this.BadRequest("Tile does not exist for this account");
            }

            this._unitOfWork.AccountTiles.Remove(accountTileFromDatabase);
            this._unitOfWork.Complete();
            
            return Ok();
        }
    }
}
