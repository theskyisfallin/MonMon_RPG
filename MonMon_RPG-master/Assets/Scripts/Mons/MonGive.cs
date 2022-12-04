using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// when givin a monmon this script is used
public class MonGive : MonoBehaviour, ISavable
{
    // user input for what monmon is given and dialog said before
    [SerializeField] Pokemon mon;
    [SerializeField] Dialog dialog;

    bool used = false;

    // gives the player a monmon if they have room in their party
    public IEnumerator GiveMon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        mon.Init();
        player.GetComponent<Party>().AddMon(mon);

        used = true;

        string dialogText = $"{player.Name} received {mon.Basic.Name}!";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    // checks if they can give a monmon
    public bool CanBeGiven()
    {
        return mon != null && !used;
    }

    // capture the used state to make sure you can't recieve them over and over again
    public object CaptureState()
    {
        return used;
    }

    // restore the used state
    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}
