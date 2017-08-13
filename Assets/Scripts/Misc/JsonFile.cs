using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class JsonFile {

    public static string[] GetAllFilesIn (string folder) {
        string folderPath = Application.persistentDataPath + "/" + folder;

        if (!Directory.Exists (folderPath)) {
            Debug.LogWarning (folder + " folder not found!");
            return null;
        }

        string[] files = Directory.GetFiles (folderPath, "*.json");
        for (int i = 0; i < files.Length; i++) {
            files[i] = Path.GetFileName (files[i]);
        }
        
        return files;
    }

    public static bool FilesExistIn (string folder) {
        string folderPath = Application.persistentDataPath + "/" + folder;
        string[] files = Directory.GetFiles (folderPath, "*.json");
        bool IsAnyFile = (files != null) && (files.Length > 0);
        return Directory.Exists (folderPath) && IsAnyFile;
    }

    public static void Save (string fileName, string folder, GameSave gameSave) {
        string folderPath = Application.persistentDataPath + "/" + folder;
        string filePath = folderPath + "/" + fileName;

        if (!Directory.Exists (folderPath)) {
            Directory.CreateDirectory (folderPath);
        }

        string jsonData = JsonUtility.ToJson (gameSave);
        File.WriteAllText (filePath, jsonData);
    }
	
	public static void Load (string fileName, string folder, ref GameSave gameSave) {
        string path = Application.persistentDataPath + "/" + folder + "/" + fileName;

        if (File.Exists (path)) {
            string jsonData = File.ReadAllText (path);
            gameSave = JsonUtility.FromJson<GameSave> (jsonData);
        } else {
            Debug.LogWarning (fileName + " not found in the " + folder + " folder!");
        }
    }

}
