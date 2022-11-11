using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBox : MonoBehaviour
{
    [SerializeField] Text diaText;
    [SerializeField] int textSpeed;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<Text> actionText;
    [SerializeField] List<Text> moveText;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;
    [SerializeField] Text yesText;
    [SerializeField] Text noText;

    Color highlight;

    private void Start()
    {
        highlight = GlobalSettings.i.Hightlight;
    }

    public void SetDialog(string dialog)
    {
        diaText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        diaText.text = "";
        foreach (var i in dialog.ToCharArray())
        {
            diaText.text += i;
            yield return new WaitForSeconds(1f / textSpeed);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        diaText.enabled = enabled;
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for (int i = 0; i < actionText.Count; i++)
        {
            if (i == selectedAction)
                actionText[i].color = highlight;
            else
                actionText[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for (int i = 0; i < moveText.Count; i++)
        {
            if (i == selectedMove)
                moveText[i].color = highlight;
            else
                moveText[i].color = Color.black;
        }

        ppText.text = $"PP {move.Pp}/{move.Base.Pp}";
        typeText.text = move.Base.Type1.ToString();

        if (move.Pp == 0)
            ppText.color = Color.red;
        else
            ppText.color = Color.black;
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if (yesSelected)
        {
            yesText.color = highlight;
            noText.color = Color.black;
        }
        else
        {
            yesText.color = Color.black;
            noText.color = highlight;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveText.Count; i++)
        {
            if (i < moves.Count)
                moveText[i].text = moves[i].Base.Name;
            else
                moveText[i].text = "-";
        }
    }
}
