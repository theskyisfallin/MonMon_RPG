using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    [SerializeField] MoveBasic move;
    [SerializeField] bool isHM;

    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon mon)
    {
        // Learning Move handled from InventoryUI, if learned return true
        return mon.HasMove(move);
    }

    public bool CanBeTaught(Pokemon mon)
    {
        return mon.Basic.LearnableByItems.Contains(move);
    }
    
    public override bool IsReusable => isHM;

    public override bool CanUseInBattle => false;

    public MoveBasic Move => move;

    public bool IsHM => isHM;
}
