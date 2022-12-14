using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    // user input set in unity
    [SerializeField] Text name;
    [SerializeField] Text level;
    [SerializeField] Text status;
    [SerializeField] HP hp;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;

    Pokemon mon;

    // using a dictonary for statuses and the color they have
    Dictionary<ConditionID, Color> statusColor;

    // sets the battle hud at the start of a battle
    // health, level, the mons, exp, and condition
    public void SetHud(Pokemon pokemon)
    {
        if (mon != null)
        {
            mon.OnStatusChange -= SetStatusText;
            mon.OnHpChange -= UpdateHp;
        }

        mon = pokemon;
        name.text = pokemon.Basic.Name;
        SetLevel();
        hp.SetHp((float)pokemon.currentHp / pokemon.MaxHp);
        SetExp();

        statusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor },
            {ConditionID.brn, brnColor },
            {ConditionID.slp, slpColor },
            {ConditionID.par, parColor },
            {ConditionID.frz, frzColor }
        };
        SetStatusText();
        mon.OnStatusChange += SetStatusText;
        mon.OnHpChange += UpdateHp;
    }

    // if you don't have a condition don't show if you do show that
    void SetStatusText()
    {
        if(mon.Status == null)
        {
            status.text = "";
        }
        else
        {
            status.text = mon.Status.id.ToString().ToUpper();
            status.color = statusColor[mon.Status.id];
        }
    }

    // sets level of the current mon
    public void SetLevel()
    {
        level.text = "Lv. " + mon.Level;
    }

    // sets exp of the current mon on a normalized scale
    public void SetExp()
    {
        if (expBar == null) return;

        float normalExp = GetNormalExp();

        expBar.transform.localScale = new Vector3(normalExp, 1, 1);
    }

    // normalizes the exp scale
    float GetNormalExp()
    {
        int currLevelExp = mon.Basic.GetExpForLevel(mon.Level);
        int nextLevelExp = mon.Basic.GetExpForLevel(mon.Level + 1);

        float normalExp = (float)(mon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);

        return Mathf.Clamp01(normalExp);
    }

    // shows the animation when gaining exp
    public IEnumerator SetExpTick(bool reset=false)
    {
        if (expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalExp = GetNormalExp();

        yield return expBar.transform.DOScaleX(normalExp, 1.5f).WaitForCompletion();
    }
    // update Hp
    public void UpdateHp()
    {
        StartCoroutine(UpdateHpAsync());
    }

    // show the hp going down over time not just instantly
    public IEnumerator UpdateHpAsync()
    {
        yield return hp.tickHp((float)mon.currentHp / mon.MaxHp);
    }

    // waiting for updates
    public IEnumerator WaitForHpUpdate()
    {
        yield return new WaitUntil(() => hp.IsUpdating == false);
    }

    // clear the data if needed
    public void ClearData()
    {
        if (mon != null)
        {
            mon.OnStatusChange -= SetStatusText;
            mon.OnHpChange -= UpdateHp;
        }
    }
}
