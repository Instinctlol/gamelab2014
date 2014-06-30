#ifndef __EXPERIMENT_H__
#define __EXPERIMENT_H__

#include <queue>

#include <OgreRoot.h>

#include <viargo.h>

// -----------------------------------------------------------
// Forward declarations
// -----------------------------------------------------------
namespace Ogre {
	class SceneManager;
}

namespace OgreBulletDynamics {
	class RigidBody;
	class DynamicsWorld;
}

namespace OgreBulletCollisions {
	class CollisionShape;
	class DebugDrawer;
}

class ShadowVolumeManager;

// -----------------------------------------------------------

class Experiment : public viargo::Metaphor {
public:
	/// ctor
	///
	Experiment(Ogre::SceneManager* sceneManager);

	/// dtor
	///
	~Experiment();

	/// Assigns instance of the shadow volume manager
	///
	void assignShadowVolumeManager(ShadowVolumeManager* shadowVolumeManager);

	/// Viargo interfaces
	///
	virtual bool onEventReceived(viargo::Event* event);
	virtual void handleEvent(viargo::Event* event);
	virtual void update(float timeSinceLastUpdate);

	/// Resets orientation of the scene node as initial orientation in the experiment
	///
	void resetOrientation(Ogre::SceneNode* node);

	/// Resynchronizates the physics object with the scene node in the inverse direction, i.e., the physics object gets the transformation from the scene node
	///
	void resyncPhysics(OgreBulletDynamics::RigidBody* rigidBody);

	/// activates or deactivates global physics
	///
	void togglePhysics(bool active);

	/// Returns a list of all scene nodes of experiment objects
	///
	std::vector<Ogre::SceneNode*> experimentObjectSceneNodes() const;

	/// Returns a list of all scene nodes of experiment objects
	///
	std::vector<OgreBulletDynamics::RigidBody*> experimentObjectRigidBodies() const;

	/// Tries to resovle the material name of the mesh / entity
	///
	std::string guessMaterialName(Ogre::SceneNode* node) const;

private:
	/// Create a static physicalized plane
	///
	void _createStaticPlane(const std::string& name, const Ogre::Vector3& position, float width, float height, const Ogre::Vector3& norm, const std::string& materialName, int segments, const Ogre::Vector3& upVector);

	/// Create a physicalized object (static or dynamic)
	///
	void _createCompoundObject(bool interactive, const std::string& name, const Ogre::Vector3& position, const std::string& meshName, bool dynamic = false, const std::string& materialOverride = "");

	/// Create a non-physicalized object
	///
	void _createObject(const std::string& name, const Ogre::Vector3& position, const std::string& meshName, const std::string& materialOverride = "");

	/// Register object for itneraction
	///
	void _addObjectToExperiment(Ogre::SceneNode* node, OgreBulletDynamics::RigidBody* body);

	/// Initialize the physics
	///
	void _initializePhysicsWorld();

	// Build fishtank
	///
	void _buildFishTank();

	void _initializeExperimentConfigurations();

	/// Picks random x numbers from [0, ..., n - 1] without repetition
	///
	void _pickXFromN(int* list, size_t n, size_t x, int seed);

	int _encodeDimension(int x, int y, int z, int max_x, int max_y);
	void _decodeDimension(int& x, int& y, int& z, int value, int max_x, int max_y);

	// Destroy the experimental scene obejcts
	///
	void _purgeScene();

	// --------------------------------------------------------------------------------------------------------------
	// --------------------------------------------------------------------------------------------------------------

	int _userID;
	int _groupID; // 0 = MT,Kinect; 1 = Kinect,MT

	int _currentTrial;
	int _currentTask;
	int _currentMode; // 0 = MT, 1 = Kinect

	// Amount of trials for current task
	int _currentTaskMaxTrials;

	/// Configuration of Task 1
	///
	struct Task1Configuration {
		std::vector<std::vector<Ogre::Vector3> > startPositions;
		std::vector<std::vector<Ogre::Vector3> > endPositions;
	}; // Task1

	int _task1MaxTrials;

	Task1Configuration _task1BaseConfiguration;

	/// List of experiment object scene nodes which are purged after each task
	///
	std::vector<Ogre::SceneNode*> _experimentPurgeNodes;

	/// List of experiment object bodies which are purged after each task
	///
	std::vector<OgreBulletDynamics::RigidBody*> _experimentPurgeBodies;

	/// List of experiment object shapes which are purged after each task
	///
	std::vector<OgreBulletCollisions::CollisionShape*> _experimentPurgeShapes;

	// --------------------------------------------------------------------------------------------------------------
	// --------------------------------------------------------------------------------------------------------------

	/// Pointer to the main scene manager
	///
	Ogre::SceneManager* _sceneManager; 

	/// Pointer to the shadow volume manager
	///
	ShadowVolumeManager* _shadowVolumeManager;

	/// Pointer to the physics world
	///
	OgreBulletDynamics::DynamicsWorld* _dynamicsWorld;

	/// Physics debug drawer
	///
	OgreBulletCollisions::DebugDrawer* _debugDrawer;
	
	/// Collection of rigid bodies
	///
	std::deque<OgreBulletDynamics::RigidBody*> _rigidBodies;

	/// Collection of collision shapes
	///
	std::deque<OgreBulletCollisions::CollisionShape*> _collisionShapes;

	/// List of experiment object scene nodes
	///
	std::vector<Ogre::SceneNode*> _experimentObjectsSceneNodes;

	/// List of experiment object rigid bodies
	///
	std::vector<OgreBulletDynamics::RigidBody*> _experimentObjectsRigidBodies;

	/// If true, overall physics are updated
	///
	bool _physicsActive;

	/// If true, physic bounding wireframes are visualized
	///
	bool _debugWireframe;

	/// Dimensions of the fishtank
	///
	Ogre::Vector3 _fishTankDimensions;
	
}; // Experiment

#endif //__EXPERIMENT_H__
