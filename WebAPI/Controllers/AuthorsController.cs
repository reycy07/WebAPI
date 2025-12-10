using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Services;
using WebAPI.Utilities;
using System.Linq.Dynamic.Core;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/authors")]
    [Authorize(Policy = "isAdmin")]
    public class AuthorsController : ControllerBase
    {
        public readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ILogger<AuthorsController> logger;
        private readonly IFileStorage fileStorage;
        private const string container = "authors";


        public AuthorsController(
            ApplicationDbContext context, 
            IMapper mapper, 
            ILogger<AuthorsController> logger, 
            IFileStorage fileStorage
            )
        {
            this.context = context;
            this.mapper = mapper;
            this.logger = logger;
            this.fileStorage = fileStorage;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IEnumerable<AuthorDTO>> Get([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = context.Authors.AsQueryable();
            await HttpContext.InsertPaginationParamsInHeader(queryable); 
            var authors = await queryable.OrderBy(x => x.Id).Paginate(paginationDTO).ToListAsync();
            var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
            return authorsDTO;
        }

        [HttpPost]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Post(AuthorCreateDTO authorCreateDTO)
        {
            var author = mapper.Map<Author>(authorCreateDTO);
            context.Add(author);
            await context.SaveChangesAsync();
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("GetAuthor", new { id = author.Id}, author);
        }
        [HttpPost("with-picture")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EndpointSummary("Create Author with picture")]
        [EndpointDescription("Recive Information with picture about author")]

        public async Task<ActionResult> PostWithPicture([FromForm] AuthorCreateWithPictureDTO authorCreateWithPictureDTO)
        {
            var author = mapper.Map<Author>(authorCreateWithPictureDTO);
            if(authorCreateWithPictureDTO.Picture is not null)
            {
                var url = await fileStorage.Store(container, authorCreateWithPictureDTO.Picture);
                author.Picture = url;
            }
            context.Add(author);
            await context.SaveChangesAsync();
            var authorDTO = mapper.Map<AuthorDTO>(author);
            return CreatedAtRoute("GetAuthor", new { id = author.Id }, authorDTO);
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult> Filter([FromQuery] AuthorFilterDTO authorFilterDTO)
        {
            var queryable = context.Authors.AsQueryable();

            if (!string.IsNullOrEmpty(authorFilterDTO.Names))
            {
                queryable = queryable.Where(x => x.Names.Contains(authorFilterDTO.Names));
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.LastNames))
            {
                queryable = queryable.Where(x => x.Names.Contains(authorFilterDTO.LastNames));
            }

            if (authorFilterDTO.IncludeBooks)
            {
                queryable = queryable.Include(x => x.Books).ThenInclude(x => x.Book);
            }

            if (authorFilterDTO.HavePicture.HasValue)
            {
                if (authorFilterDTO.HavePicture.Value)
                {
                    queryable = queryable.Where(x => x.Picture != null);
                }
                else
                {
                    queryable = queryable.Where(x => x.Picture == null);
                }
            }

            if (authorFilterDTO.HaveBooks.HasValue)
            {
                if (authorFilterDTO.HaveBooks.Value)
                {
                    queryable = queryable.Where(x => x.Books.Any());
                }
                else
                {
                    queryable = queryable.Where(x => !x.Books.Any());
                }
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.BookTitle))
            {
                queryable = queryable.Where(x => x.Books
                                            .Any(y => y.Book!.Title.Contains(authorFilterDTO.BookTitle)));
            }

            if (!string.IsNullOrEmpty(authorFilterDTO.FileOrder))
            {
                var orderType = authorFilterDTO.AscOrder ? "ascending" : "descending";

                try
                {
                    queryable = queryable.OrderBy($"{authorFilterDTO.FileOrder} {orderType}");
                }
                catch (Exception ex)
                {
                    queryable = queryable.OrderBy(x => x.Names);
                    logger.LogError(ex.Message, ex);
                }
            }
            else
            {
                queryable = queryable.OrderBy(x => x.Names);
            }

                var authors = await queryable.Paginate(authorFilterDTO.PaginationDTO)
                    .ToListAsync();

            if (authorFilterDTO.IncludeBooks)
            {
                var authorsWithBooksDTO = mapper.Map<IEnumerable<AuthorWithBooksDTO>>(authors);
                return Ok(authorsWithBooksDTO);
            }
            else
            {
                var authorsDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
                return Ok(authorsDTO);
            }

        }


        [HttpGet("{id:int}", Name = "GetAuthor")]
        [AllowAnonymous]
        [EndpointSummary("Get Author by Id")]
        [EndpointDescription("Get author by id, Include its books if not exist author return 404")]
        [ProducesResponseType<AuthorWithBooksDTO>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuthorWithBooksDTO>> Get([Description("Author ID")]int id)
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
        public async Task<ActionResult> Put(int id, [FromForm] AuthorCreateWithPictureDTO authorCreateWithPictureDTO)
        {
            var existAuthor = await context.Authors.AnyAsync(x => x.Id == id);

            if (!existAuthor) return NotFound();

            var author = mapper.Map<Author>(authorCreateWithPictureDTO);
            author.Id = id;

            if(authorCreateWithPictureDTO.Picture is not null)
            {
                var currentlyPicture = await context
                                        .Authors.Where(x => x.Id == id)
                                        .Select(x => x.Picture)
                                        .FirstAsync();

                var url = await fileStorage.Edit(currentlyPicture, container, authorCreateWithPictureDTO.Picture);
                author.Picture = url;
            }

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
            var author = await context.Authors.FirstOrDefaultAsync(x => x.Id == id);

            if (author is null) return NotFound();

            context.Remove(author);
            await context.SaveChangesAsync();
            await fileStorage.Delete(author.Picture, container);

            return NoContent();
        }

    }
}
