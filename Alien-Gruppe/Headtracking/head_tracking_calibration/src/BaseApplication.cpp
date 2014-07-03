/*
-----------------------------------------------------------------------------
Filename:    BaseApplication.cpp
-----------------------------------------------------------------------------


This source file is generated by the
   ___                   _              __    __ _                  _ 
  /___\__ _ _ __ ___    /_\  _ __  _ __/ / /\ \ (_)______ _ _ __ __| |
 //  // _` | '__/ _ \  //_\\| '_ \| '_ \ \/  \/ / |_  / _` | '__/ _` |
/ \_// (_| | | |  __/ /  _  \ |_) | |_) \  /\  /| |/ / (_| | | | (_| |
\___/ \__, |_|  \___| \_/ \_/ .__/| .__/ \/  \/ |_/___\__,_|_|  \__,_|
      |___/                 |_|   |_|                                 
      Ogre 1.7.x Application Wizard for VC10 (August 2010)
      http://code.google.com/p/ogreappwizards/
-----------------------------------------------------------------------------
*/
#include "BaseApplication.h"
#include "globals.h"

#if OGRE_PLATFORM == OGRE_PLATFORM_WIN32
#include "../res/resource.h"
#endif


// help function declaration
viargo::KeyboardKey ogreKey2ViargoKey(OIS::KeyCode keyCode);


//-------------------------------------------------------------------------------------
BaseApplication::BaseApplication(void)
    : mRoot(0),
    mCamera(0),
    mSceneMgr(0),
    mWindow(0),
    mResourcesCfg(Ogre::StringUtil::BLANK),
    mPluginsCfg(Ogre::StringUtil::BLANK),
    mTrayMgr(0),
	mOverlaySystem(0),
    mCameraMan(0),
    mDetailsPanel(0),
    mCursorWasVisible(false),
    mShutDown(false),
    mInputManager(0),
    mMouse(0),
    mKeyboard(0),
	_mouseDragged(false),
	_lastMousePos (viargo::vec2(0, 0)),
	_lastMouseAction(0)
{
}

//-------------------------------------------------------------------------------------
BaseApplication::~BaseApplication(void)
{
    if (mTrayMgr) delete mTrayMgr;
    if (mCameraMan) delete mCameraMan;
	if (mOverlaySystem) delete mOverlaySystem;

    //Remove ourself as a Window listener
    Ogre::WindowEventUtilities::removeWindowEventListener(mWindow, this);
    windowClosed(mWindow);
    delete mRoot;
}

//-------------------------------------------------------------------------------------
bool BaseApplication::configure(void)
{
    // Show the configuration dialog and initialise the system
    // You can skip this and use root.restoreConfig() to load configuration
    // settings if you were sure there are valid ones saved in ogre.cfg
	if(/*mRoot->restoreConfig()||*/mRoot->showConfigDialog())
    {
        // If returned true, user clicked OK so initialise
        // Here we choose to let the system create a default rendering window by passing 'true'
        mWindow = mRoot->initialise(true, "OgreBulletStereo Render Window");

        // Let's add a nice window icon
#if OGRE_PLATFORM == OGRE_PLATFORM_WIN32
        HWND hwnd;
        mWindow->getCustomAttribute("WINDOW", (void*)&hwnd);
        LONG iconID   = (LONG)LoadIcon( GetModuleHandle(0), MAKEINTRESOURCE(IDI_APPICON) );
        SetClassLong( hwnd, GCL_HICON, iconID );
#endif
        return true;
    }
    else
    {
        return false;
    }
}
//-------------------------------------------------------------------------------------
void BaseApplication::chooseSceneManager(void)
{
    // Get the SceneManager, in this case a generic one
    mSceneMgr = mRoot->createSceneManager(Ogre::ST_GENERIC);

	mSceneMgr->addRenderQueueListener(mOverlaySystem);
}
//-------------------------------------------------------------------------------------
void BaseApplication::createCamera(void)
{
    // Create the camera
    mCamera = mSceneMgr->createCamera("PlayerCam");

    // Position it at 500 in Z direction
    mCamera->setPosition(Ogre::Vector3(0,0,80));
	mCamera->setFocalLength(10.0f);
	mCamera->setFrustumOffset(Ogre::Vector2(-0.05f, 0.0f));
    // Look back along -Z
    mCamera->lookAt(Ogre::Vector3(0,0,-300));
    mCamera->setNearClipDistance(5);

	// we are using VIARGO for ALL camera manipulations 
	// -> don't create a default camera controller, since they are not compatible!
    //mCameraMan = new OgreBites::SdkCameraMan(mCamera);
}
//-------------------------------------------------------------------------------------
void BaseApplication::createFrameListener(void)
{
	// init the viargo lib
	//viargo::initialize("../viargo_settings.xml");

	// ...
    Ogre::LogManager::getSingletonPtr()->logMessage("*** Initializing OIS ***");
    OIS::ParamList pl;
    size_t windowHnd = 0;
    std::ostringstream windowHndStr;

    mWindow->getCustomAttribute("WINDOW", &windowHnd);
    windowHndStr << windowHnd;
    pl.insert(std::make_pair(std::string("WINDOW"), windowHndStr.str()));

    mInputManager = OIS::InputManager::createInputSystem( pl );

    mKeyboard = static_cast<OIS::Keyboard*>(mInputManager->createInputObject( OIS::OISKeyboard, true ));
    mMouse = static_cast<OIS::Mouse*>(mInputManager->createInputObject( OIS::OISMouse, true ));

    mMouse->setEventCallback(this);
    mKeyboard->setEventCallback(this);

    //Set initial mouse clipping size
    windowResized(mWindow);

    //Register as a Window listener
    Ogre::WindowEventUtilities::addWindowEventListener(mWindow, this);

	OgreBites::InputContext inputContext;
	inputContext.mMouse = mMouse;
    mTrayMgr = new OgreBites::SdkTrayManager("InterfaceName", mWindow, inputContext, this);
    mTrayMgr->showFrameStats(OgreBites::TL_BOTTOMLEFT);
    //mTrayMgr->showLogo(OgreBites::TL_BOTTOMRIGHT);
   // mTrayMgr->hideCursor();

    // create a params panel for displaying sample details
    Ogre::StringVector items;
    items.push_back("cam.pX");
    items.push_back("cam.pY");
    items.push_back("cam.pZ");
    items.push_back("");
    items.push_back("cam.oW");
    items.push_back("cam.oX");
    items.push_back("cam.oY");
    items.push_back("cam.oZ");
    items.push_back("");
    items.push_back("Filtering");
    items.push_back("Poly Mode");

    mDetailsPanel = mTrayMgr->createParamsPanel(OgreBites::TL_NONE, "DetailsPanel", 200, items);
    mDetailsPanel->setParamValue(9, "Bilinear");
    mDetailsPanel->setParamValue(10, "Solid");
    mDetailsPanel->hide();

    mRoot->addFrameListener(this);
}
//-------------------------------------------------------------------------------------
void BaseApplication::destroyScene(void)
{
}
//-------------------------------------------------------------------------------------
//void BaseApplication::createViewports(void)
//{
//    // Create one viewport, entire window
//    Ogre::Viewport* vp = mWindow->addViewport(mCamera);
//    vp->setBackgroundColour(Ogre::ColourValue(0,0,0));
//
//    // Alter the camera aspect ratio to match the viewport
//    mCamera->setAspectRatio(
//        Ogre::Real(vp->getActualWidth()) / Ogre::Real(vp->getActualHeight()));
//}

