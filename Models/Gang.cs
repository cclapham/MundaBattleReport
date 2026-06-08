namespace MundaBattleReport.Models;

public class Gang
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string House { get; set; } = "";
    public string? MmSource { get; set; }
    public string RawData { get; set; } = "[]";
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<MundaFighter> Fighters =>
        System.Text.Json.JsonSerializer.Deserialize<List<MundaFighter>>(RawData) ?? [];
}
