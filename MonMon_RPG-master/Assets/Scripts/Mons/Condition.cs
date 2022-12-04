using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// status conditions
// TODO: add weather in the future
public class Condition
{
    public ConditionID id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string StartMess { get; set; }

    public Action<Pokemon> OnStart { get; set; }
    public Func<Pokemon, bool> OnBeforeMove { get; set; }
    public Action<Pokemon> OnAfterTurn { get; set; }
}
