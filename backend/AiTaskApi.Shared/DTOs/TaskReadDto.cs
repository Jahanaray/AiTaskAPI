namespace AiTaskApi.Shared.DTOs
{
    public class TaskReadDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public string Priority { get; set; } = "Medium";
        public string Status { get; set; } = "Todo";

        public DateTime? DueDate { get; set; }
    }
}