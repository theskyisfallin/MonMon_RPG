using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDB
{
    static Dictionary<string, MoveBasic> moves;

    public static void Init()
    {
        moves = new Dictionary<string, MoveBasic>();

        var moveList = Resources.LoadAll<MoveBasic>("");

        foreach (var move in moveList)
        {
            if (moves.ContainsKey(move.Name))
            {
                Debug.LogError($"There are two moves with the name {move.Name}");
                continue;
            }

            moves[move.Name] = move;
        }
    }

    public static MoveBasic GetMoveViaName(string name)
    {
        if (!moves.ContainsKey(name))
        {
            Debug.LogError($"Move \"{name}\" not found");
            return null;
        }

        return moves[name];
    }
}
