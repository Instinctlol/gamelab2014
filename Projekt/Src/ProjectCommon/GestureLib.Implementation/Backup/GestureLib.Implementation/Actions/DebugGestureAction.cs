using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GestureLib
{
    public class DebugStreamGestureAction : IGestureAction
    {
        #region IGestureAction Members

        public void Execute()
        {
            if (WriteNewLine)
            {
                Output.WriteLine(DebugMessage);
            }
            else
            {
                Output.Write(DebugMessage);
            }

            if (AutoFlush)
            {
                Output.Flush();
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating whether a new line should be written automatically after a debug message.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the new line should be written; otherwise, <c>false</c>.
        /// </value>
        public bool WriteNewLine { get; set; }
        
        /// <summary>
        /// Gets or sets the debug message, which should be written into the output, when the Execute()-method is called.
        /// </summary>
        /// <value>The debug message.</value>
        public string DebugMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all buffers shoud be cleared automatically and forced to be written immediately into the output.
        /// </summary>
        /// <value><c>true</c> if the buffers should be written automatically; otherwise, <c>false</c>.</value>
        public bool AutoFlush { get; set; }

        /// <summary>
        /// Gets or sets the output target, where the debug messages are written to
        /// </summary>
        /// <value>The output target.</value>
        public TextWriter Output { get; set; }

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
