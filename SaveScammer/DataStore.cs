using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveScammer
{
    public static class DataStore
    {
        public static List<ScamProfile> ScamProfiles;

        static DataStore()
        {
            ScamProfiles = new List<ScamProfile>();
            foreach (var path in Directory.GetFiles(Settings.ProfilesStoragePath))
            {
                ScamProfiles.Add(ScamProfile.FromJson(path));
            }
        }
    }
}
