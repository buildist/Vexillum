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
    class ErrorDialog : WindowControl
    {
        private LabelControl errorLabel;
        private ButtonControl okButton;
        public ErrorDialog(string message)
        {
            int width = (int) TextRenderer.MeasureString(TextRenderer.DefaultFont, message).X;
            int frameWidth = width + 20;

            errorLabel = new LabelControl();
            okButton = new ButtonControl();
         
            errorLabel.Text = message;
            errorLabel.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -width/2), 25), new UniVector(width, 24));

            okButton.Text = "OK";
            okButton.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -40f), 50), new UniVector(80, 24));
            
            Children.Add(errorLabel);
            Children.Add(okButton);

            Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -frameWidth/2), new UniScalar(0.5f, -80f)), new UniVector(frameWidth, 80));

            okButton.Pressed += new EventHandler(close);
        }
        private void close(object sender, EventArgs arguments)
        {
            Close();
        }
    }
}
