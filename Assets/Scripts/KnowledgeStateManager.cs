using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tracks the user's knowledge state for each GHS symbol throughout the session
public class KnowledgeStateManager : MonoBehaviour
{
    public static KnowledgeStateManager Instance;

    // Stores the state of each symbol by its id
    Dictionary<string, SymbolState> states = new Dictionary<string, SymbolState>();

    public enum SymbolState
    {
        Untested,
        Known,
        Unknown
    }

    void Awake()
    {
        Instance = this;
    }

    // Initialise all symbols as Untested at the start of the session
    public void Initialise()
    {
        states.Clear();
        foreach (GHSSymbol symbol in GHSDataLoader.Database.symbols)
        {
            states[symbol.id] = SymbolState.Untested;
        }
        Debug.Log("Knowledge state initialised for all symbols.");
    }

    public void SetState(string id, SymbolState state)
    {
        if (states.ContainsKey(id))
            states[id] = state;
    }

    public SymbolState GetState(string id)
    {
        if (states.ContainsKey(id))
            return states[id];
        return SymbolState.Untested;
    }

    // Returns only symbols the user has not yet learned
    public List<GHSSymbol> GetUnknownSymbols()
    {
        List<GHSSymbol> unknown = new List<GHSSymbol>();
        foreach (GHSSymbol symbol in GHSDataLoader.Database.symbols)
        {
            if (states[symbol.id] != SymbolState.Known)
                unknown.Add(symbol);
        }
        return unknown;
    }
}
