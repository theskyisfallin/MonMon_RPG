using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new ball")]
public class BallItem : ItemBase
{
    [SerializeField] float catchRateModifer = 1;

    public override bool Use(Pokemon mon)
    {
        return true;
    }

    public override bool CanUseOutsideBattle => false;

    public float CatchRateModifer => catchRateModifer;
}
