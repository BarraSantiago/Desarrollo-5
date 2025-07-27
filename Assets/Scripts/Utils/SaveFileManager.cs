using System.IO;
using UnityEngine;
namespace Utils
{

    public static class SaveFileManager
    {
        public static bool ResetGame = false;
        // Checks if there are any files in the persistent data path
        public static bool HasSavedFiles()
        {
            string path = Application.persistentDataPath;
            if (!Directory.Exists(path)) return false;
            string[] files = Directory.GetFiles(path);
            return files.Length > 0;
        }

        // Deletes all files in the persistent data path
        public static void DeleteAllSaves()
        {
            string path = Application.persistentDataPath;
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}