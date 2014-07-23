using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.IO;
using Engine;
using Engine.MathEx;
using Engine.Renderer;
using Engine.MapSystem;
using ProjectCommon;
using ProjectEntities;
using Engine.PhysicsSystem;

namespace Game
{
    class CaveManager
    {
        static CaveManager instance;

        const int GWL_STYLE = (-16);

       // const IntPtr WS_NOFRAME = 0x10000000L;
        const int WS_VISIBLE = 0x10000000;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

      
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

        //public static IntPtr SetWindowLongPtr(IntPtr hWnd, Int32 nIndex, IntPtr dwNewLong)
        //{
        //    if (IntPtr.Size == 4)
        //    {
        //        return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        //    }
        //    else
        //    {
        //        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        //    }
        //}

        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLong")]
        //private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, Int32 nIndex, IntPtr dwNewLong);

        //[DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        //private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, Int32 nIndex, IntPtr dwNewLong);
       

        //[DllImport("user32.dll")]
        //static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, UInt32 dwNewLong);



        List<View> views = new List<View>();
        ReadOnlyCollection<View> viewsReadOnly;

        int preventTextureCreationRemainingFrames;

        //Wiimote _wiimote = null;
        //bool _wiimiteInitialized = false;
        //WiimoteState _lastState = null;//new WiimoteState();
        //bool _useWiiMote = true;

        static String _CAVEConfigFile = "cave_config.txt";

        struct CaveWallConfig
        {

            public Vec2 tctl;
            public Vec2 tctr;
            public Vec2 tcbl;
            public Vec2 tcbr;
        }

        CaveWallConfig[] caveConfig = new CaveWallConfig[6];

        bool _caveAdjustment = false;
        int _currentCAVEIndex = -1;
        int _currentCAVEMarker = 1;

        const float FrontWidth = 2.66f; // In metres
        const float FrontHeight = 1.98f;

        const float RightWidth = 2.0f;//2.69f;
        const float RightHeight = 1.98f;

        const float BottomWidth = 2.66f;
        const float BottomHeight = 2.0f;

        // x / y / h
        Vec3 PlayerDistance = new Vec3(1.34f, 1.34f, 1.66f); // NOTE: Vec3 for convenience, this is not a position vector

        ///////////////////////////////////////////

        public class View
        {
            Rect rectangle;
            float opacity = 1;

            public bool guirender;

            Texture texture;
            Vec2I initializedTextureSize;
            Camera camera;
            Viewport viewport;
            ViewRenderTargetListener renderTargetListener;

            GuiRenderer guiRenderer;

            ///////////////

            public delegate void RenderDelegate(View view, Camera camera);
            public event RenderDelegate Render;

            ///////////////

            class ViewRenderTargetListener : RenderTargetListener
            {
                View owner;

                public ViewRenderTargetListener(View owner)
                {
                    this.owner = owner;
                }

                protected override void OnPreRenderTargetUpdate(RenderTargetEvent evt)
                {
                    base.OnPreRenderTargetUpdate(evt);

                    Camera defaultCamera = RendererWorld.Instance.DefaultCamera;
                    Camera camera = owner.camera;

                    //set camera settings to default state
                    camera.ProjectionType = defaultCamera.ProjectionType;
                    camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                    camera.NearClipDistance = defaultCamera.NearClipDistance;
                    camera.FarClipDistance = defaultCamera.FarClipDistance;

                    Vec2I sizeInPixels = owner.Viewport.DimensionsInPixels.Size;
                    camera.AspectRatio = (float)sizeInPixels.X / (float)sizeInPixels.Y;

                    camera.Fov = defaultCamera.Fov;
                    camera.FixedUp = defaultCamera.FixedUp;
                    camera.Direction = defaultCamera.Direction;
                    camera.Position = defaultCamera.Position;

                    ////override visibility (hide main scene objects, show from lists)
                    //List<SceneNode> sceneNodes = new List<SceneNode>();
                    //if( owner.sceneNode != null )
                    //   sceneNodes.Add( owner.sceneNode );
                    //SceneManager.Instance.SetOverrideVisibleObjects( new SceneManager.OverrideVisibleObjectsClass(
                    //   new StaticMeshObject[ 0 ], sceneNodes.ToArray(), new RenderLight[ 0 ] ) );

                    if (owner.Render != null)
                        owner.Render(owner, camera);

                }

                protected override void OnPostRenderTargetUpdate(RenderTargetEvent evt)
                {
                    //SceneManager.Instance.ResetOverrideVisibleObjects();

                    base.OnPostRenderTargetUpdate(evt);

                    if (owner.Render != null && owner.guirender)
                    {
                        GameEngineApp.Instance.ControlManager.DoRenderUI(owner.guiRenderer);
                    }
                }
            }

