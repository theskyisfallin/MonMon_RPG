using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this script sets global settings, right now this is only for the highlight color when playing
public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlight;

    public Color Hightlight => highlight;

    public static GlobalSettings i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
