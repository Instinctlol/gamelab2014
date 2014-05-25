// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
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



/*
 * TODO's:
 * es wurde was an Game/GameEngineApp.cs geändert für den GameType AlienGame
 * genauso wurde an ProjectEntities/GameMap.cs was geändert für den GameType AlienGame
 * 
 * 1. Irgendwie herausfinden, wie sich die Gebäude anklicken lassen und das für den MySqawner nachbauen, damit dieser spawnt.
 * 2. aliens/rabbits auswählbar machen (so wie hier die units)
 * 3. Tasks implementieren (active, passive), auch in die KI und dann auswählbar per Button machen (GUI)
 * 
 */
namespace Game
{
    public class AlienGameWindow : GameWindow
    {
        enum CameraType
        {
            Game,
            Free,

            Count
        }
        static CameraType cameraType;

        [Config("Map", "drawPathMotionMap")]
        public static bool mapDrawPathMotionMap;

        Range cameraDistanceRange = new Range(10, 300);
        Range cameraAngleRange = new Range(.001f, MathFunctions.PI / 2 - .001f);
        float cameraDistance = 23;
        SphereDir cameraDirection = new SphereDir(1.5f, .85f);
        Vec2 cameraPosition;

        //HUD
        Control hudControl;

        //Select
        List<Unit> selectedUnits = new List<Unit>();

        //Spawning
        int possibleNumberSpawnAliens = 10;
        ListBox numberSpawnUnitsList;
        int spawnNumber = 1;

        //Select mode
        bool selectMode;
        Vec2 selectStartPos;
        bool selectDraggedMouse;

        //Task target choose
        int taskTargetChooseIndex = -1;
        
        //Minimap
        bool minimapChangeCameraPosition;
        Control minimapControl;

        float timeForUpdateGameStatus;

        ScrollBar cameraDistanceScrollBar;
        ScrollBar cameraHeightScrollBar;
        bool disableUpdatingCameraScrollBars;

