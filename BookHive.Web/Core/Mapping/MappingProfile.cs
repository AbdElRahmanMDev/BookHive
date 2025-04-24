using Microsoft.AspNetCore.Mvc.Rendering;
namespace BookHive.Web.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ///<Source,Destination>
            ///.formember(dest=>dest.countryName,obj=>obj.MapFrom(src=>src.Name)

            //Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>().
            ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)).
            ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            //Text: The value displayed in the dropdown.
            // Value: The underlying value(usually an Id).
            CreateMap<Author, SelectListItem>().
            ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)).
            ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
            //Books
            CreateMap<BookFormViewModel, Book>().ReverseMap()
            .ForMember(dest => dest.Categories, opt => opt.Ignore()); // Also ignore in reverse mapping

            CreateMap<Book, BookViewModel>().
           ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name)).
           ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(x => x.Category!
           .Name)));

            CreateMap<BookCopy, BookCopyViewModel>().
          ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title)).
          ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.BookId)).
          ForMember(dest => dest.BookThumbnailUrl, opt => opt.MapFrom(src => src.Book!.ImageUrlThumb));



            CreateMap<BookCopyFormViewModel, BookCopy>().ReverseMap();


            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<ApplicationUser, UserFormViewModel>().ReverseMap();

            CreateMap<Governorate, SelectListItem>().
                ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)).
                ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            CreateMap<Area, SelectListItem>().
                ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id)).
                ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            CreateMap<SubscriberFormViewModel, Subscriber>().ReverseMap();
            CreateMap<Subscriber, SubscriberSearchResultViewModel>()
               .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

            CreateMap<Subscriber, SubscriberViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<Subscribtion, SubscriptionViewModel>();
            //Rental

            CreateMap<Rental, RentalViewModel>()
                ;
            CreateMap<RentalCopy, RentalCopyViewModel>();

            CreateMap<RentalCopy, RentalHistoryViewModel>()
                .ForMember(dest => dest.Subscriber, opt => opt.MapFrom(src => $"{src.Rentals!.Subscriber!.FirstName} {src.Rentals.Subscriber.LastName}"))
                .ForMember(dest => dest.MobileNumber, opt => opt.MapFrom(src => src.Rentals!.Subscriber!.MobileNumber));
        }
    }
}
