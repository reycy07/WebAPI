using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/authors")]
    public class AuthorsController : ControllerBase
    {
        public readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AuthorsController> logger;


        public AuthorsController(ApplicationDbContext context, IMapper mapper, ILogger<AuthorsController> logger)
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<IEnumerable<AuthorDTO>> Get()
        {
            var authors = await context.Authors
                .Include(x => x.Books)
                .ToListAsync();

            var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return authorsDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(AuthorCreateDTO authorCreateDTO)
        {
            var author = mapper.Map<Author>(authorCreateDTO);
            context.Add(author);
            await context.SaveChangesAsync();
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("GetAuthor", new { id = author.Id}, author);
        }

        [HttpGet("{id:int}", Name = "GetAuthor")]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get(int id)
            {
            var author = await context.Authors
                .Include(x => x.Books )
                    .ThenInclude(x => x.Book)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (author is null)
            {
                return NotFound();
            }
            var authorDTO = mapper.Map<AuthorWithBooksDTO>(author);
            return authorDTO;
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, AuthorCreateDTO authorDTO)
        {
            var author = mapper.Map<Author>(authorDTO);
            author.Id = id;
            context.Update(author);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<AuthorPatchDTO> patchDoc)
        {
            if (patchDoc is null)
            {
                return BadRequest();
            }

            var authorDB = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);


            if (authorDB is null)
            {
                return NotFound();
            }

            var authorPatchDTO = mapper.Map<AuthorPatchDTO>(authorDB);

            patchDoc.ApplyTo(authorPatchDTO, ModelState);

            var IsValidate = TryValidateModel(authorPatchDTO);

            if (!IsValidate)
            {
                return ValidationProblem();
            }
            var autorPatchDTO = mapper.Map(authorPatchDTO, authorDB);

            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var author = await context.Authors.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (author == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

    }
}
