using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoryGenerator : MonoBehaviour {

    public string category = "Default";
    public int maxWords = 10;
    [TextArea (3, 20)]
    public string sample;

    MarkovChainStoryDatabase table = new MarkovChainStoryDatabase ();

    void Start () {
        LoadDatabase ();
    }
    
    public string GenerateStory () {
        string result = "";

        int wordCount = 0;
        PrefixWord currentPrefix = new PrefixWord (2);
        SuffixWord currentSuffix;
        do {
            currentSuffix = table.GetSuffixOf (currentPrefix);

            if (currentSuffix.value != "*") {
                result += currentSuffix.value + " ";
            } else {
                result += '\n';
            }

            currentPrefix.Add (currentSuffix.value);
            wordCount++;
        } while (/*currentSuffix.value != "*" && */wordCount < maxWords);

        return result;
    }

    public void BuildDatabase () {
        PrefixWord currentPrefix = new PrefixWord (2);

        char[] delimiterChars = { ' ', '\t', '\n', '\r' };
        string[] words = sample.Split (delimiterChars, StringSplitOptions.RemoveEmptyEntries);

        //string pattern = @"\s?";
        //string[] words = Regex.Split (sample, pattern);

        for (int i = 0; i <= words.Length; i++) {
            string next = i < words.Length ? words[i] : "*";
            SuffixWord currentSuffix = new SuffixWord (next);
            AddToDatabase (currentPrefix, currentSuffix);
            currentPrefix.Add (next);
        }

        SaveDatabase ();
		LoggerTool.Post ("Finished building story database!");
	}
    
    void AddToDatabase (PrefixWord prefix, SuffixWord suffix) {
        string prefixValue = prefix.GetConcatenatedPrefix ();

        if (prefixValue == "*|*" && suffix.value == "*") {
            // Don't add [*|* -> *]
            return;
        }

        int index = table.database.FindIndex (p => (p.prefix == prefixValue));
        if (index != -1) {
            table.database[index].AddSuffix (suffix);
        } else {
            table.database.Add (new PreSuffixWordEntry (prefixValue, suffix));
        }
    }

    void SaveDatabase () {
        JsonFile.Save (category + "StoryDatabase.json", "StoryGenerator", table);
    }

    void LoadDatabase () {
        JsonFile.Load (category + "StoryDatabase.json", "StoryGenerator", ref table);
    }

}

[System.Serializable]
public class MarkovChainStoryDatabase {
    public List<PreSuffixWordEntry> database = new List<PreSuffixWordEntry> ();

    public SuffixWord GetSuffixOf (PrefixWord prefix) {
        int index = database.FindIndex (p => (p.prefix == prefix.GetConcatenatedPrefix ()));
        if (index != -1) {
            int multiplicitySum = database[index].suffixes.Select (m => m.multiplicity).ToArray ().Sum ();
            int roll = Random.Range (0, multiplicitySum);

            int cumulative = 0;
            for (int i = 0; i < database[index].suffixes.Count; i++) {
                SuffixWord suffix = database[index].suffixes[i];
                cumulative += suffix.multiplicity;

                if (roll < cumulative) {
                    return suffix;
                }
            }
        }

        return new SuffixWord ("*");
    }
}

[System.Serializable]
public class PreSuffixWordEntry {

    public string prefix = "";
    public List<SuffixWord> suffixes = new List<SuffixWord> ();

    public PreSuffixWordEntry () { }
    public PreSuffixWordEntry (string pre, SuffixWord suf) {
        prefix = pre;
        AddSuffix (suf);
    }

    public void AddSuffix (SuffixWord suf) {
        int index = suffixes.FindIndex (s => s.value == suf.value);
        if (index != -1) {
            suffixes[index].multiplicity++;
        } else {
            suffixes.Add (suf);
        }
    }
}

public class PrefixWord {

    public string[] value;
    int length;

    public PrefixWord(int size) {
        length = size;
        value = new string[length];
        for (int i = 0; i < length; i++) {
            value[i] = "*";
        }
    }

    public void Add (string newWord) {
        for (int i = 0; i < length - 1; i++) {
            value[i] = value[i + 1];
        }
        value[length - 1] = newWord;
    }

    public string GetConcatenatedPrefix () {
        string con = "";
        if (length <= 0) {
            return con;
        }

        for (int i = 0; i < length - 1; i++) {
            con += value[i] + "|";
        }

        return con + value[length - 1];
    }
}

[System.Serializable]
public class SuffixWord {

    public string value;
    public int multiplicity = 1;

    public SuffixWord (string val) {
        value = val;
    }

}
