using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Engine;
using Engine.MathEx;
using Engine.Renderer;
using Engine.MapSystem;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
    class OculusManager
    {
        static OculusManager instance;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

		List<View> views = new List<View>();
		ReadOnlyCollection<View> viewsReadOnly;

		int preventTextureCreationRemainingFrames;

        //HeadTracker _tracker;

        float realYaw = 0.0f;
        float lastYaw = 0.0f;
        float mouseDiffX = 0.0f;
        bool mouseInitialized = false;

       // Texture calibrationPattern = null;
        bool showCalibration = false;

        #region Tracking
        bool _trackingInitialized = false;
        Vec3 _initialTrackingPosition;
        float _initialYaw;
        

        Vec3 _currentTrackingPosition;

        Vec3[] _sensors = new Vec3[3];
        int _currentSensorID = -1;
        const int _maxSensorID = 2;

        Vec3 _direction = new Vec3();
        Vec3 _debugOffset = new Vec3();
        float walkingScale = 2.0f;

        #endregion

        public bool ShowCalibration
        {
            get { return showCalibration; }
            set { showCalibration = value; }
        }
                                
		///////////////////////////////////////////

		public class View
		{
			Rect rectangle;
			float opacity = 1;

			Texture texture;
			Vec2I initializedTextureSize;
			Camera camera;
			Viewport viewport;
			ViewRenderTargetListener renderTargetListener;

			///////////////

			public delegate void RenderDelegate( View view, Camera camera );
			public event RenderDelegate Render;

			///////////////

            GuiRenderer guiRenderer; // <<<<



			class ViewRenderTargetListener : RenderTargetListener
			{
				View owner;

				public ViewRenderTargetListener( View owner )
				{
					this.owner = owner;
				}

				protected override void OnPreRenderTargetUpdate( RenderTargetEvent evt )
				{
					base.OnPreRenderTargetUpdate( evt );
                    
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

					if( owner.Render != null )
						owner.Render( owner, camera );

				}

				protected override void OnPostRenderTargetUpdate( RenderTargetEvent evt )
				{
					//SceneManager.Instance.ResetOverrideVisibleObjects();
                   
                    base.OnPostRenderTargetUpdate(evt);

                    if (owner.Render != null)
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
				int index = instance.views.IndexOf( this );

				DestroyViewport();

				Vec2I textureSize = GetNeededTextureSize();

				string textureName = TextureManager.Instance.GetUniqueName(
					string.Format( "MultiViewRendering{0}", index ) );
				PixelFormat format = PixelFormat.R8G8B8;

				int fsaa;
				if( !int.TryParse( RendererWorld.InitializationOptions.FullSceneAntialiasing, out fsaa ) )
					fsaa = 0;

				texture = TextureManager.Instance.Create( textureName, Texture.Type.Type2D,
					textureSize, 1, 0, format, Texture.Usage.RenderTarget, false, fsaa );
				if( texture == null )
				{
					Log.Fatal( "OculusManager: Unable to create texture." );
					return;
				}

				RenderTarget renderTarget = texture.GetBuffer().GetRenderTarget();
				renderTarget.AutoUpdate = true;
				renderTarget.AllowAdditionalMRTs = true;

				//create camera
				camera = SceneManager.Instance.CreateCamera(
					SceneManager.Instance.GetUniqueCameraName( string.Format( "MultiViewRendering{0}", index ) ) );
				camera.Purpose = Camera.Purposes.MainCamera;
                camera.Enable3DSceneRendering = true;

				//add viewport
				viewport = renderTarget.AddViewport( camera, 0 );
				viewport.ShadowsEnabled = true;

                // Add OVR Compositor
                viewport.AddCompositor("OVR");
                viewport.SetCompositorEnabled("OVR", true);
                guiRenderer = new GuiRenderer(viewport);
                guiRenderer.ApplyPostEffectsToScreenRenderer = true;

				//Create compositor for HDR render technique
                //bool hdrCompositor =
                //    RendererWorld.Instance.DefaultViewport.GetCompositorInstance( "HDR" ) != null;
                //if( hdrCompositor )
                //{
                //    viewport.AddCompositor( "HDR" );
                //    viewport.SetCompositorEnabled( "HDR", true );
                //}

                ////FXAA antialiasing post effect
                //bool fxaaCompositor =
                //    RendererWorld.Instance.DefaultViewport.GetCompositorInstance( "FXAA" ) != null;
                //if( fxaaCompositor )
                //{
                //    viewport.AddCompositor( "FXAA" );
                //    viewport.SetCompositorEnabled( "FXAA", true );
                //}

				//add listener
				renderTargetListener = new ViewRenderTargetListener( this );
				renderTarget.AddListener( renderTargetListener );

				initializedTextureSize = textureSize;
			}

			Vec2I GetNeededTextureSize()
			{
				Vec2I result = Vec2I.Zero;

				Vec2I screenSize = RendererWorld.Instance.DefaultViewport.DimensionsInPixels.Size;
				if( screenSize.X > 0 && screenSize.Y > 0 )
					result = ( rectangle.GetSize() * screenSize.ToVec2() ).ToVec2I();

				if( result.X < 1 )
					result.X = 1;
				if( result.Y < 1 )
					result.Y = 1;

				return result;
			}

			internal void UpdateViewport()
			{
				if( initializedTextureSize != GetNeededTextureSize() )
				{
					DestroyViewport();
					if( instance.preventTextureCreationRemainingFrames <= 0 )
						CreateViewport();
				}
			}

			internal void DestroyViewport()
			{
				if( renderTargetListener != null )
				{
					RenderTarget renderTarget = texture.GetBuffer().GetRenderTarget();
					renderTarget.RemoveListener( renderTargetListener );
					renderTargetListener.Dispose();
					renderTargetListener = null;
				}
				if( viewport != null )
				{
					viewport.Dispose();
					viewport = null;
				}
				if( camera != null )
				{
					camera.Dispose();
					camera = null;
				}
				if( texture != null )
				{
					texture.Dispose();
					texture = null;
					instance.preventTextureCreationRemainingFrames = 2;
				}

				initializedTextureSize = Vec2I.Zero;
			}
		}

		///////////////////////////////////////////

		OculusManager()
		{
			viewsReadOnly = new ReadOnlyCollection<View>( views );
		}

		public static OculusManager Instance
		{
			get { return instance; }
		}

        private void _swapTrackingToNeoAxis(ref Vec3 value)
        {
            Vec3 temp = new Vec3();
            //temp.Y = value.X;
            //temp.Z = value.Y;
            //temp.X = value.Z;

            ////Test
            temp.Y = value.Y * walkingScale * (-1);
            temp.Z = value.Z;
            temp.X = value.X * walkingScale;

            value = temp;
        }

        public void headTracking(int sensorID, double x, double y, double z)
        {
            //Vec3 position = new Vec3();
            //float yaw = 0.0f;

            #region calculate orientation
            if (sensorID >= 0 && sensorID < 3)
            {
                _sensors[sensorID] = new Vec3((float)x, (float)y, (float)z);
                _currentSensorID = sensorID;
            }

            // NOTE: Assuming head position is stable, using heuristics for only this situation to search for markers
            if (_currentSensorID == _maxSensorID)
            {
                // Marker Front - Characteristics: min y
                // Marker Right - Characteristics: mid y
                // Marker Left  - Characteristics: max y

                int frontID, leftID, rightID;

                //// Search left marker
                //float currentValue = -9999.0f;
                //int currentID = -1;
                //for (int i = 0; i < 3; ++i)
                //{
                //    if (_sensors[i].Y > currentValue)
                //    {
                //        currentValue = _sensors[i].Y;
                //        currentID = i;
                //    }
                //}

                //leftID = currentID;

                // Search front marker
                float currentValue = -9999.0f;
                int currentID = -1;
                for (int i = 0; i < 3; ++i)
                {
                    if (_sensors[i].Y > currentValue)
                    {
                        currentValue = _sensors[i].Y;
                        currentID = i;
                    }
                }
                frontID = currentID;

                //// Search front marker
                //currentValue = 9999.0f;
                //currentID = -1;
                //for (int i = 0; i < 3; ++i)
                //{
                //    if (_sensors[i].Y < currentValue)
                //    {
                //        currentValue = _sensors[i].Y;
                //        currentID = i;
                //    }
                //}

                //frontID = currentID;

                // Search left marker
                currentValue = 9999.0f;
                currentID = -1;
                for (int i = 0; i < 3; ++i)
                {
                    if (_sensors[i].Z < currentValue)
                    {
                        currentValue = _sensors[i].Y;
                        currentID = i;
                    }
                }

                leftID = currentID;


                // Assign right marker to rest
                if (leftID == 0 && frontID == 1 || leftID == 1 && frontID == 0)
                    rightID = 2;
                if (leftID == 1 && frontID == 2 || leftID == 2 && frontID == 1)
                    rightID = 0;
                if (leftID == 0 && frontID == 2 || leftID == 2 && frontID == 0)
                    rightID = 1;

                // Get center
                Vec3 position = (_sensors[0] + _sensors[1] + _sensors[2]) / 3.0f;

                if (!_trackingInitialized)
                {
                    // Get front direction vector
                    Vec3 direction = -position + _sensors[frontID];

                    Vec3 temp = direction;
                    _swapTrackingToNeoAxis(ref temp);
                    SphereDir testDir = SphereDir.FromVector(temp);

                    // Project direction vector on XZ-Plane
                    direction.Y = 0.0f;

                    // Get angle between direction and X-Axis
                    float dot = Vec3.Dot(direction, Vec3.ZAxis);
                    float len = direction.Length();
                    double alpha = Math.Acos(dot / len);

                    // Store initial position (NOTE: Adjust by offset to head center position)
                    _initialTrackingPosition = position;
                    _currentTrackingPosition = new Vec3(0.0f, 0.0f, 0.0f);
                    //SphereDir.FromVector(
                    // Store initial yaw
                    _initialYaw = (float)alpha;// -(float)Math.PI / 2.0f;
                    //_initialYaw -= (PlayerIntellect.Instance.LookDirection.Horizontal);

                    while (_initialYaw < 0)
                        _initialYaw += (float)Math.PI * 2.0f;

                    while (_initialYaw > (float)Math.PI * 2.0f)
                        _initialYaw -= (float)Math.PI * 2.0f;

                    _trackingInitialized = true;

                    // Debug stuff
                    _direction = direction;
                    _direction.Normalize();
                    _swapTrackingToNeoAxis(ref _direction);

                    _direction = Mat3.FromRotateByZ(_initialYaw) * _direction;
                    // Debug stuff end


                    //double angle = Math.Atan2(direction.X, direction.Z);

                    // TEST
                    //direction.Normalize();
                    //_currentTrackingPosition = 100 * direction;
                    
                    //_currentTrackingPosition = Mat3.FromRotateByY(_initialYaw) * _currentTrackingPosition;
                    //Vec3 test = Mat3.FromRotateByY(-_initialYaw) * Vec3.ZAxis;

                    //// Swap coordinates for NeoAxis System
                    //Vec3 temp = new Vec3();
                    //temp.Y = _currentTrackingPosition.X;
                    //temp.Z = _currentTrackingPosition.Y;
                    //temp.X = _currentTrackingPosition.Z;

                    //_currentTrackingPosition = temp;

                    

                    // TEST
                }
                else
                {
                    _currentTrackingPosition = position - _initialTrackingPosition;

                    _swapTrackingToNeoAxis(ref _currentTrackingPosition);
                    // Swap coordinates for NeoAxis System
                    //Vec3 temp = new Vec3();
                    
                    //temp.Y = _currentTrackingPosition.X;
                    //temp.Z = _currentTrackingPosition.Y;
                    //temp.X = _currentTrackingPosition.Z;

                    //_currentTrackingPosition = temp;
                    
                    _currentTrackingPosition = Mat3.FromRotateByZ(_initialYaw) * _currentTrackingPosition;
                    _currentTrackingPosition.Y *= -1.0f;
                    
                }
            }

            #endregion
        }

		public static void Init(bool displayOnRift)
		{
			if( instance != null )
				Log.Fatal( "MultiDisplayRenderingManager: Init: is already initialized." );

			instance = new OculusManager();
			instance.InitInternal();

            //instance.calibrationPattern = TextureManager.Instance.Load("Base/FullScreenEffects/OVR/pattern.png");

            // Add viewport for left eye
            View view = null;
            
            view = instance.AddView(new Rect(0, 0, .5f, 1));
            view.Render += viewRender;

            // Add viewport for right eye
            view = instance.AddView(new Rect(0.5f, 0, 1, 1));
            view.Render += viewRender;

            Instance.lastYaw = 0.0f;

            // Initialize Oculus Rift
            if (!OVRWrapper.Oculus.initialize())
            {
                Log.Error("Could not initialize Oculus Rift.");
            }
            else
            {
                // Move window to Oculus
                if (displayOnRift)
                {
                    RectI mostLeft = new RectI(9999, 9999, 9999, 9999);

                    foreach (DisplayInfo display in EngineApp.Instance.AllDisplays)
                    {
                        int width = display.Bounds.Right - display.Bounds.Left;
                        int height = display.Bounds.Bottom - display.Bounds.Top;

                        if (width == 1280 && height == 800)
                        {
                            if (display.Bounds.Left < mostLeft.Left)
                            {
                                mostLeft = display.Bounds;
                            }
                        }
                    }

                    IntPtr handle = EngineApp.Instance.WindowHandle;
                    if (handle != IntPtr.Zero) 
                    {
                        if (EngineApp.Instance.FullScreen == false)
                        {
                            EngineApp.Instance.VideoMode = new Vec2I(1280, 800);
                            EngineApp.Instance.FullScreen = true;
                            Program.needRestartApplication = true;
                            EngineApp.Instance.SetNeedExit();
                        }
                        else
                        {
                            SetWindowPos(handle, 0, mostLeft.Left, mostLeft.Top, 0, 0, 0x0004 | 0x0001 | 0x0040);
                        }
                                 
                    }                               
                        
                }
            }

            // Initialize head tracking
            HeadTracker.Instance.TrackingEvent += new HeadTracker.receiveTrackingData(Instance.headTracking);
		}

		public static void Shutdown()
		{
			if( instance != null )
			{
                //instance.calibrationPattern = null;

				instance.ShutdownInternal();
				instance = null;

                // Deinitialize Oculus Rift
                OVRWrapper.Oculus.deinitialize();

                //Instance._tracker.stop();
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

			if( RendererWorld.Instance != null )
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
			foreach( View view in views )
				view.DestroyViewport();
		}

		void RendererWorld_BeginRenderFrame()
		{
			RendererWorld.Instance.DefaultCamera.Enable3DSceneRendering = false;
            MapCompositorManager.ApplyToMainCamera = false;

			if( preventTextureCreationRemainingFrames > 0 )
				preventTextureCreationRemainingFrames--;
			foreach( View view in views )
				view.UpdateViewport();
		}

		void AddTextWithShadow( GuiRenderer renderer, Font font, string text, Vec2 position,
			HorizontalAlign horizontalAlign, VerticalAlign verticalAlign, ColorValue color )
		{
			Vec2 shadowOffset = 1.0f / renderer.ViewportForScreenGuiRenderer.DimensionsInPixels.Size.ToVec2();

			renderer.AddText( font, text, position + shadowOffset, horizontalAlign, verticalAlign,
				new ColorValue( 0, 0, 0, color.Alpha / 2 ) );
			renderer.AddText( font, text, position, horizontalAlign, verticalAlign, color );
		}

		public void RenderScreenUI( GuiRenderer renderer )
		{
            // Activate Post-Effects for RenderTexture and GUI
            //renderer.ApplyPostEffectsToScreenRenderer = true;

			for( int viewIndex = 0; viewIndex < views.Count; viewIndex++ )
			{
				View view = views[ viewIndex ];

				//draw view on screen
				if( view.Opacity > 0 )
				{
					renderer.PushTextureFilteringMode( GuiRenderer.TextureFilteringModes.Point );

                    renderer.AddQuad(view.Rectangle, new Rect(0, 0, 1, 1), view.Texture,
                        new ColorValue(1, 1, 1, view.Opacity), true);

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
                    
					renderer.PopTextureFilteringMode();
				}

                
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

		void RenderSystem_RenderSystemEvent( RenderSystemEvents name )
		{
			if( name == RenderSystemEvents.DeviceLost )
			{
				foreach( View view in views )
					view.DestroyViewport();
			}
		}

		public View AddView( Rect rectangle )
		{
			View view = new View();
			view.Rectangle = rectangle;
			views.Add( view );
			return view;
		}

		public void RemoveView( View view )
		{
			view.DestroyViewport();
			views.Remove( view );
		}

		public void RemoveAllViews()
		{
			while( views.Count > 0 )
				RemoveView( views[ views.Count - 1 ] );
		}

        float testValue = 0.0f;//0.00544f;
        static float inc = 0.0001f;

        public void plus()
        {
            testValue += inc;
        }

        public void minus()
        {
            testValue -= inc;
        }

        static void viewRender(View view, Camera camera)
        {
            Camera defaultCamera = RendererWorld.Instance.DefaultCamera;
            Viewport defaultViewport = RendererWorld.Instance.DefaultViewport;

            // TEST !!!!
            //SphereDir dir = PlayerIntellect.Instance.LookDirection;
            //dir.Horizontal = 0.0f;
            //PlayerIntellect.Instance.LookDirection = dir;
            // TEST !!!!

            int index = Instance.Views.IndexOf(view);

            Vec3 position = defaultCamera.Position + Instance._currentTrackingPosition;
            Vec3 direction = new Vec3();
            Vec3 up = new Vec3();
            Vec3 right = new Vec3();

            float IOD = 0.065f;

            #region LookAt Calculation

            float yaw = 0.0f, pitch = 0.0f, roll = 0.0f;
            OVRWrapper.Oculus.orientation(ref yaw, ref pitch, ref roll);

            // Dont ask...
            yaw *= -1.0f;
            pitch *= -1.0f;
            roll *= -1.0f;

            // TEST !!!
            //yaw = 0.0f;
            //pitch = 0.0f;
            //roll = 0.0f;
            // TEST !!!

            Instance.realYaw += (yaw - Instance.lastYaw);
            Instance.lastYaw = yaw;

            

            //Log.Info("Yaw: " + yaw);

            if (!Instance.mouseInitialized)
            {
                Instance.mouseInitialized = true;
                Instance.mouseDiffX = 0.0f;
            }

            const float EPS = 0.0001f;
            if (GameEngineApp.Instance.MouseRelativeMode && Math.Abs(Instance.mouseDiffX) > EPS)
            {
                Instance.realYaw += Instance.mouseDiffX * 3.0f;
                Instance.mouseDiffX = 0.0f;
            }
            else
            {
                Instance.mouseDiffX = 0.0f;
            }

            // Get orientation matrix and calculate up and forward vector from it
            Mat3 rollPitchYaw = Mat3.FromRotateByZ(Instance.realYaw) * Mat3.FromRotateByY(pitch) * Mat3.FromRotateByX(roll);
            up = rollPitchYaw * new Vec3(0.0f, 0.0f, 1.0f);
            direction = rollPitchYaw * new Vec3(-1.0f, 0.0f, 0.0f);

            // Minimal head modelling (some adjustments to lenses and HMD display)
            float headBaseToEyeHeight = 0.15f;  // Vertical height of eye from base of head
            float headBaseToEyeProtrusion = 0.09f;  // Distance forward of eye from base of head

            Vec3 eyeCenterInHeadFrame = new Vec3(-headBaseToEyeProtrusion, 0.0f, headBaseToEyeHeight);
            Vec3 shiftedEyePos = position + rollPitchYaw * eyeCenterInHeadFrame;
            shiftedEyePos.Z -= eyeCenterInHeadFrame.Z; // Bring the head back down to original height

            // Assign new position
            position = shiftedEyePos;

            right = Vec3.Normalize(Vec3.Cross(direction, up));

            // Apply orientation to movement logic
            if (PlayerIntellect.Instance != null)
            {
                PlayerIntellect.Instance.LookDirection = new SphereDir(-Instance.realYaw - (float)(Math.PI), -pitch);
            }
            
            #endregion

            #region Assign Camera Properties

            float fov    = OVRWrapper.Oculus.fov();
            float aspect = OVRWrapper.Oculus.aspectRatio();

            OVRCompositorInstance compositor = view.Viewport.GetCompositorInstance("OVR") as OVRCompositorInstance;

            OVRWrapper.DistortionConfig config = OVRWrapper.Oculus.distortionConfiguration();

            if (compositor != null)
            {
                //Vec4 lensCenter = new Vec4();
                Vec4 screenScenter = new Vec4();
                Vec4 scale = new Vec4();
                Vec4 scaleIn = new Vec4();
                Vec4 hmdWarpParam = new Vec4();


                hmdWarpParam.X = config.K[0];
                hmdWarpParam.Y = config.K[1];
                hmdWarpParam.Z = config.K[2];
                hmdWarpParam.W = config.K[3];

                //float w = 640.0f / 1280;
                //float h = 800.0f / 800.0f;
                //float viewportAspect = 640.0f / 800.0f;
                float scaleFactor = 1.0f / config.Scale;

                //scale = new Vec4((w / 2) * scaleFactor, (h / 2) * scaleFactor * viewportAspect, 0, 0);
                //scale = new Vec4(1.0f / 3.0f, 1.0f / 3.0f * viewportAspect, 0, 0);
                //scale = new Vec4(0.25f, 0.25f * viewportAspect, 0, 0);
                scale = new Vec4(0.3f, 0.24f, 0, 0);


                //scaleIn.X = scaleIn.Y = 2.0f;
                //scaleIn = new Vec4((2 / w), (2 / h) / viewportAspect, 0, 0);
                //scaleIn = new Vec4(2, 2 / viewportAspect, 0, 0);
                scaleIn = new Vec4(2, 2.5f, 0, 0);

                compositor.lensCenter.Y = 0.5f;// +config.YCenterOffset;

                screenScenter.X = 0.5f;
                screenScenter.Y = 0.5f;

                //compositor.lensCenter = lensCenter;
                compositor.screenScenter = screenScenter;
                compositor.scale = scale;
                compositor.scaleIn = scaleIn;
                compositor.hmdWarpParam = hmdWarpParam;


                float pco = OVRWrapper.Oculus.projectionCenterOffset(); //0.14529906f;

                float lensSeparationDistance = OVRWrapper.Oculus.lensSeparationDistance();
                float hScreenSize = OVRWrapper.Oculus.hScreenSize();

                float hMeters = hScreenSize / 4.0f - lensSeparationDistance / 2.0f;
                float hView = hMeters / hScreenSize;

                if (index == 0) // left eye
                {
                    //compositor.lensCenter.X = 0.5f + pco / 2.0f;
                    compositor.lensCenter.X = 0.5f + hView / 2.0f;
                    compositor.projectionCorrection = -hView;

                    camera.ProjectionType = defaultCamera.ProjectionType;
                    camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                    camera.NearClipDistance = defaultCamera.NearClipDistance;
                    camera.FarClipDistance = defaultCamera.FarClipDistance;
                    camera.AspectRatio = aspect;
                    camera.Fov = fov;
                    camera.FixedUp = up;
                    camera.Direction = direction;
                    camera.Position = position - (IOD / 2.0f) * right;
                }
                if (index == 1) // right eye
                {
                    //compositor.lensCenter.X = 0.5f - pco / 2.0f;
                    compositor.lensCenter.X = 0.5f - hView / 2.0f;
                    compositor.projectionCorrection = hView;

                    camera.ProjectionType = defaultCamera.ProjectionType;
                    camera.OrthoWindowHeight = defaultCamera.OrthoWindowHeight;
                    camera.NearClipDistance = defaultCamera.NearClipDistance;
                    camera.FarClipDistance = defaultCamera.FarClipDistance;
                    camera.AspectRatio = aspect;
                    camera.Fov = fov;
                    camera.FixedUp = up;
                    camera.Direction = direction;
                    camera.Position = position + (IOD / 2.0f) * right;
                }
            }

            #endregion

            //set up material scheme for viewport
            view.Viewport.MaterialScheme = defaultViewport.MaterialScheme;
        }

        public void OnMouseMove(Vec2 mousePosition)
        {
            mouseDiffX = mousePosition.X;
        }

        public bool OnKeyDown(KeyEvent e)
        {
            if (e.Key == EKeys.LAlt)
            {
                // Reset Tracking
                _trackingInitialized = false;
            }

            //_direction = Mat3.FromRotateByZ(_initialYaw) * _direction;

            if (e.Key == EKeys.Home)
            {
                // Advance by tracking look direction
                _debugOffset += 1.0f * _direction;
            }

            if (e.Key == EKeys.End)
            {
                // Advance by tracking look direction
                _debugOffset -= 1.0f * _direction;
            }

            if (e.Key == EKeys.Delete)
            {
                Vec3 up = Vec3.ZAxis;
                Vec3 right = Vec3.Cross(_direction, up).GetNormalize();
                
                // Advance by tracking look direction
                _debugOffset -= 1.0f * right;
            }

            if (e.Key == EKeys.PageDown)
            {
                Vec3 up = Vec3.ZAxis;
                Vec3 right = Vec3.Cross(_direction, up).GetNormalize();

                // Advance by tracking look direction
                _debugOffset += 1.0f * right;
            }

            return true;
        }
    }
}
