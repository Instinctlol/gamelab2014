#include "RTTTargetListener.h"

#include <OgreCamera.h>
#include <OgreViewport.h>
#include <OgreRenderSystem.h>
#include <OgreRoot.h>

#include "HeadtrackingMetaphor.h"
#include "StereoRenderTargetListener.h"
#include "globals.h"

//-------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------
RTTTargetListener::RTTTargetListener(const std::string& viargoCameraName, bool isLeft, StereoRenderTargetListener::StereoMode mode)
: mViargoCameraName(viargoCameraName)
, mLeft(isLeft)
,_mode(mode)
{
}

void getAsymmetricFrustum2(viargo::vec3f cameraPosition, float& nearLeft, float& nearRight, float& nearTop, float& nearBottom, float& focal, float nearDist) {
	// TODO: Do not forget to adjust camera.LookAt(..) such that camera looks perpendicular to image plane on (cam.x, cam.y, 0.0)

	// Constants
	//static float width = 47.5f; // Screen surface width in cm
	//static float height = 29.5f; // Screen surface height in cm

	static float width = 136.0f; // displayWidth; // Screen surface width in cm
	static float height = 102.0f; // displayHeight; // Screen surface height in cm

	// Focal length = orthogonal distance to image plane
	focal = cameraPosition.z;

	// Ratio for intercept theorem
	float ratio = focal / nearDist;

	// Compute size for focal
	float imageLeft   = (-width/2.0)  - cameraPosition.x;
	float imageRight  = (width/2.0)   - cameraPosition.x;
	float imageTop    = (height/2.0)  - cameraPosition.y;
	float imageBottom = (-height/2.0) - cameraPosition.y;

	// Intercept theorem
	nearLeft   = imageLeft   / ratio;
	nearRight  = imageRight  / ratio;
	nearTop    = imageTop    / ratio;
	nearBottom = imageBottom / ratio;
}



//-------------------------------------------------------------------------------------
void RTTTargetListener::preRenderTargetUpdate(const Ogre::RenderTargetEvent& evt) {

	viargo::vec3f pos;
	viargo::quat  ori;
	float left, right, bottom, top;
	float zfar=10000.0f;
	float znear=0.1f;

	HeadTrackingMetaphor * Metaphor = static_cast<HeadTrackingMetaphor*>(& Viargo.metaphor("HeadTracking"));


	if (mLeft) {
		if (_mode == StereoRenderTargetListener::SM_QUADBUFFERED)
		{
			Ogre::Root::getSingleton().getRenderSystem()->setQuadbufferedFramebuffer(Ogre::QBF_LEFT);
		}
		// params for the left eye
		pos = Metaphor->leftEyePos();
		//ori = Metaphor->orientation();
		//pos = Viargo.camera(mViargoCameraName).leftEye.position();
		//ori = Viargo.camera(mViargoCameraName).leftEye.attitude();
		Viargo.camera().leftEye.frustum(left, right, bottom, top, znear, zfar);



		// *** ERKLÄRUNG VON DIMITAR ZU 3 MARKERN ***
		// (ich glaub ich weiß jetzt, wie es geht)
		// Es gibt 2 RTTTargetListener, je einen pro Auge
		// diese werden in OgreBulletStereo erzeugt
		// die Headtracking-Metapher muss beim Erstellen einen Pointer auf beide Listener bekommen (TargetListenerLeft, TargetListenerRight)
		// (muss entsprechende Member-Variablen + Parameter im Konstruktor bekommen)
		// der Listener bekommt eine Methode setCameraPos() und eine entsprechende Membervariable, die die aktuelle Kameraposition speichert
		// Die Update-Methode der Headtracking-Metapher ruft TargetListenerLeft.setCameraPos() auf, und übergibt die neue Position des linken Auges (z.B. linker Marker + konstanter Offset)
		// *HIER* wird für jedes Auge separat pos, ori und das Frustum der Viargo-Kamera gesetzt.
		// (pos ist die gespeicherte Kamera-Position, ori bleibt so, wie es hier steht, und das Frustum wird hier separat für beide Augen berechnet)

	}
	else {
		if (_mode == StereoRenderTargetListener::SM_QUADBUFFERED)
		{
			Ogre::Root::getSingleton().getRenderSystem()->setQuadbufferedFramebuffer(Ogre::QBF_LEFT);
		}
		// params for the right eye
		pos = Metaphor->rightEyePos();
		//ori = Metaphor->orientation();
		//pos = Viargo.camera(mViargoCameraName).rightEye.position();
		//ori = Viargo.camera(mViargoCameraName).rightEye.attitude();
		Viargo.camera().rightEye.frustum(left, right, bottom, top, znear, zfar);
	}

	float focal;
	getAsymmetricFrustum2(pos, left, right, top, bottom, focal, znear);
	// get current camera
	if (evt.source->getNumViewports() <= 0) {
		return;
	}


	Ogre::Viewport* viewport = evt.source->getViewport(0);
	if (viewport) {

		Ogre::Camera* camera = viewport->getCamera();

		// synch the camera
		camera->setPosition(pos.x, pos.y, pos.z);
		
		//camera->setOrientation(Ogre::Quaternion(ori.w, ori.x, ori.y, ori.z));
		camera->setDirection(0,0,-1);
		//camera->setFocalLength(Viargo.camera().focalLength());

		camera->setNearClipDistance(znear);
		camera->setFarClipDistance(zfar);
		camera->setFrustumExtents(left, right, top, bottom);
	}
}

//-------------------------------------------------------------------------------------
void RTTTargetListener::postRenderTargetUpdate(const Ogre::RenderTargetEvent& evt) {
	Ogre::Viewport* viewport = evt.source->getViewport(0);
	if (viewport) {
		
	}
	// nop
}
