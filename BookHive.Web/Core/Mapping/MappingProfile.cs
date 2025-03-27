using AutoMapper;
using BookHive.Web.Core.Models;
using BookHive.Web.Core.ViewModels;
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
                CreateMap<Category,CategoryViewModel>();
                CreateMap<CategoryFormViewModel, Category>().ReverseMap();
                CreateMap<Category,SelectListItem>().
                ForMember(dest=>dest.Value,opt=>opt.MapFrom(src=>src.Id)).
                ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));
                
                    //Authors
                    CreateMap<Author,AuthorViewModel>();
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
          ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book!.Title));

           
            CreateMap<BookCopyFormViewModel, BookCopy>().ReverseMap();


            CreateMap<ApplicationUser, UserViewModel>();

            CreateMap<ApplicationUser, UserFormViewModel>().ReverseMap();



        }
    }
}
