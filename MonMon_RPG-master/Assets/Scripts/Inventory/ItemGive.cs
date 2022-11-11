using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGive : MonoBehaviour
{
    [SerializeField] ItemBase item;
    [SerializeField] Dialog dialog;
    [SerializeField] int count = 1;

    bool used = false;

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

    public bool CanBeGiven()
    {
        return item != null && count > 0 && !used;
    }
}