using AutoMapper;
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
    [Authorize(Roles = "Can Manage Cards")]
    [RoutePrefix("api/admin/cards")]
    public class AdminCardController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminCardController(IUnitOfWork unitOfWork)
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
        [Route("{cardID}")]
        public IHttpActionResult Get(int cardID)
        {
            if (cardID <= 0)
            {
                return this.BadRequest("A valid cardID is required");
            }

            var card = this._unitOfWork.Cards.GetWithTilesTagsCategories(cardID);
            if (card == null)
            {
                return this.NotFound();
            }

            var cardDTO = new CardDTO
            {
                CardID = card.CardID,
                Title = card.Title,
                Category = card.Category?.Title,
                CreatedDateTime = card.CreatedDate,
                Tags = card.Tags
                    .Select(t => Mapper.Map<TagDTO>(t))
                    .OrderBy(t => t.Name),
                Tiles = this.MapTileDTO(card),
            };

            return this.Ok(cardDTO);
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var cards = this._unitOfWork.Cards.GetAllWithCategoriesAndTiles();

            var cardDTOs = this.MapCardDTOs(cards).ToList();

            return this.Ok(cardDTOs);
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
            var cardSearchResults = this._unitOfWork.Cards.SearchWithCategoriesAndTiles(this.CurrentAccountID, criteria);

            var cardDTOs = this.MapCardDTOs(cardSearchResults).ToList();

            return this.Ok(cardDTOs);
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post(CardDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("payload is null");
            }

            if (dto.Tiles == null)
            {
                return this.BadRequest("Card must contain tiles");
            }

            if (dto.CardID > 0)
            {
                return this.BadRequest("Card already contains a CardID.  To update card use an http PUT request");
            }

            if (dto.Title == null)
            {
                return this.BadRequest("Card must contain a title");
            }

            //TODO: If category is to be required, do a null check.

            //TODO: how to handle unique name constraint for cards?
            var cardFoundByName = this._unitOfWork.Cards.Find(c => c.Title.ToLower() == dto.Title.ToLower());
            if (cardFoundByName.Any())
            {
                return this.BadRequest($"A card with the title {dto.Title} already exists.  Card titles must be unique");
            }

            Category matchingCategoryFromDatabase = null;
            if (dto.Category != null)
            {
                matchingCategoryFromDatabase = this._unitOfWork.Categories.Find(c => c.Title == dto.Category).FirstOrDefault();
                if (matchingCategoryFromDatabase == null)
                {
                    return this.BadRequest("Card must contain a valid category");
                }
            }

            dto.Tags = dto.Tags.Where(t => t.Name != null);
            var tagsToAddToDatabase = new List<Tag>();
            if (dto.Tags != null && dto.Tags.Any())
            {
                var dtoTagNames = dto.Tags.Select(t => t.Name.Trim().ToLower());
                var tagsFoundInDatabase = this._unitOfWork.Tags.Find(t => dtoTagNames.Contains(t.Name.ToLower()));
                tagsToAddToDatabase = this.MergeTagsMatchingFromDatabase(dto.Tags, tagsFoundInDatabase).ToList();
            }

            //TODO: Use automapper
            Card card = new Card
            {
                Title = dto.Title.Trim(),
                Tiles = dto.Tiles.Select(t => new Tile { Title = t.Title, Position = t.Position }).ToList(),
                Tags = tagsToAddToDatabase,
                Category = matchingCategoryFromDatabase,
            };

            this._unitOfWork.Cards.Add(card);
            this._unitOfWork.Complete();

            dto.CardID = card.CardID;

            return this.Ok(dto);
        }

        [HttpPut]
        [Route("")]
        public IHttpActionResult Put(CardDTO dto)
        {
            if (dto == null)
            {
                return this.BadRequest("payload is null");
            }

            if (dto.Tiles == null)
            {
                return this.BadRequest("Card must contain tiles");
            }

            if (dto.CardID <= 0)
            {
                return this.BadRequest("Card must contain a valid cardID");
            }
            
            if (dto.Title == null)
            {
                return this.BadRequest("Card must contain a title");
            }

            var cardFoundInDatabaseByID = this._unitOfWork.Cards.GetWithTilesTagsCategories(dto.CardID);
            if (cardFoundInDatabaseByID == null)
            {
                return this.NotFound();
            }

            if (!dto.Title.Equals(cardFoundInDatabaseByID.Title, StringComparison.InvariantCultureIgnoreCase))
            {
                return this.BadRequest("Card title cannot change once the card has been created");
            }

            //TODO: If category is to be required, do a null check.
            
            Category matchingCategoryFromDatabase = null;
            if (dto.Category != null)
            {
                matchingCategoryFromDatabase = this._unitOfWork.Categories.Find(c => c.Title == dto.Category).FirstOrDefault();
                if (matchingCategoryFromDatabase == null)
                {
                    return this.BadRequest("Card must contain a valid category");
                }
            }

            dto.Tags = dto.Tags.Where(t => t.Name != null);
            var tagsToAddOrUpdateInDatabase = new List<Tag>();
            if (dto.Tags != null && dto.Tags.Any())
            {
                var dtoTagNames = dto.Tags.Select(t => t.Name.Trim().ToLower());
                var tagsFoundInDatabase = this._unitOfWork.Tags.Find(t => dtoTagNames.Contains(t.Name.ToLower()));
                tagsToAddOrUpdateInDatabase = this.MergeTagsMatchingFromDatabase(dto.Tags, tagsFoundInDatabase).ToList();
            }

            //take all elements in cardFoundInDatabaseByID.Tags except the same ones in tagsToAddOrUpdateInDatabase
            var tagsToRemove = cardFoundInDatabaseByID.Tags.Except(tagsToAddOrUpdateInDatabase).ToArray();
            foreach (var tag in tagsToRemove)
            {
                cardFoundInDatabaseByID.Tags.Remove(tag);
            }

            //take all elements in tagsToAddOrUpdateInDatabase except the same ones in cardFoundInDatabaseByID.Tags
            var tagsToAdd = tagsToAddOrUpdateInDatabase.Except(cardFoundInDatabaseByID.Tags).ToArray();
            foreach (var tag in tagsToAdd)
            {
                cardFoundInDatabaseByID.Tags.Add(tag);
            }

            foreach (var tile in cardFoundInDatabaseByID.Tiles)
            {
                var dtoTile = dto.Tiles.FirstOrDefault(t => t.TileID == tile.TileID);
                if (dtoTile != null)
                {
                    tile.Title = dtoTile.Title;
                }
            }

            cardFoundInDatabaseByID.Category = matchingCategoryFromDatabase;

            this._unitOfWork.Complete();

            return this.Ok(dto);
        }

        private IEnumerable<CardDTO> MapCardDTOs(IEnumerable<Card> cards)
        {
            return cards.Select(card => new CardDTO
            {
                CardID = card.CardID,
                Title = card.Title,
                Category = card.Category?.Title,
                CreatedDateTime = card.CreatedDate,
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

        private IEnumerable<Tag> MergeTagsMatchingFromDatabase(IEnumerable<TagDTO> tagDTOs, IEnumerable<Tag> tagsFoundInDatabase)
        {
            List<Tag> tagsToAddOrUpdateInDatabase = new List<Tag>();
            foreach (var tag in tagDTOs)
            {
                tag.Name = tag.Name.Trim();
                var matchingTagFromDatabase = tagsFoundInDatabase.FirstOrDefault(t => t.Name.ToLower() == tag.Name.ToLower());
                if (matchingTagFromDatabase != null)
                {
                    tagsToAddOrUpdateInDatabase.Add(matchingTagFromDatabase);
                }
                else
                {
                    tagsToAddOrUpdateInDatabase.Add(new Tag { Name = tag.Name });
                }
            }
            return tagsToAddOrUpdateInDatabase;
        }
    }
}
