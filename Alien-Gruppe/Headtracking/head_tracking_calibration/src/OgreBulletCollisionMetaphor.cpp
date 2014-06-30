#include "OgreBulletCollisionMetaphor.h"
#include "globals.h"
#include "SdkTrays.h"

//#include "BulletCollision\CollisionShapes\btBvhTriangleMeshShape.h"
#include "Shapes/OgreBulletCollisionsCompoundShape.h"
#include "Shapes/OgreBulletCollisionsGImpactShape.h"
#include "Shapes/OgreBulletCollisionsTrimeshShape.h"
#include "Utils/OgreBulletCollisionsMeshToShapeConverter.h"

//// Callback-Handler, wenn eine neue Collision auftritt
//typedef bool (*ContactAddedCallback)(
//    btManifoldPoint& cp,
//    const btCollisionObject* colObj0,
//    int partId0,
//    int index0,
//    const btCollisionObject* colObj1,
//    int partId1,
//    int index1);
//
//// Callback-Handler, wenn eine Collision verarbeitet wurde
//typedef bool (*ContactProcessedCallback)(
//    btManifoldPoint& cp,
//    void* body0,void* body1);

//-------------------------------------------------------------------------------------------------

bool MyContactProcessedCallback(btManifoldPoint& cp, void* body0,void* body1) {
	/*btRigidBody* ob0 = (btRigidBody*) body0;
	btRigidBody* ob1 = (btRigidBody*) body1;*/
	return true;
}

void OgreBulletCollisionMetaphor::add_shadow_to_material(const Ogre::String& name, Ogre::Frustum* projectiveFrustum) {
	// get material
	Ogre::MaterialPtr material = (Ogre::MaterialPtr)Ogre::MaterialManager::getSingleton().getByName(name);

	Ogre::Material::TechniqueIterator it = material->getTechniqueIterator();
	Ogre::Pass* pass = 0;
            
	while (it.hasMoreElements())
	{
		Ogre::Technique* tech = it.getNext();
		if (tech->getSchemeName() != "depthSurfaceRendering") {
			pass = tech->createPass();
			break;
		}
	}
	
	if (pass == 0) { // Error
		std::cout << "Error in material " << name << " during creation of additional shadow pass !" << std::endl;
		return;
	}

	//pass = material->getTechnique(0)->createPass();
	pass->setName("ShadowPass");

	// some settings
	pass->setLightingEnabled(false);
	//pass->setDepthWriteEnabled(false);
	pass->setDepthBias(1);
	pass->setSceneBlending(Ogre::SBT_TRANSPARENT_ALPHA);

	

	// add kinect-shadow
	Ogre::TextureUnitState* texState = pass->createTextureUnitState("KinectShadow");	// Kinect-Shadow texture
	texState->setProjectiveTexturing(true, projectiveFrustum);
	//texState->setTextureAddressingMode(Ogre::TextureUnitState::TAM_CLAMP);
	texState->setTextureAddressingMode(Ogre::TextureUnitState::TAM_BORDER);
	texState->setTextureBorderColour(Ogre::ColourValue(0.0f, 0.0f, 0.0f, 0.0f));
	texState->setTextureFiltering(Ogre::FO_POINT, Ogre::FO_LINEAR, Ogre::FO_NONE);
}

