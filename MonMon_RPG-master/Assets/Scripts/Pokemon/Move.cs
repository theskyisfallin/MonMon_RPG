using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    public MoveBasic Base { get; set; }

    public int Pp { get; set; }

    public Move(MoveBasic pBase)
    {
        Base = pBase;
        Pp = pBase.Pp;
    }
}
