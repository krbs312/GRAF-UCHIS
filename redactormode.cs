using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;
using System.Collections;

namespace DiplomApp
{
    public class Redactormode
    {

        public Vector3 parsevector3(string input)
        {
            string[] parts = input.Split(',');
            if (parts.Length != 3)
            {
                throw new FormatException("Input must be in the format 'x,y,z'");
            }
            float x = float.Parse(parts[0]);
            float y = float.Parse(parts[1]);
            float z = float.Parse(parts[2]);
            return new Vector3(x, y, z);
        }

        public Color parsecolor(string input)
        {
            string[] parts = input.Split(',');
            if (parts.Length != 3)
            {
                throw new FormatException("Input must be in the format 'r,g,b'");
            }
            int r = int.Parse(parts[0]);
            int g = int.Parse(parts[1]);
            int b = int.Parse(parts[2]);
            return Color.FromArgb(r, g, b);
        }
        List<Mapobject> mapObjects = new List<Mapobject> { };
        Form redactform = new Form();
        ToolTip scriptToolTip = new ToolTip();
        Dictionary<string, string> helpDict = null;
        string lastTipKey = null;
        public Action renderwindow = () => { };
        public void Addallitems()
        {
            mapObjects.Clear();
            mapObjects.Add(new Cube(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, true, "test_cube"));
            mapObjects.Add(new Sphere(new Vector3(0,0,0), new Vector3(0,0,0), Color.Red ,true ,true, "test_sphere"));
            mapObjects.Add(new Player(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue, true, true, "test_player"));
            mapObjects.Add(new Wall(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Gray, true, false, "left", "test_wall"));
            mapObjects.Add(new Lamp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Yellow, true, false, false, "test_lamp"));
            mapObjects.Add(new Buttongi(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Green, true, false, false, "test_buttongi"));
            mapObjects.Add(new CubeText(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, true, "hello word","test_cubetexture"));
        }

