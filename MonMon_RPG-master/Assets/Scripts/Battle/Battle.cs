using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Linq;

public enum BattleState { Start, ActionSelect, MoveSelect, RunningTurn, Busy, AboutToUse ,PartyScreen, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class Battle : MonoBehaviour
{
    [SerializeField] PlayerMon playerMon;
    [SerializeField] PlayerMon enemyMon;
    [SerializeField] BattleBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject ballSprite;
    [SerializeField] MoveSelectUI moveSelect;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;

    int currentAction;
    int currentMove;
    int currentMon;

    bool aboutToUseChoice = true;

    Party playerParty;
    Party trainerParty;
    Pokemon wildMon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBasic moveToLearn;

    public void StartBattle(Party playerParty, Pokemon wildMon)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildMon = wildMon;

        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(Party playerParty, Party trainerParty)
    {
        
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerMon.Clear();
        enemyMon.Clear();

        if (!isTrainerBattle)
        {
            playerMon.Setup(playerParty.GetNotFaintedMon());
            enemyMon.Setup(wildMon);

            dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

            yield return dialogBox.TypeDialog($"A wild {enemyMon.Pokemon.Basic.Name} appeared!");
        }
        else
        {
            playerMon.gameObject.SetActive(false);
            enemyMon.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} is trying to mug you");

            trainerImage.gameObject.SetActive(false);
            enemyMon.gameObject.SetActive(true);

            var enemyPokemon = trainerParty.GetNotFaintedMon();
            enemyMon.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Basic.Name}");

            playerImage.gameObject.SetActive(false);
            playerMon.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetNotFaintedMon();
            playerMon.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Basic.Name}!");
            dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

        }
        escapeAttempts = 0;

        partyScreen.Init();

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

    IEnumerator AboutToUse(Pokemon newMon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMon.Basic.Name}. Do you want to switch?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBasic newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");

        moveSelect.gameObject.SetActive(true);
        moveSelect.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
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
            else if (playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowBall();
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
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
                yield return HandlePokemonFainted(target);
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
            yield return HandlePokemonFainted(source);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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


    IEnumerator HandlePokemonFainted(PlayerMon faintedMon)
    {
        yield return dialogBox.TypeDialog($"{faintedMon.Pokemon.Basic.Name} Fainted");
        faintedMon.FaintAnimation();

        yield return new WaitForSeconds(2f);

        if (!faintedMon.IsPlayer)
        {
            int expYield = faintedMon.Pokemon.Basic.ExpYield;
            int enemyLevel = faintedMon.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);

            playerMon.Pokemon.Exp += expGain;

            yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} gained {expGain} exp");

            yield return playerMon.Hud.SetExpTick();


            while (playerMon.Pokemon.CheckForLevelUp())
            {
                playerMon.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} grew to level {playerMon.Pokemon.Level}");


                var newMove = playerMon.Pokemon.GetLearnableMoveAtCurrent();

                if (newMove != null)
                {
                    if (playerMon.Pokemon.Moves.Count < PokemonBasic.MaxNumOfMoves)
                    {
                        playerMon.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerMon.Pokemon.Moves);
                    }
                    else
                    {
                        yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} is trying to learn {newMove.Base.Name}");
                        yield return dialogBox.TypeDialog($"But it cannot learn more than {PokemonBasic.MaxNumOfMoves} moves");
                        yield return ChooseMoveToForget(playerMon.Pokemon, newMove.Base);

                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);
                    }
                }

                yield return playerMon.Hud.SetExpTick(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckBattle(faintedMon);
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
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextMon = trainerParty.GetNotFaintedMon();
                if (nextMon != null)
                    StartCoroutine(AboutToUse(nextMon));
                else
                    BattleOver(true);
            }
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
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelect.gameObject.SetActive(false);
                if (moveIndex == PokemonBasic.MaxNumOfMoves)
                {
                    StartCoroutine(dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} did not learn {moveToLearn.Name}"));
                }
                else
                {
                    var selected = playerMon.Pokemon.Moves[moveIndex].Base;

                    StartCoroutine(dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} forgot {selected.Name} and learned {moveToLearn.Name}"));

                    playerMon.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }

                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelect.HandleMoveSelection(onMoveSelected);
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
                StartCoroutine(RunTurns(BattleAction.UseItem));
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
                StartCoroutine(RunTurns(BattleAction.Run));
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
            if (playerMon.Pokemon.currentHp <= 0)
            {
                partyScreen.SetMessage("You have to choose a MonMon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if (prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerMon());
            }   
            else
                ActionSelect();
        }
    }

    void HandleAboutToUse()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                prevState = BattleState.AboutToUse;
                OpenParty();
            }
            else
            {
                StartCoroutine(SendNextTrainerMon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerMon());
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

        if (prevState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if (prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerMon());
        }
    }

    IEnumerator SendNextTrainerMon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetNotFaintedMon();

        enemyMon.Setup(nextPokemon);

        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Basic.Name}");

        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowBall()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't catch other trainers MonMons!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a MonBall!");

        var ballObject = Instantiate(ballSprite, playerMon.transform.position - new Vector3(2, 0), Quaternion.identity);
        var ball = ballObject.GetComponent<SpriteRenderer>();

        yield return ball.transform.DOJump(enemyMon.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();

        yield return enemyMon.CaptureAnimation();

        yield return ball.transform.DOLocalMoveY(enemyMon.transform.position.y - 3.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatch(enemyMon.Pokemon);

        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return ball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if (shakeCount == 4)
        {
            yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} was caught!");
            yield return ball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddMon(enemyMon.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} was added to your party");

            Destroy(ball);
            BattleOver(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);
            ball.DOFade(0, 0.2f);
            yield return enemyMon.BreakOutAnimation();

            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} broke free");
            else
                yield return dialogBox.TypeDialog($"{enemyMon.Pokemon.Basic.Name} was almost caught!");

            Destroy(ball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatch(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.currentHp) * pokemon.Basic.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;


            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You cannot run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerMon.Pokemon.Speed;
        int enemySpeed = enemyMon.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Got away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Got away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}