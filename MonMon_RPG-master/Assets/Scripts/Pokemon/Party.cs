using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemon;

    public event Action OnUpdated;

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

    private void Awake()
    {
        foreach (var _pokemon in pokemon)
        {
            _pokemon.Init();
        }
    }

    private void Start()
    {
        
    }

    public Pokemon GetNotFaintedMon()
    {
        return pokemon.Where(x => x.currentHp > 0).FirstOrDefault();
    }

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

    public static Party GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Party>();
    }
}
