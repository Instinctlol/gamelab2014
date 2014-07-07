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
        enum NetworkMessages
        {
            Client_SchereButtonClicked,
            Client_PapierButtonClicked,
            Client_SteinButtonClicked,
            Client_PlayButtonClicked,   //not for this class
            Server_UpdateTimer,
            Server_ClientLoses,
            Server_ClientWins,
            Server_Draw,
            Server_EvaluatingSolutions
        }

        private EngineConsole console = EngineConsole.Instance;

        private Control window;
        private TextBox countdownBox, enemySelectedBox;
        private Button schereButton, steinButton, papierButton;
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

            if (task.IsServer)
            {
                task.Server_WindowDataReceived += Server_WindowDataReceived;
                aTimer = new System.Timers.Timer(1000); //jede Sekunde
                aTimer.Elapsed += countdown;
            }
                
        }

        private void countdown(object sender, ElapsedEventArgs e)
        {
            countdownBox.Text = ""+(int.Parse(countdownBox.Text) -1);
            task.Server_SendWindowString(countdownBox.Text, (UInt16)NetworkMessages.Server_UpdateTimer);
            if (countdownBox.Text.Equals("0"))
                compareSolutions();
        }   
        

        private void Stein_clicked(Button sender)
        {
            if(!(countdownBox.Text.Equals("0")))
                lastSelected = steinButton.Text;
        }

        private void Papier_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
                lastSelected = steinButton.Text;
        }

        private void Schere_clicked(Button sender)
        {
            if (!(countdownBox.Text.Equals("0")))
                lastSelected = steinButton.Text;
        }

        private void compareSolutions()
        {
            task.Server_SendWindowData((UInt16)NetworkMessages.Server_EvaluatingSolutions);

            schereButton.Enable = false;
            papierButton.Enable = false;
            steinButton.Enable = false;

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
            task.Server_SendWindowData((UInt16)NetworkMessages.Server_ClientLoses);
        }

        private void drawStuff()
        {
            task.Server_SendWindowData((UInt16)NetworkMessages.Server_Draw);
        }

        private void defeatStuff()
        {
            task.Server_SendWindowData((UInt16)NetworkMessages.Server_ClientWins);
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if(!task.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch(msg)
                {
                    case NetworkMessages.Client_PapierButtonClicked:
                        enemyLastSelected = papierButton.Text;
                        break;
                    case NetworkMessages.Client_SchereButtonClicked:
                        enemyLastSelected = schereButton.Text;
                        break;
                    case NetworkMessages.Client_SteinButtonClicked:
                        enemyLastSelected = steinButton.Text;
                        break;
                }
            }
        }
    }
}