            ///////////////

            internal View()
            {
            }

            public Rect Rectangle
            {
                get { return rectangle; }
                set { rectangle = value; }
            }

            public float Opacity
            {
                get { return opacity; }
                set { opacity = value; }
            }

            public Texture Texture
            {
                get { return texture; }
            }

            public Camera Camera
            {
                get { return camera; }
            }

            public Viewport Viewport
            {
                get { return viewport; }
            }

            void CreateViewport()
            {
                int index = instance.views.IndexOf(this);

                DestroyViewport();

                Vec2I textureSize = GetNeededTextureSize();

                string textureName = TextureManager.Instance.GetUniqueName(
                    string.Format("MultiViewRendering{0}", index));
                PixelFormat format = PixelFormat.R8G8B8;

                int fsaa;
                if (!int.TryParse(RendererWorld.InitializationOptions.FullSceneAntialiasing, out fsaa))
                    fsaa = 0;

                texture = TextureManager.Instance.Create(textureName, Texture.Type.Type2D,
                    textureSize, 1, 0, format, Texture.Usage.RenderTarget, false, fsaa);
                if (texture == null)
                {
                    Log.Fatal("CaveManager: Unable to create texture.");
                    return;
                }

                RenderTarget renderTarget = texture.GetBuffer().GetRenderTarget();
                renderTarget.AutoUpdate = true;
                renderTarget.AllowAdditionalMRTs = true;

                //create camera
                camera = SceneManager.Instance.CreateCamera(
                    SceneManager.Instance.GetUniqueCameraName(string.Format("MultiViewRendering{0}", index)));
                camera.Purpose = Camera.Purposes.MainCamera;
                camera.Enable3DSceneRendering = true;

                //add viewport
                viewport = renderTarget.AddViewport(camera, 0);
                viewport.ShadowsEnabled = true;
                guiRenderer = new GuiRenderer(viewport);
                guiRenderer.ApplyPostEffectsToScreenRenderer = true;
                
                //Create compositor for HDR render technique
                bool hdrCompositor =
                    RendererWorld.Instance.DefaultViewport.GetCompositorInstance("HDR") != null;
                if (hdrCompositor)
                {
                    viewport.AddCompositor("HDR");
                    viewport.SetCompositorEnabled("HDR", true);
                }

                ////FXAA antialiasing post effect
                bool fxaaCompositor =
                    RendererWorld.Instance.DefaultViewport.GetCompositorInstance("FXAA") != null;
                if (fxaaCompositor)
                {
                    viewport.AddCompositor("FXAA");
                    viewport.SetCompositorEnabled("FXAA", true);
                }

                // Add CAVE Compositor
                viewport.AddCompositor("CAVE");
                viewport.SetCompositorEnabled("CAVE", true);

                //add listener
                renderTargetListener = new ViewRenderTargetListener(this);
                renderTarget.AddListener(renderTargetListener);

                initializedTextureSize = textureSize;
            }

            Vec2I GetNeededTextureSize()
            {
                Vec2I result = Vec2I.Zero;

                Vec2I screenSize = RendererWorld.Instance.DefaultViewport.DimensionsInPixels.Size;
                if (screenSize.X > 0 && screenSize.Y > 0)
                    result = (rectangle.GetSize() * screenSize.ToVec2()).ToVec2I();

                if (result.X < 1)
                    result.X = 1;
                if (result.Y < 1)
                    result.Y = 1;

                return result;
            }

            internal void UpdateViewport()
            {
                if (initializedTextureSize != GetNeededTextureSize())
                {
                    DestroyViewport();
                    if (instance.preventTextureCreationRemainingFrames <= 0)
                        CreateViewport();
                }
            }

            internal void DestroyViewport()
            {
                if (renderTargetListener != null)
                {
                    RenderTarget renderTarget = texture.GetBuffer().GetRenderTarget();
                    renderTarget.RemoveListener(renderTargetListener);
                    renderTargetListener.Dispose();
                    renderTargetListener = null;
                }
                if (viewport != null)
                {
                    viewport.Dispose();
                    viewport = null;
                }
                if (camera != null)
                {
                    camera.Dispose();
                    camera = null;
                }
                if (texture != null)
                {
                    texture.Dispose();
                    texture = null;
                    instance.preventTextureCreationRemainingFrames = 2;
                }

                initializedTextureSize = Vec2I.Zero;
            }
        }

        ///////////////////////////////////////////

        CaveManager()
        {
            viewsReadOnly = new ReadOnlyCollection<View>(views);
        }

        public static CaveManager Instance
        {
            get { return instance; }
        }

