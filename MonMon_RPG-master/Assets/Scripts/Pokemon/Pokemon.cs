using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]

public class Pokemon
{

    [SerializeField] PokemonBasic _base;
    [SerializeField] int level;

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

    public int currentHp { get; set; }

    public List<Move> Moves { get; set; }

    public void Init()
    {
        currentHp = MaxHp;

        //creating moves based on level
        Moves = new List<Move>();
        foreach (var move in Basic.LearnableMoves)
        {
            if (move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));
            }
            if (Moves.Count >= 4)
            {
                break;
            }
        }
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((Basic.MaxHp * Level) / 100f) + 10; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((Basic.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Basic.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Basic.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Basic.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Basic.Speed * Level) / 100f) + 5; }
    }


    // found at https://bulbapedia.bulbagarden.net/wiki/Damage
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
        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;


        float modifiers = Random.Range(0.85f, 1f) * type * crit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            damageDets.Fainted = true;
        }

        return damageDets;
    }

    public Move randomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

public class DamageDets
{
    public bool Fainted { get; set; }

    public float Crit { get; set; }

    public float Type { get; set; }
}
