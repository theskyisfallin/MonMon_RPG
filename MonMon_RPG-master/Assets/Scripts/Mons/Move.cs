using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// builds individual moves
public class Move
{
    public MoveBasic Base { get; set; }

    public int Pp { get; set; }

    // base class pluss the power points
    public Move(MoveBasic pBase)
    {
        Base = pBase;
        Pp = pBase.Pp;
    }

    // gets moves from their names using save data
    public Move(MoveSave saveData)
    {
        Base = MoveDB.GetObjectViaName(saveData.name);
        Pp = saveData.pp;
    }

    // fetches save data and returns it
    public MoveSave GetSaveData()
    {
        var saveData = new MoveSave()
        {
            name = Base.name,
            pp = Pp
        };
        return saveData;
    }

    // used when a power point recovery item is used
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
