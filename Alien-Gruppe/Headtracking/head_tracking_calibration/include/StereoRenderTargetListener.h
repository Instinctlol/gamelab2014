#pragma once

#include <viargo.h>

#include <OgreRenderTargetListener.h>
#include <OgreCamera.h>
#include <OgreLight.h>
#include <OgreSceneManager.h>

class StereoRenderTargetListener : public Ogre::RenderTargetListener, public Ogre::SceneManager::Listener
{
public:
    enum StereoMode
    {
        SM_MONOSCOPIC,
        SM_QUADBUFFERED,
        SM_SIDEBYSIDE
        //SM_ANAGLYPH
    };

	StereoRenderTargetListener(StereoMode stereoMode = SM_MONOSCOPIC);
    StereoRenderTargetListener(StereoMode stereoMode, Ogre::Viewport* leftViewport, Ogre::Viewport* rightViewport);
    virtual ~StereoRenderTargetListener();
    
    virtual void preViewportUpdate(Ogre::RenderTargetViewportEvent const & evt);
    virtual void postViewportUpdate(Ogre::RenderTargetViewportEvent const & evt);

	void setLeftRenderTexture(Ogre::RenderTexture* leftTexture);
	void setRightRenderTexture(Ogre::RenderTexture* rightTexture);
    
	void switchEyes();

	/** See SceneManager::shadowTextureCasterPreViewProj
	@note This callback is only usefull if you have activated the quad buffer rendering option, and we render a shadow texture
	*/
	virtual void shadowTextureCasterPreViewProj(Ogre::Light* light, Ogre::Camera* camera, size_t iteration);

	/** See SceneManager::shadowTextureReceiverPreViewProj
	@note This callback is only usefull if you have activated the quad buffer rendering option, and we render a shadow texture
	*/
	virtual void shadowTexturesUpdated(size_t numberOfShadowTextures);

private:
    StereoMode mStereoMode;
    Ogre::Viewport* mLeftViewport;
    Ogre::Viewport* mRightViewport;

	Ogre::RenderTexture* mRenderTextureLeft;
	Ogre::RenderTexture* mRenderTextureRight;

	/// Flag to know when we are rendering textures shadows with quad buffer enabled.
    bool mIsQuadBufferShadowsRendering;
	bool mIsQuadBufferInitialized;

	bool _eyesSwitched;
};

