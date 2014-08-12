// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.Renderer;
using Engine.MathEx;
using Engine.SoundSystem;
using Engine.UISystem;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.FileSystem;
using Engine.Utils;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    public class AlienGameWindow : GameWindow
    {
        /*************/
        /* Attribute */
        /*************/
        // Kameraattribute
        enum CameraType
        {
            Game,
            //Count
        }
		static float mydelta  = 0;
        static Vec2 todoTranslate = Vec2.Zero;
        static float todoRotate = 0;
        static CameraType cameraType = CameraType.Game;
        float cameraDistance = 30;//200
        // Kamera sehr steil einstellen
        SphereDir cameraDirection = new SphereDir((float)Math.PI / 2 - 0.01f, (float)Math.PI / 2 - 0.01f);
        //SphereDir cameraDirection = new SphereDir(1.5f, 0.85f);
        Vec2 cameraPosition;
        bool endkampfraum = false;
        Vec2 oldCameraPosition;

        // Pathfinding Attribute
        [Config("Map", "drawPathMotionMap")]
        public static bool mapDrawPathMotionMap;

        // GUI Attribute
        Control hudControl;
        Control numPad;
        Control bigMinimap;
        BigMinimapWindow bigMinimapObj;

        // Spawning
        int spawnNumber = 1;

        // Select mode
        List<Unit> selectedUnits = new List<Unit>();
        bool selectMode;
        Vec2 selectStartPos;
        bool selectDraggedMouse;
        bool selectworkbench;
        Vec2 selectworkbenchcoord;

        
        // Task Attribute
        int taskTargetChooseIndex = -1;

        // Minimap
        Control minimapControl;

        // Time Counter
        float timeForUpdateNotificationStatus;
        float timeForDeleteNotificationMessage;
        float timeForDropItemIncrementation = 300;

        // Headtracking
        Vec3 headtrackingOffset;
        bool isHeadtrackingActive = false;

        //SchereSteinPapierWindow
        Server_SchereSteinPapierWindow sspw;



        /**************/
        /* Funktionen */
        /**************/
        /// <summary>
        /// Funktion für notSpawnableEvent aus der AlienSpawner-Klasse. Dieses Event wird getriggert, wenn in der AlienSpawner-Klasse 
        /// festgestellt wird, dass keine Aliens gespawnt werden dürfen/können. Dann muss nämlich eine entsprechende Nachricht angezeigt werden.
        /// </summary>
        /// <param name="message"></param>
        public void AddNotificationMessage(String message)
        {
            hudControl.Controls["ActiveArea"].Controls["StatusMessage"].Text = message;
            // Restzeit setzen für Anzeige und das dann bei ontick runterzählen, damit irgendwann text gelöscht wird
            timeForDeleteNotificationMessage = 5;
        }

        public void AddControlMessage(String message)
        {
            if (message != "")
            {
                hudControl.Controls["ControlMessage"].Controls["Message"].Text = message;
                hudControl.Controls["ControlMessage"].Visible = true;
            }
            else
            {
                hudControl.Controls["ControlMessage"].Visible = false;
            }
        }

        /// <summary>
        /// Event Listener initialisieren
        /// </summary>
        private void InitializeEventListener()
        {
            // Event zum Erhalten von Status Nachrichten, die angezeigt werden müssen registrieren
            StatusMessageHandler.showMessage += new StatusMessageHandler.StatusMessageEventDelegate(AddNotificationMessage);
            StatusMessageHandler.showControlMessage += new StatusMessageHandler.StatusMessageEventDelegate(AddControlMessage);
            HeadTracker.Instance.TrackingEvent += new HeadTracker.receiveTrackingData(receiveTrackingData);
            // Event zum Starten von Duell-Spielen
            TaskWindow.startAlienGame += csspwSet;
        }

        //hudFunktionen

        private void hudFunctions(String name){

            if (name == "Menu") {
                Controls.Add(new MenuWindow());
            }
            if (name == "Help") {
                hudControl.Controls["HelpWindow"].Visible = !hudControl.Controls["HelpWindow"].Visible;
            }
            if (name == "HelpClose") {
                hudControl.Controls["HelpWindow"].Visible = false;
            }
            if (name == "Path") {
                mapDrawPathMotionMap = !mapDrawPathMotionMap;
            }
        
        }


        // Beim Starten des Spiels GUI initialisieren und co
        protected override void OnAttach()
        {
            base.OnAttach();

            // Computer resetten
            Computer.Reset();

            EngineApp.Instance.KeysAndMouseButtonUpAll();
            InitializeEventListener();

            //hudControl
            //hudControl = ControlDeclarationManager.Instance.CreateControl("GUI\\AlienHUD.gui");
            hudControl = ControlDeclarationManager.Instance.CreateControl("GUI\\AlienHUD2.gui");
            Controls.Add(hudControl);

            //((Button)hudControl.Controls["StatusNotificationTop"].Controls["Menu"]).Click += delegate(Button sender)
            //{
            //    hudFunctions("Menu");
            //};

            /*
            ((Button)hudControl.Controls["StatusNotificationTop"].Controls["Help"]).Click += delegate(Button sender)
            {
                hudFunctions("Help");
            };
             

            ((Button)hudControl.Controls["HelpWindow"].Controls["Close"]).Click += delegate(Button sender)
            {
                hudFunctions("HelpClose");
            };
            
            ((Button)hudControl.Controls["StatusNotificationTop"].Controls["DebugPath"]).Click += delegate(Button sender)
            {
                hudFunctions("Path");
            };
             */

            ((Button)hudControl.Controls["SchereSteinPapier"].Controls["SchereButton"]).Click += delegate(Button sender)
            {
                EngineConsole.Instance.Print("sherebutton clicked");
            };
            ((Button)hudControl.Controls["SchereSteinPapier"].Controls["SchereButton"]).MouseEnter += AlienGameWindow_MouseEnter;
            //InitControlPanelButtons();
            numPad = hudControl.Controls["rechts"].Controls["NumPad"];
            UpdateControlPanel();

            // BigMinimap
            bigMinimap = hudControl.Controls["BigMinimap"].Controls["BigMinimap"];

            //miniminimap
            minimapControl = hudControl.Controls["Minimap"].Controls["MiniMiniMap"];
            //string textureName = Map.Instance.GetSourceMapVirtualFileDirectory() + "\\Minimap\\Minimap";
            //Texture minimapTexture = TextureManager.Instance.Load(textureName, Texture.Type.Type2D, 0);
            //minimapControl.BackTexture = minimapTexture;
            minimapControl.RenderUI += new RenderUIDelegate(Minimap_RenderUI);

            //set camera position
            //foreach (Entity entity in Map.Instance.Children)
            //{
            //    SpawnPoint spawnPoint = entity as SpawnPoint;
            //    if (spawnPoint == null)
            //        continue;
            //    cameraPosition = spawnPoint.Position.ToVec2();
            //    break;
            //}

            IEnumerable<AlienSpawner> spawnerList = Entities.Instance.EntitiesCollection.OfType<AlienSpawner>();
            IEnumerable<AlienSpawner> aliensp = from AlienSpawner asp in spawnerList
                                                where CheckMapPosition(asp.Position.ToVec2() * 0.9f)
                                             select asp;
            int izahl = new Random().Next(0, aliensp.Count());
            cameraPosition = aliensp.ElementAt(izahl).Position.ToVec2();
            cameraPosition.X = cameraPosition.X * 0.9f;
            cameraPosition.Y = cameraPosition.Y * 0.9f;

            //World serialized data
            if (World.Instance.GetCustomSerializationValue("cameraDistance") != null)
                cameraDistance = (float)World.Instance.GetCustomSerializationValue("cameraDistance");
            if (World.Instance.GetCustomSerializationValue("cameraDirection") != null)
                cameraDirection = (SphereDir)World.Instance.GetCustomSerializationValue("cameraDirection");
            if (World.Instance.GetCustomSerializationValue("cameraPosition") != null)
                cameraPosition = (Vec2)World.Instance.GetCustomSerializationValue("cameraPosition");
            for (int n = 0; ; n++)
            {
                Unit unit = World.Instance.GetCustomSerializationValue("selectedUnit" + n.ToString()) as Unit;
                if (unit == null)
                    break;
                SetEntitySelected(unit, true);
            }

            ResetTime();

            //render scene for loading resources
            EngineApp.Instance.RenderScene();

            EngineApp.Instance.MousePosition = new Vec2(1f, 1f);

            bigMinimapObj = new BigMinimapWindow(hudControl.Controls["BigMinimap"]);
        }

        private void AlienGameWindow_MouseEnter(Control sender)
        {
            EngineConsole.Instance.Print("shere entered");
        }

        /// <summary>
        /// Duell-Spiel starten
        /// </summary>
        private void csspwSet()
        {
            if(sspw==null)
            {
                sspw = new Server_SchereSteinPapierWindow(Computer.CsspwTask, hudControl.Controls["SchereSteinPapier"]);
                sspw.start(Computer.CsspwTask);
            }
            else
            {
                sspw.start(Computer.CsspwTask);
            }
        }

        // Beim Beenden des Spiels minimap freigeben
        protected override void OnDetach()
        {
            //minimap
            if (minimapControl.BackTexture != null)
            {
                minimapControl.BackTexture.Dispose();
                minimapControl.BackTexture = null;
            }

            base.OnDetach();
        }
		
		 protected override bool OnCustomInputDeviceEvent(InputEvent e) {
            TuioInputDeviceSpecialEvent test = (TuioInputDeviceSpecialEvent)e;
            if (test != null)
            {
                // HIER Desoxyribonukleinsaeure
                if (test.getOPType() == opType.translation)
                {
                    #region translate
                    Console.WriteLine("Trying to Translate");
                    todoTranslate.X += test.getx()*-10;
                    todoTranslate.Y += test.gety()*10;
                    #endregion
                }
                else if(test.getOPType() == opType.selection)
                {
                    #region selection
                    Vec2 select = new Vec2(test.getx(),test.gety());
                    if (select.X == 0f && select.Y == 0f)
                    {
                        if(selectworkbench){
                            DoEndSelectModeWorkbench(selectworkbenchcoord);
                            selectworkbench = false;
                        Console.WriteLine("endselection");
                        }
                    }
                    else {

                        selectworkbenchcoord = select;
                        if (!selectworkbench)
                        {
                            selectworkbench = true;
                            selectDraggedMouse = false;
                            selectStartPos = select;
                        }
                        else
                        {
                            Vec2 diffPixels = (select - selectStartPos) * new Vec2(EngineApp.Instance.VideoMode.X, EngineApp.Instance.VideoMode.Y);
                            if (Math.Abs(diffPixels.X) >= 3 || Math.Abs(diffPixels.Y) >= 3)
                            {
                                selectDraggedMouse = true;
                            }

                        }


                        Console.WriteLine("Selection @ " + test.getx() + " | " + test.gety());
                    }
                    #endregion
                }
                else if (test.getOPType() == opType.rotation)
                {
                    #region rotation
                    todoRotate = 0;
                    float angle = test.getx() - test.gety();
                    if (angle > 0) todoRotate += 1;
                    if (angle < 0) todoRotate -= 1;
                    Console.WriteLine("Trying to Rotate " + angle);
                    #endregion
                }
                else if (test.getOPType() == opType.click)
                {
                    #region click
                    #region vardef
                    Vec2 MousePos = Vec2.Zero;
                    MousePos.X = test.getx();
                    MousePos.Y = test.gety();
                    Console.WriteLine("tuioclick");
                    Button b;
                    #endregion
                    if (Controls.OfType<MenuWindow>().Count() == 1)
                    {
                        #region Menue
                        Controls.OfType<MenuWindow>().First().workbench_Click(MousePos);
                        #endregion
                    }
                    //else if (IsMouseInButtonArea(MousePos, (Button)hudControl.Controls["StatusNotificationTop"].Controls["Menu"]) && Controls.OfType<MenuWindow>().Count() == 0)
                    else if (hudControl.Controls["Strahlen"].Controls["Menu"].GetScreenRectangle().IsContainsPoint(MousePos) && Controls.OfType<MenuWindow>().Count() == 0)
                    {
                        #region Menue button
                        Console.WriteLine("inbuttonarea");
                        hudFunctions("Menu");
                        #endregion
                    }
                    else if(hudControl.Controls["SchereSteinPapier"].Visible && hudControl.Controls["SchereSteinPapier"].Controls["CloseButton"].GetScreenRectangle().IsContainsPoint(MousePos))
                    {
                        #region SchereSteinPapierClose
                        if (sspw != null) sspw.close();
                        #endregion
                    }
                    else if (hudControl.Controls["Strahlen"].Controls["ButtonArea"].Controls["ChangeRoom"].GetScreenRectangle().IsContainsPoint(MousePos))
                    {
                        #region SwitchButton
                        //veraendert die Position der Camera in den Endkampfraum
                        changeCameraPosition();
                        #endregion
                    }
                    else if (hudControl.Controls["BigMinimap"].Visible)
                    {
                        #region bigminimap
                        bigMinimapObj.workbench_Click(MousePos);
                        #endregion
                    }
                    else if (hudControl.Controls["Minimap"].Visible && hudControl.Controls["Minimap"].GetScreenRectangle().IsContainsPoint(MousePos) && !hudControl.Controls["BigMinimap"].Visible)
                    {
                        #region openminimap
                        DoOpenMinimap();
                        #endregion
                    }
                    else if (numPad.Visible && numPad.GetScreenRectangle().IsContainsPoint(MousePos))
                    {
                        #region numpad
                        // Alle Buttons des NumPads durchiterieren
                        foreach (Control c in numPad.Controls)
                        {
                            b = c as Button;
                            if (b != null && b.Visible && b.GetScreenRectangle().IsContainsPoint(MousePos))
                            {
                                if (b.Name == "Clear")
                                {
                                    NumPadClear_Click(b);
                                }
                                else if (b.Name == "Enter")
                                {
                                    NumPadEnter_Click(b);
                                }
                                else
                                {
                                    NumPadButton_Click(b);
                                }
                            }
                        }
                        #endregion
                    }
                    else if (hudControl.Controls["rechts"].Controls["ControlPanelControl"].Visible && hudControl.Controls["rechts"].Controls["ControlPanelControl"].GetScreenRectangle().IsContainsPoint(MousePos))
                    {
                        #region controlbuttons
                        // Alle Buttons für Alien/Spawner durchiterieren
                        foreach (Control c in hudControl.Controls["rechts"].Controls["ControlPanelControl"].Controls)
                        {
                            if (c != null && c.Visible && c.GetScreenRectangle().IsContainsPoint(MousePos))
                            {
                                ControlPanelButton_Click(int.Parse(c.Name.Substring("ControlPanelButton".Length)));
                            }
                        }
                        #endregion
                    }
                    else if (hudControl.Controls["ActiveArea"].GetScreenRectangle().IsContainsPoint(MousePos))
                    {
                        #region ActiveAreaClick
                        bool pickingSuccess = false;
                        Vec3 mouseMapPos = Vec3.Zero;
                        Unit mouseOnObject = null;
                        //get pick information
                        Ray ray = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                            MousePos);
                        if (!float.IsNaN(ray.Direction.X))
                        {
                            RayCastResult result = PhysicsWorld.Instance.RayCast(ray, (int)ContactGroup.CastOnlyContact);
                            if (result.Shape != null)
                            {
                                pickingSuccess = true;
                                mouseOnObject = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as Unit;
                                mouseMapPos = result.Position;
                            }
                        }

                        if (pickingSuccess)
                        {
                            //do tasks
                            if (TaskTargetChooseIndex != -1)
                            {
                                DoTaskTargetChooseTasks(mouseMapPos, mouseOnObject);
                            }
                        }
                        if (TaskTargetChooseIndex == -1)
                        {
                            ClearEntitySelection();
                        }
                        #endregion
                    }

                    //hudControl.Controls["rechts"].Controls["ControlPanelControl"].Visible
                    //EngineApp.Instance.DoMouseDown(EMouseButtons.Left);
                    //EngineApp.Instance.DoMouseUp(EMouseButtons.Left);
                    #endregion
                }
                else if (test.getOPType() == opType.iuhr || test.getOPType() == opType.guhr || test.getOPType() == opType.blitz || test.getOPType() == opType.unselect)
                {
                    bigMinimapObj.workbench_gestures(test.getOPType());
                }
            }
            return false;
        }
		
        protected override bool OnKeyDown(KeyEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnKeyDown(e);

            if (e.Key == EKeys.P)
            {
                mapDrawPathMotionMap = !mapDrawPathMotionMap;
            }

            // Alles auf Maximum (Rotation, Strom, Aliens)
            if (e.Key == EKeys.F4)
            {
                Computer.SetToMaximum();
            }

            // Spawntime an-/ausschalten
            if (e.Key == EKeys.F5)
            {
                Computer.noSpawnTime = !Computer.noSpawnTime;
            }

            // Alle Aliens selektieren
            if (e.Key == EKeys.F6)
            {
                SelectAllAliens();
            }

            if (e.Key == EKeys.F7)
            {
                Computer.SetAlienControlPaused();
            }

            if (e.Key == EKeys.F8)
            {
                ShowStatistic();
            }            

            if (e.Key == EKeys.F10)
            {
                isHeadtrackingActive = !isHeadtrackingActive;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnKeyUp(KeyEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnKeyUp(e);

            return base.OnKeyUp(e);
        }

        bool IsMouseInActiveArea()
        {
            if (!hudControl.Controls["Strahlen"].GetScreenRectangle().IsContainsPoint(MousePosition))
                if (!hudControl.Controls["Minimap"].Visible || !hudControl.Controls["Minimap"].GetScreenRectangle().IsContainsPoint(MousePosition))
                    if (!hudControl.Controls["rechts"].Visible || !hudControl.Controls["rechts"].GetScreenRectangle().IsContainsPoint(MousePosition))
                        if (!hudControl.Controls["links"].Visible || !hudControl.Controls["links"].GetScreenRectangle().IsContainsPoint(MousePosition))
                            return true;
            return false;
        }

        //Pommes
        bool IsMouseInButtonArea(Vec2 Pos, Button button)
        {
            if (!button.GetScreenRectangle().IsContainsPoint(Pos))
                
                return false;
            return true;
        }

        /// <summary>
        /// Hier alles reinpacken, wenn ein neues Window geöffnet wird, wo der Client nicht pausieren soll, wo also nur die Visibility verändert wird.
        /// </summary>
        /// <returns></returns>
        private bool IsExtraWindowOpened()
        {
            return hudControl.Controls["SchereSteinPapier"].Visible || hudControl.Controls["BigMinimap"].Visible;
        }

        protected override bool OnMouseDown(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
            {
                return base.OnMouseDown(button);
            }
            // Mouse click for select unit
            if ( !IsExtraWindowOpened() )
            {
                if (button == EMouseButtons.Left && IsMouseInActiveArea() && TaskTargetChooseIndex == -1)
                {
                    selectMode = true;
                    selectDraggedMouse = false;
                    selectStartPos = EngineApp.Instance.MousePosition;
                    return true;
                }

                //minimap mouse change camera position
                // läuft auch über workbench code
                //if (button == EMouseButtons.Left && taskTargetChooseIndex == -1)
                //{
                //    if (minimapControl.GetScreenRectangle().IsContainsPoint(MousePosition))
                //    {
                //        minimapClick = true;
                //        return true;
                //    }
                //}
            }

            // tuio mapping
            TuioInputDeviceSpecialEvent customEvent =
                        new TuioInputDeviceSpecialEvent(new TuioInputDevice("muh"), opType.click, MousePosition.X, MousePosition.Y);
            InputDeviceManager.Instance.SendEvent(customEvent);

            return base.OnMouseDown(button);
        }

        protected override bool OnMouseUp(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseUp(button);

            //do tasks
            if ((button == EMouseButtons.Right || button == EMouseButtons.Left) && (!FreeCameraMouseRotating || !EngineApp.Instance.MouseRelativeMode))
            {
                bool pickingSuccess = false;
                Vec3 mouseMapPos = Vec3.Zero;
                Unit mouseOnObject = null;

                //pick on active area
                if (IsMouseInActiveArea())
                {
                    //get pick information
                    Ray ray = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                        EngineApp.Instance.MousePosition);
                    if (!float.IsNaN(ray.Direction.X))
                    {
                        RayCastResult result = PhysicsWorld.Instance.RayCast(ray,
                            (int)ContactGroup.CastOnlyContact);
                        if (result.Shape != null)
                        {
                            pickingSuccess = true;
                            mouseOnObject = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as Unit;
                            mouseMapPos = result.Position;
                        }
                    }
                }

                if (pickingSuccess)
                {
                    //do tasks
                    if (TaskTargetChooseIndex != -1)
                    {
                        if (button == EMouseButtons.Left)
                            DoTaskTargetChooseTasks(mouseMapPos, mouseOnObject);
                        if (button == EMouseButtons.Right)
                            TaskTargetChooseIndex = -1;
                    }
                    else
                    {
                        if (button == EMouseButtons.Right)
                            DoRightClickTasks(mouseMapPos, mouseOnObject);
                    }
                }
            }

            //select mode
            if (selectMode && button == EMouseButtons.Left)
                DoEndSelectMode();

            //minimap mouse change camera position
            //if (minimapClick)
            //{
            //    minimapClick = false;
            //    DoOpenMinimap();
            //}

            return base.OnMouseUp(button);
        }

        bool IsEnableTaskTypeInTasks(List<AlienUnitAI.UserControlPanelTask> tasks, AlienUnitAI.Task.Types taskType)
        {
            if (tasks == null)
                return false;
            foreach (AlienUnitAI.UserControlPanelTask task in tasks)
                if (task.Task.Type == taskType && task.Enable)
                    return true;
            return false;
        }

        void DoTaskTargetChooseTasks(Vec3 mouseMapPos, Unit mouseOnObject)
        {
            //Do task after task target choose
            bool toQueue = EngineApp.Instance.IsKeyPressed(EKeys.Shift);

            List<AlienUnitAI.UserControlPanelTask> tasks = GetControlTasks();
            int index = TaskTargetChooseIndex;

            if (tasks != null && index < tasks.Count && tasks[index].Enable)
            {
                foreach (Unit unit in selectedUnits)
                {
                    AlienUnitAI intellect = unit.Intellect as AlienUnitAI;
                    if (intellect == null)
                        continue;

                    AlienUnitAI.Task.Types taskType = tasks[index].Task.Type;

                    List<AlienUnitAI.UserControlPanelTask> aiTasks = intellect.GetControlPanelTasks();

                    if (!IsEnableTaskTypeInTasks(aiTasks, taskType))
                        continue;

                    switch (taskType)
                    {
                        //Patrol, Move, Attack, Repair
                        
                        case AlienUnitAI.Task.Types.Patrol:
                        case AlienUnitAI.Task.Types.Move:
                        case AlienUnitAI.Task.Types.Attack:
                        //case AlienUnitAI.Task.Types.Repair:
                            if (mouseOnObject != null)
                                intellect.DoTask(new AlienUnitAI.Task(taskType, mouseOnObject), toQueue);
                            else
                            {
                                if (taskType == AlienUnitAI.Task.Types.Move)
                                {
                                    intellect.DoTask(new AlienUnitAI.Task(taskType, mouseMapPos), toQueue);
                                }

                                if(taskType == AlienUnitAI.Task.Types.Patrol)
                                    intellect.DoTask(new AlienUnitAI.Task(taskType), toQueue);

                                if (taskType == AlienUnitAI.Task.Types.Attack )//|| taskType == AlienUnitAI.Task.Types.Repair)
                                {
                                    intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.BreakableMove, mouseMapPos), toQueue);
                                }
                            }
                            break;
                    }
                }
            }
            TaskTargetChooseIndex = -1;
        }

        void DoRightClickTasks(Vec3 mouseMapPos, Unit mouseOnObject)
        {
            bool toQueue = EngineApp.Instance.IsKeyPressed(EKeys.Shift);

            foreach (Unit unit in selectedUnits)
            {
                AlienUnitAI intellect = unit.Intellect as AlienUnitAI;
                if (intellect == null)
                    continue;

                //if (intellect.Faction != playerFaction)
                //    continue;

                List<AlienUnitAI.UserControlPanelTask> tasks = intellect.GetControlPanelTasks();

                if (mouseOnObject != null)
                {
                    bool tasked = false;

                    
                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Patrol) &&
                      mouseOnObject.Intellect != null &&
                      intellect.Faction != null && mouseOnObject.Intellect.Faction != null &&
                      intellect.Faction == mouseOnObject.Intellect.Faction)
                    {
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Patrol,
                            mouseOnObject), toQueue);
                        tasked = true;
                    }

                                     

                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Attack) &&
                        mouseOnObject.Intellect != null &&
                        intellect.Faction != null && mouseOnObject.Intellect.Faction != null &&
                        intellect.Faction != mouseOnObject.Intellect.Faction)
                    {
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Attack,
                            mouseOnObject), toQueue);
                        tasked = true;
                    }

                    /*
                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Repair) &&
                        mouseOnObject.Intellect != null && intellect.Faction != null && mouseOnObject.Intellect.Faction != null &&
                        intellect.Faction == mouseOnObject.Intellect.Faction)
                    {
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Repair, mouseOnObject), toQueue);
                        tasked = true;
                    }
                     */

                    if (!tasked && IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Move))
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Move, mouseOnObject), toQueue);
                }
                else
                {
                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Move))
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Move, mouseMapPos), toQueue);
                }
            }
        }

        void DoEndSelectMode()
        {
            selectMode = false;

            List<AlienUnit> areaObjs = new List<AlienUnit>();
            {
                if (selectDraggedMouse)
                {
                    Rect rect = new Rect(selectStartPos);
                    rect.Add(EngineApp.Instance.MousePosition);

                    Map.Instance.GetObjectsByScreenRectangle(RendererWorld.Instance.DefaultCamera, rect,
                        MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject obj)
                        {
                            if (obj is AlienUnit)
                            {
                                AlienUnit unit = (AlienUnit)obj;
                                areaObjs.Add(unit);
                            }
                        });
                }
                else
                {
                    Ray ray = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                        EngineApp.Instance.MousePosition);

                    RayCastResult result = PhysicsWorld.Instance.RayCast(ray,
                        (int)ContactGroup.CastOnlyContact);
                    if (result.Shape != null)
                    {
                        AlienUnit unit = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as AlienUnit;
                        if (unit != null)
                            areaObjs.Add(unit);
                    }
                }
            }

            //do select/unselect
            if (!EngineApp.Instance.IsKeyPressed(EKeys.Shift))
                ClearEntitySelection();

            if (areaObjs.Count == 0)
                return;

            if (!selectDraggedMouse && EngineApp.Instance.IsKeyPressed(EKeys.Shift))
            {
                //unselect
                foreach (AlienUnit obj in areaObjs)
                {
                    if (selectedUnits.Contains(obj))
                    {
                        SetEntitySelected(obj, false);
                        return;
                    }
                }
            }

            // alle objekte aus der area durchgehen
            // nur die als selected hinzunehmen, die auch...
            foreach (AlienUnit obj in areaObjs)
            {
                SetEntitySelected(obj, true);
            }
        }
       
        void DoEndSelectModeWorkbench(Vec2 mousepos)
        {

            List<AlienUnit> areaObjs = new List<AlienUnit>();
            {
                if (selectDraggedMouse)
                {
                    Rect rect = new Rect(selectStartPos);
                    rect.Add(mousepos);

                    Map.Instance.GetObjectsByScreenRectangle(RendererWorld.Instance.DefaultCamera, rect,
                        MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject obj)
                        {
                            if (obj is AlienUnit)
                            {
                                AlienUnit unit = (AlienUnit)obj;
                                areaObjs.Add(unit);
                            }
                        });
                }
                else
                {
                    Ray ray = RendererWorld.Instance.DefaultCamera.GetCameraToViewportRay(
                        mousepos);

                    RayCastResult result = PhysicsWorld.Instance.RayCast(ray,
                        (int)ContactGroup.CastOnlyContact);
                    if (result.Shape != null)
                    {
                        AlienUnit unit = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as AlienUnit;
                        if (unit != null)
                            areaObjs.Add(unit);
                    }
                }
            }


            if (areaObjs.Count == 0)
                return;

            // alle objekte aus der area durchgehen
            // nur die als selected hinzunehmen, die auch...
            foreach (AlienUnit obj in areaObjs)
            {
                SetEntitySelected(obj, true);
            }
        }
        /// <summary>
        /// Alle Aliens auswählen
        /// </summary>
        private void SelectAllAliens()
        {
            IEnumerable<Alien> aliens = Entities.Instance.EntitiesCollection.OfType<Alien>();
            foreach (Alien alien in aliens)
            {
                SetEntitySelected(alien, true);
            }
        }

        protected override bool OnMouseDoubleClick(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseDoubleClick(button);

            return base.OnMouseDoubleClick(button);
        }

        protected override void OnMouseMove()
        {
            base.OnMouseMove();

            //If atop openly any window to not process
            if (Controls.Count != 1)
                return;

            //select mode
            if (selectMode)
            {
                Vec2 diffPixels = (MousePosition - selectStartPos) * new Vec2(EngineApp.Instance.VideoMode.X, EngineApp.Instance.VideoMode.Y);
                if (Math.Abs(diffPixels.X) >= 3 || Math.Abs(diffPixels.Y) >= 3)
                {
                    selectDraggedMouse = true;
                }
            }

            //minimap mouse change camera position TODO
            //if (minimapClick)
            //    cameraPosition = GetMapPositionByMouseOnMinimap();
        }

        protected override void OnTick(float delta)
        {
            mydelta = delta;
            base.OnTick(delta);

            // Status Notification Top aktualisieren
            UpdateStatusNotificationTop(delta);
            // Status Nachrichten löschen
            UpdateStatusMessage(delta);

            // Drop-Items aktualisieren
            UpdateDropItems(delta);

            //If atop openly any window to not process
            if (Controls.Count != 1)
                return;

            //Endkampfraum-Minimap ausblenden/ Map-Minimap einblenden
            if (endkampfraum)
            {
                hudControl.Controls["Minimap"].Visible = false;
            }
            else
            {
                hudControl.Controls["Minimap"].Visible = true;
            }

            //Remove deleted selected objects
            for (int n = 0; n < selectedUnits.Count; n++)
            {
                if (selectedUnits[n].IsSetForDeletion)
                {
                    selectedUnits.RemoveAt(n);
                    n--;
                }
            }
            if (selectedUnits.Count != 0)
            {
                hudControl.Controls["rechts"].Visible = true;
                if (selectedUnits.OfType<Alien>().Count() != 0){
                    hudControl.Controls["links"].Visible = true;
                } else{
                    hudControl.Controls["links"].Visible = false;
                }             
            } else
            {
                hudControl.Controls["rechts"].Visible = false;
                hudControl.Controls["links"].Visible = false;
            }

            if (!FreeCameraMouseRotating)
                EngineApp.Instance.MouseRelativeMode = false;

            bool activeConsole = EngineConsole.Instance != null && EngineConsole.Instance.Active;

            if (GetRealCameraType() == CameraType.Game && !activeConsole)
            {
                //if (todoRotate != 0)
                //{
                //    if (todoRotate > 0) {
                //        cameraDirection.Horizontal += (delta * 2) * 2;
                //    }
                //    else if (todoRotate < 0) {
                //        cameraDirection.Horizontal -= (delta * 2) * 2;
                //    }
                    
                //    if (cameraDirection.Horizontal >= MathFunctions.PI * 2)
                //        cameraDirection.Horizontal -= MathFunctions.PI * 2;
                //    if (cameraDirection.Horizontal < 0)
                //        cameraDirection.Horizontal += MathFunctions.PI * 2;
                //    todoRotate = 0;

                //}
                if (EngineApp.Instance.IsKeyPressed(EKeys.Q))
                {
                    //cameraDirection.Horizontal += delta * 2;
                    //if (cameraDirection.Horizontal >= MathFunctions.PI * 2)
                    //    cameraDirection.Horizontal -= MathFunctions.PI * 2;
                    todoRotate += 1;
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.E))
                {

                    todoRotate -= 1;
                    //cameraDirection.Horizontal -= delta * 2;
                    //if (cameraDirection.Horizontal < 0)
                    //    cameraDirection.Horizontal += MathFunctions.PI * 2;
                }

                //change cameraPosition
                if (!selectMode && Time > 2)
                {
                    Vec2 vector = Vec2.Zero;
                    //if (todoTranslate != Vec2.Zero) {
                    //    vector.X -= todoTranslate.X*15;
                    //    vector.Y += todoTranslate.Y*15;
                    //    todoTranslate = Vec2.Zero;
                    //}
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Left) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.A))
                    {
                        vector.X--;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Right) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.D))
                    {
                        vector.X++;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Up) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.W))
                    {
                        vector.Y++;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Down) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.S))
                    {
                        vector.Y--;
                    }

                    if (vector != Vec2.Zero)
                    {
                        //rotate vector
                        //float angle = MathFunctions.ATan(-vector.Y, vector.X) +
                        //    cameraDirection.Horizontal;
                        //vector = new Vec2(MathFunctions.Sin(angle), MathFunctions.Cos(angle));

                        todoTranslate += vector * delta * 5;
                        //if (CheckMapPosition(neueCameraPosition)) { 
                        //    cameraPosition = neueCameraPosition;
                        //}
                    }
                }

            }
            

            //gameStatus
            //if (string.IsNullOrEmpty(hudControl.Controls["GameStatus"].Text))
            if (Computer.Alienwin != Computer.Astronautwin)
            {
                if (Computer.Alienwin)
                {
                    hudControl.Controls["Statistic"].Controls["Status"].Text = "Sieger";
                } else {
                    hudControl.Controls["Statistic"].Controls["Status"].Text = "Verlierer";
                }
            }
            else
            {
                hudControl.Controls["Statistic"].Controls["Status"].Text = "";
            }
        }

        protected override void OnRender()
        {
            base.OnRender();

            if (GridBasedNavigationSystem.Instances.Count != 0)
                GridBasedNavigationSystem.Instances[0].AlwaysDrawGrid = mapDrawPathMotionMap;

            UpdateHUD();
        }

        void UpdateHUD()
        {
            Camera camera = RendererWorld.Instance.DefaultCamera;

            hudControl.Visible = EngineDebugSettings.DrawGui;

            //Computer station status notification (bottom)
            //{
            //    float stationStatus = Computer.GetStationStatus();

            //    Control healthBar = hudControl.Controls["StationStatusBar"];
            //    Vec2 originalSize = new Vec2(256, 32);
            //    Vec2 interval = new Vec2(117, 304);
            //    float sizeX = (117 - 82) + stationStatus * (interval[1] - interval[0]);
            //    healthBar.Size = new ScaleValue(ScaleType.ScaleByResolution, new Vec2(sizeX, originalSize.Y));
            //    healthBar.BackTextureCoord = new Rect(0, 0, sizeX / originalSize.X, 1);
            //}

            // AlienIcon
            /*
            {
                Control alienIcon = hudControl.Controls["StatusNotificationTop"].Controls["AlienIconBox"].Controls["AlienIcon"];
                string fileName = "Gui\\HUD\\Icons\\Alien.png";
                alienIcon.BackTexture = TextureManager.Instance.Load(fileName, Texture.Type.Type2D, 0);
            }
            */

            // Computer status notifications (top)
            /*
            {
                hudControl.Controls["StatusNotificationTop"].Controls["AlienIconBox"].Controls["AlienCount"].Controls["AlienCountActive"].Text = "" + Computer.UsedAliens;
                hudControl.Controls["StatusNotificationTop"].Controls["AlienIconBox"].Controls["AlienCount"].Controls["AlienCountPossible"].Text = "" + Computer.AvailableAliens;
                hudControl.Controls["StatusNotificationTop"].Controls["RotationIcon"].Controls["RotationCouponCount"].Text = "" + Computer.RotationCoupons;
                hudControl.Controls["StatusNotificationTop"].Controls["EnergyIcon"].Controls["EnergyCouponCount"].Text = "" + Computer.PowerCoupons;
                hudControl.Controls["StatusNotificationTop"].Controls["ExperienceIcon"].Controls["ExperienceCount"].Text = "" + Computer.ExperiencePoints;
            }
            */
            {
                hudControl.Controls["Strahlen"].Controls["AktuelleAlien"].Controls["AlienCountActive"].Text = "" + Computer.UsedAliens;
                hudControl.Controls["Strahlen"].Controls["MaximalAlien"].Controls["AlienCountPossible"].Text = "" + Computer.AvailableAliens;
                hudControl.Controls["Strahlen"].Controls["Rotation"].Controls["RotationCouponCount"].Text = "" + Computer.RotationCoupons;
                hudControl.Controls["Strahlen"].Controls["LightSwitch"].Controls["EnergyCouponCount"].Text = "" + Computer.PowerCoupons;
                hudControl.Controls["Strahlen"].Controls["ExperienceCount"].Text = "" + Computer.ExperiencePoints;
                
                //Anpassung der AlienHUD (rechter Bereich) an die Aufloesung

                Vec2I resolution = EngineApp.Instance.VideoMode;
                float ResSize = resolution.Y;

                hudControl.Controls["Strahlen"].Size = new ScaleValue(Control.ScaleType.Pixel, new Vec2(ResSize * 0.35f, ResSize * 0.3f));
                hudControl.Controls["Strahlen"].Position = new ScaleValue(Control.ScaleType.Pixel, new Vec2(resolution.X -
                    hudControl.Controls["Strahlen"].Size.Value.X, resolution.Y - hudControl.Controls["Strahlen"].Size.Value.Y));

                hudControl.Controls["rechts"].Size = new ScaleValue(Control.ScaleType.Pixel, new Vec2(ResSize * 0.35f, ResSize));
                hudControl.Controls["rechts"].Position = new ScaleValue(Control.ScaleType.Pixel, new Vec2(resolution.X -
                    hudControl.Controls["rechts"].Size.Value.X,0));

               
            }


            // Astronauten
            /*
            {
                hudControl.Controls["StatusNotificationTop"].Controls["AstronautIcon"].Controls["AstronautCount"].Text = "" + Computer.GetNumberOfActiveAstronauts();
            }
             */

            Vec3 mouseMapPos = Vec3.Zero;
            Unit mouseOnObject = null;

            bool pickingSuccess = false;

            if (!EngineApp.Instance.MouseRelativeMode)
            {
                Ray ray = camera.GetCameraToViewportRay(EngineApp.Instance.MousePosition);
                if (!float.IsNaN(ray.Direction.X))
                {
                    RayCastResult result = PhysicsWorld.Instance.RayCast(ray,
                        (int)ContactGroup.CastOnlyContact);
                    if (result.Shape != null)
                    {
                        pickingSuccess = true;
                        mouseOnObject = MapSystemWorld.GetMapObjectByBody(result.Shape.Body) as AlienUnit;
                        mouseMapPos = result.Position;
                    }
                }

                if (selectMode && selectDraggedMouse)
                {
                    Rect rect = new Rect(selectStartPos);
                    rect.Add(EngineApp.Instance.MousePosition);

                    Map.Instance.GetObjectsByScreenRectangle(RendererWorld.Instance.DefaultCamera, rect,
                        MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject obj)
                        {
                            Unit unit = (Unit)obj;

                            camera.DebugGeometry.Color = new ColorValue(1, 1, 0);
                            Bounds bounds = obj.MapBounds;
                            bounds.Expand(.1f);
                            camera.DebugGeometry.AddBounds(bounds);
                        });
                }
                else if (selectDraggedMouse && selectworkbench)
                {
                    Rect rect = new Rect(selectStartPos);
                    rect.Add(selectworkbenchcoord);

                    Map.Instance.GetObjectsByScreenRectangle(RendererWorld.Instance.DefaultCamera, rect,
                        MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject obj)
                        {
                            Unit unit = (Unit)obj;

                            camera.DebugGeometry.Color = new ColorValue(1, 1, 0);
                            Bounds bounds = obj.MapBounds;
                            bounds.Expand(.1f);
                            camera.DebugGeometry.AddBounds(bounds);
                        });
                }
                else
                {
                    // Wenn nichts ausgewählt ist
                    if (pickingSuccess && IsMouseInActiveArea())
                    {
                        // Befindet sich die Maus auf einem AlienUnit-Objekt?
                        if (mouseOnObject != null)
                        {
                            // Dann zeichnen wir einen Quader um das Objekt
                            //camera.DebugGeometry.Color = new ColorValue(1, 1, 0);
                            camera.DebugGeometry.Color = GetColor(mouseOnObject);

                            Bounds bounds = mouseOnObject.MapBounds;
                            bounds.Expand(.1f);
                            camera.DebugGeometry.AddBounds(bounds);
                        }
                        else
                        {
                            // ansonsten passiert nichts?!
                            camera.DebugGeometry.Color = new ColorValue(1, 0, 0);
                            camera.DebugGeometry.AddSphere(new Sphere(mouseMapPos, .4f), 16);
                        }
                    }
                }
            }

            //objects selected
            foreach (Unit unit in selectedUnits)
            {
                ColorValue color = GetColor(unit);
                camera.DebugGeometry.Color = color;
                camera.DebugGeometry.AddBounds(unit.MapBounds);
            }

            //Selected units HUD
            {
                //string text = "";
                if (selectedUnits.Count >= 1)
                {
                    if (selectedUnits.OfType<Alien>().Count() != 0)
                    {
                        int zahl = 0;
                        foreach (Alien alien in selectedUnits.OfType<Alien>())
                        {
                            Control control = hudControl.Controls["links"].Controls["Alienanzeige" + zahl.ToString()];;

                            if (control == null)
                                break;

                            control.Visible = true;

                            Control healthBar = control.Controls["StatusAlien"];
                            float sizeX = alien.Health / alien.Type.HealthMax;
                            if (sizeX < 0.3f)
                            {
                                healthBar.BackColor = new ColorValue(255, 0, 0);
                            }
                            else
                            {
                                healthBar.BackColor = new ColorValue(0, 255, 0);
                            }
                            healthBar.Size = new ScaleValue(ScaleType.Parent, new Vec2(sizeX, 0.2f));
                            zahl++;
                        }
                    }
                }
                else
                {
                    foreach(Control control in hudControl.Controls["links"].Controls)
                    {
                        control.Visible = false;
                    }
                }
                //text += unit.ToString();
                //if (unit is Alien)
                //{
                //text += string.Format(": {0:0%}", unit.Health / unit.Type.HealthMax);//.HealthFactorAtBeginning
                //}
                //text += "\n";

                //hudControl.Controls["SelectedUnitsInfo"].Text = text;
                //hudControl.Controls["links"].Controls["SelectedUnitsInfo"].Text = text;

                UpdateControlPanel();
            }

            UpdateHUDControlIcon();
        }

        void UpdateStatusNotificationTop(float delta)
        {
            timeForUpdateNotificationStatus -= delta;
            if (timeForUpdateNotificationStatus < 0) //time to do it
            {
                timeForUpdateNotificationStatus = 60;
                Computer.IncrementAvailableAliens();
                Computer.IncrementPowerCoupons();
                Computer.IncrementRotationCoupons();
            }
        }

        void UpdateStatusMessage(float delta)
        {
            timeForDeleteNotificationMessage -= delta;
            if (timeForDeleteNotificationMessage < 0)
            {
                hudControl.Controls["ActiveArea"].Controls["StatusMessage"].Text = "";
            }
        }

        void UpdateDropItems(float delta)
        {
            timeForDropItemIncrementation -= delta;
            if (timeForDropItemIncrementation < 0)
            {
                Computer.IncrementMaxItemDropGroupNr();
                timeForDropItemIncrementation = 300;
            }
        }

        /// <summary>
        /// Farbe für kleine Aliens und Spawnpoint verschieden
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        ColorValue GetColor(Dynamic unit)
        {
            if (unit is Alien)
            {
                return new ColorValue(1, 1, 0);
            }
            else if (unit is AlienSpawner)
            {
                return new ColorValue(0, 1, 0);
            } 
            else if (unit is Terminal) // Hier werden Sigale farblich hervorgehoben
            {
                return new ColorValue(1, 0, 0);
            }
            return new ColorValue();
        }

        List<AlienUnitAI.UserControlPanelTask> GetControlTasks()
        {
            List<AlienUnitAI.UserControlPanelTask> tasks = null;
            if (selectedUnits.Count != 0)
            {
                IEnumerable<Alien> alienList = selectedUnits.OfType<Alien>();
                IEnumerable<AlienSpawner> alienSpawnerList = selectedUnits.OfType<AlienSpawner>();
                if (alienList.Count() != 0)
                {
                    foreach (Alien a in alienList)
                    {
                        List<AlienUnitAI.UserControlPanelTask> t = (a.Intellect as AlienUnitAI).GetControlPanelTasks();
                        if (tasks == null)
                        {
                            tasks = t;
                        }
                        else
                        {
                            for (int n = 0; n < tasks.Count; n++)
                            {
                                if (n >= t.Count)
                                    continue;// break??

                                if (tasks[n].Task.Type != t[n].Task.Type)
                                    continue;

                                if (t[n].Active)
                                {
                                    tasks[n] = new AlienUnitAI.UserControlPanelTask(tasks[n].Task, true, tasks[n].Enable);
                                }
                            }
                        }
                    }


                    //AlienAI aai = alienList.ElementAt(0).Intellect as AlienAI;
                    //tasks = aai.GetControlPanelTasks();
                }
                else if (alienSpawnerList.Count() != 0)
                {
                    foreach (AlienSpawner a in alienSpawnerList)
                    {
                        List<AlienUnitAI.UserControlPanelTask> t = (a.Intellect as AlienUnitAI).GetControlPanelTasks();
                        if (tasks == null)
                        {
                            tasks = t;
                        }
                        else
                        {
                            for (int n = 0; n < tasks.Count; n++)
                            {
                                if (n >= t.Count)
                                    continue;// break??

                                if (tasks[n].Task.Type != t[n].Task.Type)
                                    continue;

                                if (t[n].Active)
                                {
                                    tasks[n] = new AlienUnitAI.UserControlPanelTask(tasks[n].Task, true, tasks[n].Enable);
                                }

                                if (tasks[n].Task.Type == AlienUnitAI.Task.Types.ProductUnit)
                                {
                                    if (tasks[n].Task.EntityType != t[n].Task.EntityType)
                                        tasks[n] = new AlienUnitAI.UserControlPanelTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.None));
                                }
                            }
                        }
                    }
                    //AlienSpawnerAI aai = alienSpawnerList.ElementAt(0).Intellect as AlienSpawnerAI;
                    //tasks = aai.GetControlPanelTasks();
                }

                


                //foreach (Unit unit in selectedUnits)
                //{
                //    AlienUnitAI intellect = unit.Intellect as AlienUnitAI;
                //    if (intellect != null)
                //    {
                //        List<AlienUnitAI.UserControlPanelTask> t = intellect.GetControlPanelTasks();
                //        if (tasks == null)
                //        {
                //            tasks = t;
                //        }
                //        else
                //        {
                //            for (int n = 0; n < tasks.Count; n++)
                //            {
                //                if (n >= t.Count)
                //                    continue;// break??

                //                if (tasks[n].Task.Type != t[n].Task.Type)
                //                    continue;
                //                if (t[n].Active)
                //                {
                //                    tasks[n] = new AlienUnitAI.UserControlPanelTask(
                //                        tasks[n].Task, true, tasks[n].Enable);
                //                }

                //                if (tasks[n].Task.Type == AlienUnitAI.Task.Types.ProductUnit)
                //                {
                //                    if (tasks[n].Task.EntityType != t[n].Task.EntityType)
                //                        tasks[n] = new AlienUnitAI.UserControlPanelTask(
                //                            new AlienUnitAI.Task(AlienUnitAI.Task.Types.None));
                //                }
                //            }
                //        }
                //    }
                //}
            }
            return tasks;
        }

        void NumPadClear_Click(Button sender)
        {
            numPad.Controls["Output"].Text = "";
        }

        void NumPadEnter_Click(Button sender)
        {
            // 1. Task des AlienSpawners 
            ControlPanelButton_Click(0);
        }

        void NumPadButton_Click(Button sender)
        {
            if (numPad.Controls["Output"].Text.Length < 2)
            {
                numPad.Controls["Output"].Text += "" + sender.Text;
            }
            else
            {
                // Meldung anzeigen
                StatusMessageHandler.sendMessage("Nur zweistellige Zahlen möglich");
            }
        }

        // Wenn ein Button zum Navigieren der Aliens/Spawnpoints geklickt wurde
        void ControlPanelButton_Click(int index)
        {
            TaskTargetChooseIndex = -1;

            List<AlienUnitAI.UserControlPanelTask> tasks = GetControlTasks();

            if (tasks == null || index >= tasks.Count)
                return;
            if (!tasks[index].Enable)
                return;

            AlienUnitAI.Task.Types taskType = tasks[index].Task.Type;
            switch (taskType)
            {
                //Stop, SelfDestroy
                case AlienUnitAI.Task.Types.Stop:
                case AlienUnitAI.Task.Types.SelfDestroy:
                    foreach (Unit unit in selectedUnits)
                    {
                        AlienUnitAI intellect = unit.Intellect as AlienUnitAI;
                        if (intellect == null)
                            continue;

                        if (IsEnableTaskTypeInTasks(intellect.GetControlPanelTasks(), taskType))
                            intellect.DoTask(new AlienUnitAI.Task(taskType), false);
                    }
                    break;

                //ProductUnit
                case AlienUnitAI.Task.Types.ProductUnit:
                    foreach (Unit unit in selectedUnits)
                    {
                        AlienSpawnerAI intellect = unit.Intellect as AlienSpawnerAI;
                        if (intellect == null)
                            continue;

                        if (IsEnableTaskTypeInTasks(intellect.GetControlPanelTasks(), taskType))
                        {
                            try
                            {
                                spawnNumber = int.Parse(numPad.Controls["Output"].Text);
                                intellect.DoTask(new AlienUnitAI.Task(taskType, tasks[index].Task.EntityType, spawnNumber), false);
                            }
                            catch (FormatException e)
                            {
                                StatusMessageHandler.sendMessage("Keine gültige Zahl");
                            }
                        }
                    }
                    break;

                //Patrol, Suicide
                case AlienUnitAI.Task.Types.Patrol:
                case AlienUnitAI.Task.Types.Suicide:
                    foreach (Unit unit in selectedUnits)
                    {
                        AlienAI intellect = unit.Intellect as AlienAI;
                        if (intellect == null)
                            continue;

                        if (IsEnableTaskTypeInTasks(intellect.GetControlPanelTasks(), taskType))
                        {
                            intellect.DoTask(new AlienUnitAI.Task(taskType), false);
                        }
                    }
                    break;

                // Move, Attack
                case AlienUnitAI.Task.Types.Move:
                case AlienUnitAI.Task.Types.Attack:
                    //do taskTargetChoose
                    TaskTargetChooseIndex = index;
                    break;

            }
        }


        // Buttons zum Durchführen der Alien/AlienSpawner/Computer Logik
        void UpdateControlPanel()
        {
            List<AlienUnitAI.UserControlPanelTask> tasks = GetControlTasks();

            //check for need reset taskTargetChooseIndex
            if (TaskTargetChooseIndex != -1)
            {
                if (tasks == null || TaskTargetChooseIndex >= tasks.Count || !tasks[TaskTargetChooseIndex].Enable)
                    TaskTargetChooseIndex = -1;
            }

            // Controls fürs Spawning zunächst verstecken
            numPad.Visible = false;

            // make all buttons for AlienUnit visible or not
            // Für Spawner:
            if (tasks != null && tasks.Count >= 0 && tasks[0].Task.EntityType != null && tasks[0].Task.EntityType.FullName == "Alien")
            {
                numPad.Visible = true;
            }
            // Für Alien: 
            for (int n = 0; ; n++)
            {
                Control control = hudControl.Controls["rechts"].Controls["ControlPanelControl"].Controls["ControlPanelButton" + n.ToString()];

                if (control == null)
                    break;

                control.Visible = tasks != null && n < tasks.Count;

                if (control.Visible)
                {
                    // Wenn Kein Task vorhanden oder wenn Spawner-Task, dann Button verstecken
                    if (tasks[n].Task.Type == AlienUnitAI.Task.Types.None || (tasks[n].Task.EntityType != null && tasks[0].Task.EntityType.FullName == "Alien"))
                    {
                        control.Visible = false;
                    }
                }

                if (control.Visible)
                {
                    control.Enable = tasks[n].Enable;

                    if (n == TaskTargetChooseIndex)
                        control.ColorMultiplier = new ColorValue(0, 1, 0);
                    else if (tasks[n].Active)
                        control.ColorMultiplier = new ColorValue(1, 1, 0);
                    else
                        control.ColorMultiplier = new ColorValue(1, 1, 1);
                }
            }
        }

        // Rechtecke zeichnen für ausgewählte Elemente
        void DrawHUD(GuiRenderer renderer)
        {
            if (selectMode && selectDraggedMouse)
            {
                Rect rect = new Rect(selectStartPos);
                rect.Add(EngineApp.Instance.MousePosition);

                Vec2I windowSize = EngineApp.Instance.VideoMode;
                Vec2 thickness = new Vec2(1.0f / (float)windowSize.X, 1.0f / (float)windowSize.Y);

                renderer.AddQuad(new Rect(rect.Left, rect.Top + thickness.Y,
                    rect.Right, rect.Top + thickness.Y * 2), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Left, rect.Bottom,
                    rect.Right, rect.Bottom + thickness.Y), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Left + thickness.X, rect.Top,
                    rect.Left + thickness.X * 2, rect.Bottom), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Right, rect.Top,
                    rect.Right + thickness.X, rect.Bottom), new ColorValue(0, 0, 0, .5f));

                renderer.AddQuad(new Rect(rect.Left, rect.Top,
                    rect.Right, rect.Top + thickness.Y), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Left, rect.Bottom - thickness.Y,
                    rect.Right, rect.Bottom), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Left, rect.Top,
                    rect.Left + thickness.X, rect.Bottom), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Right - thickness.X, rect.Top,
                    rect.Right, rect.Bottom), new ColorValue(0, 1, 0, 1));
            } else if (selectworkbench && selectDraggedMouse)
            {
                Rect rect = new Rect(selectStartPos);
                rect.Add(selectworkbenchcoord);

                Vec2I windowSize = EngineApp.Instance.VideoMode;
                Vec2 thickness = new Vec2(1.0f / (float)windowSize.X, 1.0f / (float)windowSize.Y);

                renderer.AddQuad(new Rect(rect.Left, rect.Top + thickness.Y,
                    rect.Right, rect.Top + thickness.Y * 2), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Left, rect.Bottom,
                    rect.Right, rect.Bottom + thickness.Y), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Left + thickness.X, rect.Top,
                    rect.Left + thickness.X * 2, rect.Bottom), new ColorValue(0, 0, 0, .5f));
                renderer.AddQuad(new Rect(rect.Right, rect.Top,
                    rect.Right + thickness.X, rect.Bottom), new ColorValue(0, 0, 0, .5f));

                renderer.AddQuad(new Rect(rect.Left, rect.Top,
                    rect.Right, rect.Top + thickness.Y), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Left, rect.Bottom - thickness.Y,
                    rect.Right, rect.Bottom), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Left, rect.Top,
                    rect.Left + thickness.X, rect.Bottom), new ColorValue(0, 1, 0, 1));
                renderer.AddQuad(new Rect(rect.Right - thickness.X, rect.Top,
                    rect.Right, rect.Bottom), new ColorValue(0, 1, 0, 1));
            }
        }

        //Draw minimap
        void Minimap_RenderUI(Control sender, GuiRenderer renderer)
        {
            Rect screenMapRect = sender.GetScreenRectangle();

            Bounds initialBounds = Map.Instance.InitialCollisionBounds;
            // Der zweite Wert (obere rechte Ecke) des Rechtecks darf nicht von der gesamten Map der obere rechte Punkt sein (da wir noch ganz weit
            // weg einen einzelnen Raum habne) Deshalb wird einfach der Wert um den Ursprung gespiegelt (Absolut-Werte genommen)
            Vec2 vec2 = new Vec2(Math.Abs(initialBounds.Minimum.ToVec2().X), Math.Abs(initialBounds.Minimum.ToVec2().Y));
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), vec2);

            Vec2 mapSizeInv = new Vec2(1, 1) / mapRect.Size;

            //draw units
            Vec2 screenPixel = new Vec2(1, 1) / new Vec2(EngineApp.Instance.VideoMode.ToVec2());

            foreach (Entity entity in Map.Instance.Children)
            {
                AlienUnit unit = entity as AlienUnit;
                if (unit == null)
                    continue;

                if (CheckMapPosition(unit.Position.ToVec2() * 0.9f))
                {
                    Rect rect = new Rect(unit.MapBounds.Minimum.ToVec2(), unit.MapBounds.Maximum.ToVec2());

                    rect -= mapRect.Minimum;
                    rect.Minimum *= mapSizeInv;
                    rect.Maximum *= mapSizeInv;
                    rect.Minimum = new Vec2(rect.Minimum.X, 1.0f - rect.Minimum.Y);
                    rect.Maximum = new Vec2(rect.Maximum.X, 1.0f - rect.Maximum.Y);
                    rect.Minimum *= screenMapRect.Size;
                    rect.Maximum *= screenMapRect.Size;
                    rect += screenMapRect.Minimum;

                    //increase 1 pixel
                    rect.Maximum += new Vec2(screenPixel.X, -screenPixel.Y);
                    ColorValue color = GetColor(unit);
                    renderer.AddQuad(rect, color);
                }

            }
            
            for (int i = 0; i < Computer.signalList.Count(); i++)
            {
                Signal s;
                bool peek = Computer.signalList.TryGet(i, out s);
                if (!peek)
                    continue;

                Rect rect = new Rect(s.Min, s.Max);

                rect -= mapRect.Minimum;
                rect.Minimum *= mapSizeInv;
                rect.Maximum *= mapSizeInv;
                rect.Minimum = new Vec2(rect.Minimum.X, 1.0f - rect.Minimum.Y);
                rect.Maximum = new Vec2(rect.Maximum.X, 1.0f - rect.Maximum.Y);
                rect.Minimum *= screenMapRect.Size;
                rect.Maximum *= screenMapRect.Size;
                rect += screenMapRect.Minimum;

                //increase 1 pixel
                rect.Maximum += new Vec2(screenPixel.X, -screenPixel.Y);

                //ColorValue color = GetColor(unit);
                renderer.AddQuad(rect, new ColorValue(1, 0, 0));
            }
            

            //Draw camera borders
            {
                Camera camera = RendererWorld.Instance.DefaultCamera;

                if (camera.Position.Z > 0)
                {
                    Plane groundPlane = new Plane(0, 0, 1, 0);

                    Vec2[] points = new Vec2[4];

                    for (int n = 0; n < 4; n++)
                    {
                        Vec2 p = Vec2.Zero;

                        switch (n)
                        {
                            case 0: p = new Vec2(0, 0); break;
                            case 1: p = new Vec2(1, 0); break;
                            case 2: p = new Vec2(1, 1); break;
                            case 3: p = new Vec2(0, 1); break;
                        }

                        Ray ray = camera.GetCameraToViewportRay(p);
                        float scale;
                        groundPlane.RayIntersection(ray, out scale);

                        Vec3 pos = ray.GetPointOnRay(scale);

                        if (ray.Direction.Z > 0)
                        {
                            pos = ray.Origin + ray.Direction.GetNormalize() * 10000;
                        }
                        Vec2 point = pos.ToVec2();

                        point -= mapRect.Minimum;
                        point *= mapSizeInv;
                        point = new Vec2(point.X, 1.0f - point.Y);
                        point *= screenMapRect.Size;
                        point += screenMapRect.Minimum;
                        points[n] = point;
                    }

                    renderer.PushClipRectangle(screenMapRect);
                    for (int n = 0; n < 4; n++)
                        renderer.AddLine(points[n], points[(n + 1) % 4], new ColorValue(1, 1, 1));
                    renderer.PopClipRectangle();
                }
            }
        }

        protected override void OnRenderUI(GuiRenderer renderer)
        {
            base.OnRenderUI(renderer);
            DrawHUD(renderer);
        }

        //Begrenzung der Mapansicht
        bool CheckMapPosition(Vec2 cameraPosition)
        {
            Bounds initialBounds;
            Vec2 vec2;
            Rect mapRect;
            if (!endkampfraum)
            {
                initialBounds = Map.Instance.InitialCollisionBounds;
                vec2 = new Vec2(Math.Abs(initialBounds.Minimum.ToVec2().X), Math.Abs(initialBounds.Minimum.ToVec2().Y));
                mapRect = new Rect(initialBounds.Minimum.ToVec2() * 0.9f, vec2 * 0.9f);
            }
            else
            {
                Sector s = Entities.Instance.GetByName("Sector_Endkampf") as Sector;
                initialBounds = s.MapBounds;
                vec2 = new Vec2(Math.Abs(initialBounds.Minimum.ToVec2().X), Math.Abs(initialBounds.Minimum.ToVec2().Y));
                mapRect = new Rect(initialBounds.Minimum.ToVec2(), initialBounds.Maximum.ToVec2());
            }
            return mapRect.IsContainsPoint(cameraPosition);
        }

        /// <summary>
        /// Ändert die CameraPosition von Map zu Endraum und umgekehrt
        /// </summary>
        void changeCameraPosition()
        {
            endkampfraum = !endkampfraum;
            if (endkampfraum)
            {
                if (CheckMapPosition(cameraPosition))
                {
                    oldCameraPosition = cameraPosition;
                }
                else
                {
                    oldCameraPosition = Map.Instance.Children.OfType<AlienSpawner>().First().Position.ToVec2();
                }
                Room r = Entities.Instance.GetByName("Endkampf-Raum") as Room;
                cameraPosition = r.Position.ToVec2();
            }
            else
            {
                cameraPosition = oldCameraPosition;
            }

        }

        CameraType GetRealCameraType()
        {
            return cameraType;
        }

        /// <summary>
        /// removes all small aliens from selectedUnits
        /// </summary>
        public void ClearEntitySelection()
        {
            while (selectedUnits.Count != 0)
            {
                SetEntitySelected(selectedUnits[selectedUnits.Count - 1], false);
            }
        }

        /// <summary>
        /// Adds or removes a small alien from selectedUnits
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="selected"></param>
        public void SetEntitySelected(Unit entity, bool selected)
        {
            if (entity is AlienUnit)
            {
                bool modified = false;

                if (selected)
                {
                    if (!selectedUnits.Contains(entity))
                    {
                        selectedUnits.Add(entity);
                        modified = true;
                    }
                }
                else
                    modified = selectedUnits.Remove(entity);
            }
        }

        int TaskTargetChooseIndex
        {
            get { return taskTargetChooseIndex; }
            set
            {
                taskTargetChooseIndex = value;
            }
        }

        void UpdateHUDControlIcon()
        {
            string iconName = null;
            if (selectedUnits.Count != 0)
                iconName = selectedUnits[0].Type.Name;
            /*
            //Bearbeiten
            Control control = hudControl.Controls["ControlUnitIcon"];

            if (!string.IsNullOrEmpty(iconName))
            {
                string fileName = string.Format("Gui\\HUD\\Icons\\{0}.png", iconName);

                bool needUpdate = false;

                if (control.BackTexture != null)
                {
                    string current = control.BackTexture.Name;
                    current = current.Replace('/', '\\');

                    if (string.Compare(fileName, current, true) != 0)
                        needUpdate = true;
                }
                else
                    needUpdate = true;

                if (needUpdate)
                {
                    if (VirtualFile.Exists(fileName))
                        control.BackTexture = TextureManager.Instance.Load(fileName, Texture.Type.Type2D, 0);
                    else
                        control.BackTexture = null;
                }
            }
            else
                control.BackTexture = null;
            */
        }

        void adjustCamera(ref Camera camera, float x, float y, float z, float displayWidth, float displayHeight)
        {
            // Strahlensatz verwenden, um den Ausschnitt des Bildes festzusetzen
            // Aktuell liegt der auf dem MultiTouchTisch
            #region Aspect Ratio and Vertical Field of View
            float mapWidth = (displayWidth / 2.0f) * (z + cameraDistance) / z;
            // ist nur die halbe map-Breite
            float mapHeight = (displayHeight / 2.0f) * (z + cameraDistance) / z;

            //float aspect = displayWidth / displayHeight;
            float aspect = mapWidth / mapHeight;
            //float c = (float)Math.Sqrt(z * z + (displayHeight / 2.0f) * (displayHeight / 2.0f));
            float hypotenuse1 = (float)Math.Sqrt((z + cameraDistance) * (z + cameraDistance) + (mapHeight + y) * (mapHeight + y)); //(mapWidth + x) * (mapWidth + x) +
            float hypotenuse2 = (float)Math.Sqrt((z + cameraDistance) * (z + cameraDistance) + (mapHeight - y) * (mapHeight - y)); //(mapWidth - x) * (mapWidth - x) + 
            
            float alpha1 = (float)Math.Acos((z + cameraDistance) / hypotenuse1);
            float alpha2 = (float)Math.Acos((z + cameraDistance) / hypotenuse2);
            float fovy = new Degree(new Radian(alpha1 + alpha2));
            #endregion

            #region Asymetric Frustum
            Vec2 frustumOffset = new Vec2();

            //float nearPlane = camera.NearClipDistance;
            //float nearDistanceRatio = nearPlane / z;
            //float nearDistanceRatio = nearPlane / (z + cameraDistance);
            // vielleicht hier noch: camera.NearClipDistance = nearDistanceRatio;

            frustumOffset.X = x - (z / (z + cameraDistance) * x);// / z;
            frustumOffset.Y = y - (z / (z + cameraDistance) * y);// / z;
            //frustumOffset.X = x / (z + cameraDistance);
            //frustumOffset.Y = y / (z + cameraDistance);
            #endregion

            #region Application to Camera
            //camera.Fov = fovy;
            camera.AspectRatio = aspect;
            camera.FrustumOffset = frustumOffset;
            #endregion
        }

        void receiveTrackingData(int sensorID, double x, double y, double z, Boolean output)
        {
            //headtracking
            if(output){
                if (Math.Sqrt(x * x + y * y + z * z) <= 2.5 && z >= 0)
                {
                    headtrackingOffset = new Vec3((float)Math.Round(x, 3), (float)Math.Round(y, 3), (float)Math.Round(z, 3));
                }
            }
        }

        protected override void OnGetCameraTransform(out Vec3 position, out Vec3 forward, out Vec3 up, ref Degree cameraFov)
        {
            //Vec3 offset;
            //{
            //    Quat rot = new Angles(0, 0, MathFunctions.RadToDeg(cameraDirection.Horizontal)).ToQuat();
            //    rot *= new Angles(0, MathFunctions.RadToDeg(cameraDirection.Vertical), 0).ToQuat();
            //    offset = rot * new Vec3(1, 0, 0);
            //    offset *= cameraDistance;
            //}
            //Currywurst


                //Vec3 lookAt = new Vec3(cameraPosition.X, cameraPosition.Y, 0);
                //Vec2 vector = Vec2.Zero;
                //if (todoTranslate != Vec2.Zero) {
                //    vector.X -= todoTranslate.X*15;
                //    vector.Y += todoTranslate.Y*15;
                //}
                //position = lookAt + offset;
                //up = new Vec3(0, 0, 1);
                //forward = -offset;
            if (todoRotate != 0)
            {
                if (todoRotate > 0)
                {
                    cameraDirection.Horizontal +=  0.01f;
                }
                else if (todoRotate < 0)
                {
                    cameraDirection.Horizontal -=  0.01f;
                }

                if (cameraDirection.Horizontal >= MathFunctions.PI * 2)
                    cameraDirection.Horizontal -= MathFunctions.PI * 2;
                if (cameraDirection.Horizontal < 0)
                    cameraDirection.Horizontal += MathFunctions.PI * 2;
                todoRotate = 0;

            }
            Quat rot = new Angles(0, 0, cameraDirection.Horizontal / (2 * MathFunctions.PI)*360-90).ToQuat();
            Vec3 translate;
            translate = new Vec3(todoTranslate.X, todoTranslate.Y, 0);
            translate *= rot;
            todoTranslate.X = translate.X;
            todoTranslate.Y = translate.Y;
            Vec3 lookAt2 = new Vec3(cameraPosition.X, cameraPosition.Y, cameraDistance);

            if (CheckMapPosition(cameraPosition + todoTranslate * 15))
            {
                lookAt2 = new Vec3(cameraPosition.X += todoTranslate.X * 15, cameraPosition.Y += todoTranslate.Y * 15, cameraDistance);
            }

            position = lookAt2;
            forward = new Vec3(0, 0, -1);
            up = new Vec3(0, 1, 0);
            up *= rot;

            todoTranslate = Vec2.Zero;
            //// Headtracking Daten
            //Vec3 headTrackingOffset = new Vec3(0, 0, 0);

            // Asymmetrisches Frustum
            Vec2 workbenchDimension = new Vec2(1.02f, 0.5f);
            Camera camera = RendererWorld.Instance.DefaultCamera;
            //camera.NearClipDistance = .1f;

            if (isHeadtrackingActive)
            {
                adjustCamera(ref camera, headtrackingOffset.X, headtrackingOffset.Y, headtrackingOffset.Z, workbenchDimension.X, workbenchDimension.Y);

                // Headtracking position addieren
                position += headtrackingOffset;
            }
        }

        // Brauchen wir das??
        public override void OnBeforeWorldSave()
        {
            base.OnBeforeWorldSave();

            //World serialized data
            World.Instance.ClearAllCustomSerializationValues();
            World.Instance.SetCustomSerializationValue("cameraDistance", cameraDistance);
            World.Instance.SetCustomSerializationValue("cameraDirection", cameraDirection);
            World.Instance.SetCustomSerializationValue("cameraPosition", cameraPosition);
            for (int n = 0; n < selectedUnits.Count; n++)
            {
                Unit unit = selectedUnits[n];
                World.Instance.SetCustomSerializationValue("selectedUnit" + n.ToString(), unit);
            }
        }

        /// <summary>
        /// Öffnet die Statistik
        /// </summary>
        void ShowStatistic()
        {
            // Text anpassen
            hudControl.Controls["Statistic"].Controls["StatisticAlien"].Controls["StatisticDataAlien"].Text = Computer.Statistic.GetAlienData();
            hudControl.Controls["Statistic"].Controls["StatisticAstronaut"].Controls["StatisticDataAstronaut"].Text = Computer.Statistic.GetAstronoutData();

            // Statistik anzeigen
            hudControl.Controls["Statistic"].Visible = !hudControl.Controls["Statistic"].Visible;
        }

        
        //////////////////////////////////////////////////////////////////
        ////                        BigMinimap                        ////
        //////////////////////////////////////////////////////////////////
        /// <summary>
        /// BigMinimap öffnen
        /// </summary>
        void DoOpenMinimap()
        {
            hudControl.Controls["BigMinimap"].Visible = true;
            bigMinimapObj.optimizedRotation();
        }
        //////////////////////////////////////////////////////////////////
        ////                   Ende BigMinimap                        ////
        //////////////////////////////////////////////////////////////////
    }
}
