using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZenGarden.Domain.DTOs;
using ZenGarden.Domain.Entities;
using AutoMapper;

namespace ZenGarden.Core.Mappings
{
    public class ItemMappingProfile : Profile
    {
        public ItemMappingProfile()
        {
            // Ánh xạ từ Item sang ItemDto và ngược lại
            CreateMap<Item, ItemDto>()
                .ForMember(dest => dest.ItemDetail, opt => opt.MapFrom(src => src.ItemDetail));

            CreateMap<ItemDetail, ItemDetailDto>();
        }
    }
    
}
