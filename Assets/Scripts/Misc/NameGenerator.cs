using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class NameGenerator : MonoBehaviour {

    public string category = "Default";
    public int maxCharacter = 10;
    [TextArea (3, 20)]
    public string sample;

    MarkovChainDatabase table = new MarkovChainDatabase ();

    void Start () {
        LoadDatabase ();
    }
    
    public string GenerateName () {
        string result = "";

        Prefix currentPrefix = new Prefix (2);
        Suffix currentSuffix;
        do {
            currentSuffix = table.GetSuffixOf (currentPrefix);

            if (currentSuffix.value != "*") {
                result += currentSuffix.value;
            }

            currentPrefix.Add (currentSuffix.value[0]);
        } while (currentSuffix.value != "*" && result.Length < maxCharacter);

        return result;
    }

    public void BuildDatabase () {
        Prefix currentPrefix = new Prefix (2);

        for (int i = 0; i <= sample.Length; i++) {
            char next = i < sample.Length ? CheckCharOf (sample[i]) : '*';
            Suffix currentSuffix = new Suffix (next);
            AddToDatabase (currentPrefix, currentSuffix);
            currentPrefix.Add (next);
        }

        SaveDatabase ();
        Debug.Log ("Finished building database!");
    }

    char CheckCharOf (char input) {
        char output = char.ToLower (input);
        return char.IsLetter (output) ? output : '*';
    }

    void AddToDatabase (Prefix prefix, Suffix suffix) {
        string prefixValue = new string (prefix.value);

        if (prefixValue == "**" && suffix.value == "*") {
            // Don't add [** -> *]
            return;
        }

        int index = table.database.FindIndex (p => (p.prefix == prefixValue));
        if (index != -1) {
            table.database[index].AddSuffix (suffix);
        } else {
            table.database.Add (new PreSuffixEntry (prefixValue, suffix));
        }
    }

    void SaveDatabase () {
        JsonFile.Save (category + "NameDatabase.json", "NameGenerator", table);
    }

    void LoadDatabase () {
        JsonFile.Load (category + "NameDatabase.json", "NameGenerator", ref table);
    }

}

[System.Serializable]
public class MarkovChainDatabase {
    public List<PreSuffixEntry> database = new List<PreSuffixEntry> ();

    public Suffix GetSuffixOf (Prefix prefix) {
        string prefixValue = new string (prefix.value);
        int index = database.FindIndex (p => (p.prefix == prefixValue));
        if (index != -1) {
            int multiplicitySum = database[index].suffixes.Select (m => m.multiplicity).ToArray ().Sum ();
            int roll = Random.Range (0, multiplicitySum);

            int cumulative = 0;
            for (int i = 0; i < database[index].suffixes.Count; i++) {
                Suffix suffix = database[index].suffixes[i];
                cumulative += suffix.multiplicity;

                if (roll < cumulative) {
                    return suffix;
                }
            }
        }

        return new Suffix ('*');
    }
}

[System.Serializable]
public class PreSuffixEntry {

    public string prefix = "";
    public List<Suffix> suffixes = new List<Suffix> ();

    public PreSuffixEntry () { }
    public PreSuffixEntry (string pre, Suffix suf) {
        prefix = pre;
        AddSuffix (suf);
    }

    public void AddSuffix (Suffix suf) {
        int index = suffixes.FindIndex (s => s.value == suf.value);
        if (index != -1) {
            suffixes[index].multiplicity++;
        } else {
            suffixes.Add (suf);
        }
    }
}

public class Prefix {

    public char[] value;
    int length;

    public Prefix(int size) {
        length = size;
        value = new char[length];
        for (int i = 0; i < length; i++) {
            value[i] = '*';
        }
    }

    public void Add (char newChar) {
        for (int i = 0; i < length - 1; i++) {
            value[i] = value[i + 1];
        }
        value[length - 1] = newChar;
    }
}

[System.Serializable]
public class Suffix {

    public string value;
    public int multiplicity = 1;

    public Suffix (char val) {
        value = new string (val, 1);
    }

}
