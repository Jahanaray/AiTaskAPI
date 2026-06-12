public class AiTaskResult
{
    public string Title { get; set; }
    public string Description { get; set; }

    public string? DueDate { get; set; }   // 👈 safer
    public string? Priority { get; set; }
}