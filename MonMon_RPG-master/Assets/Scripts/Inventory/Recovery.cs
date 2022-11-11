using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class Recovery : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHp;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAll;

    [Header("Fainted")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Pokemon mon)
    {
        // revives
        if (revive || maxRevive)
        {
            if (mon.currentHp > 0)
                return false;

            if (revive)
                mon.IncreaseHp(mon.MaxHp / 2);
            else
                mon.IncreaseHp(mon.MaxHp);

            mon.CureStatus();

            return true;
        }
        // No other items can be used on fainted Mons
        if (mon.currentHp == 0)
            return false;

        // restore Hp
        if (restoreMaxHp || hpAmount > 0)
        {
            if (mon.currentHp == mon.MaxHp)
                return false;

            if (restoreMaxHp)
                mon.IncreaseHp(mon.MaxHp);
            else
                mon.IncreaseHp(hpAmount);
        }

        // recover status
        if (recoverAll || status != ConditionID.none)
        {
            if (mon.Status == null && mon.dynamicStatus == null)
                return false;

            if (recoverAll)
            {
                mon.CureStatus();
                mon.CureDynamicStatus();
            }
            else
            {
                if (mon.Status.id == status)
                    mon.CureStatus();
                else if (mon.dynamicStatus.id == status)
                    mon.CureDynamicStatus();
                else
                    return false;
            }
        }

        // restore power points
        if (restoreMaxPP)
        {
            mon.Moves.ForEach(m => m.IncreasePp(m.Base.Pp));
        }
        else if (ppAmount > 0)
        {
            mon.Moves.ForEach(m => m.IncreasePp(ppAmount));
        }

        return true;
    }
}
