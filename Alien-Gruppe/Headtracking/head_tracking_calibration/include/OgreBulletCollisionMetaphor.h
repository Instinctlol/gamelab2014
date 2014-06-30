
#ifndef OGRE_BULLET_COLLISION_METAPHOR
#define OGRE_BULLET_COLLISION_METAPHOR

// def
#include "BaseApplication.h"
#include "DynamicLines.h"

// bullet physics
#include "OgreBulletDynamicsRigidBody.h"
#include "Shapes/OgreBulletCollisionsStaticPlaneShape.h"
#include "Shapes/OgreBulletCollisionsBoxShape.h"
#include "Shapes/OgreBulletCollisionsSphereShape.h"


//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------


class OgreBulletCollisionMetaphor : public viargo::Metaphor {

private:
	Ogre::SceneManager*                mSceneMgr; 
	OgreBulletDynamics::DynamicsWorld *mWorld;	// OgreBullet World
	OgreBulletCollisions::DebugDrawer *debugDrawer;
	OgreBites::SdkTrayManager*		   mTrayMgr;
	int                                mNumEntitiesInstanced;
	OgreBulletDynamics::RigidBody     *currentBody;

	void OgreBulletCollisionMetaphor::bauePlane(Ogre::String name, Ogre::Vector3 pos, float width, float height, Ogre::Vector3 norm, Ogre::String texture, int segments, Ogre::Vector3 upVector, bool bb);
	void OgreBulletCollisionMetaphor::createBox(Ogre::Vector3 position, float scale, Ogre::String name, Ogre::String materialname, bool bb);
	void OgreBulletCollisionMetaphor::createBox(Ogre::Vector3 position, Ogre::Vector3 scale, Ogre::String name, Ogre::String materialname, bool bb, bool sShape, const Ogre::String& meshName = "cube.mesh");
	//void OgreBulletCollisionMetaphor::updateShadows();
	void OgreBulletCollisionMetaphor::resetRotation();
	void OgreBulletCollisionMetaphor::resetPosition();

	void createTriangleMesh(const Ogre::Vector3& position, const std::string& name, const std::string& materialName, const std::string& meshName);

	std::deque< OgreBulletDynamics::RigidBody* >         mBodies;
	std::deque< OgreBulletCollisions::CollisionShape* >  mShapes;

	OgreBulletDynamics::RigidBody *rb[4];
	Ogre::SceneNode *bn[4];

	//DynamicLines *line1, *line2, *line3, *line4;
	bool rotation;
	Ogre::Vector3 stage2Pos[4];
	OgreBulletDynamics::RigidBody *stage3BoxRb;


	
	bool _active;
public:
	// ctor
	// (defines by default a floor-plane)
	OgreBulletCollisionMetaphor(Ogre::SceneManager *sceneMgr, OgreBulletDynamics::DynamicsWorld *World, OgreBites::SdkTrayManager* trayMgr);

	// dtor
	~OgreBulletCollisionMetaphor();

	/// Resets orientation of the scene node as initial orientation in the experiment
	///
	void resetOrientation(Ogre::SceneNode* node);

	/// Resynchronizates the physics object with the scene node in the inverse direction, i.e., the physics object gets the transformation from the scene node
	///
	void resyncPhysics(OgreBulletDynamics::RigidBody* rigidBody);

	void add_shadow_to_material(const Ogre::String& name, Ogre::Frustum* projectiveFrustum);


	std::deque< OgreBulletDynamics::RigidBody* >* rigidBodies() { return &mBodies; }
	OgreBulletDynamics::DynamicsWorld* dynamicWorld() { return mWorld; }

	void toggleSimulation(bool active) {
		_active = active;
	}


	// viargo::Metaphor interface
	// handle event - throw a box on B pressed
	virtual void handleEvent(viargo::Event* event);

	// update Bullet Physics animation
	virtual void update(float timeSinceLastUpdate);
	

};

//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
#endif // OGRE_BULLET_COLLISION_METAPHOR



