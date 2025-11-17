using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/books/{bookId:int}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ApplicationDbContext context;

        public CommentsController(IMapper mapper, ApplicationDbContext context)
        {
            this.mapper = mapper;
            this.context = context;
        }
        [HttpGet]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var isExistBook = await context.Books.AnyAsync(x => x.Id == bookId);

            if (!isExistBook) return NotFound();

            var comments = await context.Comments
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.PublishDate)
                .ToListAsync();

            return mapper.Map<List<CommentDTO>>(comments);
        }

        [HttpGet("{id}", Name = "GetComment")]

        public async Task<ActionResult<CommentDTO>> Get(Guid id)
        {
            var comment = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null) return NotFound();

            return mapper.Map<CommentDTO>(comment);
        }

        [HttpPost]

        public async Task<ActionResult> Post(int bookId, CommentCreateDTO commentCreateDTO)
        {
            var isExistBook = await context.Books.AnyAsync(x => x.Id == bookId);
            if (!isExistBook) return NotFound();

            var comment = mapper.Map<Comment>(commentCreateDTO);
            comment.BookId = bookId;
            comment.PublishDate = DateTime.UtcNow;

            context.Add(comment);
            await context.SaveChangesAsync();
            var commentDTO = mapper.Map<CommentDTO>(comment);

            return CreatedAtRoute("GetComment", new { id = comment.Id, bookId }, comment);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int bookId, JsonPatchDocument<CommentPatchDTO> patchDoc)
        {
            if(patchDoc is null) return BadRequest();

            var bookDB = await context.Books.FirstOrDefaultAsync( x => x.Id == bookId);
            var commetDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if ((bookDB is null) || (commetDB is null)) return NotFound();

            var commentPatchDTO = mapper.Map<CommentPatchDTO>(commetDB);
            patchDoc.ApplyTo(commentPatchDTO, ModelState);
            var IsValidate = TryValidateModel(commentPatchDTO);

            if (!IsValidate) return ValidationProblem();

            mapper.Map(commentPatchDTO, commetDB);

            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete]
        public async Task<ActionResult> Delete (Guid id, int bookId)
        {
            var bookDB = await context.Books.FirstOrDefaultAsync(x => x.Id == bookId);
            var commetDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);

            if ((bookDB is null) || (commetDB is null)) return NotFound();

            var deletedRecord = await context.Comments.Where(x => x.Id == id).ExecuteDeleteAsync();
            if (deletedRecord == 0) return NotFound();

            return NoContent();
        }
    }
 }
