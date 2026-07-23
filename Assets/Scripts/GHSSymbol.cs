using System.Collections.Generic;

// Represents a single GHS hazard symbol loaded from ghs_symbols.json
[System.Serializable]
public class GHSSymbol
{
    public string id;
    public string display_name;
    public string image_resource;
    public string correct_option;
    public string correct_meaning;
    public List<string> wrong_options;
    public string training_tip;
}

// Represents the full JSON file as a list of all GHS symbols
[System.Serializable]
public class GHSSymbolDatabase
{
    public List<GHSSymbol> symbols;
}