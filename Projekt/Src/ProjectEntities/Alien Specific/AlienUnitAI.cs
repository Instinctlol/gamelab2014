using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities.Alien_Specific
{
    /// <summary>
    /// Defines the <see cref="AlienUnitAI"/> entity type.
    /// </summary>
    public class AlienUnitAIType : AlienAIType
    {
    }

    /// <summary>
    /// AI for small aliens
    /// </summary>
    class AlienUnitAI : AlienAI
    {
        AlienUnitAIType _type = null; public new AlienUnitAIType Type { get { return _type; } }
    }
}
