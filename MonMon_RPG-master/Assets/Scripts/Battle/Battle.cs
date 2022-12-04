using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using DG.Tweening;
using System.Linq;

// Need to know what state the battle is in or what action is selected during battle thus BattleState and BattleAction
public enum BattleState { Start, ActionSelect, MoveSelect, RunningTurn, Busy, Bag, AboutToUse, PartyScreen, MoveToForget, BattleOver }
public enum BattleAction { Move, SwitchPokemon, UseItem, Run }

public class Battle : MonoBehaviour
{
    // Fields the user can input from Unity itself [SerializeField] are user input fields
    [SerializeField] PlayerMon playerMon;
    [SerializeField] PlayerMon enemyMon;
    [SerializeField] BattleBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject ballSprite;
    [SerializeField] MoveSelectUI moveSelect;
    [SerializeField] InventoryUI inventoryUI;

    // Action var
    public event Action<bool> OnBattleOver;

    // var to save state
    BattleState state;
    

    int currentAction;
    int currentMove;

    bool aboutToUseChoice = true;

    // init parties and wild mon
    Party playerParty;
    Party trainerParty;
    Pokemon wildMon;
    int currentMon;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    int escapeAttempts;
    MoveBasic moveToLearn;

    // Starting a wild battle needs a player part and wild mon then calls SetupBattle
    public void StartBattle(Party playerParty, Pokemon wildMon)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildMon = wildMon;

        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine(SetupBattle());
    }

    // Does basically the same as wild battles but needs trainer party and different contorller
    public void StartTrainerBattle(Party playerParty, Party trainerParty)
    {
        
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        isTrainerBattle = true;

        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    // Sets up the battle
    public IEnumerator SetupBattle()
    {
        // clears Mons from last battle if you had one so they don't overlay
        playerMon.Clear();
        enemyMon.Clear();

        // if it is a wild battle show the mon and text
        if (!isTrainerBattle)
        {
            playerMon.Setup(playerParty.GetNotFaintedMon());
            enemyMon.Setup(wildMon);

            dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

            yield return dialogBox.TypeDialog($"A wild {enemyMon.Pokemon.Basic.Name} appeared!");
        }
        // Trainer battles
        else
        {
            // do not show the monmon but instead the trainers and then the monmon after
            playerMon.gameObject.SetActive(false);
            enemyMon.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);

            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} is trying to battle you");

            trainerImage.gameObject.SetActive(false);
            enemyMon.gameObject.SetActive(true);

            // trainers will never have a fainted mon in their party if you haven't battled them before
            // that said we use the same method for the players party so needs to be called here
            // to send out their first monmon
            var enemyPokemon = trainerParty.GetNotFaintedMon();
            enemyMon.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyPokemon.Basic.Name}");

            // sends out players first non-fainted monomon
            playerImage.gameObject.SetActive(false);
            playerMon.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetNotFaintedMon();
            playerMon.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Basic.Name}!");
            dialogBox.SetMoveNames(playerMon.Pokemon.Moves);

        }
        // sets escapeAttemps to 0
        // the more attempts you make the higher chance you have for escaping
        escapeAttempts = 0;

        // init party screen
        partyScreen.Init();

        // go into action selection
        ActionSelect();
    }

    // when battle is either won or lost: set state
    // clear everything
    // call OnBattleOver and pass the bool
    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemon.ForEach(p => p.OnBattleOver());
        playerMon.Hud.ClearData();
        enemyMon.Hud.ClearData();
        OnBattleOver(won);
    }

    // player needs to choose an action, don't show moves
    void ActionSelect()
    {
        state = BattleState.ActionSelect;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(false);

    }

    // state is in teh bag and calls the inventoryUI gameObject
    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    // because party can be opened in and outside of battle you need the
    // partyScreen.CalledFrom = state to know where it is called from
    // state is set to party screen and the gameObject is set to active
    void OpenParty()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);

    }

    // Show moves in the player's dialog box
    void MoveSelect()
    {
        state = BattleState.MoveSelect;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    // when fainting a trainers monmon they will say who they are about to use and the player can
    // choose to either say in or switch to another monmon
    // busy is an important state, basically just removes player control until the state is changed
    IEnumerator AboutToUse(Pokemon newMon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMon.Basic.Name}. Do you want to switch?");

        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    // when a monmon already knows 4 moves and tries to learn another
    // this willl pop up and show your move list
    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBasic newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");

        moveSelect.gameObject.SetActive(true);
        moveSelect.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
    }

    // This funciton handles the running of turns during battles
    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        // player chooses to use a move
        if (playerAction == BattleAction.Move)
        {
            // gets what move the player chose and enemy picks a random move
            playerMon.Pokemon.CurrentMove = playerMon.Pokemon.Moves[currentMove];
            enemyMon.Pokemon.CurrentMove = enemyMon.Pokemon.randomMove();

            int playerMovePriority = playerMon.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyMon.Pokemon.CurrentMove.Base.Priority;

            // check priortiy to see if one used a move with a higher prioity
            bool playerGoesFirst = true;

            if (enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if (enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerMon.Pokemon.Speed >= enemyMon.Pokemon.Speed;

            // based on speed picks which monmon goes first
            var firstMon = (playerGoesFirst) ? playerMon : enemyMon;
            var secondMon = (playerGoesFirst) ? enemyMon : playerMon;

            var secondPokemon = secondMon.Pokemon;

            // run move and any after turn effects
            yield return RunMove(firstMon, secondMon, firstMon.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstMon);
            if (state == BattleState.BattleOver) yield break;

            // if the second monmon didn't faint run their move and after turn effects
            if (secondPokemon.currentHp > 0)
            {
                yield return RunMove(secondMon, firstMon, secondMon.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondMon);
                if (state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            // if player choose to switch monmon
            // note: the word "pokemon" was attempted to remove but caused so many errors
            //       I did not think it was worth it
            if (playerAction == BattleAction.SwitchPokemon)
            {
                // pull up party screen and call SwitchMon
                var selectedPokemon = partyScreen.SelectedMon;
                state = BattleState.Busy;
                yield return SwitchMon(selectedPokemon);
            }
            // if player uses an item
            else if (playerAction == BattleAction.UseItem)
            {
                // handled from item screen
                dialogBox.EnableActionSelector(false);
            }
            // if player tries to run call TryToEscape
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
            }

            // after you use one of these the enemy monmon goes as this counts as your turn
            var enemyMove = enemyMon.Pokemon.randomMove();
            yield return RunMove(enemyMon, playerMon, enemyMove);
            yield return RunAfterTurn(enemyMon);
            if (state == BattleState.BattleOver) yield break;
        }
        // if the battle didn't end go back to ActionSelect and keep running
        if (state != BattleState.BattleOver)
            ActionSelect();
    }

    // Running moves of each of the monmon
    IEnumerator RunMove(PlayerMon source, PlayerMon target, Move move)
    {
        // checks for statuses before moves such as par and confusion/sleep to see if
        // the source monmon can move
        bool canMove = source.Pokemon.OnBeforeMove();
        // if you can't move show why
        if (!canMove)
        {
            yield return ShowStatChange(source.Pokemon);
            yield return source.Hud.WaitForHpUpdate();
            yield break;
        }
        yield return ShowStatChange(source.Pokemon);

        // remove power points from the source monmon and show what move they used
        move.Pp--;
        yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name} used {move.Base.Name}");

        // checkIfMoveHits for accuracy and if it does run move
        if (CheckIfMoveHits(move, source.Pokemon, target.Pokemon))
        {
            // show animations
            source.AttackAnimation();
            yield return new WaitForSeconds(1f);
            target.HitAnimation();

            // if the move is a status move run the move effects
            if (move.Base.Category == Category.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, source.Pokemon, target.Pokemon, move.Base.Target);
            }
            // target takes damage from the move and source and shows viusal feedback
            else
            {
                var damageDets = target.Pokemon.TakeDamage(move, source.Pokemon);
                yield return target.Hud.WaitForHpUpdate();

                yield return ShowDamageDets(damageDets);
            }
            // check if the move used has secondary effects
            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && target.Pokemon.currentHp > 0)
            {
                foreach (var seconday in move.Base.Secondaries)
                {
                    // the chance for it to happen and roll and random number for the effect to proc
                    var rand = UnityEngine.Random.Range(1, 101);
                    if(rand <= seconday.Chance)
                        yield return RunMoveEffects(seconday, source.Pokemon, target.Pokemon, seconday.Target);
                }
            }
            // if the target faints handle this
            if (target.Pokemon.currentHp <= 0)
            {
                yield return HandlePokemonFainted(target);
            }
        }
        // if you missed earlier show that this move missed
        else
        {
            yield return dialogBox.TypeDialog($"{source.Pokemon.Basic.Name}'s move missed");
        }
    }

    // if the move has effects such as stat boosts or statuses it is handled here
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

    // if a status does damage or something else after the turn it is run here
    IEnumerator RunAfterTurn(PlayerMon source)
    {
        // battle needs to not be over
        if (state == BattleState.BattleOver) yield break;

        yield return new WaitUntil(() => state == BattleState.RunningTurn);
        // damage after turn from status effects
        source.Pokemon.OnAfterTurn();
        yield return ShowStatChange(source.Pokemon);
        yield return source.Hud.WaitForHpUpdate();
        //checks if the pokemon fainted from status effects
        if (source.Pokemon.currentHp <= 0)
        {
            yield return HandlePokemonFainted(source);

            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    // shows status changes the the dialog box
    IEnumerator ShowStatChange(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    // if a monmon faints this function is called
    IEnumerator HandlePokemonFainted(PlayerMon faintedMon)
    {
        // shows the fainted animation and dialog
        yield return dialogBox.TypeDialog($"{faintedMon.Pokemon.Basic.Name} Fainted");
        faintedMon.FaintAnimation();

        yield return new WaitForSeconds(2f);

        // if it not the player give the player exp and calculate that
        if (!faintedMon.IsPlayer)
        {
            int expYield = faintedMon.Pokemon.Basic.ExpYield;
            int enemyLevel = faintedMon.Pokemon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);

            playerMon.Pokemon.Exp += expGain;

            yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} gained {expGain} exp");

            yield return playerMon.Hud.SetExpTick();

            // checks if the player monmon leveled duriong the handling of fainting
            while (playerMon.Pokemon.CheckForLevelUp())
            {
                // shows the level update and what level they go to
                playerMon.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} grew to level {playerMon.Pokemon.Level}");

                // if the monmon is trying to learn a new move and their new level
                var newMove = playerMon.Pokemon.GetLearnableMoveAtCurrent();

                // if newMove is not null try to learn it
                if (newMove != null)
                {
                    // if the monmon doesn't already know 4 moves just learn it without anything else
                    if (playerMon.Pokemon.Moves.Count < PokemonBasic.MaxNumOfMoves)
                    {
                        playerMon.Pokemon.LearnMove(newMove.Base);
                        yield return dialogBox.TypeDialog($"{playerMon.Pokemon.Basic.Name} learned {newMove.Base.Name}");
                        dialogBox.SetMoveNames(playerMon.Pokemon.Moves);
                    }
                    // if they know 4 moves player has to choose a move to forget
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

        // call CheckBattle to see the state of the battle
        CheckBattle(faintedMon);
    }

    // checks the battle if a monmon faints
    void CheckBattle(PlayerMon fainted)
    {
        // if it's the player pull up their party or lose the battle if they are out of useable mons
        // right now if you lose nothing changes but all your monmons are fainted
        if (fainted.IsPlayer)
        {
            var nextMon = playerParty.GetNotFaintedMon();

            if (nextMon != null)
                OpenParty();
            else 
                BattleOver(false);
        }
        // checks if it is wild or trainer battle
        else
        {
            // wild battle, battle ends
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            // trainer battle, if they can use their next mon call AboutToUse if they have no more healthy monmons
            // end battle
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

    // checks accuracy of the move and if it hits
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

    // shows damage detals such as super effective, not very effective, and crits
    IEnumerator ShowDamageDets(DamageDets damageDets)
    {
        if (damageDets.Crit > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");
        if (damageDets.Type > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDets.Type < 1f)
            yield return dialogBox.TypeDialog("It's not very effecitve.");
    }

    // handles all updates during the battle system
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
        // you can back out of these and not use your turn so using an action for that
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelect;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };
            // handled in InventoryUI script
            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        // trying to learn a new move if monmon has more than 4 moves already
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

    // shows visual feed back on what the user has selected and what they choose
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

        // so they can't go below index 1 and above index 3
        // basically keeps the user inbounds
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
                //StartCoroutine(RunTurns(BattleAction.UseItem));
                OpenBag();
            }
            else if (currentAction == 2)
            {
                //switch
                OpenParty();
            }
            else if (currentAction == 3)
            {
                //run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }

    // shows what moves the monmon has and visual feedback to the player
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

        // can select a move to run turns or back out
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

    // handles party selection screen and checks if the monmon is fainted, out or you are being forced to choose
    // because your previous monmon fainted
    void HandlePartySelection()
    {
        Action onSelect = () =>
        {
            var selected = partyScreen.SelectedMon;
            if (selected.currentHp <= 0)
            {
                partyScreen.SetMessage("This MonMon is fainted");
                return;
            }
            if (selected == playerMon.Pokemon)
            {
                partyScreen.SetMessage("This MonMon is already out");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if (partyScreen.CalledFrom == BattleState.ActionSelect)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;

                StartCoroutine(SwitchMon(selected, isTrainerAboutToUse));
            }
            partyScreen.CalledFrom = null;
        };

        // you can't back out of the party screen without picking a healthy monmon
        Action onBack = () =>
        {
            if (playerMon.Pokemon.currentHp <= 0)
            {
                partyScreen.SetMessage("You have to choose a MonMon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if (partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerMon());
            }
            else
                ActionSelect();

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelect, onBack);
    }

    // shows what the trainer is about to use and lets the player pick if they want to switch
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

    // handles the actual switching of the monomon
    IEnumerator SwitchMon(Pokemon newMon, bool isTrainerAboutToUse=false)
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

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerMon());
        else
            state = BattleState.RunningTurn;
    }

    // shows the trainer sending out their next monmon
    IEnumerator SendNextTrainerMon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetNotFaintedMon();

        enemyMon.Setup(nextPokemon);

        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextPokemon.Basic.Name}");

        state = BattleState.RunningTurn;
    }

    // runs item used in battle 
    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        // checks if the item was a ball
        if (usedItem is BallItem)
        {
            yield return ThrowBall((BallItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    // first checks if it is a trainer battle and if it is not then you attempt to capture the wild monmon
    // after an animation
    IEnumerator ThrowBall(BallItem ballItem)
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't catch other trainers MonMons!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a {ballItem.Name}!");

        var ballObject = Instantiate(ballSprite, playerMon.transform.position - new Vector3(2, 0), Quaternion.identity);
        var ball = ballObject.GetComponent<SpriteRenderer>();
        ball.sprite = ballItem.Icon;

        yield return ball.transform.DOJump(enemyMon.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();

        yield return enemyMon.CaptureAnimation();

        yield return ball.transform.DOLocalMoveY(enemyMon.transform.position.y - 3.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatch(enemyMon.Pokemon, ballItem);

        // shows how many times the ball shakes
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return ball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        // if the monmon was caught vs if it wasn't
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

    // formula to actually try and capture a wild monmon so we have a shake count
    int TryToCatch(Pokemon pokemon, BallItem ball)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.currentHp) * pokemon.Basic.CatchRate * ball.CatchRateModifer * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

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
    
    // if you try to run this is the attempt to escape
    // you cannot run from trainer battles
    // if you are faster than the wild monmon always escape
    // if slower run a calculation
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