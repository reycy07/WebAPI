using AutoMapper;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Utilities
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Author, AuthorDTO>()
                .ForMember(dto => dto.FullName,
                config => config.MapFrom(author => MappFullName(author)));

            CreateMap<Author, AuthorWithBooksDTO>()
                .ForMember(dto => dto.FullName,
                config => config.MapFrom(author => MappFullName(author)));

            CreateMap<AuthorCreateDTO, Author>();
            CreateMap<Author, AuthorPatchDTO>().ReverseMap();
            

            CreateMap<Book, BookDTO>();
            CreateMap<Book, BookWithAuthorDTO>()
                .ForMember(dto => dto.AuthorName,
                config => config.MapFrom(ent => MappFullName(ent.Author!)));
            CreateMap<BookCreateDTO, Book>();

        }
        private string MappFullName(Author author)
        {
            return $"{author.Names} {author.LastNames}";
        }
    }
}
