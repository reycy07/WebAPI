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
            CreateMap<AuthorBook, BookDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.BookId))
                .ForMember(dto => dto.Title, config => config.MapFrom(ent => ent.Book!.Title));
            CreateMap<Author, AuthorPatchDTO>().ReverseMap();
            

            CreateMap<Book, BookDTO>();
            CreateMap<BookCreateDTO, Book>()
                .ForMember(ent => ent.Authors, config => config.MapFrom(dto => dto.AuthorsIds.Select(id => new AuthorBook {AuthorId = id })));

            CreateMap<Book, BookWithAuthorsDTO>();

            CreateMap<BookCreateDTO, AuthorBook>()
                .ForMember(ent => ent.Book, config => config.MapFrom(dto => new Book { Title = dto.Title }));

            CreateMap<AuthorBook, AuthorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(ent => ent.AuthorId))
                .ForMember(dto => dto.FullName, config => config.MapFrom(ent => MappFullName(ent.Author!)));

            CreateMap<CommentCreateDTO, Comment>();
            CreateMap<Comment, CommentDTO>();
            CreateMap<CommentPatchDTO, Comment>().ReverseMap();


        }
        private string MappFullName(Author author)
        {
            return $"{author.Names} {author.LastNames}";
        }
    }
}