void BaseApplication::createViewports(void)
{
    Ogre::String stereoMode = "Monoscopic";
    Ogre::ConfigOptionMap& options = Ogre::Root::getSingleton().getRenderSystem()->getConfigOptions();
    Ogre::ConfigOptionMap::iterator opt = options.find("Stereo Mode");
    
	if (opt != options.end()) {
        stereoMode = opt->second.currentValue;
    }

    if (stereoMode == "Quadbuffer") {
        Ogre::Viewport* viewportLeft = mWindow->addViewport(mCamera, 0, 0, 0, 1, 1);
        //viewportLeft->setBackgroundColour(Ogre::ColourValue(0.6, 0.4, 0.4));
        viewportLeft->setBackgroundColour(Ogre::ColourValue(0, 0, 0));

        
        Ogre::Viewport* viewportRight = mWindow->addViewport(mCamera, 1, 0, 0, 1, 1);
        //viewportRight->setBackgroundColour(Ogre::ColourValue(0.4, 0.6, 0.4));
        viewportRight->setBackgroundColour(Ogre::ColourValue(0, 0, 0));

		// set the aspect ratio
        mCamera->setAspectRatio((float)viewportLeft->getActualWidth() / (float)viewportLeft->getActualHeight());
        //mCamera->setAspectRatio(5);

        StereoRenderTargetListener* stereoRTListener = new StereoRenderTargetListener(
			StereoRenderTargetListener::SM_QUADBUFFERED, viewportLeft, viewportRight);

        mWindow->addListener(stereoRTListener);
    }

    else if (stereoMode == "Side-by-side") {
        Ogre::Viewport* viewportLeft = mWindow->addViewport(mCamera, 0, 0, 0, 0.5, 1);
        //viewportLeft->setBackgroundColour(Ogre::ColourValue(0.6, 0.4, 0.4));
        viewportLeft->setBackgroundColour(Ogre::ColourValue(0, 0, 0));

        
        Ogre::Viewport* viewportRight = mWindow->addViewport(mCamera, 1, 0.5, 0, 0.5, 1);
        //viewportRight->setBackgroundColour(Ogre::ColourValue(0.4, 0.6, 0.4));
        viewportLeft->setBackgroundColour(Ogre::ColourValue(0, 0, 0));

        // set the aspect ratio
        mCamera->setAspectRatio((float)viewportLeft->getActualWidth() / (float)viewportLeft->getActualHeight());

        StereoRenderTargetListener* stereoRTListener = new StereoRenderTargetListener(
			StereoRenderTargetListener::SM_SIDEBYSIDE, viewportLeft, viewportRight);
        
		mWindow->addListener(stereoRTListener);
    }

    else {
        Ogre::Viewport* viewport = mWindow->addViewport(mCamera);
        viewport->setBackgroundColour(Ogre::ColourValue(0, 0, 0));

        // set the aspect ratio
        mCamera->setAspectRatio((float)viewport->getActualWidth() / (float)viewport->getActualHeight());

        StereoRenderTargetListener* stereoRTListener = new StereoRenderTargetListener(
			StereoRenderTargetListener::SM_MONOSCOPIC);
        
		mWindow->addListener(stereoRTListener);
    }
}


