using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Made for the purpose of cleaning up code
// generic class that I connect with files, stated at the top of them
// finds the items using the name of the file and populates them
// name of the scriptable obejct and the name variable in the scriptable object MUST be the same
public class ScriptableObjectDB<T> : MonoBehaviour where T: ScriptableObject
{
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");

        foreach (var obj in objectArray)
        {
            if (objects.ContainsKey(obj.name))
            {
                Debug.LogError($"There are two items with the name {obj.name}");
                continue;
            }

            objects[obj.name] = obj;
        }
    }

    public static T GetObjectViaName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"Object \"{name}\" not found");
            return null;
        }

        return objects[name];
    }
}
