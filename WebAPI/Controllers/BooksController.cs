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
                .Include(x => x.Author)
                .ToListAsync();

            var booksDTO = mapper.Map<IEnumerable<BookDTO>>(books);
            return booksDTO;
        }

        [HttpGet("{id:int}", Name ="GetBook")]
        public async Task<ActionResult<BookWithAuthorDTO>> Get(int id)
        {
            var book = await context.Books
                .Include(x => x.Author)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (book is null)
            {
                return NotFound();
            }
            var bookDTO = mapper.Map<BookWithAuthorDTO>(book);
            return bookDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(BookCreateDTO bookCreateDTO)
        {
            var authorExist = await context.Authors.AnyAsync(x => x.Id == bookCreateDTO.AuthorId);
            if (!authorExist)
            {
                ModelState.AddModelError(nameof(BookCreateDTO.AuthorId), $"El autor de i {bookCreateDTO.AuthorId} no existe");
                return ValidationProblem();
            }
            var book = mapper.Map<Book>(bookCreateDTO);
            context.Books.Add(book);
            await context.SaveChangesAsync();
            var bookDTO = mapper.Map<BookDTO>(book);


            return CreatedAtRoute("GetBook", new {id = book.Id }, bookDTO);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, BookCreateDTO bookDTO)
        {
            var authorExist = await context.Authors.AnyAsync(x => x.Id == bookDTO.AuthorId);
            if (!authorExist)
            {
                return BadRequest($"El autor de id {bookDTO.AuthorId} no existe");
            }
            var book = mapper.Map<Book>(bookDTO);
            book.Id = id;
            Console.WriteLine(book.AuthorId);
            context.Update(book);
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
