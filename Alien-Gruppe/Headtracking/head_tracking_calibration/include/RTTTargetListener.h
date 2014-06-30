#pragma once

#include <viargo.h>

#include <OgreRenderTargetListener.h>
#include <OgreRenderTarget.h>
#include <OgreCamera.h>

#include "StereoRenderTargetListener.h"

//-------------------------------------------------------------------------------------
// RTT listener
//-------------------------------------------------------------------------------------
class RTTTargetListener : public Ogre::RenderTargetListener {
public:
	// ctor
	RTTTargetListener(const std::string& viargoCameraName, bool isLeft, StereoRenderTargetListener::StereoMode mode);

	// callbacks
	virtual void preRenderTargetUpdate(const Ogre::RenderTargetEvent& evt);
	virtual void postRenderTargetUpdate(const Ogre::RenderTargetEvent& evt);

private:
	std::string mViargoCameraName;
	bool mLeft;
	StereoRenderTargetListener::StereoMode _mode;
};