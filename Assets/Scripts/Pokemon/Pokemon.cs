using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    PokemonBasic basic;
    int level;

    public Pokemon(PokemonBasic pBasic, int pLevel)
    {
        basic = pBasic;
        level = pLevel;
    }

    public int MaxHp
    {
        get { return Mathf.FloorToInt((basic.MaxHp * level) / 100f) + 10; }
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((basic.Attack * level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((basic.Defense * level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((basic.SpAttack * level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((basic.SpDefense * level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((basic.Speed * level) / 100f) + 5; }
    }
}
