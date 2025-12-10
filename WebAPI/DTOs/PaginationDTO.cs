namespace WebAPI.DTOs
{
    public record PaginationDTO(int Page, int RecordsPerPage = 10)
    {
        private const int MaximunRecordsQtyPerPage = 50;

        public int Page { get; init; } = Math.Max(1, Page);
        public int RecordsPerPage { get; init; } = Math.Clamp(RecordsPerPage, 1, MaximunRecordsQtyPerPage);
    }
}
