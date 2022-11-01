using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMon : MonoBehaviour
{
    [SerializeField] PokemonBasic Basic;
    [SerializeField] int level;
    [SerializeField] bool isPlayer;

    public Pokemon Pokemon { get; set; }

    public void Setup()
    {
        Pokemon = new Pokemon(Basic, level);
        if (isPlayer)
            GetComponent<Image>().sprite = Pokemon.Basic.Back;
        else
            GetComponent<Image>().sprite = Pokemon.Basic.Front;
    }
}
