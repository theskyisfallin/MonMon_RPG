using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class Battle : MonoBehaviour
{
    [SerializeField] PlayerMon playerMon;
    [SerializeField] BattleHud playerHud;
    [SerializeField] PlayerMon enemyMon;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleBox dialogBox;

    public event Action<bool> BattleOver;

    BattleState state;

    int currentAction;
    int currentMove;

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
        playerHud.SetHud(playerMon.Pokemon);
        enemyHud.SetHud(enemyMon.Pokemon);
        dialogBox.EnableMoveSelector(false);

        dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyMon.Pokemon.Basic.Name} appeared!");

        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action"));
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(false);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var move = playerMon.Pokemon.Moves[currentMove];
        move.Pp--;
        yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} used {move.Base.Name}");

        playerMon.AttackAnimation();
        yield return new WaitForSeconds(1f);


        enemyMon.HitAnimation();
        var damageDets = enemyMon.Pokemon.TakeDamage(move, playerMon.Pokemon);
        yield return enemyHud.UpdateHp();

        yield return ShowDamageDets(damageDets);

        if (damageDets.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} Fainted.");
            enemyMon.FaintAnimation();

            yield return new WaitForSeconds(2f);
            BattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyMon.Pokemon.randomMove();
        move.Pp--;
        yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} used {move.Base.Name}");

        enemyMon.AttackAnimation();
        yield return new WaitForSeconds(1f);

        playerMon.HitAnimation();
        var damageDets = playerMon.Pokemon.TakeDamage(move, enemyMon.Pokemon);
        yield return playerHud.UpdateHp();

        yield return ShowDamageDets(damageDets);


        if (damageDets.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} Fainted");
            playerMon.FaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextMon = playerParty.GetNotFaintedMon();

            if(nextMon != null)
            {
                playerMon.Setup(nextMon);
                playerHud.SetHud(nextMon);

                dialogBox.SetMoveNames(nextMon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextMon.Basic.Name}!");

                PlayerAction();
            }
            else
            {
                yield return dialogBox.TypeDialog($"You are out of useable MonMons.");
                BattleOver(false);
            }
            
        }
        else
        {
            PlayerAction();
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
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (currentAction == 0)
            {
                //fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //run
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerMon.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerMon.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }
        dialogBox.UpdateMoveSelection(currentMove, playerMon.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
