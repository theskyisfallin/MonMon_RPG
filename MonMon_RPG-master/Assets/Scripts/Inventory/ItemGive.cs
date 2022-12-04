using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// when an NPC can give an item this script is ran
public class ItemGive : MonoBehaviour, ISavable
{
    // user input in unity
    [SerializeField] ItemBase item;
    [SerializeField] Dialog dialog;
    [SerializeField] int count = 1;

    bool used = false;

    // gives an item to the player in their inventory and lets the player know they got the item(s)
    public IEnumerator GiveItem(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        player.GetComponent<Inventory>().AddItem(item, count);

        used = true;

        string dialogText = $"{player.Name} received {item.Name}!";
        if (count > 1)
            dialogText = $"{player.Name} received {count} {item.Name}s!";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    // checks if the item can be given
    public bool CanBeGiven()
    {
        return item != null && count > 0 && !used;
    }

    // capture if you have already been given the item or not
    public object CaptureState()
    {
        return used;
    }

    // restores the captured state
    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
