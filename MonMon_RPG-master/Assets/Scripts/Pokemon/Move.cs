using System;
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

    public Move(MoveSave saveData)
    {
        Base = MoveDB.GetMoveViaName(saveData.name);
        Pp = saveData.pp;
    }

    public MoveSave GetSaveData()
    {
        var saveData = new MoveSave()
        {
            name = Base.Name,
            pp = Pp
        };
        return saveData;
    }

    public void IncreasePp(int amount)
    {
        Pp = Mathf.Clamp(Pp + amount, 0, Base.Pp);
    }
}

[Serializable]
public class MoveSave
{
    public string name;
    public int pp;
}
