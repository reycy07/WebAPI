using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksControllerL : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public BooksControllerL(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<BookDTO>> Get()
        {
            var books = await context.Books
                .Include(x => x.Authors)
                .ToListAsync();

            var booksDTO = mapper.Map<IEnumerable<BookDTO>>(books);
            return booksDTO;
        }

        [HttpGet("{id:int}", Name ="GetBook")]
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

        [HttpPost]
        public async Task<ActionResult> Post(BookCreateDTO bookCreateDTO)
        {
           
            if (bookCreateDTO.AuthorsIds is null || bookCreateDTO.AuthorsIds.Count == 0)
            {
                ModelState.AddModelError(nameof(BookCreateDTO.AuthorsIds), $"No se puede crear un libro sin autores");
                return ValidationProblem();
            }

            var authorsExist = await context.Authors
                                .Where(x => bookCreateDTO.AuthorsIds.Contains(x.Id))
                                .Select(x => x.Id)
                                .ToListAsync();

            if (authorsExist.Count != bookCreateDTO.AuthorsIds.Count)
            {
                var authorsNoExist = bookCreateDTO.AuthorsIds.Except(authorsExist);
                var authorsNoExsitString = string.Join(",", authorsNoExist);
                var errorMessage = $"Los siguientes autores no exsiten: {authorsNoExsitString}";

                ModelState.AddModelError(nameof(bookCreateDTO.AuthorsIds), errorMessage);
                return ValidationProblem();
            }

            var book = mapper.Map<Book>(bookCreateDTO);
            AssignAuthorOrder(book);

            context.Books.Add(book);
            await context.SaveChangesAsync();
            var bookDTO = mapper.Map<BookDTO>(book);

            return CreatedAtRoute("GetBook", new { id = book.Id }, bookDTO);
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

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, BookCreateDTO bookCreateDTO)
        {
            var authorsExist = await context.Authors
                .Where(x => bookCreateDTO.AuthorsIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            if (authorsExist.Count != bookCreateDTO.AuthorsIds.Count)
            {
                var authorsNoExist = bookCreateDTO.AuthorsIds.Except(authorsExist);
                var authorsNoExsitString = string.Join(",", authorsNoExist);
                var errorMessage = $"Los siguientes autores no exsiten: {authorsNoExsitString}";

                ModelState.AddModelError(nameof(bookCreateDTO.AuthorsIds), errorMessage);
                return ValidationProblem();
            }
            
            var bookDB = await context.Books
                        .Include(x => x.Authors)
                        .FirstOrDefaultAsync(x=>x.Id == id);

            if (bookDB is null) return NotFound();

            bookDB = mapper.Map(bookCreateDTO, bookDB);
            AssignAuthorOrder(bookDB);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
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