        public void Showredactform(List<Mapobject> mapObjectsList)
        {
            helpDict = BuildHelpDictionaryLocal();
            mapObjects = new List<Mapobject>(mapObjectsList);

            Vector3 position = new Vector3(0, 0, 0);
            Vector3 size = new Vector3(1, 1, 1);
            Color color = Color.Red;
            bool isSolid = true;
            bool isVisible = true;
            bool isActive = false;
            string name = "test";
            string direction = "left";



            
            redactform.Text = "Редактор";
            redactform.Size = new Size(600, 400);

            Label objectsLabel = new Label { Text = "Объекты на карте:", Location = new Point(10, 10), Size = new Size(250, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            redactform.Controls.Add(objectsLabel);
            TreeView tree = new TreeView();
            tree.Location = new Point(10, 35);
            tree.Size = new Size(250, 315);

            void clearDetail()
            {
                for (int i = redactform.Controls.Count - 1; i >= 0; i--)
                {
                    if (redactform.Controls[i] != tree && redactform.Controls[i] != objectsLabel)
                        redactform.Controls.RemoveAt(i);
                }
            }


            void PopulateTree()
            {
                tree.Nodes.Clear();
                var groups = mapObjects.GroupBy(o => o.GetType().Name);
                foreach (var grp in groups)
                {
                    TreeNode parent = new TreeNode(grp.Key);
                    foreach (var obj in grp)
                    {
                        TreeNode child = new TreeNode(obj.name) { Tag = obj };
                        parent.Nodes.Add(child);
                    }
                    tree.Nodes.Add(parent);
                }
                tree.ExpandAll();
            }

            tree.AfterSelect += (sender, e) =>
            {
                clearDetail();
                Mapobject obj = e.Node.Tag as Mapobject;
                if (obj != null)
                    ShowEditorFor(obj, e.Node);
            };

            PopulateTree();
            redactform.Controls.Add(tree);

            redactform.ShowDialog();
        }
        private void ShowEditorFor(Mapobject obj, TreeNode node)
        {
            Label editorTitle = new Label { Text = "Параметры объекта:", Location = new Point(270, 10), Size = new Size(300, 20), Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            redactform.Controls.Add(editorTitle);
            switch (obj)
            {
                case Cube cube:
                    {
                        Label nameLabel = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel);
                        TextBox nameInput = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = cube.name };
                        redactform.Controls.Add(nameInput);
                        Label positionLabel = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel);
                        TextBox positionInput = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{cube.position.X},{cube.position.Y},{cube.position.Z}" };
                        redactform.Controls.Add(positionInput);
                        Label sizeLabel = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel);
                        TextBox sizeInput = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{cube.size.X},{cube.size.Y},{cube.size.Z}" };
                        redactform.Controls.Add(sizeInput);
                        Label colorLabel = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel);
                        TextBox colorInput = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{cube.color.R},{cube.color.G},{cube.color.B}" };
                        redactform.Controls.Add(colorInput);
                        CheckBox canScript = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = cube.canscriptable };
                        redactform.Controls.Add(canScript);
                        CheckBox canWrite = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Можно редактировать", Checked = cube.canscriptwrite };
                        redactform.Controls.Add(canWrite);
                        Button save = new Button { Location = new Point(270, 220), Size = new Size(150, 30), Text = "Сохранить" };
                        save.Click += (s, ev) =>
                        {
                            cube.name = nameInput.Text;
                            cube.position = parsevector3(positionInput.Text);
                            cube.size = parsevector3(sizeInput.Text);
                            cube.color = parsecolor(colorInput.Text);
                            cube.canscriptable = canScript.Checked;
                            cube.canscriptwrite = canWrite.Checked;
                            node.Text = cube.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save);
                    }
                    break;
                case Sphere sphere:
                    {
                        Label nameLabel1 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel1);
                        TextBox nameInput1 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = sphere.name };
                        redactform.Controls.Add(nameInput1);
                        Label positionLabel1 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel1);
                        TextBox positionInput1 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{sphere.position.X},{sphere.position.Y},{sphere.position.Z}" };
                        redactform.Controls.Add(positionInput1);
                        Label sizeLabel1 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel1);
                        TextBox sizeInput1 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{sphere.size.X},{sphere.size.Y},{sphere.size.Z}" };
                        redactform.Controls.Add(sizeInput1);
                        Label colorLabel1 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel1);
                        TextBox colorInput1 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{sphere.color.R},{sphere.color.G},{sphere.color.B}" };
                        redactform.Controls.Add(colorInput1);
                        CheckBox canScript1 = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = sphere.canscriptable };
                        redactform.Controls.Add(canScript1);
                        CheckBox canWrite1 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Можно редактировать", Checked = sphere.canscriptwrite };
                        redactform.Controls.Add(canWrite1);
                        Button save1 = new Button { Location = new Point(270, 220), Size = new Size(150, 30), Text = "Сохранить" };
                        save1.Click += (s, ev) =>
                        {
                            sphere.name = nameInput1.Text;
                            sphere.position = parsevector3(positionInput1.Text);
                            sphere.size = parsevector3(sizeInput1.Text);
                            sphere.color = parsecolor(colorInput1.Text);
                            sphere.canscriptable = canScript1.Checked;
                            sphere.canscriptwrite = canWrite1.Checked;
                            node.Text = sphere.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save1);
                    }
                    break;
                case Player player:
                    {
                        Label nameLabel3 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel3);
                        TextBox nameInput3 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = player.name };
                        redactform.Controls.Add(nameInput3);
                        Label positionLabel3 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel3);
                        TextBox positionInput3 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{player.position.X},{player.position.Y},{player.position.Z}" };
                        redactform.Controls.Add(positionInput3);
                        Label sizeLabel3 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel3);
                        TextBox sizeInput3 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{player.size.X},{player.size.Y},{player.size.Z}" };
                        redactform.Controls.Add(sizeInput3);
                        Label colorLabel3 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel3);
                        TextBox colorInput3 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{player.color.R},{player.color.G},{player.color.B}" };
                        redactform.Controls.Add(colorInput3);
                        Label directionLabel3 = new Label { Text = "Направление:", Location = new Point(270, 160), Size = new Size(80, 20) };
                        redactform.Controls.Add(directionLabel3);
                        TextBox directionInput3 = new TextBox { Location = new Point(350, 160), Size = new Size(150, 20), Text = player.direction };
                        redactform.Controls.Add(directionInput3);
                        CheckBox canScript3 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = player.canscriptable };
                        redactform.Controls.Add(canScript3);
                        CheckBox canWrite3 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = player.canscriptwrite };
                        redactform.Controls.Add(canWrite3);
                        TextBox scriptInput3 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = player.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput3);
                        AttachScriptTooltip(scriptInput3);
                        Button save3 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save3.Click += (s, ev) =>
                        {
                            player.name = nameInput3.Text;
                            player.position = parsevector3(positionInput3.Text);
                            player.size = parsevector3(sizeInput3.Text);
                            player.color = parsecolor(colorInput3.Text);
                            player.canscriptable = canScript3.Checked;
                            player.canscriptwrite = canWrite3.Checked;
                            player.direction = directionInput3.Text;
                            player.script = scriptInput3.Text;
                            node.Text = player.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save3);
                    }
                    break;
                case Wall wall:
                    {
                        Label nameLabel4 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel4);
                        TextBox nameInput4 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = wall.name };
                        redactform.Controls.Add(nameInput4);
                        Label positionLabel4 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel4);
                        TextBox positionInput4 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{wall.position.X},{wall.position.Y},{wall.position.Z}" };
                        redactform.Controls.Add(positionInput4);
                        Label sizeLabel4 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel4);
                        TextBox sizeInput4 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{wall.size.X},{wall.size.Y},{wall.size.Z}" };
                        redactform.Controls.Add(sizeInput4);
                        Label colorLabel4 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel4);
                        TextBox colorInput4 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{wall.color.R},{wall.color.G},{wall.color.B}" };
                        redactform.Controls.Add(colorInput4);
                        Label sideLabel4 = new Label { Text = "Сторона:", Location = new Point(270, 160), Size = new Size(80, 20) };
                        redactform.Controls.Add(sideLabel4);
                        TextBox sideInput4 = new TextBox { Location = new Point(350, 160), Size = new Size(150, 20), Text = wall.side };
                        redactform.Controls.Add(sideInput4);
                        CheckBox canScript4 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = wall.canscriptable };
                        redactform.Controls.Add(canScript4);
                        CheckBox canWrite4 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = wall.canscriptwrite };
                        redactform.Controls.Add(canWrite4);
                        TextBox scriptInput4 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = wall.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput4);
                        AttachScriptTooltip(scriptInput4);
                        Button save4 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save4.Click += (s, ev) =>
                        {
                            wall.name = nameInput4.Text;
                            wall.position = parsevector3(positionInput4.Text);
                            wall.size = parsevector3(sizeInput4.Text);
                            wall.color = parsecolor(colorInput4.Text);
                            wall.side = sideInput4.Text;
                            wall.canscriptable = canScript4.Checked;
                            wall.canscriptwrite = canWrite4.Checked;
                            wall.script = scriptInput4.Text;
                            node.Text = wall.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save4);
                    }
                    break;
                case Lamp lamp:
                    {
                        Label nameLabel5 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel5);
                        TextBox nameInput5 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = lamp.name };
                        redactform.Controls.Add(nameInput5);
                        Label positionLabel5 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel5);
                        TextBox positionInput5 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{lamp.position.X},{lamp.position.Y},{lamp.position.Z}" };
                        redactform.Controls.Add(positionInput5);
                        Label sizeLabel5 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel5);
                        TextBox sizeInput5 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{lamp.size.X},{lamp.size.Y},{lamp.size.Z}" };
                        redactform.Controls.Add(sizeInput5);
                        Label colorLabel5 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel5);
                        TextBox colorInput5 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{lamp.color.R},{lamp.color.G},{lamp.color.B}" };
                        redactform.Controls.Add(colorInput5);
                        CheckBox stateInput5 = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Включено", Checked = lamp.state };
                        redactform.Controls.Add(stateInput5);
                        CheckBox canScript5 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = lamp.canscriptable };
                        redactform.Controls.Add(canScript5);
                        CheckBox canWrite5 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = lamp.canscriptwrite };
                        redactform.Controls.Add(canWrite5);
                        TextBox scriptInput5 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = lamp.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput5);
                        AttachScriptTooltip(scriptInput5);
                        Button save5 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save5.Click += (s, ev) =>
                        {
                            lamp.name = nameInput5.Text;
                            lamp.position = parsevector3(positionInput5.Text);
                            lamp.size = parsevector3(sizeInput5.Text);
                            lamp.color = parsecolor(colorInput5.Text);
                            lamp.state = stateInput5.Checked;
                            lamp.canscriptable = canScript5.Checked;
                            lamp.canscriptwrite = canWrite5.Checked;
                            lamp.script = scriptInput5.Text;
                            lamp.SetState(stateInput5.Checked);
                            node.Text = lamp.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save5);
                    }
                    break;
                case Buttongi buttongi:
                    {
                        Label nameLabel6 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel6);
                        TextBox nameInput6 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = buttongi.name };
                        redactform.Controls.Add(nameInput6);
                        Label positionLabel6 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel6);
                        TextBox positionInput6 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{buttongi.position.X},{buttongi.position.Y},{buttongi.position.Z}" };
                        redactform.Controls.Add(positionInput6);
                        Label sizeLabel6 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel6);
                        TextBox sizeInput6 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{buttongi.size.X},{buttongi.size.Y},{buttongi.size.Z}" };
                        redactform.Controls.Add(sizeInput6);
                        Label colorLabel6 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel6);
                        TextBox colorInput6 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{buttongi.color.R},{buttongi.color.G},{buttongi.color.B}" };
                        redactform.Controls.Add(colorInput6);
                        CheckBox stateInput6 = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Активно", Checked = buttongi.state };
                        redactform.Controls.Add(stateInput6);
                        CheckBox canScript6 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = buttongi.canscriptable };
                        redactform.Controls.Add(canScript6);
                        CheckBox canWrite6 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = buttongi.canscriptwrite };
                        redactform.Controls.Add(canWrite6);
                        TextBox scriptInput6 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = buttongi.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput6);
                        AttachScriptTooltip(scriptInput6);
                        Button save6 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save6.Click += (s, ev) =>
                        {
                            buttongi.name = nameInput6.Text;
                            buttongi.position = parsevector3(positionInput6.Text);
                            buttongi.size = parsevector3(sizeInput6.Text);
                            buttongi.color = parsecolor(colorInput6.Text);
                            buttongi.state = stateInput6.Checked;
                            buttongi.canscriptable = canScript6.Checked;
                            buttongi.canscriptwrite = canWrite6.Checked;
                            buttongi.script = scriptInput6.Text;
                            buttongi.color = buttongi.state ? Color.Green : Color.Red;
                            node.Text = buttongi.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save6);
                    }
                    break;
                case CubeText cubeText:
                    {
                        Label nameLabel7 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel7);
                        TextBox nameInput7 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = cubeText.name };
                        redactform.Controls.Add(nameInput7);
                        Label positionLabel7 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel7);
                        TextBox positionInput7 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{cubeText.position.X},{cubeText.position.Y},{cubeText.position.Z}" };
                        redactform.Controls.Add(positionInput7);
                        Label sizeLabel7 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel7);
                        TextBox sizeInput7 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{cubeText.size.X},{cubeText.size.Y},{cubeText.size.Z}" };
                        redactform.Controls.Add(sizeInput7);
                        Label colorLabel7 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel7);
                        TextBox colorInput7 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{cubeText.color.R},{cubeText.color.G},{cubeText.color.B}" };
                        redactform.Controls.Add(colorInput7);
                        Label textLabel7 = new Label { Text = "Текст:", Location = new Point(270, 160), Size = new Size(80, 20) };
                        redactform.Controls.Add(textLabel7);
                        TextBox textInput7 = new TextBox { Location = new Point(350, 160), Size = new Size(150, 20), Text = cubeText.text };
                        redactform.Controls.Add(textInput7);
                        CheckBox canScript7 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = cubeText.canscriptable };
                        redactform.Controls.Add(canScript7);
                        CheckBox canWrite7 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = cubeText.canscriptwrite };
                        redactform.Controls.Add(canWrite7);
                        TextBox scriptInput7 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = cubeText.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput7);
                        AttachScriptTooltip(scriptInput7);
                        Button save7 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save7.Click += (s, ev) =>
                        {
                            cubeText.name = nameInput7.Text;
                            cubeText.position = parsevector3(positionInput7.Text);
                            cubeText.size = parsevector3(sizeInput7.Text);
                            cubeText.color = parsecolor(colorInput7.Text);
                            cubeText.text = textInput7.Text;
                            cubeText.canscriptable = canScript7.Checked;
                            cubeText.canscriptwrite = canWrite7.Checked;
                            cubeText.script = scriptInput7.Text;
                            node.Text = cubeText.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save7);
                    }
                    break;
                case Redstone redstone:
                    {
                        Label nameLabel8 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel8);
                        TextBox nameInput8 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = redstone.name };
                        redactform.Controls.Add(nameInput8);
                        Label positionLabel8 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel8);
                        TextBox positionInput8 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{redstone.position.X},{redstone.position.Y},{redstone.position.Z}" };
                        redactform.Controls.Add(positionInput8);
                        Label sizeLabel8 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel8);
                        TextBox sizeInput8 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{redstone.size.X},{redstone.size.Y},{redstone.size.Z}" };
                        redactform.Controls.Add(sizeInput8);
                        Label colorLabel8 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel8);
                        TextBox colorInput8 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{redstone.color.R},{redstone.color.G},{redstone.color.B}" };
                        redactform.Controls.Add(colorInput8);
                        CheckBox canScript8 = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = redstone.canscriptable };
                        redactform.Controls.Add(canScript8);
                        CheckBox canWrite8 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Можно редактировать", Checked = redstone.canscriptwrite };
                        redactform.Controls.Add(canWrite8);
                        TextBox scriptInput8 = new TextBox { Location = new Point(270, 220), Size = new Size(300, 80), Multiline = true, Text = redstone.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput8);
                        AttachScriptTooltip(scriptInput8);
                        Button save8 = new Button { Location = new Point(270, 310), Size = new Size(150, 30), Text = "Сохранить" };
                        save8.Click += (s, ev) =>
                        {
                            redstone.name = nameInput8.Text;
                            redstone.position = parsevector3(positionInput8.Text);
                            redstone.size = parsevector3(sizeInput8.Text);
                            redstone.color = parsecolor(colorInput8.Text);
                            redstone.canscriptable = canScript8.Checked;
                            redstone.canscriptwrite = canWrite8.Checked;
                            redstone.script = scriptInput8.Text;
                            node.Text = redstone.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save8);
                    }
                    break;
                case Redstonedust redstonedust:
                    {
                        Label nameLabel9 = new Label { Text = "Имя объекта:", Location = new Point(270, 35), Size = new Size(80, 20) };
                        redactform.Controls.Add(nameLabel9);
                        TextBox nameInput9 = new TextBox { Location = new Point(350, 35), Size = new Size(150, 20), Text = redstonedust.name };
                        redactform.Controls.Add(nameInput9);
                        Label positionLabel9 = new Label { Text = "Позиция:", Location = new Point(270, 70), Size = new Size(80, 20) };
                        redactform.Controls.Add(positionLabel9);
                        TextBox positionInput9 = new TextBox { Location = new Point(350, 70), Size = new Size(150, 20), Text = $"{redstonedust.position.X},{redstonedust.position.Y},{redstonedust.position.Z}" };
                        redactform.Controls.Add(positionInput9);
                        Label sizeLabel9 = new Label { Text = "Размер:", Location = new Point(270, 100), Size = new Size(80, 20) };
                        redactform.Controls.Add(sizeLabel9);
                        TextBox sizeInput9 = new TextBox { Location = new Point(350, 100), Size = new Size(150, 20), Text = $"{redstonedust.size.X},{redstonedust.size.Y},{redstonedust.size.Z}" };
                        redactform.Controls.Add(sizeInput9);
                        Label colorLabel9 = new Label { Text = "Цвет:", Location = new Point(270, 130), Size = new Size(80, 20) };
                        redactform.Controls.Add(colorLabel9);
                        TextBox colorInput9 = new TextBox { Location = new Point(350, 130), Size = new Size(150, 20), Text = $"{redstonedust.color.R},{redstonedust.color.G},{redstonedust.color.B}" };
                        redactform.Controls.Add(colorInput9);
                        CheckBox stateInput9 = new CheckBox { Location = new Point(270, 160), Size = new Size(150, 20), Text = "Включено", Checked = redstonedust.state };
                        redactform.Controls.Add(stateInput9);
                        CheckBox canScript9 = new CheckBox { Location = new Point(270, 190), Size = new Size(150, 20), Text = "Скрипт доступен", Checked = redstonedust.canscriptable };
                        redactform.Controls.Add(canScript9);
                        CheckBox canWrite9 = new CheckBox { Location = new Point(270, 220), Size = new Size(150, 20), Text = "Можно редактировать", Checked = redstonedust.canscriptwrite };
                        redactform.Controls.Add(canWrite9);
                        TextBox scriptInput9 = new TextBox { Location = new Point(270, 250), Size = new Size(300, 80), Multiline = true, Text = redstonedust.script, ScrollBars = ScrollBars.Vertical };
                        redactform.Controls.Add(scriptInput9);
                        AttachScriptTooltip(scriptInput9);
                        Button save9 = new Button { Location = new Point(270, 340), Size = new Size(150, 30), Text = "Сохранить" };
                        save9.Click += (s, ev) =>
                        {
                            redstonedust.name = nameInput9.Text;
                            redstonedust.position = parsevector3(positionInput9.Text);
                            redstonedust.size = parsevector3(sizeInput9.Text);
                            redstonedust.color = parsecolor(colorInput9.Text);
                            redstonedust.state = stateInput9.Checked;
                            redstonedust.canscriptable = canScript9.Checked;
                            redstonedust.canscriptwrite = canWrite9.Checked;
                            redstonedust.script = scriptInput9.Text;
                            redstonedust.SetState(stateInput9.Checked);
                            node.Text = redstonedust.name;
                            renderwindow();
                        };
                        redactform.Controls.Add(save9);
                    }
                    break;
                default:
                    break;
            }
        }

        private Dictionary<string, string> BuildHelpDictionaryLocal()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dict["moveto"] = "moveto(x, y); — переместить объект в координаты x,y. Пример: this.moveto(10, 15);";
            dict["move"] = "move(dx, dy); — сдвинуть объект. Пример: this.move(1, 0);";
            dict["setcolor"] = "setcolor(r, g, b); — установить цвет. Пример: this.setcolor(255, 0, 0);";
            dict["setdirection"] = "setdirection(dir); — установить направление. Пример: this.setdirection(\"right\");";
            dict["rotate"] = "rotate(angle); — повернуть объект. Пример: this.rotate(90);";
            dict["mforward"] = "mforward(n); — переместиться вперед. Пример: this.mforward(3);";
            dict["mbackward"] = "mbackward(n); — переместиться назад. Пример: this.mbackward(2);";
            dict["settext"] = "settext(\"text\"); — установить текст. Пример: this.settext(\"Hello\");";
            dict["interact"] = "interact(target); — взаимодействовать. Пример: this.interact(\"door\");";
            dict["push"] = "push(collection, item); — добавить элемент. Пример: this.push(\"inventory\", \"sword\");";
            dict["remove"] = "remove(obj); — удалить объект. Пример: this.remove(\"enemy1\");";
            dict["for"] = "for (i=0;i<n;i++) { ... } — цикл for. Пример: for (i=0;i<3;i++) { this.move(1,0); }";
            dict["while"] = "while (cond) { ... } — цикл while. Пример: while (not at_goal) { this.mforward(1); }";
            dict["if"] = "if (cond) { ... } — условный оператор. Пример: if (this.getcolor() == 255) { this.settext(\"red\"); }";
            dict["foreach"] = "foreach (item in collection) { ... } — итерация. Пример: foreach (obj in enemies) { this.remove(obj); }";
            dict["print"] = "print(expr); — вывести значение. Пример: this.print(\"Done\");";
            dict["paint"] = "paint(x,y,r,g,b); — закрасить позицию. Пример: this.paint(5,4,0,255,0);";
            dict["getcolor"] = "getcolor() — получить цвет this. Пример: int c = this.getcolor();";
            dict["getdirection"] = "getdirection() — получить направление this. Пример: string d = this.getdirection();";
            dict["getposition"] = "getposition() — получить позицию this. Пример: var pos = this.getposition();";
            dict["gettext"] = "gettext() — получить текст this. Пример: string t = this.gettext();";
            dict["and"] = "and — логическое И. Пример: if (a and b) { ... }";
            dict["or"] = "or — логическое ИЛИ. Пример: if (a or b) { ... }";
            dict["not"] = "not — логическое НЕ. Пример: if (not open) { this.open(); }";
            dict["int"] = "int — число. Пример: int a = 5;";
            dict["string"] = "string — строка. Пример: string s = \"hi\";";
            dict["bool"] = "bool — булево. Пример: bool ok = true;";
            dict["object"] = "object — ссылка. Пример: object o = this;";
            dict["array"] = "array — массив. Пример: array a = [1,2,3];";
            return dict;
        }

        private void AttachScriptTooltip(TextBox tb)
        {
            if (tb == null) return;
            tb.MouseMove += (s, e) =>
            {
                try
                {
                    int idx = tb.GetCharIndexFromPosition(e.Location);
                    if (idx < 0 || idx >= tb.Text.Length) { scriptToolTip.Hide(tb); lastTipKey = null; return; }
                    int start = idx;
                    while (start > 0 && (char.IsLetterOrDigit(tb.Text[start - 1]) || tb.Text[start - 1] == '_')) start--;
                    int end = idx;
                    while (end < tb.Text.Length && (char.IsLetterOrDigit(tb.Text[end]) || tb.Text[end] == '_')) end++;
                    if (end <= start) { scriptToolTip.Hide(tb); lastTipKey = null; return; }
                    string word = tb.Text.Substring(start, end - start).Trim();
                    if (string.IsNullOrEmpty(word)) { scriptToolTip.Hide(tb); lastTipKey = null; return; }
                    string key = word.ToLower();
                    if (helpDict != null && helpDict.TryGetValue(key, out string tip))
                    {
                        if (lastTipKey == key) return;
                        lastTipKey = key;
                        scriptToolTip.AutoPopDelay = 5000;
                        scriptToolTip.InitialDelay = 300;
                        scriptToolTip.ReshowDelay = 100;
                        scriptToolTip.Show(tip.Replace("\\n", Environment.NewLine), tb, e.Location.X + 15, e.Location.Y + 15, 4000);
                    }
                    else
                    {
                        scriptToolTip.Hide(tb);
                        lastTipKey = null;
                    }
                }
                catch { }
            };
            tb.MouseLeave += (s, e) => { scriptToolTip.Hide(tb); lastTipKey = null; };
        }

    }
}


