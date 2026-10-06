using System;
using System.IO;
using UnityEngine;

namespace HuntingBoat
{
    public static class SaveStore
    {
        public static string PathName { get { return Path.Combine(Application.persistentDataPath, "hunting-boat-save.json"); } }
        public static bool Write(SaveData data, out string error)
        {
            error = null;
            try
            {
                if (!data.IsValid()) throw new InvalidDataException("Save data outside supported bounds.");
                string path = PathName;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path + ".tmp", JsonUtility.ToJson(data, true));
                if (File.Exists(path)) File.Copy(path, path + ".bak", true);
                // Replace in one operation where supported; preserve the prior file on failure.
                if (File.Exists(path)) File.Replace(path + ".tmp", path, null);
                else File.Move(path + ".tmp", path);
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }
        public static bool Read(out SaveData data, out string error)
        {
            data = null; error = null;
            try
            {
                if (!File.Exists(PathName)) { error = "No saved voyage yet."; return false; }
                if (new FileInfo(PathName).Length > 2 * 1024 * 1024) throw new InvalidDataException("Save file exceeds size limit.");
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(PathName));
                if (data == null || !data.IsValid()) throw new InvalidDataException("Invalid or incompatible save file.");
                return true;
            }
            catch (Exception ex) { data = null; error = ex.Message; return false; }
        }
    }
}
