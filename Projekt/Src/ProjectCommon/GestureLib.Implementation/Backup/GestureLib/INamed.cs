using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Defines the methods and properties for classes which can be identified by a name.
    /// </summary>
    public interface INamed
    {
        /// <summary>
        /// Gets or sets the name to identifiy this object.
        /// </summary>
        /// <value>The name to identify this object.</value>
        string Name { get; set; }
    }
}
