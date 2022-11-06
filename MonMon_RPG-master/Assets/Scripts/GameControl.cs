using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Free, Battle, Dialog, Cutscene }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Battle battle;
    [SerializeField] Camera worldCam;

    GameState state;

    public static GameControl Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        ConditionsDB.Init();
    }

    private void Start()
    {
        playerController.Encounter += StartBattle;
        battle.OnBattleOver += EndBattle;

        playerController.OnEnterTrainersView += (Collider2D trainerCol) =>
        {
            var trainer = trainerCol.GetComponentInParent<TrainerController>();
            if (trainer != null)
            {
                state = GameState.Cutscene;
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if(state == GameState.Dialog)
                state = GameState.Free;
        };
    }

    void StartBattle()
    {
        state = GameState.Battle;
        battle.gameObject.SetActive(true);
        worldCam.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildMon = FindObjectOfType<MapArea>().GetComponent<MapArea>().getRandomWild();

        var wildMonCopy = new Pokemon(wildMon.Basic, wildMon.Level);

        battle.StartBattle(playerParty, wildMonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battle.gameObject.SetActive(true);
        worldCam.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<Party>();
        var trainerParty = trainer.GetComponent<Party>();

        battle.StartTrainerBattle(playerParty, trainerParty);
    }

    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
            

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
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
    }
}
