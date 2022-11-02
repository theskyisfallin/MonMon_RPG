using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Free, Battle }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Battle battle;
    [SerializeField] Camera worldCam;

    GameState state;

    private void Start()
    {
        playerController.Encounter += StartBattle;
        battle.BattleOver += EndBattle;
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battle.gameObject.SetActive(true);
        worldCam.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildMon = FindObjectOfType<MapArea>().GetComponent<MapArea>().getRandomWild();

        battle.StartBattle(playerParty, wildMon);
    }

    void EndBattle(bool won)
    {
        state = GameState.Free;
        battle.gameObject.SetActive(false);
        worldCam.gameObject.SetActive(true);
    }

    private void Update()
    {
        if(state == GameState.Free)
        {
            playerController.HandleUpdate();
        }
        else if(state == GameState.Battle)
        {
            battle.HandleUpdate();
        }
    }
}