        // Beim Starten des Spiels GUI initialisieren und co
        protected override void OnAttach()
        {
            base.OnAttach();

            EngineApp.Instance.KeysAndMouseButtonUpAll();

            //hudControl
            hudControl = ControlDeclarationManager.Instance.CreateControl("Maps\\AlienDemo\\Gui\\HUD.gui");
            Controls.Add(hudControl);

            ((Button)hudControl.Controls["Menu"]).Click += delegate(Button sender)
            {
                Controls.Add(new MenuWindow());
            };

            ((Button)hudControl.Controls["Exit"]).Click += delegate(Button sender)
            {
                GameWorld.Instance.NeedChangeMap("Maps\\MainDemo\\Map.map", "Teleporter_Maps", null);
            };

            ((Button)hudControl.Controls["Help"]).Click += delegate(Button sender)
            {
                hudControl.Controls["HelpWindow"].Visible = !hudControl.Controls["HelpWindow"].Visible;
            };

            ((Button)hudControl.Controls["HelpWindow"].Controls["Close"]).Click += delegate(Button sender)
            {
                hudControl.Controls["HelpWindow"].Visible = false;
            };

            ((Button)hudControl.Controls["DebugPath"]).Click += delegate(Button sender)
            {
                mapDrawPathMotionMap = !mapDrawPathMotionMap;
            };

            cameraDistanceScrollBar = hudControl.Controls["CameraDistance"] as ScrollBar;
            if (cameraDistanceScrollBar != null)
            {
                cameraDistanceScrollBar.ValueRange = cameraDistanceRange;
                cameraDistanceScrollBar.ValueChange += cameraDistanceScrollBar_ValueChange;
            }

            cameraHeightScrollBar = hudControl.Controls["CameraHeight"] as ScrollBar;
            if (cameraHeightScrollBar != null)
            {
                cameraHeightScrollBar.ValueRange = cameraAngleRange;
                cameraHeightScrollBar.ValueChange += cameraHeightScrollBar_ValueChange;
            }

            // default listbox for number of spawn aliens is disabled
            numberSpawnUnitsList = hudControl.Controls["NumberSpawnUnitsList"] as ListBox;
            if (numberSpawnUnitsList != null)
            {
                for (int i = 1; i <= possibleNumberSpawnAliens; i++)
                {
                    numberSpawnUnitsList.Items.Add(i);
                }
                numberSpawnUnitsList.SelectedIndex = 0;
                numberSpawnUnitsList.SelectedIndexChange += numberSpawnUnitsList_SelectedIndexChange;
                numberSpawnUnitsList.ItemMouseDoubleClick += numberSpawnUnitsList_ItemMouseDoubleClick;
            }

            InitControlPanelButtons();
            UpdateControlPanel();

            //set playerFaction for small aliens
            //playerFaction = (FactionType)EntityTypes.Instance.GetByName("BadFaction");
            //if (RTSFactionManager.Instance != null && RTSFactionManager.Instance.Factions.Count != 0)
            //playerFaction = RTSFactionManager.Instance.Factions[0].FactionType;

            //minimap
            minimapControl = hudControl.Controls["Minimap"];
            string textureName = Map.Instance.GetSourceMapVirtualFileDirectory() + "\\Minimap\\Minimap";
            Texture minimapTexture = TextureManager.Instance.Load(textureName, Texture.Type.Type2D, 0);
            minimapControl.BackTexture = minimapTexture;
            minimapControl.RenderUI += new RenderUIDelegate(Minimap_RenderUI);

            //set camera position
            foreach (Entity entity in Map.Instance.Children)
            {
                SpawnPoint spawnPoint = entity as SpawnPoint;
                if (spawnPoint == null)
                    continue;
                cameraPosition = spawnPoint.Position.ToVec2();
                break;
            }

            //World serialized data
            if (World.Instance.GetCustomSerializationValue("cameraDistance") != null)
                cameraDistance = (float)World.Instance.GetCustomSerializationValue("cameraDistance");
            if (World.Instance.GetCustomSerializationValue("cameraDirection") != null)
                cameraDirection = (SphereDir)World.Instance.GetCustomSerializationValue("cameraDirection");
            if (World.Instance.GetCustomSerializationValue("cameraPosition") != null)
                cameraPosition = (Vec2)World.Instance.GetCustomSerializationValue("cameraPosition");
            for (int n = 0; ; n++)
            {
                Unit unit = World.Instance.GetCustomSerializationValue(
                    "selectedUnit" + n.ToString()) as Unit;
                if (unit == null)
                    break;
                SetEntitySelected(unit, true);
            }

            ResetTime();

            //render scene for loading resources
            EngineApp.Instance.RenderScene();

            EngineApp.Instance.MousePosition = new Vec2(.5f, .5f);

            UpdateCameraScrollBars();
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

        protected override bool OnKeyDown(KeyEvent e)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnKeyDown(e);

            //change camera type
            //if (e.Key == EKeys.F7)
            //{
            //    cameraType = (CameraType)((int)GetRealCameraType() + 1);
            //    if (cameraType == CameraType.Count)
            //        cameraType = (CameraType)0;

            //    FreeCameraEnabled = cameraType == CameraType.Free;

            //    GameEngineApp.Instance.AddScreenMessage("Camera type: " + cameraType.ToString());

            //    return true;
            //}

            //select another demo map
            //if (e.Key == EKeys.F3)
            //{
            //    GameWorld.Instance.NeedChangeMap("Maps\\MainDemo\\Map.map", "Teleporter_Maps", null);
            //    return true;
            //}

            // Aliens spawnen
            //if (e.Key == EKeys.F1)
            //{
            //    List<MapObject> myspawnerpoints = Map.Instance.SceneGraphObjects.FindAll(delegate(MapObject obj)
            //    {
            //        ProjectEntities.MySpawner myspawnpoint = obj as ProjectEntities.MySpawner;

            //        if (myspawnpoint != null)
            //        {
            //            return true;
            //        }
            //        else
            //        {
            //            return false;
            //        }
            //    });

            //    ProjectEntities.MySpawner mypawnpoint = myspawnerpoints[0] as ProjectEntities.MySpawner;
            //    mypawnpoint.SpawnSmallAlien();
            //}

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
            if (!hudControl.Controls["ActiveArea"].GetScreenRectangle().IsContainsPoint(MousePosition))
                return false;
            return true;
        }

        protected override bool OnMouseDown(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseDown(button);

            // Mouse click for select unit
            if (button == EMouseButtons.Left && IsMouseInActiveArea() && TaskTargetChooseIndex == -1)
            {
                selectMode = true;
                selectDraggedMouse = false;
                selectStartPos = EngineApp.Instance.MousePosition;
                return true;
            }

            //minimap mouse change camera position
            if (button == EMouseButtons.Left && taskTargetChooseIndex == -1)
            {
                if (minimapControl.GetScreenRectangle().IsContainsPoint(MousePosition))
                {
                    minimapChangeCameraPosition = true;
                    cameraPosition = GetMapPositionByMouseOnMinimap();
                    return true;
                }
            }

            return base.OnMouseDown(button);
        }