        public static void Init()
        {
            if (instance != null)
                Log.Fatal("CaveManager: Init: is already initialized.");

            instance = new CaveManager();
            instance.InitInternal();
            
            View view = null;

            const float stepX = 1.0f / 2.0f;
            const float stepY = 1.0f / 3.0f;

            // Front (0, 1)
            view = instance.AddView(new Rect(0.0f, 0.0f, stepX, stepY), true);
            view.Render += viewRender;
            view = instance.AddView(new Rect(stepX, 0.0f, 1.0f, stepY), true);
            view.Render += viewRender;

            // Right (2, 3)
            view = instance.AddView(new Rect(0.0f, stepY, stepX, 2.0f * stepY), false);
            view.Render += viewRender;
            view = instance.AddView(new Rect(stepX, stepY, 1.0f, 2.0f * stepY), false);
            view.Render += viewRender;

            // Bottom (2, 3)
            view = instance.AddView(new Rect(0.0f, 2.0f * stepY, stepX, 3.0f * stepY), false);
            view.Render += viewRender;
            view = instance.AddView(new Rect(stepX, 2.0f * stepY, 1.0f, 3.0f * stepY), false);
            view.Render += viewRender;

            const int widthBeamer = 1400;
            const int heightBeamer = 1050;

            const int width = 2 * widthBeamer;
            const int height = 3 * heightBeamer;

            const int x = -2 * widthBeamer;
            const int y = 0;

            IntPtr handle = EngineApp.Instance.WindowHandle;
            if (handle != IntPtr.Zero)
            {
              // SetWindowLongPtr(handle, GWL_STYLE, IntPtr.Zero);
                SetWindowLong(handle, -16, 0x10000000L); // http://msdn.microsoft.com/en-us/library/windows/desktop/ms632600%28v=vs.85%29.aspx
                SetWindowPos(handle, 0, x, y, width, height, 0x0004 | 0x0040 | 0x0020); // http://msdn.microsoft.com/en-us/library/windows/desktop/ms633545%28v=vs.85%29.aspx
                EngineApp.Instance.VideoMode = new Vec2I(width, height);
            }

            //if (Instance._useWiiMote)
            //{
            //    try
            //    {
            //        Instance._wiimote = new Wiimote();
            //        Instance._lastState = new WiimoteState();

            //        Instance._wiimote.Connect();
            //        Instance._wiimiteInitialized = true;
            //        Instance._wiimote.SetLEDs(false, true, true, false);
            //        Instance._lastState = Instance._wiimote.WiimoteState;

            //        Instance._lastState.ButtonState.A = false;
            //        Instance._lastState.ButtonState.Up = false;
            //        Instance._lastState.ButtonState.Down = false;
            //        Instance._lastState.ButtonState.Left = false;
            //        Instance._lastState.ButtonState.Right = false;
            //    }
            //    catch (Exception  /*ex*/)
            //    {
            //        Instance._wiimiteInitialized = false;
            //    }
            //}
            //else
            //{
            //    Instance._wiimiteInitialized = false;
            //}

            Instance.loadCAVEConfig(_CAVEConfigFile);
        }

        public static void Shutdown()
        {
            if (instance != null)
            {
                instance.ShutdownInternal();
                instance = null;

                EngineApp.Instance.VideoMode = new Vec2I(1280, 800);

                //if (Instance._wiimiteInitialized)
                //{
                //    Instance._wiimote.SetLEDs(false, false, false, false);
                //    Instance._wiimote.Disconnect();
                //}
            }
        }

        void InitInternal()
        {
            RenderSystem.Instance.RenderSystemEvent += RenderSystem_RenderSystemEvent;
            RendererWorld.Instance.BeginRenderFrame += RendererWorld_BeginRenderFrame;
        }

        void ShutdownInternal()
        {
            RemoveAllViews();

            if (RendererWorld.Instance != null)
            {
                RenderSystem.Instance.RenderSystemEvent -= RenderSystem_RenderSystemEvent;
                RendererWorld.Instance.BeginRenderFrame -= RendererWorld_BeginRenderFrame;

                //restore main camera
                RendererWorld.Instance.DefaultCamera.Enable3DSceneRendering = true;
                MapCompositorManager.ApplyToMainCamera = true;
            }
        }

        public IList<View> Views
        {
            get { return viewsReadOnly; }
        }

        void DestroyViewports()
        {
            foreach (View view in views)
                view.DestroyViewport();
        }

        void RendererWorld_BeginRenderFrame()
        {
            RendererWorld.Instance.DefaultCamera.Enable3DSceneRendering = false;
            MapCompositorManager.ApplyToMainCamera = false;

            if (preventTextureCreationRemainingFrames > 0)
                preventTextureCreationRemainingFrames--;
            foreach (View view in views)
                view.UpdateViewport();
        }

