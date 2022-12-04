using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBox : MonoBehaviour
{
    // user input from Unity
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

    // get text highlight color from global settings
    private void Start()
    {
        highlight = GlobalSettings.i.Hightlight;
    }

    // setting the dialog for the battle dialog box
    public void SetDialog(string dialog)
    {
        diaText.text = dialog;
    }

    // how I type out the dialog character by character to show the player
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

    // if you need to enable dialog text it is done with this function
    public void EnableDialogText(bool enabled)
    {
        diaText.enabled = enabled;
    }

    // enables which election you are in during battle
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

    // updates what the player is on and highlights it for all the Update____ functions
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

    // Sets the move names in the players list
    // if you don't have 4 moves just show - where you don't have one
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
