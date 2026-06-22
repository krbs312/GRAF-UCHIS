
using System.Drawing;
using System.Linq;
using System.Numerics;
using DiplomApp;

namespace DiplomApp

{
    public class Mapobject
    {
        public string name { get; set; }
        public Map_info map_info = new Map_info();
        public static Action renderwindow = () => { };
        public Vector3 position { get; set; }
        public Vector3 size { get; set; }
        public Color color { get; set; }
        public bool canscriptable { get; set; }
        public bool canscriptwrite { get; set; }

        public string script { get; set; } = "";

        public Mapobject(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "", string script = "")
        {
            this.name = name;
            this.position = position;
            this.size = size;
            this.color = color;
            this.canscriptable = canscriptable;
            this.canscriptwrite = canscriptwrite;
            this.script = script;
        }

        protected Vector3[] GetAdjacentPositions(Vector3? exclude = null)
        {
            Vector3[] positions = new Vector3[]
            {
                new Vector3(position.X, position.Y - 1, position.Z),
                new Vector3(position.X, position.Y + 1, position.Z),
                new Vector3(position.X - 1, position.Y, position.Z),
                new Vector3(position.X + 1, position.Y, position.Z)
            };
            if (exclude.HasValue)
            {
                positions = positions.Where(p => p != exclude.Value).ToArray();
            }
            return positions;
        }

            public void Move(Vector3 newPosition, List<Mapobject> mapObjects)
        {
            if (!(map_info.CheckCollision(this, map_info.Getobjectbyposition(newPosition, mapObjects))))
            {
                return;
            }
            position = newPosition;
            renderwindow();
        }

        public void MoveByOneCell(string direction, List<Mapobject> mapObjects)
        {
                switch (direction)
            {
                case "up":
                    if (map_info.CanMoveWall(direction, position, mapObjects))
                    {
                        Move(new Vector3(position.X, position.Y - 1, position.Z), mapObjects);
                    }
                    break;
                case "down":
                    if (map_info.CanMoveWall(direction, position, mapObjects))
                    {
                        Move(new Vector3(position.X, position.Y + 1, position.Z), mapObjects);
                    }
                    break;
                case "left":
                    if (map_info.CanMoveWall(direction, position, mapObjects))
                    {
                        Move(new Vector3(position.X - 1, position.Y, position.Z), mapObjects);
                    }
                    break;
                case "right":
                    if (map_info.CanMoveWall(direction, position, mapObjects))
                    {
                        Move(new Vector3(position.X + 1, position.Y, position.Z), mapObjects);
                    }
                    break;
            }
            renderwindow();
        }
        public void SetColor(Color newColor)
        {
            color = newColor;
            renderwindow();
        }
    }
    public class Cube : Mapobject
{
    
    public Cube(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "", string script = "") : base(position, size, color, canscriptable, canscriptwrite, name, script)
    {

    }

}

    public class Sphere : Mapobject
    {
        public Sphere(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "", string script = "") : base(position, size, color, canscriptable, canscriptwrite, name, script)
        {

        }
    }

    public class Wall : Mapobject
    {
        public string side;
        public Wall(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string side, string name = "") : base(position, size, color, canscriptable, canscriptwrite, name)
        {
            this.side = side;
        }
    }

    public class Rotated : Mapobject
    {
        public string direction = "left";
        public Rotated(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "", string direction = "left", string script = "") : base(position, size, color, canscriptable, canscriptwrite, name, script)
        {
            this.direction = direction;
        }

        public void ChangeDirection(string newDirection)
        {
            direction = newDirection;
            renderwindow();
        }

        public void MoveForward(List<Mapobject> mapObjects)
        {
            MoveByOneCell(direction, mapObjects);
        }

        public void MoveBackward(List<Mapobject> mapObjects)
        {
            string oppositeDirection = direction switch
            {
                "up" => "down",
                "down" => "up",
                "left" => "right",
                "right" => "left",
                _ => direction
            };
            MoveByOneCell(oppositeDirection, mapObjects);
        }

        public void Rotate(string direction)
        {
            if (direction == "left")
            {
                this.direction = this.direction switch
                {
                    "up" => "left",
                    "down" => "right",
                    "left" => "down",
                    "right" => "up",
                    _ => this.direction
                };
            }
            else if (direction == "right")
            {
                this.direction = this.direction switch
                {
                    "up" => "right",
                    "down" => "left",
                    "left" => "up",
                    "right" => "down",
                    _ => this.direction
                };
            }
            renderwindow();
        }

