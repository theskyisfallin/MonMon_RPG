using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this is an interface to show that this is player triggerable
public interface IPlayerTriggerable
{
    void OnPlayerTriggered(PlayerController playerxw);

    bool TriggerMulti { get; }
}
