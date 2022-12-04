using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    // user input for dialog box, text, and speed
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int textSpeed;

    // public actions
    public event Action OnShowDialog;
    public event Action OnDialogEnd;

    public static DialogManager Instance { get; private set; }

    // on awake get this instance
    public void Awake()
    {
        Instance = this;
    }

    public bool IsShowing { get; private set; }

    // show dialog text in box
    public IEnumerator ShowDialogText(string text, bool waitForInput=true, bool autoClose=true)
    {
        OnShowDialog?.Invoke();
        IsShowing = true;
        dialogBox.SetActive(true);

        yield return TypeDialog(text);

        // if needing user input to close wait until Z is pressed
        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }
        // if not wait and then close
        if (autoClose)
        {
            CloseDialog();
        }
        OnDialogEnd?.Invoke();
    }

    // closes the dialog box
    public void CloseDialog()
    {
        dialogBox.SetActive(false);
        IsShowing = false;
    }

    // shows the dialog box and calls to TypeDialog funciton
    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();
        OnShowDialog?.Invoke();

        IsShowing = true;

        dialogBox.SetActive(true);

        foreach (var line in dialog.Lines)
        {
            yield return TypeDialog(line);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        dialogBox.SetActive(false);
        IsShowing = false;
        OnDialogEnd?.Invoke();
    }

    // doesn't do anything right now
    public void HandleUpdate()
    {
        // TODO: add yes/no options in dialog
    }

    // types out dialog charcter by character
    public IEnumerator TypeDialog(string line)
    {
        dialogText.text = "";
        foreach (var i in line.ToCharArray())
        {
            dialogText.text += i;
            yield return new WaitForSeconds(1f / textSpeed);
        }
    }
}
