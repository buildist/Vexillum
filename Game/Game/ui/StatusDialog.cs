using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;
using Vexillum.util;

namespace Vexillum.ui
{
    public class StatusDialog : WindowControl
    {
        private LabelControl errorLabel;
        private ButtonControl okButton;
        public StatusDialog(string message)
        {
            errorLabel = new LabelControl();
            okButton = new ButtonControl();

            okButton.Text = "Cancel";
            okButton.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -40f), 50), new UniVector(80, 24));
            
            Children.Add(errorLabel);
            Children.Add(okButton);

            Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -200), new UniScalar(0.5f, -80f)), new UniVector(400, 80));

            okButton.Pressed += new EventHandler(close);

            SetMessage(message);
        }
        public void SetCancelAction(EventHandler action)
        {
            okButton.Pressed += action;
        }
        public void SetMessage(String value)
        {
            int width = (int)TextRenderer.MeasureString(TextRenderer.TitleFont, value).X;
            errorLabel.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -width / 2), 25), new UniVector(new UniScalar(width, 0), 24));
            errorLabel.Text = value;
        }
        private void close(object sender, EventArgs arguments)
        {
            Close();
        }
    }
}
