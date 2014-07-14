using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.UISystem;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace ProjectEntities
{

    public class TerminalType : DynamicType
    {
        const float automaticHideDistanceDefault = 3.0f;
        [FieldSerialize]
        [DefaultValue(automaticHideDistanceDefault)]
        float automaticHideDistance = automaticHideDistanceDefault;

        [DefaultValue(automaticHideDistanceDefault)]
        public float AutomaticHideDistance
        {
            get { return automaticHideDistance; }
            set { automaticHideDistance = value; }
        }

    }


    public class Terminal : Dynamic
    {

        TerminalType _type = null; public new TerminalType Type { get { return _type; } }

        enum NetworkMessages
        {
            ButtonTypeToClient,
            TaskTypeToClient,
            FindControlManagerToClient,
            PressToServer,
            ActiveValueToClient,
        };


        //Task typen, spezifizieren welche art von Task ausgeführt wird.
        //None = Task succesfull nach button betätigen
        public enum TerminalTaskType
        {
            None,
            PIN,
            ColorSequence,
            Quiz,
            Duel_SchereSteinPapier,
        };

        //SmartButton type, spezifizieren welche art von Button initial angezeigt wird
        //None = Task wird direkt angezeigt
        public enum TerminalWindowType
        {
            Rotate,
            RotateAndSingleSwitch,
            RotateAndDoubleSwitch,
            SingleSwitch,
            DoubleSwitch,
            SectorStatus
        };


        //Repairable das zunächst reperiert werden muss
        [FieldSerialize]
        private List<RepairableItem> repairables = new List<RepairableItem>();

        //Zusätzliche daten die an die Task übergeben werden können
        [FieldSerialize]
        private string taskData;


        //Das gehört in TYPE
        //Sound der abgespielt wird, falls ein Task nicht erfolgreich beendet wurde
        [FieldSerialize]
        private string taskFailSound = "Sounds\\taskFail.ogg";

        //Sound der abgespielt wird, falls ein Task erfolgreich beendet wurde
        [FieldSerialize]
        private string taskSuccessSound = "Sounds\\taskSuccess.ogg";
        
        //WindowType
        [FieldSerialize]
        private TerminalWindowType windowType;

        //TaskType
        [FieldSerialize]
        private TerminalTaskType taskType;
        

        //GUI Fenster das auf dem Terminal angezeigt wird
        private Window window;

        //Angefügtes GUI objekt
        private MapObjectAttachedGui attachedGuiObject;

        //ControlManager zum anzeigen der GUI
        private In3dControlManager controlManager;

        //MainControl, GUI die angezeigt wird
        private Control mainControl;

        //Task die angezeigt werden soll
        private Task task;

        //SmartButton der angezeigt werden soll
        private SmartButton button;

        //Ob das terminal grad aktiv is oder nicht
        private bool active = false;



        //Attached meshes
        private MapObjectAttachedMesh terminalProjector;

        private MapObjectAttachedMesh terminalScreen;

        private float nextCheckRemainingTime = 0.2f;
        
        //***************************
        //*******Getter-Setter*******
        //***************************   
        public bool Active
        {
            get { return active; }
            set { 
                active = value;
                terminalScreen.Visible = value;

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendActiveValueToAllClients();
            }
        }


        [LocalizedDescription("A string which some tasks use. \n"+"For PINTasks enter the code e.g. 1234", "TaskData")]
        public string TaskData
        {
            get { return taskData; }
            set { taskData = value; }
        }

        [LocalizedDescription("Choose what kind of task is to be assigned to this Terminal", "TaskType")]
        public TerminalTaskType TaskType
        {
            get { return taskType; }
            set
            {
                if(WindowType!= TerminalWindowType.SectorStatus)
                    taskType = value;
            }
        }

        [LocalizedDescription("None: Task is directly shown \n"+"Default: You have to click on a button to start and show the task", "WindowType")]
        public TerminalWindowType WindowType
        {
            get { return windowType; }
            set
            {
                windowType = value;
                if (windowType == TerminalWindowType.SectorStatus)
                    taskType = TerminalTaskType.None;
            }
        }

        public List<RepairableItem> Repairables
        {
            get { return repairables; }
        }

        public class RepairableItem
        {
            [FieldSerialize]
            Repairable repairable;

            public Repairable Repairable
            {
                get { return repairable; }
                set { repairable = value; }
            }

            public override string ToString()
            {
                if (repairable == null)
                    return "(not initialized)";
                return repairable.Name;
            }
        }

        [Browsable(false)]
        public Task Task
        {
            get { return task; }
            set { task = value; }
        }

        [Browsable(false)]
        public SmartButton Button
        {
            get { return button; }
            set { button = value; }
        }

        [Browsable(false)]
        public In3dControlManager ControlManager
        {
            get { return controlManager; }
        }

        [Browsable(false)]
        public Control MainControl
        {
            get { return mainControl; }
            set { mainControl = value; }
        }

        [Browsable(false)]
        public Window Window
        {
            get { return window; }
            set
            {
                window = value;
                CreateMainControl();
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

        public MapObjectAttachedMesh TerminalScreen
        {
            get { return terminalScreen; }
        }

        public MapObjectAttachedMesh TerminalProjector
        {
            get { return terminalProjector; }
        }
        //*********************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalSwitchAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalLightAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalDoorAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalRotateLeftAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalRotateRightAction;
        

        //*****************************

        public void OnTerminalFinishedDoLight()
        {
            if (TerminalLightAction != null)
                TerminalLightAction(this);
        }
        public void OnTerminalFinishedDoDoor()
        {
            if (TerminalDoorAction != null)
                TerminalDoorAction(this);
        }
        public void OnTerminalFinishedDoSwitch()
        {
            if (TerminalSwitchAction != null)
                TerminalSwitchAction(this);
        }
        public void OnTerminalFinishedDoRotateLeft()
        {
            if (TerminalRotateLeftAction != null)
                TerminalRotateLeftAction(this);
        }
        public void OnTerminalFinishedDoRotateRight()
        {
            if (TerminalRotateRightAction != null)
                TerminalRotateRightAction(this);
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);

            Server_SendButtonType(windowType);
            Server_SendTaskType(taskType);
            Server_SendFindControlManager();
        }


        //Ausführen beim laden des Objektes
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            foreach(MapObjectAttachedObject obj in AttachedObjects)
            {
                MapObjectAttachedMesh mesh = obj as MapObjectAttachedMesh;
                if(mesh != null)
                {
                    if (mesh.Alias == "terminalProjector")
                        terminalProjector = mesh;
                    else if (mesh.Alias == "terminalScreen")
                        terminalScreen = mesh;
                }
            }

            if (!loaded)
                return;

            button = null;
            task = null;



            foreach(MapObject o in Map.Instance.GetObjects(this.MapBounds))
            {
                SmartButton b = o as SmartButton;
                if (b != null)
                {
                    button = b;
                    continue;
                }

                Task t = o as Task;
                if (t != null)
                {
                    task = t;
                    continue;
                }

                if (button != null && task != null)
                    break;
            }

            button.Terminal = this;
            task.Terminal = this;

            //task.TaskFinished += OnTaskFinished;

            //Ggfs repairable an Button weiter reichen
            if (repairables != null)
                foreach (RepairableItem r in repairables)
                    button.AttachRepairable(r.Repairable);
            else
                button.SetWindowEnabled();
            
            //AttachedGUIObjekt finden
            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }

            //MainControl erzeugen zum anzeigen der GUI
            CreateMainControl();

            SubscribeToTickEvent();
        }

        //Beim zerstören alles wieder rückgängig
        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
        }

        protected override void OnTick()
        {
            base.OnTick();

            nextCheckRemainingTime -= TickDelta;
            if (nextCheckRemainingTime <= 0)
            {
                nextCheckRemainingTime = .2f;

                Sphere sphere = new Sphere(Position, Type.AutomaticHideDistance);

                bool hide = true;
                Map.Instance.GetObjects(sphere, delegate(MapObject mapObject)
                {
                    if ((mapObject as GameCharacter) == null)
                        return;

                    hide = false;
                });


                if (hide && active)
                    Active = false;
            }
        }
        
        //MainControl erzeugen
        private void CreateMainControl()
        {
            if (mainControl != null)
            {
                controlManager.Controls.Remove(mainControl);
                mainControl = null;
            }

            if (controlManager != null && window != null)
            {
                mainControl = window.CurWindow;
                if (mainControl != null)
                    controlManager.Controls.Add(mainControl);
            }

            //update MapBounds
            //SetTransform(Position, Rotation, Scale);
        }

        public void Press()
        {
            if (!EntitySystemWorld.Instance.IsServer())
                Client_SendPressToServer();
        }

        private void Server_SendTaskType(TerminalTaskType taskType)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Terminal),
                (ushort)NetworkMessages.TaskTypeToClient);
            writer.Write((Int16)taskType);
            EndNetworkMessage();
        }

        private void Server_SendButtonType(TerminalWindowType buttonType)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Terminal),
                (ushort)NetworkMessages.ButtonTypeToClient);
            writer.Write((Int16)buttonType);
            EndNetworkMessage();
        }

        private void Server_SendFindControlManager()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Terminal),
                (ushort)NetworkMessages.FindControlManagerToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.FindControlManagerToClient)]
        private void Client_FindControlManager(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }
        }


        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TaskTypeToClient)]
        private void Client_ReceiveTaskType(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            TerminalTaskType type = (TerminalTaskType)reader.ReadInt16();
            if (!reader.Complete())
                return;

            TaskType = type;
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ButtonTypeToClient)]
        private void Client_ReceiveButtonType(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            TerminalWindowType type = (TerminalWindowType)reader.ReadInt16();
            if (!reader.Complete())
                return;

            WindowType = type;
        }

        public void DoRotateLeftEvent()
        {
            if (TerminalRotateLeftAction != null)
                TerminalRotateLeftAction(this);
        }

        public void DoRotateRightEvent()
        {
            if (TerminalRotateRightAction != null)
                TerminalRotateRightAction(this);
        }

        public void DoDoorEvent()
        {
            if (TerminalDoorAction != null)
                TerminalDoorAction(this);
        }

        public void DoLightEvent()
        {
            if (TerminalLightAction != null)
                TerminalLightAction(this);
        }

        public void DoSwitchEvent()
        {
            if (TerminalSwitchAction != null)
                TerminalSwitchAction(this);
        }

        void Client_SendPressToServer()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Terminal),
                (ushort)NetworkMessages.PressToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.PressToServer)]
        void Server_ReceivePress(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            Active = !active;
        }

        void Server_SendActiveValueToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Terminal),
                (ushort)NetworkMessages.ActiveValueToClient);
            writer.Write(active);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ActiveValueToClient)]
        void Client_ReceiveActiveValue(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool act = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            Active = act;
        }

    }
}
