using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// creating a quest scriptable object
[CreateAssetMenu(menuName = "Quest/Create new quest")]
public class QuestBase : ScriptableObject
{
    // user input in Unity
    [SerializeField] string name;
    [SerializeField] string desc;

    [SerializeField] Dialog startDia;
    [SerializeField] Dialog midDia;
    [SerializeField] Dialog endDia;

    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    // exposing vars
    public string Name => name;
    public string Desc => desc;

    public Dialog StartDia => startDia;
    public Dialog MidDia => midDia?.Lines?.Count > 0 ? midDia : startDia;
    public Dialog EndDia => endDia;

    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
