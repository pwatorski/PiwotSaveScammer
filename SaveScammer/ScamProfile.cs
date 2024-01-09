using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SaveScammer
{
    public class ScamProfile
    {
        public string Name { get; set; }
        public long ID { get; set; }
        protected string name = "Unknown";
        public string SaveTarget { get; set; } = string.Empty;
        public string ScamStorage { get; set; } = string.Empty;
        public int Interval { get; set; } = 60;
        public int MaxSaves { get; set; } = 5;
        public string ScamStorageFull { get => Path.Combine(ScamStorage, ID.ToString()); }

        public string MetadataPath { get => Path.Combine(ScamStorageFull, "meta"); }
        public string SavesPath { get => Path.Combine(ScamStorageFull, "saves"); }

        public List<StorageFile> StorageFiles { get; set; }



        public ScamProfile()
        {
            ID = Misc.GetLongTimestamp();
            StorageFiles = new List<StorageFile>();
        }

        public ScamProfile Duplicate()
        {
            ScamProfile newProfile = new ScamProfile()
            {
                Name = $"{Name}_copy",
                SaveTarget = SaveTarget,
                ScamStorage = ScamStorage,
                Interval = Interval,
                MaxSaves = MaxSaves
            };

            return newProfile;
        }


        public void LoadStorage()
        {
            StorageFiles.Clear();
            if (!Directory.Exists(ScamStorageFull))
            {
                Directory.CreateDirectory(ScamStorageFull);
                Directory.CreateDirectory(MetadataPath);
                Directory.CreateDirectory(SavesPath);
            }
                
            foreach (var metaPath in Directory.EnumerateFiles(MetadataPath))
            {
                var sf = StorageFile.FromFile(metaPath);
                if (sf != null)
                    StorageFiles.Add(sf);
            }
            StorageFiles = StorageFiles.OrderByDescending(x=>x.SaveDateTime).ToList();
        }

        public long GetTotalSize()
        {
            return StorageFiles.Sum(x=>x.SizeBytes);
        }

        static string GetTimestamp(DateTime? dateTime=null)
        {
            if (dateTime == null) dateTime = DateTime.Now;
            return dateTime?.ToString("yyyy-MM-dd_HH-mm-ss")??"0001-01-01_00-00-00";
        }

        public StorageFile? StoreSave(string saveName="New save")
        {
            if (!Directory.Exists(ScamStorageFull))
                Directory.CreateDirectory(ScamStorageFull);


            DateTime dateTime = DateTime.Now;
            StorageFile storageFile = new StorageFile(Misc.GetLongTimestamp(), ID, dateTime, ScamStorageFull, saveName);
            var scamPath = storageFile.StorageFilePath;
            if (File.Exists(scamPath))
                return null;
            
            if (Directory.Exists(SaveTarget))
            {
                ZipFile.CreateFromDirectory(SaveTarget, scamPath, CompressionLevel.SmallestSize, false);
            }
            else
            {
                ZipSingleFile(SaveTarget, scamPath);
            }
            storageFile.UpdateSize();
            storageFile.Save();
            StorageFiles.Insert(0, storageFile);
            return storageFile;
        }

        void ZipSingleFile(string sourceFile, string targetPath) 
        {
            var tempDir = Path.Join(ScamStorage, "temp");
            var tempLoc = Path.Combine(tempDir, Path.GetFileName(sourceFile));
            if (Directory.Exists(tempDir))
            {
                foreach( var f in Directory.EnumerateFiles(tempDir))
                {
                    File.Delete(f);
                }
            }
            else
            {
                Directory.CreateDirectory(tempDir);
            }
            File.Copy(sourceFile, tempLoc);
            ZipFile.CreateFromDirectory(tempDir, targetPath, CompressionLevel.SmallestSize, false);
            Misc.PurgeDirectory(tempDir);
        }

        public void DeleteStoredSave(StorageFile storageFile)
        {
            StorageFiles.Remove(storageFile);
            storageFile.Delete();
        }

        public void RestoreLatestSave()
        {

            if (StorageFiles.Count == 0)
                return;
            RestoreSave(StorageFiles.First().StorageFilePath);

        }
        public void RestoreSave(string saveLocation)
        {
            var targetDir = Path.GetDirectoryName(SaveTarget);
            Misc.PurgeDirectory(targetDir);
            if (Directory.Exists(SaveTarget))
                ZipFile.ExtractToDirectory(saveLocation, SaveTarget);
            else
                ZipFile.ExtractToDirectory(saveLocation, Path.GetDirectoryName(SaveTarget) ?? "");
        }

        public void Save()
        {
            var profilePath = Path.Combine(Settings.ProfilesStoragePath, $"{ID}.json");
            using (StreamWriter sw = new StreamWriter(profilePath, false, encoding: Encoding.UTF8))
            {
                sw.Write(ToJson());
            }
        }
        public static ScamProfile Load(string name)
        {
            var profilePath = Path.Combine(Settings.ProfilesStoragePath, $"{Misc.HashString(name)}.json");
            if (!Path.Exists(profilePath))
            {
                return new ScamProfile();
            }
            return FromJson(profilePath);
        }

        public void Delete()
        {
            var profilePath = Path.Combine(Settings.ProfilesStoragePath, $"{ID}.json");
            File.Delete(profilePath);
            foreach(var file in StorageFiles)
            {
                file.Delete();
            }
            Directory.Delete(MetadataPath);
            Directory.Delete(SavesPath);
            Directory.Delete(ScamStorageFull);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, Misc.JsonOptions);
        }

        public static ScamProfile FromJson(string settingsPath)
        {
            string json = "";
            using (StreamReader sr = new StreamReader(settingsPath, encoding: Encoding.UTF8))
            {
                json = sr.ReadToEnd();
            }
            return JsonSerializer.Deserialize<ScamProfile>(json) ?? new ScamProfile();
        }

        internal void StopAll()
        {
            
        }

        public static ScamProfile GetDefaultProfile()
        {
            var ID = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            return new ScamProfile()
            {
                Name = "New profile",
                ScamStorage = Settings.StoragePath
            };
        }

        internal void SortSaves(int zeroBasedDisplayIndex, ListSortDirection direction)
        {
            int dir = direction == ListSortDirection.Ascending ? 1 : -1;
            switch(zeroBasedDisplayIndex)
            {
                case 0:
                case 1:
                    StorageFiles.Sort((x, y) => dir * x.DateTimeString.CompareTo(y.DateTimeString));
                    break;
                case 2:
                    StorageFiles.Sort((x, y) => dir * x.SaveName.CompareTo(y.SaveName));
                    break;
                case 3:
                    StorageFiles.Sort((x, y) => dir * x.SizeBytes.CompareTo(y.SizeBytes));
                    break;
                default:
                    StorageFiles.Sort((x, y) => dir * x.SaveID.CompareTo(y.SaveID));
                    break;
            }
        }
    }
}
