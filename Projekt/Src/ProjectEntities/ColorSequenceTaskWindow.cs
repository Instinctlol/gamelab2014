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

        //NICHTS AENDERN!!!
        private string blue = "blue";
        private string red = "red";
        private string yellow = "yellow";
        private string green = "green";
        private string[] colors = {"blue", "red", "yellow", "green"};
        private Button greenButton, redButton, yellowButton, blueButton, playButton;
        private static Timer aTimer;
        private Random rnd = new Random();
        private int solvedCount = 0;    //zaehlt wie oft bereits geloest wurde
        private int currPlayerResultPos = 0;    //pusht die vom Spieler gewaehlte Farbe an die richtige Stelle seines Loesungsarrays
        private int currLightButton = 0; //Laufzaehler, der alle solution Farben durchgehen soll
        private bool play = false;  //Indikator, prueft ob Sequenz abgespielt wird
        private bool buttonLighted = false; //Indikator, prueft ob ein Button beleuchtet ist
        private string[] solution;  //enthaelt die Loesung, von der Form z.B. : {"blue","red","yellow",...}
        private List<string> playerResultList=new List<string>(); //enthaelt die Spielereingaben
        
        

        //KANN GEAENDERT WERDEN:
        private static float initialTime = 500; //Zeit zwischen jeweiligem highlighten der Buttons, 1000=1sek
        private int solvedMax = 5; //Wie oft der Spieler abgefragt werden soll
        private int colorsToAdd = 2; // Wie viele Farben hinzukommen sollen
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



            /* Altes Testzeug, arbeitet mit TaskData, also nicht randomisiert
            string taskData = task.Terminal.TaskData;
            if (IsLegit(taskData))
                solution = TaskDataToArray(taskData);
            //else
                //throw new Exception("Your given TaskDataText is not matching the standard: \"color,color,...,color\"");
             * */

            createSolution(solutionStartColors);
        }

        //"Spielt" alle Farben in Solution ab, abgerufen von Play_click(Button sender)
        private void playButtons(object sender, ElapsedEventArgs e)
        {
            if(play)
            {
                //Wenn zaehler nicht out of bounds und kein Button beleuchtet -> beleuchte den Button
                if(currLightButton<=solution.Length && !buttonLighted)
                {
                    lightButton(solution[currLightButton], true);
                    buttonLighted = true;
                }
                else if(currLightButton <=solution.Length && buttonLighted) //sonst beleuchte ihn nicht mehr und gehe zum naechsten
                {
                    lightButton(solution[currLightButton], false);
                    buttonLighted = false;
                    if (currLightButton == solution.Length-1)
                    {
                        //letzter Button -> nicht mehr spielen, zaehler wieder auf 0 setzen, timer ausstellen
                        play = false;
                        currLightButton = 0;

                        aTimer.Enabled = false;
                    }
                    else
                    {
                        //nicht letzter Button -> naechsten Button setzen
                        currLightButton++;
                    }
                }
            }
        }

        //Spieler sagt: spiel mir alle Farben vor, so dass ich die Loesung sehen kann
        private void Play_click(Button sender)
        {
            play = true;
            playButton.Visible = false;
            playButton.Enable = false;
            aTimer = new System.Timers.Timer(initialTime);
            aTimer.Elapsed += playButtons;  //playButtons wird jedes mal ausgefuehrt, wenn Timer initialTime erreicht hat
            aTimer.Enabled = true;          //timer startet
        }

        //Farbbuttons fuegen Eingabe in die Spielereingabe zu und pruefen, falls SpielerEingabe gleich groß ist wie solution, ob richtig eingegeben wurde
        private void Blue_click(Button sender)
        {
            if (currPlayerResultPos < solution.Length)
            {
                playerResultList.Add(blue);
                if (++currPlayerResultPos == solution.Length)
                {
                    checkSolution();
                }
            }
            else //Hier sollte es nie hingehen
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

        private void Red_click(Button sender)
        {
            if (currPlayerResultPos < solution.Length)
            {
                playerResultList.Add(red);
                if (++currPlayerResultPos == solution.Length)
                {
                    checkSolution();
                }
            }
            else //Hier sollte es nie hingehen
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

        private void Yellow_click(Button sender)
        {
            if (currPlayerResultPos < solution.Length)
            {
                playerResultList.Add(yellow);
                if (++currPlayerResultPos == solution.Length)
                {
                    checkSolution();
                }
            }
            else //Hier sollte es nie hingehen
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

        private void Green_click(Button sender)
        {
            if(currPlayerResultPos < solution.Length)
            {
                playerResultList.Add(green);
                if (++currPlayerResultPos == solution.Length)
                {
                    checkSolution();
                }
            }
            else //Hier sollte es nie hingehen
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

        //Beleuchtet einen Button, falls lighted=true, sonst wird er nicht mehr beleuchtet
        private void lightButton(String color, bool lighted)
        {
            switch(color)
            {
                case "green":
                    if (lighted)
                        greenButton.Active = true;
                    else
                        greenButton.Active = false;
                    
                    break;
                case "blue":
                    if(lighted)
                        blueButton.Active = true;
                    else
                        blueButton.Active = false;
                    break;
                case "yellow":
                    if(lighted)
                        yellowButton.Active = true;
                    else
                        yellowButton.Active = false;
                    break;
                case "red":
                    if (lighted)
                        redButton.Active = true;
                    else
                        redButton.Active = false;
                    break;
                default:
                    console.Print("Something went wrong lighting buttons...");
                    break;
            }
        }

        //Erstellt eine zufaellige solution, uebernimmt - falls vorhanden - die alte
        private void createSolution(int length)
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
                        solution[i] = colors[rnd.Next(colors.Length)];
                }
            }
            else
            {
                solution = new string[length];

                for(int i=0; i< solution.Length;i++)
                {
                    solution[i] = colors[rnd.Next(colors.Length)];
                }
            }
        }

        //was gemacht werden soll, wenn eine Spielereingabe richtig oder falsch ist
        private void checkSolution()
        {
            if(compareSolutions())
            {
                ++solvedCount;
                if (solvedCount == solvedMax)   //wenn limit erreicht
                {
                    playerResultList = new List<string>(); //nicht sicher ob das hier noetig ist,
                    currPlayerResultPos = 0;               //besser drinlassen

                    task.Success = true;
                }
                    
                else //neue solution erstellen, playButton zeigen und playerResult loeschen
                {
                    createSolution(solution.Length + colorsToAdd);
                    playButton.Enable = true;
                    playButton.Visible = true;

                    playerResultList=new List<string>();
                    currPlayerResultPos = 0;

                    //TODO: Anzeige des Erfolgs oder anderer Indikator..
                }
            }
            else
            {
                task.Success = false;
            }
        }

        //Vergleicht SpielerEingabe mit solution
        private bool compareSolutions()
        {
            bool check=false;
            string[] playerResult;
            playerResult=playerResultList.ToArray();

            if (solution.Length != playerResult.Length)
                return false;
            else
            {
                for (int i = 0; i < solution.Length; i++ )
                {
                    if (String.Compare(solution[i], playerResult[i], true)==0)
                    {
                        check = true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return check;
        }

        //Altes Testzeug, arbeitet mit TaskData, also nicht randomisiert
        private string[] TaskDataToArray(string tskdt)
        {
            string[] rtrn = tskdt.Split(',');
            return rtrn;
        }

        //Altes Testzeug, arbeitet mit TaskData, also nicht randomisiert
        private bool IsLegit(string input)
        {
            int[] indexes = CharPos(input, ',');
            bool check=false;

            int pos = 0;

            for (int i = 0; i < indexes.Length; i++)
            {
                if (input.Substring(pos, indexes[i]-1) == blue || input.Substring(pos, indexes[i]-1) == red ||
                    input.Substring(pos, indexes[i]-1) == yellow || input.Substring(pos, indexes[i]-1) == green)
                {
                    check = true;
                    pos = indexes[i] + 1;
                }
                else
                    return false;
            }
            if (input.Substring(pos, input.Length - 1) == blue || input.Substring(pos, input.Length - 1) == red ||
                    input.Substring(pos, input.Length - 1) == yellow || input.Substring(pos, input.Length - 1) == green)
            {
                check = true;
            }
            else
            {
                return false;
            }

            return check;
        }

        //Altes Testzeug, arbeitet mit TaskData, also nicht randomisiert
        private int[] CharPos(string Input, char C)
        {
            var foundIndexes = new List<int>();

            for (int i = Input.IndexOf(C); i > -1; i = Input.IndexOf(C, i + 1))
            {
                // for loop end when i=-1 ('a' not found)
                foundIndexes.Add(i);
            }

            return foundIndexes.ToArray();
        }
    }
}

