using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] Text status;
    [SerializeField] HP hp;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon mon;

    Dictionary<ConditionID, Color> statusColor;

    public void SetHud(Pokemon pokemon)
    {
        mon = pokemon;
        name.text = pokemon.Basic.Name;
        level.text = "Lv. " + pokemon.Level;
        hp.SetHp((float)pokemon.currentHp / pokemon.MaxHp);

        statusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor }
        };
        SetStatusText();
        mon.OnStatusChange += SetStatusText;
    }

    void SetStatusText()
    {
        if(mon.Status == null)
        {
            status.text = "";
        }
        else
        {
            status.text = mon.Status.id.ToString().ToUpper();
            status.color = statusColor[mon.Status.id];
        }
    }

    public IEnumerator UpdateHp()
    {
        if (mon.HpChange)
        {
            yield return hp.tickHp((float)mon.currentHp / mon.MaxHp);
            mon.HpChange = false;
        }
    }
}
