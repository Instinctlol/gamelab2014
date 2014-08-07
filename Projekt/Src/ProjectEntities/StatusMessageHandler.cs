using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class StatusMessageHandler
    {

        // Event und Delegate für Spawner-Nachrichten
        public static event StatusMessageEventDelegate showMessage;
        public static event StatusMessageEventDelegate showControlMessage;
        public delegate void StatusMessageEventDelegate(String message);

        public static void sendMessage(String message)
        {
            if (showMessage != null)
            {
                showMessage(message);
            }
        }

        public static void sendControlMessage(String message)
        {
            if (showControlMessage != null)
            {
                showControlMessage(message);
            }
        }
    }
}
