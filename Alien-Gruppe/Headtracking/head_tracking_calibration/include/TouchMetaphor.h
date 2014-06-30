#ifndef TOUCH_METAPHOR_H
#define TOUCH_METAPHOR_H

// base app
#include <fstream>
#include "BaseApplication.h"
#include "globals.h"
#include "DynamicRenderable.h"
#include "DynamicLines.h"
#include "SdkTrays.h"
#include "OgreBulletCollisionMetaphor.h"

class TouchMetaphor : public viargo::Metaphor {

protected:
	Ogre::SceneManager*  _mSceneMgr;
	Ogre::Camera* _mCamera;
	OgreBites::SdkTrayManager* _mTrayMgr;
	OgreBulletCollisionMetaphor* _mCollisionMetaphor;
	std::deque< OgreBulletDynamics::RigidBody* >* _rigidBodies;
	Ogre::RaySceneQuery* _mRayScnQuery;

	int touchId1;
	int touchId2;
	bool move1;
	bool move2;
	int shadow;
	bool twoTouch;
	float distance, height;
	OgreBulletDynamics::RigidBody *rb[4];
	Ogre::SceneNode *schatten[4];
	Ogre::Vector2 dis1;


public:
	// ctor
	TouchMetaphor(std::string name, Ogre::SceneManager* sceneMgr, Ogre::Camera* camera, OgreBites::SdkTrayManager* trayMgr, OgreBulletCollisionMetaphor* collisionMetaphor)
			:Metaphor(name),
			_mSceneMgr(sceneMgr),
			_mCamera(camera),
			_mTrayMgr(trayMgr),
			_mCollisionMetaphor(collisionMetaphor)
		{
			_rigidBodies = collisionMetaphor->rigidBodies();

			move1 = move2 = false;
			shadow = -1;
			//curTouchID = -1;
			touchId1 = touchId2 = 0;
			dis1.x = 0.0f;
			dis1.y = 0.0f;
			height = 0.0f;
			twoTouch = false;

			distance = 0.0f;

		for (unsigned int i=0; i < _rigidBodies->size(); i++) {
			if(_rigidBodies->at(i)->getName() == "rbbox1") {
				rb[0] =  _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox2") {
				rb[1] = _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox3") {
				rb[2] = _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox4") {
				rb[3] = _rigidBodies->at(i);
			}
		}

		schatten[0] = _mSceneMgr->getSceneNode("Shadow1Node");
		schatten[1] = _mSceneMgr->getSceneNode("Shadow2Node");
		schatten[2] = _mSceneMgr->getSceneNode("Shadow3Node");
		schatten[3] = _mSceneMgr->getSceneNode("Shadow4Node");
	}

	// dtor
	~TouchMetaphor() {
		 
	}
	
	// we only need the multitouch events
	virtual bool onEventReceived(viargo::Event* event) {
		
		if (typeid(*event) == typeid(viargo::MultiTouchEvent)) {
			return true;
		}
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			return true;
		}

