public class TaskItem
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public string Priority { get; set; } = "Medium";

    public string Status { get; set; } = "Todo";

    public DateTime? DueDate { get; set; }

    public bool IsDone { get; set; } // optional (you can remove later)
}