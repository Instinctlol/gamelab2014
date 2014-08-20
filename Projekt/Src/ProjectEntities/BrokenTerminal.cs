using Engine.EntitySystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class BrokenTerminalType : RepairableType { }

    public class BrokenTerminal : Repairable
    {
        BrokenTerminalType _type = null; public new BrokenTerminalType Type { get { return _type; } }

        [FieldSerialize]
        private string sectorStatusData;
        [FieldSerialize]
        private string taskData;
        [FieldSerialize]
        private Terminal.TerminalTaskType taskType;
        [FieldSerialize]
        private Terminal.TerminalWindowType windowType;
        [FieldSerialize]
        private string taskFailSound;
        [FieldSerialize]
        private string taskSuccessSound;

        [FieldSerialize]
        private string terminalLogic;
        

        //***************************
        //*******Getter-Setter*******
        //***************************
        public String  TerminalLogic
        {
            get { return terminalLogic; }
            set { terminalLogic = value; }
        }

        [LocalizedDescription("String which describes the position of this Terminal", "SectorStatusData")]
        public string SectorStatusData
        {
            get { return sectorStatusData; }
            set { sectorStatusData = value; }
        }


        [LocalizedDescription("A string which some tasks use. \n" + "For PINTasks enter the code e.g. 1234", "TaskData")]
        public string TaskData
        {
            get { return taskData; }
            set { taskData = value; }
        }

        [LocalizedDescription("Choose what kind of task is to be assigned to this Terminal", "TaskType")]
        public Terminal.TerminalTaskType TaskType
        {
            get { return taskType; }
            set
            {
                if (WindowType != Terminal.TerminalWindowType.SectorStatus)
                    taskType = value;
            }
        }

        [LocalizedDescription("None: Task is directly shown \n" + "Default: You have to click on a button to start and show the task", "WindowType")]
        public Terminal.TerminalWindowType WindowType
        {
            get { return windowType; }
            set
            {
                windowType = value;
                if (windowType == Terminal.TerminalWindowType.SectorStatus)
                    taskType = Terminal.TerminalTaskType.None;
            }
        }

        [LocalizedDescription("The file name of the sound to play, when a task was failed.", "TaskFailSound")]
        [DefaultValue("Sounds\\taskFail.ogg")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string TaskFailSound
        {
            get { return taskFailSound; }
            set { taskFailSound = value; }
        }

        [LocalizedDescription("The file name of the sound to play, when a task was failed.", "TaskFailSound")]
        [DefaultValue("Sounds\\taskSuccess.ogg")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string TaskSuccessSound
        {
            get { return taskSuccessSound; }
            set { taskSuccessSound = value; }
        }
        //*********************************


        protected override void OnRepair()
        {
            base.OnRepair();


            Terminal terminal = (Terminal)Entities.Instance.Create(
                        "Terminal", GameWorld.Instance);

            terminal.Position = Position + new Engine.MathEx.Vec3(1.007327f, 0.0f, 0.3f);
            terminal.Rotation = Rotation;

            terminal.WindowType = WindowType;
            terminal.TaskData = TaskData;
            terminal.TaskType = TaskType;
            terminal.SectorStatusData = SectorStatusData;

            terminal.LogicClass = (LogicClass)Entities.Instance.Create(TerminalLogic, terminal);

            terminal.PostCreate();

            this.Die();
        }


    }
}
