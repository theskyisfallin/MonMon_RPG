using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    [SerializeField] GameObject health;

    public bool IsUpdating { get; private set; }

    // set normaized Hp on the bar
    public void SetHp(float hpNormal)
    {
        health.transform.localScale = new Vector3(hpNormal, 1f);
    }

    // done a different way than the exp because i did it before i found DOtween
    // sets healthy down over time on a normalized scale
    public IEnumerator tickHp(float newHp)
    {
        IsUpdating = true;

        float currentHp = health.transform.localScale.x;
        float change = currentHp - newHp;

        while (currentHp - newHp > Mathf.Epsilon)
        {
            currentHp -= change * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHp, 1f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
        IsUpdating = false;
    }
}
