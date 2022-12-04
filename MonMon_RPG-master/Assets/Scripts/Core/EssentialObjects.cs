using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script is what is considered essential objects which is not destoryed on load in,
// such as player and the main camera
public class EssentialObjects : MonoBehaviour
{
    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