OgreBulletCollisionMetaphor::OgreBulletCollisionMetaphor(Ogre::SceneManager *sceneMgr, OgreBulletDynamics::DynamicsWorld *World, OgreBites::SdkTrayManager* trayMgr)
	: viargo::Metaphor("OgreBulletCollisionMetaphor"), 
      mSceneMgr(sceneMgr),
	  mTrayMgr(trayMgr),
	  mNumEntitiesInstanced(0),
	  _active(true)
{
	//rotation = true;

	gContactProcessedCallback = MyContactProcessedCallback;

	// Start Bullet

	mWorld = World;
    // add Debug info display tool
	debugDrawer = new OgreBulletCollisions::DebugDrawer();
	debugDrawer->setDrawWireframe(true);	// we want to see the Bullet containers
	mWorld->setDebugDrawer(debugDrawer);
	mWorld->setShowDebugShapes(true);		// enable it if you want to see the Bullet containers
	Ogre::SceneNode *node = mSceneMgr->getRootSceneNode()->createChildSceneNode("debugDrawer", Ogre::Vector3::ZERO);
	node->attachObject(static_cast <Ogre::SimpleRenderable *> (debugDrawer));

	//bauePlane("static_walls_floor", Ogre::Vector3(0.0f, 0.0f, -50.0f), global::tableWidth, global::tableHeight, Ogre::Vector3::UNIT_Z, "Examples/Boden", 5, Ogre::Vector3::UNIT_Y, true);


	const float fishTankDepth = 31.0f;

	bauePlane("static_walls_floor", Ogre::Vector3(0.0f, 0.0f, -fishTankDepth), global::tableWidth, global::tableHeight, Ogre::Vector3::UNIT_Z, "Examples/Boden", 5, Ogre::Vector3::UNIT_Y, true);



	//bauePlane("static_walls_back",  Ogre::Vector3(0.0f, global::tableHeight / 2.0f, -25.0f), global::tableWidth, 50.0f, Ogre::Vector3::NEGATIVE_UNIT_Y, "Examples/Wand", 5, Ogre::Vector3::NEGATIVE_UNIT_Z, true);

	
	const Ogre::Real coverRatio = 1.0f / 3.0f;

	// ---------------------------------------
	//const Ogre::Real    halfRatio      = 1.0f / 2.0f - coverRatio;
	const Ogre::Vector3 topMidPoint    = Ogre::Vector3(0.0f, global::tableHeight / 2.0f,                 0.0f);
	//const Ogre::Vector3 bottomMidPoint = Ogre::Vector3(0.0f, halfRatio * (global::tableHeight / 2.0f), -50.0f);
	//const Ogre::Vector3 bottomMidPoint = Ogre::Vector3(0.0f, (global::tableHeight / 2.0f) - (coverRatio * global::tableHeight), -50.0f);
	const Ogre::Vector3 bottomMidPoint = Ogre::Vector3(0.0f, (global::tableHeight / 2.0f) - (coverRatio * global::tableHeight), -fishTankDepth);

	const Ogre::Vector3 position = bottomMidPoint + 1.0f / 2.0f * (topMidPoint - bottomMidPoint);

	Ogre::Vector3 up     = (topMidPoint - bottomMidPoint).normalisedCopy();
	Ogre::Vector3 normal = Ogre::Vector3::UNIT_X.crossProduct(up).normalisedCopy();
	
	//const Ogre::Real planeHeight = Ogre::Math::Sqrt( Ogre::Math::Sqr( coverRatio * global::tableHeight ) + Ogre::Math::Sqr(50.0f));
	const Ogre::Real planeHeight = Ogre::Math::Sqrt( Ogre::Math::Sqr( coverRatio * global::tableHeight ) + Ogre::Math::Sqr(fishTankDepth));
	// ---------------------------------------

	bauePlane("static_walls_back", position, global::tableWidth, planeHeight, normal, "Examples/Wand2", 5, up, true);
	//bauePlane("static_walls_back",  Ogre::Vector3(0.0f, global::tableHeight / 3.0f, -25.0f), global::tableWidth, planeHeight, Ogre::Vector3(0, -1, 3).normalisedCopy(), "Examples/Wand", 5, Ogre::Vector3(0, 3, 1).normalisedCopy(), true);
	//bauePlane("static_walls_back",  Ogre::Vector3(0.0f, global::tableHeight / 3.0f, -25.0f), global::tableWidth, planeHeight, Ogre::Vector3(0, -1, 0).normalisedCopy(), "Examples/Wand", 5, Ogre::Vector3(0, 0, 1).normalisedCopy(), true);

	bauePlane("static_walls_front", Ogre::Vector3(0.0f, -(global::tableHeight / 2.0f), -fishTankDepth / 2.0f), global::tableWidth, fishTankDepth, Ogre::Vector3::UNIT_Y, "Examples/Wand", 5, Ogre::Vector3::NEGATIVE_UNIT_Z, true);
	bauePlane("static_walls_left",  Ogre::Vector3(-(global::tableWidth / 2.0f), 0.0f, -fishTankDepth / 2.0f), global::tableHeight, fishTankDepth, Ogre::Vector3::UNIT_X, "Examples/Wand", 5, Ogre::Vector3::NEGATIVE_UNIT_Z, true);
	bauePlane("static_walls_right", Ogre::Vector3(global::tableWidth / 2.0f, 0.0f, -fishTankDepth / 2.0f), global::tableHeight, fishTankDepth, Ogre::Vector3::NEGATIVE_UNIT_X, "Examples/Wand", 5, Ogre::Vector3::NEGATIVE_UNIT_Z, true);

	// Experiment boxes
	float scale = 1.0f;//5.0f;//0.1f;
	Ogre::Vector3 scaleShadow (0.1f, 0.1f, 0.00001f);
	
	createBox(Ogre::Vector3(-10.0f,  10.0f, -30.0f),   scale, "dynamic_box1", "Examples/Teil1", true);
	createBox(Ogre::Vector3(-10.0f,  10.0f, -30.0f),   scale, "dynamic_box2", "Examples/Teil2", true);
	createBox(Ogre::Vector3(-10.0f,  10.0f, -30.0f),   scale, "dynamic_box3", "Examples/Teil3", true);
	createBox(Ogre::Vector3(-10.0f, -10.0f, -30.0f),   scale, "dynamic_box4", "Examples/Teil4", true);


	createTriangleMesh(Ogre::Vector3(20.0f,  10.0f, -31.0f), "dynamic_tower_1",   "Examples/Teil1", "tower.mesh");

	createTriangleMesh(Ogre::Vector3(20.0f,  10.0f,-15.0f), "dynamic_ring1",   "Examples/Teil1", "ring1.mesh");
	createTriangleMesh(Ogre::Vector3(20.0f,  10.0f,-10.0f), "dynamic_ring2",   "Examples/Teil1", "ring2.mesh");
	createTriangleMesh(Ogre::Vector3(20.0f,  10.0f,-5.0f), "dynamic_ring3",   "Examples/Teil1", "ring3.mesh");


	/*createTriangleMesh(Ogre::Vector3(-0.0f,   0.0f, -25.0f), "dynamic_tower_1_1", "Examples/Teil1", "circle_1.mesh");
	createTriangleMesh(Ogre::Vector3( 0.0f,  10.0f, -10.0f), "dynamic_tower_1_2", "Examples/Teil1", "circle_2.mesh");
	createTriangleMesh(Ogre::Vector3( 0.0f, -10.0f,  -5.0f), "dynamic_tower_1_3", "Examples/Teil1", "circle_3.mesh");*/

	//createBox(Ogre::Vector3 (0.0f, 0.0f, 0.0f),   scale, "blaaa", "Examples/Black", false);
	
	bn[0] = mSceneMgr->getSceneNode("dynamic_box1_node");
	bn[1] = mSceneMgr->getSceneNode("dynamic_box2_node");
	bn[2] = mSceneMgr->getSceneNode("dynamic_box3_node");
	bn[3] = mSceneMgr->getSceneNode("dynamic_box4_node");
	
	// Stage 1
	//Ogre::Entity *stageEnt = mSceneMgr->createEntity("wallMiddle", "cube.mesh");
	//stageEnt->setCastShadows(false);
	//stageEnt->setMaterialName("Examples/GreenT");
	//Ogre::SceneNode *stageNode = mSceneMgr->getRootSceneNode()->createChildSceneNode("TrennwandNode");
	//stageNode->attachObject(stageEnt);
	//stageNode->setPosition(0.0f, 0.0f, -25.0f);
	//stageNode->setScale(0.001f, global::tableHeight / 10.0f, 0.5f);

	//// Stage 2
	//Ogre::Vector2 boxPos (0.0f, 0.0f); // Position des Boden (mitte)
	//Ogre::Vector3 boxSize (50.0f, 50.0f, 25.0f);
	//float thickness = 0.04f;
	//Ogre::String matName = "Examples/GreenT";
	//stage2Pos[0] = Ogre::Vector3(boxPos.x, boxSize.y / 2.0f + boxPos.y, -50.0f + (boxSize.z / 2.0f ));
	//stage2Pos[1] = Ogre::Vector3(boxPos.x, -boxSize.y / 2.0f + boxPos.y, -50.0f + (boxSize.z / 2.0f ));
	//stage2Pos[2] = Ogre::Vector3(boxPos.x - boxSize.y / 2.0f - 50 * thickness, boxPos.y, -50.0f + (boxSize.z / 2.0f ));
	//stage2Pos[3] = Ogre::Vector3(boxPos.x + boxSize.y / 2.0f + 50 * thickness , boxPos.y, -50.0f + (boxSize.z / 2.0f ));
	//createBox(stage2Pos[0], Ogre::Vector3(boxSize.x/100.0f, thickness, boxSize.z/100.0f), "boxWandOben", matName, false, true);
	//createBox(stage2Pos[1], Ogre::Vector3(boxSize.x/100.0f, thickness, boxSize.z/100.0f), "boxWandUnten", matName, false, true);
	//createBox(stage2Pos[2], Ogre::Vector3(thickness, boxSize.y/100.0f + thickness, boxSize.z/100.0f), "boxWandLinks", matName, false, true);
	//createBox(stage2Pos[3], Ogre::Vector3(thickness, boxSize.y/100.0f + thickness, boxSize.z/100.0f), "boxWandRechts", matName, false, true);

	//// Stage 3
	//createBox(Ogre::Vector3::ZERO, Ogre::Vector3(0.40f, 0.40f, 0.25f), "Stage3Box", "Examples/Wand", true, false);
	//for (unsigned int i=0; i < mBodies.size(); i++) {
	//		if(mBodies.at(i)->getName() == "rbStage3Box") {
	//			stage3BoxRb = mBodies.at(i);
	//		} else if(mBodies.at(i)->getName() == "rbbox1") {
	//			rb[0] =  mBodies.at(i);
	//		} else if (mBodies.at(i)->getName() == "rbbox2") {
	//			rb[1] = mBodies.at(i);>
	//		} else if (mBodies.at(i)->getName() == "rbbox3") {
	//			rb[2] = mBodies.at(i);
	//		} else if (mBodies.at(i)->getName() == "rbbox4") {
	//			rb[3] = mBodies.at(i);
	//		}
	//}

	/*for (size_t i = 0; i < mBodies.size(); ++i) {
			if (mBodies.at(i)->getName() == "rbbox1") {
				rb[0] =  mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "rbbox2") {
				rb[1] = mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "rbbox3") {
				rb[2] = mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "rbbox4") {
				rb[3] = mBodies.at(i);
			}
	}*/

	for (size_t i = 0; i < mBodies.size(); ++i) {
			if (mBodies.at(i)->getName() == "dynamic_box1_body") {
				rb[0] =  mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "dynamic_box2_body") {
				rb[1] = mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "dynamic_box3_body") {
				rb[2] = mBodies.at(i);
			} 
			else if (mBodies.at(i)->getName() == "dynamic_box4_body") {
				rb[3] = mBodies.at(i);
			}
	}

	btVector3 af (1,1,0);
	rb[0]->getBulletRigidBody()->setAngularFactor(af);
	rb[1]->getBulletRigidBody()->setAngularFactor(af);
	rb[2]->getBulletRigidBody()->setAngularFactor(af);
	rb[3]->getBulletRigidBody()->setAngularFactor(af);
}

