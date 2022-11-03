using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemon;

    public List<Pokemon> Pokemon
    {
        get
        {
            return pokemon;
        }
    }

    private void Start()
    {
        foreach (var _pokemon in pokemon)
        {
            _pokemon.Init();
        }
    }

    public Pokemon GetNotFaintedMon()
    {
        return pokemon.Where(x => x.currentHp > 0).FirstOrDefault();
    }
}
