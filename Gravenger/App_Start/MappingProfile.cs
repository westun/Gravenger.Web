using AutoMapper;
using Gravenger.Azure;
using Gravenger.Domain.Core.Dto;
using Gravenger.Domain.Core.Models;

namespace Gravenger
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            this.CreateMap<Tile, TileDTO>();
            this.CreateMap<Account, AccountDTO>();
            this.CreateMap<Account, AccountListDTO>();
            this.CreateMap<ActivityFeedEntity, FeedItemDTOVer2>();
            this.CreateMap<Tag, TagDTO>();
        }
    }
}