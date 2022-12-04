using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// if the player interacts with an item pickup in the world this script is ran
public class Pickup : MonoBehaviour, Interactable, ISavable
{
    // what item this is, set by user in unity
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    // what is done when the user interacts with the pick up
    public IEnumerator Interact(Transform initiator)
    {
        // if it is not used add the item to the players inventory and say so
        // also disable the item and collider
        // can't delete because we need to save that this item was already picked up
        // cannot do so if it is deleted
        if (!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;

            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            var playerName = initiator.GetComponent<PlayerController>().Name;

            yield return DialogManager.Instance.ShowDialogText($"{playerName} found {item.Name}!");
        }
    }

    // capture if the user has already picked up the item or not
    public object CaptureState()
    {
        return Used;
    }

    // restore the state if it was used or not
    // if it was already used disable the item and collider
    public void RestoreState(object state)
    {
        Used = (bool)state;

        if (Used)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
