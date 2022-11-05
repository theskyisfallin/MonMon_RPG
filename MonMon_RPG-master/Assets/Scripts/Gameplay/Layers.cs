using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layers : MonoBehaviour
{
    [SerializeField] LayerMask solidObjectLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grass;
    [SerializeField] LayerMask player;
    [SerializeField] LayerMask fov;

    public static Layers i { get; set; }

    private void Awake()
    {
        i = this;
    }

    public LayerMask SoildLayer
    {
        get => solidObjectLayer;
    }
    public LayerMask InteractableLayer
    {
        get => interactableLayer;
    }
    public LayerMask GrassLayer
    {
        get => grass;
    }
    public LayerMask PlayerLayer
    {
        get => player;
    }
    public LayerMask FovLayer
    {
        get => fov;
    }
}
