using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{
    static Dictionary<string, PokemonBasic> mons;

    public static void Init()
    {
        mons = new Dictionary<string, PokemonBasic>();

        var parray = Resources.LoadAll<PokemonBasic>("");

        foreach (var mon in parray)
        {
            if (mons.ContainsKey(mon.Name))
            {
                Debug.LogError($"There are two mons with the name {mon.Name}");
                continue;
            }

            mons[mon.Name] = mon;
        }
    }

    public static PokemonBasic GetPokemonViaName(string name)
    {
        if (!mons.ContainsKey(name))
        {
            Debug.LogError($"Mon \"{name}\" not found");
            return null;
        }

        return mons[name];
    }
}
