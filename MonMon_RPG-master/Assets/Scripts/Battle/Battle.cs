using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum BattleState { Start, ActionSelect, MoveSelect, RunningTurn, Busy, PartyScreen, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class Battle : MonoBehaviour
{
    [SerializeField] PlayerMon playerMon;
    [SerializeField] PlayerMon enemyMon;
    [SerializeField] BattleBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;

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

        ActionSelect();
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

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move)
        {
            playerMon.Pokemon.CurrentMove = playerMon.Pokemon.Moves[currentMove];
            enemyMon.Pokemon.CurrentMove = enemyMon.Pokemon.randomMove();

            int playerMovePriority = playerMon.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyMon.Pokemon.CurrentMove.Base.Priority;

            //check priortiy
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerMon.Pokemon.Speed >= enemyMon.Pokemon.Speed;


            var firstMon = (playerGoesFirst) ? playerMon : enemyMon;
            var secondMon = (playerGoesFirst) ? enemyMon : playerMon;

            var secondPokemon = secondMon.Pokemon;

            yield return RunMove(firstMon, secondMon, firstMon.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstMon);
            if (state == BattleState.BattleOver) yield break;


            if (secondPokemon.currentHp > 0)
            {
                yield return RunMove(secondMon, firstMon, secondMon.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondMon);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon)
            {
                var selectedPokemon = playerParty.Pokemon[currentMon];
                state = BattleState.Busy;
                yield return SwitchMon(selectedPokemon);
            }

            var enemyMove = enemyMon.Pokemon.randomMove();
            yield return RunMove(enemyMon, playerMon, enemyMove);
            yield return RunAfterTurn(enemyMon);
            if (state == BattleState.BattleOver) yield break;
        }

        if (state != BattleState.BattleOver)
            ActionSelect();
    }

    IEnumerator RunMove(PlayerMon source, PlayerMon target, Move move)
    {

        bool canMove = source.Pokemon.OnBeforeMove();
        if (!canMove)
        {
            yield return ShowStatChange(source.Pokemon);
            yield return source.Hud.UpdateHp();
            yield break;
        }
        yield return ShowStatChange(source.Pokemon);


        move.Pp--;
        yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name} used {move.Base.Name}");


        if (CheckIfMoveHits(move, source.Pokemon, target.Pokemon))
        {


            source.AttackAnimation();
            yield return new WaitForSeconds(1f);
            target.HitAnimation();


            if (move.Base.Category == Category.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, source.Pokemon, target.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDets = target.Pokemon.TakeDamage(move, source.Pokemon);
                yield return target.Hud.UpdateHp();

                yield return ShowDamageDets(damageDets);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && target.Pokemon.currentHp > 0)
            {
                foreach (var seconday in move.Base.Secondaries)
                {
                    var rand = UnityEngine.Random.Range(1, 101);
                    if(rand <= seconday.Chance)
                        yield return RunMoveEffects(seconday, source.Pokemon, target.Pokemon, seconday.Target);
                }
            }

            if (target.Pokemon.currentHp <= 0)
            {
                yield return dialogBox.TypeDialog($"{target.Pokemon.Basic.Name} Fainted");
                target.FaintAnimation();

                yield return new WaitForSeconds(2f);

                CheckBattle(target);
            }
        }

        else
        {
            yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name}'s move missed");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, Target moveTarget)
    {
        // Stat boost
        if (effects.Boosts != null)
        {
            if (moveTarget == Target.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }


        // Status Conditions
        if (effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        if (effects.DynamicStatus != ConditionID.none)
        {
            target.SetDynamicStatus(effects.DynamicStatus);
        }

        yield return ShowStatChange(source);
        yield return ShowStatChange(target);
    }

    IEnumerator RunAfterTurn(PlayerMon source)
    {
        if (state == BattleState.BattleOver) yield break;

        yield return new WaitUntil(() => state == BattleState.RunningTurn);
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

    bool CheckIfMoveHits(Move move, Pokemon source, Pokemon target)
    {
        if (move.Base.CantMiss)
            return true;

        float moveAcc = move.Base.Acc;

        int acc = source.Boosts[Stat.Accuracy];
        int eva = target.Boosts[Stat.Evasion];

        var Boost = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if (acc > 0)
            moveAcc *= Boost[acc];
        else
            moveAcc /= Boost[-acc];

        if (eva > 0)
            moveAcc /= Boost[eva];
        else
            moveAcc *= Boost[-eva];

        return UnityEngine.Random.Range(1, 101) <= moveAcc;
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
                prevState = state;
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
            var move = playerMon.Pokemon.Moves[currentMove];

            if (move.Pp == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
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

            if(prevState == BattleState.ActionSelect)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;

                StartCoroutine(SwitchMon(selected));
            }

            
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelect();
        }
    }


    IEnumerator SwitchMon(Pokemon newMon)
    {
        if (playerMon.Pokemon.currentHp > 0)
        {
            yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} Return!");
            playerMon.FaintAnimation();

            yield return new WaitForSeconds(2f);
        }

        playerMon.Setup(newMon);

        dialogBox.SetMoveNames(newMon.Moves);

        yield return dialogBox.TypeDialog($"Go {newMon.Basic.Name}!");

        state = BattleState.RunningTurn;
    }
}