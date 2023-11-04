
using System.Linq;

public enum DamageTypes
{
    #region Elements
    Fire, Ice, Water, Earthen, Lightning, Air, 
    #endregion

    #region Weapons
    Blunt, Slashing, Piercing, 
    #endregion

    #region Misc
    Psycic, Dark, Holy, Corruption, Necrotic, Force,
    #endregion

    #region Nature
    Poison, Radiation, Rot, Acid, Sound, Radiant, 
    #endregion
}

public enum HealingTypes
{
    Arcane, Medicinal, Holy, Natural, Mechanical
}

public class DamageTypeUtility
{
    public static DamageTypes[] AllTypes = new DamageTypes[]
    {
        DamageTypes.Fire, DamageTypes.Ice, DamageTypes.Water, DamageTypes.Earthen, DamageTypes.Lightning, DamageTypes.Air,
        DamageTypes.Blunt, DamageTypes.Slashing, DamageTypes.Piercing, DamageTypes.Psycic, DamageTypes.Dark, 
        DamageTypes.Holy, DamageTypes.Corruption, DamageTypes.Necrotic, DamageTypes.Force, DamageTypes.Poison, 
        DamageTypes.Radiation, DamageTypes.Rot, DamageTypes.Acid, DamageTypes.Sound, DamageTypes.Radiant, 
    };

    public static int GetNextDamageTypeIndex(DamageTypes damageType)
    {
        if (damageType == AllTypes[AllTypes.Length -1])
            return 0;
        else return AllTypes.ToList().IndexOf(damageType) +1;
    }
    public static DamageTypes GetNextDamageType(DamageTypes damageType)
    {
        if (damageType == AllTypes[AllTypes.Length -1])
            return AllTypes[0];
        else return AllTypes[AllTypes.ToList().IndexOf(damageType) +1];
    }

    public static int GetPreviousDamageTypeIndex(DamageTypes damageType)
    {
        if (damageType == AllTypes[AllTypes.Length +1])
            return 0;
        else return AllTypes.ToList().IndexOf(damageType) -1;
    }
    public static DamageTypes GetPreviousDamageType(DamageTypes damageType)
    {
        if (damageType == AllTypes[AllTypes.Length +1])
            return AllTypes[0];
        else return AllTypes[AllTypes.ToList().IndexOf(damageType) -1];
    }

    public static DamageTypes GetFirst => AllTypes[0];
    public static DamageTypes GetLast => AllTypes[AllTypes.Length - 1];
    public static int TotalDamageTypes => AllTypes.Length;
}
