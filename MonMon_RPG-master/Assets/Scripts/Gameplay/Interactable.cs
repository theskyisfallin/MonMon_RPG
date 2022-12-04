using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is an interface to show if a layer is interactable
public interface Interactable
{
    IEnumerator Interact(Transform start);

}
