using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;

namespace DiplomApp
{
    public class Renderer
    {
        public void Rendergrid(Graphics g, Form form, int plotsize, Vector2 cameraposition)
        {
            int gridCellSize = 10 * plotsize;
            int centerX = form.ClientSize.Width / 2;
            int centerY = form.ClientSize.Height / 2;

            int leftPixels = -centerX - (int)cameraposition.X;
            int rightPixels = form.ClientSize.Width - centerX - (int)cameraposition.X;
            int topPixels = -centerY - (int)cameraposition.Y;
            int bottomPixels = form.ClientSize.Height - centerY - (int)cameraposition.Y;

            int minGridX = (int)Math.Floor((double)leftPixels / gridCellSize) - 1;
            int maxGridX = (int)Math.Ceiling((double)rightPixels / gridCellSize) + 1;
            int minGridY = (int)Math.Floor((double)topPixels / gridCellSize) - 1;
            int maxGridY = (int)Math.Ceiling((double)bottomPixels / gridCellSize) + 1;

            using (var pen = new Pen(Color.LightGray, 1))
            {
                for (int gridX = minGridX; gridX <= maxGridX; gridX++)
                {
                    int x = gridX * gridCellSize + centerX + (int)cameraposition.X;
                    g.DrawLine(pen, x, 0, x, form.ClientSize.Height);
                }
                for (int gridY = minGridY; gridY <= maxGridY; gridY++)
                {
                    int y = gridY * gridCellSize + centerY + (int)cameraposition.Y;
                    g.DrawLine(pen, 0, y, form.ClientSize.Width, y);
                }
            }
        }
        public void Renderlevel(Graphics g, List<Mapobject> mapObjects, Form form, int plotsize, Vector2 cameraposition, bool redactormode, List<(Color, int,int)> mapcolors, string task)
        {
            int gridCellSize = 10 * plotsize;
            int centerX = form.ClientSize.Width / 2;
            int centerY = form.ClientSize.Height / 2;

            try
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.WhiteSmoke);

                Rendergrid(g, form, plotsize, cameraposition);

                g.DrawLine(Pens.DarkRed, centerX - 20, centerY, centerX + 20, centerY);
                g.DrawLine(Pens.DarkRed, centerX, centerY - 20, centerX, centerY + 20);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Renderlevel exception:\n" + ex.ToString());
            }


            foreach (var color in mapcolors)
            {
                int x = color.Item2 * gridCellSize + centerX + (int)cameraposition.X;
                int y = color.Item3 * gridCellSize + centerY + (int)cameraposition.Y;
                g.FillRectangle(new SolidBrush(color.Item1), x, y, gridCellSize, gridCellSize);
            }
            
