public class MaterialLink
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid HomeworkId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}