using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Create new move")]

public class MoveBasic : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string desc;

    [SerializeField] type type1;

    [SerializeField] int power;

    [SerializeField] int acc;
    [SerializeField] bool cantMiss;
    [SerializeField] int pp;
    [SerializeField] int priority;

    [SerializeField] Category category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List<SecondaryEffects> secondaries;
    [SerializeField] Target target;

    public string Name
    {
        get { return name; }
    }
    public string Desc
    {
        get { return desc; }
    }
    public type Type1
    {
        get { return type1; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Acc
    {
        get { return acc; }
    }
    public bool CantMiss
    {
        get { return cantMiss; }
    }
    public int Pp
    {
        get { return pp; }
    }
    public int Priority
    {
        get { return priority; }
    }
    public Category Category
    {
        get { return category; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }
    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }
    public Target Target
    {
        get { return target; }
    }


    // Using gen I-III physical special split #### NO LONGER USED ####
    //public bool IsSpecial
    //{
    //    get
    //    {
    //        if(type1 == type.Fire || type1 == type.Water || type1 == type.Grass || type1 == type.Ice || type1 == type.Electric || type1 == type.Psychic || type1 == type.Dragon || type1 == type.Dark || type1 == type.Fairy)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //}
}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] List<StatBoost> boosts;
    [SerializeField] ConditionID status;
    [SerializeField] ConditionID dynamicStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }
    public ConditionID DynamicStatus
    {
        get { return dynamicStatus; }
    }

}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] int chance;
    [SerializeField] Target target;

    public int Chance
    {
        get { return chance; }
    }
    public Target Target
    {
        get { return target; }
    }
}


[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}

public enum Category
{
    Physical, Special, Status
}

public enum Target
{
    Enemy, Self
}