//-------------------------------------------------------------------------------------
void BaseApplication::setupResources(void)
{
    // Load resource paths from config file
    Ogre::ConfigFile cf;
    cf.load(mResourcesCfg);

    // Go through all sections & settings in the file
    Ogre::ConfigFile::SectionIterator seci = cf.getSectionIterator();

    Ogre::String secName, typeName, archName;
    while (seci.hasMoreElements())
    {
        secName = seci.peekNextKey();
        Ogre::ConfigFile::SettingsMultiMap *settings = seci.getNext();
        Ogre::ConfigFile::SettingsMultiMap::iterator i;
        for (i = settings->begin(); i != settings->end(); ++i)
        {
            typeName = i->first;
            archName = i->second;
            Ogre::ResourceGroupManager::getSingleton().addResourceLocation(
                archName, typeName, secName);
        }
    }
}
//-------------------------------------------------------------------------------------
void BaseApplication::createResourceListener(void)
{

}
//-------------------------------------------------------------------------------------
void BaseApplication::loadResources(void)
{
    Ogre::ResourceGroupManager::getSingleton().initialiseAllResourceGroups();
}
//-------------------------------------------------------------------------------------
void BaseApplication::go(void)
{
#ifdef _DEBUG
    mResourcesCfg = "resources_d.cfg";
    mPluginsCfg = "plugins_d.cfg";
#else
    mResourcesCfg = "resources.cfg";
    mPluginsCfg = "plugins.cfg";
#endif

    if (!setup())
        return;

    mRoot->startRendering();


    // clean up
    destroyScene();
}
//-------------------------------------------------------------------------------------
bool BaseApplication::setup(void)
{
    mRoot = new Ogre::Root(mPluginsCfg);

	mOverlaySystem = new Ogre::OverlaySystem();

    setupResources();

    bool carryOn = configure();
    if (!carryOn) return false;

    chooseSceneManager();
    createCamera();
    createViewports();

    // Set default mipmap level (NB some APIs ignore this)
    Ogre::TextureManager::getSingleton().setDefaultNumMipmaps(3);

    // Create any resource listeners (for loading screens)
    createResourceListener();
    // Load resources
    loadResources();

    createFrameListener();

    // Create the scene
    createScene();

    return true;
};
//-------------------------------------------------------------------------------------
bool BaseApplication::frameStarted(const Ogre::FrameEvent& evt) {
	
	//// sync the viargo camera with the ogre cam
	//viargo::Camera& vcam = Viargo.camera("main");
	//Ogre::Vector3 pos = mCamera->getPosition();
	//Ogre::Vector3 dir = mCamera->getDirection();
	//Ogre::Vector3 rv  = mCamera->getRight();

	//viargo::vec3 eye = viargo::vec3(pos.x, pos.y, pos.z);
	//viargo::vec3 at  = eye + viargo::vec3(dir.x, dir.y, dir.z) * vcam.focalLength();
	//viargo::vec3 up  = viargo::normalize(viargo::cross(viargo::vec3(rv.x, rv.y, rv.z), at - eye));

	//vcam.setLookAt(eye, at, up);

	//float left, right, bottom, top, znear, zfar;
	//mCamera->getFrustumExtents(left, right, top, bottom);
	//znear = mCamera->getNearClipDistance();
	//zfar  = mCamera->getFarClipDistance();

	//vcam.setFrustum(left, right, bottom, top, znear, zfar);

	// update the Viargo state (workout the events)
	Viargo.update(evt.timeSinceLastFrame * 1000.0f);

	return true;
}

