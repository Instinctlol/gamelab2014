#ifndef MOUSE_METAPHOR_H
#define MOUSE_METAPHOR_H

// base app
#include "BaseApplication.h"
#include "experiment.h"
#include "globals.h"
#include "DynamicRenderable.h"
#include "DynamicLines.h"
#include "SdkTrays.h"
#include "multitouch_shadows.h"

class MouseMetaphor : public viargo::Metaphor {
protected:
	Ogre::SceneManager*  _sceneMgr;
	Ogre::Camera*		 _camera;

	/// Start position of object before dragging
	///
	Ogre::Vector3 _dragObjectStartPosition;

	/// Start position of touches before action switch
	///
	Ogre::Vector3 _dragTouchStartPosition;

	Ogre::SceneNode*               _grabbedObjectNode;
	OgreBulletDynamics::RigidBody* _grabbedRigidBody;

	Experiment* _experiment;

public:

	/// ctor
	///
	MouseMetaphor(const std::string& name, Ogre::SceneManager* sceneMgr, Ogre::Camera* camera, Experiment* experiment)
		:Metaphor(name)
		,_sceneMgr(sceneMgr)
		,_camera(camera)
		,_experiment(experiment)
		,_grabbedObjectNode(nullptr)
		,_grabbedRigidBody(nullptr)
	{
	}


	~MouseMetaphor() {
	}
	
	// we only need the mouse events
	virtual bool onEventReceived(viargo::Event* event) {
		
		if (typeid(*event) == typeid(viargo::MouseEvent)) {
			return true;
		}

		return false;
	}

	static bool zCompare(const Ogre::SceneNode* lop, const Ogre::SceneNode* rop) {
		return (lop->getPosition().z < rop->getPosition().z);
	}

	Ogre::SceneNode* rayCast(float x, float y) {
		Ogre::Ray ray = Ogre::Ray(Ogre::Vector3(x, y, 100.0f), Ogre::Vector3(0.0f, 0.0f, -1.0f));

		// Create ray scene query from ray
		Ogre::RaySceneQuery* query = _sceneMgr->createRayQuery(ray);
		query->setSortByDistance(true);
	 
		// Execute query
		Ogre::RaySceneQueryResult& result = query->execute();
		Ogre::RaySceneQueryResult::iterator iter = result.begin();

		// Collection of hit nodes
		std::vector<Ogre::SceneNode*> hitNodes;

		const std::string filter = "shadow_volume_object_top";

		// Iterate over results
		for ( ; iter != result.end(); ++iter) {
			if (iter->movable) {
				Ogre::MovableObject* movable  = iter->movable;
				Ogre::SceneNode*     node     = static_cast<Ogre::SceneNode*>(movable->getParentNode());
				const Ogre::String   nodeName = node->getName();
				
				if (movable->getName().substr(0, filter.length()) == filter) {
					hitNodes.push_back(node->getParentSceneNode());
				}
			}
		}

		std::sort(hitNodes.begin(), hitNodes.end(), zCompare);

		if (hitNodes.size() > 0) {
			//return hitNodes.back(); // Camera bottom
			return hitNodes.front(); // Camera top
		}

		return nullptr;
	}

	// Gets called for handling of an event if onEventReceived(...) returned true
	// @param: event	the event to be handled
	virtual void handleEvent(viargo::Event* event) {

		// handle mouse events
		if (typeid(*event) == typeid(viargo::MouseEvent)) {
			
			viargo::MouseEvent& mouse = *((viargo::MouseEvent*)event);

			// Screen, in [0,1]
			const float mouseXScreen = mouse.x() / 1024.0f;
			const float mouseYScreen = mouse.y() / 768.0f;
			const float mouseZScreen = mouse.z();

			// World position
			const float mouseXWorld = (mouseXScreen * 2.0f - 1.0f) * global::tableWidth  / 2.0f;
			const float mouseYWorld = (mouseYScreen * 2.0f - 1.0f) * global::tableHeight / 2.0f * -1.0f;
			const float mouseZWorld = mouseZScreen * 0.01f;

			if (mouse.getEventType() == viargo::MouseEvent::MOUSEMOVEEVENT) {
				if (_grabbedRigidBody != 0) {
					const Ogre::Vector3 touchPosition = Ogre::Vector3(mouseXWorld, mouseYWorld, mouseZWorld);

					Ogre::Vector3 touchOffset = touchPosition - _dragTouchStartPosition;
					
					const Ogre::Vector3 newPosition   = _dragObjectStartPosition + touchOffset;
					_grabbedObjectNode->setPosition(newPosition);
					_experiment->resetOrientation(_grabbedObjectNode);
					_experiment->resyncPhysics(_grabbedRigidBody);
				}
			} 
			else if (mouse.getEventType() == viargo::MouseEvent::MOUSEPRESSEVENT) {
				_grabbedObjectNode = rayCast(mouseXWorld, mouseYWorld);
				
				if (_grabbedObjectNode) {
					// Store rigid body
					_grabbedRigidBody = Ogre::any_cast<OgreBulletDynamics::RigidBody*>(_grabbedObjectNode->getUserObjectBindings().getUserAny("rigid_body"));
				
					// Reset orientation of the object
					_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));

					// Store object start position
					_dragObjectStartPosition = _grabbedObjectNode->getPosition();
									
					// Store touch offset
					_dragTouchStartPosition = Ogre::Vector3(mouseXWorld, mouseYWorld, mouseZWorld);

					// Deactivate physics
					_experiment->togglePhysics(false);
					_experiment->resetOrientation(_grabbedObjectNode);
					_experiment->resyncPhysics(_grabbedRigidBody);
				}
			} 
			else if (mouse.getEventType() == viargo::MouseEvent::MOUSERELEASEEVENT) {
				if (_grabbedRigidBody != 0) {
					_experiment->togglePhysics(true);
					_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(1, 1, 1));
					_grabbedRigidBody = 0;
				}
			} 
			else {
				return;
			}
			
		} else {
			// panic! we should never be here!
		}
	}

	// Gets called from Viargo.update() to handle frame-specific actions
	// @param:  timeSinceLastUpdate - the time since the last call of the function
	//								  in milliseconds
	virtual void update(float timeSinceLastUpdate) {
//

	}
	
};

#endif //VIARGO_DEFAULT_MOUSE_STEUERUNG_METAPHOR_H
