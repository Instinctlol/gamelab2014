using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class TaskWindow : Window
    {
        // Event und Delegate zum Starten eines Duell-Spiels beim Alien
        public static event TaskWindowEventDelegate startAlienGame;
        public delegate void TaskWindowEventDelegate();

        protected Task task;

        public TaskWindow(Task task)
        {
            this.task = task;
        }

        /// <summary>
        /// Zum Öffnen des Duell-Spiels beim Alien
        /// </summary>
        /// <param name="task"></param>
        protected void Server_createWindowForAlien(Task task)
        {
            Computer.CsspwTask = task;
            if (startAlienGame != null)
            {
                startAlienGame();
            }
        }
    }
}
