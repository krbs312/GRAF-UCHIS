
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;
using System.Diagnostics;
using static System.Diagnostics.Debug;

namespace DiplomApp
{
    public class Map_info
    {
        public Mapobject Getbyname(string name, List<Mapobject> mapObjects)
        {
            foreach (var obj in mapObjects)
            {
                if (obj.name == name)
                {
                    return obj;
                }
            }
            return null;
        }
        public bool CheckCollision(Mapobject obj1, Mapobject obj2)
        {
            if (obj2 == null) return true;
            if (obj2.GetType() == typeof(Wall)) return true;
            Rectangle rect1 = new Rectangle((int)obj1.position.X, (int)obj1.position.Y, (int)obj1.size.X, (int)obj1.size.Y);
            Rectangle rect2 = new Rectangle((int)obj2.position.X, (int)obj2.position.Y, (int)obj2.size.X, (int)obj2.size.Y);
            return rect1.IntersectsWith(rect2);
        }
        public Mapobject Getobjectbyposition(Vector3 position, List<Mapobject> mapObjects)
        {

            foreach (var obj in mapObjects)
            {
                Rectangle rect = new Rectangle((int)obj.position.X, (int)obj.position.Y, (int)obj.size.X, (int)obj.size.Y);
                if (rect.Contains((int)position.X, (int)position.Y))
                {
                    return obj;
                }
            }
            return null;
        }

        public Wall GetWallbyposition(Vector3 position, List<Mapobject> mapObjects)
        {
            foreach (var obj in mapObjects)
            {
                if (obj.GetType() == typeof(Wall))
                {
                    Rectangle rect = new Rectangle((int)obj.position.X, (int)obj.position.Y, (int)obj.size.X, (int)obj.size.Y);
                    if (rect.Contains((int)position.X, (int)position.Y))
                    {
                        return obj as Wall;
                    }
                }
            }
            return null;
        }

        public bool CanMoveWall(string direction, Vector3 wallposition, List<Mapobject> mapObjects)
        {

            Wall wall;
                switch (direction)
                {
                    case "up":
                        wall = GetWallbyposition(new Vector3(wallposition.X, wallposition.Y - 1, wallposition.Z), mapObjects);
                        if (wall == null) return true;
                        if (wall.side == "up" || wall.side == "down")
                        {
                            return false;
                        }
                        break;
                    case "down":
                        wall = GetWallbyposition(new Vector3(wallposition.X, wallposition.Y + 1, wallposition.Z), mapObjects) as Wall;
                        if (wall == null) return true;
                        if (wall.side == "up" || wall.side == "down")
                        {
                            return false;
                        }
                        break;
                    case "left":
                        wall = GetWallbyposition(new Vector3(wallposition.X, wallposition.Y, wallposition.Z), mapObjects) as Wall;
                        if (wall == null) return true;
                        if (wall.side == "left" || wall.side == "right")
                        {
                            return false;
                        }
                        break;
                    case "right":
                        wall = GetWallbyposition(new Vector3(wallposition.X + 1, wallposition.Y, wallposition.Z), mapObjects) as Wall;
                        if (wall == null) return true;
                        if (wall.side == "left" || wall.side == "right")
                        {
                            return false;
                        }
                        break;
                }
            return true;
        }
    }
}