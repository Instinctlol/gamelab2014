#include <OgreSceneManager.h>

#include "OgreBulletDynamicsRigidBody.h"

#include "Shapes/OgreBulletCollisionsStaticPlaneShape.h"
#include "Shapes/OgreBulletCollisionsCompoundShape.h"

#include "Utils/OgreBulletCollisionsMeshToShapeConverter.h"

#include "globals.h"

#include "experiment.h"
#include "ShadowVolumeManager.h"

Experiment::Experiment(Ogre::SceneManager* sceneManager) 
	:viargo::Metaphor("ExperimentMetaphor")
	,_sceneManager(sceneManager)
	,_physicsActive(true)
	,_debugWireframe(false)
	,_fishTankDimensions(global::tableWidth, global::tableHeight, 31.0f)
{
	// Initialize physics world
	_initializePhysicsWorld();

	// Buid fishtank
	_buildFishTank();

	// Initialize experiment
	_initializeExperimentConfigurations();
	
	/*btVector3 af (1,1,0);
	rb[0]->getBulletRigidBody()->setAngularFactor(af);
	rb[1]->getBulletRigidBody()->setAngularFactor(af);
	rb[2]->getBulletRigidBody()->setAngularFactor(af);
	rb[3]->getBulletRigidBody()->setAngularFactor(af);*/
}

Experiment::~Experiment() {
	// OgreBullet physic delete - RigidBodies
	std::deque<OgreBulletDynamics::RigidBody*>::iterator itBody = _rigidBodies.begin();
	while (_rigidBodies.end() != itBody) {   
		delete *itBody; 
		++itBody;
	}
	
	// OgreBullet physic delete - Shapes
	std::deque<OgreBulletCollisions::CollisionShape *>::iterator itShape = _collisionShapes.begin();
	while (_collisionShapes.end() != itShape) {   
		delete *itShape; 
		++itShape;
	}

	_rigidBodies.clear();
	_collisionShapes.clear();
	
	delete _dynamicsWorld->getDebugDrawer();
	_dynamicsWorld->setDebugDrawer(0);
	delete _dynamicsWorld;
}

void Experiment::assignShadowVolumeManager(ShadowVolumeManager* shadowVolumeManager) {
	_shadowVolumeManager = shadowVolumeManager;
}

bool Experiment::onEventReceived(viargo::Event* event) {
	return true;
}

void Experiment::handleEvent(viargo::Event* event) {
}

void Experiment::update(float timeSinceLastUpdate) {
	// Update physics
	if (_physicsActive) {
		_dynamicsWorld->stepSimulation(timeSinceLastUpdate / 100.0f, 4, 1.0f / 60.0f);
	}
}

void Experiment::resetOrientation(Ogre::SceneNode* node) {
	//static const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / -2.0f), Ogre::Vector3(0,0,1));
	//node->setOrientation(initialOrientation);
	node->setOrientation(0, 0, 0, 1);
}

void Experiment::resyncPhysics(OgreBulletDynamics::RigidBody* rigidBody) {
	btTransform transform;
	transform.setIdentity();

	// Aquire the scene node transformation
	const Ogre::Vector3    position    = rigidBody->getSceneNode()->getPosition();
	const Ogre::Quaternion orientation = rigidBody->getSceneNode()->getOrientation();

	// Assign transformation to physics object
	transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position));
	transform.setRotation(OgreBulletCollisions::OgreBtConverter::to(orientation));

	rigidBody->getBulletRigidBody()->setWorldTransform(transform);
}

void Experiment::togglePhysics(bool active) {
	_physicsActive = active;
}

std::vector<Ogre::SceneNode*> Experiment::experimentObjectSceneNodes() const {
	return _experimentObjectsSceneNodes;
}

std::vector<OgreBulletDynamics::RigidBody*> Experiment::experimentObjectRigidBodies() const {
	return _experimentObjectsRigidBodies;
}

std::string Experiment::guessMaterialName(Ogre::SceneNode* node) const {
	return static_cast<Ogre::Entity*>(node->getAttachedObject(0))->getMesh()->getSubMesh(0)->getMaterialName();;
}

