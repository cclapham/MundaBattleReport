namespace MundaBattleReport.Models;

public class MundaFighter
{
    public string Id { get; set; } = "";
    public string FighterName { get; set; } = "";
    public string FighterType { get; set; } = "";
    public string FighterClass { get; set; } = "";
    public int Credits { get; set; }

    public int Movement { get; set; }
    public int WeaponSkill { get; set; }
    public int BallisticSkill { get; set; }
    public int Strength { get; set; }
    public int Toughness { get; set; }
    public int Wounds { get; set; }
    public int Initiative { get; set; }
    public int Attacks { get; set; }
    public int Leadership { get; set; }
    public int Cool { get; set; }
    public int Willpower { get; set; }
    public int Intelligence { get; set; }

    public List<MundaWeapon> Weapons { get; set; } = [];
    public List<MundaWargear> Wargear { get; set; } = [];
    public MundaEffects Effects { get; set; } = new();
}

public class MundaWeapon
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}

public class MundaWargear
{
    public string Name { get; set; } = "";
}

public class MundaEffects
{
    public List<string> Active { get; set; } = [];
}
