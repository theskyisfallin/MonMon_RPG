using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text message;

    PartyMonUI[] members;

    List<Pokemon> pokemon;

    public void Init()
    {
        members = GetComponentsInChildren<PartyMonUI>();
    }

    public void SetPartyData(List<Pokemon> pokemon)
    {

        this.pokemon = pokemon;

        for (int i = 0; i < members.Length; i++)
        {
            if (i < pokemon.Count)
                members[i].SetHud(pokemon[i]);
            else
                members[i].gameObject.SetActive(false);
        }

        message.text = "Choose a MonMon";
    }

    public void UpdateMonSelection(int selected)
    {
        for(int i = 0; i < pokemon.Count; i++)
        {
            if (i == selected)
                members[i].Selected(true);
            else
                members[i].Selected(false);
        }
    }

    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
