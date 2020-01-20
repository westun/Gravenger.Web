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
    [RoutePrefix("api/card")]
    public class CardController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public CardController(IUnitOfWork unitOfWork)
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
        public IHttpActionResult Get(int id)
        {
            var cardID = id;
            if (cardID <= 0)
            {
                return this.BadRequest("cardID required");
            }

            var card = this._unitOfWork.Cards.GetWithTilesAndPostcards(cardID, this.CurrentAccountID);
            if (card == null)
            {
                return this.NotFound();
            }

            var cardDTO = this.MapCardDTO(card);

            return this.Ok(cardDTO);
        }

        [HttpGet]
        [Route("{cardID}/{username}")]
        public IHttpActionResult Get(int cardID, string username)
        {
            if (cardID <= 0)
            {
                return this.BadRequest("cardID required");
            }

            if (string.IsNullOrEmpty(username))
            {
                return this.BadRequest("username required");
            }

            var account = this._unitOfWork.Accounts.GetByUsername(username);
            if (account == null)
            {
                return this.NotFound();
            }
            
            var card = this._unitOfWork.Cards.GetWithTilesAndPostcards(cardID, account.AccountID);
            if (card == null)
            {
                return this.NotFound();
            }

            var cardDTO = this.MapCardDTO(card);

            return this.Ok(cardDTO);
        }
        
        [HttpGet]
        public IHttpActionResult GetAll()
        {
            //TODO: does this api call need to return tiles and postcards, or just cards?
            var cards = this._unitOfWork.Cards.GetAllWithCategories();

            var cardDTOs = this.MapCardDTOs(cards).ToList();

            return this.Ok(cardDTOs);
        }

        //TODO: does this belong in postcard api controller, or card api controller?
        //      Is it restful/okay to return a different entity in an api controller meant for another entity
        //      (i.e.: CardController api returns accounts, instead of cards)
        [HttpGet]
        [Route("{cardID}/postcards/accounts")] //TODO: what is the appropriate endpoint for getting all postcards by card id in order to be Restful?
        public IHttpActionResult GetAccountsByCardID(int cardID)
        {
            if (cardID <= 0)
            {
                return this.BadRequest("cardID required");
            }

            var accounts = this._unitOfWork.Accounts.GetAllByCardID(cardID);

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

        [HttpGet]
        [Route("search")]
        [Route("search/{criteria}")]
        public IHttpActionResult Search(string criteria = null)
        {
            if (string.IsNullOrWhiteSpace(criteria))
            {
                return this.GetAll();
            }

            //TODO: implement more sophisticated search, possibly using odata query
            var cardSearchResults = this._unitOfWork.Cards.Search(criteria);

            var cardDTOs = this.MapCardDTOs(cardSearchResults).ToList();

            return this.Ok(cardDTOs);
        }

        private CardDTO MapCardDTO(Card card)
        {
            return new CardDTO
            {
                CardID = card.CardID,
                Title = card.Title,
                Tiles = card.Tiles.Select(t => new TileDTO
                {
                    TileID = t.TileID,
                    CardID = card.CardID,
                    Title = t.Title,
                    ImageFileName = t.AccountTiles.FirstOrDefault()?.ImageFileName,
                    ImageFullPath = t.AccountTiles.FirstOrDefault()?.ImageFullPath,
                }),
                Postcards = card.Postcards.Select(pc => new PostcardDTO
                {
                    PostcardID = pc.PostcardID,
                    AccountID = pc.AccountID,
                    CardID = pc.CardID,
                    Title = card.Title,
                    Likes = pc.PostcardLikes.Count(),
                    IsMarkedCompleted = pc.IsMarkedCompleted,
                    //TODO: does this belong here, or should 
                    //      1. a separate API call made to determine this?
                    //      2. "PostcardLikeDTO" be added and contain list of users who like this postcard to determine this?
                    IsLikedByCurrentUser = pc.PostcardLikes.Any(pcl => pcl.AccountID == this.CurrentAccountID),
                    //TODO: Add AccountTiles?
                }),
            };
        }

        private IEnumerable<CardDTO> MapCardDTOs(IEnumerable<Card> cards)
        {
            return cards.Select(card => new CardDTO
            {
                CardID = card.CardID,
                Title = card.Title,
                Category = card.Category?.Title,
                Tiles = this.MapTileDTO(card),
            });
        }

        private IEnumerable<TileDTO> MapTileDTO(Card card)
        {
            return card.Tiles.Select(t => new TileDTO
            {
                TileID = t.TileID,
                CardID = t.CardID,
                Title = t.Title,
                Position = t.Position,
                ImageFileName = t.AccountTiles.Select(at => at.ImageFileName).FirstOrDefault(),
                ImageFullPath = t.AccountTiles.Select(at => at.ImageFullPath).FirstOrDefault(),
            });
        }
    }
}
