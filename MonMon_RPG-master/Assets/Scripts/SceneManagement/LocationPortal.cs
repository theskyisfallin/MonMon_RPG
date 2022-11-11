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

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
    }

    Fader fader;

    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

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
