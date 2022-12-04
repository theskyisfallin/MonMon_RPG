using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// tried to rename file, too many errors and problems for it to have been worth it

[System.Serializable]

// creaetes the actual monmon
public class Pokemon
{
    // get the base and level
    [SerializeField] PokemonBasic _base;
    [SerializeField] int level;

    // init the monmon
    public Pokemon(PokemonBasic pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        Init();
    }

    public PokemonBasic Basic {
        get
        {
            return _base;
        }
    }
    public int Level {
        get
        {
            return level;
        }
    }

    // stat fields for individual monmons
    public int Exp { get; set; }

    public int currentHp { get; set; }

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }

    public Dictionary<Stat, int> Boosts { get; private set; }

    public Condition Status { get; private set; }
    public int StatusTime { get; set; }
    public Queue<string> StatusChanges { get; private set; }

    public Condition dynamicStatus { get; private set; }
    public int dynamicStatusTime { get; set; }

    public event Action OnStatusChange;
    public event Action OnHpChange;

    // init the monmon
    public void Init()
    {
        //creating moves based on level
        Moves = new List<Move>();
        foreach (var move in Basic.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= PokemonBasic.MaxNumOfMoves)
            {
                break;
            }
        }

        // get exp needed for level up, statues, calculate stats, reset hp, and boosts
        Exp = Basic.GetExpForLevel(Level);

        StatusChanges = new Queue<string>();

        CalcStats();

        currentHp = MaxHp;

        ResetBoost();

        Status = null;
        dynamicStatus = null;
    }

    // create a monmon from teh save data you have
    public Pokemon(PokemonSave saveData)
    {
        _base = PokemonDB.GetObjectViaName(saveData.name);

        currentHp = saveData.hp;
        level = saveData.level;
        Exp = saveData.exp;

        if (saveData.statusId != null)
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        else
            Status = null;

        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalcStats();
        StatusChanges = new Queue<string>();
        ResetBoost();
        dynamicStatus = null;
    }

    // fetch save data from your save
    public PokemonSave GetSaveData()
    {
        var saveData = new PokemonSave()
        {
            name = Basic.name,
            hp = currentHp,
            level = Level,
            exp = Exp,
            statusId = Status?.id,
            moves = Moves.Select(m => m.GetSaveData()).ToList()
        };

        return saveData;
    }

    // stat calculations
    void CalcStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Basic.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Basic.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Basic.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Basic.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Basic.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Basic.MaxHp * Level) / 100f) + 10 + Level;
    }

    // reseting boosts
    void ResetBoost()
    {
        Boosts = new Dictionary<Stat, int>()
        {
            { Stat.Attack, 0 },
            { Stat.Defense, 0 },
            { Stat.SpAttack, 0 },
            { Stat.SpDefense, 0 },
            { Stat.Speed, 0 },
            { Stat.Accuracy, 0 },
            { Stat.Evasion, 0 }
        };
    }

    // get stat when boosted
    int GetStat(Stat stat)
    {
        int temp = Stats[stat];

        int boost = Boosts[stat];
        var boostVal = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
            temp = Mathf.FloorToInt(temp * boostVal[boost]);
        else
            temp = Mathf.FloorToInt(temp / boostVal[-boost]);

        return temp;
    }

    // applys boost in battle from -6 to 6 starts at 0
    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            Boosts[stat] = Mathf.Clamp(Boosts[stat] + boost, -6, 6);

            if (boost > 0)
                StatusChanges.Enqueue($"{Basic.Name}'s {stat} rose!");
            else
                StatusChanges.Enqueue($"{Basic.Name}'s {stat} fell!");

            Debug.Log($"{stat} was boosted {Boosts[stat]}");
        }
    }

    // checks if the monmon levels up
    public bool CheckForLevelUp()
    {
        if (Exp > Basic.GetExpForLevel(level + 1))
        {
            level++;
            return true;
        }

        return false;
    }

    // monmon learns a move
    public void LearnMove(MoveBasic moveToLearn)
    {
        if (Moves.Count > PokemonBasic.MaxNumOfMoves)
            return;

        Moves.Add(new Move(moveToLearn));
    }

    // when the monmon levels up check if it can learn a new move at that level
    public Learnable GetLearnableMoveAtCurrent()
    {
        return Basic.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    // check if the monmon already has a move
    public bool HasMove(MoveBasic moveToCheck)
    {
        return Moves.Count(m => m.Base == moveToCheck) > 0;
    }

    // exposing vars
    public int MaxHp { get; private set; }
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }


    // found at https://bulbapedia.bulbagarden.net/wiki/Damage
    // damage calculations
    public DamageDets TakeDamage(Move move, Pokemon attacker)
    {
        float crit = 1;

        if (Random.value * 100 <= 6.25f)
            crit = 2;


        float type = TypeChart.GetEffect(move.Base.Type1, this.Basic.Type1) * TypeChart.GetEffect(move.Base.Type1, this.Basic.Type2);

        var damageDets = new DamageDets()
        {
            Type = type,
            Crit = crit,
            Fainted = false
        };


        //Checks if move is physical or special
        float attack = (move.Base.Category == Category.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == Category.Special) ? SpDefense : Defense;

        // adds in modifiers
        float modifiers = Random.Range(0.85f, 1f) * type * crit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        DecreaseHp(damage);

        return damageDets;
    }

    // decreases hp by the damage done and shows in hp ui
    public void DecreaseHp(int damage)
    {
        currentHp = Mathf.Clamp(currentHp - damage, 0, MaxHp);
        OnHpChange?.Invoke();
    }

    // increases hp by amount healing and shows in ui
    public void IncreaseHp(int amount)
    {
        currentHp = Mathf.Clamp(currentHp + amount, 0, MaxHp);
        OnHpChange?.Invoke();
    }

    // set statues when a change happens
    public void SetStatus(ConditionID ID)
    {
        if(Status != null)
        {
            return;
        }

        Status = ConditionsDB.Conditions[ID];

        // DO NOT REMOVE ?s CAUSES CRASH IF IT IS NULL
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Basic.Name} {Status.StartMess}");

        OnStatusChange?.Invoke();
    }

    // cure statues
    public void CureStatus()
    {
        Status = null;
        OnStatusChange?.Invoke();
    }

    // sets the dynamic status when used
    public void SetDynamicStatus(ConditionID ID)
    {
        if (dynamicStatus != null)
        {
            return;
        }

        dynamicStatus = ConditionsDB.Conditions[ID];

        // DO NOT REMOVE ?s CAUSES CRASH IF IT IS NULL
        dynamicStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Basic.Name} {dynamicStatus.StartMess}");
    }

    // cures dynamic status
    public void CureDynamicStatus()
    {
        dynamicStatus = null;
    }

    // gets the random move the enemy monmons use
    public Move randomMove()
    {
        var movesWithPP = Moves.Where(x => x.Pp > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    // runs statuses that effect monmons before moves
    public bool OnBeforeMove()
    {
        bool canMove = true;
        if(Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
                canMove = false;
        }
        if (dynamicStatus?.OnBeforeMove != null)
        {
            if (!dynamicStatus.OnBeforeMove(this))
                canMove = false;
        }


        return canMove;
    }

    // runs statuses after monmon moves
    public void OnAfterTurn()
    {
        // Second ? is null conditional operator (it's pretty neat ALSO DO NOT REMOVE CAUSES CRASH)
        Status?.OnAfterTurn?.Invoke(this);
        dynamicStatus?.OnAfterTurn?.Invoke(this);
    }

    // on battle over get rid of dynamic statuses and reset boosts
    public void OnBattleOver()
    {
        dynamicStatus = null;
        ResetBoost();
    }
}

// expose damage details
public class DamageDets
{
    public bool Fainted { get; set; }

    public float Crit { get; set; }

    public float Type { get; set; }
}

// save data
[System.Serializable]
public class PokemonSave
{
    public string name;
    public int hp;
    public int level;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSave> moves;
}