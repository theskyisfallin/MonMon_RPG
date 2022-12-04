using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    // user input in Unity
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog lostDialog;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    bool battleLost = false;

    Character character;

    // gets the trainer component
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    // set the default direction of the fov box so it's not always facing down but
    // instead follows along with the trainers direction
    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    // updates
    private void Update()
    {
        character.HandleUpdate();
    }

    // if the player interacts with the trainer start a battle if they haven't already lost
    public IEnumerator Interact(Transform start)
    {
        character.LookTowards(start.position);

        if (!battleLost)
        {
            yield return DialogManager.Instance.ShowDialog(dialog);

            GameControl.Instance.StartTrainerBattle(this);
        }
        // show the lost dialog if they have already lost
        else
        {
            yield return DialogManager.Instance.ShowDialog(lostDialog);
        }
    }

    // when the player walks into the trainer fov walk towards the player and start a battle
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;

        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        yield return DialogManager.Instance.ShowDialog(dialog);

        GameControl.Instance.StartTrainerBattle(this);
    }

    // gets rid of the trainer fov if they have already lost
    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    // actually sets teh fov rotation to follow along with the trainer
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    // need to save if the player has already beat this trainer
    public object CaptureState()
    {
        return battleLost;
    }

    // when restoring make sure this trianer has the right data
    public void RestoreState(object state)
    {
        battleLost = (bool)state;

        if (battleLost)
        {
            fov.gameObject.SetActive(false);
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
