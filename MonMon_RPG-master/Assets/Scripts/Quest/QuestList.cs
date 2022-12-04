using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// player's quest list and stores where they are in the quest
public class QuestList : MonoBehaviour, ISavable
{
    List<Quest> quests = new List<Quest>();

    public event Action OnUpdate;

    // adds a quest to the player quest list
    public void AddQuest(Quest quest)
    {
        if (!quests.Contains(quest))
            quests.Add(quest);

        OnUpdate?.Invoke();
    }

    // if the player has already started the quest give progress
    public bool IsStarted(string questName)
    {
        var questProgress = quests.FirstOrDefault(q => q.Base.Name == questName)?.Progress;
        return questProgress == QuestProgress.Started || questProgress == QuestProgress.Completed;
    }

    // when the player compeltes the quest
    public bool IsCompleted(string questName)
    {
        var questProgress = quests.FirstOrDefault(q => q.Base.Name == questName)?.Progress;
        return questProgress == QuestProgress.Completed;
    }

    // fetches the quest list
    public static QuestList GetQuestList()
    {
        return FindObjectOfType<PlayerController>().GetComponent<QuestList>();
    }

    // needs to be able to be saved so captures the list
    public object CaptureState()
    {
        return quests.Select(q => q.GetSaveData()).ToList();
    }

    // restores the quest list
    public void RestoreState(object state)
    {
        var saveData = state as List<QuestSaveData>;
        if (saveData != null)
        {
            quests = saveData.Select(q => new Quest(q)).ToList();
            OnUpdate?.Invoke();
        }
    }
}
