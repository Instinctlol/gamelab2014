using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine.MapSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienSpawnerAI"/> entity type.
    /// </summary>
    public class AlienSpawnerAIType : AlienUnitAIType
    {
    }

    /// <summary>
    /// AI for small-alien-spawnpoint
    /// </summary>
    public class AlienSpawnerAI : AlienUnitAI
    {
        /*************/
        /* Attribute */
        /*************/
        AlienSpawnerAIType _type = null; public new AlienSpawnerAIType Type { get { return _type; } }



        /*******************/
        /* Getter / Setter */
        /*******************/
        /// <summary>
        /// Getter for AlienSpawner Object
        /// </summary>
        [Browsable(false)]
        public new AlienSpawner ControlledObject
        {
            get { return (AlienSpawner)base.ControlledObject; }
        }



        /**************/
        /* Funktionen */
        /**************/
        /// <summary>
        /// Get all tasks for AlienSpawner
        /// </summary>
        /// <returns></returns>
        public override List<AlienUnitAI.UserControlPanelTask> GetControlPanelTasks()
        {
            List<UserControlPanelTask> list = new List<UserControlPanelTask>();
            if (ControlledObject.SpawnedUnit == null)
            {
                // Create task for producing small aliens
                AlienType unitType = (AlienType)EntityTypes.Instance.GetByName("Alien");
                
                list.Add(new UserControlPanelTask(new Task(Task.Types.ProductUnit, unitType),
                    CurrentTask.Type == Task.Types.ProductUnit));
            }
            else
            {
                // stop task for producing small aliens
                list.Add(new UserControlPanelTask(new Task(Task.Types.Stop),
                    CurrentTask.Type == Task.Types.Stop));
            }

            return list;
        }

        /// <summary>
        /// Wird bei jedem Tick-Event ausgeführt (OnTick() in AlienUnitAI). Überprüft, ob es einen neuen Task gibt und führt diesen aus.
        /// </summary>
        protected override void TickTasks()
        {
            base.TickTasks();
            // AlienSpawner auslesen
            AlienSpawner controlledObj = ControlledObject;
            if (controlledObj == null)
                return;

            switch (CurrentTask.Type)
            {
                // Wenn Task ProductUnit ist, Task in Queue speichern (DoTask() inAlienUnitAI)
                case Task.Types.ProductUnit:
                    if (ControlledObject.SpawnedUnit == null)
                        DoTask(new Task(Task.Types.Stop), false);
                    break;
            }

        }

        /// <summary>
        /// Task intern ausführen
        /// </summary>
        /// <param name="task"></param>
        protected override void DoTaskInternal(AlienUnitAI.Task task)
        {
            if (task.Type != Task.Types.ProductUnit)
                ControlledObject.StopProductUnit();

            // hier wird Stop-Task u.a. geprüft
            base.DoTaskInternal(task);

            if (task.Type == Task.Types.ProductUnit)
            {
                // Produktion kleiner Aliens starten
                ControlledObject.StartProductUnit((AlienType)task.EntityType, task.SpawnNumber);
            }
        }
    }
}