		return false;
	}
	
	// Gets called for handling of an event if onEventReceived(...) returned true
	// @param: event	the event to be handled
	virtual void handleEvent(viargo::Event* event) {
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			viargo::KeyEvent& key = *((viargo::KeyEvent*)event);

		}


		// handle touch events
		if (typeid(*event) == typeid(viargo::MultiTouchEvent)) {

			
			viargo::MultiTouchEvent& mt = *((viargo::MultiTouchEvent*)event);

			
			if (!mt.touches().empty()) { // Touch vorhanden
				//std::cout << "etwas ist passiert" << std::endl;
				mt.touches().resetIterator();
				if (mt.touches().size() > 1 ) { // zwei touches
					twoTouch = true;
					//std::cout << "size " << mt.touches().size() << std::endl;
					if (touchId1 != 0) { // touchid bereits vorhanden
						if (touchId2 != 0) { //touchid2 bereits vorhanden
							if (mt.touches().exists(touchId1) && mt.touches().exists(touchId2)) { // zwei finger bewegt
								//std::cout << "Zwei finger bewegt id1= " << touchId1 << " id2= " << touchId2 << std::endl;
								viargo::Touch curTouch1 = mt.touches()[touchId1];
								viargo::Touch curTouch2 = mt.touches()[touchId2];
								move2fingers(Ogre::Vector2(curTouch1.current.position.x * global::tableWidth/2, curTouch1.current.position.y*global::tableHeight/2), Ogre::Vector2(curTouch2.current.position.x * global::tableWidth/2, curTouch2.current.position.y*global::tableHeight/2));
							} else { // ex nicht beide touch ids
								touchId1 = 0;
								touchId2 = 0;
								move1 = move2 = false;
								resetLinearFactor();
							}
						} else { // touchid2 != 0 else
							// zweiter touch erkannt
							viargo::Touch &secTouch = mt.touches().next();
							if (secTouch.id == touchId1) {
								secTouch = mt.touches().next();
							} 
							touchId2 = secTouch.id;
							move1 = false;

						}
					} else { // touch id1 != 0 else
						// zwei neue touches erkannt
						viargo::Touch &tmpTouch = mt.touches().next();
						touchId1 = tmpTouch.id;
						tmpTouch = mt.touches().next();
						touchId2 = tmpTouch.id;
						move1 = false;
					}
				} else { // zwei touche else -> nur ein touch
					if (twoTouch) {
						resetLinearFactor();
						twoTouch = false;
					}
						if (touchId1 != 0) { // alles ok touch 1 muss ex
							if (mt.touches().exists(touchId1)) {
								// ein finger wurde bewegt
								//std::cout << "Ein finger bewegt. id1= " << touchId1 << " id2= " << touchId2 << std::endl;
								viargo::Touch curTouch = mt.touches()[touchId1];
								move1finger(Ogre::Vector2(curTouch.current.position.x * global::tableWidth/2, curTouch.current.position.y*global::tableHeight/2));
							} else { // touch1 ex. nicht (mehr)
								touchId1 = 0;
								move1 = false;
								resetLinearFactor();
							}
						} else { // 1. touch gefunden
							viargo::Touch &tmpTouch = mt.touches().next();
							touchId1 = tmpTouch.id;
							move2 = false;
						}
					
				}


				
			} else {
				touchId1 = 0;
				touchId2 = 0;
				move1 = move2 = false;
				twoTouch = false;
				resetLinearFactor();
			}
		
		}
		else {
			// panic! we should never be here!
		}
	}

	// Gets called from Viargo.update() to handle frame-specific actions
	// @param:  timeSinceLastUpdate - the time since the last call of the function
	//								  in milliseconds
	virtual void update(float timeSinceLastUpdate) {
		
		
	}

	virtual void resetLinearFactor() {
		//if (shadow != -1) {
			//rb[shadow]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
		//}
		rb[0]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
		rb[1]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
		rb[2]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
		rb[3]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
	}

	// Zwei finger bewegt
	virtual void move2fingers(Ogre::Vector2 pos1, Ogre::Vector2 pos2) {
		if (!move2) {
			if (checkOnShadow(pos1) == checkOnShadow(pos2)) {
				shadow = checkOnShadow(pos1);
			}
			
			if (shadow != -1) {
				dis1.x = schatten[shadow]->getPosition().x;
				dis1.y = schatten[shadow]->getPosition().y;
				dis1 = dis1 - (pos1 + pos2) / 2;
				distance = pos1.distance(pos2);
				height = rb[shadow]->getSceneNode()->getPosition().z +2;
				rb[shadow]->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
			}
			move2 = true;
		}
		if (shadow != -1) {
			Ogre::Vector2 mid = (pos1 + pos2) / 2;
			
			Ogre::Vector3 position (mid.x + dis1.x, mid.y + dis1.y, height + (pos1.distance(pos2) - distance) * 1);
			btTransform transform; //Declaration of the btTransform
			transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
			transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
			rb[shadow]->getBulletRigidBody()->setWorldTransform(transform); //Apply the btTransform to the body
			//schatten[shadow]->setScale(5.0f, 5.0f, 5.0f);
		}
		//std::cout << "move 2 fingers" << std::endl;
	}

	// Ein finger bewegt
	virtual void move1finger(Ogre::Vector2 pos) {
		if (!move1) {
			shadow = checkOnShadow(pos);
			if (shadow != -1) {
				dis1.x = schatten[shadow]->getPosition().x;
				dis1.y = schatten[shadow]->getPosition().y;
				dis1 = dis1 - pos;
				height = rb[shadow]->getSceneNode()->getPosition().z +2;
				rb[shadow]->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
			}
			move1 = true;
		}

		if (shadow != -1) {
			Ogre::Vector3 position (pos.x + dis1.x, pos.y + dis1.y, height);
			btTransform transform; //Declaration of the btTransform
			transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
			transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
			rb[shadow]->getBulletRigidBody()->setWorldTransform(transform); //Apply the btTransform to the body
			//Ogre::Vector3 s = schatten[shadow]->getScale();
			//std::cout << "scale: " << s.x << std::endl;
			//schatten[shadow]->setScale(0.2f, 0.2f, 0.2f);
		}

	}

	virtual int checkOnShadow(Ogre::Vector2 pos) {
		float x = 0.0f;
		float y = 0.0f;
		bool candi[4] = {false, false, false, false};
		for (int i = 0; i < 4; i++) {
			x = schatten[i]->getPosition().x;
			y = schatten[i]->getPosition().y;
			Ogre::Vector3 scale = schatten[i]->getScale();
			if (pos.x > (x - scale.x * 50) && pos.x < (x + scale.x * 50)) {
				if (pos.y > (y - scale.x*50) && pos.y < (y+scale.y*50)) {
					candi[i] = true;
				}
			}
		}

		int candidate = -1;
		float z = 1000.0f;
		for (int i = 0; i < 4; i++) {
			if (candi[i] == true) {
				float tempZ = rb[i]->getSceneNode()->getPosition().z;
				if (tempZ < z) {
					candidate = i;
					z = tempZ;
				}
			}
		}

		return candidate;
	}
};

#endif //VIARGO_DEFAULT_TOUCH_STEUERUNG_METAPHOR_H