        void AddTextWithShadow(GuiRenderer renderer, Font font, string text, Vec2 position,
            HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, ColorValue color)
        {
            Vec2 shadowOffset = 1.0f / renderer.ViewportForScreenGuiRenderer.DimensionsInPixels.Size.ToVec2();

            renderer.AddText(font, text, position + shadowOffset, horizontalAlign, verticalAlign,
                new ColorValue(0, 0, 0, color.Alpha / 2));
            renderer.AddText(font, text, position, horizontalAlign, verticalAlign, color);
        }

        public void RenderScreenUI(GuiRenderer renderer)
        {
            // Activate Post-Effects for RenderTexture and GUI
            //renderer.ApplyPostEffectsToScreenRenderer = true;

            for (int viewIndex = 0; viewIndex < views.Count; viewIndex++)
            {
               // float markerSize = 0.05f;
                View view = views[viewIndex];

                Rect currentView = view.Rectangle;
                
                if (_caveAdjustment)
                {
                    renderer.PushTextureFilteringMode(GuiRenderer.TextureFilteringModes.Point);

                    if (_currentCAVEIndex == viewIndex)
                    {
                        //Vec2 markerTopLeft = new Vec2();
                        //Vec2 markerBottomRight = new Vec2();

                        //Rect markerRect = new Rect(0.0f, 0.0f, markerSize, markerSize);

                        renderer.AddQuad(view.Rectangle, new Rect(0, 0, 1, 1), view.Texture,
                           new ColorValue(1, 1, 1, 0.5f), true);

                        //Rect centerRect = new Rect(currentView.Left + 1.0f / 3.0f * currentView.Size.X,
                        //    currentView.Top + 1.0f / 3.0f * currentView.Size.Y,
                        //    currentView.Left - 1.0f / 3.0f * currentView.Size.X,
                        //    currentView.Bottom - 1.0f / 3.0f * currentView.Size.Y);

                        //renderer.AddQuad(centerRect, new ColorValue(0.0f, 1.0f, 0.0f, 1.0f));

                        renderer.AddQuad(currentView, new ColorValue(0.0f, 1.0f, 0.0f, 0.5f));
                    }
                    else
                    {
                        renderer.AddQuad(view.Rectangle, new Rect(0, 0, 1, 1), view.Texture,
                           new ColorValue(1, 1, 1, 0.5f), true);

                        // Black screen for all viewports not in focus
                        renderer.AddQuad(currentView, new ColorValue(1.0f, 0.0f, 0.0f, 0.5f));
                    }

                    renderer.PopTextureFilteringMode();
                }
                else
                {
                    if (view.Opacity > 0)
                    {
                        renderer.PushTextureFilteringMode(GuiRenderer.TextureFilteringModes.Point);

                        renderer.AddQuad(view.Rectangle, new Rect(0, 0, 1, 1), view.Texture,
                            new ColorValue(1, 1, 1, view.Opacity), true);

                        renderer.PopTextureFilteringMode();
                    }
                }

                

                    //float size = 0.001f;
                    //float xCorrect = 0.5f;
                    //float xCenter = 0.5f;

                    //float lensSeparationDistance = OVRWrapper.Oculus.lensSeparationDistance();
                    //float hScreenSize = OVRWrapper.Oculus.hScreenSize();

                    //float hMeters = hScreenSize / 4.0f - lensSeparationDistance / 2.0f;
                    //float h = /*4.0f **/hMeters / hScreenSize;

                    //Log.Info("TestValue = " + testValue);

                    //if (viewIndex == 0)
                    //{
                    //    xCorrect -= 0.25f;
                    //    xCenter -= 0.25f;
                    //    xCorrect += h;// +Instance.testValue;
                    //}
                    //else
                    //{
                    //    xCorrect += 0.25f;
                    //    xCenter += 0.25f;
                    //    xCorrect -= h;// +Instance.testValue;
                    //}
                    //renderer.AddFillEllipse(new Rect(xCorrect - size, 0.5f - size, xCorrect + size, 0.5f + size), 8, new ColorValue(1.0f, 1.0f, 1.0f));
                    //renderer.AddFillEllipse(new Rect(xCenter - size, 0.5f - size, xCenter + size, 0.5f + size), 8, new ColorValue(1.0f, 0.0f, 1.0f));



                //draw debug info
                //if( drawDebugInfo )
                //{
                //    Viewport screenViewport = renderer.ViewportForScreenGuiRenderer;
                //    Vec2 pixelOffset = 1.0f / screenViewport.DimensionsInPixels.Size.ToVec2();
                //    ColorValue color = new ColorValue( 1, 1, 0 );
                //    renderer.AddRectangle( new Rect(
                //        view.Rectangle.LeftTop + pixelOffset,
                //        view.Rectangle.RightBottom - pixelOffset * 2 ),
                //        color );
                //    renderer.AddLine( view.Rectangle.LeftTop, view.Rectangle.RightBottom, color );
                //    renderer.AddLine( view.Rectangle.RightTop, view.Rectangle.LeftBottom, color );

                //    if( debugFont == null )
                //        debugFont = FontManager.Instance.LoadFont( "Default", .03f );

                //    string sizeString = "";
                //    if( view.Texture != null )
                //        sizeString = string.Format( "{0}x{1}", view.Texture.Size.X, view.Texture.Size.Y );
                //    string text = string.Format( "View {0}, {1}", viewIndex, sizeString );
                //    Vec2 position = new Vec2( view.Rectangle.Right - pixelOffset.X * 5, view.Rectangle.Top );
                //    AddTextWithShadow( renderer, debugFont, text, position, HorizontalAlign.Right,
                //        VerticalAlign.Top, new ColorValue( 1, 1, 1 ) );
                //}
            }

            // Activate Post-Effects for RenderTexture and GUI
            //renderer.ApplyPostEffectsToScreenRenderer = false;
        }

