using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities.Alien_Specific
{
    /// <summary>
    /// Defines the <see cref="AlienSpawnerAI"/> entity type.
    /// </summary>
    public class AlienSpawnerAIType : AlienAIType
    {
    }

    /// <summary>
    /// AI for small-alien-spawnpoint
    /// </summary>
    class AlienSpawnerAI : AlienAI
    {
        AlienSpawnerAIType _type = null; public new AlienSpawnerAIType Type { get { return _type; } }
    }
}
