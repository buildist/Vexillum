using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface;
using Nuclex.UserInterface.Controls;
using Microsoft.Xna.Framework.Input;

namespace Vexillum.ui
{
    class OptionsDialog : WindowControl
    {
        private ButtonControl okButton;
        private ButtonControl cancelButton;
        private LabelControl controlsLabel;
        private Dictionary<KeySelectorControl, KeyAction> selectors;
        public OptionsDialog()
        {
            Title = "Options";

            controlsLabel = new LabelControl();
            controlsLabel.Bounds = new UniRectangle(10, 34, 200, 24);
            Children.Add(controlsLabel);

            Dictionary<Keys, KeyAction> controls = ControlSystem.GetAllControls();
            selectors = new Dictionary<KeySelectorControl, KeyAction>();
            int y = 40;
            for (int i = 0; i < controls.Count; i++)
            {
                LabelControl controlLabel = new LabelControl();
                controlLabel.Bounds = new UniRectangle(10, y, 200, 24);
                controlLabel.Text = controls.Values.ElementAt(i).ToString().Replace('_', ' ');
                Children.Add(controlLabel);

                KeySelectorControl input = new KeySelectorControl();
                selectors[input] = controls.Values.ElementAt(i);
                input.Bounds = new UniRectangle(210, y, 50, 24);
                input.Text = controls.Keys.ElementAt(i).ToString();
                input.Key = controls.Keys.ElementAt(i);
                Children.Add(input);
                y += 30;
            }

            okButton = new ButtonControl();
            okButton.Text = "OK";
            okButton.Bounds = new UniRectangle(new UniVector(new UniScalar(1f, -190), new UniScalar(1f, -37)), new UniVector(90, 24));
            okButton.Pressed += Save;

            cancelButton = new ButtonControl();
            cancelButton.Text = "Cancel";
            cancelButton.Bounds = new UniRectangle(new UniVector(new UniScalar(1f, -90), new UniScalar(1f, -37)), new UniVector(80, 24));
            cancelButton.Pressed += Close;

            Children.Add(okButton);
            Children.Add(cancelButton);

            Bounds = new UniRectangle(new UniVector(420, 20), new UniVector(275, 380));
        }
        private void Close(object source, EventArgs args)
        {
            Close();
        }
        private void Save(object source, EventArgs args)
        {
            List<Keys> keys = new List<Keys>(selectors.Count);
            foreach(KeySelectorControl s in selectors.Keys)
            {
                if (keys.Contains(s.Key))
                {
                    Vexillum.game.MessageBox("The same key may not be assigned to multiple controls.");
                    return;
                }
                else
                    keys.Add(s.Key);
            }
            ControlSystem.ClearControls();
            for (int i = 0; i < selectors.Count; i++)
            {
                Keys key = selectors.Keys.ElementAt(i).Key;
                ControlSystem.SetControl(key, selectors.Values.ElementAt(i));
            }
            ControlSystem.SaveControls();
            Close();
        }
    }
}
