using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// sets up the indiviudal quests in game
[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestProgress Progress { get; private set; }

    // was just base but that is a resevered word _ is important
    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    // creates the quest and where you are in it from save data
    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectViaName(saveData.name);
        Progress = saveData.progress;
    }

    // fetches save data
    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            progress = Progress
        };
        return saveData;
    }

    // starting the quest and sets teh enum to so
    public IEnumerator StartQuest()
    {
        Progress = QuestProgress.Started;

        yield return DialogManager.Instance.ShowDialog(Base.StartDia);

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    // complete the quest and add the reward to the inventory/tell player
    public IEnumerator CompleteQuest(Transform player)
    {
        Progress = QuestProgress.Completed;

        yield return DialogManager.Instance.ShowDialog(Base.EndDia);

        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
        }

        if (Base.RewardItem != null)
        {
            inventory.AddItem(Base.RewardItem);

            string playerName = player.GetComponent<PlayerController>().Name;
            yield return DialogManager.Instance.ShowDialogText($"{playerName} received {Base.RewardItem.Name}");
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    // checks the player's inventory if they can complete the quest
    public bool CanBeCompleted()
    {
        var inventory = Inventory.GetInventory();
        if (Base.RequiredItem != null)
        {
            if (!inventory.HasItem(Base.RequiredItem))
                return false;
        }

        return true;
    }
}

// stores the quest save datas
[System.Serializable]
public class QuestSaveData
{
    public string name;
    public QuestProgress progress;
}

public enum QuestProgress { None, Started, Completed }
