using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienAI"/> entity type.
    /// </summary>
    public class AlienAIType : AlienUnitAIType
    {
    }

    /// <summary>
    /// AI for small aliens
    /// 
    /// bleibt erstmal leer, muss nachher code von AlienUnitAI rüberkopiert werden, der nicht von der AlienSpawnerAI verwendet wird
    /// </summary>
    public class AlienAI : AlienUnitAI
    {
        AlienAIType _type = null; public new AlienAIType Type { get { return _type; } }

        
    }
}
