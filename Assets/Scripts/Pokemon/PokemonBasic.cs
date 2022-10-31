using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]

public class PokemonBasic : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]

    [SerializeField] string desc;

    [SerializeField] Sprite front;

    [SerializeField] Sprite back;

    [SerializeField] type type1;

    //stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

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
}

public enum type
{
    None,
    Fire,
    Water,
    Grass
}