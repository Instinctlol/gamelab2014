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
            TreeChangedText,
            FourChangedText,
            QuestionChangedText,
            Play,
        }

        private string loesung;

        private Boolean geloest = false;
        
        public int anzahlRunden;
        //csv einlesen
        //auf relativen Pfad ändern
        private string pfadZurDatei = "Data\\GUI\\Tasks\\questions.txt";
        private string antwort;
        private int randomPositionTrue;
        //Button
        List<Button> listButtons = new List<Button>();
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
            
            //Liste der Button
            listButtons.Add((Button)CurWindow.Controls["One"]);
            listButtons.Add((Button)CurWindow.Controls["Two"]);
            listButtons.Add((Button)CurWindow.Controls["Three"]);
            listButtons.Add((Button)CurWindow.Controls["Four"]);
                       
            //Methoden für die Button
            ((Button)CurWindow.Controls["One"]).Click += One_click;
            ((Button)CurWindow.Controls["Two"]).Click += Two_click;
            ((Button)CurWindow.Controls["Three"]).Click += Three_click;
            ((Button)CurWindow.Controls["Four"]).Click += Four_click;

            ((Button)CurWindow.Controls["Play"]).Click += Play_Click;

            task.Client_WindowStringReceived += Client_StringReceived;
            task.Server_WindowDataReceived += Server_DataReceived;
           
        }

        private NetworkMessages changedValue (Button button)
        {
            string name = button.Name;
 
            switch (name) 
            {
                case "One": 
                    return NetworkMessages.OneChangedText;
                case "Two":
                    return NetworkMessages.OneChangedText;
                case "Three":
                    return NetworkMessages.OneChangedText;
                case "Four":
                    return NetworkMessages.OneChangedText; 
            }
            return 0;
        }


        private void updateQuestion()
        {
            //Random Number für Questins
            int randomQuestionNumber = rnd.Next(1, questions.Count);
            randomPositionTrue = rnd.Next(1, 4);

            int[] loesungPosition = {0,0,0,0};

            //Position mit 1 ist die Lösungsposition
            for (int x = 0; x < loesungPosition.Length; x++)
            {
                if (x == randomPositionTrue)
                    loesungPosition[x] = 1;
                else
                    loesungPosition[x] = 0;
            }

            ((TextBox)CurWindow.Controls["Question"]).Text = questions[randomQuestionNumber][0];
            task.Server_SendWindowString(questions[randomQuestionNumber][0],(UInt16) NetworkMessages.QuestionChangedText);

            loesung = questions[randomQuestionNumber][1];
            int i = 0;
            //Zeiweisen der Texte zu den Button
            listButtons.ForEach(delegate(Button button)
            {
                if (loesungPosition[i] == 1)
                {
                    button.Text = loesung;
                    task.Server_SendWindowString(loesung, (UInt16)changedValue(button));
                }
                else
                {
                    button.Text = questions[randomQuestionNumber][i];
                    task.Server_SendWindowString(questions[randomQuestionNumber][i], (UInt16)changedValue(button));
                }
                i++;
            });
           
        }


        void Client_StringReceived(string message, UInt16 netMassage)
        {
            NetworkMessages msg = (NetworkMessages)netMassage;
            switch (msg)
            {
                case NetworkMessages.OneClicked:
                    ((Button)CurWindow.Controls["One"]).Text = message;
                    break;
                case NetworkMessages.TwoClicked:
                    ((Button)CurWindow.Controls["Two"]).Text = message;
                    break;
                case NetworkMessages.ThreeClicked:
                    ((Button)CurWindow.Controls["Three"]).Text = message;
                    break;
                case NetworkMessages.FourClicked:
                    ((Button)CurWindow.Controls["Four"]).Text = message;
                    break;
            }

        }

        void One_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.OneClicked);
        }
        void Two_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.TwoClicked);
        }
        void Three_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.ThreeClicked);
        }
        void Four_click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.FourClicked);
        }
        void Play_Click(Button b)
        {
            task.Client_SendWindowData((UInt16)NetworkMessages.Play);
        }


        void Client_DataReceived(UInt16 message)
        {
            NetworkMessages msg = (NetworkMessages)message;
            switch (msg)
            {
                case NetworkMessages.OneClicked:
                    if (randomPositionTrue == 1)
                        geloest = true;
                    break;
                case NetworkMessages.TwoClicked:
                    if (randomPositionTrue == 2)
                        geloest = true;
                    break;
                case NetworkMessages.ThreeClicked:
                    if (randomPositionTrue == 3)
                        geloest = true;
                    break;
                case NetworkMessages.FourClicked:
                    if (randomPositionTrue == 4)
                        geloest = true;
                    break;
            }
        }

        void Server_DataReceived(UInt16 message)
        {

            NetworkMessages msg = (NetworkMessages)message;
            switch (msg)
            {
                case NetworkMessages.OneClicked:
                    if (randomPositionTrue == 1)
                        geloest = true;
                    break;
                case NetworkMessages.TwoClicked:
                    if (randomPositionTrue == 2)
                        geloest = true;
                    break;
                case NetworkMessages.ThreeClicked:
                    if (randomPositionTrue == 3)
                        geloest = true;
                    break;
                case NetworkMessages.FourClicked:
                    if (randomPositionTrue == 4)
                        geloest = true;
                    break;
                case NetworkMessages.Play:
                    if (statusPlay == false)
                    {
                        statusPlay = true;
                        updateQuestion();
                    }
                    break;

            }
           
        }

    }
}
