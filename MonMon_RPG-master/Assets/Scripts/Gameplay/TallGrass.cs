using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// when walking through tall grass check for encounters
// wild grass has 10% for an encounter, if you get one
// stop the walking animation
public class TallGrass : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        if (Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameControl.Instance.StartBattle();
        }
    }

    public bool TriggerMulti => true;
}
