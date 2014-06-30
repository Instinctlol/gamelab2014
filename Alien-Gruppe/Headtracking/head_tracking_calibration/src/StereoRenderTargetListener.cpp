/*
 */

#include "StereoRenderTargetListener.h"
#include "globals.h"

#include <OgreCamera.h>
#include <OgreViewport.h>
#include <OgreRenderSystem.h>
#include <OgreRoot.h>


StereoRenderTargetListener::StereoRenderTargetListener(StereoMode stereoMode)
    : mStereoMode(stereoMode), mLeftViewport(0), mRightViewport(0), _eyesSwitched(false)
	,mIsQuadBufferInitialized(false)
	,mIsQuadBufferShadowsRendering(false)
{
}

StereoRenderTargetListener::StereoRenderTargetListener(StereoMode stereoMode, Ogre::Viewport* leftViewport, Ogre::Viewport* rightViewport)
    : mStereoMode(stereoMode), mLeftViewport(leftViewport), mRightViewport(rightViewport)
	,mIsQuadBufferInitialized(false)
	,mIsQuadBufferShadowsRendering(false)
{
	// Ogre 1.9 fix
	/*mLeftViewport->setClearEveryFrame(false);
	mRightViewport->setClearEveryFrame(false);*/
}

StereoRenderTargetListener::~StereoRenderTargetListener()
{
}

void StereoRenderTargetListener::setLeftRenderTexture(Ogre::RenderTexture* leftTexture) {
	mRenderTextureLeft = leftTexture;
}

void StereoRenderTargetListener::setRightRenderTexture(Ogre::RenderTexture* rightTexture) {
	mRenderTextureRight = rightTexture;
}

void StereoRenderTargetListener::preViewportUpdate(Ogre::RenderTargetViewportEvent const & evt)
{
	//if (mIsQuadBufferInitialized) {
		//if (!mIsQuadBufferShadowsRendering) {
			Ogre::Camera* camera         = evt.source->getCamera();
			Ogre::SceneManager* sceneMgr = camera->getSceneManager();
			viargo::Camera& viargoCamera = Viargo.camera("main");
			Ogre::SceneNode* sn;
	

			// Quadbuffered left
			if (mStereoMode == SM_QUADBUFFERED && evt.source == mLeftViewport) { 
				Ogre::Root::getSingleton().getRenderSystem()->setQuadbufferedFramebuffer(Ogre::QBF_LEFT);
		
				bool leftVisible = !_eyesSwitched;
				sn = sceneMgr->getSceneNode("ViewerScreenNodeLeft");
				if (sn) sn->setVisible(leftVisible);
				sn = sceneMgr->getSceneNode("ViewerScreenNodeRight");
				if (sn) sn->setVisible(!leftVisible);

				// Ogre 1.9 fix
			/*	mLeftViewport->setDimensions(0, 0, 1, 1);
				mRightViewport->setDimensions(0, 0, 0, 0);*/

			}
			// Quadbuffered right
			else if (mStereoMode == SM_QUADBUFFERED && evt.source == mRightViewport) {
			//		if (mRenderTextureRight) mRenderTextureRight->update();
			//		if (mRenderTextureLeft) mRenderTextureLeft->update();
		
				Ogre::Root::getSingleton().getRenderSystem()->setQuadbufferedFramebuffer(Ogre::QBF_RIGHT);
		
				bool rightVisible = !_eyesSwitched;
				sn = sceneMgr->getSceneNode("ViewerScreenNodeLeft");
				if (sn) sn->setVisible(!rightVisible);
				sn = sceneMgr->getSceneNode("ViewerScreenNodeRight");
				if (sn) sn->setVisible(rightVisible);

				// Ogre 1.9 fix
			/*	mLeftViewport->setDimensions(0, 0, 0, 0);
				mRightViewport->setDimensions(0, 0, 1, 1);*/
			}

			// Side by side
			else if (mStereoMode == SM_SIDEBYSIDE && evt.source == mRightViewport) {
				sn = sceneMgr->getSceneNode("ViewerScreenNodeLeft");
				if (sn) sn->setVisible(true);
				sn = sceneMgr->getSceneNode("ViewerScreenNodeRight");
				if (sn) sn->setVisible(true);
			}
			// Monoscopic
			else {
				// TODO: dummy
				sn = sceneMgr->getSceneNode("ViewerScreenNodeLeft");
				if (sn) sn->setVisible(true);
				sn = sceneMgr->getSceneNode("ViewerScreenNodeRight");
				if (sn) sn->setVisible(false);
			}
	//	}
	//}
}

void StereoRenderTargetListener::postViewportUpdate(Ogre::RenderTargetViewportEvent const & evt)
{
    /*if (mIsQuadBufferInitialized) {
		if (!mIsQuadBufferShadowsRendering) {
		}
	}
	else {
		mIsQuadBufferInitialized = true;
	}*/
}

void StereoRenderTargetListener::switchEyes() {
	_eyesSwitched = !_eyesSwitched;
}

void StereoRenderTargetListener::shadowTextureCasterPreViewProj(Ogre::Light* light, Ogre::Camera* camera, size_t iteration)
{
    mIsQuadBufferShadowsRendering = true;
}

void StereoRenderTargetListener::shadowTexturesUpdated(size_t numberOfShadowTextures)
{
    mIsQuadBufferShadowsRendering = false;
}
