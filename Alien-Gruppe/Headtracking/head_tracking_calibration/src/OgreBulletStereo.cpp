#include "OgreBulletStereo.h"
#include "HeadTrackingMetaphor.h"
#include "StereoBeamerCalibrationMetaphor.h"
#include "globals.h"

#include "RTTTargetListener.h"

#include "tracking/viargo_head_tracking_device.h"
#include "tracking/viargo_head_tracking_filter_metaphor.h"
#include "tracking/viargo_ogre_head_tracking_metaphor.h"
#include "tracking/viargo_ogre_head_tracking_calibration_metaphor.h"

//-------------------------------------------------------------------------------------
OgreBulletStereo::OgreBulletStereo()
{
}
//-------------------------------------------------------------------------------------
OgreBulletStereo::~OgreBulletStereo()
{
}

//-------------------------------------------------------------------------------------
bool OgreBulletStereo::frameStarted(const Ogre::FrameEvent& evt) {
	bool result = BaseApplication::frameStarted(evt);
	return result;
}

bool OgreBulletStereo::frameEnded(const Ogre::FrameEvent& evt) {
	return BaseApplication::frameEnded(evt);
}

bool OgreBulletStereo::frameRenderingQueued(const Ogre::FrameEvent& evt) {
	return BaseApplication::frameRenderingQueued(evt);
}


//-------------------------------------------------------------------------------------
void OgreBulletStereo::chooseSceneManager()
{
    // Get the SceneManager, in this case a generic one
    mSceneMgr		= mRoot->createSceneManager(Ogre::ST_GENERIC);
	mViewerSceneMgr = mRoot->createSceneManager(Ogre::ST_GENERIC);
	mSceneMgr->addRenderQueueListener(mOverlaySystem);
}

void OgreBulletStereo::createCamera()
{
    // Create the camera
    mCamera = mSceneMgr->createCamera("PlayerCamera");
    mCamera->setPosition(60, 200, 70);
	mCamera->lookAt(0,0,0);
    mCamera->setNearClipDistance(5);

	/*Ogre::Plane plane = Ogre::Plane(Ogre::Vector3(0.0f, 0.0f, 1.0f), 0.0f);

	mCamera->enableCustomNearClipPlane(plane);*/

	// Create a default camera controller
    mCameraMan = 0;//new OgreBites::SdkCameraMan(mCamera); 

	// Create the viewer camera
	mViewerCamera = mViewerSceneMgr->createCamera("ViewerCam");
	mViewerCamera->setPosition(Ogre::Vector3(0,0,80));
    mViewerCamera->lookAt(Ogre::Vector3(0,0,-300));
    mViewerCamera->setNearClipDistance(5);  
}

//-------------------------------------------------------------------------------------
Ogre::RenderTexture* OgreBulletStereo::createRenderTexture(bool isLeft) 
{
	std::string app = (isLeft ? "Left" : "Right");

	// Create the RTT texture
	Ogre::TexturePtr rtt_texture = Ogre::TextureManager::getSingleton().createManual("RttTex" + app, 
				Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME, 
				Ogre::TEX_TYPE_2D, 
				mWindow->getWidth(), 
				mWindow->getHeight(), 
				0, 
				Ogre::PF_R8G8B8, 
				Ogre::TU_RENDERTARGET);

	Ogre::RenderTexture* renderTexture = rtt_texture->getBuffer()->getRenderTarget();
	
	// add the main camera to the RTT
	renderTexture->addViewport(mCamera);

	// add RenderTarget listener for syncronization with Viargo
	RTTTargetListener* lsn = new RTTTargetListener("main", isLeft, _stereoMode);
	renderTexture->addListener(lsn);

	renderTexture->getViewport(0)->setClearEveryFrame(true);
	renderTexture->getViewport(0)->setBackgroundColour(Ogre::ColourValue::Black);
	renderTexture->getViewport(0)->setOverlaysEnabled(true);
	renderTexture->setAutoUpdated(true); // Updated in RenderTargetListener manually

	return renderTexture;
}

