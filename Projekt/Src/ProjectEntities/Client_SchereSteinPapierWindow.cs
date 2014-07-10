using Engine;
using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ProjectEntities
{
    public class Client_SchereSteinPapierWindow : TaskWindow
    {
        public enum NetworkMessages
        {
            Client_SchereButtonClicked,
            Client_PapierButtonClicked,
            Client_SteinButtonClicked,
            Client_PlayButtonClicked,   //create window for alien
            Server_UpdateTimer,
            Server_ClientLoses,
            Server_ClientWins,
            Server_Draw,
            Server_EvaluatingSolutions,  //disable Buttons
        }

        private EngineConsole console = EngineConsole.Instance;

        

        private Control window;
        private TextBox countdownBox, enemySelectedBox;
        private Button schereButton, steinButton, papierButton, playButton, tempButton;
        private string lastSelected;

        public Client_SchereSteinPapierWindow(Task task) : base(task)
        {
            window = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\SchereSteinPapier.gui");
            Controls.Add(window);

            schereButton = (Button)window.Controls["SchereButton"];
            steinButton = (Button)window.Controls["SteinButton"];
            papierButton = (Button)window.Controls["PapierButton"];
            playButton = (Button)window.Controls["PlayButton"];
            enemySelectedBox = (TextBox)window.Controls["GegnerWahl"];
            countdownBox = (TextBox)window.Controls["Countdown"];

            schereButton.Click += Schere_clicked;
            papierButton.Click += Papier_clicked;
            steinButton.Click += Stein_clicked;
            playButton.Click += Play_clicked;

            enemySelectedBox.Visible = false;


            if (!task.IsServer)
            {
                task.Client_WindowStringReceived += Client_StringReceived;
                task.Client_WindowDataReceived += Client_WindowDataReceived;
            }
                
        }

        

        private void Play_clicked(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.Client_PlayButtonClicked);
            
            playButton.Enable = false;
            playButton.Visible = false;

            schereButton.Enable = true;
            steinButton.Enable = true;
            papierButton.Enable = true;
        }

        

        private void Stein_clicked(Button sender)
        {
            lastSelected = steinButton.Text;
            if(!(tempButton==null))
            {
                tempButton.DefaultControl.BackColor = new ColorValue(74/255, 154/255, 221/255, 200/255);
            }
            steinButton.DefaultControl.BackColor = new ColorValue(181/255, 215/255, 255/255, 200/255);
            tempButton = steinButton;
            task.Client_SendWindowData((UInt16)NetworkMessages.Client_SteinButtonClicked);
        }

        private void Papier_clicked(Button sender)
        {
            lastSelected = papierButton.Text;
            if (!(tempButton == null))
            {
                tempButton.DefaultControl.BackColor = new ColorValue(74 / 255, 154 / 255, 221 / 255, 200 / 255);
            }
            papierButton.DefaultControl.BackColor = new ColorValue(181 / 255, 215 / 255, 255 / 255, 200 / 255);
            tempButton = papierButton;
            task.Client_SendWindowData((UInt16)NetworkMessages.Client_PapierButtonClicked);
        }

        private void Schere_clicked(Button sender)
        {
            lastSelected = schereButton.Text;
            if (!(tempButton == null))
            {
                tempButton.DefaultControl.BackColor = new ColorValue(74 / 255, 154 / 255, 221 / 255, 200 / 255);
            }
            schereButton.DefaultControl.BackColor = new ColorValue(181 / 255, 215 / 255, 255 / 255, 200 / 255);
            tempButton = schereButton;
            task.Client_SendWindowData((UInt16)NetworkMessages.Client_SchereButtonClicked);
        }

        private void victoryStuff()
        {
            switch(lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }

            task.Success = true;
        }

        private void drawStuff()
        {
            switch (lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }
        }

        private void defeatStuff()
        {
            switch (lastSelected)
            {
                case "Schere":
                    enemySelectedBox.Text = steinButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Papier":
                    enemySelectedBox.Text = schereButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
                case "Stein":
                    enemySelectedBox.Text = papierButton.Text;
                    enemySelectedBox.Visible = true;
                    break;
            }
        }

        private void Client_StringReceived(string message, UInt16 netMessage)
        {
            if (!task.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)netMessage;
                switch (msg)
                {
                    case NetworkMessages.Server_UpdateTimer:
                        countdownBox.Text = message;
                        break;

                }
            }
        }

        

        private void Client_WindowDataReceived(ushort message)
        {
            if(!task.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch(msg)
                {
                    case NetworkMessages.Server_Draw:
                        drawStuff();
                        break;
                    case NetworkMessages.Server_ClientWins:
                        victoryStuff();
                        break;
                    case NetworkMessages.Server_ClientLoses:
                        defeatStuff();
                        break;
                    case NetworkMessages.Server_EvaluatingSolutions:
                        schereButton.Enable = false;
                        steinButton.Enable = false;
                        papierButton.Enable = false;
                        break;
                }
            }
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if(task.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch(msg)
                {
                    case NetworkMessages.Client_PlayButtonClicked:
                        Server_createWindowForAlien(task);
                        break;
                }
            }
        }
    }
}
