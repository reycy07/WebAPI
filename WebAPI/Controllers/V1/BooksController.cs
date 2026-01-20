using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Utilities;

namespace WebAPI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/books")]
    [Authorize]
    [Authorize(Policy = "isAdmin")]
    public class BooksController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public BooksController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet(Name = "GetBooksV1")]
        [AllowAnonymous]
        public async Task<IEnumerable<BookDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = context.Books.AsQueryable();
            await HttpContext.InsertPaginationParamsInHeader(queryable);
            var books = await queryable
                    .OrderBy(x => x.Id)
                    .Paginate(paginationDTO)
                    .ToListAsync();

            var booksDTO = mapper.Map<IEnumerable<BookDTO>>(books);
            return booksDTO;
        }

        [HttpGet("{id:int}", Name ="GetBookV1")]
        [AllowAnonymous]
        public async Task<ActionResult<BookWithAuthorsDTO>> Get(int id)
        {
            var book = await context.Books
                .Include(x => x.Authors)
                .ThenInclude(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (book is null)
            {
                return NotFound();
            }
            var bookDTO = mapper.Map<BookWithAuthorsDTO>(book);
            return bookDTO;
        }

        [HttpPost(Name = "CreateBookV1")]
        [ServiceFilter<ValidationFilter>]

        public async Task<ActionResult> Post(BookCreateDTO bookCreateDTO)
        {

            var book = mapper.Map<Book>(bookCreateDTO);
            AssignAuthorOrder(book);

            context.Books.Add(book);
            await context.SaveChangesAsync();
            var bookDTO = mapper.Map<BookDTO>(book);

            return CreatedAtRoute("GetBookV1", new { id = book.Id }, bookDTO);
        }

        private void AssignAuthorOrder(Book book)
        {
            if(book.Authors is not null)
            {
                for (int i =0; i < book.Authors.Count; i++)
                {
                    book.Authors[i].Order = i;
                }
            }
        }

        [HttpPut("{id:int}", Name = "UpdateBookV1")]
        [ServiceFilter<ValidationFilter>]
        public async Task<ActionResult> Put(int id, BookCreateDTO bookCreateDTO)
        {
            
            var bookDB = await context.Books
                        .Include(x => x.Authors)
                        .FirstOrDefaultAsync(x=>x.Id == id);

            if (bookDB is null) return NotFound();

            bookDB = mapper.Map(bookCreateDTO, bookDB);
            AssignAuthorOrder(bookDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name ="DeleteBookV1")]
        public async Task<ActionResult> Delete(int id)
        {
            var deleteRegisters = await context.Books.Where(x => x.Id == id).ExecuteDeleteAsync();

            if(deleteRegisters == 0)
            {
                return NotFound();
            }

            return NoContent();

        }
    }
}