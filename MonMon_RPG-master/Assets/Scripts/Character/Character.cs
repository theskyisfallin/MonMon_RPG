using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// script for all characters in game such as player, npc, and trainer
public class Character : MonoBehaviour
{
    public float moveSpeed;

    public bool IsMoving { get; private set; }

    public float OffsetY { get; private set; } = 0.3f;

    CharacterAnimator animator;

    // gets the animator connected and snaps characters to the center of their tile
    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
        SetPositionAndSnap(transform.position);
    }

    // snaps to the center of the tile plus an offset on Y if wanted
    // mine is 0.3 to give some more depth when playing
    public void SetPositionAndSnap(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }

    // handles the movement of characters
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        // because npcs can have movement patterns that aren't just the tile in front of them
        // they must check if all tiles in their path are clear before walking
        if (!IsPathClear(targetPos))
            yield break;

        IsMoving = true;
        //takes current position and target position and if it's greater than a small amount start to move to target until
        //current position and target are too close and breaks the loop then does the last little bit outside the loop
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;
        // handles the onmoveover action
        OnMoveOver?.Invoke();
    }

    // shows walking animation if you are walking
    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    // checks if the path for npcs is clear before they walk
    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized;

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, Layers.i.SoildLayer | Layers.i.InteractableLayer | Layers.i.PlayerLayer) == true)
            return false;

        return true;
    }

    // checks if the area that a character is trying to walk if walkable
    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, Layers.i.SoildLayer | Layers.i.InteractableLayer) != null)
        {
            return false;
        }

        return true;
    }

    // makes the character look at the player if they are talked to 
    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);

        if (xdiff == 0 || ydiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xdiff, -1f, 1f);
            animator.MoveY = Mathf.Clamp(ydiff, -1f, 1f);
        }
        // if you somehow talk to an npc from a diagonal direction they can't
        else
            Debug.LogError("Error in LookTowards: You cannot ask to look diagonal");
    }

    // get the character animator
    public CharacterAnimator Animator
    {
        get => animator;
    }
}
