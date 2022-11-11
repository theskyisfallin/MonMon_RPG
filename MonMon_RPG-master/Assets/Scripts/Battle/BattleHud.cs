using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
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

    Dictionary<ConditionID, Color> statusColor;

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

    public void SetLevel()
    {
        level.text = "Lv. " + mon.Level;
    }

    public void SetExp()
    {
        if (expBar == null) return;

        float normalExp = GetNormalExp();

        expBar.transform.localScale = new Vector3(normalExp, 1, 1);
    }

    float GetNormalExp()
    {
        int currLevelExp = mon.Basic.GetExpForLevel(mon.Level);
        int nextLevelExp = mon.Basic.GetExpForLevel(mon.Level + 1);

        float normalExp = (float)(mon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);

        return Mathf.Clamp01(normalExp);
    }

    public IEnumerator SetExpTick(bool reset=false)
    {
        if (expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalExp = GetNormalExp();

        yield return expBar.transform.DOScaleX(normalExp, 1.5f).WaitForCompletion();
    }

    public void UpdateHp()
    {
        StartCoroutine(UpdateHpAsync());
    }

    public IEnumerator UpdateHpAsync()
    {
        yield return hp.tickHp((float)mon.currentHp / mon.MaxHp);
    }

    public IEnumerator WaitForHpUpdate()
    {
        yield return new WaitUntil(() => hp.IsUpdating == false);
    }

    public void ClearData()
    {
        if (mon != null)
        {
            mon.OnStatusChange -= SetStatusText;
            mon.OnHpChange -= UpdateHp;
        }
    }
}
