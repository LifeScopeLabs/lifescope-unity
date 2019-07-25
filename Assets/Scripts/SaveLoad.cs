using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveLoad
{
    public static Credentials credentials = new Credentials();

    public static void Save() {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs = File.Create(Path.Combine(Application.persistentDataPath, "LifeScopeCredentials.gd"));
        bf.Serialize(fs, SaveLoad.credentials);
        fs.Close();
    }

    public static void Load() {
        if (File.Exists(Path.Combine(Application.persistentDataPath, "LifeScopeCredentials.gd"))) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(Path.Combine(Application.persistentDataPath, "LifeScopeCredentials.gd"), FileMode.Open);
            SaveLoad.credentials = (Credentials)bf.Deserialize(fs);
            fs.Close();
        }
    }
}