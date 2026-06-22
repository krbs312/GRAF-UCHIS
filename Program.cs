using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using DiplomApp;

namespace DiplomApp
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup exception:\n" + ex.ToString());
            }
        }
    }

    class MainForm : Form
    {
        bool redactormode = false;
        string tasktext = "";
        List<string> task_ogranich = new List<string> { };

        List<Mapobject> mapObjects = new List<Mapobject> { };
        List<(Color, int,int)> mapcolors = new List<(Color, int,int)> { };
        string? currentMapFilePath = null;

        public void addmapobject(Mapobject obj)
        {
            mapObjects.Add(obj);
        }
        public List<Mapobject> GetMapobjects()
        {
            return mapObjects;
        }
        public bool getredactormode()
        {
            return redactormode;
        }

        int plotsize = 5;
        private Renderer renderer;

        public Vector2 cameraposition = new Vector2(0, 0);
        private bool isDragging = false;
        private Point lastMousePos;

        public void renderwindow()
        {
            this.Invalidate();
        }
        
        public MainForm()
        {
            this.MouseDown += MainForm_MouseDown;
            this.MouseMove += MainForm_MouseMove;
            this.MouseUp += MainForm_MouseUp;
            this.MouseWheel += MainForm_MouseWheel;

            this.Shown += (s, e) => {
                renderwindow();
            };
            this.Resize += (sender, e) => { renderwindow(); };
            this.Text = "Граф Учис";
            this.Size = new System.Drawing.Size(1200, 1000);
            this.DoubleBuffered = true;

            // Add modern menu
            MenuStrip menuStrip = new MenuStrip();
            menuStrip.BackColor = Color.WhiteSmoke;
            menuStrip.Font = new Font("Segoe UI", 9);

            ToolStripMenuItem fileМеню = new ToolStripMenuItem("Файл");
            ToolStripMenuItem newItem = new ToolStripMenuItem("Новая карта", null, (s, e) => { mapObjects.Clear(); mapcolors.Clear(); tasktext = ""; task_ogranich.Clear(); renderwindow(); });
            ToolStripMenuItem openItem = new ToolStripMenuItem("Открыть", null, (s, e) => {
                var loaded = MapSaver.LoadMap(ref tasktext, task_ogranich, out string? loadedFilePath);
                if (loaded != null)
                {
                    mapObjects = loaded;
                    currentMapFilePath = loadedFilePath;
                    mapcolors.Clear();
                    renderwindow();
                }
            });
            ToolStripMenuItem saveItem = new ToolStripMenuItem("Сохранить", null, (s, e) => { MapSaver.SaveMap(mapObjects, tasktext, task_ogranich); });
            fileМеню.DropDownItems.AddRange(new ToolStripItem[] { newItem, openItem, saveItem });

            ToolStripMenuItem editМеню = new ToolStripMenuItem("Редактор");
            ToolStripMenuItem toggleRedactor = new ToolStripMenuItem("Переключить режим редактора", null, (s, e) => { redactormode = !redactormode; renderwindow(); });
            editМеню.DropDownItems.Add(toggleRedactor);

            ToolStripMenuItem viewМеню = new ToolStripMenuItem("Вид");
            ToolStripMenuItem zoomIn = new ToolStripMenuItem("Увеличить", null, (s, e) => { plotsize = Math.Min(plotsize + 1, 10); renderwindow(); });
            ToolStripMenuItem zoomOut = new ToolStripMenuItem("Уменьшить", null, (s, e) => { plotsize = Math.Max(plotsize - 1, 1); renderwindow(); });
            viewМеню.DropDownItems.AddRange(new ToolStripItem[] { zoomIn, zoomOut });

            ToolStripMenuItem tasksМеню = new ToolStripMenuItem("Задания");
            LoadTasksMenu(tasksМеню);

            ToolStripMenuItem helpМеню = new ToolStripMenuItem("Справка");
            helpМеню.Click += (s, e) => { ShowHelpDialog(); };

            menuStrip.Items.AddRange(new ToolStripItem[] { fileМеню, editМеню, viewМеню, tasksМеню, helpМеню });

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);
            
            renderer = new Renderer();
            Mapobject.renderwindow = () => this.renderwindow();
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;
        }

        private void LoadTasksMenu(ToolStripMenuItem tasksМеню)
        {
            try
            {
                string tasksFolderPath = System.IO.Path.Combine(Application.StartupPath, "task");
                
                if (System.IO.Directory.Exists(tasksFolderPath))
                {
                    string[] taskFiles = System.IO.Directory.GetFiles(tasksFolderPath, "*.map");
                    
                    if (taskFiles.Length == 0)
                    {
                        ToolStripMenuItem noTasksItem = new ToolStripMenuItem("Нет заданий");
                        noTasksItem.Enabled = false;
                        tasksМеню.DropDownItems.Add(noTasksItem);
                    }
                    else
                    {
                        System.Array.Sort(taskFiles);
                        
                        foreach (string taskFile in taskFiles)
                        {
                            string taskName = System.IO.Path.GetFileNameWithoutExtension(taskFile);
                            ToolStripMenuItem taskItem = new ToolStripMenuItem(taskName, null, (s, e) =>
                            {
                                try
                                {
                                    mapObjects.Clear();
                                    mapcolors.Clear();
                                    tasktext = "";
                                    task_ogranich.Clear();
                                    
                                    var loaded = MapSaver.LoadMapFromFile(taskFile, ref tasktext, task_ogranich);
                                    if (loaded != null)
                                    {
                                        mapObjects = loaded;
                                        currentMapFilePath = taskFile;
                                        renderwindow();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show("Ошибка при загрузке задания:\n" + ex.Message);
                                }
                            });
                            tasksМеню.DropDownItems.Add(taskItem);
                        }
                    }
                }
                else
                {
                    ToolStripMenuItem noFolderItem = new ToolStripMenuItem("Папка заданий не найдена");
                    noFolderItem.Enabled = false;
                    tasksМеню.DropDownItems.Add(noFolderItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке меню заданий:\n" + ex.Message);
            }
        }

        private Dictionary<string, string> BuildHelpDictionary()
        {
            var dict = new Dictionary<string, string>();

            foreach (var name in Enum.GetNames(typeof(Commands)))
            {
                dict[name] = name switch
                {
                    "moveto" => "moveto(x, y); — переместить объект в координаты x, y.\nПример:\nthis.moveto(10, 15);",
                    "move" => "move(dx, dy); — сдвинуть объект на dx, dy относительно текущей позиции.\nПример:\nthis.move(1, 0);",
                    "setcolor" => "setcolor(r, g, b); — установить цвет объекта (0-255).\nПример:\nthis.setcolor(255, 0, 0);",
                    "setdirection" => "setdirection(dir); — установить направление (например: up, down, left, right или угол).\nПример:\nthis.setdirection(\"right\");",
                    "rotate" => "rotate(angle); — повернуть объект на угол (градусы).\nПример:\nthis.rotate(90);",
                    "mforward" => "mforward(n); — переместиться вперед на n шагов.\nПример:\nthis.mforward(3);",
                    "mbackward" => "mbackward(n); — переместиться назад на n шагов.\nПример:\nthis.mbackward(2);",
                    "settext" => "settext(\"text\"); — установить текст у объекта.\nПример:\nthis.settext(\"Hello\");",
                    "interact" => "interact(target); — взаимодействовать с указанным объектом по имени.\nПример:\nthis.interact(\"door\");",
                    "push" => "push(collection, item); — добавить элемент в коллекцию.\nПример:\nthis.push(\"inventory\", \"sword\");",
                    "remove" => "remove(obj); — удалить объект по имени.\nПример:\nthis.remove(\"enemy1\");",
                    "_for_" => "for (i=0; i<n) { ... } — цикл for.\nПример:\nfor (i=0; i<3; i++) { this.move(1, 0); }",
                    "_if_" => "if (cond) { ... } else { ... } — условный оператор.\nПример:\nif (this.getcolor() == 255) { this.settext(\"red\"); }",
                    "_print_" => "print(expr); — вывести выражение в лог/консоль.\nПример:\nthis.print(\"Done\");",
                    "paint" => "paint(x, y, r, g, b); — закрасить клетку/позицию цветом (RGB).\nПример:\nthis.paint(5, 4, 0, 255, 0);",
                    _ => name + " — описание отсутствует.",
                };
            }

            dict["getcolor"] = "getcolor() — получить цвет текущего объекта (this).\nПример:\nint c = this.getcolor();";
            dict["getdirection"] = "getdirection() — получить направление текущего объекта.\nПример:\nstring d = this.getdirection();";
            dict["getposition"] = "getposition() — получить позицию текущего объекта (возвращает структуру с x/y).\nПример:\nvar pos = this.getposition();\nthis.print(pos.x);";
            dict["gettext"] = "gettext() — получить текст текущего объекта.\nПример:\nstring t = this.gettext();";

            dict["and"] = "and — логическое И.\nПример:\nif (cond1 and cond2) { ... }";
            dict["or"] = "or — логическое ИЛИ.\nПример:\nif (cond1 or cond2) { ... }";
            dict["not"] = "not — логическое отрицание.\nПример:\nif (not is_open) { this.open(); }";

            dict["int"] = "int — целочисленный тип переменной.\nПример:\nint a = 5;";
            dict["string"] = "string — строковый тип переменной.\nПример:\nstring s = \"hello\";";
            dict["bool"] = "bool — логический тип переменной.\nПример:\nbool ok = true;";
            dict["object"] = "object — ссылка на объект в сцене.\nПример:\nobject o = this;";
            dict["array"] = "array — массив значений.\nПримеры:\nint[] arr = {1, 2, 3};\nint[] empty = {};\npush(empty, 10);\nremove(arr, 0);";

            return dict;
        }

        private void ShowHelpDialog()
        {
            var dict = BuildHelpDictionary();
            Form helpForm = new Form();
            helpForm.Text = "Справка по командам и операторам";
            helpForm.Size = new Size(800, 520);

            TextBox searchBox = new TextBox();
            searchBox.PlaceholderText = "Поиск...";
            searchBox.Dock = DockStyle.Top;

            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 220;

            ListBox list = new ListBox();
            list.Dock = DockStyle.Fill;
            list.Font = new Font("Consolas", 10);

            TextBox details = new TextBox();
            details.Multiline = true;
            details.ReadOnly = true;
            details.ScrollBars = ScrollBars.Vertical;
            details.Dock = DockStyle.Fill;
            details.Font = new Font("Consolas", 10);

            foreach (var k in dict.Keys.OrderBy(k => k)) list.Items.Add(k);

            searchBox.TextChanged += (s, e) =>
            {
                string q = searchBox.Text.ToLower();
                list.BeginUpdate();
                list.Items.Clear();
                foreach (var k in dict.Keys.OrderBy(k => k))
                {
                    if (string.IsNullOrEmpty(q) || k.ToLower().Contains(q) || dict[k].ToLower().Contains(q))
                        list.Items.Add(k);
                }
                list.EndUpdate();
            };

            list.SelectedIndexChanged += (s, e) =>
            {
                if (list.SelectedItem != null)
                {
                    string key = list.SelectedItem.ToString();
                    if (dict.ContainsKey(key)) details.Text = dict[key].Replace("\n", Environment.NewLine);
                    else details.Text = "";
                }
            };

            split.Panel1.Controls.Add(list);
            split.Panel2.Controls.Add(details);

            helpForm.Controls.Add(split);
            helpForm.Controls.Add(searchBox);

            if (list.Items.Count > 0) list.SelectedIndex = 0;

            helpForm.ShowDialog();
        }

        private void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                ShowHelpDialog();
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            try
            {
                renderer.Renderlevel(e.Graphics, mapObjects, this, plotsize, cameraposition, redactormode, mapcolors, tasktext);
            }
            catch (Exception ex)
            {
                MessageBox.Show("OnPaint exception:\n" + ex.ToString());
            }
        }

        enum Ogranich { moveto, move, mforvard, mbackward, rotate, setdirection, _if_, _for_ };

        public bool OgranichCheck()
        {
            List<string> commandsogran = new List<string> { "moveto", "move", "mforvard", "mbackward", "rotate", "setdirection", "_if_", "_for_" };
            foreach (var ogranich in task_ogranich)
            {
                if (commandsogran.Contains(ogranich))
                {
                    foreach (var obj in mapObjects){
                        if (obj.script.Contains(ogranich))
                        {
                            MessageBox.Show($"Ogranich {ogranich} is in use in object {obj.name}");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            mousehandle mouseHandler = new mousehandle();
            if (e.Button == MouseButtons.Left)
            {

                if (!redactormode && mouseHandler.isstartbutton(e.Location, this))
                {
                }
                else if (redactormode && mouseHandler.isaddbutton(e.Location, this))
                {
                    Form addForm = new Form();
                    addForm.Text = "Добавить объект";
                    addForm.Size = new Size(300, 200);

                    Label typeLabel = new Label { Text = "Выберите тип объекта:", Location = new Point(10, 10), Size = new Size(260, 20) };
                    addForm.Controls.Add(typeLabel);
                    ComboBox typeBox = new ComboBox { Location = new Point(10, 35), Size = new Size(260, 24) };
                    typeBox.Items.AddRange(new string[] { "Cube", "Sphere", "Player", "Wall", "Lamp", "Buttongi", "CubeText", "Redstone", "Redstonedust" });
                    typeBox.SelectedIndex = 0;
                    addForm.Controls.Add(typeBox);

                    Button createButton = new Button { Text = "Создать объект", Location = new Point(10, 70), Size = new Size(120, 30) };
                    createButton.Click += (s, ev) =>
                    {
                        string type = typeBox.SelectedItem.ToString();
                        Mapobject newObj;
                        switch (type)
                        {
                            case "Cube":
                                newObj = new Cube(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, true, "cube");
                                break;
                            case "Sphere":
                                newObj = new Sphere(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, true, "sphere");
                                break;
                            case "Player":
                                newObj = new Player(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Blue, true, true, "player");
                                break;
                            case "Wall":
                                newObj = new Wall(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Gray, true, false, "left", "wall");
                                break;
                            case "Lamp":
                                newObj = new Lamp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Yellow, true, false, false, "lamp");
                                break;
                            case "Buttongi":
                                newObj = new Buttongi(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Green, true, false, false, "buttongi");
                                break;
                            case "CubeText":
                                newObj = new CubeText(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, true, "text", "cubetext");
                                break;
                            case "Redstone":
                                newObj = new Redstone(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Red, true, false, "redstone");
                                break;
                            case "Redstonedust":
                                newObj = new Redstonedust(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.Gray, true, false, false, "redstonedust");
                                break;
                            default:
                                newObj = null;
                                break;
                        }
                        if (newObj != null)
                        {
                            mapObjects.Add(newObj);
                            renderwindow();
                        }
                        addForm.Close();
                    };
                    addForm.Controls.Add(createButton);

                    addForm.ShowDialog();
                    return;
                }
                else if (mouseHandler.istreebutton(e.Location, this))
                {
                    if (redactormode)
                    {
                        Redactormode redactForm = new Redactormode();
                        redactForm.renderwindow = () => this.renderwindow();
                        redactForm.Showredactform(mapObjects);
                        return;
                    }
                    return;
                }
                
                if (mouseHandler.isstartbutton(e.Location, this))
                {
                    if (redactormode)
                    {
                        Redactormode redactForm = new Redactormode();
                        redactForm.renderwindow = () => this.renderwindow();
                        redactForm.Showredactform(mapObjects);
                        return;
                    }
                    if (!string.IsNullOrEmpty(currentMapFilePath))
                    {
                        // Сохранить скрипты текущих объектов
                        var scriptsMap = new Dictionary<string, string>();
                        foreach (var obj in mapObjects)
                        {
                            scriptsMap[obj.name] = obj.script;
                        }
                        
                        var reloaded = MapSaver.LoadMapFromFile(currentMapFilePath, ref tasktext, task_ogranich);
                        if (reloaded == null)
                        {
                            MessageBox.Show("Не удалось перезагрузить карту для выполнения скриптов.");
                            return;
                        }
                        
                        // Применить сохраненные скрипты к загруженным объектам
                        foreach (var obj in reloaded)
                        {
                            if (scriptsMap.ContainsKey(obj.name))
                            {
                                obj.script = scriptsMap[obj.name];
                            }
                        }
                        
                        mapObjects = reloaded;
                        mapcolors.Clear();
                        renderwindow();
                    }
                    Interpriter interpriter = new Interpriter();
                    foreach (var obj in mapObjects)
                    {
                        if (obj.canscriptable)
                        {
                            if (!OgranichCheck())
                            {
                                MessageBox.Show("Cannot start task due to ogranich in use");
                                return;
                            }
                            interpriter.ExecuteScript(obj, mapObjects, mapcolors, null, null);
                        }
                    }
                    return;
                }
                if (mouseHandler.istaskbutton(e.Location, this))
                {
                    if (redactormode)
                    {
                        Form taskForm = new Form();
                        taskForm.Text = "Создать задачу";
                        taskForm.Size = new Size(300, 320);

                        Label taskTextLabel = new Label { Text = "Описание задачи:", Location = new Point(10, 10), Size = new Size(260, 20) };
                        taskForm.Controls.Add(taskTextLabel);
                        TextBox taskTextBox = new TextBox();
                        taskTextBox.Location = new Point(10, 35);
                        taskTextBox.Size = new Size(260, 24);
                        taskForm.Controls.Add(taskTextBox);

                        Label restrictionLabel = new Label { Text = "Ограничения для задачи:", Location = new Point(10, 70), Size = new Size(260, 20) };
                        taskForm.Controls.Add(restrictionLabel);
                        CheckedListBox taskOgranichList = new CheckedListBox();
                        taskOgranichList.Location = new Point(10, 95);
                        taskOgranichList.Size = new Size(260, 120);

                        taskOgranichList.Items.AddRange(Enum.GetNames(typeof(Ogranich)));

                        taskOgranichList.ItemCheck += (s, ev) =>
                        {
                            string selectedOgranich = taskOgranichList.Items[ev.Index].ToString();
                            if (ev.NewValue == CheckState.Checked)
                            {
                                task_ogranich.Add(selectedOgranich);
                            }
                            else
                            {
                                task_ogranich.Remove(selectedOgranich);
                            }
                        };
                        taskForm.Controls.Add(taskOgranichList);
                        Button createTaskButton = new Button { Text = "Сохранить задачу", Location = new Point(10, 225), Size = new Size(100, 30) };
                        createTaskButton.Click += (s, ev) =>
                        {
                            tasktext = taskTextBox.Text;
                            taskForm.Close();
                            renderwindow();
                        };
                        taskForm.Controls.Add(createTaskButton);
                        taskForm.Show();
                        return;
                    }
                    return;
                }
                if (mouseHandler.isstartbutton(e.Location, this))
                {
                    if (redactormode)
                    {
                        Redactormode redactForm = new Redactormode();
                        redactForm.renderwindow = () => this.renderwindow();
                        redactForm.Showredactform(mapObjects);
                        return;
                    }
                    Interpriter interpriter = new Interpriter();
                    foreach (var obj in mapObjects)
                    {
                        if (obj.canscriptable)
                        {
                            if (!OgranichCheck())
                            {
                                MessageBox.Show("Cannot start task due to ogranich in use");
                                return;
                            }
                            interpriter.ExecuteScript(obj, mapObjects, mapcolors, null, null);
                        }
                    }
                    return;
                }
                if (mouseHandler.ismenubutton(e.Location, this))
                {
                    Form menuForm = new Form();
                    menuForm.Text = "Меню";
                    menuForm.Size = new Size(300, 200);
                    Button redactButton = new Button();
                    redactButton.Text = "Режим редактора";
                    redactButton.Location = new Point(0, 10);
                    redactButton.Size = new Size(150, 50);
                    if (redactormode)
                    {
                        redactButton.BackColor = Color.Green;
                    }
                    else
                    {
                        redactButton.BackColor = Color.Red;
                    }
                    redactButton.Click += (s, ev) =>
                    {
                        redactormode = !redactormode;
                        renderwindow();
                        if (redactormode)
                        {
                            redactButton.BackColor = Color.Green;
                        }
                        else
                        {
                            redactButton.BackColor = Color.Red;
                        }
                    };
                    menuForm.Controls.Add(redactButton);
                    Label menuHelpLabel = new Label { Text = "Сохранить/загрузить карту или переключить режим редактора.", Location = new Point(160, 10), Size = new Size(130, 30) };
                    menuForm.Controls.Add(menuHelpLabel);
                    Button saveButton = new Button();
                    saveButton.Text = "Сохранить карту";
                    saveButton.Location = new Point(0, 70);
                    saveButton.Size = new Size(150, 50);
                    saveButton.Click += (s, ev) =>
                    {
                        MapSaver.SaveMap(mapObjects, tasktext, task_ogranich);
                    };
                    menuForm.Controls.Add(saveButton);
                    Button loadButton = new Button();
                    loadButton.Text = "Загрузить карту";
                    loadButton.Location = new Point(0, 130);
                    loadButton.Size = new Size(150, 50);
                    loadButton.Click += (s, ev) =>
                    {
                        var loaded = MapSaver.LoadMap(ref tasktext, task_ogranich, out string? loadedFilePath);
                        if (loaded != null)
                        {
                            mapObjects = loaded;
                            currentMapFilePath = loadedFilePath;
                            renderwindow();
                        }
                    };
                    menuForm.Controls.Add(loadButton);
                    menuForm.ShowDialog();
                    return;
                }
            }
            Objectinfo objectInfoForm = new Objectinfo();
            objectInfoForm.renderwindow = () => this.renderwindow();
            base.OnMouseClick(e);
            Mapobject clickedObject = mouseHandler.GetObjectAtPosition(mapObjects, e.Location, this, plotsize, cameraposition, redactormode);
            if (clickedObject != null)
            {
                objectInfoForm.GetObjectInfo(clickedObject, redactormode);
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isDragging = true;
                lastMousePos = e.Location;
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var delta = new Point(e.X - lastMousePos.X, e.Y - lastMousePos.Y);
                cameraposition += new Vector2(delta.X, delta.Y);
                lastMousePos = e.Location;
                renderwindow();
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                isDragging = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            int oldSize = plotsize;
            int change = e.Delta / 120;
            plotsize = Math.Max(1, plotsize + change);
            if (plotsize != oldSize)
            {
                float scale = (float)plotsize / oldSize;
                Vector2 center = new Vector2(ClientSize.Width / 2f, ClientSize.Height / 2f);
                Vector2 mousePos = new Vector2(e.Location.X, e.Location.Y);
                Vector2 offset = mousePos - center - cameraposition;
                cameraposition = mousePos - center - offset * scale;
                renderwindow();
            }
        }
    }
}