        void RenderSystem_RenderSystemEvent(RenderSystemEvents name)
        {
            if (name == RenderSystemEvents.DeviceLost)
            {
                foreach (View view in views)
                    view.DestroyViewport();
            }
        }

        public View AddView(Rect rectangle, bool gui)
        {
            View view = new View();
            view.Rectangle = rectangle;
            view.guirender = gui;
            views.Add(view);

            return view;
        }

        public void RemoveView(View view)
        {
            view.DestroyViewport();
            views.Remove(view);
        }

        public void RemoveAllViews()
        {
            while (views.Count > 0)
                RemoveView(views[views.Count - 1]);
        }

        public static Mat3 RotationMatrix(float angle, Vec3 axis)
        {
            float cos = (float)Math.Cos(angle);
            float inv = 1 - (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            return new Mat3(
            cos + axis.X * axis.X * inv, axis.X * axis.Y * inv - axis.Z * sin, axis.X * axis.Z * inv + axis.Y * sin,
            axis.Y * axis.Z * inv + axis.Z * sin, cos + axis.Y * axis.Y * inv, axis.Y * axis.Z * inv - axis.X * sin,
            axis.Z * axis.X * inv - axis.Y * sin, axis.Z * axis.Y * inv + axis.X * sin, cos + axis.Z * axis.Z * inv
            );
        }

        void stereoCameraProperties(ref float fovy, ref float aspect, float distance, float displayWidth, float displayHeight)
        {
            aspect = displayWidth / displayHeight;

            float c = (float)Math.Sqrt(distance * distance + (displayHeight / 2.0f) * (displayHeight / 2.0f));

            float alpha = (float)Math.Acos(distance / c);

            fovy = 2.0f * alpha;

            fovy = new Degree(new Radian(fovy));
        }

        void computeStereoCameraProperties(ref Camera camera, float distance, float horizontalCameraOffset, float verticalCameraOffset, float displayWidth, float displayHeight, bool isLeftEye)
        {
            #region Aspect Ratio and Vertical Field of View
            float aspect = displayWidth / displayHeight;
            float c = (float)Math.Sqrt(distance * distance + (displayHeight / 2.0f) * (displayHeight / 2.0f));
            float alpha = (float)Math.Acos(distance / c);
            float fovy = new Degree(new Radian(2.0f * alpha));
            #endregion

            #region Asymetric Frustum
            Vec2 frustumOffset = new Vec2();

            float nearPlane = camera.NearClipDistance;
            float nearDistanceRatio = nearPlane / distance;
            
            frustumOffset.X = horizontalCameraOffset / distance;
            frustumOffset.Y = verticalCameraOffset / distance;
            
            #endregion

            #region Eye Offset
            Vec3 cameraPosition = camera.Position;
            Vec3 cameraDirection = camera.Direction;
            Vec3 cameraUp = camera.Up;
            Vec3 cameraRightNormalized = Vec3.Cross(cameraDirection, cameraUp).GetNormalize();

            const float IODHalf = 0.065f / 2.0f;
            Vec3 eyeOffset = new Vec3();

            if (isLeftEye)
            {
                eyeOffset = -1.0f * cameraRightNormalized * IODHalf;
                frustumOffset.X += (1.0f * IODHalf) / distance;
            }
            else
            {
                eyeOffset = cameraRightNormalized * IODHalf;
                frustumOffset.X += (-1.0f * IODHalf) / distance;
            }        
            #endregion

            #region Application to Camera
            camera.Fov = fovy;
            camera.AspectRatio = aspect;
            camera.FrustumOffset = frustumOffset;

            camera.Position += eyeOffset;
            #endregion
        }

        static void viewRender(View view, Camera camera)
        {
            #region Obtain Camera Properties
            Camera defaultCamera = RendererWorld.Instance.DefaultCamera;
            Viewport defaultViewport = RendererWorld.Instance.DefaultViewport;

            int index = Instance.Views.IndexOf(view);

            Vec3 position = defaultCamera.Position;
            Vec3 direction = defaultCamera.Direction;
            Vec3 up = defaultCamera.Up;
            Vec3 right = Vec3.Cross(direction, up);

            const float heightOffset = 1.66f;
            //direction.Z = 0.0f;
            //position.Z = 1.66f;
         //   up = new Vec3(0, 0, 1);

            #endregion

            #region Adjust Height
            //Ray ray = new Ray(new Vec3(position.X, position.Y, position.Z + 100), new Vec3(0, 0, -1));
            //RayCastResult[] results = PhysicsWorld.Instance.RayCastPiercing(ray, (int)ContactGroup.CastOnlyCollision);

            //foreach (RayCastResult result in results)
            //{
            //    if (result.Shape != null && result.Shape.ShapeType == Shape.Type.HeightField)
            //    {
            //        position.Z = result.Position.Z + heightOffset;                    
            //    }
            //}

            #endregion


            //#region Wiimote controls
            //if (Instance._wiimiteInitialized)
            //{
            //    WiimoteState wiiState = Instance._wiimote.WiimoteState;
            //    const float nunchuckThreshold = 0.1f;
            //    const float nunchuckScale = 0.01f;

            //    float nunchuckX = wiiState.NunchukState.Joystick.X; // [-0.5, 0.5], left negative, right positive
            //    float nunchuckY = wiiState.NunchukState.Joystick.Y; // [-0.5, 0.5], bottom negative, top positive

            //    bool buttonUpPressed = wiiState.ButtonState.Up;
            //    bool buttonDownPressed = wiiState.ButtonState.Down;
            //    bool buttonLeftPressed = wiiState.ButtonState.Left;
            //    bool buttonRightPressed = wiiState.ButtonState.Right;

            //    bool buttonUpPressedOld = Instance._lastState.ButtonState.Up;
            //    bool buttonDownPressedOld = Instance._lastState.ButtonState.Down;
            //    bool buttonLeftPressedOld = Instance._lastState.ButtonState.Left;
            //    bool buttonRightPressedOld = Instance._lastState.ButtonState.Right;

            //    bool buttonAPressed = wiiState.ButtonState.A;
            //    bool buttonAPressedOld = Instance._lastState.ButtonState.A;

            //    if (buttonUpPressed /*&& !buttonUpPressedOld*/)
            //    {
            //        GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Up));
            //    }
            //    else /*if (!buttonUpPressed && buttonUpPressedOld)*/
            //    {
            //        GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Up));
            //    }

            //    if (buttonDownPressed)
            //    {
            //        GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Down));
            //    }
            //    else
            //    {
            //        GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Down));
            //    }

            //    if (buttonLeftPressed)
            //    {
            //        GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Left));
            //    }
            //    else
            //    {
            //        GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Left));
            //    }

            //    if (buttonRightPressed)
            //    {
            //        GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Right));
            //    }
            //    else
            //    {
            //        GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Right));
            //    }

            //    if (buttonAPressed)
            //    {
            //        GameControlsManager.Instance.DoMouseDown(EMouseButtons.Left);
            //    }
            //    else
            //    {
            //        GameControlsManager.Instance.DoMouseUp(EMouseButtons.Left);
            //    }
                
            //    if (Math.Abs(nunchuckX) > nunchuckThreshold)
            //    {
            //        GameControlsManager.Instance.DoMouseMoveRelative(new Vec2(nunchuckScale * nunchuckX, 0.0f));
            //    }

            //    if (Math.Abs(nunchuckY) > nunchuckThreshold)
            //    {
            //        GameControlsManager.Instance.DoMouseMoveRelative(new Vec2(0.0f, nunchuckScale * -1.0f * nunchuckY));
            //    }

            //    Instance._lastState = wiiState;
            //}
            //#endregion

            #region Assign Camera Properties

            float fovy = 90.0f;
            float aspect = 1.0f;

            switch (index)
            {
                case 0: // Front Cam (Left)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.Y, FrontWidth, FrontHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;
                        camera.FixedUp = up;
                        camera.Direction = direction;
                        camera.Position = position;

                        //Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.Y, Instance.PlayerDistance.X, Instance.PlayerDistance.Z, FrontWidth, FrontHeight, true);

                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.Y, FrontWidth / 2.0f - Instance.PlayerDistance.X, FrontHeight / 2.0f - Instance.PlayerDistance.Z, FrontWidth, FrontHeight, true);
                        
                        //camera.FrustumOffset

                        break;
                    }

                case 1: // Front Cam (Right)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.Y, FrontWidth, FrontHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;
                        camera.FixedUp = up;
                        camera.Direction = direction;
                        camera.Position = position;

                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.Y, FrontWidth / 2.0f - Instance.PlayerDistance.X, FrontHeight / 2.0f - Instance.PlayerDistance.Z, FrontWidth, FrontHeight, false);

