using System.IO;
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

    public static void Save<T> (string fileName, string folder, T saveData) {
        string folderPath = Application.persistentDataPath + "/" + folder;
        string filePath = folderPath + "/" + fileName;

        if (!Directory.Exists (folderPath)) {
            Directory.CreateDirectory (folderPath);
        }

        string jsonData = JsonUtility.ToJson (saveData, true);
        File.WriteAllText (filePath, jsonData);
    }
	
	public static void Load<T> (string fileName, string folder, ref T saveData) {
        string path = Application.persistentDataPath + "/" + folder + "/" + fileName;

        if (File.Exists (path)) {
            string jsonData = File.ReadAllText (path);
            saveData = JsonUtility.FromJson<T> (jsonData);
        } else {
            Debug.LogWarning (fileName + " not found in the " + folder + " folder!");
        }
    }

}
