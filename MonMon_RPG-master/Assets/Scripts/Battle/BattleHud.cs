using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] HP hp;

    Pokemon mon;

    public void SetHud(Pokemon pokemon)
    {
        mon = pokemon;
        name.text = pokemon.Basic.Name;
        level.text = "Lv. " + pokemon.Level;
        hp.SetHp((float)pokemon.currentHp / pokemon.MaxHp);
    }

    public IEnumerator UpdateHp()
    {
        yield return hp.tickHp((float)mon.currentHp / mon.MaxHp);
    }
}
