using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine.MapSystem;

namespace ProjectEntities.Alien_Specific
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
    class AlienSpawnerAI : AlienUnitAI
    {
        AlienSpawnerAIType _type = null; public new AlienSpawnerAIType Type { get { return _type; } }

        public override List<AlienUnitAI.UserControlPanelTask> GetControlPanelTasks()
        {
            List<UserControlPanelTask> list = new List<UserControlPanelTask>();

            if (ControlledObject.BuildedProgress == 1)
            {
                if (ControlledObject.BuildUnitType == null)
                {
                    //RTSHeadquaters specific
                    if (ControlledObject.Type.Name == "RTSHeadquaters")
                    {
                        AlienUnitType unitType = (AlienUnitType)EntityTypes.Instance.GetByName("RTSConstructor");
                        list.Add(new UserControlPanelTask(new Task(Task.Types.ProductUnit, unitType),
                            CurrentTask.Type == Task.Types.ProductUnit));
                    }

                    //RTSFactory specific
                    if (ControlledObject.Type.Name == "RTSFactory")
                    {
                        AlienUnitType unitType = (AlienUnitType)EntityTypes.Instance.GetByName("RTSRobot");
                        list.Add(new UserControlPanelTask(new Task(Task.Types.ProductUnit, unitType),
                            CurrentTask.Type == Task.Types.ProductUnit));
                    }
                }
                else
                {
                    list.Add(new UserControlPanelTask(new Task(Task.Types.Stop),
                        CurrentTask.Type == Task.Types.Stop));
                }
            }
            else
            {
                //building
                list.Add(new UserControlPanelTask(new Task(Task.Types.SelfDestroy)));
            }

            return list;
        }

        [Browsable(false)]
        public new AlienSpawner ControlledObject
        {
            get { return (AlienSpawner)base.ControlledObject; }
        }

        protected override void TickTasks()
        {
            base.TickTasks();

            AlienSpawner controlledObj = ControlledObject;
            if (controlledObj == null)
                return;

            switch (CurrentTask.Type)
            {

                case Task.Types.ProductUnit:
                    if (ControlledObject.BuildUnitType == null)
                        DoTask(new Task(Task.Types.Stop), false);
                    break;
            }

        }

        protected override void DoTaskInternal(AlienUnitAI.Task task)
        {
            if (task.Type != Task.Types.ProductUnit)
                ControlledObject.StopProductUnit();

            base.DoTaskInternal(task);

            if (task.Type == Task.Types.ProductUnit)
            {
                ControlledObject.StartProductUnit((AlienUnitType)task.EntityType);
            }
        }
    }
}
