using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/authors-collection")]
    [Authorize]
    public class AuthorsCollectionController: ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public AuthorsCollectionController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet("{ids}", Name ="GetAuthorsByIds")]
        public async Task<ActionResult<List<AuthorWithBooksDTO>>> GET(string ids)
        {
            var idsCollection = new List<int>();

            foreach (var id in ids.Split(","))
            {
                if(int.TryParse(id, out int idInt))
                {
                    idsCollection.Add(idInt);
                }

                if (!idsCollection.Any())
                {
                    ModelState.AddModelError(nameof(ids), "No se encontraron ids");
                    return ValidationProblem();
                }
            }

            var authors = await context.Authors
                .Include(x => x.Books)
                    .ThenInclude(x => x.Book)
                .Where(x => idsCollection
                .Contains(x.Id))
                .ToListAsync();

            if (authors.Count() != idsCollection.Count()) return NotFound();

            var authorsDTO = mapper.Map<List<AuthorWithBooksDTO>>(authors);

            return authorsDTO;
        }

        [HttpPost]
        public async Task<ActionResult> Post(IEnumerable<AuthorCreateDTO> authorsCreateDTO)
        {
            var authors = mapper.Map<IEnumerable<Author>>(authorsCreateDTO);
            context.AddRange(authors);
            await context.SaveChangesAsync();

            var authoresDTO = mapper.Map<IEnumerable<AuthorDTO>>(authors);
            var ids = authors.Select(x => x.Id);
            var idStrings = string.Join(",", ids);
            return CreatedAtRoute("GetAuthorsByIds", new {ids = idStrings}, authoresDTO);
        }
    }
}
