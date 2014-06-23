using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectCommon;


namespace ProjectEntities
{
    class TaskIOManager
    {
        static TaskIOManager instance;

        public static TaskIOManager Instance
        {
            get { return instance; }
        }

        public static void initIOM()
        {

            if (instance != null)
            {
                instance = new TaskIOManager();
            }

        }

        // aktuelle Task-ToDoListe für Astronauten
        List<Task> TaskListe = new List<Task>();

        //Task der liste hinzufügen + Nachricht für die Astronauten, die ausgegeben werden soll
        public void addTaskToDo(Task task,String massage){

            if (task != null && massage == "")
            {

                TaskListe.Add(task);
                StatusMessageHandler.sendMessage("Erfülle DIE Aufgabe");
            }
            else if (task != null && massage != "")
            {
                TaskListe.Add(task);
                StatusMessageHandler.sendMessage(massage);
            }
            else
            {
                StatusMessageHandler.sendMessage("Task = null!!");
            }
        }

        // task aus der Liste entfernen + Nachricht für die Astronauten, die ausgegeben werden soll
        public void removeTaskToDo(Task task, String massage){

            if (task != null && massage == "")
            {
                TaskListe.Remove(task);
                StatusMessageHandler.sendMessage("Mission erfüllt");
            }
            else if (task != null && massage != "")
            {
                TaskListe.Remove(task);
                StatusMessageHandler.sendMessage(massage);
            }


        }

        public void CleanTaskList()
        {
            
        }       



    }
}
