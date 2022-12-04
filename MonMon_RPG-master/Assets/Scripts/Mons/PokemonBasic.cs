using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// be able to create scriptable obejcts of MonMon type in Unity
[CreateAssetMenu(fileName = "MonMon", menuName = "MonMon/Create new MonMon")]

public class PokemonBasic : ScriptableObject
{
    // user input in unity
    [SerializeField] string name;

    [TextArea]

    [SerializeField] string desc;

    [SerializeField] Sprite front;

    [SerializeField] Sprite back;

    [SerializeField] type type1;
    [SerializeField] type type2;

    //stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    // exp stats
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    // catch rate
    [SerializeField] int catchRate = 255;

    // moves
    [SerializeField] List<Learnable> learableMoves;
    [SerializeField] List<MoveBasic> learnableByItems;

    // started workin on evolutions before realizing I didn't really have time so stopped
    //[SerializeField] List<Evolution> evolutions;

    // 4 is max moves
    public static int MaxNumOfMoves { get; set; } = 4;

    // calculates how much exp is needed to level for each growth rate type of the monmons
    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return (6 / 5) * (level * level * level) - (15 * (level * level)) + 100 * level - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return (5 * (level * level * level)) / 4;
        }

        return -1;
    }

    // expose vars
    public string Name
    {
        get { return name; }
    }
    public string Desc
    {
        get { return desc; }
    }
    public Sprite Front
    {
        get { return front; }
    }
    public Sprite Back
    {
        get { return back; }
    }
    public type Type1
    {
        get { return type1; }
    }
    public type Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<Learnable> LearnableMoves
    {
        get { return learableMoves; }
    }
    public List<MoveBasic> LearnableByItems
    {
        get { return learnableByItems; }
    }
    public List<Evolution> Evolutions => evolutions;
    public int CatchRate
    {
        get { return catchRate; }
    }
    public int ExpYield
    {
        get { return expYield; }
    }
    public GrowthRate GrowthRate
    {
        get { return growthRate; }
    }
}

// gets the learnable moves of the monmon
[System.Serializable]
public class Learnable
{
    [SerializeField] MoveBasic moveBase;
    [SerializeField] int level;

    public MoveBasic Base
    {
        get { return moveBase; }
    }
    public int Level
    {
        get { return level; }
    }
}

//[System.Serializable]
//public class Evolution
//{
//    [SerializeField] PokemonBasic evoInto;
//    [SerializeField] int evoLvl;

//    public PokemonBasic EvoInto => evoInto;
//    public int EvoLvl => evoLvl;
//}

// typing list of the monmons
public enum type
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Posion,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

// list of the growth rates
public enum GrowthRate
{
    Fast, MediumFast, MediumSlow, Slow
}

// list of the stats
public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,
    Accuracy,
    Evasion
}

// type chart used for type effectiveness
public class TypeChart
{
    static float[][] chart =
    {
        //                  Nor Fir Wat Ele Gra Ice Fig Poi Gro Fly Psy Bug Roc   Gho Dra Dar Ste   Fai
        /*Nor*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 0f, 1f, 1f, 0.5f, 1f},
        /*Fir*/new float[] {1f, 0.5f, 0.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 1f, 2f, 1f},
        /*Wat*/new float[] {1f, 2f, 0.5f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f, 1f},
        /*Ele*/new float[] {1f, 1f, 2f, 0.5f, 0.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f},
        /*Gra*/new float[] {1f, 0.5f, 2f, 1f, 0.5f, 1f, 1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 0.5f, 1f, 0.5f, 1f},
        /*Ice*/new float[] {1f, 0.5f, 0.5f, 1f, 2f, 0.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f},
        /*Fig*/new float[] {2f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f, 0.5f, 0.5f, 2f, 0f, 1f, 2f, 2f, 0.5f},
        /*Poi*/new float[] {1f, 1f, 1f, 1f, 2f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 1f, 0.5f, 0.5f, 1f, 1f, 0f, 2f},
        /*Gro*/new float[] {1f, 2f, 1f, 2f, 0.5f, 1f, 1f, 2f, 1f, 0f, 1f, 0.5f, 2f, 1f, 1f, 1f, 2f, 1f},
        /*Fly*/new float[] {1f, 1f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 0.5f, 1f},
        /*Psy*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f, 0.5f, 1f, 1f, 1f, 1f, 0f, 0.5f, 1f},
        /*Bug*/new float[] {1f, 0.5f, 1f, 1f, 2f, 1f, 0.5f, 0.5f, 1f, 0.5f, 2f, 1f, 1f, 0.5f, 1f, 2f, 0.5f, 0.5f},
        /*Roc*/new float[] {1f, 2f, 1f, 1f, 1f, 2f, 0.5f, 1f, 0.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 0.5f, 1f},
        /*Gho*/new float[] {0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 1f},
        /*Dra*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 0.5f, 0f},
        /*Dar*/new float[] {1f, 1f, 1f, 1f, 1f, 1f, 0.5f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f, 0.5f, 1f, 0.5f},
        /*Ste*/new float[] {1f, 0.5f, 0.5f, 0.5f, 1f, 2f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 0.5f, 2f},
        /*Fai*/new float[] {1f, 0.5f, 1f, 1f, 1f, 1f, 2f, 0.5f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 0.5f, 1f}
    };

    // gets the value stored in the 2D array for the type effectiveness
    public static float GetEffect(type attackType, type defenseType)
    {
        if (attackType == type.None || defenseType == type.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}