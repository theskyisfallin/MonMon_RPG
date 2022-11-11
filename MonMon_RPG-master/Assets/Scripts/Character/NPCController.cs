using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;
    [SerializeField] List<Vector2> pattern;
    [SerializeField] float timeBetweenPattern;

    NPCState state;
    float idleTimer = 0f;
    int currentPattern = 0;

    Character character;
    ItemGive itemGive;

    private void Awake()
    {
        character = GetComponent<Character>();
        itemGive = GetComponent<ItemGive>();
    }

    public IEnumerator Interact(Transform start)
    {

        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;

            character.LookTowards(start.position);

            if (itemGive != null && itemGive.CanBeGiven())
            {
                yield return itemGive.GiveItem(start.GetComponent<PlayerController>());
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }

            idleTimer = 0f;
            state = NPCState.Idle;
        }
    }

    private void Update()
    {
        if (state == NPCState.Idle)
        {
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

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        var oldPos = transform.position;

        yield return character.Move(pattern[currentPattern]);

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % pattern.Count;

        state = NPCState.Idle;
    }
}

public enum NPCState { Idle, Walking, Dialog }
