#ifndef __VIARGO_HEAD_TRACKING_METAPHOR_H__
#define __VIARGO_HEAD_TRACKING_METAPHOR_H__

#include <viargo.h>
#include <OgreSceneManager.h>

#include "viargo_ogre_head_tracking_calibration_metaphor.h"

namespace viargo {

/// Filters ehad tracking data
///
class ViargoHeadTrackingMetaphor : public Metaphor {

public:
	/// ctor
	///
	ViargoHeadTrackingMetaphor(const std::string& name, Ogre::SceneManager* sceneManager, bool start = true)
		:Metaphor(name, start) 
		,_sceneMgr(sceneManager)
	{
		// Create an Entity
		Ogre::Entity* ogreHead = _sceneMgr->createEntity("HeadSample", "ogrehead.mesh");
	 
		// Create a SceneNode and attach the Entity to it
		Ogre::SceneNode* headNode = _sceneMgr->getRootSceneNode()->createChildSceneNode("SampleHeadNode");
		headNode->attachObject(ogreHead);
		headNode->scale(Ogre::Vector3(0.1f));

		headNode->setVisible(true);
	}

	/// dtor
	///
	virtual ~ViargoHeadTrackingMetaphor() {}

	/// Called by all events
	///
	bool onEventReceived(viargo::Event* event) {
		return false;
	}

	/// Called by responsible events
	///
	void handleEvent(viargo::Event* event) {
		CalibratedSensorPositionEvent* ev = dynamic_cast<CalibratedSensorPositionEvent*>(event);

		if (ev == 0) {
			return;
		}

		_sceneMgr->getSceneNode("SampleHeadNode")->setPosition(ev->x(), ev->y(), ev->z());
	}

	/// Called by engine
	///
	void update(float timeSinceLastUpdate) {}



protected:
	Ogre::SceneManager* _sceneMgr;

}; // class ViargoHeadTrackingMetaphor

} // namespace viargo

#endif // __VIARGO_HEAD_TRACKING_METAPHOR_H__