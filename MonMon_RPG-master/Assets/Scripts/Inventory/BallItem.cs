using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// how to create a new ball item in unity
[CreateAssetMenu(menuName = "Items/Create new ball")]
public class BallItem : ItemBase
{
    // default catch rate is 1 but can be changed in unity
    [SerializeField] float catchRateModifer = 1;

    public override bool Use(Pokemon mon)
    {
        return true;
    }

    // don't allow to be used outside battle
    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifer => catchRateModifer;
}
