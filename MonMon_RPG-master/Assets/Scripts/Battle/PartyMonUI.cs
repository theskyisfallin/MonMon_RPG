using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMonUI : MonoBehaviour
{
    // user input on unity, will not show in game but is useful for testing
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] HP hp;
    [SerializeField] Text message;

    Pokemon mon;

    // init the mons you have
    public void Init(Pokemon pokemon)
    {
        mon = pokemon;
        UpdateData();

        mon.OnHpChange += UpdateData;
        SetMessage("");
    }

    // get name, level, and current Hp
    void UpdateData()
    {
        name.text = mon.Basic.Name;
        level.text = "Lv. " + mon.Level;
        hp.SetHp((float)mon.currentHp / mon.MaxHp);
    }

    // show player what monmon is selected
    public void Selected(bool selected)
    {
        if (selected)
            name.color = GlobalSettings.i.Hightlight;
        else
            name.color = Color.black;
    }

    // if a message is needed show here such as "choose a monmon"
    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
