using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// sets tiles as invisible, this is how i do walls inside of houses
// so you can't walk out but you can make them any solid object you want
public class InvisibleTiles : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }
}
