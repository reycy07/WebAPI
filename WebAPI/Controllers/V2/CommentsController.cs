using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Services;

namespace WebAPI.Controllers.V2
{
    [ApiController]
    [Route("api/v2/books/{bookId:int}/comments")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly ApplicationDbContext context;
        private readonly IUserService userService;

        public CommentsController(IMapper mapper, ApplicationDbContext context, IUserService userService)
        {
            this.mapper = mapper;
            this.context = context;
            this.userService = userService;
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<CommentDTO>>> Get(int bookId)
        {
            var isExistBook = await context.Books.AnyAsync(x => x.Id == bookId);

            if (!isExistBook) return NotFound();

            var comments = await context.Comments
                .Include(x => x.User)
                .Where(x => x.BookId == bookId)
                .OrderByDescending(x => x.PublishDate)
                .ToListAsync();

            return mapper.Map<List<CommentDTO>>(comments);
        }

        [HttpGet("{id}", Name = "GetCommentV2")]
        [AllowAnonymous]
        public async Task<ActionResult<CommentDTO>> Get(Guid id)
        {
            var comment = await context.Comments.Include(x=>x.User).FirstOrDefaultAsync(x => x.Id == id);

            if (comment == null) return NotFound();

            return mapper.Map<CommentDTO>(comment);
        }

        [HttpPost]

        public async Task<ActionResult> Post(int bookId, CommentCreateDTO commentCreateDTO)
        {
            
            var isExistBook = await context.Books.AnyAsync(x => x.Id == bookId);
            
            if (!isExistBook) return NotFound();

            var user = await userService.GetUser();

            if (user == null) return NotFound();

            var comment = mapper.Map<Comment>(commentCreateDTO);
            comment.BookId = bookId;
            comment.PublishDate = DateTime.UtcNow;
            comment.UserId = user.Id;

            context.Add(comment);
            await context.SaveChangesAsync();
            var commentDTO = mapper.Map<CommentDTO>(comment);
            
            return CreatedAtRoute("GetCommentV2", new { id = comment.Id, bookId }, commentDTO);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> Patch(Guid id, int bookId, JsonPatchDocument<CommentPatchDTO> patchDoc)
        {
            if(patchDoc is null) return BadRequest();

            var bookDB = await context.Books.FirstOrDefaultAsync( x => x.Id == bookId);
            var commetDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            var user = await userService.GetUser();
            if (bookDB is null || commetDB is null || user is null) return NotFound();

            if (commetDB.UserId != user.Id) return Forbid();

            var commentPatchDTO = mapper.Map<CommentPatchDTO>(commetDB);
            patchDoc.ApplyTo(commentPatchDTO, ModelState);
            var IsValidate = TryValidateModel(commentPatchDTO);

            if (!IsValidate) return ValidationProblem();

            mapper.Map(commentPatchDTO, commetDB);

            await context.SaveChangesAsync();

            return NoContent();

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete (Guid id, int bookId)
        {
            var bookDB = await context.Books.FirstOrDefaultAsync(x => x.Id == bookId);
            var commetDB = await context.Comments.FirstOrDefaultAsync(x => x.Id == id);
            var user = await userService.GetUser();

            if (bookDB is null || commetDB is null || user is null) return NotFound();

            if (commetDB.UserId != user.Id) return Forbid();

            commetDB.IsDeleted = true;
            context.Update(commetDB);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
 }
