using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// while I technically call this a data base it is not but it is a dictionary that holds my conditions
public class ConditionsDB
{
    // inits the keys and values pair in my dictonary
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.id = conditionId;
        }
    }

    // shows the conditon, what it does, it's message, and it's name
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn, new Condition()
            {
                Name = "Poison",
                StartMess = "has been poisoned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHp(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} got hurt from poison");
                }
            }
        },
        {
            ConditionID.brn, new Condition()
            {
                Name = "Burn",
                StartMess = "has been burned",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHp(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} got hurt from burning");
                }
            }
        },
        {
            ConditionID.par, new Condition()
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
            ConditionID.frz, new Condition()
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
            ConditionID.slp, new Condition()
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
        },
        // these are the dynamic statuses, similar to sleep but doesn't save on switch out or battle end
        {
            ConditionID.confusion, new Condition()
            {
                Name = "Confusion",
                StartMess = "has been confused",
                OnStart = (Pokemon pokemon) =>
                {
                    //confusion timer
                    pokemon.dynamicStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confused for {pokemon.dynamicStatusTime} turns");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if(pokemon.dynamicStatusTime <= 0)
                    {
                        pokemon.CureDynamicStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} snapped out of confusion!");
                        return true;
                    }

                    pokemon.dynamicStatusTime--;

                    if(Random.Range(1,3) == 1)
                        return true;


                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} is confused");
                    pokemon.DecreaseHp(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Basic.Name} hurt itself in confusion");

                    return false;
                }
            }
        }
    };

    // modifies catach rate if the wild monmon has a status
    public static float GetStatusBonus(Condition condition)
    {
        if (condition == null)
            return 1f;
        else if (condition.id == ConditionID.slp || condition.id == ConditionID.frz)
            return 2f;
        else if (condition.id == ConditionID.brn || condition.id == ConditionID.psn || condition.id == ConditionID.par)
            return 1.5f;

        return 1f;

    }
}

// which status the monmon has
public enum ConditionID
{
    none, psn, brn, slp, par, frz, confusion
}
