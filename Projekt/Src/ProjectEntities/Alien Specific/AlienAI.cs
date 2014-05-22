using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities.Alien_Specific
{
    /// <summary>
    /// Defines the <see cref="AlienAI"/> entity type.
    /// </summary>
    public class AlienAIType : AIType
    {
    }

    /// <summary>
    /// base AI for both, AlienUnitAI and AlienSpawnerAI.
    /// contains the logic which will be used from spawnpoint and small alien.
    /// </summary>
    class AlienAI : AI
    {
        AlienAIType _type = null; public new AlienAIType Type { get { return _type; } }
    }
}
