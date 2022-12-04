using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialog
{
    // user input for the lines in dialog
    [SerializeField] List<string> lines;

    public List<string> Lines
    {
        get { return lines; }
    }
}
