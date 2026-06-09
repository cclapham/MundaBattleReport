using MundaBattleReport.Models;

namespace MundaBattleReport.Services;

public class BattleSessionService
{
    public List<Gang> Gangs { get; } = new();

    public bool CanProceed => Gangs.Count >= 2;

    public void AddGang(Gang gang) => Gangs.Add(gang);

    public void RemoveGang(Guid id) => Gangs.RemoveAll(g => g.Id == id);

    public void Clear() => Gangs.Clear();
}
