using System.Collections.Generic;
using UnityEngine;

public class RuneRoller : MonoBehaviour
{
    [SerializeField] private BaseSpell[] baseSpells;

    [SerializeField] private Catalyst[] cats;

    [SerializeField] private GameObject runeHolder;

    [SerializeField] private DropTable[] CostTypeTable;
    [SerializeField] private DropTable[] CatTable;

    public Item RollRune()
    {
        int chance = Random.Range((int)SpellType.DamageSpell, (int)SpellType.None);

        SpellType spellType;
        AttributesEnum costType;
        CastType castType = CastType.Channelled;
        int damageType;
        int cat_id;
        int level;

        spellType = (SpellType)chance;

        switch (spellType)
        {
            case SpellType.DamageSpell:
                break;
            case SpellType.GolemSpell:
                castType = CastType.Aura;
                break;
            default:
                break;
        }

        chance = Random.Range(GlobalValues.MinRoll, GlobalValues.MaxRoll);

        if (chance < CostTypeTable[0].Chances[0])
        {
            costType = AttributesEnum.Mana;
        }
        else if (chance < CostTypeTable[0].Chances[1])
        {
            costType = AttributesEnum.Health;
        }
        else
        {
            costType = AttributesEnum.Stamina;
        }

        if (spellType == SpellType.DamageSpell)
        {
            chance = Random.Range(0, 2);

            castType = (CastType)chance;
        }
        else if (spellType == SpellType.GolemSpell)
        {
            castType = CastType.Aura;
        }

        chance = Random.Range(0, 4);

        damageType = chance;

        cat_id = damageType * 6;

        chance = Random.Range(GlobalValues.MinRoll, GlobalValues.MaxRoll);

        for (int i = 0; i < CatTable[0].Chances.Length; i++)
        {
            if (chance < CatTable[0].Chances[i])
            {
                cat_id += i;
                break;
            }
        }

        level = Random.Range(1, Player.player.GetLevel() * 4 + 1);

        if (spellType == SpellType.GolemSpell)
        {
            damageType = 0;
            castType = CastType.Channelled;
            cat_id = 0;
        }

        Item rune = CreateRune(spellType, costType, castType, damageType, cat_id, level);

        return rune;
    }

    public Item CreateRune
        (
        SpellType spellType,
        AttributesEnum costType,
        CastType castType,
        int damageType,
        int cat_id,
        int level
        )
    {

        Catalyst cat = cats[cat_id];

        int id = (int)spellType;
        int castId = (int)castType;
        int rarityId = cat_id % 6;

        Item item = Instantiate(runeHolder).GetComponent<Item>();

        RuneHolderStats runeStats = new RuneHolderStats();
        
        SpellStats stats = null;

        DamageType damage;

        float value = 0.0f;


        switch (spellType)
        {
            case SpellType.DamageSpell:
                DamageSpell dRune = item.gameObject.AddComponent<DamageSpell>();

                runeStats.spell = dRune;

                stats = new DamageSpellStats();

                DamageSpellStats statsD = stats as DamageSpellStats;

                statsD.ranges = new List<DamageType>();
                statsD.StatusChances = new List<int>();

                damage = new DamageType(baseSpells[id].Ranges[castId][damageType], cat.CatMultis[damageType]);

                statsD.ranges.Add(damage);

                statsD.StatusChances.Add(60);

                break;
            case SpellType.GolemSpell:

                GolemSpell gRune = item.gameObject.AddComponent<GolemSpell>();

                runeStats.spell = gRune;

                stats = new GolemSpellStats();
                GolemSpellStats statsG = stats as GolemSpellStats;

                castId = 0;
                damage = new DamageType(baseSpells[id].Ranges[castId][damageType], cat.CatMultis[damageType]);

                statsG.range = damage;

                castType = CastType.Aura;

                statsG.number = 1;
                break;
            default:
                break;
        }


        switch (damageType)
        {
            case 0:
                stats.SkillType = SkillType.Geomancy;
                break;
            case 1:
                stats.SkillType = SkillType.Pyromancy;
                break;
            case 2:
                stats.SkillType = SkillType.Astromancy;
                break;
            default:
                stats.SkillType = SkillType.Cryomancy;
                break;
        }

        value = baseSpells[id].Value[castId][damageType];
        value *= 1.25f;

        stats.SpellType = spellType;
        stats.CastType = castType;
        stats.CostType = costType;
        stats.SpellAffect = baseSpells[id].SpellAffects[castId][damageType];
        stats.ManaCost = baseSpells[id].ManaCost[castId][damageType];
        stats.CastRate = baseSpells[id].CastsPerSecond[castId][damageType];

        string name = stats.SpellAffect.name;
        string tempName = "";

        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == '(')
            {
                break;
            }

            tempName += name[i];
        }

        stats.Name = tempName;
        
        runeStats.Name = tempName + " Rune";
        runeStats.Rarity = GlobalValues.rarities[rarityId];

        runeStats.Value = (int)value;

        RuneHolder runeH = item as RuneHolder;

        runeH.SetStats(runeStats);

        runeH.GetSpell().SetStats(stats);

        return item;
    }
}