void OgreBulletCollisionMetaphor::resetOrientation(Ogre::SceneNode* node) {
	static const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / -2.0f), Ogre::Vector3(0,0,1));
	node->setOrientation(initialOrientation);
}

void OgreBulletCollisionMetaphor::resyncPhysics(OgreBulletDynamics::RigidBody* rigidBody) {
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

//-------------------------------------------------------------------------------------------------
OgreBulletCollisionMetaphor::~OgreBulletCollisionMetaphor(){

	// OgreBullet physic delete - RigidBodies
	std::deque<OgreBulletDynamics::RigidBody *>::iterator itBody = mBodies.begin();
	while (mBodies.end() != itBody) {   
		delete *itBody; 
		++itBody;
	}
	
	// OgreBullet physic delete - Shapes
	std::deque<OgreBulletCollisions::CollisionShape *>::iterator itShape = mShapes.begin();
	while (mShapes.end() != itShape) {   
		delete *itShape; 
		++itShape;
	}

	mBodies.clear();
	mShapes.clear();
	
//	delete mWorld->getDebugDrawer();
//	mWorld->setDebugDrawer(0);
//	delete mWorld;
}



//-------------------------------------------------------------------------------------------------
void OgreBulletCollisionMetaphor::bauePlane(Ogre::String name, Ogre::Vector3 pos, float width, float height, Ogre::Vector3 norm, Ogre::String texture, int segments, Ogre::Vector3 upVector, bool bb) {
	  
	Ogre::Plane plane(norm, 0.0f); //(normal, distance)
    Ogre::MeshManager::getSingleton().createPlane( name + "_mesh", 
                                            Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME, 											
											plane, width, height, 1, 1, true, 1, segments, segments, upVector);
	// Create an entity 
    Ogre::Entity* ent = mSceneMgr->createEntity(name + "_entity", name + "_mesh");
	ent->setMaterialName(texture); // change it to whatever you want
	Ogre::SceneNode *node = mSceneMgr->getRootSceneNode()->createChildSceneNode(name + "_node");
	node->attachObject(ent);
	node->setPosition(pos);

	//boundingbox
	if(bb) {
		//Ogre::Vector3 BSize(ent->getBoundingBox().getSize() / 2);
		//BSize.z = 1.0f;
		OgreBulletCollisions::CollisionShape *planeShape = new OgreBulletCollisions::StaticPlaneCollisionShape(norm,0);
		//planeShape->getBulletShape()->setUserPointer(ent);
		OgreBulletDynamics::RigidBody *planeBody = new OgreBulletDynamics::RigidBody(name + "_body" , mWorld);
		planeBody->setStaticShape(node, planeShape, 0.9f, 0.8f, // dynamic bodymass
							pos);	// starting position of the CollisionBox
	
		planeBody->setDamping(0.0f, 0.0f);
		mShapes.push_back(planeShape);
		mBodies.push_back(planeBody);
	}

}

//-------------------------------------------------------------------------------------------------
void OgreBulletCollisionMetaphor::update(float timeSinceLastUpdate) {
	// update Bullet Physics animation
	if (_active)
		mWorld->stepSimulation(timeSinceLastUpdate / 100.0f, 4, 1.0f/60);
	//updateShadows(); // NOTE: Alex: Removed for kinect
	//if (!rotation) {
	//	resetRotation();
	//}
	
}

//-------------------------------------------------------------------------------------------------
void OgreBulletCollisionMetaphor::handleEvent(viargo::Event* event) {

	
	if (typeid(*event) == typeid(viargo::KeyEvent)) {
		//viargo::KeyEvent& key = *((viargo::KeyEvent*)event);
		//
		//viargo::KeyboardKey::KeyCode key_1, key_2;

		//key_1 = viargo::KeyboardKey::KEY_9;
		//key_2 = viargo::KeyboardKey::KEY_P;

		//bool is_equal_1 = key_1 == key_2;
		//bool is_equal_2 = ((int)key_1) == ((int)key_2);
		//int  is_equal_3 = key_1 & key_2;
		//int  is_equal_4 = ((int)key_1) & ((int)key_2);

		//if (key.action() == viargo::KeyEvent::KEY_PRESSED && key.key() == viargo::KeyboardKey::KEY_6) {	
		//	mSceneMgr->getSceneNode("boxWandObenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandUntenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandLinksNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandRechtsNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("TrennwandNode")->setPosition(0.0f, 0.0f, -25.0f);
		//	stage3BoxRb->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
		//	Ogre::Vector3 position (0.0f, 0.0f, -500.0f);
		//	btTransform transform; //Declaration of the btTransform
		//	transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		//	transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
		//	stage3BoxRb->getBulletRigidBody()->setWorldTransform(transform);
		//	resetPosition();
		//} else if (key.action() == viargo::KeyEvent::KEY_PRESSED && key.key() == viargo::KeyboardKey::KEY_7) {
		//	mSceneMgr->getSceneNode("boxWandObenNode")->setPosition(stage2Pos[0]);
		//	mSceneMgr->getSceneNode("boxWandUntenNode")->setPosition(stage2Pos[1]);
		//	mSceneMgr->getSceneNode("boxWandLinksNode")->setPosition(stage2Pos[2]);
		//	mSceneMgr->getSceneNode("boxWandRechtsNode")->setPosition(stage2Pos[3]);
		//	mSceneMgr->getSceneNode("TrennwandNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	stage3BoxRb->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
		//	Ogre::Vector3 position (0.0f, 0.0f, -500.0f);
		//	btTransform transform; //Declaration of the btTransform
		//	transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		//	transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
		//	stage3BoxRb->getBulletRigidBody()->setWorldTransform(transform);
		//	resetPosition();
		//} else if (key.action() == viargo::KeyEvent::KEY_PRESSED && key.key() == viargo::KeyboardKey::KEY_8) {
		//	mSceneMgr->getSceneNode("boxWandObenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandUntenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandLinksNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandRechtsNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("TrennwandNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	stage3BoxRb->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
		//	Ogre::Vector3 position (0.0f, 0.0f, 0.0f);
		//	btTransform transform; //Declaration of the btTransform
		//	transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		//	transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
		//	stage3BoxRb->getBulletRigidBody()->setWorldTransform(transform);
		//	stage3BoxRb->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
		//	resetPosition();
		//} else if (key.action() == viargo::KeyEvent::KEY_PRESSED && key.key() == viargo::KeyboardKey::KEY_9) {
		//	mSceneMgr->getSceneNode("boxWandObenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandUntenNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandLinksNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("boxWandRechtsNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	mSceneMgr->getSceneNode("TrennwandNode")->setPosition(0.0f, 0.0f, -100.0f);
		//	stage3BoxRb->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));
		//	Ogre::Vector3 position (0.0f, 0.0f, -500.0f);
		//	btTransform transform; //Declaration of the btTransform
		//	transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		//	transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
		//	stage3BoxRb->getBulletRigidBody()->setWorldTransform(transform);
		//	resetPosition();
		//}

	}
	else {
		// TODO: handle all other events here

	}
}

void OgreBulletCollisionMetaphor::createBox(Ogre::Vector3 position, float scale, Ogre::String name, Ogre::String materialname, bool bb) {
	createBox(position, Ogre::Vector3(scale, scale, scale), name, materialname, bb, false);
}

void OgreBulletCollisionMetaphor::createBox(Ogre::Vector3 position, Ogre::Vector3 scale, Ogre::String name, Ogre::String materialname, bool bb, bool sShape, const Ogre::String& meshName) {
	Ogre::Vector3 size = Ogre::Vector3::ZERO;
	// starting position of the box (10 units in front of the camera)
	// create an ordinary, Ogre mesh with texture
	Ogre::Entity *entity = mSceneMgr->createEntity(name + "_entity", meshName);			

	//Ogre::Entity *entity = mSceneMgr->createEntity(name, "Book.mesh");			    
	entity->setCastShadows(true);
	// we need the bounding box of the box to be able to set the size of the Bullet-box
	Ogre::AxisAlignedBox bbox = entity->getBoundingBox();
	size = bbox.getSize();
	size /= 2.0f;  // only the half needed
	size *= 0.95f; // Bullet margin is a bit bigger so we need a smaller size
	//size *= 1.5f;
							   // (Bullet 2.76 Physics SDK Manual page 18)
	entity->setMaterialName(materialname);

	//Ogre::SceneNode *secondTransformNode = mSceneMgr->getRootSceneNode()->createChildSceneNode(name + "_transform_node");

	Ogre::SceneNode *node = mSceneMgr->getRootSceneNode()->createChildSceneNode(name+ "_node");  
	//Ogre::SceneNode *node = secondTransformNode->createChildSceneNode(name + "_node");  
	node->attachObject(entity);
	//node->scale(scale);	// the cube is too big for us
	//size.x *= scale.x;
	//size.y *= scale.y;
	//size.z *= scale.z; // don't forget to scale down the Bullet-box too
	if (bb) { 
		// after that create the Bullet shape with the calculated size
		OgreBulletCollisions::BoxCollisionShape *sceneBoxShape = new OgreBulletCollisions::BoxCollisionShape(size);
		// and the Bullet rigid body
		//OgreBulletDynamics::RigidBody *defaultBody = new OgreBulletDynamics::RigidBody("rb" + name,	mWorld);
		OgreBulletDynamics::RigidBody *defaultBody = new OgreBulletDynamics::RigidBody(name + "_body", mWorld);

		node->getUserObjectBindings().setUserAny("rigid_body", Ogre::Any(defaultBody));

		const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / -2.0f), Ogre::Vector3(0,0,1));

		if (sShape) {
			//defaultBody->setStaticShape(node, sceneBoxShape, 0.02f, 110.0f, position, Ogre::Quaternion(0, 0, 0, 1));
			defaultBody->setStaticShape(node, sceneBoxShape, 0.02f, 110.0f, position, initialOrientation);
		} 
		else {
			defaultBody->setShape(	node,
								sceneBoxShape,
								0.2f,			// dynamic body restitution
								110.6f,			// dynamic body friction
								100.0f, 			// dynamic bodymass
								position,		// starting position of the box
								initialOrientation);
								//Ogre::Quaternion(0, 0, 0, 1));// orientation of the box
		}
					// update the entity counter
		mNumEntitiesInstanced++;
		defaultBody->getBulletRigidBody()->setActivationState(DISABLE_DEACTIVATION);
				
					// push the created objects to the dequese
		mShapes.push_back(sceneBoxShape);
		mBodies.push_back(defaultBody);
	}
}

