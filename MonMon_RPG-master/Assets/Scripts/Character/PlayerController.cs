using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;

    public event Action Encounter;
    public event Action<Collider2D> OnEnterTrainersView;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

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
            Interact();
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.blue, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, Layers.i.InteractableLayer);

        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }


    private void OnMoveOver()
    {
        checkForEncounter();
        CheckIfInTrainerView();
    }


    private void checkForEncounter()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, Layers.i.GrassLayer) != null)
        {
            if (Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                Encounter();
            }
        }
    }

    private void CheckIfInTrainerView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, Layers.i.FovLayer);

        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    public string Name
    {
        get => name;
    }
    public Sprite Sprite
    {
        get => sprite;
    }
}
