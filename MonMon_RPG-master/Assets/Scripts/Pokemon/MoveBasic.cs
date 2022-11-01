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

    [SerializeField] int pp;

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
    public int Pp
    {
        get { return pp; }
    }
}

