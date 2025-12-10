namespace WebAPI.DTOs
{
    public class AuthorFilterDTO
    {
        public int Page { get; set; } = 1;
        public int RecordPerPage { get; set; } = 10;
        public PaginationDTO PaginationDTO { get
            {
                return new PaginationDTO(Page, RecordPerPage);
            } 
        }
        public string? Names { get; set; }
        public string? LastNames { get; set; }
        public bool? HavePicture { get; set; }
        public bool? HaveBooks { get; set; }
        public string? BookTitle { get; set; }
        public bool IncludeBooks { get; set; }
        public string? FileOrder { get; set; }
        public bool AscOrder { get; set; } = true;

    }
}
