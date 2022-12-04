using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// simple box collider if the player walks into it, the onEneterTrainerVeiw from trainer function from
// GameControl script is called
public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameControl.Instance.OnEnterTrainersView(GetComponentInParent<TrainerController>());
    }

    public bool TriggerMulti => false;
}
