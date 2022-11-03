using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ConditionsDB
{
    public static Dictionary<ConditonID, Condition> Conditions { get; set; } = new Dictionary<ConditonID, Condition>()
    {
        {
            ConditonID.psn, new Condition()
            {
                Name = "Poison",
                StartMess = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} got hurt from poison");
                }
            }
        },
        {
            ConditonID.brn, new Condition()
            {
                Name = "Burn",
                StartMess = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.UpdateHp(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} got hurt from burning");
                }
            }
        },
        {
            ConditonID.par, new Condition()
            {
                Name = "Paralyzed",
                StartMess = "has been paralyzed",
                // true (able to preform a move || false (unable to preform a move)
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} is paralyzed and can't move");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditonID.frz, new Condition()
            {
                Name = "Freeze",
                StartMess = "has been frozen",
                // true (cure status) || false (fail to cure status and can't move)
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} thawed out");
                        return true;
                    }

                    return false;
                }
            }
        },
        {
            ConditonID.slp, new Condition()
            {
                Name = "Sleep",
                StartMess = "has fallen asleep",
                OnStart = (Pokemon pokemon) =>
                {
                    //sleep timer
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} woke up");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} is fast asleep");
                    return false;
                }
            }
        }
    };
}

public enum ConditonID
{
    none, psn, brn, slp, par, frz
}
