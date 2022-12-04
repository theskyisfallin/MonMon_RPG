using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    // list of monmons in the player's party
    [SerializeField] List<Pokemon> pokemon;

    public event Action OnUpdated;

    // on update make sure to invoke
    public List<Pokemon> Pokemon
    {
        get
        {
            return pokemon;
        }
        set
        {
            pokemon = value;
            OnUpdated?.Invoke();
        }
    }

    // on awake init the party
    private void Awake()
    {
        foreach (var _pokemon in pokemon)
        {
            _pokemon.Init();
        }
    }

    // nothing on start
    private void Start()
    {
        
    }

    // when sending out your first monmon in battle needs to not be fainted
    public Pokemon GetNotFaintedMon()
    {
        return pokemon.Where(x => x.currentHp > 0).FirstOrDefault();
    }

    // adds monmon to the party
    public void AddMon(Pokemon newMon)
    {
        if (pokemon.Count < 6)
        {
            pokemon.Add(newMon);
            OnUpdated?.Invoke();
        }
        else
        {
            // TODO: transfer to PC
        }
    }

    // fetches palyer party
    public static Party GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Party>();
    }
}
