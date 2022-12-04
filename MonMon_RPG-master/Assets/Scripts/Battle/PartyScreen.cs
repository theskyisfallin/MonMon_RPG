using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text message;

    // get a list of members and mons for the party
    PartyMonUI[] members;

    List<Pokemon> mons;
    Party party;

    int selected = 0;

    public Pokemon SelectedMon => mons[selected];

    // get where this function is called from
    public BattleState? CalledFrom { get; set; }

    // init the players party
    public void Init()
    {
        members = GetComponentsInChildren<PartyMonUI>(true);
        party = Party.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    // set the party's data 
    public void SetPartyData()
    {

        mons = party.Pokemon;

        // to not show the default in make sure to SetActive to false if it's > the players current party mons
        for (int i = 0; i < members.Length; i++)
        {
            if (i < mons.Count)
            {
                members[i].gameObject.SetActive(true);
                members[i].Init(mons[i]);
            }
            else
                members[i].gameObject.SetActive(false);
        }
        UpdateMonSelection(selected);

        message.text = "Choose a MonMon";
    }

    // handle the selction and invoke on select and on back if needed
    public void HandleUpdate(Action onSelect, Action onBack)
    {
        var prevSelection = selected;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            selected++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            selected--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selected += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selected -= 2;

        selected = Mathf.Clamp(selected, 0, mons.Count - 1);

        if (selected != prevSelection)
            UpdateMonSelection(selected);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelect?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    // show which mon is selected
    public void UpdateMonSelection(int selected)
    {
        for(int i = 0; i < mons.Count; i++)
        {
            if (i == selected)
                members[i].Selected(true);
            else
                members[i].Selected(false);
        }
    }

    // show if a Tm item is usuable on the party mons
    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < mons.Count; i++)
        {
            string message = tmItem.CanBeTaught(mons[i]) ? "Able" : "Unable";
            members[i].SetMessage(message);
        }
    }

    // clears the message
    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < mons.Count; i++)
        {
            members[i].SetMessage("");
        }
    }

    // sets the message
    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