        protected override bool OnMouseUp(EMouseButtons button)
        {
            //If atop openly any window to not process
            if (Controls.Count != 1)
                return base.OnMouseUp(button);

            //do tasks
            if ((button == EMouseButtons.Right || button == EMouseButtons.Left) &&
                (!FreeCameraMouseRotating || !EngineApp.Instance.MouseRelativeMode))
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

                //pick on minimap
                if (minimapControl.GetScreenRectangle().IsContainsPoint(MousePosition))
                {
                    pickingSuccess = true;
                    Vec2 pos = GetMapPositionByMouseOnMinimap();
                    mouseMapPos = new Vec3(pos.X, pos.Y, GridBasedNavigationSystem.Instances[0].GetMotionMapHeight(pos));
                }

                if (pickingSuccess)
                {
                    //do tasks
                        //if (button == EMouseButtons.Right)
                        //    DoRightClickTasks(mouseMapPos, mouseOnObject);
                    // TODO hier ganz anders: das bräuchte ich:
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
            if (minimapChangeCameraPosition)
                minimapChangeCameraPosition = false;

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
                        //Move, Attack, Repair
                        case AlienUnitAI.Task.Types.Move:
                        case AlienUnitAI.Task.Types.Attack:
                        case AlienUnitAI.Task.Types.Repair:
                            if (mouseOnObject != null)
                                intellect.DoTask(new AlienUnitAI.Task(taskType, mouseOnObject), toQueue);
                            else
                            {
                                if (taskType == AlienUnitAI.Task.Types.Move)
                                    intellect.DoTask(new AlienUnitAI.Task(taskType, mouseMapPos), toQueue);

                                if (taskType == AlienUnitAI.Task.Types.Attack ||
                                    taskType == AlienUnitAI.Task.Types.Repair)
                                {
                                    intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.BreakableMove,
                                        mouseMapPos), toQueue);
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

                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Attack) &&
                        mouseOnObject.Intellect != null &&
                        intellect.Faction != null && mouseOnObject.Intellect.Faction != null &&
                        intellect.Faction != mouseOnObject.Intellect.Faction)
                    {
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Attack,
                            mouseOnObject), toQueue);
                        tasked = true;
                    }

                    if (IsEnableTaskTypeInTasks(tasks, AlienUnitAI.Task.Types.Repair) &&
                        mouseOnObject.Intellect != null &&
                        intellect.Faction != null && mouseOnObject.Intellect.Faction != null &&
                        intellect.Faction == mouseOnObject.Intellect.Faction)
                    {
                        intellect.DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Repair,
                            mouseOnObject), toQueue);
                        tasked = true;
                    }

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
                            AlienUnit unit = (AlienUnit)obj;
                            areaObjs.Add(unit);
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
                Vec2 diffPixels = (MousePosition - selectStartPos) *
                    new Vec2(EngineApp.Instance.VideoMode.X, EngineApp.Instance.VideoMode.Y);
                if (Math.Abs(diffPixels.X) >= 3 || Math.Abs(diffPixels.Y) >= 3)
                {
                    selectDraggedMouse = true;
                }
            }

            //minimap mouse change camera position
            if (minimapChangeCameraPosition)
                cameraPosition = GetMapPositionByMouseOnMinimap();
        }

        protected override void OnTick(float delta)
        {
            base.OnTick(delta);

            //If atop openly any window to not process
            if (Controls.Count != 1)
                return;

            //Remove deleted selected objects
            for (int n = 0; n < selectedUnits.Count; n++)
            {
                if (selectedUnits[n].IsSetForDeletion)
                {
                    selectedUnits.RemoveAt(n);
                    n--;
                }
            }

            if (!FreeCameraMouseRotating)
                EngineApp.Instance.MouseRelativeMode = false;

            bool activeConsole = EngineConsole.Instance != null && EngineConsole.Instance.Active;

            if (GetRealCameraType() == CameraType.Game && !activeConsole)
            {
                if (EngineApp.Instance.IsKeyPressed(EKeys.PageUp))
                {
                    cameraDistance -= delta * (cameraDistanceRange[1] - cameraDistanceRange[0]) / 10.0f;
                    if (cameraDistance < cameraDistanceRange[0])
                        cameraDistance = cameraDistanceRange[0];
                    UpdateCameraScrollBars();
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.PageDown))
                {
                    cameraDistance += delta * (cameraDistanceRange[1] - cameraDistanceRange[0]) / 10.0f;
                    if (cameraDistance > cameraDistanceRange[1])
                        cameraDistance = cameraDistanceRange[1];
                    UpdateCameraScrollBars();
                }

                //alienCameraDirection

                if (EngineApp.Instance.IsKeyPressed(EKeys.Home))
                {
                    cameraDirection.Vertical += delta * (cameraAngleRange[1] - cameraAngleRange[0]) / 2;
                    if (cameraDirection.Vertical >= cameraAngleRange[1])
                        cameraDirection.Vertical = cameraAngleRange[1];
                    UpdateCameraScrollBars();
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.End))
                {
                    cameraDirection.Vertical -= delta * (cameraAngleRange[1] - cameraAngleRange[0]) / 2;
                    if (cameraDirection.Vertical < cameraAngleRange[0])
                        cameraDirection.Vertical = cameraAngleRange[0];
                    UpdateCameraScrollBars();
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.Q))
                {
                    cameraDirection.Horizontal += delta * 2;
                    if (cameraDirection.Horizontal >= MathFunctions.PI * 2)
                        cameraDirection.Horizontal -= MathFunctions.PI * 2;
                }

                if (EngineApp.Instance.IsKeyPressed(EKeys.E))
                {
                    cameraDirection.Horizontal -= delta * 2;
                    if (cameraDirection.Horizontal < 0)
                        cameraDirection.Horizontal += MathFunctions.PI * 2;
                }


                //change cameraPosition
                if (!selectMode && Time > 2)
                {
                    Vec2 vector = Vec2.Zero;

                    if (EngineApp.Instance.IsKeyPressed(EKeys.Left) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.A) || MousePosition.X < .005f)
                    {
                        vector.X--;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Right) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.D) || MousePosition.X > 1.0f - .005f)
                    {
                        vector.X++;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Up) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.W) || MousePosition.Y < .005f)
                    {
                        vector.Y++;
                    }
                    if (EngineApp.Instance.IsKeyPressed(EKeys.Down) ||
                        EngineApp.Instance.IsKeyPressed(EKeys.S) || MousePosition.Y > 1.0f - .005f)
                    {
                        vector.Y--;
                    }

                    if (vector != Vec2.Zero)
                    {
                        //rotate vector
                        float angle = MathFunctions.ATan(-vector.Y, vector.X) +
                            cameraDirection.Horizontal;
                        vector = new Vec2(MathFunctions.Sin(angle), MathFunctions.Cos(angle));

                        cameraPosition += vector * delta * 50;
                    }
                }

            }


            //gameStatus
            if (string.IsNullOrEmpty(hudControl.Controls["GameStatus"].Text))
            {
                timeForUpdateGameStatus -= delta;
                if (timeForUpdateGameStatus < 0)
                {
                    timeForUpdateGameStatus += 1;

                    bool existsAlly = false;
                    bool existsEnemy = false;

                    foreach (Entity entity in Map.Instance.Children)
                    {
                        Unit unit = entity as Unit;
                        if (unit == null)
                            continue;
                        if (unit is Alien)
                        {
                            existsEnemy = true;
                        }
                        else if (unit is GameCharacter)// TODO Astronaut
                        {
                            existsAlly = true;
                        }
                    }

                    string gameStatus = "";
                    if (!existsAlly)
                        gameStatus = "!!! Victory !!!";
                    if (!existsEnemy)
                        gameStatus = "!!! Defeat !!!";

                    hudControl.Controls["GameStatus"].Text = gameStatus;
                }
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
                string text = "";

                if (selectedUnits.Count > 1)
                {
                    foreach (Unit unit in selectedUnits)
                        text += unit.ToString() + "\n";
                }

                if (selectedUnits.Count == 1)
                {
                    Unit unit = selectedUnits[0];

                    text += unit.ToString() + "\n";
                    text += "\n";
                    text += string.Format("Life: {0}/{1}\n", unit.Health, unit.Type.HealthMax);

                    text += "Intellect:\n";
                    if (unit.Intellect != null)
                    {
                        text += string.Format("- {0}\n", unit.Intellect.ToString());
                        FactionType faction = unit.Intellect.Faction;
                        text += string.Format("- Faction: {0}\n", faction != null ? faction.ToString() : "null");

                        AlienUnitAI AlienUnitAI = unit.Intellect as AlienUnitAI;
                        if (AlienUnitAI != null)
                        {
                            text += string.Format("- CurrentTask: {0}\n", AlienUnitAI.CurrentTask.ToString());
                        }
                    }
                    else
                        text += string.Format("- null\n");

                }

                hudControl.Controls["SelectedUnitsInfo"].Text = text;

                UpdateControlPanel();
            }

            UpdateHUDControlIcon();
        }

        /// <summary>
        /// Farbe für kleine Aliens und Spawnpoint verschieden
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        ColorValue GetColor(Unit unit)
        {
            if (unit is Alien)
            {
                return new ColorValue(1, 0, 0);
            }
            else if (unit is AlienSpawner)
            {
                return new ColorValue(0, 1, 0);
            }
            return new ColorValue();
        }

        List<AlienUnitAI.UserControlPanelTask> GetControlTasks()
        {
            List<AlienUnitAI.UserControlPanelTask> tasks = null;

            if (selectedUnits.Count != 0)
            {
                foreach (Unit unit in selectedUnits)
                {
                    AlienUnitAI intellect = unit.Intellect as AlienUnitAI;
                    if (intellect != null)
                    {
                        List<AlienUnitAI.UserControlPanelTask> t = intellect.GetControlPanelTasks();
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
                                    tasks[n] = new AlienUnitAI.UserControlPanelTask(
                                        tasks[n].Task, true, tasks[n].Enable);
                                }

                                if (tasks[n].Task.Type == AlienUnitAI.Task.Types.ProductUnit)
                                {
                                    if (tasks[n].Task.EntityType != t[n].Task.EntityType)
                                        tasks[n] = new AlienUnitAI.UserControlPanelTask(
                                            new AlienUnitAI.Task(AlienUnitAI.Task.Types.None));
                                }
                            }
                        }
                    }
                }
            }

            return tasks;
        }

        void InitControlPanelButtons()
        {
            for (int n = 0; ; n++)
            {
                Button button = (Button)hudControl.Controls["ControlPanelButton" + n.ToString()];
                if (button == null)
                    break;
                button.Click += new Button.ClickDelegate(ControlPanelButton_Click);
            }
        }

        // Wenn ein Button zum Navigieren der Aliens/Spawnpoints geklickt wurde
        void ControlPanelButton_Click(Button sender)
        {
            int index = int.Parse(sender.Name.Substring("ControlPanelButton".Length));

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
                            EngineConsole.Instance.Print("number" + spawnNumber);
                            spawnNumber = 2;
                            intellect.DoTask(new AlienUnitAI.Task(taskType, tasks[index].Task.EntityType, spawnNumber), false);
                        }
                    }
                    break;

                //Move, Attack, Repair
                case AlienUnitAI.Task.Types.Move:
                case AlienUnitAI.Task.Types.Attack:
                case AlienUnitAI.Task.Types.Repair:
                    //do taskTargetChoose
                    TaskTargetChooseIndex = index;
                    break;

            }
        }

        // Buttons zum Navigieren der kleinen aliens zeichnen
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
            Control controlNumberSpawnUnitsText = hudControl.Controls["NumberSpawnUnitsText"];
            controlNumberSpawnUnitsText.Visible = false;
            numberSpawnUnitsList.Visible = false;

            // make all buttons visible or not
            for (int n = 0; ; n++)
            {
                Control control = hudControl.Controls["ControlPanelButton" + n.ToString()];

                if (control == null)
                    break;

                control.Visible = tasks != null && n < tasks.Count;

                if (control.Visible)
                    if (tasks[n].Task.Type == AlienUnitAI.Task.Types.None)
                        control.Visible = false;

                if (control.Visible)
                {
                    string text = null;

                    if (tasks[n].Task.EntityType != null)
                    {
                        text += tasks[n].Task.EntityType.FullName;
                        // if task is to spawn aliens we have to show the listbox so that the player can choose the number of aliens to be spawned
                        if (tasks[n].Task.EntityType.FullName == "Alien")
                        {
                            numberSpawnUnitsList.Visible = true;
                            controlNumberSpawnUnitsText.Visible = true;
                        }
                    }
                    if (text == null)
                    {
                        text = tasks[n].Task.ToString();
                    }

                    control.Text = text;
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
            }
        }

        //Draw minimap
        void Minimap_RenderUI(Control sender, GuiRenderer renderer)
        {
            Rect screenMapRect = sender.GetScreenRectangle();

            Bounds initialBounds = Map.Instance.InitialCollisionBounds;
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), initialBounds.Maximum.ToVec2());

            Vec2 mapSizeInv = new Vec2(1, 1) / mapRect.Size;

            //draw units
            Vec2 screenPixel = new Vec2(1, 1) / new Vec2(EngineApp.Instance.VideoMode.ToVec2());

            foreach (Entity entity in Map.Instance.Children)
            {
                AlienUnit unit = entity as AlienUnit;
                if (unit == null)
                    continue;

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

                //if (playerFaction == null || unit.Intellect == null || unit.Intellect.Faction == null)
                //    color = new ColorValue(1, 1, 0);
                //else if (playerFaction == unit.Intellect.Faction)
                //    color = new ColorValue(0, 1, 0);
                //else
                //    color = new ColorValue(1, 0, 0);



                renderer.AddQuad(rect, color);
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
                            pos = ray.Origin + ray.Direction.GetNormalize() * 10000;

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
                SetEntitySelected(selectedUnits[selectedUnits.Count - 1], false);
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

        /// <summary>
        /// Changes Map position by calculating the position from the position of the mouse on the minimap
        /// </summary>
        /// <returns></returns>
        Vec2 GetMapPositionByMouseOnMinimap()
        {
            Rect screenMapRect = minimapControl.GetScreenRectangle();

            Bounds initialBounds = Map.Instance.InitialCollisionBounds;
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), initialBounds.Maximum.ToVec2());

            Vec2 point = MousePosition;

            point -= screenMapRect.Minimum;
            point /= screenMapRect.Size;
            point = new Vec2(point.X, 1.0f - point.Y);
            point *= mapRect.Size;
            point += mapRect.Minimum;

            return point;
        }

        void UpdateHUDControlIcon()
        {
            string iconName = null;
            if (selectedUnits.Count != 0)
                iconName = selectedUnits[0].Type.Name;

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
        }

        protected override void OnGetCameraTransform(out Vec3 position, out Vec3 forward,
           out Vec3 up, ref Degree cameraFov)
        {
            Vec3 offset;
            {
                Quat rot = new Angles(0, 0, MathFunctions.RadToDeg(
                    cameraDirection.Horizontal)).ToQuat();
                rot *= new Angles(0, MathFunctions.RadToDeg(cameraDirection.Vertical), 0).ToQuat();
                offset = rot * new Vec3(1, 0, 0);
                offset *= cameraDistance;
            }
            Vec3 lookAt = new Vec3(cameraPosition.X, cameraPosition.Y, 0);

            position = lookAt + offset;
            forward = -offset;
            up = new Vec3(0, 0, 1);
        }

        void cameraDistanceScrollBar_ValueChange(ScrollBar sender)
        {
            if (disableUpdatingCameraScrollBars)
                return;
            cameraDistance = sender.Value;
        }

        void cameraHeightScrollBar_ValueChange(ScrollBar sender)
        {
            if (disableUpdatingCameraScrollBars)
                return;
            cameraDirection.Vertical = sender.Value;
        }

        void numberSpawnUnitsList_SelectedIndexChange(ListBox sender)
        {
            // Den Index (beginnt bei 0) plus 1
            spawnNumber = (int)sender.SelectedIndex + 1;
            EngineConsole.Instance.Print("changed" + spawnNumber);
        }

        void numberSpawnUnitsList_ItemMouseDoubleClick(object sender, ListBox.ItemMouseEventArgs e)
        {
            spawnNumber = (int)e.ItemIndex + 1;
            EngineConsole.Instance.Print("doubleclick" + e.Item + " " + spawnNumber);
        }

        void UpdateCameraScrollBars()
        {
            disableUpdatingCameraScrollBars = true;
            if (cameraDistanceScrollBar != null)
                cameraDistanceScrollBar.Value = cameraDistance;
            if (cameraHeightScrollBar != null)
                cameraHeightScrollBar.Value = cameraDirection.Vertical;
            disableUpdatingCameraScrollBars = false;
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

    }
}
