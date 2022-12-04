using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layers : MonoBehaviour
{
    // user input in unity
    [SerializeField] LayerMask solidObjectLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask grass;
    [SerializeField] LayerMask player;
    [SerializeField] LayerMask fov;
    [SerializeField] LayerMask portal;
    [SerializeField] LayerMask triggerLayer;

    public static Layers i { get; set; }

    // awake init this
    private void Awake()
    {
        i = this;
    }

    // get all layers in game
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
    public LayerMask PortalLayer
    {
        get => portal;
    }

    // triggerable layers
    public LayerMask TriggerableLayers
    {
        get => grass | fov | portal | triggerLayer;
    }
}
