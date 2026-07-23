using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Loads GHS symbol data from JSON at startup and makes it globally accessible
public class GHSDataLoader : MonoBehaviour
{
    public static GHSSymbolDatabase Database;

    void Awake()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("ghs_symbols");

        if (jsonFile == null)
        {
            Debug.LogError("ghs_symbols.json not found in Resources folder.");
            return;
        }

        // Parse JSON into C# objects and store it in Database
        Database = JsonUtility.FromJson<GHSSymbolDatabase>(jsonFile.text);

        Debug.Log($"Loaded {Database.symbols.Count} GHS symbol(s) successfully.");
    }
}
