using pharmacyBackend.DTO;
using pharmacyBackend.Models;
using AutoMapper;
namespace pharmacyBackend.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            //лекарства
            CreateMap<Product, ProductShortDTO>()
                .ForMember(dest => dest.MinPrice, opt => opt.MapFrom(src =>
                    src.Stocks.Any() ? src.Stocks.Min(s => s.Price) : 0));

            CreateMap<Product, ProductMediumDTO>()
                .IncludeBase<Product, ProductShortDTO>() 
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                .ForMember(dest => dest.MaxPrice, opt => opt.MapFrom(src =>
                    src.Stocks.Any() ? src.Stocks.Max(s => s.Price) : 0));

            CreateMap<Product, ProductLongDTO>()
                .IncludeBase<Product, ProductMediumDTO>()
                .ForMember(dest => dest.AvailableIn, opt => opt.MapFrom(src => src.Stocks));

            CreateMap<Product, ProductFullDTO>()
                .IncludeBase<Product, ProductLongDTO>();

            //категории
            CreateMap<Category, CategoryDTO>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                    src.CategoryTags.Select(ct => ct.Tag.Name)));

            CreateMap<Category, CategoryFullDTO>()
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src =>
                    src.CategoryTags.Select(ct => ct.TagId)));

            //аптеки
            CreateMap<Pharmacy, PharmacyDTO>();

            CreateMap<Pharmacy, PharmacyFullDTO>();

            //хранение
            CreateMap<Stock, PharmacyStockDTO>()
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.Pharmacy.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            CreateMap<Stock, PharmacyStockFullDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId));

            CreateMap<Stock, CartItemDTO>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price));

            //заказ
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.PharmacyName, opt => opt.MapFrom(src => src.Pharmacy.Name));

            CreateMap<Order, OrderWithItemsDTO>()
                .IncludeBase<Order, OrderDTO>()
                .ForMember(dest => dest.PharmacyAddress, opt => opt.MapFrom(src => src.Pharmacy.Address))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<Order, OrderFullDTO>()
                .IncludeBase<Order, OrderWithItemsDTO>()
                .ForMember(dest => dest.UserFirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.UserPhone, opt => opt.MapFrom(src => src.User.Phone));

            CreateMap<OrderItem, OrderItemDTO>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price));

            //пользователь
            CreateMap<User, UserDataDTO>();
        } 
    }
}
