using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// controller for the NPCs (non playable characters)
public class NPCController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] Dialog dialog;
    // in unity seperating out the movement and quest section of their controller
    [Header("Movement")]
    [SerializeField] List<Vector2> pattern;
    [SerializeField] float timeBetweenPattern;

    [Header("Quest")]
    [SerializeField] QuestBase quest;
    [SerializeField] QuestBase questToComplete;

    // give the npc a state an idle timer before they move their pattern again and quest
    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;
    Quest activeQuest;

    // has to know the character and if the npc gives an item or monmon
    Character character;
    ItemGive itemGive;
    MonGive monGive;

    // gets character, item, and monmon when they are applicable
    private void Awake()
    {
        character = GetComponent<Character>();
        itemGive = GetComponent<ItemGive>();
        monGive = GetComponent<MonGive>();
    }

    // handles if the player interacts with an npc
    public IEnumerator Interact(Transform start)
    {
        // look towards the player
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;

            character.LookTowards(start.position);

            // see the the player can complete a quest
            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(start);
                questToComplete = null;
                // shows this in debug log
                Debug.Log($"{quest.Base.Name} was completed");
            }

            // gives an item if it can
            if (itemGive != null && itemGive.CanBeGiven())
            {
                yield return itemGive.GiveItem(start.GetComponent<PlayerController>());
            }
            // gives a monmon if it can
            else if (monGive != null && monGive.CanBeGiven())
            {
                yield return monGive.GiveMon(start.GetComponent<PlayerController>());
            }
            // completes the quest if you are able to and sets that quest to null so you don't keep getting it
            else if (quest != null)
            {
                activeQuest = new Quest(quest);
                yield return activeQuest.StartQuest();
                quest = null;


                // Completes quest as you start, didn't like
                // due to having no contol over the option to deny turing in the quest
                // uncomment to put back in

                //if (activeQuest.CanBeCompleted())
                //{
                //    yield return activeQuest.CompleteQuest(start);
                //    activeQuest = null;
                //}
            }
            // sees if the player have an active quest with that npc
            else if (activeQuest != null)
            {
                // if the npc does have a quest and the player can complete it, compelete it
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(start);
                    activeQuest = null;
                }
                // if the npc does have a quest and the player cannot complete it, show the in progress dialog
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.MidDia);
                }
            }
            // if you have already finished their quest, got their item, or they don't have either of those
            // show the npc dialog
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }

            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    // update the npc movement
    private void Update()
    {
        if (state == NPCState.Idle)
        {
            // setting the idle timer and when the time is up, move
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0f;
                if(pattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }

        character.HandleUpdate();
    }

    // show the npc walking
    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(pattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % pattern.Count;

        state = NPCState.Idle;
    }

    // need to save npcs giving items/quests this is how to capture that state
    public object CaptureState()
    {
        var saveData = new NPCQuestSaveData();
        saveData.activeQuest = activeQuest?.GetSaveData();

        if (quest != null)
            saveData.questToStart = (new Quest(quest)).GetSaveData();
        if (questToComplete != null)
            saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();

        return saveData;
    }

    // after saving the state you need to restore the state when the player selects load
    public void RestoreState(object state)
    {
        var saveData = state as NPCQuestSaveData;
        if (saveData != null)
        {
            activeQuest = (saveData.activeQuest != null)? new Quest(saveData.activeQuest) : null;
            quest = (saveData.questToStart != null) ? new Quest(saveData.questToStart).Base : null;
            questToComplete = (saveData.questToComplete != null) ? new Quest(saveData.questToComplete).Base : null;
        }
    }
}

// Npc quest data
[System.Serializable]
public class NPCQuestSaveData
{
    public QuestSaveData activeQuest;
    public QuestSaveData questToStart;
    public QuestSaveData questToComplete;
}

// what state the npc is in
public enum NPCState { Idle, Walking, Dialog }