//-------------------------------------------------------------------------------------
void OgreBulletStereo::createViewerScreen(bool isLeft) 
{
	std::string app = (isLeft ? "Left" : "Right");

	// create a rendering texture
	if (isLeft)
		mRenderTextureLeft = createRenderTexture(isLeft);
	else
		mRenderTextureRight = createRenderTexture(isLeft);

	// create a manual object to display the rendering texture
	Ogre::ManualObject* viewerScreen 
		= mViewerSceneMgr->createManualObject("ViewerScreen" + app);
	
	// Use identity view/projection matrices
	viewerScreen->setUseIdentityProjection(true);
	viewerScreen->setUseIdentityView(true);

	// Side by side & left
	if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE  && isLeft) {
		viewerScreen->begin("RTTViewerMaterial" + app, Ogre::RenderOperation::OT_TRIANGLE_LIST);
			viewerScreen->position(-1.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 1.0f);
			viewerScreen->position(0.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 1.0f);
			viewerScreen->position(0.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 0.0f);
			viewerScreen->position(-1.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 0.0f);
			 
			viewerScreen->quad(0, 1, 2, 3);
		viewerScreen->end();
	}
	// Side by side & right
	else if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE && !isLeft) {
		viewerScreen->begin("RTTViewerMaterial" + app, Ogre::RenderOperation::OT_TRIANGLE_LIST);
			viewerScreen->position(0.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 1.0f);
			viewerScreen->position(1.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 1.0f);
			viewerScreen->position(1.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 0.0f);
			viewerScreen->position(0.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 0.0f);
			 
			viewerScreen->quad(0, 1, 2, 3);
		viewerScreen->end();
	}
	// Quad & Monoscopic => Fullscreen
	else if (_stereoMode == StereoRenderTargetListener::SM_QUADBUFFERED ||
			 _stereoMode == StereoRenderTargetListener::SM_MONOSCOPIC) {
		viewerScreen->begin("RTTViewerMaterial" + app, Ogre::RenderOperation::OT_TRIANGLE_LIST);
			viewerScreen->position(-1.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 1.0f);
			viewerScreen->position(1.0f, -1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 1.0f);
			viewerScreen->position(1.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(1.0f, 0.0f);
			viewerScreen->position(-1.0f, 1.0f, 0.0f);
			viewerScreen->textureCoord(0.0f, 0.0f);
			 
			viewerScreen->quad(0, 1, 2, 3);
		viewerScreen->end();
	}

	// Use infinite AABB to forse visibility
	Ogre::AxisAlignedBox aabInf;
	aabInf.setInfinite();
	viewerScreen->setBoundingBox(aabInf);
	 
	// Render just before overlays
	viewerScreen->setRenderQueueGroup(Ogre::RENDER_QUEUE_OVERLAY - 1);
	 
	// Attach to viewer scene manager
	mViewerSceneMgr->getRootSceneNode()->createChildSceneNode("ViewerScreenNode" + app)->attachObject(viewerScreen);
}



//-------------------------------------------------------------------------------------
void OgreBulletStereo::createViewports()
{
	//--- init the Viargo library
	viargo::initialize("../viargo_settings.xml");

	Ogre::String stereoMode = "Monoscopic";
    Ogre::ConfigOptionMap& options = Ogre::Root::getSingleton().getRenderSystem()->getConfigOptions();
    Ogre::ConfigOptionMap::iterator opt = options.find("Stereo Mode");
    
	_stereoMode = StereoRenderTargetListener::SM_MONOSCOPIC;
	if (opt != options.end()) {
        stereoMode = opt->second.currentValue;
    }

	// dimi: HACK!!!
	//stereoMode = "Quadbuffer";

    if (stereoMode == "QuadBuffer") {
		_stereoMode = StereoRenderTargetListener::SM_QUADBUFFERED;
    }
    else if (stereoMode == "Side-By-Side") {
		_stereoMode = StereoRenderTargetListener::SM_SIDEBYSIDE;
    }
    else {
		_stereoMode = StereoRenderTargetListener::SM_MONOSCOPIC;
    }

	//--- create the viewer
	createViewerScreen(true);	
	createViewerScreen(false);

	//---
    // Create viewports for the Viewer camera
	Ogre::Viewport* vpLeft = mWindow->addViewport(mViewerCamera, 0);
    vpLeft->setBackgroundColour(Ogre::ColourValue(0, 0, 0));
	vpLeft->setOverlaysEnabled(false);
	
	Ogre::Viewport* vpRight = mWindow->addViewport(mViewerCamera, 1);
    vpRight->setBackgroundColour(Ogre::ColourValue(0, 0, 0));
	vpRight->setOverlaysEnabled(false);

	mWindow->setDeactivateOnFocusChange(false);

	_stereoListener = new StereoRenderTargetListener(_stereoMode, vpLeft, vpRight);

	_stereoListener->setLeftRenderTexture(mRenderTextureLeft);
	_stereoListener->setRightRenderTexture(mRenderTextureRight);

	mWindow->addListener(_stereoListener);

    // Alter the rendering camera's aspect ratio to match the viewport
	double aspectRatio = Ogre::Real(vpRight->getActualWidth()) / Ogre::Real(vpRight->getActualHeight());
	//Viargo.camera("main").setAspectRatio(aspectRatio);

	// and the same for the viewer camera
    mViewerCamera->setAspectRatio(aspectRatio);
}

