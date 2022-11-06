using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerMon : MonoBehaviour
{
    [SerializeField] bool isPlayer;
    [SerializeField] BattleHud hud;

    public Pokemon Pokemon { get; set; }

    public bool IsPlayer
    {
        get
        {
            return isPlayer;
        }
    }

    public BattleHud Hud
    {
        get
        {
            return hud;
        }
    }

    Image image;

    Vector3 orginalPos;

    Color orginalCol;

    private void Awake()
    {
        image = GetComponent<Image>();
        orginalPos = image.transform.localPosition;
        orginalCol = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if (isPlayer)
            image.sprite = Pokemon.Basic.Back;
        else
            image.sprite = Pokemon.Basic.Front;

        hud.gameObject.SetActive(true);
        hud.SetHud(pokemon);

        transform.localScale = new Vector3(1, 1, 1);
        image.color = orginalCol;
        EnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void EnterAnimation()
    {
        if (isPlayer)
            image.transform.localPosition = new Vector3(-500f, orginalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, orginalPos.y);

        image.transform.DOLocalMoveX(orginalPos.x, 1f);
    }

    public void AttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayer)
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x + 50f, 0.25f));
        else
            sequence.Append(image.transform.DOLocalMoveX(orginalPos.x - 50f, 0.25f));

        sequence.Append(image.transform.DOLocalMoveX(orginalPos.x, 0.25f));
    }

    public void HitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(orginalCol, 0.1f));
    }

    public void FaintAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(orginalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0f, 0.5f));
    }

    public IEnumerator CaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y + 150f, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator BreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(image.transform.DOLocalMoveY(orginalPos.y, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
