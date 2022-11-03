using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BattleState { Start, ActionSelect, MoveSelect, PerformMove, Busy, PartyScreen, BattleOver }

public class Battle : MonoBehaviour
{
    [SerializeField] PlayerMon playerMon;
    [SerializeField] PlayerMon enemyMon;
    [SerializeField] BattleBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;

    int currentAction;
    int currentMove;
    int currentMon;

    Party playerParty;
    Pokemon wildMon;

    public void StartBattle(Party playerParty, Pokemon wildMon)
    {
        this.playerParty = playerParty;
        this.wildMon = wildMon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerMon.Setup(playerParty.GetNotFaintedMon());
        enemyMon.Setup(wildMon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyMon.Pokemon.Basic.Name} appeared!");

        FirstTurn();
    }

    void FirstTurn()
    {
        if (playerMon.Pokemon.Speed >= enemyMon.Pokemon.Speed)
            ActionSelect();
        else if(enemyMon.Pokemon.Speed >= playerMon.Pokemon.Speed)
            StartCoroutine(EnemyMove());
        else
        {
            int rand = Random.Range(0, 2);
            if (rand == 0)
                ActionSelect();
            else
                StartCoroutine(EnemyMove());
        }
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemon.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelect()
    {
        state = BattleState.ActionSelect;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(false);

    }

    void OpenParty()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemon);
        partyScreen.gameObject.SetActive(true);

    }

    void MoveSelect()
    {
        state = BattleState.MoveSelect;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerMon.Pokemon.Moves[currentMove];

        yield return RunMove(playerMon, enemyMon, move);

        if (state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyMon.Pokemon.randomMove();

        yield return RunMove(enemyMon, playerMon, move);


        if(state == BattleState.PerformMove)
            ActionSelect();
        
    }


    IEnumerator RunMove(PlayerMon source, PlayerMon target, Move move)
    {

        bool canMove = source.Pokemon.OnBeforeMove();
        if (!canMove)
        {
            yield return ShowStatChange(source.Pokemon);

            yield break;
        }
        yield return ShowStatChange(source.Pokemon);


        move.Pp--;
        yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name} used {move.Base.Name}");

        source.AttackAnimation();
        yield return new WaitForSeconds(1f);
        target.HitAnimation();


        if(move.Base.Category == Category.Status)
        {
            yield return RunMoveEffects(move, source.Pokemon, target.Pokemon);
        }
        else
        {
            var damageDets = target.Pokemon.TakeDamage(move, source.Pokemon);
            yield return target.Hud.UpdateHp();

            yield return ShowDamageDets(damageDets);
        }

        if (target.Pokemon.currentHp <= 0)
        {
            yield return dialogBox.TypeDialog($"{target.Pokemon.Basic.Name} Fainted.");
            target.FaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckBattle(target);
        }


        // damage after turn from status effects
        source.Pokemon.OnAfterTurn();
        yield return ShowStatChange(source.Pokemon);
        yield return source.Hud.UpdateHp();
        //checks if the pokemon fainted from status effects
        if (source.Pokemon.currentHp <= 0)
        {
            yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name} Fainted.");
            source.FaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckBattle(source);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effects;

        // Stat boost
        if (effects.Boosts != null)
        {
            if (move.Base.Target == Target.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }


        // Status Conditions
        if (effects.Status != ConditonID.none)
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatChange(source);
        yield return ShowStatChange(target);
    }


    IEnumerator ShowStatChange(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }


    void CheckBattle(PlayerMon fainted)
    {
        if (fainted.IsPlayer)
        {
            var nextMon = playerParty.GetNotFaintedMon();

            if (nextMon != null)
                OpenParty();
            else 
                BattleOver(false);
        }
        else
        {
            BattleOver(true);
        }
    }


    IEnumerator ShowDamageDets(DamageDets damageDets)
    {
        if (damageDets.Crit > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDets.Type > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDets.Type < 1f)
            yield return dialogBox.TypeDialog("It's not very effecitve.");
    }

    public void HandleUpdate()
    {
        if (state == BattleState.ActionSelect)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelect)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentAction++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentAction--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);


        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //fight
                MoveSelect();
            }
            else if (currentAction == 1)
            {
                //bag
            }
            else if (currentAction == 2)
            {
                //switch
                OpenParty();
            }
            else if (currentAction == 3)
            {
                //run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerMon.Pokemon.Moves.Count - 1);


        dialogBox.UpdateMoveSelection(currentMove, playerMon.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelect();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMon++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMon--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMon += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMon -= 2;

        currentMon = Mathf.Clamp(currentMon, 0, playerParty.Pokemon.Count - 1);

        partyScreen.UpdateMonSelection(currentMon);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selected = playerParty.Pokemon[currentMon];
            if (selected.currentHp <= 0)
            {
                partyScreen.SetMessage("This MonMon is fainted");
                return;
            }
            if(selected == playerMon.Pokemon)
            {
                partyScreen.SetMessage("This MonMon is already out");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;

            StartCoroutine(SwitchMon(selected));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelect();
        }
    }


    IEnumerator SwitchMon(Pokemon newMon)
    {

        bool currentMonFainted = true;

        if (playerMon.Pokemon.currentHp > 0)
        {
            currentMonFainted = false;
            yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} Return!");
            playerMon.FaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        playerMon.Setup(newMon);

        dialogBox.SetMoveNames(newMon.Moves);

        yield return dialogBox.TypeDialog($"Go {newMon.Basic.Name}!");

        if (currentMonFainted)
            FirstTurn();
        else
            StartCoroutine(EnemyMove());
    }
}