void OgreBulletCollisionMetaphor::createTriangleMesh(const Ogre::Vector3& position, const std::string& name, const std::string& materialName, const std::string& meshName) {
	Ogre::Entity *entity = mSceneMgr->createEntity(name + "_entity", meshName);			    
	entity->setCastShadows(true);
	//entity->setMaterialName(materialName);
	
	const Ogre::Quaternion initialOrientation = Ogre::Quaternion(Ogre::Radian(Ogre::Math::PI / 2.0f), Ogre::Vector3(1,0,0));

	Ogre::SceneNode *node = mSceneMgr->getRootSceneNode()->createChildSceneNode(name+ "_node");  
	node->attachObject(entity);
	node->setPosition(position);
	node->setOrientation(initialOrientation);
	//Ogre::Vector3 scale = Ogre::Vector3(0.1f, 0.1f, 0.1f);

	//node->setScale(scale);


	

	OgreBulletCollisions::StaticMeshToShapeConverter *smtsc = new OgreBulletCollisions::StaticMeshToShapeConverter();
	smtsc->addEntity(entity);
	
	OgreBulletCollisions::CompoundCollisionShape* shape = smtsc->createConvexDecomposition();
	//OgreBulletCollisions::TriangleMeshCollisionShape *tri = smtsc->createTrimesh();
	
	OgreBulletDynamics::RigidBody *body    = new OgreBulletDynamics::RigidBody(name + "_body", mWorld);
	//btScaledBvhTriangleMeshShape *triShape = new btScaledBvhTriangleMeshShape((btBvhTriangleMeshShape*)(tri->getBulletShape()), btVector3(2.0f, 2.0f, 2.0f));

	body->setShape(node, shape, 0.02f, 110.0f, 100.0f, position, initialOrientation);
	//body->setStaticShape(tri, 0.02f, 110.0f, position, initialOrientation);
	//body->setStaticShape(triShape, 0.02f, 110.0f, position, initialOrientation);
	//body->setShape(node, tri, 0.02f, 110.0f, 100.0f, position, initialOrientation);
	//body->setDebugDisplayEnabled(true);

	node->getUserObjectBindings().setUserAny("rigid_body", Ogre::Any(body));


				// update the entity counter
	mNumEntitiesInstanced++;
	body->getBulletRigidBody()->setActivationState(DISABLE_DEACTIVATION);
				
				// push the created objects to the dequese
	//mShapes.push_back(tri);
	//mShapes.push_back(shape);
	mBodies.push_back(body);
	
}

