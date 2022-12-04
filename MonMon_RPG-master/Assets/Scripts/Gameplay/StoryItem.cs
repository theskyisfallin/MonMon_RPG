using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// plays when you interact with a story item
// this is used to tell player to get a monmon before heading out of town
public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }

    public bool TriggerMulti => false;
}