void Experiment::_buildFishTank() {
	// Calculate parameters of tilted back
	const Ogre::Real coverRatio = 1.0f / 3.0f;

	const Ogre::Vector3 topMidPoint    = Ogre::Vector3(0.0f, _fishTankDimensions.y / 2.0f, 0.0f);
	const Ogre::Vector3 bottomMidPoint = Ogre::Vector3(0.0f, (_fishTankDimensions.y / 2.0f) - (coverRatio * _fishTankDimensions.y), -_fishTankDimensions.z);
	const Ogre::Vector3 position       = bottomMidPoint + 1.0f / 2.0f * (topMidPoint - bottomMidPoint);

	const Ogre::Vector3 up     = (topMidPoint - bottomMidPoint).normalisedCopy();
	const Ogre::Vector3 normal = Ogre::Vector3::UNIT_X.crossProduct(up).normalisedCopy();

	const Ogre::Real planeHeight = Ogre::Math::Sqrt( Ogre::Math::Sqr( coverRatio * _fishTankDimensions.y ) + Ogre::Math::Sqr(_fishTankDimensions.z));

	// Buid planes
	_createStaticPlane("static_walls_back",  position,                                                                             _fishTankDimensions.x,  planeHeight,          normal,                         "FishTankTiltedBack", 5, up);
	_createStaticPlane("static_walls_front", Ogre::Vector3(0.0f, -(_fishTankDimensions.y / 2.0f),  -_fishTankDimensions.z / 2.0f), _fishTankDimensions.x, _fishTankDimensions.z, Ogre::Vector3::UNIT_Y,          "FishTankSides",      5, Ogre::Vector3::NEGATIVE_UNIT_Z);
	_createStaticPlane("static_walls_left",  Ogre::Vector3(-(_fishTankDimensions.x / 2.0f), 0.0f,  -_fishTankDimensions.z / 2.0f), _fishTankDimensions.y, _fishTankDimensions.z, Ogre::Vector3::UNIT_X,          "FishTankSides",      5, Ogre::Vector3::NEGATIVE_UNIT_Z);
	_createStaticPlane("static_walls_right", Ogre::Vector3(_fishTankDimensions.x / 2.0f, 0.0f,     -_fishTankDimensions.z / 2.0f), _fishTankDimensions.y, _fishTankDimensions.z, Ogre::Vector3::NEGATIVE_UNIT_X, "FishTankSides",      5, Ogre::Vector3::NEGATIVE_UNIT_Z);
	_createStaticPlane("static_walls_floor", Ogre::Vector3(0.0f, 0.0f, -_fishTankDimensions.z),                                    _fishTankDimensions.x, _fishTankDimensions.y, Ogre::Vector3::UNIT_Z,          "FishTankFloor",      5, Ogre::Vector3::UNIT_Y);




	// Experiment boxes	
	/*_createCompoundObject(true, "dynamic_box1", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "Puzzle1");
	_createCompoundObject(true, "dynamic_box2", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "Puzzle2");
	_createCompoundObject(true, "dynamic_box3", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "Puzzle3");
	_createCompoundObject(true, "dynamic_box4", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "Puzzle4");
	
	_createCompoundObject(false, "static_table", Ogre::Vector3(0.0f,  -20.0f, -31.0f), "table.mesh", false);*/

	/*_createCompoundObject(false, "dynamic_tower_1", Ogre::Vector3(20.0f,  10.0f, -31.0f), "tower.mesh", false);
	_createCompoundObject(true, "dynamic_ring1",   Ogre::Vector3(20.0f,  10.0f,-15.0f),  "ring_big.mesh", true);
	_createCompoundObject(true, "dynamic_ring2",   Ogre::Vector3(20.0f,  10.0f,-15.0f),  "ring_mid.mesh", true);
	_createCompoundObject(true, "dynamic_ring3",   Ogre::Vector3(20.0f,  10.0f,-15.0f),   "ring_small.mesh", true);
	_createCompoundObject(false, "dynamic_tower_2", Ogre::Vector3(-20.0f,  10.0f, -31.0f), "tower.mesh", false);*/





	//_createCompoundObject(true, "puzzle_1_1", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_1_1");
	//_createCompoundObject(true, "puzzle_1_2", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_1_2");
	//_createCompoundObject(true, "puzzle_1_3", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_1_3");
	//_createCompoundObject(true, "puzzle_2_1", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_2_1");
	//_createCompoundObject(true, "puzzle_2_2", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_2_2");
	//_createCompoundObject(true, "puzzle_2_3", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_2_3");
	//_createCompoundObject(true, "puzzle_3_1", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_3_1");
	//_createCompoundObject(true, "puzzle_3_2", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_3_2");
	//_createCompoundObject(true, "puzzle_3_3", Ogre::Vector3(-10.0f,  10.0f, -30.0f), "cube_max.mesh", true, "puzzle_3_3");

}

