using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Free, Battle, Dialog, Menu, Party, Bag, Cutscene, Paused }

public class GameControl : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] Battle battle;
    [SerializeField] Camera worldCam;
    [SerializeField] PartyScreen party;
    [SerializeField] InventoryUI inventory;


    GameState state;

    GameState prevState;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }
    MenuController menuController;

    public static GameControl Instance { get; private set; }

    public void Awake()
    {
        Instance = this;
        menuController = GetComponent<MenuController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battle.OnBattleOver += EndBattle;

        party.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };

        DialogManager.Instance.OnCloseDialog += () =>
        {
            if(state == GameState.Dialog)
                state = prevState;
        };

        menuController.onBack += () =>
        {
            state = GameState.Free;
        };

        menuController.onMenuSelect += OnMenuSelect;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battle.gameObject.SetActive(true);
        worldCam.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<Party>();
        var wildMon = CurrentScene.GetComponent<MapArea>().getRandomWild();

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

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
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

            if (Input.GetKeyDown(KeyCode.C))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if(state == GameState.Battle)
        {
            battle.HandleUpdate();
        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.Party)
        {
            Action onSelected = () =>
            {

            };
            Action onBack = () =>
            {
                party.gameObject.SetActive(false);
                state = GameState.Free;
            };
            party.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventory.gameObject.SetActive(false);
                state = GameState.Free;
            };
            inventory.HandleUpdate(onBack);
        }
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    void OnMenuSelect(int selected)
    {
        if (selected == 0)
        {
            //MonMon
            party.gameObject.SetActive(true);
            state = GameState.Party;
        }
        else if (selected == 1)
        {
            //Bag
            inventory.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selected == 2)
        {
            //Save
            SavingSystem.i.Save("save1");
            state = GameState.Free;
        }
        else if (selected == 3)
        {
            //Load
            SavingSystem.i.Load("save1");
            state = GameState.Free;
        }
    }

    public GameState State => state;
}
