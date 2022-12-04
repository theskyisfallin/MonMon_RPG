using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// be able to create the scriptable object in unity
[CreateAssetMenu(menuName = "Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    // user input from unity
    [SerializeField] MoveBasic move;
    [SerializeField] bool isHM;

    // overrides the name from the interface with the base name of the item
    public override string Name => base.Name + $": {move.Name}";

    public override bool Use(Pokemon mon)
    {
        // Learning Move handled from InventoryUI, if learned return true
        return mon.HasMove(move);
    }

    // checks if the monmon can be taught the move you are trying to teach
    public bool CanBeTaught(Pokemon mon)
    {
        return mon.Basic.LearnableByItems.Contains(move);
    }

    // if it is an HM it is reusable
    public override bool IsReusable => isHM;

    // cannot be used in battle
    public override bool CanUseInBattle => false;

    public MoveBasic Move => move;

    public bool IsHM => isHM;
}
