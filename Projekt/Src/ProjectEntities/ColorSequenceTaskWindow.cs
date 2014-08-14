using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ProjectEntities
{
    public class ColorSequenceTaskWindow : TaskWindow
    {
        private EngineConsole console = EngineConsole.Instance;

        enum NetworkMessages
        {
            LightYellowToClient,
            LightBlueToClient,
            LightGreenToClient,
            LightRedToClient,
            ShowPlayButtonToClient,
            HidePlayButtonToClient,
            UnlightButtonsToClient,
            YellowClickToClient,
            BlueClickToClient,
            GreenClickToClient,
            RedClickToClient,
            YellowClickToServer,
            BlueClickToServer,
            GreenClickToServer,
            RedClickToServer,
            PlayClickToServer,
            ShowMouseOverControls,
        }
        
        //Konstanten
        private const string BLUE = "blue";
        private const string RED = "red";
        private const string YELLOW = "yellow";
        private const string GREEN = "green";
        private string[] COLOURS = {BLUE, RED, YELLOW, GREEN};



        private Button greenButton, redButton, yellowButton, blueButton, playButton;
        private Engine.MathEx.ColorValue yellowOVC, greenOVC, redOVC, blueOVC, yellowPSC, greenPSC, redPSC, bluePSC;
        private static Timer aTimer;
        private Random rnd = new Random();
        private int solvedCount = 0;    //zaehlt wie oft bereits geloest wurde
        private int currPlayerResultPos = -1;    //pusht die vom Spieler gewaehlte Farbe an die richtige Stelle seines Loesungsarrays
        private int currLightButton = 0; //Laufzaehler, der alle solution Farben durchgehen soll
        private bool play = false;  //Indikator, prueft ob Sequenz abgespielt wird
        private bool buttonLighted = false; //Indikator, prueft ob ein Button beleuchtet ist
        private string[] solution;  //enthaelt die Loesung, von der Form z.B. : {"blue","red","yellow",...       
        

        //KANN GEAENDERT WERDEN:
        private static float initialTime = 200; //Zeit zwischen jeweiligem highlighten der Buttons, 1000=1sek
        private int solvedMax = 3; //Wie oft der Spieler abgefragt werden soll
        private int colorsToAdd = 1; // Wie viele Farben hinzukommen sollen
        private int solutionStartColors = 4; //Mit wie vielen Farben gestartet wird
        


        public ColorSequenceTaskWindow(Task task) : base(task)
        {
            //GUI Erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\ColorSequenceGUI.gui");

            //Methoden für Buttons
            ((Button)CurWindow.Controls["Green"]).Click += Green_click;
            ((Button)CurWindow.Controls["Yellow"]).Click += Yellow_click;
            ((Button)CurWindow.Controls["Red"]).Click += Red_click;
            ((Button)CurWindow.Controls["Blue"]).Click += Blue_click;
            ((Button)CurWindow.Controls["Play"]).Click += Play_click;

            greenButton = ((Button)CurWindow.Controls["Green"]);
            yellowButton = ((Button)CurWindow.Controls["Yellow"]);
            redButton = ((Button)CurWindow.Controls["Red"]);
            blueButton = ((Button)CurWindow.Controls["Blue"]);
            playButton = ((Button)CurWindow.Controls["Play"]);

            greenOVC = greenButton.OverControl.BackColor;
            yellowOVC = yellowButton.OverControl.BackColor;
            redOVC = redButton.OverControl.BackColor;
            blueOVC = blueButton.OverControl.BackColor;

            greenPSC = greenButton.PushControl.BackColor;
            yellowPSC = yellowButton.PushControl.BackColor;
            redPSC = redButton.PushControl.BackColor;
            bluePSC = blueButton.PushControl.BackColor;

            task.Client_WindowDataReceived += Client_ReceiveClicks;
            task.Client_WindowDataReceived += Client_ReceiveLightButton;
            task.Client_WindowDataReceived += Client_ReceivePlayButtonStatus;

            task.Server_WindowDataReceived += Server_ReceiveClicks;

            if(task.IsServer)
                CreateSolution(solutionStartColors);
        }

        //"Spielt" alle Farben in Solution ab, abgerufen von Play_click(Button sender)
        private void PlayButtons(object sender, ElapsedEventArgs e)
        {
            //Falls er abspielen soll
            if(play)
            {
                //Wenn zaehler nicht out of bounds und kein Button beleuchtet -> beleuchte den Button
                if(currLightButton<=solution.Length)
                {
                    if (!buttonLighted)
                    {
                        LightButton(solution[currLightButton], true);
                        buttonLighted = true;
                    }
                    else
                    {
                        LightButton(solution[currLightButton], false);
                        buttonLighted = false;
                        if (currLightButton == solution.Length-1)
                        {
                            //letzter Button -> nicht mehr spielen, zaehler wieder auf 0 setzen, timer ausstellen
                            play = false;
                            currLightButton = 0;

                            aTimer.Enabled = false;
                            SwitchControlForButton(yellowButton, true);
                            SwitchControlForButton(redButton, true);
                            SwitchControlForButton(blueButton, true);
                            SwitchControlForButton(greenButton, true);
                            task.Server_SendWindowData((UInt16)NetworkMessages.ShowMouseOverControls);
                        }
                        else
                        {
                            //nicht letzter Button -> naechsten Button setzen
                            currLightButton++;
                        }
                    }
                }        
            } 
        }

        //Spieler sagt: spiel mir alle Farben vor, so dass ich die Loesung sehen kann
        private void Play_click(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.PlayClickToServer);
        }

        //Farbbuttons fuegen Eingabe in die Spielereingabe zu und pruefen, falls SpielerEingabe gleich groß ist wie solution, ob richtig eingegeben wurde
        private void Blue_click(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.BlueClickToServer);
        }

        private void Red_click(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.RedClickToServer);
        }

        private void Yellow_click(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.YellowClickToServer);
        }

        private void Green_click(Button sender)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.GreenClickToServer);
        }

        //Zeigt die MouseOverControl des Buttons oder verbirgt sie.
        private void SwitchControlForButton(Button b, bool show)
        {
            if(!show)
            {
                b.OverControl.BackColor = b.DefaultControl.BackColor;
                b.PushControl.BackColor = b.DefaultControl.BackColor;
            }
            else
            {
                switch(b.Name)
                {
                    case "Yellow":
                        {
                            b.OverControl.BackColor = yellowOVC;
                            b.PushControl.BackColor = yellowPSC;
                            break;
                        }
                    case "Red":
                        {
                            b.OverControl.BackColor = redOVC;
                            b.PushControl.BackColor = redPSC;
                            break;
                        }
                    case "Green":
                        {
                            b.OverControl.BackColor = greenOVC;
                            b.PushControl.BackColor = greenPSC;
                            break;
                        }
                    case "Blue":
                        {
                            b.OverControl.BackColor = blueOVC;
                            b.PushControl.BackColor = bluePSC;
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }
            
                
        }

        //Beleuchtet einen Button, falls lighted=true, sonst wird er nicht mehr beleuchtet
        private void LightButton(String color, bool lighted)
        {
            if(lighted)
                switch(color)
                {
                    case "green":
                        //Grün leuchtet
                        greenButton.Active = true;
                        task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\greenSimon.ogg", .5f, false);

                        //Alle anderen aus
                        blueButton.Active = false;
                        yellowButton.Active = false;
                        redButton.Active = false;                      
                    
                        if(task.IsServer)
                            task.Server_SendWindowData((UInt16)NetworkMessages.LightGreenToClient);
                        break;
                    case "blue":
                        //Blau leuchtet
                        blueButton.Active = true;
                        task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\blueSimon.ogg", .5f, false);

                        //Alle anderen aus
                        greenButton.Active = false;
                        yellowButton.Active = false;
                        redButton.Active = false;

                        if (task.IsServer)
                            task.Server_SendWindowData((UInt16)NetworkMessages.LightBlueToClient);
                        break;
                    case "yellow":
                        //Gelb leuchtet
                        yellowButton.Active = true;
                        task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\yellowSimon.ogg", .5f, false);

                        //Alle anderen aus
                        blueButton.Active = false;
                        greenButton.Active = false;
                        redButton.Active = false;

                        if (task.IsServer)
                            task.Server_SendWindowData((UInt16)NetworkMessages.LightYellowToClient);
                        break;
                    case "red":
                        //Rot leuchtet
                        redButton.Active = true;
                        task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\redSimon.ogg", .5f, false);

                        //Alle anderen aus
                        blueButton.Active = false;
                        yellowButton.Active = false;
                        greenButton.Active = false;

                        if (task.IsServer)
                            task.Server_SendWindowData((UInt16)NetworkMessages.LightRedToClient);
                        break;
                    default:
                        console.Print("Something went wrong lighting buttons...");
                        break;
                }
            else
            {
                blueButton.Active = false;
                yellowButton.Active = false;
                greenButton.Active = false;
                redButton.Active = false;

                if (task.IsServer)
                    task.Server_SendWindowData((UInt16)NetworkMessages.UnlightButtonsToClient);
            }
        }

        //Eingabe des spielers überprüfen
        private void CheckInput(string colour, int pos)
        {
            if (solution[pos] != colour)
            {
                //Bei Falscher Eingabe
                task.Success = false;
            }
            else
            {
                //Wenn es letzte Eingabe für momentane Lösung war
                if (solution.Length - 1 == pos)
                {
                    //Wenn noch nicht genug Iterationen gelöst, neue erstellen
                    if (solvedCount < solvedMax)
                    {
                        CreateSolution(solution.Length + colorsToAdd);
                        task.Server_SendWindowData((UInt16)NetworkMessages.ShowPlayButtonToClient);
                        currPlayerResultPos = -1;
                        solvedCount++;
                    }
                    else
                    {
                        task.Success = true;
                    }
                }
            }
        }

        //Erstellt eine zufaellige solution, uebernimmt - falls vorhanden - die alte
        private void CreateSolution(int length)
        {
            //Wenn solution bereits existiert...
            if(!(solution==null || solution.Length==0))
            {
                string[] oldSolution;
                oldSolution = solution;
                solution=new string[length];

                for (int i = 0; i < solution.Length; i++)
                {
                    if (i < oldSolution.Length)
                        solution[i] = oldSolution[i];
                    else
                        solution[i] = COLOURS[rnd.Next(COLOURS.Length)];
                }
            }
            else
            {
                solution = new string[length];

                for(int i=0; i< solution.Length;i++)
                {
                    solution[i] = COLOURS[rnd.Next(COLOURS.Length)];
                }
            }
        }



        private void Server_ReceiveClicks(UInt16 click)
        {
            NetworkMessages msg = (NetworkMessages) click;
            switch (msg)
            {
                case NetworkMessages.BlueClickToServer:
                    if (!play)
                    {
                        currPlayerResultPos++;
                        if (currPlayerResultPos < solution.Length)
                        {
                            CheckInput(BLUE, currPlayerResultPos);
                            task.Server_SendWindowData((UInt16)NetworkMessages.BlueClickToClient);
                        }
                    }
                    break;
                case NetworkMessages.GreenClickToServer:
                    if (!play)
                    {
                        currPlayerResultPos++;
                        if (currPlayerResultPos < solution.Length)
                        {
                            CheckInput(GREEN, currPlayerResultPos);
                            task.Server_SendWindowData((UInt16)NetworkMessages.GreenClickToClient);
                        }
                    }
                    break;
                case NetworkMessages.RedClickToServer:
                    if (!play)
                    {
                        currPlayerResultPos++;
                        if (currPlayerResultPos < solution.Length)
                        {
                            CheckInput(RED, currPlayerResultPos);
                            task.Server_SendWindowData((UInt16)NetworkMessages.RedClickToClient);
                        }
                    }
                    break;
                case NetworkMessages.YellowClickToServer:
                    if (!play)
                    {
                        currPlayerResultPos++;
                        if (currPlayerResultPos < solution.Length)
                        {
                            CheckInput(YELLOW, currPlayerResultPos);
                            task.Server_SendWindowData((UInt16)NetworkMessages.YellowClickToClient);
                        }
                    }
                    break;
                case NetworkMessages.PlayClickToServer:
                    play = true;

                    aTimer = new System.Timers.Timer(initialTime);
                    aTimer.Elapsed += PlayButtons;  //playButtons wird jedes mal ausgefuehrt, wenn Timer initialTime erreicht hat
                    aTimer.Enabled = true;          //timer startet

                    task.Server_SendWindowData((UInt16)NetworkMessages.HidePlayButtonToClient);
                    break;
                default:
                    return;

            }

        }

        private void Client_ReceiveClicks(UInt16 click)
        {
            NetworkMessages msg = (NetworkMessages) click;

            switch (msg)
            {
                case NetworkMessages.YellowClickToClient:
                    task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\yellowSimon.ogg", .5f, false);
                    break;
                case NetworkMessages.BlueClickToClient:
                    task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\blueSimon.ogg", .5f, false);
                    break;
                case NetworkMessages.GreenClickToClient:
                    task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\greenSimon.ogg", .5f, false);
                    break;
                case NetworkMessages.RedClickToClient:
                    task.Terminal.SoundPlay3D("Sounds\\ColorSequenceSounds\\redSimon.ogg", .5f, false);
                    break;
                default:
                    return;

            }
        }

        private void Client_ReceivePlayButtonStatus(UInt16 status)
        {

            NetworkMessages msg = (NetworkMessages)status;

            switch (msg)
            {
                case NetworkMessages.ShowPlayButtonToClient:
                    playButton.Visible = true;
                    playButton.Enable = true;
                    break;
                case NetworkMessages.HidePlayButtonToClient:
                    playButton.Visible = false;
                    playButton.Enable = false;

                    SwitchControlForButton(yellowButton, false);
                    SwitchControlForButton(redButton, false);
                    SwitchControlForButton(blueButton, false);
                    SwitchControlForButton(greenButton, false);
                    break;
                case NetworkMessages.ShowMouseOverControls:
                    SwitchControlForButton(yellowButton, true);
                    SwitchControlForButton(redButton, true);
                    SwitchControlForButton(blueButton, true);
                    SwitchControlForButton(greenButton, true);
                    break;
                default:
                    return;
            }
        }

        private void Client_ReceiveLightButton(UInt16 button)
        {
            NetworkMessages msg = (NetworkMessages)button;
            switch (msg)
            {
                case NetworkMessages.LightYellowToClient:
                    LightButton(YELLOW, true);
                    break;
                case NetworkMessages.LightBlueToClient:
                    LightButton(BLUE, true);
                    break;
                case NetworkMessages.LightGreenToClient:
                    LightButton(GREEN, true);
                    break;
                case NetworkMessages.LightRedToClient:
                    LightButton(RED, true);
                    break;
                case NetworkMessages.UnlightButtonsToClient:
                    LightButton(null, false);
                    break;
                default:
                    return;
            }
        }
    }
}

