
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;

namespace DiplomApp
{
    public class mousehandle
    {

        public bool isstartbutton(Point mousePosition, Form form)
        {

            Rectangle startButton = new Rectangle(10, 10, 100, 50);
            return startButton.Contains(mousePosition);
        }

        public bool istaskbutton(Point mousePosition, Form form)
        {

            Rectangle taskButton = new Rectangle(120, 10, 100, 50);
            return taskButton.Contains(mousePosition);
        }

        public bool ismenubutton(Point mousePosition, Form form)
        {

            Rectangle menuButton = new Rectangle(form.ClientSize.Width - 110, 10, 100, 50);
            return menuButton.Contains(mousePosition);
        }

        public bool isaddbutton(Point mousePosition, Form form)
        {
            Rectangle addButton = new Rectangle(10, 10, 100, 50);

            return addButton.Contains(mousePosition);
        }

        public bool istreebutton(Point mousePosition, Form form)
        {
            Rectangle treeButton = new Rectangle(230, 10, 100, 50);
            return treeButton.Contains(mousePosition);
        }
        
        public Mapobject GetObjectAtPosition(List<Mapobject> mapObjects, Point mousePosition, Form form, int plotsize, Vector2 cameraposition, bool redactormode)
        {

            foreach (var obj in mapObjects)
            {
                switch (obj)
                {
                    case Mapobject mapobj:
                        {
                            int x = (int)(mapobj.position.X * 10 * plotsize + form.ClientSize.Width / 2 + cameraposition.X);
                            int y = (int)(mapobj.position.Y * 10 * plotsize + form.ClientSize.Height / 2 + cameraposition.Y);
                            int size = (int)(mapobj.size.X * 10 * plotsize);
                            Rectangle rect = new Rectangle(x, y, size, size);
                            if (rect.Contains(mousePosition))
                            {
                                return mapobj;
                            }
                        }
                        break;
                }
            }

            if (redactormode)
            {
                
                
                return null;
            }

            return null;
        }
    }
}