using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tracks the user's knowledge state for each GHS symbol throughout the session
public class KnowledgeStateManager : MonoBehaviour
{
    public static KnowledgeStateManager Instance;

    // Stores the state of each symbol by its id
    Dictionary<string, SymbolState> states = new Dictionary<string, SymbolState>();
    private Dictionary<string, string> lastWrongAnswer = new Dictionary<string, string>();
    private Dictionary<string, string> confusedWithSymbolId = new Dictionary<string, string>();

    public enum SymbolState
    {
        Untested,
        KnownBefore,    // correct in pre-assessment
        LearnedDuring,  // wrong in pre-assessment, correct in training
        Struggling      // wrong in both pre-assessment and training
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
            if (states[symbol.id] == SymbolState.Struggling)
                unknown.Add(symbol);
        }
        return unknown;
    }

    public void RecordWrongAnswer(string symbolId, string wrongAnswerText)
    {
        lastWrongAnswer[symbolId] = wrongAnswerText;
    }

    public string GetLastWrongAnswer(string symbolId)
    {
        return lastWrongAnswer.ContainsKey(symbolId) ? lastWrongAnswer[symbolId] : null;
    }

    public void RecordConfusedSymbol(string symbolId, string confusedSymbolId)
    {
        confusedWithSymbolId[symbolId] = confusedSymbolId;
    }

    public string GetConfusedSymbol(string symbolId)
    {
        return confusedWithSymbolId.ContainsKey(symbolId) ? confusedWithSymbolId[symbolId] : null;
    }

    // Returns count of symbols known before training
    public int GetKnownBeforeTraining()
    {
        int count = 0;
        foreach (var state in states.Values)
            if (state == SymbolState.KnownBefore)
                count++;
        return count;
    }

    // Returns count of symbols learned during training
    public int GetLearnedDuringTraining()
    {
        int count = 0;
        foreach (var state in states.Values)
            if (state == SymbolState.LearnedDuring)
                count++;
        return count;
    }

    // Returns count of symbols still not mastered
    public int GetStillStruggling()
    {
        int count = 0;
        foreach (var state in states.Values)
            if (state == SymbolState.Struggling)
                count++;
        return count;
    }
}