                        //camera.Fov = 61.4046f;

                        float muh = camera.Fov;

                        //camera.FrustumOffset

                        break;
                    }

                case 2: // Right Cam (Left)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.X, RightWidth, RightHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;
                        camera.FixedUp = up;

                        Mat3 rotation = RotationMatrix((float)(Math.PI / 2.0), up);

                        camera.Direction = rotation * direction;
                        camera.Position = position;

                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.X, RightWidth / 2.0f - Instance.PlayerDistance.Y, RightHeight / 2.0f - Instance.PlayerDistance.Z, RightWidth, RightHeight, true);

                        break;
                    }

                case 3: // Right Cam (Right)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.X, RightWidth, RightHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;
                        camera.FixedUp = up;

                        Mat3 rotation = RotationMatrix((float)(Math.PI / 2.0), up);

                        camera.Direction = rotation * direction;
                        camera.Position = position;

                        //Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.X, RightWidth / 2.0f - Instance.PlayerDistance.Y, Instance.PlayerDistance.Z - RightHeight / 2.0f, RightWidth, RightHeight, false);
                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.X, RightWidth / 2.0f - Instance.PlayerDistance.Y, RightHeight / 2.0f - Instance.PlayerDistance.Z, RightWidth, RightHeight, false);

                        break;
                    }

                case 4: // Bottom Cam (Left)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.Z, BottomWidth, BottomHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;

                        Mat3 rotation = RotationMatrix((float)(Math.PI / 2.0), right);

                        //camera.FixedUp = rotation * up;
                        camera.FixedUp = direction;
                        camera.Direction = rotation * direction;
                        camera.Position = position;

                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.Z, Instance.PlayerDistance.X - BottomWidth / 2.0f, Instance.PlayerDistance.Y - BottomHeight / 2.0f, BottomWidth, BottomHeight, true);

                        break;
                    }

                case 5: // Bottom Cam (Right)
                    {
                        //Instance.stereoCameraProperties(ref fovy, ref aspect, Instance.PlayerDistance.Z, BottomWidth, BottomHeight);

                        camera.ProjectionType = defaultCamera.ProjectionType;
                        camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                        camera.NearClipDistance = defaultCamera.NearClipDistance;
                        camera.FarClipDistance = defaultCamera.FarClipDistance;
                        camera.AspectRatio = aspect;
                        camera.Fov = fovy;

                        Mat3 rotation = RotationMatrix((float)(Math.PI / 2.0), right);

                        //camera.FixedUp = rotation * up;
                        camera.FixedUp = direction;
                        camera.Direction = rotation * direction;
                        camera.Position = position;

                        Instance.computeStereoCameraProperties(ref camera, Instance.PlayerDistance.Z, Instance.PlayerDistance.X - BottomWidth / 2.0f, Instance.PlayerDistance.Y - BottomHeight / 2.0f, BottomWidth, BottomHeight, false);

                        break;
                    }
            }

            #endregion

            #region Assign CAVE Compositor Properties
            CAVECompositorInstance compositor = view.Viewport.GetCompositorInstance("CAVE") as CAVECompositorInstance;

            if (compositor != null)
            {
                compositor.Tctl = Instance.caveConfig[index].tctl;
                compositor.Tctr = Instance.caveConfig[index].tctr;
                compositor.Tcbl = Instance.caveConfig[index].tcbl;
                compositor.Tcbr = Instance.caveConfig[index].tcbr;
            }
            #endregion

            //set up material scheme for viewport
            view.Viewport.MaterialScheme = defaultViewport.MaterialScheme;
        }

        void adjustCAVEProperty(Vec2 offset)
        {
            if (_currentCAVEIndex < 0 || _currentCAVEIndex > 5)
                return;

            switch(_currentCAVEMarker)
            {
                case 1: 
                    {
                        caveConfig[_currentCAVEIndex].tctl += offset;
                        break;
                    }

                case 2:
                    {
                        caveConfig[_currentCAVEIndex].tctr += offset;
                        break;
                    }

                case 3:
                    {
                        caveConfig[_currentCAVEIndex].tcbl += offset;
                        break;
                    }

                case 4:
                    {
                        caveConfig[_currentCAVEIndex].tcbr += offset;
                        break;
                    }
            }
        }

        public bool OnKeyDown(KeyEvent e)
        {
            const float adjustmentStep = 0.01f;

            // Start Adjustment
            if (e.Key == EKeys.F9)
            {
                _caveAdjustment = true;
                _currentCAVEIndex = 0;
                _currentCAVEMarker = 1;
                Log.Info("START");
            }

            // Stop Adjustment
            if (e.Key == EKeys.F10)
            {
                _caveAdjustment = false;
            }

            if (e.Key == EKeys.Left)
            {
                adjustCAVEProperty(new Vec2(adjustmentStep, 0.0f));
            }

            if (e.Key == EKeys.Right)
            {
                adjustCAVEProperty(new Vec2(-adjustmentStep, 0.0f));
            }

            if (e.Key == EKeys.Up)
            {
                adjustCAVEProperty(new Vec2(0.0f, adjustmentStep));
            }

            if (e.Key == EKeys.Down)
            {
                adjustCAVEProperty(new Vec2(0.0f, -adjustmentStep));
            }

            // Advance to next marker / viewport
            if (e.Key == EKeys.Add)
            {
                _currentCAVEMarker++;
                if (_currentCAVEMarker > 4)
                {
                    _currentCAVEMarker = 1;
                    _currentCAVEIndex++;
                }

                if (_currentCAVEIndex > 5)
                {
                    _currentCAVEIndex = -1;
                    _currentCAVEMarker = 1;
                    _caveAdjustment = false;

                    saveCAVEConfig(_CAVEConfigFile);
                }
            }

            // Switch back to previous marker / viewport
            if (e.Key == EKeys.Subtract)
            {
                _currentCAVEMarker--;
                if (_currentCAVEMarker < 1)
                {
                    _currentCAVEMarker = 4;
                    _currentCAVEIndex--;
                }

                if (_currentCAVEIndex < 0)
                {
                    _currentCAVEIndex = 0;
                    _currentCAVEMarker = 1;
                }
            }

            // Store Adjustments
            if (e.Key == EKeys.Enter)
            {
                saveCAVEConfig(_CAVEConfigFile);
            }

            if (_caveAdjustment)
                return true;
            else
                return false;
        }

        void loadCAVEConfig(String filename)
        {
            if (File.Exists(filename))
            {
                FileStream streamIn = new FileStream(filename, FileMode.Open);
                BinaryReader reader = new BinaryReader(streamIn);

                for (int i = 0; i < 6; ++i)
                {
                    caveConfig[i].tctl.X = reader.ReadSingle();
                    caveConfig[i].tctl.Y = reader.ReadSingle();
                    caveConfig[i].tctr.X = reader.ReadSingle();
                    caveConfig[i].tctr.Y = reader.ReadSingle();
                    caveConfig[i].tcbl.X = reader.ReadSingle();
                    caveConfig[i].tcbl.Y = reader.ReadSingle();
                    caveConfig[i].tcbr.X = reader.ReadSingle();
                    caveConfig[i].tcbr.Y = reader.ReadSingle();
                }

                reader.Close();
                streamIn.Close();
            }
            else
            {
                for (int i = 0; i < 6; ++i)
                {
                    caveConfig[i].tctl = new Vec2(0.0f, 0.0f);
                    caveConfig[i].tctr = new Vec2(1.0f, 0.0f);
                    caveConfig[i].tcbl = new Vec2(0.0f, 1.0f);
                    caveConfig[i].tcbr = new Vec2(1.0f, 1.0f);
                }
            }
        }

        void saveCAVEConfig(String filename)
        {
            FileStream streamOut = new FileStream(filename, FileMode.OpenOrCreate);
            BinaryWriter writer = new BinaryWriter(streamOut);

            for (int i = 0; i < 6; ++i)
            {
                writer.Write(caveConfig[i].tctl.X);
                writer.Write(caveConfig[i].tctl.Y);
                writer.Write(caveConfig[i].tctr.X);
                writer.Write(caveConfig[i].tctr.Y);
                writer.Write(caveConfig[i].tcbl.X);
                writer.Write(caveConfig[i].tcbl.Y);
                writer.Write(caveConfig[i].tcbr.X);
                writer.Write(caveConfig[i].tcbr.Y);
            }

            writer.Close();
            streamOut.Close();
        }
    }
}