        public string GetNameForward(List<Mapobject> mapObjects)
        {
            Vector3 newPosition = direction switch
            {
                "up" => new Vector3(position.X, position.Y - 1, position.Z),
                "down" => new Vector3(position.X, position.Y + 1, position.Z),
                "left" => new Vector3(position.X - 1, position.Y, position.Z),
                "right" => new Vector3(position.X + 1, position.Y, position.Z),
                _ => position
            };
            Mapobject obj = map_info.Getobjectbyposition(newPosition, mapObjects);
            return obj != null ? obj.name : "";
        }

    }

    public class Player : Rotated
    {
        public Player(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "", string direction = "left", string script = "") : base(position, size, color, canscriptable, canscriptwrite, name, direction, script)
        {
            
        }
    }

    public class CubeText : Mapobject
    {
        public string text;
        public CubeText(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string text, string name = "", string script = "") : base(position, size, color, canscriptable, canscriptwrite, name, script)
        {
            this.text = text;
        }
        public void SetText(string newText)
        {
            text = newText;
            renderwindow();
        }
    }

    public class Electronic : Mapobject
    {
        public bool state;
        public Electronic(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, bool state, string name = "") : base(position, size, color, canscriptable, canscriptwrite, name)
        {
            this.state = state;
        }
        public void SetState(bool newState)
        {
            state = newState;
            renderwindow();
        }

        
        public virtual void checknearactivated(List<Mapobject> mapObjects)
        {
            bool shouldBeOn = false;
            foreach (var adjacentPosition in GetAdjacentPositions())
            {
                Mapobject obj = map_info.Getobjectbyposition(adjacentPosition, mapObjects);
                if (obj is Buttongi buttongi && buttongi.state)
                {
                    shouldBeOn = true;
                    break;
                }
                if (obj is Redstonedust rd && rd.state)
                {
                    shouldBeOn = true;
                    break;
                }
                if (obj is Redstone)
                {
                    shouldBeOn = true;
                    break;
                }
            }
            SetState(shouldBeOn);
        }
    }

    public class Lamp : Electronic
    {
        public Lamp(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, bool state, string name = "") : base(position, size, color, canscriptable, canscriptwrite, state, name)
        {

        }
        public new void SetState(bool newState)
        {
            base.SetState(newState);
            color = newState == true ? Color.Yellow : Color.Gray;
            renderwindow();
        }
    }

    public class Buttongi : Electronic
    {
        public Buttongi(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, bool state, string name = "") : base(position, size, color, canscriptable, canscriptwrite, state, name)
        {

        }
        public new void activate(List<Mapobject> mapObjects)
        {
            base.SetState(!state);
            color = state == true ? Color.Green : Color.Red;
            if (state == true)
            {
                activatenearbyElectronic(mapObjects, true);
            }
            else
            {
                activatenearbyElectronic(mapObjects, false);
            }
            renderwindow();
        }

        public void activatenearbyElectronic(List<Mapobject> mapObjects, bool newState)
        {
            foreach (var adjacentPosition in GetAdjacentPositions())
            {
                Mapobject obj = map_info.Getobjectbyposition(adjacentPosition, mapObjects);
                if (obj is Redstonedust rd)
                {
                    
                    rd.checknearactivated(mapObjects, newState);
                    continue;
                }
                if (obj is Electronic electronic)
                {
                    electronic.checknearactivated(mapObjects);
                }
            }
        }
    }

    public class Redstone : Mapobject
    {
        public Redstone(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, string name = "") : base(position, size, color, canscriptable, canscriptwrite, name)
        {
            this.color = Color.Red;
        }
    }

    public class Redstonedust : Electronic
    {
        public Redstonedust(Vector3 position, Vector3 size, Color color, bool canscriptable, bool canscriptwrite, bool state, string name = "") : base(position, size, color, canscriptable, canscriptwrite, state, name)
        {
            this.color = Color.Gray;
        }

        public new void checknearactivated(List<Mapobject> mapObjects, bool newState)
        {
            if (state == newState)
                return; 

            SetState(newState);

            foreach (var adjacentPosition in GetAdjacentPositions())
            {
                Mapobject obj = map_info.Getobjectbyposition(adjacentPosition, mapObjects);
                if (obj is Redstonedust rd)
                {
                    rd.checknearactivated(mapObjects, newState);
                }
                else if (obj is Electronic electronic)
                {
                    
                    electronic.checknearactivated(mapObjects);
                }
            }
        }
    }

}