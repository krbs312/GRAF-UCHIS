

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using DiplomApp;

namespace DiplomApp
{
    public class Objectinfo
    {
        public Action renderwindow = () => { };
        public void GetObjectInfo(Mapobject clickedObject, bool redactormode)
        {
            Form infoForm = new Form();
            infoForm.Text = "Информация об объекте";
            infoForm.Size = new System.Drawing.Size(380, 360);

            Label titleLabel = new Label
            {
                Text = $"Объект: {clickedObject.name} ({clickedObject.GetType().Name})",
                Location = new Point(10, 10),
                Size = new Size(340, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            infoForm.Controls.Add(titleLabel);

            Label infoLabel = new Label();
            infoLabel.Text = $"Позиция: {clickedObject.position}\nРазмер: {clickedObject.size}\nЦвет: {clickedObject.color}";
            infoLabel.Location = new Point(10, 40);
            infoLabel.Size = new Size(340, 60);
            infoLabel.AutoSize = false;
            infoForm.Controls.Add(infoLabel);

            Label scriptLabel = new Label
            {
                Text = "Скрипт:",
                Location = new Point(10, 105),
                Size = new Size(340, 20)
            };
            infoForm.Controls.Add(scriptLabel);

            TextBox scriptTextBox = new TextBox()
            {
                Location = new Point(10, 130),
                Size = new Size(340, 160),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Text = clickedObject.script
            };
            infoForm.Controls.Add(scriptTextBox);

            Button saveButton = new Button() { Text = "Сохранить скрипт", Location = new Point(10, 295), Size = new Size(120, 30) };
            saveButton.Click += (sender, e) =>
            {
                if (scriptTextBox != null)
                {
                    clickedObject.script = scriptTextBox.Text;
                    infoForm.Close();
                }
            };
            infoForm.Controls.Add(saveButton);

            Button destroyButton = new Button();
            destroyButton.Click += (sender, e) =>
            {
                List<Mapobject> mapObjects = ((MainForm)Application.OpenForms[0]).GetMapobjects();
                mapObjects.Remove(clickedObject);
                renderwindow();
                infoForm.Close();
            };
            destroyButton.Text = "Удалить объект";
            destroyButton.Location = new Point(230, 295);
            destroyButton.Size = new Size(120, 30);
            if (redactormode)
            {
                infoForm.Controls.Add(destroyButton);
            }

            infoForm.ShowDialog();

        }
    }
}