//-------------------------------------------------------------------------------------
bool OgreBulletStereo::keyPressed( const OIS::KeyEvent &arg ) {
	//if (arg.key == OIS::KC_F12) {
	//	_stereoListener->switchEyes();
	//} 
	//else if (arg.key == OIS::KC_S) {
	//	//_doBackgroundSubtraction = true;
	//} 
	//else if (arg.key == OIS::KC_A) { // ::startCalibration
	//	//_metaphor->startCalibration();
	//	//kinectCali = true;
	//} 
	//else if (arg.key == OIS::KC_D) {
	//	_doIt = !_doIt;
	//	_asyncKinectProcessor->toggleProcessing(_doIt);
	//}
	//else if (arg.key == OIS::KC_M) {
	//	_processMultitouchShadows = !_processMultitouchShadows;
	//	_multitouchShadows->toggleProcessing(_processMultitouchShadows);
	//}
	//else if (arg.key == OIS::KC_Q) {
	//	_asyncKinectProcessor->toggleShowDebugInformation();
	//	std::cout << "Debug toggled" << std::endl;
	//}
	//

	// Redirect call to super class
	return BaseApplication::keyPressed(arg);
}


 bool OgreBulletStereo::keyReleased( const OIS::KeyEvent &arg ) {
	 if (arg.key == OIS::KC_F12) {
		_stereoListener->switchEyes();
	} 
	else if (arg.key == OIS::KC_S) {
		//_doBackgroundSubtraction = true;
	} 
	else if (arg.key == OIS::KC_A) { // ::startCalibration
		//_metaphor->startCalibration();
		//kinectCali = true;
	} 
	

	// Redirect call to super class
	return BaseApplication::keyReleased(arg);
 }

//-------------------------------------------------------------------------------------
void OgreBulletStereo::createScene() {


	// Set ambient light
	mSceneMgr->setAmbientLight(Ogre::ColourValue(0.5f, 0.5f, 0.5f));
	mSceneMgr->setShadowTechnique(Ogre::SHADOWTYPE_STENCIL_ADDITIVE);  // *** Schatten - könnten die Performance reduzieren...

	// Create a light
	Ogre::Light* l1 = mSceneMgr->createLight("MainLight");
	l1->setDiffuseColour(1.0f, 1.0f, 1.0f);
	l1->setSpecularColour(0.75f, 0.75f, 0.75f);
	l1->setPosition(-20.0f, 0.0f, 50.0f);
	
	// add the viargo's default camera navigation (WASD)
	Viargo.addMetaphor(new DefaultCameraMetaphor("WASD", "main", 150.0f));


	// add the the physics metaphor
	//OgreBulletCollisionMetaphor* collisionMetaphor = new OgreBulletCollisionMetaphor(mSceneMgr,	mWorld, mTrayMgr);
	//Viargo.addMetaphor(collisionMetaphor);
	

	


	// ----- hier beginnt das Headtracking -----------

	// Metapher für das Headtracking
	Viargo.addMetaphor(new HeadTrackingMetaphor("HeadTracking"));

	// Create heacktracking device
	viargo::ViargoHeadtrackingDevice* headTracker = new viargo::ViargoHeadtrackingDevice("HeadTracker-Device", "", 2424); // Listen to port 3334

	// Add headtracking device to viargo
	Viargo.addDevice(headTracker);

	// Create filter metaphor
	viargo::ViargoHeadTrackingFilterMetaphor* metaphor = new viargo::ViargoHeadTrackingFilterMetaphor("HeadtrackingFilter-Metaphor", true);

	// Add filter metaphor to viargo
	Viargo.addMetaphor(metaphor);
	Viargo.addDevice(metaphor);
	
	// Add sample metaphor
	//viargo::ViargoHeadTrackingMetaphor* headtrackingMetaphor =
	//	new viargo::ViargoHeadTrackingMetaphor("HeadtrackingMetaphor", mSceneMgr);

	//Viargo.addMetaphor(headtrackingMetaphor);


	// Size of window in cm
	Ogre::Vector2 windowSizeCm = Ogre::Vector2(global::tableWidth, global::tableHeight);  // Bildschirm?

	// Create calibration metaphor
	viargo::ViargoOgreHeadTrackingCalibrationMetaphor* calibrationMetaphor = 
		new viargo::ViargoOgreHeadTrackingCalibrationMetaphor("HeadtrackingCalibration-Metaphor", windowSizeCm, 2, Ogre::Vector4(0.3f, 0.3f, 0.3f, 0.3f), true);

	// Add calibration metaphor to viargo
	Viargo.addMetaphor(calibrationMetaphor);
	Viargo.addDevice(calibrationMetaphor);

	

	// --- NOTE: ALEX: Removed for Kinect ----------
	//Viargo.addMetaphor(new TouchMetaphor("TouchSteuerung", mSceneMgr, shadowCasterCamera, mTrayMgr, collisionMetaphor));

	// Metapher für Maus-Steuerung 
	// ---------------------------------------------
}
