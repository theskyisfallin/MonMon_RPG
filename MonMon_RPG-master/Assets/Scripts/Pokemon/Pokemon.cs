using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pokemon
{
    public PokemonBasic Basic { get; set; }
    public int Level { get; set; }

    public int currentHp { get; set; }

    public List<Move> Moves { get; set; }

    public Pokemon(PokemonBasic pBasic, int pLevel)
    {
        Basic = pBasic;
        Level = pLevel;
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

        float modifiers = Random.Range(0.85f, 1f) * type * crit;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / Defense) + 2;
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
