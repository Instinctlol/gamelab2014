using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Permissions;
using ProjectCommon;
using GestureLib;
using Engine;

namespace ProjectCommon
{
    public class ActionTest : IGestureAction
    {
        #region IGestureAction Members

        ///// <summary>
        ///// Executes this instance.
        ///// </summary>
        //[EnvironmentPermissionAttribute(SecurityAction.LinkDemand)]
        public void Execute()
        {

            GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.I));
            
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
