using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildMons;

    public Pokemon getRandomWild()
    {
        var wildMon = wildMons[Random.Range(0, wildMons.Count)];
        wildMon.Init();
        return wildMon;
    }
}
