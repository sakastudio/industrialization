﻿using System.IO;

namespace Game.Save.Json
{
    public class MapConfigFile
    {
        public string FullMapObjectConfigFilePath { get; }
        
        public MapConfigFile(string fullMapObjectConfigFilePath)
        {
            FullMapObjectConfigFilePath = Path.Combine(fullMapObjectConfigFilePath,"mapObjects.json");
        }
    }
}