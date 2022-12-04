using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] QuestBase questCheck;
    [SerializeField] ObjectActions onStart;
    [SerializeField] ObjectActions onEnd;

    QuestList questList;

    // on start get the quest list and subscribe to UpdateObject
    private void Start()
    {
        questList = QuestList.GetQuestList();
        questList.OnUpdate += UpdateObject;

        UpdateObject();
    }

    // on destory unsubscribe to UpdateObject because we don't care about it anymore
    public void OnDestroy()
    {
        questList.OnUpdate -= UpdateObject;
    }

    // show quest obejct if needed at the start of a quest and make sure it is saveable and active
    public void UpdateObject()
    {
        if (onStart != ObjectActions.None && questList.IsStarted(questCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onStart == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var savable = child.GetComponent<SavableEntity>();

                    if (savable != null)
                        SavingSystem.i.RestoreEntity(savable);
                }
                else if (onStart == ObjectActions.Disable)
                    child.gameObject.SetActive(false);
            }
        }

        // when you finish save it and disable the obejct
        if (onEnd != ObjectActions.None && questList.IsCompleted(questCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (onEnd == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    var savable = child.GetComponent<SavableEntity>();

                    if (savable != null)
                        SavingSystem.i.RestoreEntity(savable);
                }
                else if (onEnd == ObjectActions.Disable)
                    child.gameObject.SetActive(false);
            }
        }
    }
}

public enum ObjectActions { None, Enable, Disable }
