using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMonUI : MonoBehaviour
{
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] HP hp;
    [SerializeField] Text message;

    Pokemon mon;

    public void Init(Pokemon pokemon)
    {
        mon = pokemon;
        UpdateData();

        mon.OnHpChange += UpdateData;
        SetMessage("");
    }

    void UpdateData()
    {
        name.text = mon.Basic.Name;
        level.text = "Lv. " + mon.Level;
        hp.SetHp((float)mon.currentHp / mon.MaxHp);
    }

    public void Selected(bool selected)
    {
        if (selected)
            name.color = GlobalSettings.i.Hightlight;
        else
            name.color = Color.black;
    }

    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
