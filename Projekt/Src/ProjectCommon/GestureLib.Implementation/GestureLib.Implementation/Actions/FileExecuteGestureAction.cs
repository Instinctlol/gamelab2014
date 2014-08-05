using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;
using ProjectCommon;
using ProjectEntities;
using Engine;

namespace GestureLib
{
    public class FileExecuteGestureAction : IGestureAction
    {
        #region IGestureAction Members

        ///// <summary>
        ///// Executes this instance.
        ///// </summary>
        //[EnvironmentPermissionAttribute(SecurityAction.LinkDemand)]
        public void Execute()
        {
            
            GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.I));
            GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.I));
        }

        #endregion

        ///// <summary>
        ///// Gets or sets the process-starting-information.
        ///// </summary>
        ///// <value>The process-starting-information.</value>
        //public ProcessStartInfo StartInfo { get; set; }

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