//-------------------------------------------------------------------------------------
bool BaseApplication::frameRenderingQueued(const Ogre::FrameEvent& evt)
{
    if(mWindow->isClosed())
        return false;

    if(mShutDown)
        return false;

    //Need to capture/update each device
    mKeyboard->capture();
    mMouse->capture();

    mTrayMgr->frameRenderingQueued(evt);

    if (!mTrayMgr->isDialogVisible())
    {
        if (mCameraMan)
			mCameraMan->frameRenderingQueued(evt);   // if dialog isn't up, then update the camera
        
		if (mDetailsPanel->isVisible())   // if details panel is visible, then update its contents
        {
            mDetailsPanel->setParamValue(0, Ogre::StringConverter::toString(mCamera->getDerivedPosition().x));
            mDetailsPanel->setParamValue(1, Ogre::StringConverter::toString(mCamera->getDerivedPosition().y));
            mDetailsPanel->setParamValue(2, Ogre::StringConverter::toString(mCamera->getDerivedPosition().z));
            mDetailsPanel->setParamValue(4, Ogre::StringConverter::toString(mCamera->getDerivedOrientation().w));
            mDetailsPanel->setParamValue(5, Ogre::StringConverter::toString(mCamera->getDerivedOrientation().x));
            mDetailsPanel->setParamValue(6, Ogre::StringConverter::toString(mCamera->getDerivedOrientation().y));
            mDetailsPanel->setParamValue(7, Ogre::StringConverter::toString(mCamera->getDerivedOrientation().z));
        }
    }

	//if (_mouseDragged) {
	//	//Viargo.dispatchEvent(new viargo::RawMultiTouchEvent("OgreMouseEvent",
	//														viargo::RawMultiTouchEvent::CURSOR_2D,
	//														viargo::RawMultiTouchEvent::UPDATED,
	//														0, 0,
	//														viargo::vec3((_lastMousePos.x / mCamera->getViewport()->getActualWidth()), (_lastMousePos.y / mCamera->getViewport()->getActualHeight()), 0)));
	//}
	//if ((!_mouseDragged) && ((std::clock() - _lastMouseAction) > CLOCKS_PER_SEC/2)) {
	//	mTrayMgr->hideCursor();
	//}
    return true;
}
//-------------------------------------------------------------------------------------
bool BaseApplication::keyPressed( const OIS::KeyEvent &arg )
{
	// inject the key event in the viargo engine
	Viargo.dispatchEvent(new viargo::KeyEvent("OgreEngine", viargo::KeyEvent::KEY_PRESSED, ogreKey2ViargoKey(arg.key)));

    if (mTrayMgr->isDialogVisible()) return true;   // don't process any more keys if dialog is up

    if (arg.key == OIS::KC_F)   // toggle visibility of advanced frame stats
    {
        mTrayMgr->toggleAdvancedFrameStats();
    }
    else if (arg.key == OIS::KC_G)   // toggle visibility of even rarer debugging details
    {
        if (mDetailsPanel->getTrayLocation() == OgreBites::TL_NONE)
        {
            mTrayMgr->moveWidgetToTray(mDetailsPanel, OgreBites::TL_TOPRIGHT, 0);
            mDetailsPanel->show();
        }
        else
        {
            mTrayMgr->removeWidgetFromTray(mDetailsPanel);
            mDetailsPanel->hide();
        }
    }
    else if (arg.key == OIS::KC_T)   // cycle polygon rendering mode
    {
        Ogre::String newVal;
        Ogre::TextureFilterOptions tfo;
        unsigned int aniso;

        switch (mDetailsPanel->getParamValue(9).asUTF8()[0])
        {
        case 'B':
            newVal = "Trilinear";
            tfo = Ogre::TFO_TRILINEAR;
            aniso = 1;
            break;
        case 'T':
            newVal = "Anisotropic";
            tfo = Ogre::TFO_ANISOTROPIC;
            aniso = 8;
            break;
        case 'A':
            newVal = "None";
            tfo = Ogre::TFO_NONE;
            aniso = 1;
            break;
        default:
            newVal = "Bilinear";
            tfo = Ogre::TFO_BILINEAR;
            aniso = 1;
        }

        Ogre::MaterialManager::getSingleton().setDefaultTextureFiltering(tfo);
        Ogre::MaterialManager::getSingleton().setDefaultAnisotropy(aniso);
        mDetailsPanel->setParamValue(9, newVal);
    }
    else if (arg.key == OIS::KC_R)   // cycle polygon rendering mode
    {
        Ogre::String newVal;
        Ogre::PolygonMode pm;

        switch (mCamera->getPolygonMode())
        {
        case Ogre::PM_SOLID:
            newVal = "Wireframe";
            pm = Ogre::PM_WIREFRAME;
            break;
        case Ogre::PM_WIREFRAME:
            newVal = "Points";
            pm = Ogre::PM_POINTS;
            break;
        default:
            newVal = "Solid";
            pm = Ogre::PM_SOLID;
        }

        mCamera->setPolygonMode(pm);
        mDetailsPanel->setParamValue(10, newVal);
    }
    else if(arg.key == OIS::KC_F5)   // refresh all textures
    {
        Ogre::TextureManager::getSingleton().reloadAll();
    }
    else if (arg.key == OIS::KC_SYSRQ)   // take a screenshot
    {
		mWindow->writeContentsToTimestampedFile("screenshot", ".jpg");
    }
    else if (arg.key == OIS::KC_ESCAPE)
    {
        mShutDown = true;
    }
	
	if (mCameraMan)
		mCameraMan->injectKeyDown(arg);
    return true;
}

bool BaseApplication::keyReleased( const OIS::KeyEvent &arg )
{
	// inject the key event in the viargo engine
	Viargo.dispatchEvent(new viargo::KeyEvent("OgreEngine", viargo::KeyEvent::KEY_RELEASED, ogreKey2ViargoKey(arg.key)));

    if (mCameraMan)
		mCameraMan->injectKeyUp(arg);
    return true;
}

bool BaseApplication::mouseMoved( const OIS::MouseEvent &arg )
{
	//mTrayMgr->showCursor();
	//_lastMouseAction = std::clock();
	
	// inject the mouse event in the viargo engine
	Viargo.dispatchEvent(new viargo::MouseEvent("OgreEngine", arg.state.X.abs, arg.state.Y.abs, arg.state.Z.abs, 
		viargo::MouseEvent::MOTION, (viargo::MouseEvent::MouseButtons)arg.state.buttons));

	//std::cout << "Mouse: " << arg.state.X.abs / arg.state.width << ", " << arg.state.Y.abs / arg.state.height << std::endl;

	// inject a RawTouchEvent with the mouse coordinates (*CW*)
	// *** PROBLEM BEI DIESER METHODE: ***
	// Der Touch wird nur aktualisiert, wenn man die Maus bewegt. Somit geht er bei der ersten kurzen Ruhepause der Maus verloren.
	// --> wie k�nnte man das beheben?
/*	if ((arg.state.buttons & ( 1L << OIS::MB_Left )) != 0) {
		Viargo.dispatchEvent(new viargo::RawMultiTouchEvent("OgreMouseEvent",
															viargo::RawMultiTouchEvent::CURSOR_2D,
															viargo::RawMultiTouchEvent::UPDATED,
															0, 0,
															viargo::vec3(2*arg.state.X.abs / mCamera->getViewport()->getActualWidth() - 1, 2*arg.state.Y.abs / mCamera->getViewport()->getActualHeight() - 1, arg.state.Z.abs)));
	} */
	//_lastMousePos.x = arg.state.X.abs;
	//_lastMousePos.y = arg.state.Y.abs;

	if (mTrayMgr->injectMouseMove(arg)) return true;
    if (mCameraMan)
		mCameraMan->injectMouseMove(arg);
    return true;
}

