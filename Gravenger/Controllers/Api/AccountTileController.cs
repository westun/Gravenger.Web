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
    [Authorize]
    public class AccountTileController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountTileController(IUnitOfWork unitOfWork)
        {
            if (unitOfWork == null)
            {
                throw new ArgumentNullException(nameof(unitOfWork));
            }

            this._unitOfWork = unitOfWork;
        }
        
        [HttpPost]
        public IHttpActionResult Post(AccountTileDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload is null");
            }

            if (dto.TileID <= 0)
            {
                return this.BadRequest("Payload required a valid TileID");
            }

            if (dto.ImageFileName == null || dto.ImageFullPath == null)
            {
                return this.BadRequest("Image file name and full path are required");
            }

            if (dto.CardID <= 0)
            {
                return this.BadRequest("Payload requires a valid CardID");
            }

            //TODO: Consider if this is the correct approach
            var currentAccountID = this.User.Identity.GetAccountID();
            var postcard = this._unitOfWork.Postcards.GetWithAccountTiles(currentAccountID, dto.CardID);
            if (postcard == null)
            {
                //If user is adding a tile, and they have not yet added the Postcard, add the Postcard
                this._unitOfWork.Postcards.Add(currentAccountID, dto.CardID);
                this._unitOfWork.Complete();

                postcard = this._unitOfWork.Postcards.GetWithAccountTiles(currentAccountID, dto.CardID);
            }
            
            var accountTile = new AccountTile
            {
                AccountID = currentAccountID,
                PostcardID = postcard.PostcardID,
                TileID = dto.TileID,
                ImageFileName = dto.ImageFileName,
                ImageFullPath = dto.ImageFullPath,
            };

            this._unitOfWork.AccountTiles.Add(accountTile);
            this._unitOfWork.Complete();

            //TODO: entity framework saves postcard completed date after account tiles are saved. 
            //      This causes the "user completed card..." feed item to show after the last (tile 9) in the feed.
            //      This code also has the issue that if the second database call fails, the postcard is never marked as completed
            //      Is there another way to handle this scenario?
            if (postcard.TryMarkCompleted())
            {
                this._unitOfWork.Complete();
            }

            return Ok(dto);
        }
        
        //TODO: This may actually need to be a "PATCH" request?
        [HttpPut]
        public IHttpActionResult Put(AccountTileDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("Payload is null");
            }

            if (dto.TileID <= 0)
            {
                return this.BadRequest("Payload required a valid TileID");
            }

            var accountTileFromDatabase = this._unitOfWork.AccountTiles.Get(this.User.Identity.GetAccountID(), dto.TileID);
            if (accountTileFromDatabase == null)
            {
                return this.BadRequest("Tile does not exist for this account");
            }

            if (dto.ImageFileName == null || dto.ImageFullPath == null)
            {
                return this.BadRequest("Image file name and full path are required");
            }

            accountTileFromDatabase.ImageFileName = dto.ImageFileName;
            accountTileFromDatabase.ImageFullPath = dto.ImageFullPath;

            this._unitOfWork.Complete();

            return this.Ok();
        }

        [HttpDelete]
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

            var accountTileFromDatabase = this._unitOfWork.AccountTiles.Get(this.User.Identity.GetAccountID(), dto.TileID);
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