void Experiment::_initializePhysicsWorld() {
	_dynamicsWorld = new OgreBulletDynamics::DynamicsWorld(_sceneManager,
		Ogre::AxisAlignedBox (Ogre::Vector3 ((float)-global::tableWidth, (float)-global::tableHeight, -10.0f), 
		Ogre::Vector3((float)global::tableWidth,  (float)global::tableHeight,  100)),  
		Ogre::Vector3(0.0f, 0.0f, -9.81f));

	// Create debug drawer for physics
	_debugDrawer = new OgreBulletCollisions::DebugDrawer();
	_debugDrawer->setDrawWireframe(true);

	// Assign debug drawer to physics world
	_dynamicsWorld->setDebugDrawer(_debugDrawer);
	_dynamicsWorld->setShowDebugShapes(_debugWireframe);

	// Add debug drawer to the scene
	Ogre::SceneNode *node = _sceneManager->getRootSceneNode()->createChildSceneNode("debugDrawer", Ogre::Vector3::ZERO);
	node->attachObject(static_cast<Ogre::SimpleRenderable*>(_debugDrawer));
}

void Experiment::_addObjectToExperiment(Ogre::SceneNode* node, OgreBulletDynamics::RigidBody* body) {
	_experimentObjectsSceneNodes.push_back(node);
	_experimentObjectsRigidBodies.push_back(body);
}

void Experiment::_createStaticPlane(const std::string& name, const Ogre::Vector3& position, float width, float height, const Ogre::Vector3& norm, const std::string& materialName, int segments, const Ogre::Vector3& upVector) {
	Ogre::Plane plane(norm, 0.0f);

	// Create mesh and entity
    Ogre::MeshManager::getSingleton().createPlane(name + "_mesh", Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME, plane, width, height, 1, 1, true, 1, (Ogre::Real)segments, (Ogre::Real)segments, upVector);
    Ogre::Entity* entity = _sceneManager->createEntity(name + "_entity", name + "_mesh");
	entity->setMaterialName(materialName);

	// Create and add scene node
	Ogre::SceneNode *node = _sceneManager->getRootSceneNode()->createChildSceneNode(name + "_node");
	node->attachObject(entity);
	node->setPosition(position);

	// Physicalize plane
	OgreBulletCollisions::CollisionShape* shape = new OgreBulletCollisions::StaticPlaneCollisionShape(norm, 0);
	OgreBulletDynamics::RigidBody* body = new OgreBulletDynamics::RigidBody(name + "_body" , _dynamicsWorld);
	 
	body->setStaticShape(node, shape, 0.9f, 0.8f, position);
	body->setDamping(0.0f, 0.0f);

	// Add shape and body to collections
	_collisionShapes.push_back(shape);
	_rigidBodies.push_back(body);
}