bool BaseApplication::mousePressed( const OIS::MouseEvent &arg, OIS::MouseButtonID id )
{
	// inject the mouse event in the viargo engine
	Viargo.dispatchEvent(new viargo::MouseEvent("OgreEngine", arg.state.X.abs, arg.state.Y.abs, arg.state.Z.abs, 
		viargo::MouseEvent::PRESSED, (viargo::MouseEvent::MouseButtons)arg.state.buttons));

	//// inject a RawTouchEvent with the mouse coordinates (*CW*)
	//if ((arg.state.buttons & ( 1L << OIS::MB_Left )) != 0) {
	//	Viargo.dispatchEvent(new viargo::RawMultiTouchEvent("OgreMouseEvent",
	//														viargo::RawMultiTouchEvent::CURSOR_2D,
	//														viargo::RawMultiTouchEvent::APPEARED,
	//														0, 0,
	//														viargo::vec3((float)arg.state.X.abs / mCamera->getViewport()->getActualWidth(), (float)arg.state.Y.abs / mCamera->getViewport()->getActualHeight(), arg.state.Z.abs)));
	//	_mouseDragged = true;
	//}
	if (mTrayMgr->injectMouseDown(arg, id)) return true;
    if (mCameraMan)
		mCameraMan->injectMouseDown(arg, id);
    return true;
}

bool BaseApplication::mouseReleased( const OIS::MouseEvent &arg, OIS::MouseButtonID id )
{
	// inject the mouse event in the viargo engine
	Viargo.dispatchEvent(new viargo::MouseEvent("OgreEngine", arg.state.X.abs, arg.state.Y.abs, arg.state.Z.abs, 
		viargo::MouseEvent::RELEASED, (viargo::MouseEvent::MouseButtons)arg.state.buttons));

	//// inject a RawTouchEvent with the mouse coordinates (*CW*)
	//if ((arg.state.buttons & ( 1L << OIS::MB_Left )) == 0) {
	//	_mouseDragged = false;
	//	Viargo.dispatchEvent(new viargo::RawMultiTouchEvent("OgreMouseEvent",
	//														viargo::RawMultiTouchEvent::CURSOR_2D,
	//														viargo::RawMultiTouchEvent::DISAPPEARED,
	//														0, 0,
	//														viargo::vec3((float)arg.state.X.abs / mCamera->getViewport()->getActualWidth(), (float)arg.state.Y.abs / mCamera->getViewport()->getActualHeight(), arg.state.Z.abs)));
	//}
    if (mTrayMgr->injectMouseUp(arg, id)) return true;
    if (mCameraMan)
		mCameraMan->injectMouseUp(arg, id);
    return true;
}

//Adjust mouse clipping area
void BaseApplication::windowResized(Ogre::RenderWindow* rw)
{
    unsigned int width, height, depth;
    int left, top;
    rw->getMetrics(width, height, depth, left, top);

    const OIS::MouseState &ms = mMouse->getMouseState();
    ms.width = width;
    ms.height = height;
}

//Unattach OIS before window shutdown (very important under Linux)
void BaseApplication::windowClosed(Ogre::RenderWindow* rw)
{
    //Only close for window that created OIS (the main window in these demos)
    if( rw == mWindow )
    {
        if( mInputManager )
        {
            mInputManager->destroyInputObject( mMouse );
            mInputManager->destroyInputObject( mKeyboard );

            OIS::InputManager::destroyInputSystem(mInputManager);
            mInputManager = 0;
        }
    }
}