            foreach (var obj in mapObjects)
            {
                switch (obj)
                {
                    case Cube cube:
                        {
                            int x = (int)(cube.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(cube.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), cube.color, ControlPaint.Light(cube.color), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillRectangle(brush, x, y, size, size);
                            }
                            g.DrawRectangle(new Pen(Color.DarkGray, 1), x, y, size, size);

                        }
                        break;
                    case Sphere sphere:
                        {
                            int x = (int)(sphere.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(sphere.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), sphere.color, ControlPaint.Light(sphere.color), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillEllipse(brush, x, y, size, size);
                            }
                            g.DrawEllipse(new Pen(Color.DarkGray, 1), x, y, size, size);
                        }
                        break;
                    case Wall wall:
                        {
                            int x = (int)(wall.position.X * gridCellSize + centerX + cameraposition.X);
                            int y = (int)(wall.position.Y * gridCellSize + centerY + cameraposition.Y);
                            int size = gridCellSize;
                            int halfThickness = plotsize / 2;
                            switch (wall.side)
                            {
                                case "left":
                                    g.FillRectangle(new SolidBrush(wall.color), x - halfThickness, y, plotsize, size);
                                    break;
                                case "right":
                                    g.FillRectangle(new SolidBrush(wall.color), x + gridCellSize - halfThickness, y, plotsize, size);
                                    break;
                                case "up":
                                    g.FillRectangle(new SolidBrush(wall.color), x, y - halfThickness, size, plotsize);
                                    break;
                                case "down":
                                    g.FillRectangle(new SolidBrush(wall.color), x, y + gridCellSize - halfThickness, size, plotsize);
                                    break;
                            }
                        }
                        break;
                    case Player player:
                        {
                            int x = (int)(player.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(player.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), player.color, ControlPaint.Light(player.color), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillEllipse(brush, x, y, size, size);
                            }
                            g.DrawEllipse(new Pen(Color.DarkGray, 1), x, y, size, size);
                            int eyeSize = size / 5;
                            switch (player.direction)
                            {
                                case "up":
                                    g.FillEllipse(Brushes.White, x + size / 2 - eyeSize / 2, y + eyeSize, eyeSize, eyeSize);
                                    g.DrawEllipse(Pens.Black, x + size / 2 - eyeSize / 2, y + eyeSize, eyeSize, eyeSize);
                                    break;
                                case "down":
                                    g.FillEllipse(Brushes.White, x + size / 2 - eyeSize / 2, y + size - 2 * eyeSize, eyeSize, eyeSize);
                                    g.DrawEllipse(Pens.Black, x + size / 2 - eyeSize / 2, y + size - 2 * eyeSize, eyeSize, eyeSize);
                                    break;
                                case "left":
                                    g.FillEllipse(Brushes.White, x + eyeSize, y + size / 2 - eyeSize / 2, eyeSize, eyeSize);
                                    g.DrawEllipse(Pens.Black, x + eyeSize, y + size / 2 - eyeSize / 2, eyeSize, eyeSize);
                                    break;
                                case "right":
                                    g.FillEllipse(Brushes.White, x + size - 2 * eyeSize, y + size / 2 - eyeSize / 2, eyeSize, eyeSize);
                                    g.DrawEllipse(Pens.Black, x + size - 2 * eyeSize, y + size / 2 - eyeSize / 2, eyeSize, eyeSize);
                                    break;
                            }
                            
                        }
                        break;
                        case CubeText cubeText:
                        {
                            int x = (int)(cubeText.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(cubeText.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), cubeText.color, ControlPaint.Light(cubeText.color), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillRectangle(brush, x, y, size, size);
                            }
                            g.DrawRectangle(new Pen(Color.DarkGray, 1), x, y, size, size);
                            var stringSize = g.MeasureString(cubeText.text, new Font("Segoe UI", 10, FontStyle.Bold));
                            g.DrawString(cubeText.text, new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, x + (size - stringSize.Width) / 2, y + (size - stringSize.Height) / 2);
                        }
                        break;
                        case Lamp lamp:
                        {
                            int x = (int)(lamp.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(lamp.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            Color baseColor = lamp.state ? Color.Yellow : Color.Gray;
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), baseColor, ControlPaint.Light(baseColor), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillEllipse(brush, x, y, size, size);
                            }
                            g.DrawEllipse(new Pen(Color.DarkGray, 1), x, y, size, size);
                        }
                        break;
                        case Buttongi buttongi:
                        {
                            int x = (int)(buttongi.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(buttongi.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            Color baseColor = buttongi.state ? Color.Green : Color.Red;
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), baseColor, ControlPaint.Light(baseColor), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillEllipse(brush, x, y, size, size);
                            }
                            g.DrawEllipse(new Pen(Color.DarkGray, 1), x, y, size, size);
                        }
                        break;
                        case Redstone redstone:
                        {
                            int x = (int)(redstone.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(redstone.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), Color.Red, ControlPaint.Light(Color.Red), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillRectangle(brush, x, y, size, size);
                            }
                            g.DrawRectangle(new Pen(Color.DarkGray, 1), x, y, size, size);
                        }
                        break;
                        case Redstonedust redstonedust:
                        {
                            int x = (int)(redstonedust.position.X * gridCellSize + centerX + cameraposition.X) + plotsize;
                            int y = (int)(redstonedust.position.Y * gridCellSize + centerY + cameraposition.Y) + plotsize;
                            int size = gridCellSize - (2 * plotsize);
                            Color baseColor = redstonedust.state ? Color.Red : Color.OrangeRed;
                            using (var brush = new LinearGradientBrush(new Rectangle(x, y, size, size), baseColor, ControlPaint.Light(baseColor), LinearGradientMode.ForwardDiagonal))
                            {
                                g.FillEllipse(brush, x, y, size, size);
                            }
                            g.DrawEllipse(new Pen(Color.DarkGray, 1), x, y, size, size);
                        }
                        break;
                }   
            }


