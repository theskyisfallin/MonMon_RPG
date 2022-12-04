using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    private Vector2 input;

    private Character character;

    // know what chacter you are to, in this script it is the player
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // handle the player movement
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //getting rid of diaganal movement (to readd comment out code)
            if (input.x != 0)
                input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(Interact());
    }

    // when the player interacts with something check the palyer and interact
    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.blue, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, Layers.i.InteractableLayer);

        if(collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable InTrigger;

    // checks for things such as trainer fov and wild grass when the player moves over and runs these
    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, Layers.i.TriggerableLayers);

        IPlayerTriggerable triggerable = null;
        foreach (var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                if (triggerable == InTrigger && !triggerable.TriggerMulti)
                    break;

                triggerable.OnPlayerTriggered(this);
                InTrigger = triggerable;
                break;
            }
        }

        if (colliders.Count() == 0 || triggerable != InTrigger)
            InTrigger = null;
    }

    // if you need to save just save the players current position and party
    public object CaptureState()
    {
        var saveData = new PlayerSave()
        {
            position = new float[] { transform.position.x, transform.position.y },
            mons = GetComponent<Party>().Pokemon.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    // restores the players saved position and party
    public void RestoreState(object state)
    {
        var saveData = (PlayerSave)state;
        var pos = saveData.position;

        transform.position = new Vector3(pos[0], pos[1]);

        GetComponent<Party>().Pokemon = saveData.mons.Select(s => new Pokemon(s)).ToList();
    }

    public string Name
    {
        get => name;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
    public Character Character => character;
}

// players save data
[Serializable]
public class PlayerSave
{
    public float[] position;
    public List<PokemonSave> mons;
}
