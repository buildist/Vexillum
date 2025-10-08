using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace Vexillum.util
{
    public enum SettingType
    {
        Username, Password
    }
    public class Settings
    {
        private static Dictionary<SettingType, object> settings = new Dictionary<SettingType,object>(2);
        private static Dictionary<SettingType, object> defaultSettings = new Dictionary<SettingType,object>(2);
        private static string path = Util.GetGameFile("settings.xml");
        static Settings()
        {
            defaultSettings[SettingType.Username] = "";
            defaultSettings[SettingType.Password] = "";
        }
        public static void LoadSettings()
        {
            if (!File.Exists(path))
            {
                SetDefaultSettings();
                SaveSettings();
            }
            else
            {
                FileStream file = File.OpenRead(path);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedSettings));
                SerializedSettings loadedSettings = (SerializedSettings)serializer.Deserialize(file);
                UpdateSettings(loadedSettings);
                if (loadedSettings.keys.Length != defaultSettings.Count)
                    SetDefaultSettings();
                file.Close();
            }
        }
        public static void SaveSettings()
        {
            if(File.Exists(path))
                File.Delete(path);
            FileStream fileOut = File.Create(path);
            XmlSerializer serializer = new XmlSerializer(typeof(SerializedSettings));
            serializer.Serialize(fileOut, GetStruct());
            fileOut.Close();
        }
        public static object Get(SettingType s)
        {
            return settings[s];
        }
        public static void Set(SettingType s, object v)
        {
            settings[s] = v;
            SaveSettings();
        }
        private static void SetDefaultSettings()
        {
            foreach(SettingType s in defaultSettings.Keys)
            {
                if(!settings.ContainsKey(s))
                    settings[s] = defaultSettings[s];
            }
        }
        public static SerializedSettings GetStruct()
        {
            SettingType[] keys = new SettingType[settings.Count];
            object[] values = new object[settings.Count];
            for (int i = 0; i < settings.Count; i++)
            {
                keys[i] = settings.Keys.ElementAt(i);
                values[i] = settings.Values.ElementAt(i);
            }
            return new SerializedSettings(keys, values);
        }
        private static void UpdateSettings(SerializedSettings s)
        {

            for(int i = 0; i < s.keys.Length; i++)
            {
                settings[s.keys[i]] = s.values[i];
            }
        }
        [Serializable]
        public struct SerializedSettings
        {
            public SettingType[] keys;
            public object[] values;
            public SerializedSettings(SettingType[] k, object[] v)
            {
                keys = k;
                values = v;
            }
        }
    }
}
