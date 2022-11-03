using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMonUI : MonoBehaviour
{
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] HP hp;

    [SerializeField] Color highlight;

    Pokemon mon;

    public void SetHud(Pokemon pokemon)
    {
        mon = pokemon;
        name.text = mon.Basic.Name;
        level.text = "Lv. " + mon.Level;
        hp.SetHp((float)mon.currentHp / mon.MaxHp);
    }

    public void Selected(bool selected)
    {
        if (selected)
            name.color = highlight;
        else
            name.color = Color.black;
    }
    
}
