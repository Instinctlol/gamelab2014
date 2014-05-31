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

        private string blue = "blue";
        private string red = "red";
        private string yellow = "yellow";
        private string green = "green";

        private string[] solution;
        private string[] playerResult;

        private List<string> playerResultList=new List<string>();

        private int currPlayerResultPos=0;

        private Button greenButton, redButton, yellowButton, blueButton, playButton;

        //Zeug zum abspielen von Sequenzen
        private bool play = false;
        private bool buttonLighted = false;
        private static float initialTime = 1000; //Zeit zwischen jeweiligem highlighten der Buttons
        private int currLightButton = 0;
        private static Timer aTimer;


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

            string taskData = task.Terminal.TaskData;

            //if (IsLegit(taskData))
                solution = TaskDataToArray(taskData);
            //else
                //throw new Exception("Your given TaskDataText is not matching the standard: \"color,color,...,color\"");

            
        }

        private void playButtons(object sender, ElapsedEventArgs e)
        {
            if(play)
            {
                if(currLightButton<=solution.Length && !buttonLighted)
                {
                    lightButton(solution[currLightButton], true);
                    buttonLighted = true;
                }
                else if(currLightButton <=solution.Length && buttonLighted)
                {
                    lightButton(solution[currLightButton], false);
                    buttonLighted = false;
                    if (currLightButton == solution.Length-1)
                    {
                        //letzter Button -> nicht mehr spielen, zaehler wieder auf 0 setzen, playButton zeigen, timer ausstellen
                        console.Print("done playing");
                        play = false;
                        currLightButton = 0;
                        playButton.Enable = true;
                        playButton.Visible = true;
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


        private void Play_click(Button sender)
        {
            play = true;
            playButton.Visible = false;
            playButton.Enable = false;
            aTimer = new System.Timers.Timer(initialTime);
            aTimer.Elapsed += playButtons;
            aTimer.Enabled = true;
        }

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
            else
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
            else
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
            else
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
            else
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

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

        private void checkSolution()
        {
            if(compareSolutions())
            {
                task.Success = true;
            }
            else
            {
                task.Success = false;
            }
        }

        private bool compareSolutions()
        {
            bool check=false;

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

        private string[] TaskDataToArray(string tskdt)
        {
            string[] rtrn = tskdt.Split(',');
            return rtrn;
        }

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

