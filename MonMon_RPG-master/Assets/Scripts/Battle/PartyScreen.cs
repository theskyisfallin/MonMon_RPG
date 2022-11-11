using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text message;

    PartyMonUI[] members;

    List<Pokemon> mons;
    Party party;

    int selected = 0;

    public Pokemon SelectedMon => mons[selected];


    public BattleState? CalledFrom { get; set; }

    public void Init()
    {
        members = GetComponentsInChildren<PartyMonUI>(true);
        party = Party.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {

        mons = party.Pokemon;

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

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for (int i = 0; i < mons.Count; i++)
        {
            string message = tmItem.CanBeTaught(mons[i]) ? "Able" : "Unable";
            members[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for (int i = 0; i < mons.Count; i++)
        {
            members[i].SetMessage("");
        }
    }

    public void SetMessage(string message)
    {
        this.message.text = message;
    }
}
