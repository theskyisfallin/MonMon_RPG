using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script overlays map areas and controls what list of wild mon mons you can get
public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildMons;

    // gets the random monmon, inits it, and then returns that monmon
    public Pokemon getRandomWild()
    {
        var wildMon = wildMons[Random.Range(0, wildMons.Count)];
        wildMon.Init();
        return wildMon;
    }
}
