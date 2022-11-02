using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerMon : MonoBehaviour
{
    [SerializeField] bool isPlayer;

    public Pokemon Pokemon { get; set; }

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


        image.color = orginalCol;
        EnterAnimation();
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
}