            using (var brush = new LinearGradientBrush(new Rectangle(form.ClientSize.Width - 110, 10, 100, 50), Color.SteelBlue, Color.LightBlue, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, form.ClientSize.Width - 110, 10, 100, 50);
            }
            g.DrawRectangle(Pens.DarkBlue, form.ClientSize.Width - 110, 10, 100, 50);
            g.DrawString("Меню", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, new PointF(form.ClientSize.Width - 100, 20));

            if (!string.IsNullOrWhiteSpace(task))
            {
                Rectangle taskRect = new Rectangle(10, 70, form.ClientSize.Width - 20, 60);
                using (var brush = new SolidBrush(Color.FromArgb(220, Color.White)))
                {
                    g.FillRectangle(brush, taskRect);
                }
                g.DrawRectangle(Pens.DarkGray, taskRect);
                g.DrawString("Задача: " + task, new Font("Segoe UI", 10, FontStyle.Regular), Brushes.Black, new RectangleF(15, 75, taskRect.Width - 10, taskRect.Height));
            }

            if (redactormode)
            {
                using (var brush = new LinearGradientBrush(new Rectangle(10, 10, 100, 50), Color.SteelBlue, Color.LightBlue, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 10, 10, 100, 50);
                }
                g.DrawRectangle(Pens.DarkBlue, 10, 10, 100, 50);
                g.DrawString("Добавить", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, new PointF(20, 20));

                using (var brush = new LinearGradientBrush(new Rectangle(120, 10, 100, 50), Color.SteelBlue, Color.LightBlue, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 120, 10, 100, 50);
                }
                g.DrawRectangle(Pens.DarkBlue, 120, 10, 100, 50);
                g.DrawString("Задача", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, new PointF(130, 20));

                using (var brush = new LinearGradientBrush(new Rectangle(230, 10, 100, 50), Color.SteelBlue, Color.LightBlue, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 230, 10, 100, 50);
                }
                g.DrawRectangle(Pens.DarkBlue, 230, 10, 100, 50);
                g.DrawString("Дерево", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, new PointF(240, 20));
            }
             else
            {
                using (var brush = new LinearGradientBrush(new Rectangle(10, 10, 100, 50), Color.LightGreen, Color.Green, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(brush, 10, 10, 100, 50);
                }
                g.DrawRectangle(Pens.DarkGreen, 10, 10, 100, 50);
                g.DrawString("Старт", new Font("Segoe UI", 14, FontStyle.Bold), Brushes.White, new PointF(20, 20));
            }

            if (string.IsNullOrWhiteSpace(task))
            {
                g.DrawString("Режим: " + (redactormode ? "Редактор" : "Игра"), new Font("Segoe UI", 10, FontStyle.Regular), Brushes.DarkSlateGray, new PointF(10, 70));
                g.DrawString("Зажмите правую кнопку для панорамирования, щёлкните объект для редактирования или запуска скрипта.", new Font("Segoe UI", 9, FontStyle.Regular), Brushes.DarkSlateGray, new PointF(10, 90));
                g.DrawString("Используйте меню Файл для сохранения/открытия и меню Редактор для переключения режима.", new Font("Segoe UI", 9, FontStyle.Regular), Brushes.DarkSlateGray, new PointF(10, 108));
            }

        }
    }
}



