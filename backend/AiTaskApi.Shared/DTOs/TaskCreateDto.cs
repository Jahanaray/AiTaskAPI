using System.ComponentModel.DataAnnotations;

public class TaskCreateDto
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }

    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Todo";

    public DateTime? DueDate { get; set; }
}