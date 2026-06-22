using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;
using System.Text.Json.Nodes;


namespace DiplomApp
{
    class MapSaver
    {
        public static void SaveMap(List<Mapobject> mapObjects, string tasktext, List<string> task_ogranich)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Map files (*.map)|*.map|All files (*.*)|*.*";
            saveFileDialog.DefaultExt = "map";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath))
                {
                    file.WriteLine("MAPDATA");
                    foreach (var obj in mapObjects)
                    {
                        string typeName = obj.GetType().Name;
                        string name = obj.name;
                        string position = $"{obj.position.X},{obj.position.Y},{obj.position.Z}";
                        string color = $"{obj.color.R},{obj.color.G},{obj.color.B}";
                        string size = $"{obj.size.X},{obj.size.Y},{obj.size.Z}";
                        string script = obj.script;
                        string scriptable = obj.canscriptable.ToString();
                        string writescript = obj.canscriptwrite.ToString();
                        
                        string text = "";
                        string direction = "";
                        string side = "";
                        string state = "";
                        if (obj is Rotated)
                        {
                            direction = $"{((Rotated)obj).direction}";
                        }
                        if (obj is CubeText)
                        {
                            text = ((CubeText)obj).text;
                        }
                        if (obj is Wall)
                        {
                            side = ((Wall)obj).side;
                        }
                        if (obj is Electronic)
                        {
                            state = ((Electronic)obj).state.ToString();
                        }
                        string jsonString = $"{typeName}|{name}|{position}|{size}|{color}|{script}|{scriptable}|{writescript}|{direction}|{text}|{side}|{state}";
                        file.WriteLine(jsonString);
                    }
                    file.WriteLine("END");
                    file.WriteLine("TASKDATA");
                    file.WriteLine(tasktext);
                    file.WriteLine("END");
                    file.WriteLine("TASKOGRANICH");
                    foreach (var ogran in task_ogranich)
                    {
                        file.WriteLine(ogran);
                    }
                    file.WriteLine("END");
                }
            }
        }

        public static List<Mapobject>? LoadMap(ref string tasktext, List<string> task_ogranich)
        {
            return LoadMap(ref tasktext, task_ogranich, out _);
        }

        public static List<Mapobject>? LoadMap(ref string tasktext, List<string> task_ogranich, out string? loadedFilePath)
        {
            loadedFilePath = null;
            List<Mapobject> mapObjects = new List<Mapobject>();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Map files (*.map)|*.map|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                task_ogranich.Clear();
                tasktext = "";
                string filePath = openFileDialog.FileName;
                loadedFilePath = filePath;
                using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
                {
                    string? line;
                    bool readingMapData = false;
                    bool readingTaskData = false;
                    bool readingTaskOgranich = false;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line == "MAPDATA")
                        {
                            readingMapData = true;
                            continue;
                        }
                        if (line == "END")
                        {
                            readingMapData = false;
                            readingTaskData = false;
                            readingTaskOgranich = false;
                            continue;
                        }
                        if (line == "TASKDATA")
                        {
                            readingTaskData = true;
                            continue;
                        }
                        if (line == "TASKOGRANICH")
                        {
                            readingTaskOgranich = true;
                            continue;
                        }
                        if (readingMapData)
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 9)
                            {
                                string typeName = parts[0];
                                string name = parts[1];
                                Vector3 position = ParseVector3(parts[2]);
                                Vector3 size = ParseVector3(parts[3]);
                                Color color = ParseColor(parts[4]);
                                string script = parts[5];
                                bool canscriptable = bool.Parse(parts[6]);
                                bool canscriptwrite = bool.Parse(parts[7]);
                                string direction = parts.Length > 8 ? parts[8] : "left";
                                string text = parts.Length > 9 ? parts[9] : "";
                                string side = parts.Length > 10 ? parts[10] : "";
                                bool state = parts.Length > 11 && bool.TryParse(parts[11], out bool parsedState) ? parsedState : false;
                                Mapobject obj;
                                switch (typeName)
                                {
                                    case "Cube":
                                        obj = new Cube(position, size, color, canscriptable, canscriptwrite, name, script);
                                        break;
                                    case "Sphere":
                                        obj = new Sphere(position, size, color, canscriptable, canscriptwrite, name, script);
                                        break;
                                    case "Player":
                                        obj = new Player(position, size, color, canscriptable, canscriptwrite, name, direction, script);
                                        break;
                                    case "Wall":
                                        obj = new Wall(position, size, color, canscriptable, canscriptwrite, side, name) { script = script };
                                        break;
                                    case "Lamp":
                                        obj = new Lamp(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    case "Buttongi":
                                        obj = new Buttongi(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    case "CubeText":
                                        obj = new CubeText(position, size, color, canscriptable, canscriptwrite, text, name, script);
                                        break;
                                    case "Redstone":
                                        obj = new Redstone(position, size, color, canscriptable, canscriptwrite, name) { script = script };
                                        break;
                                    case "Redstonedust":
                                        obj = new Redstonedust(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    default:
                                        continue;
                                }
                                mapObjects.Add(obj);
                            }
                        }
                        if (readingTaskData)
                        {
                            tasktext += line + "\n";
                        }
                        if (readingTaskOgranich)
                        {
                            task_ogranich.Add(line);
                        }
                    }
                }
                return mapObjects;
            }
            return null;
        }

        public static List<Mapobject>? LoadMapFromFile(string filePath, ref string tasktext, List<string> task_ogranich)
        {
            try
            {
                List<Mapobject> mapObjects = new List<Mapobject>();
                task_ogranich.Clear();
                tasktext = "";
                
                using (System.IO.StreamReader file = new System.IO.StreamReader(filePath))
                {
                    string? line;
                    bool readingMapData = false;
                    bool readingTaskData = false;
                    bool readingTaskOgranich = false;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line == "MAPDATA")
                        {
                            readingMapData = true;
                            continue;
                        }
                        if (line == "END")
                        {
                            readingMapData = false;
                            readingTaskData = false;
                            readingTaskOgranich = false;
                            continue;
                        }
                        if (line == "TASKDATA")
                        {
                            readingTaskData = true;
                            continue;
                        }
                        if (line == "TASKOGRANICH")
                        {
                            readingTaskOgranich = true;
                            continue;
                        }
                        if (readingMapData)
                        {
                            string[] parts = line.Split('|');
                            if (parts.Length >= 9)
                            {
                                string typeName = parts[0];
                                string name = parts[1];
                                Vector3 position = ParseVector3(parts[2]);
                                Vector3 size = ParseVector3(parts[3]);
                                Color color = ParseColor(parts[4]);
                                string script = parts[5];
                                bool canscriptable = bool.Parse(parts[6]);
                                bool canscriptwrite = bool.Parse(parts[7]);
                                string direction = parts.Length > 8 ? parts[8] : "left";
                                string text = parts.Length > 9 ? parts[9] : "";
                                string side = parts.Length > 10 ? parts[10] : "";
                                bool state = parts.Length > 11 && bool.TryParse(parts[11], out bool parsedState) ? parsedState : false;
                                Mapobject obj;
                                switch (typeName)
                                {
                                    case "Cube":
                                        obj = new Cube(position, size, color, canscriptable, canscriptwrite, name, script);
                                        break;
                                    case "Sphere":
                                        obj = new Sphere(position, size, color, canscriptable, canscriptwrite, name, script);
                                        break;
                                    case "Player":
                                        obj = new Player(position, size, color, canscriptable, canscriptwrite, name, direction, script);
                                        break;
                                    case "Wall":
                                        obj = new Wall(position, size, color, canscriptable, canscriptwrite, side, name) { script = script };
                                        break;
                                    case "Lamp":
                                        obj = new Lamp(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    case "Buttongi":
                                        obj = new Buttongi(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    case "CubeText":
                                        obj = new CubeText(position, size, color, canscriptable, canscriptwrite, text, name, script);
                                        break;
                                    case "Redstone":
                                        obj = new Redstone(position, size, color, canscriptable, canscriptwrite, name) { script = script };
                                        break;
                                    case "Redstonedust":
                                        obj = new Redstonedust(position, size, color, canscriptable, canscriptwrite, state, name) { script = script };
                                        break;
                                    default:
                                        continue;
                                }
                                mapObjects.Add(obj);
                            }
                        }
                        if (readingTaskData)
                        {
                            tasktext += line + "\n";
                        }
                        if (readingTaskOgranich)
                        {
                            task_ogranich.Add(line);
                        }
                    }
                }
                return mapObjects;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке файла:\n" + ex.Message);
                return null;
            }
        }

        private static Vector3 ParseVector3(string str)
        {
            string[] parts = str.Split(',');
            if (parts.Length == 3)
            {
                float x = float.Parse(parts[0]);
                float y = float.Parse(parts[1]);
                float z = float.Parse(parts[2]);
                return new Vector3(x, y, z);
            }
            return Vector3.Zero;
        }

        private static Color ParseColor(string str)
        {
            string[] parts = str.Split(',');
            if (parts.Length == 3)
            {
                int r = int.Parse(parts[0]);
                int g = int.Parse(parts[1]);
                int b = int.Parse(parts[2]);
                return Color.FromArgb(r, g, b);
            }
            return Color.White;
        }

    }
}