void Experiment::_createCompoundObject(bool interactive, const std::string& name, const Ogre::Vector3& position, const std::string& meshName, bool dynamic, const std::string& materialOverride) {
	// Create entity and add to scene
	Ogre::Entity *entity = _sceneManager->createEntity(name + "_entity", meshName);			    
	entity->setCastShadows(true);

	if (materialOverride != "") {
		entity->setMaterialName(materialOverride);
	}
	
	//const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / 2.0f), Ogre::Vector3(1,0,0));

	Ogre::SceneNode *node = _sceneManager->getRootSceneNode()->createChildSceneNode(name + "_node");  
	node->attachObject(entity);
	node->setPosition(position);
	//node->setOrientation(initialOrientation);
	
	// Create physical compound object
	OgreBulletCollisions::StaticMeshToShapeConverter *smtsc = new OgreBulletCollisions::StaticMeshToShapeConverter();
	smtsc->addEntity(entity);
	OgreBulletCollisions::CompoundCollisionShape* shape = smtsc->createConvexDecomposition();
	OgreBulletDynamics::RigidBody *body = new OgreBulletDynamics::RigidBody(name + "_body", _dynamicsWorld);
	
	// Create either static or dynamic object
	if (dynamic) {
		body->setShape(node, shape, 0.02f, 110.0f, 100.0f, position);
	}
	else {
		body->setStaticShape(shape, 0.02f, 110.0f, position);
	}

	// Store rigid body into scene node for easier access
	node->getUserObjectBindings().setUserAny("rigid_body", Ogre::Any(body));

	// Disable physical deactivation (why ?)
	body->getBulletRigidBody()->setActivationState(DISABLE_DEACTIVATION);
				
	// Add shape and body to collections
	_collisionShapes.push_back(shape);
	_rigidBodies.push_back(body);

	if (interactive) {
		_addObjectToExperiment(node, body);
	}

	_experimentPurgeNodes.push_back(node);
	_experimentPurgeBodies.push_back(body);
	_experimentPurgeShapes.push_back(shape);

	resetOrientation(node);
}

void Experiment::_createObject(const std::string& name, const Ogre::Vector3& position, const std::string& meshName, const std::string& materialOverride) {
	// Create entity and add to scene
	Ogre::Entity *entity = _sceneManager->createEntity(name + "_entity", meshName);			    
	entity->setCastShadows(true);

	if (materialOverride != "") {
		entity->setMaterialName(materialOverride);
	}
	
	//const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / 2.0f), Ogre::Vector3(1,0,0));

	Ogre::SceneNode *node = _sceneManager->getRootSceneNode()->createChildSceneNode(name + "_node");  
	node->attachObject(entity);
	node->setPosition(position);

	resetOrientation(node);

	_experimentPurgeNodes.push_back(node);
}