//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
viargo::KeyboardKey ogreKey2ViargoKey(OIS::KeyCode keyCode) {

	switch (keyCode) {
		case OIS::KC_UNASSIGNED: return viargo::KeyboardKey::KEY_UNKNOWN; 	break;
		case OIS::KC_ESCAPE: 	return viargo::KeyboardKey::KEY_ESCAPE; 		break;
		case OIS::KC_1: 			return viargo::KeyboardKey::KEY_1; 	break;
		case OIS::KC_2: 			return viargo::KeyboardKey::KEY_2; 	break;
		case OIS::KC_3: 			return viargo::KeyboardKey::KEY_3; 	break;
		case OIS::KC_4: 			return viargo::KeyboardKey::KEY_4; 	break;
		case OIS::KC_5: 			return viargo::KeyboardKey::KEY_5; 	break;
		case OIS::KC_6: 			return viargo::KeyboardKey::KEY_6; 	break;
		case OIS::KC_7: 			return viargo::KeyboardKey::KEY_7; 	break;
		case OIS::KC_8: 			return viargo::KeyboardKey::KEY_8; 	break;
		case OIS::KC_9: 			return viargo::KeyboardKey::KEY_9; 	break;
		case OIS::KC_0: 			return viargo::KeyboardKey::KEY_0; 	break;
		case OIS::KC_MINUS: 		return viargo::KeyboardKey::KEY_MINUS; 		break;    // - on main keyboard
		case OIS::KC_EQUALS: 	return viargo::KeyboardKey::KEY_EQUALS; 		break;
		//case OIS::KC_BACK: 		return viargo::KeyboardKey::KEY_BACK; 		break;    // backspace
		case OIS::KC_TAB: 		return viargo::KeyboardKey::KEY_TAB; 		break;
		case OIS::KC_Q: 			return viargo::KeyboardKey::KEY_Q; break;
		case OIS::KC_W: 			return viargo::KeyboardKey::KEY_W; break;
		case OIS::KC_E: 			return viargo::KeyboardKey::KEY_E; break;
		case OIS::KC_R: 			return viargo::KeyboardKey::KEY_R; break;
		case OIS::KC_T: 			return viargo::KeyboardKey::KEY_T; break;
		case OIS::KC_Y: 			return viargo::KeyboardKey::KEY_Y; break;
		case OIS::KC_U: 			return viargo::KeyboardKey::KEY_U; break;
		case OIS::KC_I: 			return viargo::KeyboardKey::KEY_I; break;
		case OIS::KC_O: 			return viargo::KeyboardKey::KEY_O; break;
		case OIS::KC_P: 			return viargo::KeyboardKey::KEY_P; break;
		case OIS::KC_LBRACKET: 	return viargo::KeyboardKey::KEY_LEFTBRACKET; 	break;
		case OIS::KC_RBRACKET: 	return viargo::KeyboardKey::KEY_RIGHTBRACKET; 	break;
		case OIS::KC_RETURN: 	return viargo::KeyboardKey::KEY_ENTER; 		break;    // Enter on main keyboard
		case OIS::KC_LCONTROL: 	return viargo::KeyboardKey::KEY_LCTRL; 	break;
		case OIS::KC_A: 			return viargo::KeyboardKey::KEY_A; break;
		case OIS::KC_S: 			return viargo::KeyboardKey::KEY_S; break;
		case OIS::KC_D: 			return viargo::KeyboardKey::KEY_D; break;
		case OIS::KC_F: 			return viargo::KeyboardKey::KEY_F; break;
		case OIS::KC_G: 			return viargo::KeyboardKey::KEY_G; break;
		case OIS::KC_H: 			return viargo::KeyboardKey::KEY_H; break;
		case OIS::KC_J: 			return viargo::KeyboardKey::KEY_J; break;
		case OIS::KC_K: 			return viargo::KeyboardKey::KEY_K; break;
		case OIS::KC_L: 			return viargo::KeyboardKey::KEY_L; break;
		case OIS::KC_SEMICOLON: 	return viargo::KeyboardKey::KEY_SEMICOLON; 	break;
		//case OIS::KC_APOSTROPHE: return viargo::KeyboardKey::KEY_APOSTROPHE; 	break;
		//case OIS::KC_GRAVE: 		return viargo::KeyboardKey::KEY_GRAVE; 		break;    // accent
		case OIS::KC_LSHIFT: 	return viargo::KeyboardKey::KEY_LSHIFT; 		break;
		case OIS::KC_BACKSLASH: 	return viargo::KeyboardKey::KEY_BACKSLASH; 	break;
		case OIS::KC_Z: 			return viargo::KeyboardKey::KEY_Z; break;
		case OIS::KC_X: 			return viargo::KeyboardKey::KEY_X; break;
		case OIS::KC_C: 			return viargo::KeyboardKey::KEY_C; break;
		case OIS::KC_V: 			return viargo::KeyboardKey::KEY_V; break;
		case OIS::KC_B: 			return viargo::KeyboardKey::KEY_B; break;
		case OIS::KC_N: 			return viargo::KeyboardKey::KEY_N; break;
		case OIS::KC_M: 			return viargo::KeyboardKey::KEY_M; break;
		case OIS::KC_COMMA: 		return viargo::KeyboardKey::KEY_COMMA; 		break;
		case OIS::KC_PERIOD: 	return viargo::KeyboardKey::KEY_PERIOD;		break;    // . on main keyboard
		case OIS::KC_SLASH: 		return viargo::KeyboardKey::KEY_SLASH; 		break;    // / on main keyboard
		case OIS::KC_RSHIFT: 	return viargo::KeyboardKey::KEY_RSHIFT; 		break;
		case OIS::KC_MULTIPLY: 	return viargo::KeyboardKey::KEY_ASTERISK; 	break;    // * on numeric keypad
		//case OIS::KC_LMENU: 		return viargo::KeyboardKey::KEY_LMENU; 		break;    // left Alt
		case OIS::KC_SPACE: 		return viargo::KeyboardKey::KEY_SPACE; 		break;
		//case OIS::KC_CAPITAL: 	return viargo::KeyboardKey::KEY_CAPITAL; 	break;
		case OIS::KC_F1: 		return viargo::KeyboardKey::KEY_F1; break;
		case OIS::KC_F2: 		return viargo::KeyboardKey::KEY_F2; break;
		case OIS::KC_F3: 		return viargo::KeyboardKey::KEY_F3; break;
		case OIS::KC_F4: 		return viargo::KeyboardKey::KEY_F4; break;
		case OIS::KC_F5: 		return viargo::KeyboardKey::KEY_F5; break;
		case OIS::KC_F6: 		return viargo::KeyboardKey::KEY_F6; break;
		case OIS::KC_F7: 		return viargo::KeyboardKey::KEY_F7; break;
		case OIS::KC_F8: 		return viargo::KeyboardKey::KEY_F8; break;
		case OIS::KC_F9: 		return viargo::KeyboardKey::KEY_F9; break;
		case OIS::KC_F10: 		return viargo::KeyboardKey::KEY_F10; break;
		case OIS::KC_NUMLOCK: 	return viargo::KeyboardKey::KEY_NUMLOCK; 	break;
		//case OIS::KC_SCROLL: 	return viargo::KeyboardKey::KEY_SCROLL; 		break;    // Scroll Lock
		case OIS::KC_NUMPAD7: 	return viargo::KeyboardKey::KEY_KP7; 	break;
		case OIS::KC_NUMPAD8: 	return viargo::KeyboardKey::KEY_KP8; 	break;
		case OIS::KC_NUMPAD9: 	return viargo::KeyboardKey::KEY_KP9; 	break;
		case OIS::KC_SUBTRACT: 	return viargo::KeyboardKey::KEY_KP_MINUS; 	break;    // - on numeric keypad
		case OIS::KC_NUMPAD4: 	return viargo::KeyboardKey::KEY_KP4; break;
		case OIS::KC_NUMPAD5: 	return viargo::KeyboardKey::KEY_KP5; break;
		case OIS::KC_NUMPAD6: 	return viargo::KeyboardKey::KEY_KP6; break;
		case OIS::KC_ADD: 		return viargo::KeyboardKey::KEY_KP_PLUS; 	break;    // + on numeric keypad
		case OIS::KC_NUMPAD1: 	return viargo::KeyboardKey::KEY_KP1; break;
		case OIS::KC_NUMPAD2: 	return viargo::KeyboardKey::KEY_KP2; break;
		case OIS::KC_NUMPAD3: 	return viargo::KeyboardKey::KEY_KP3; break;
		case OIS::KC_NUMPAD0: 	return viargo::KeyboardKey::KEY_KP0; break;
		case OIS::KC_DECIMAL: 	return viargo::KeyboardKey::KEY_KP_PERIOD; break;    // . on numeric keypad
		//case OIS::KC_OEM: 		return viargo::KeyboardKey::KEY_OEM; break;    // < > | on UK/Germany keyboards
		case OIS::KC_F11: 		return viargo::KeyboardKey::KEY_F11; break;
		case OIS::KC_F12: 		return viargo::KeyboardKey::KEY_F12; break;
		case OIS::KC_F13: 		return viargo::KeyboardKey::KEY_F13; break;    //                     (NEC PC98)
		case OIS::KC_F14: 		return viargo::KeyboardKey::KEY_F14; break;    //                     (NEC PC98)
		case OIS::KC_F15: 		return viargo::KeyboardKey::KEY_F15; break;    //                     (NEC PC98)
		//case OIS::KC_KANA: 		return viargo::KeyboardKey::KEY_KANA; 		break;    // (Japanese keyboard)
		//case OIS::KC_ABNT: 		return viargo::KeyboardKey::KEY_ABNT; 		break;    // / ? on Portugese (Brazilian) keyboards
		//case OIS::KC_CONVERT: 	return viargo::KeyboardKey::KEY_CONVERT; 	break;    // (Japanese keyboard)
		//case OIS::KC_NOCONVERT: 	return viargo::KeyboardKey::KEY_NOCONVERT; 	break;    // (Japanese keyboard)
		//case OIS::KC_YEN: 		return viargo::KeyboardKey::KEY_YEN; 	break;    // (Japanese keyboard)
		//case OIS::KC_ABNT: 		return viargo::KeyboardKey::KEY_ABNT; 	break;    // Numpad . on Portugese (Brazilian) keyboards
		case OIS::KC_NUMPADEQUALS: 	return viargo::KeyboardKey::KEY_KP_EQUALS; 	break;    // = on numeric keypad (NEC PC98)
		//case OIS::KC_PREVTRACK: 	return viargo::KeyboardKey::KEY_PREVTRACK; 			break;    // Previous Track (OIS::KC_CIRCUMFLEX on Japanese keyboard)
		case OIS::KC_AT: 		return viargo::KeyboardKey::KEY_AT; 			break;    //                     (NEC PC98)
		case OIS::KC_COLON: 		return viargo::KeyboardKey::KEY_COLON; 		break;    //                     (NEC PC98)
		case OIS::KC_UNDERLINE: 	return viargo::KeyboardKey::KEY_UNDERSCORE; 	break;    //                     (NEC PC98)
		//case OIS::KC_KANJI: 		return viargo::KeyboardKey::KEY_KANJI; 		break;    // (Japanese keyboard)
		//case OIS::KC_STOP: 		return viargo::KeyboardKey::KEY_STOP; 		break;    //                     (NEC PC98)
		//case OIS::KC_AX: 		return viargo::KeyboardKey::KEY_AX; 			break;    //                     (Japan AX)
		//case OIS::KC_UNLABELED: 	return viargo::KeyboardKey::KEY_UNLABELED; 	break;    //                        (J3100)
		//case OIS::KC_NEXTTRACK: 	return viargo::KeyboardKey::KEY_NEXTTRACK; 	break;    // Next Track
		case OIS::KC_NUMPADENTER: 	return viargo::KeyboardKey::KEY_KP_ENTER; break;    // Enter on numeric keypad
		case OIS::KC_RCONTROL: 	return viargo::KeyboardKey::KEY_RCTRL; 		break;
		//case OIS::KC_MUTE: return viargo::KeyboardKey::KEY_MUTE; 				break;    // Mute
		//case OIS::KC_CALCULATOR: return viargo::KeyboardKey::KEY_CALCULATOR; 	break;    // Calculator
		//case OIS::KC_PLAYPAUSE: 	return viargo::KeyboardKey::KEY_PLAYPAUSE; 	break;    // Play / Pause
		//case OIS::KC_MEDIASTOP: 	return viargo::KeyboardKey::KEY_MEDIASTOP; 	break;    // Media Stop
		//case OIS::KC_VOLUMEDOWN: return viargo::KeyboardKey::KEY_VOLUMEDOWN; 	break;    // Volume -
		//case OIS::KC_VOLUMEUP: 	return viargo::KeyboardKey::KEY_VOLUMEUP; 	break;    // Volume +
		//case OIS::KC_WEBHOME: 	return viargo::KeyboardKey::KEY_WEBHOME; 	break;    // Web home
		case OIS::KC_NUMPADCOMMA: 	return viargo::KeyboardKey::KEY_KP_PERIOD; 	break;    // , on numeric keypad (NEC PC98)
		case OIS::KC_DIVIDE: 	return viargo::KeyboardKey::KEY_KP_DIVIDE; 		break;    // / on numeric keypad
		case OIS::KC_SYSRQ: 		return viargo::KeyboardKey::KEY_SYSREQ; 		break;
		case OIS::KC_RMENU: 		return viargo::KeyboardKey::KEY_MENU; 	break;    // right Alt
		case OIS::KC_PAUSE: 		return viargo::KeyboardKey::KEY_PAUSE; 	break;    // Pause
		case OIS::KC_HOME: 		return viargo::KeyboardKey::KEY_HOME; 	break;    // Home on arrow keypad
		case OIS::KC_UP: 		return viargo::KeyboardKey::KEY_UP; 		break;    // UpArrow on arrow keypad
		case OIS::KC_PGUP: 		return viargo::KeyboardKey::KEY_PAGEUP; 	break;    // PgUp on arrow keypad
		case OIS::KC_LEFT: 		return viargo::KeyboardKey::KEY_LEFT; 	break;    // LeftArrow on arrow keypad
		case OIS::KC_RIGHT: 		return viargo::KeyboardKey::KEY_RIGHT; 	break;    // RightArrow on arrow keypad
		case OIS::KC_END: 		return viargo::KeyboardKey::KEY_END; 	break;    // End on arrow keypad
		case OIS::KC_DOWN: 		return viargo::KeyboardKey::KEY_DOWN; 	break;    // DownArrow on arrow keypad
		case OIS::KC_PGDOWN: 	return viargo::KeyboardKey::KEY_PAGEDOWN; 	break;    // PgDn on arrow keypad
		case OIS::KC_INSERT: 	return viargo::KeyboardKey::KEY_INSERT; 	break;    // Insert on arrow keypad
		case OIS::KC_DELETE: 	return viargo::KeyboardKey::KEY_DELETE; 	break;    // Delete on arrow keypad
		//case OIS::KC_LWIN: 		return viargo::KeyboardKey::KEY_LWIN; 	break;    // Left Windows key
		//case OIS::KC_RWIN: 		return viargo::KeyboardKey::KEY_RWIN; 	break;    // Right Windows key
		//case OIS::KC_APPS: 		return viargo::KeyboardKey::KEY_APPS; 	break;    // AppMenu key
		case OIS::KC_POWER: 		return viargo::KeyboardKey::KEY_POWER; 	break;    // System Power
		//case OIS::KC_SLEEP: 		return viargo::KeyboardKey::KEY_SLEEP; 	break;    // System Sleep
		//case OIS::KC_WAKE: 		return viargo::KeyboardKey::KEY_WAKE; 	break;    // System Wake
		//case OIS::KC_WEBSEARCH: 	return viargo::KeyboardKey::KEY_WEBSEARCH; 			break;    // Web Search
		//case OIS::KC_WEBFAVORITES: 	return viargo::KeyboardKey::KEY_WEBFAVORITES; 	break;    // Web Favorites
		//case OIS::KC_WEBREFRESH: return viargo::KeyboardKey::KEY_WEBREFRESH; 	break;    // Web Refresh
		//case OIS::KC_WEBSTOP: 	return viargo::KeyboardKey::KEY_WEBSTOP; 	break;    // Web Stop
		//case OIS::KC_WEBFORWARD: return viargo::KeyboardKey::KEY_WEBFORWARD; 	break;    // Web Forward
		//case OIS::KC_WEBBACK: 	return viargo::KeyboardKey::KEY_WEBBACK; 	break;    // Web Back
		//case OIS::KC_MYCOMPUTER: return viargo::KeyboardKey::KEY_MYCOMPUTER; 	break;    // My Computer
		//case OIS::KC_MAIL: 		return viargo::KeyboardKey::KEY_MAIL; 		break;    // Mail
	}

	return viargo::KeyboardKey::KEY_UNKNOWN;
}



