using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using game.Environment;

namespace game.core
{
    public class SaveData
    {
        public float PlayerX { get; set; } 
        public float PlayerY { get; set; }
        public int PlayerHealth { get; set; }
        public int PlayerMaxHealth { get; set; }
        public int CurrentRoomX { get; set; }
        public int CurrentRoomY { get; set; }

        public int Seed { get; set; }
        public int LevelWidth { get; set; }
        public int LevelHeight { get; set; }
        public int[][] LayoutRaw { get; set; } 
        public List<string> ClearedRooms { get; set; } = new List<string>();
        public string[] WeaponSlotNames { get; set; } 
        public int[] WeaponSlotAmmo { get; set; }  
        public int CurrentWeaponIndex { get; set; }
        public string[] MainSlotNames { get; set; }
        public int[] MainSlotAmmo { get; set; }
    }

    public static class SaveManager
    {
        private static string savePath = "savegame.json";
        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public static void Save(SaveData data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data, options);
                File.WriteAllText(savePath, json);
                Console.WriteLine("Игра сохранена успешно.");
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка при сохранении: " + ex.Message);
            }
        }

        public static SaveData Load()
        {
            if (!File.Exists(savePath)) return null;

            try
            {
                string json = File.ReadAllText(savePath);
                return JsonSerializer.Deserialize<SaveData>(json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Ошибка при загрузке файла сохранения: " + ex.Message);
                return null;
            }
        }

        public static bool HasSave()
        {
            return File.Exists(savePath);
        }

        public static void DeleteSave()
        {
            if (File.Exists(savePath)) File.Delete(savePath);
        }
    }
}