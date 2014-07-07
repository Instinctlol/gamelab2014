using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Timers;
namespace ProjectEntities
{
    public class Server_SchereSteinPapierWindow : TaskWindow
    {
        private EngineConsole console = EngineConsole.Instance;

        private Control window;
        private TextBox countdownBox, enemySelectedBox;
        private Button schereButton, steinButton, papierButton, tempButton;
        private string lastSelected, enemyLastSelected;
        private Timer aTimer;

        public Server_SchereSteinPapierWindow(Task task) : base(task)
        {
            window = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\SchereSteinPapier.gui");
            Controls.Add(window);

            schereButton = (Button)window.Controls["SchereButton"];
            steinButton = (Button)window.Controls["SteinButton"];
            papierButton = (Button)window.Controls["PapierButton"];
            enemySelectedBox = (TextBox)window.Controls["GegnerWahl"];
            countdownBox = (TextBox)window.Controls["Countdown"];

            schereButton.Click += Schere_clicked;
            papierButton.Click += Papier_clicked;
            steinButton.Click += Stein_clicked;

            schereButton.Enable = true;
            steinButton.Enable = true;
            papierButton.Enable = true;
            enemySelectedBox.Visible = false;
            ((Button)window.Controls["PlayButton"]).Visible = false;

            if (task.IsServer)
            {
                task.Server_WindowDataReceived += Server_WindowDataReceived;
                aTimer = new System.Timers.Timer(1000); //jede Sekunde
                aTimer.Elapsed += countdown;
                aTimer.Enabled = true;
            }
                
        }

        private void countdown(object sender, ElapsedEventArgs e)
        {
            countdownBox.Text = ""+(int.Parse(countdownBox.Text) -1);
            task.Server_SendWindowString(countdownBox.Text, (UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_UpdateTimer);
            if (countdownBox.Text.Equals("0"))
            {
                aTimer.Enabled = false;
                compareSolutions();
            }
                
        }   
        

        private void Stein_clicked(Button sender)
        {
            if(!(countdownBox.Text.Equals("0")))
            {
                lastSelected = steinButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.DefaultControl.BackColor = new ColorValue(74 / 255, 154 / 255, 221 / 255, 200 / 255);
                }
                steinButton.DefaultControl.BackColor = new ColorValue(181 / 255, 215 / 255, 255 / 255, 200 / 255);
                tempButton = steinButton;
            }
                
        }

        private void Papier_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
            {
                lastSelected = papierButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.DefaultControl.BackColor = new ColorValue(74 / 255, 154 / 255, 221 / 255, 200 / 255);
                }
                papierButton.DefaultControl.BackColor = new ColorValue(181 / 255, 215 / 255, 255 / 255, 200 / 255);
                tempButton = papierButton;
            }
                
        }

        private void Schere_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
            {
                lastSelected = schereButton.Text;
                if (!(tempButton == null))
                {
                    tempButton.DefaultControl.BackColor = new ColorValue(74 / 255, 154 / 255, 221 / 255, 200 / 255);
                }
                schereButton.DefaultControl.BackColor = new ColorValue(181 / 255, 215 / 255, 255 / 255, 200 / 255);
                tempButton = schereButton;
            }
        }

        private void compareSolutions()
        {
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_EvaluatingSolutions);

            schereButton.Enable = false;
            papierButton.Enable = false;
            steinButton.Enable = false;
            enemySelectedBox.Text = enemyLastSelected;
            enemySelectedBox.Visible = true;

            switch(lastSelected)
            {
                case "Schere":
                    if (enemyLastSelected.Equals("Stein"))
                        defeatStuff();
                    if (enemyLastSelected.Equals("Schere"))
                        drawStuff();
                    if (enemyLastSelected.Equals("Papier"))
                        victoryStuff();
                    break;
                case "Papier":
                    if (enemyLastSelected.Equals("Schere"))
                        defeatStuff();
                    if (enemyLastSelected.Equals("Papier"))
                        drawStuff();
                    if (enemyLastSelected.Equals("Stein"))
                        victoryStuff();
                    break;
                case "Stein":
                    if (enemyLastSelected.Equals("Papier"))
                        defeatStuff();
                    if (enemyLastSelected.Equals("Stein"))
                        drawStuff();
                    if (enemyLastSelected.Equals("Schere"))
                        victoryStuff();
                    break;
            }
                
        }

        private void victoryStuff()
        {
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_ClientLoses);
        }

        private void drawStuff()
        {
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_Draw);
        }

        private void defeatStuff()
        {
            task.Server_SendWindowData((UInt16)Client_SchereSteinPapierWindow.NetworkMessages.Server_ClientWins);
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if(!task.IsServer)
            {
                Client_SchereSteinPapierWindow.NetworkMessages msg = (Client_SchereSteinPapierWindow.NetworkMessages)message;
                switch(msg)
                {
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_PapierButtonClicked:
                        enemyLastSelected = papierButton.Text;
                        break;
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_SchereButtonClicked:
                        enemyLastSelected = schereButton.Text;
                        break;
                    case Client_SchereSteinPapierWindow.NetworkMessages.Client_SteinButtonClicked:
                        enemyLastSelected = steinButton.Text;
                        break;
                }
            }
        }
    }
}
