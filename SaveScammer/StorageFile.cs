using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SaveScammer
{
    public class StorageFile
    {
        public DateTime SaveDateTime { get; set; }
        [JsonIgnore]
        public string DateTimeString => SaveDateTime.ToString("yyyy.MM.dd HH:mm:ss");
        [JsonIgnore]
        public string DateString => SaveDateTime.ToString("yyyy.MM.dd");
        [JsonIgnore]
        public string TimeString => SaveDateTime.ToString("HH:mm:ss");
        [JsonIgnore]
        public string SizeString => Misc.GetSizeString(SizeBytes);
        public long ProfileID { get; set; }
        public long SaveID { get; set; }
        public long SizeBytes { get; set; }
        public string StorageFilePath { get; set; }
        public string MetaFilePath { get; set; }
        public string SaveName { get; set; }
        [JsonIgnore]
        public bool ViewMode { get; set; } = true;

        [JsonConstructor]
        public StorageFile() { }

        public StorageFile(long saveId, long profileId, DateTime dateTime, string profileStoragePath, string saveName)
        {
            SaveID = saveId;
            ProfileID = profileId;
            SaveDateTime = dateTime;
            StorageFilePath = Path.Combine(profileStoragePath, "saves", $"{SaveID}.zip");
            MetaFilePath = Path.Combine(profileStoragePath, "meta", $"{SaveID}.json");
            SaveName = saveName;
        }

        public static StorageFile? FromFile(string filepath)
        {
            StorageFile? storageFile = FromJson(filepath);
            if(storageFile == null)
                return null;
            return storageFile;
        }

        public void Rename(string newName)
        {
            SaveName = newName;
            Save();
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, Misc.JsonOptions);
        }

        public void Save()
        {
            string filePath = Path.Combine(MetaFilePath);
            using StreamWriter sw = new(filePath, append: false, encoding: Encoding.UTF8);
            sw.WriteLine(ToJson());
        }

        public static StorageFile? FromJson(string filePath)
        {
            string json = "";
            using (StreamReader sr = new(filePath, encoding: Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            return JsonSerializer.Deserialize<StorageFile>(json);
        }

        internal void Delete()
        {
            File.Delete(MetaFilePath);
            File.Delete(StorageFilePath);
        }

        internal void UpdateSize()
        {
            SizeBytes = Misc.GetFileSize(StorageFilePath);
        }
    }
}
