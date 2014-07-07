using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProjectEntities
{
    class QuestionTaskWindow : TaskWindow
    {
        enum NetworkMessages
        {
            OneClicked,
            TwoClicked,
            ThreeClicked,
            FourClicked,
            OneChangedText,
            TwoChangedText,
            ThreeChangedText,
            FourChangedText,
            QuestionChangedText,
            PlayClicked,
        }

        private string loesung;

        private Boolean geloest = false;
        
        public int anzahlRunden;
        //csv einlesen
        //auf relativen Pfad ändern
        private string pfadZurDatei = "Data\\GUI\\Tasks\\questions.txt";
        private string antwort;
        private int randomPositionTrue;

        private static Timer aTimer;
        private Random rnd = new Random();

        //Varianblen können verändert werden
        private static float initialTime = 200; //Zeit zwischen jeweiligem highlighten der Buttons, 1000=1sek
        private static int rounds = 1;

        private bool statusPlay = false;
        private List<string[]> questions = new List<string[]>();

        public QuestionTaskWindow(Task task): base(task)
        {
            //Fragen einlesen
            StreamReader sr = new StreamReader(pfadZurDatei);
            string inputLine = "";

            string[] valueLine = { "question", "answer1", "answer2", "answer3", "answer4" };

            int i = 0;
            while ((inputLine = sr.ReadLine()) != null)
            {
                valueLine = inputLine.Split(new Char[] { ';' });
                questions.Add(valueLine);
                i++;
            }

            //GUI erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Tasks\\QuestionTaskWindow.gui");
            
                       
            //Methoden für die Button
            ((Button)CurWindow.Controls["One"]).Click += One_Click;
            ((Button)CurWindow.Controls["Two"]).Click += Two_Click;
            ((Button)CurWindow.Controls["Three"]).Click += Three_Click;
            ((Button)CurWindow.Controls["Four"]).Click += Four_Click;

            ((Button)CurWindow.Controls["Play"]).Click += Play_Click;

            if(task.IsServer)
                task.Server_WindowDataReceived += Server_DataReceived;
            else
                task.Client_WindowStringReceived += Client_StringReceived;
        }


        private NetworkMessages changedValue (Button button)
        {
            string name = button.Name;
 
            switch (name) 
            {
                case "One": 
                    return NetworkMessages.OneChangedText;
                case "Two":
                    return NetworkMessages.TwoChangedText;
                case "Three":
                    return NetworkMessages.ThreeChangedText;
                case "Four":
                    return NetworkMessages.FourChangedText; 
            }
            return 0;
        }


        private void updateQuestion()
        {
            if (!statusPlay)
                return;

            //Random Number für Questins
            int randomQuestionNumber = rnd.Next(1, questions.Count - 1);
            randomPositionTrue = rnd.Next(1, 4);

            int[] loesungPosition = {0,0,0,0};
            int pos = 2;

            //Position mit 1 ist die Lösungsposition
            for (int i = 0; i < loesungPosition.Length; i++)
            {
                if (i == randomPositionTrue - 1)
                {
                    loesungPosition[i] = 1;
                }
                else
                {
                    loesungPosition[i] = pos;
                    pos++;
                }
            }

            ((TextBox)CurWindow.Controls["Question"]).Text = questions[randomQuestionNumber][0];
            task.Server_SendWindowString(questions[randomQuestionNumber][0],(UInt16) NetworkMessages.QuestionChangedText);

            loesung = questions[randomQuestionNumber][1];
            int ii = 0;
            //Zeiweisen der Texte zu den Button
            foreach(String button in new string[] {"One", "Two", "Three", "Four"})
            {
                Button b = ((Button)CurWindow.Controls[button]);

                b.Text = questions[randomQuestionNumber][loesungPosition[ii]];
                task.Server_SendWindowString(questions[randomQuestionNumber][loesungPosition[ii]], (UInt16) changedValue(b));
                ii++;
            }           
        }


        void Client_StringReceived(string message, UInt16 netMessage)
        {
            NetworkMessages msg = (NetworkMessages)netMessage;
            ((Button)CurWindow.Controls["Play"]).Visible = false;
            ((Button)CurWindow.Controls["Play"]).Enable = false;
            switch (msg)
            {
                case NetworkMessages.OneChangedText:
                    ((Button)CurWindow.Controls["One"]).Text = message;
                    break;
                case NetworkMessages.TwoChangedText:
                    ((Button)CurWindow.Controls["Two"]).Text = message;
                    break;
                case NetworkMessages.ThreeChangedText:
                    ((Button)CurWindow.Controls["Three"]).Text = message;
                    break;
                case NetworkMessages.FourChangedText:
                    ((Button)CurWindow.Controls["Four"]).Text = message;
                    break;
                case NetworkMessages.QuestionChangedText:
                    ((TextBox)CurWindow.Controls["Question"]).Text = message;
                    break;
            }

        }

        void One_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.OneClicked);
        }
        void Two_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.TwoClicked);
        }
        void Three_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.ThreeClicked);
        }
        void Four_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.FourClicked);
        }
        void Play_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.PlayClicked);
        }


        void Server_DataReceived(UInt16 message)
        {
            NetworkMessages msg = (NetworkMessages)message;
            switch (msg)
            {
                case NetworkMessages.OneClicked:
                    task.Success = randomPositionTrue == 1;
                    statusPlay = false;
                    break;
                case NetworkMessages.TwoClicked:
                    task.Success = randomPositionTrue == 2;
                    statusPlay = false;
                    break;
                case NetworkMessages.ThreeClicked:
                    task.Success = randomPositionTrue == 3;
                    statusPlay = false;
                    break;
                case NetworkMessages.FourClicked:
                    task.Success = randomPositionTrue == 4;
                    statusPlay = false;
                    break;
                case NetworkMessages.PlayClicked:
                    if (!statusPlay)
                    {
                        statusPlay = true;
                        updateQuestion();
                    }
                    break;
            }
           
        }

    }
}