void Experiment::_initializeExperimentConfigurations() {
	// ------------------------------------------------------
	// GRID
	// ------------------------------------------------------

	const float floor_width    = 130.0f;
	const float floor_height   = 62.0f;

	const float floor_y_offset = 17.0f;

	const int grid_x_num = 10;
	const int grid_y_num =  5;
	const int grid_z_num =  5;
	const int grid_sum   = grid_x_num * grid_y_num * grid_z_num;

	std::vector<std::vector<std::vector<Ogre::Vector3> > > gridPositions; // Layout = gridPositions[x][y][z]

	for (size_t index_z = 0; index_z < grid_z_num; ++index_z) {
		for (size_t index_y = 0; index_y < grid_y_num; ++index_y) {
			for (size_t index_x = 0; index_x < grid_x_num; ++index_x) {
				// Compute position
				const float x = -floor_width  / 2.0f + (floor_width / grid_x_num) * index_x;
				const float y = -floor_height / 2.0f + floor_y_offset + (floor_height / grid_y_num) * index_y;
				const float z = -_fishTankDimensions.z + 5.0f + ((_fishTankDimensions.z + 5.0f) / grid_z_num) * index_z; // Add 5cm above screen surface
				const Ogre::Vector3 position = Ogre::Vector3(x, y, z);

				// Add into x dimension
				if (gridPositions.size() < grid_x_num) {
					// Create dimension for first iteration
					gridPositions.push_back(std::vector<std::vector<Ogre::Vector3> >());
				}
				
				// Add into y dimension
				std::vector<std::vector<Ogre::Vector3> >& xDimension = gridPositions[index_x];
				if (xDimension.size() < grid_y_num) {
					// Create dimension for first iteration
					xDimension.push_back(std::vector<Ogre::Vector3>());
				}

				// Add into z dimension
				std::vector<Ogre::Vector3>& yDimension = xDimension[index_y];
				yDimension.push_back(position);
			}
		}
	}

	// ------------------------------------------------------
	// TASK 1
	// ------------------------------------------------------
	
	// Amount of objects per trial
	const int TASK1_COUNT_OBJECTS_PER_TRIAL = 5;

	// Amount of trials in task 1
	const int TASK1_COUNT_TRIALS = 10;
	
	// Fixed seed for pseudo random number generator in this task
	const int TASK1_FIXED_SEED = 3245;

	// Array of positions for objects and goal
	int* task1PositionIndices = new int[2 * TASK1_COUNT_OBJECTS_PER_TRIAL];	// 0, ..., n/2 - 1 => Start, n/2, ..., n - 1 => End Positions

	// Resize containers
	_task1BaseConfiguration.startPositions.resize(TASK1_COUNT_TRIALS);
	_task1BaseConfiguration.endPositions.resize(TASK1_COUNT_TRIALS);

	// Build positions
	for (size_t trialIndex = 0; trialIndex < TASK1_COUNT_TRIALS; ++trialIndex) {
		// Build array of position indices for current trial
		_pickXFromN(task1PositionIndices, grid_sum, 2 * TASK1_COUNT_OBJECTS_PER_TRIAL, TASK1_FIXED_SEED + trialIndex); // Index influences rng for variations in trials
		
		for (size_t objectPairIndex = 0; objectPairIndex < TASK1_COUNT_OBJECTS_PER_TRIAL; ++objectPairIndex) {
			// Prepare final positions
			Ogre::Vector3 gridStartPosition;
			Ogre::Vector3 gridEndPosition;

			// Indices
			int indexStartX, indexStartY, indexStartZ;
			int indexEndX, indexEndY, indexEndZ;

			// Translate the random value into x,y,z indices
			_decodeDimension(indexStartX, indexStartY, indexStartZ, task1PositionIndices[objectPairIndex], grid_x_num, grid_y_num);
			_decodeDimension(indexEndX, indexEndY, indexEndZ, task1PositionIndices[TASK1_COUNT_OBJECTS_PER_TRIAL + objectPairIndex], grid_x_num, grid_y_num);

			// Get positions from grid and indices
			gridStartPosition = gridPositions[indexStartX][indexStartY][indexStartZ];
			gridEndPosition = gridPositions[indexEndX][indexEndY][indexEndZ];

			// Push into configuration
			_task1BaseConfiguration.startPositions[trialIndex].push_back(gridStartPosition);
			_task1BaseConfiguration.endPositions[trialIndex].push_back(gridEndPosition);
		}
	}

	_task1MaxTrials = TASK1_COUNT_TRIALS;

	// Free space, positions are ready now
	delete[] task1PositionIndices;

	// Create objects
	_createCompoundObject(true, "dynamic_sphere1", Ogre::Vector3(-10.0f,  10.0f, -15.0f), "green_sphere.mesh", true);
	_createObject("static_sphere1", Ogre::Vector3(  0.0f,   0.0f, 0.0f), "red_sphere.mesh");

	_purgeScene();
}

void Experiment::_purgeScene() {
	auto itBody = _experimentPurgeBodies.begin();
	while (_experimentPurgeBodies.end() != itBody) {   
		delete *itBody; 
		++itBody;
	}
	
	auto itShape = _experimentPurgeShapes.begin();
	while (_experimentPurgeShapes.end() != itShape) {   
		delete *itShape; 
		++itShape;
	}

	auto itNode = _experimentPurgeNodes.begin();
	while (_experimentPurgeNodes.end() != itNode) {   
		delete *itNode; 
		++itNode;
	}

	_experimentPurgeBodies.clear();
	_experimentPurgeShapes.clear();
	_experimentPurgeNodes.clear();
}

int Experiment::_encodeDimension(int x, int y, int z, int max_x, int max_y) {
	return z * (max_x * max_y) + y * (max_x) + x;
}

void Experiment::_decodeDimension(int& x, int& y, int& z, int value, int max_x, int max_y) {
	z = value / (max_x * max_y);
	y = (value % (max_x * max_y)) / max_x;
	x = (value % (max_x * max_y)) % max_x;
}

void Experiment::_pickXFromN(int* list, size_t n, size_t x, int seed) {
	srand(seed);

	for (size_t index = 0; index < x; ++index) {
		bool indexFound = false;
		
		// Search for index, that was'nt assigned yet
		while (!indexFound) {
			int guess = rand() % n;

			indexFound = true;

			for (size_t j = 0; j < index; j++) {
				if (guess == list[j]) {
					// Found this index at another position
					indexFound = false;
				}
			}

			if (indexFound) {
				// Index not assigned yet, so take it
				list[index] = guess;
			}
		}
	}
}
