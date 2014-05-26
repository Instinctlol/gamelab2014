using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Text;

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

        private int currPlayerResultPos=0;

        public ColorSequenceTaskWindow(Task task) : base(task)
        {
            //GUI Erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\ColorSequenceGUI");

            //Methoden für Buttons
            ((Button)CurWindow.Controls["Green"]).Click += Green_click;
            ((Button)CurWindow.Controls["Yellow"]).Click += Yellow_click;
            ((Button)CurWindow.Controls["Red"]).Click += Red_click;
            ((Button)CurWindow.Controls["Blue"]).Click += Blue_click;


            string taskData = task.Terminal.TaskData;

            if (!IsLegit(taskData))
                console.Print("Given TaskData not correct");
            else
                solution = TaskDataToArray(taskData);
        }

        private void Blue_click(Button sender)
        {
            throw new NotImplementedException();
        }

        private void Red_click(Button sender)
        {
            throw new NotImplementedException();
        }

        private void Yellow_click(Button sender)
        {
            throw new NotImplementedException();
        }

        private void Green_click(Button sender)
        {
            if (currPlayerResultPos<solution.Length)
            {
                playerResult[currPlayerResultPos] = green;
                if(++currPlayerResultPos == solution.Length-1)
                {
                    checkSolution();
                }
            }
            else
            {
                console.Print("Not allowed to add more colors to solution");
            }
        }

        private void checkSolution()
        {
            throw new NotImplementedException();
        }

        private string[] TaskDataToArray(string tskdt)
        {
            int[] indexes = CharPos(tskdt, ',');
            string[] rtrn = new string[tskdt.Split(',').Length];
            int pos=0;

            for (int i = 0; i <= indexes.Length; i++)
            {
                rtrn[pos] = tskdt.Substring(pos, indexes[i] - 1);
                pos = indexes[i] + 1;
            }

            return rtrn;
        }

        private bool IsLegit(string input)
        {
            int[] indexes = CharPos(input, ',');
            bool check=false;

            int pos = 0;

            for (int i = 0; i <= indexes.Length; i++)
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

            return check;
        }

        private int[] CharPos(string Input, char C)
        {
            var foundIndexes = new List<int>();
            int[] rtrn;

            for (int i = Input.IndexOf(C); i > -1; i = Input.IndexOf(C, i + 1))
            {
                // for loop end when i=-1 ('a' not found)
                foundIndexes.Add(i);
            }

            return foundIndexes.ToArray();
        }
    }
}

