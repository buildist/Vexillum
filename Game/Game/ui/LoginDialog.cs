using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nuclex.UserInterface.Controls.Desktop;
using Nuclex.UserInterface.Controls;
using Nuclex.UserInterface;
using Vexillum.util;
using System.Threading;
using Vexillum.game;

namespace Vexillum.ui
{
    class LoginDialog : WindowControl
    {
        private LabelControl usernameLabel;
        private CustomInputControl usernameBox;
        private LabelControl passwordLabel;
        private CustomInputControl passwordBox;
        private ButtonControl loginButton;
        private LabelControl instructionsLabel;
        private LabelControl rememberLabel;
        private OptionControl remember;
        public LoginDialog()
        {
            usernameLabel = new LabelControl();
            usernameBox = new CustomInputControl();

            passwordLabel = new LabelControl();
            passwordBox = new CustomInputControl();
            passwordBox.password = true;
            passwordBox.next = usernameBox;
            usernameBox.next = passwordBox;

            loginButton = new ButtonControl();
            instructionsLabel = new LabelControl();
            rememberLabel = new LabelControl();
            remember = new OptionControl();

            usernameLabel.Text = "Username";
            passwordLabel.Text = "Password";
            usernameBox.Text = Settings.Get(SettingType.Username).ToString();
            passwordBox.Text = Settings.Get(SettingType.Password).ToString();
            if (usernameBox.Text.Length > 0)
                remember.Selected = true;
            loginButton.Text = "Log in";
            instructionsLabel.Text = "The password is\nonly necessary if\nyou've registered\nyour username.";
            rememberLabel.Text = "Remember";

            usernameLabel.Bounds = new UniRectangle(10, 30, 70, 24);
            passwordLabel.Bounds = new UniRectangle(10, 60, 70, 24);
            usernameBox.Bounds = new UniRectangle(80, 30, 160, 24);
            passwordBox.Bounds = new UniRectangle(80, 60, 160, 24);
            loginButton.Bounds = new UniRectangle(370 - 10 - 80, 130 - 24 - 10, 80, 24);
            instructionsLabel.Bounds = new UniRectangle(250, 10 - 20, 100, 80);
            rememberLabel.Bounds = new UniRectangle(10, 90, 70, 24);
            remember.Bounds = new UniRectangle(80, 94, 16, 16);
            this.Bounds = new UniRectangle(new UniVector(new UniScalar(0.5f, -185), new UniScalar(0.5f, -130f)), new UniVector(370, 130));

            this.Title = "Log in";

            loginButton.Pressed += DoLogin;
            usernameBox.Enter = passwordBox.Enter = DoLogin;

            Children.Add(usernameLabel);
            Children.Add(passwordLabel);
            Children.Add(usernameBox);
            Children.Add(passwordBox);
            Children.Add(loginButton);
            Children.Add(instructionsLabel);
            Children.Add(rememberLabel);
            Children.Add(remember);

        }
        private void DoLogin(object sender, EventArgs arguments)
        {
            string username = usernameBox.Text;
            string password = passwordBox.Text;
            if (username == "")
            {
                Vexillum.game.MessageBox("Please enter a username.");
                return;
            }
            else if (!Util.ValidateUsername(username))
            {
                Vexillum.game.MessageBox("Your username contains weird characters! Please use only letters, numbers, and underscores (_).");
                return;
            }
            Settings.Set(SettingType.Username, username);
            if (remember.Selected)
            {
                Settings.Set(SettingType.Password, password);
            }
            else
            {
                Settings.Set(SettingType.Password, "");
            }
            WindowControl status = Vexillum.game.StatusBox("Logging in...");
            new Thread(delegate()
                {
                    Identity.username = username;
                    try
                    {
                        string[] parts = Util.HttpGet("login.php?user=" + username + "&pass=" + Uri.EscapeUriString(password)).Split('|');
                        //Identity.uid = int.Parse(parts[0]);
                        //Identity.key = parts[1];
                        if (parts[1] == "0")
                        {
                            Vexillum.game.MessageBox("This name is registered; please enter the correct password.");
                            status.Close();
                        }
                        else
                        {
                            status.Close();
                            Close();
                            Vexillum.game.SetMenuView(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        status.Close();
                        Close();
                        Vexillum.game.SetMenuView(true);
                        Vexillum.game.MessageBox("Problem contacting the master server. You'll be logged in as a guest.");
                        //entity.uid = 0;
                        //Identity.key = "guest";
                        Vexillum.game.SetMenuView(true);
                    }
                }).Start();
        }
    }
}