//void OgreBulletCollisionMetaphor::updateShadows() {
//	return;
//	float scalefac = 18.0f; // 20 noch etwas kleiner machen!!
//
//	Ogre::SceneNode *box1 = mSceneMgr->getSceneNode("box1Node");
//	Ogre::SceneNode *shadow1 = mSceneMgr->getSceneNode("Shadow1Node");
//	shadow1->setPosition(box1->getPosition().x, box1->getPosition().y, 0.0f);
//	float scale = 0.1f + ((0.1f/scalefac) * (box1->getPosition().z + 45.155f)); 
//	//std::cout << "scale: " << box1->getPosition().z << std::endl;
//	shadow1->setScale(scale, scale, 0.000001f);
//	//shadow1->setOrientation(Ogre::Quaternion(box1->getOrientation().getRoll(), Ogre::Vector3(0.0f, 0.0f, 1.0f)));
//
//	line1->clear();
//	line1->addPoint(box1->getPosition());
//	line1->addPoint(shadow1->getPosition());
//	line1->update();
//
//	
//	Ogre::SceneNode *box2 = mSceneMgr->getSceneNode("box2Node");
//	Ogre::SceneNode *shadow2 = mSceneMgr->getSceneNode("Shadow2Node");
//	shadow2->setPosition(box2->getPosition().x, box2->getPosition().y, 0.0f);
//	scale = 0.1f + ((0.1f/scalefac) * (box2->getPosition().z + 45.155f)); 
//	shadow2->setScale(scale, scale, 0.000001f);
//	//shadow2->setOrientation(Ogre::Quaternion(box2->getOrientation().getRoll(), Ogre::Vector3(0.0f, 0.0f, 1.0f)));
//
//	line2->clear();
//	line2->addPoint(box2->getPosition());
//	line2->addPoint(shadow2->getPosition());
//	line2->update();
//
//
//	Ogre::SceneNode *box3 = mSceneMgr->getSceneNode("box3Node");
//	Ogre::SceneNode *shadow3 = mSceneMgr->getSceneNode("Shadow3Node");
//	shadow3->setPosition(box3->getPosition().x, box3->getPosition().y, 0.0f);
//	scale = 0.1f + ((0.1f/scalefac) * (box3->getPosition().z + 45.155f)); 
//	shadow3->setScale(scale, scale, 0.000001f);
//	//shadow3->setOrientation(Ogre::Quaternion(box3->getOrientation().getRoll(), Ogre::Vector3(0.0f, 0.0f, 1.0f)));
//
//	line3->clear();
//	line3->addPoint(box3->getPosition());
//	line3->addPoint(shadow3->getPosition());
//	line3->update();
//
//
//	Ogre::SceneNode *box4 = mSceneMgr->getSceneNode("box4Node");
//	Ogre::SceneNode *shadow4 = mSceneMgr->getSceneNode("Shadow4Node");
//	shadow4->setPosition(box4->getPosition().x, box4->getPosition().y, 0.0f);
//	scale = 0.1f + ((0.1f/scalefac) * (box4->getPosition().z + 45.155f)); 
//	shadow4->setScale(scale, scale, 0.000001f);
//	//shadow4->setOrientation(Ogre::Quaternion(box4->getOrientation().getRoll(), Ogre::Vector3(0.0f, 0.0f, 1.0f)));
//
//	line4->clear();
//	line4->addPoint(box4->getPosition());
//	line4->addPoint(shadow4->getPosition());
//	line4->update();
//}

void OgreBulletCollisionMetaphor::resetRotation() {
	for (int i = 0; i < 4; i++) {
		Ogre::Vector3 position = bn[i]->getPosition();
		btTransform transform; //Declaration of the btTransform
		transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position)); //Set the new position/origin
		rb[i]->getBulletRigidBody()->setWorldTransform(transform); //Apply the btTransform to the body
	}
}

void OgreBulletCollisionMetaphor::resetPosition() {
	Ogre::Vector3 position[4];
	position[0] = Ogre::Vector3(-30.0f, 0.0f, -30.0f);
	position[1] = Ogre::Vector3(30.0f, 0.0f, -30.0f);
	position[2] = Ogre::Vector3(-15.0f, -25.0f, -30.0f);
	position[3] = Ogre::Vector3(15.0f, -25.0f, -30.0f);

	for (int i = 0; i < 4; i++) {
		btTransform transform; //Declaration of the btTransform
		transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.
		transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(position[i])); //Set the new position/origin
		rb[i]->getBulletRigidBody()->setWorldTransform(transform); //Apply the btTransform to the body
	}
}

