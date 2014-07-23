using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;

namespace GestureLib
{
    public class FileExecuteGestureAction : IGestureAction
    {
        #region IGestureAction Members

        /// <summary>
        /// Executes this instance.
        /// </summary>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand)]
        public void Execute()
        {
            //uses the stored process-starting-information to start the process
            Process.Start(StartInfo);
        }

        #endregion

        /// <summary>
        /// Gets or sets the process-starting-information.
        /// </summary>
        /// <value>The process-starting-information.</value>
        public ProcessStartInfo StartInfo { get; set; }

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
