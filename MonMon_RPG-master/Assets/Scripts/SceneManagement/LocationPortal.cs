using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// Teleports player without switching scene, due to it already being loaded
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Destination destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    // when the player triggers the protal teleport them
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    public bool TriggerMulti => false;

    Fader fader;

    // uses the fader to make the teleporting less jarring
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    // pauses you so you can't move during the teleport and gets the destination portal spawn point
    // and telports you there
    IEnumerator Teleport()
    {

        GameControl.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f);

        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);

        player.Character.SetPositionAndSnap(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameControl.Instance.PauseGame(false);
    }

    public Transform SpawnPoint => spawnPoint;
}
