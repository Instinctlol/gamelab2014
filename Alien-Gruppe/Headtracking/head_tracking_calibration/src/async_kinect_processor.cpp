#include <windows.h>
#include <sstream>

#include "async_kinect_processor.h"

#include "experiment.h"

#include "globals.h"
#include "kinect.h"
#include "kinect_wrapper.h"

#include "OgreBulletCollisionMetaphor.h"

#include <OgreManualObject.h>

AsyncKinectProcessor::AsyncKinectProcessor(Ogre::SceneManager* sceneManager, Experiment* experiment)
	:_active(false)
	,_open(false)
	,_lastState(false)
	,_timerFilter(false)
	,_calibrationRunning(false)
	,_calibrationFingerFound(false)
	,_stopThread(false)
	,_objectGrabbing(false)
	,_showDebugInformation(false)
	,_oldHandPos(0, 0, 0)
	,_oldPoint(0, 0, 0)
	,_sceneMgr(sceneManager)
	,_experiment(experiment)
	,_activeNode(nullptr)
	,_kinectInitialized(false)
{
	_initialize();
}

AsyncKinectProcessor::~AsyncKinectProcessor() {
	// Deinitialize and stop thread
	_stopThread = true;
	_thread->join();
	delete _thread;

	delete _kinect;
	delete[] _colorCoordinates;
}

void AsyncKinectProcessor::toggleProcessing(bool active) {
	if (_kinectInitialized) {
		_active = active;
	}
}

void AsyncKinectProcessor::update(double deltaTime) {
	if (!_active) {
		return;
	}

	// Check for changes
	bool stateChange = false;
	if (_open && !_lastState || !_open && _lastState) {
		stateChange = true;
	}

	// ----------------------------------------------------------------
	
	// Direction form kinect sensor to detected hand position
	Ogre::Vector3 kinectHandDirection = _currentHandPosition - _kinectSensorPosition;

	// Get distance between sensor and hand (needed for ray)
	const float distanceKinectHand = kinectHandDirection.length();

	// Normalize direction vector
	kinectHandDirection.normalise();

	// Ray from kinect sensor to detected hand position
	Ogre::Ray handSceneRay = Ogre::Ray(_kinectSensorPosition, kinectHandDirection);

	// Start ray at hand and add additional offset into the scene (related to simulated workbench depth)
	const float rayLength = distanceKinectHand + 30.0f;

	// Get projected hand position
	Ogre::Vector3 newHandPosition = handSceneRay.getPoint(rayLength);

	// ----------------------------------------------------------------

	
	// Handle logic
	_handNode->setPosition(newHandPosition);
	_handleMoveObject(newHandPosition);

	// Highlight objects in hand region
	if (!stateChange && _open) {
		bool marked = false;

		std::vector<Ogre::SceneNode*> sceneNodes = _experiment->experimentObjectSceneNodes();

		for (auto iter = sceneNodes.begin(); iter != sceneNodes.end(); ++iter) {
			bool result = _testObjectIntersection(*iter, newHandPosition);

			const std::string& materialName = _experiment->guessMaterialName(*iter);

			/*std::stringstream sstream;
			sstream << "Examples/Teil" << (i + 1);*/

			//Ogre::MaterialPtr material = (Ogre::MaterialPtr)Ogre::MaterialManager::getSingleton().getByName(sstream.str());

			Ogre::MaterialPtr material = (Ogre::MaterialPtr)Ogre::MaterialManager::getSingleton().getByName(materialName);

			Ogre::Pass* pass = 0;
			Ogre::Material::TechniqueIterator it = material->getTechniqueIterator();
            
			while (it.hasMoreElements())
			{
				Ogre::Technique* tech = it.getNext();
				
				if (tech->getSchemeName() == "Default") {
					pass = tech->getPass(0);
					break;
				}
			}
	
			if (pass == 0) { // Error
				std::cout << "Error in material " << materialName << " during query of default pass !" << std::endl;
				return;
			}
			
			if (result && !marked) {
				pass->setAmbient(0.3f, 0.3f, 1.0f);
				pass->setDiffuse(0.3f, 0.3f, 1.0f, 1.0f);
				marked = true;
			}
			else {
				pass->setAmbient(1.0f, 1.0f, 1.0f);
				pass->setDiffuse(1.0f, 1.0f, 1.0f, 1.0f);
			}

			material->compile();
		}
	}

	if (stateChange && !_open) {
		_handleObjectGrabbing(newHandPosition);
	}
	else if (stateChange && _open) {
		if (_activeNode != nullptr) {

			OgreBulletDynamics::RigidBody* body = Ogre::any_cast<OgreBulletDynamics::RigidBody*>(_activeNode->getUserObjectBindings().getUserAny("rigid_body"));

			_experiment->resetOrientation(_activeNode);
			_experiment->resyncPhysics(body);
			_experiment->togglePhysics(true);			
			
			_activeNode = nullptr;
		}
	}

	// Copy current state
	_lastState = _open;

	if (_calibrationRunning) {
		//Update calibration timer
		_kinectCalibrationMetaphor->update(deltaTime);//(evt.timeSinceLastFrame);

		// Find fingers
		//cv::Point3f finger;

		//bool found  = _findFinger(finger, &depthFrame);

		bool holding = false;

		// Check if holding
		const double EPS = 1.0;

		if (_calibrationFingerFound) {
			// Distance from current point to last point
			const double distance = sqrt( 
				(_calibrationFinger.x - _oldPoint.x) * (_calibrationFinger.x - _oldPoint.x) +
				(_calibrationFinger.y - _oldPoint.y) * (_calibrationFinger.y - _oldPoint.y) +
				(_calibrationFinger.z - _oldPoint.z) * (_calibrationFinger.z - _oldPoint.z)
				);

			if (distance < EPS) {
				holding = true;
			}
		}


		// Update calibration
		if (_calibrationFingerFound) {
			_kinectCalibrationMetaphor->updateCalibration(_calibrationFinger, holding);
			//cv::Point3f transformedFinger;
			//_kinectCalibrationMetaphor->transform(_calibrationFinger, transformedFinger);
			_oldPoint = _calibrationFinger;
		}

		if (_kinectCalibrationMetaphor->isCali() == false) {
			_calibrationRunning = false;
		}
	}

	_mutex.lock();

	const size_t imageSize = 4 * 640 * 480;

	_kinectShadowBuffer->lock(Ogre::HardwareBuffer::HBL_DISCARD);

	const Ogre::PixelBox& pixelBox = _kinectShadowBuffer->getCurrentLock();

	unsigned char* pixelDestination = static_cast<unsigned char*>(pixelBox.data);

	// Reset destination image to zero
	memset(pixelDestination, 0, imageSize);
	
	// Pointer to kinect source image (8 bit grayscale depth)
	unsigned char* pixelSource = _depth8Bit.data;

	// Iterate over destination image
	for(size_t index = 0; index < imageSize; ) {
		if(*pixelSource != 0) {		
			/*pixelDestination[index++] =  26;
			pixelDestination[index++] =  34;
			pixelDestination[index++] =  99;
			pixelDestination[index++] = 150;*/
			pixelDestination[index++] =   0;
			pixelDestination[index++] =   0;
			pixelDestination[index++] =   0;
			pixelDestination[index++] = 125;
		}
		else {
			pixelDestination[index++] = 255;
			pixelDestination[index++] = 255;
			pixelDestination[index++] = 255;
			pixelDestination[index++] =   0;
		}*
		
		// Increment depth frame pixel
		++pixelSource;
	}

	_kinectShadowBuffer->unlock();

	_mutex.unlock();

	// ---------------------------------------------------------------------------------------------

	std::vector<Ogre::SceneNode*> sceneNodes = _experiment->experimentObjectSceneNodes();

	for (auto iter = sceneNodes.begin(); iter != sceneNodes.end(); ++iter) {

	//for (int i = 0; i < 4; i++) {
		//Ogre::Vector3 boxPosition = _testBoxes[i]->getSceneNode()->getPosition();
		Ogre::Vector3 boxPosition = (*iter)->getPosition();
		const float distance = -5.0f;
		const Ogre::Vector3 difference = newHandPosition - boxPosition;

		/*std::stringstream sstream;
		sstream << "Examples/Teil" << (i + 1);*/

		const std::string& materialName = _experiment->guessMaterialName(*iter);

		Ogre::MaterialPtr material = (Ogre::MaterialPtr)Ogre::MaterialManager::getSingleton().getByName(materialName);

		Ogre::Pass* pass = 0;
		Ogre::Material::TechniqueIterator it = material->getTechniqueIterator();
            
		while (it.hasMoreElements())
		{
			Ogre::Technique* tech = it.getNext();
			pass = tech->getPass("ShadowPass");

			if (pass != 0) {
				break;
			}
		}
	
		if (pass == 0) { // Error
			std::cout << "Error in material " << materialName << " during query of additional shadow pass !" << std::endl;
			return;
		}

		//Ogre::Pass* pass = material->getTechnique(0)->getPass("ShadowPass");

		//bool result = _testObjectIntersection(_testBoxes[i]->getSceneNode(), newHandPosition);
		bool result = _testObjectIntersection(*iter, newHandPosition);

		if (difference.z <= distance && !result) {
			pass->getTextureUnitState(0)->setTextureName("KinectShadowEmpty");
		}
		else {
			pass->getTextureUnitState(0)->setTextureName("KinectShadow");
		}
	}
}

void AsyncKinectProcessor::_initialize() {
	// Create and initialize kinect wrapper
	_kinect = new Kinect();
	_kinectInitialized = _kinect->initialize(0, Kinect::DepthColor);

	_colorCoordinates = new LONG[2 * 640 * 480];

	// ----------------------------------------------------------------------
	// Kinect hand detection default parameters
	// ----------------------------------------------------------------------
	thresh = 210;
	contourFilterArea = 4000;
	threshValue = 10;
	// ----------------------------------------------------------------------
	
#ifdef VOID_SHADOWS_SHOW_OPENCV_TRACKBARS
	cv::namedWindow("depthMask");
	cv::createTrackbar("threshValue", "depthMask", &threshValue, 255);

	cv::namedWindow("grayImage cutoff test");
	cv::createTrackbar("thresh", "grayImage cutoff test", &thresh, 255);
	cv::createTrackbar("area_filter", "grayImage cutoff test", &contourFilterArea, 6000);
#endif
	
	// Create kinect calibration metaphor
	Ogre::Vector2 windowSize = Ogre::Vector2(global::tableWidth, global::tableHeight);
	_kinectCalibrationMetaphor = new ViargoOgreKinectTrackingCalibrationMetaphor(windowSize, 2, Ogre::Vector4(0.4f, 0.4f, 0.3f, 0.3f));

	// TODO: Remove
	//km = new KinectMetaphor(sceneManager, collisionMetaphor, _kinectCalibrationMetaphor);

	

	// Initialize and run thread for processing
	_thread = new nixie::thread(&AsyncKinectProcessor::_runThread, this);

	// Initialize physical objects which can be grabbed
	/*std::deque<OgreBulletDynamics::RigidBody*>* rigidBodies = _collisionMetaphor->rigidBodies();

	for (size_t i = 0; i < rigidBodies->size(); ++i) {
		if (rigidBodies->at(i)->getName() == "dynamic_box1_body") {
			_testBoxes[0] =  rigidBodies->at(i);
		} 
		else if (rigidBodies->at(i)->getName() == "dynamic_box2_body") {
			_testBoxes[1] = rigidBodies->at(i);
		} 
		else if (rigidBodies->at(i)->getName() == "dynamic_box3_body") {
			_testBoxes[2] = rigidBodies->at(i);
		} 
		else if (rigidBodies->at(i)->getName() == "dynamic_box4_body") {
			_testBoxes[3] = rigidBodies->at(i);
		}
	}*/
	
	// Initialize hand distant cursor
	Ogre::Entity *entity = _sceneMgr->createEntity("handpos", "cube.mesh");		
	entity->setVisible(false); // TODO
	entity->setCastShadows(false);
	//entity->setMaterialName("Examples/CubeTest");
	_handEntity = entity;

	Ogre::SceneNode *node = _sceneMgr->getRootSceneNode()->createChildSceneNode("handposNode");  
	node->attachObject(entity);
	node->scale(0.1f, 0.1f, 0.0001f);
	//node->scale(50.0f, 50.0f, 50.0f);
	_handNode = _sceneMgr->getSceneNode("handposNode");

	// Create texture storage for shadow
	Ogre::TexturePtr shadowTexture = Ogre::TextureManager::getSingleton().createManual(
		"KinectShadow", 
		Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME,
		Ogre::TEX_TYPE_2D,
		640, 
		480, 
		0, 
		Ogre::PF_BYTE_RGBA, 
		Ogre::TU_DYNAMIC_WRITE_ONLY);

	_kinectShadowBuffer = shadowTexture->getBuffer();

	// Initialize texture
	const size_t imageSize = 4 * 640 * 480;

	_kinectShadowBuffer->lock(Ogre::HardwareBuffer::HBL_DISCARD);

	const Ogre::PixelBox& pixelBox = _kinectShadowBuffer->getCurrentLock();

	unsigned char* pixelDestination = static_cast<unsigned char*>(pixelBox.data);

	// Reset destination image to zero
	memset(pixelDestination, 0, imageSize);

	_kinectShadowBuffer->unlock();

	// Create texture storage for empty shadow
	Ogre::TexturePtr emptyTexture = Ogre::TextureManager::getSingleton().createManual(
		"KinectShadowEmpty", 
		Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME,
		Ogre::TEX_TYPE_2D,
		640, 
		480, 
		0, 
		Ogre::PF_BYTE_RGBA, 
		Ogre::TU_DYNAMIC_WRITE_ONLY);

	Ogre::HardwarePixelBufferSharedPtr emptyShadowPixelBuffer = emptyTexture->getBuffer();

	// Initialize texture
	emptyShadowPixelBuffer->lock(Ogre::HardwareBuffer::HBL_DISCARD);

	const Ogre::PixelBox& pixelBox2 = emptyShadowPixelBuffer->getCurrentLock();

	pixelDestination = static_cast<unsigned char*>(pixelBox2.data);

	// Reset destination image to zero
	memset(pixelDestination, 0, imageSize);

	emptyShadowPixelBuffer->unlock();

	// Initialize buffered depth frame
	_depth8Bit = cv::Mat(480, 640, CV_8UC1, cv::Scalar(0));


	// Initialize texture projection
	//const Ogre::Radian fov_Y = 2.0f * Ogre::Math::ATan((global::tableHeight / 2.0f) / 150.0f);

	/*_projectiveFurstum = new Ogre::Frustum();
	_projectiveFurstum->setFOVy(fov_Y);
	_projectiveFurstum->setAspectRatio(global::tableWidth / global::tableHeight);

	Ogre::SceneNode* mProjectorNode = _sceneMgr->getRootSceneNode()->createChildSceneNode("DecalProjectorNode");
	mProjectorNode->attachObject(_projectiveFurstum);
	mProjectorNode->setPosition(0, 0, 150);
	mProjectorNode->setDirection(0, 0, -1);*/

	// Position and lookat in Kinect 3D space
	cv::Point3f startPosition  = cv::Point3f(0, 0,    0);
	cv::Point3f lookAtPosition = cv::Point3f(0, 0, -100);

	// Transform from kinect 3D space into Ogre3D scene space
	_kinectCalibrationMetaphor->transform(startPosition,  startPosition);
	_kinectCalibrationMetaphor->transform(lookAtPosition, lookAtPosition);

	// Calculate direction from look at position
	lookAtPosition -= startPosition;

	// Assign kinect location information
	_kinectSensorPosition = Ogre::Vector3(startPosition.x, startPosition.y, startPosition.z);
	_kinectSensorDirection = Ogre::Vector3(lookAtPosition.x, lookAtPosition.y, lookAtPosition.z);
	 
	// Setup projective frustum with kinect specific information
	_projectiveFurstum = new Ogre::Frustum();
	_projectiveFurstum->setFOVy(Ogre::Degree(43));
	_projectiveFurstum->setAspectRatio(640.0f / 480.0f);

	// Setup scene node for projective frustum with correct kinect sensor location and orientation
	Ogre::SceneNode* mProjectorNode = _sceneMgr->getRootSceneNode()->createChildSceneNode("DecalProjectorNode");
	mProjectorNode->attachObject(_projectiveFurstum);
	mProjectorNode->setPosition(startPosition.x, startPosition.y, startPosition.z);
	mProjectorNode->setDirection(lookAtPosition.x, lookAtPosition.y, lookAtPosition.z);

	// Setup main light to kinect sensor position
	_sceneMgr->getLight("MainLight")->setPosition(_kinectSensorPosition);

	// Add additional shadow pass to all relevant scene objects
	_addKinectMaterial("FishTankFloor",       _projectiveFurstum);
	_addKinectMaterial("FishTankSides",       _projectiveFurstum);
	_addKinectMaterial("FishTankTiltedBack",  _projectiveFurstum);
	_addKinectMaterial("Puzzle1", _projectiveFurstum);
	_addKinectMaterial("Puzzle2", _projectiveFurstum);
	_addKinectMaterial("Puzzle3", _projectiveFurstum);
	_addKinectMaterial("Puzzle4", _projectiveFurstum);
}

void AsyncKinectProcessor::_update(double deltaTime) {
	if (!_kinect) {
		return;
	}

	/*if (!_kinect->hasFrame()) {
		return;
	}*/

	// Block and wait for new frames (color + depth)
	_kinect->waitFrame();

	// Raw depth and color frame
	cv::Mat *depthMat = _kinect->depthFrame();
	cv::Mat *colorMat = _kinect->colorFrame();

	// ------------------------------------------------------------------------------------------
	// 1. Color => Grayscale
	// ------------------------------------------------------------------------------------------
	cv::Mat grayImage;

	_processConvertGrayScale(*colorMat, grayImage);
	// ------------------------------------------------------------------------------------------
	// 2. Clip grayscale color image
	// ------------------------------------------------------------------------------------------
	_processClipGrayScale(grayImage);

	// ------------------------------------------------------------------------------------------
	// 3. Color => Threshold
	// ------------------------------------------------------------------------------------------
	_processThresholdGrayScale(grayImage);
		
	// ------------------------------------------------------------------------------------------
	// 4. Color => Contour Detection
	// ------------------------------------------------------------------------------------------
	std::vector< std::vector< cv::Point > > filteredContours;
	_processContourDetection(grayImage, filteredContours);

	// ------------------------------------------------------------------------------------------
	// 6. Contour => Color Mask
	// ------------------------------------------------------------------------------------------
	cv::Mat colorMask;
	_processMaskContours(colorMask, filteredContours);

	// ------------------------------------------------------------------------------------------
	// 7. Color Mask => Depth Space Transformation
	// ------------------------------------------------------------------------------------------
	cv::Mat registeredColorMat;
	_processTransformColorToDepth(*colorMat, *depthMat, colorMask, registeredColorMat);

	// ------------------------------------------------------------------------------------------
	// 8. Mask Depth Image
	// ------------------------------------------------------------------------------------------
	_processMaskDepth(*depthMat, registeredColorMat, colorMask);
		
	// ------------------------------------------------------------------------------------------
	// 9. Remove shadow fingers
	// ------------------------------------------------------------------------------------------
	cv::Mat depth8;
	_processRemoveShadowsFromDepth(*depthMat, depth8, colorMask);

	// ------------------------------------------------------------------------------------------
	// 9b. Calibration
	// ------------------------------------------------------------------------------------------
	_processCalibration(*depthMat);

	// ------------------------------------------------------------------------------------------
	// 10. Approximate hand and cutoff arm from depth image
	// ------------------------------------------------------------------------------------------
	_processCutoffArm(*depthMat, depth8);

	// ----------------------------------------------------------------------------------------------
	// 11. Detect open / closed hand by convexity defects
	// ----------------------------------------------------------------------------------------------
	_processHandDetection(*depthMat);

	// ----------------------------------------------------------------------------------------------
	// 12. Handle shadow grabbing logic
	// ----------------------------------------------------------------------------------------------
	//_handleHandOperation(_currentHandPosition, _open);

	// Process shadow texturing
	_processDepthShadowTransformation(depth8);
		
	// Wait one frame to update opencv GUI
	if (_showDebugInformation) {
		cv::waitKey(1);
	}
}

bool AsyncKinectProcessor::isHandOpen() const {
	return _open;
}

Ogre::Vector3 AsyncKinectProcessor::position() const {
	return _currentHandPosition;
}

void AsyncKinectProcessor::startCalibration() {
	_kinectCalibrationMetaphor->startCalibration();
	_calibrationRunning = true;
}

void AsyncKinectProcessor::toggleShowDebugInformation() {
	_showDebugInformation = !_showDebugInformation;
}

bool AsyncKinectProcessor::_findFinger(cv::Point3f& point, const cv::Mat* depth) {
		// Scale image into 8 bit
		cv::Mat depth8 = cv::Mat(480, 640, CV_8UC1);
		depth->convertTo(depth8, CV_8UC1, 0.00390625);

		std::vector< std::vector< cv::Point > > contours;

		cv::threshold(depth8, depth8, 0.0, 255, cv::THRESH_BINARY);
		cv::findContours(depth8, contours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);
	
		cv::Point2i fingerTipCandidate = cv::Point2i(0, 480);

		const double MinAreaSize = 5000;

		cv::Mat debugMat(480, 640, CV_8UC3, cv::Scalar(0));
		std::vector< std::vector< cv::Point > > debugContours1;
		std::vector< std::vector< cv::Point > > debugContours2;

		// Search maximum x (side window), kick small areas away
		for (size_t outerIndex = 0; outerIndex < contours.size(); ++outerIndex) {
			const double areaSize = cv::contourArea(contours[outerIndex]);

			if (areaSize < MinAreaSize) {
				debugContours1.push_back(contours[outerIndex]);
				continue;
			}
			else {
				debugContours2.push_back(contours[outerIndex]);
			}

			for (size_t innerIndex = 0; innerIndex < contours[outerIndex].size(); ++innerIndex) {
				const cv::Point2i currentPoint = contours[outerIndex][innerIndex];

				// Search most right contour point
				if (currentPoint.y < fingerTipCandidate.y) {
					fingerTipCandidate = currentPoint;
				}
			}
		}

		cv::circle(debugMat, fingerTipCandidate, 2, cv::Scalar(0, 0, 255), CV_FILLED);
		cv::drawContours(debugMat, debugContours1, -1, cv::Scalar(255, 0, 0), 1);
		cv::drawContours(debugMat, debugContours2, -1, cv::Scalar(255, 255, 255), 1);
		cv::imshow("debugMat2", debugMat);

		if (fingerTipCandidate.x != 0 && fingerTipCandidate.y != 0) {
			unsigned short depthValue = depth->at<unsigned short>(fingerTipCandidate.y, fingerTipCandidate.x);
			Vector4 position = NuiTransformDepthImageToSkeleton(fingerTipCandidate.x, fingerTipCandidate.y, depthValue, NUI_IMAGE_RESOLUTION_640x480);
			point.x = position.x * 100.0 * -1.0; // Mirrored 
			point.y = position.y * 100.0;
			point.z = position.z * 100.0 * -1.0;

			return true;
		}	

		return false;
	}

Ogre::Vector3 AsyncKinectProcessor::_transformHandPosition(const cv::Point2f& position, unsigned short depthValue) {
	// Transform into kinect space coordinates
	Vector4 kinectSpacePosition = NuiTransformDepthImageToSkeleton(position.x, position.y, depthValue, NUI_IMAGE_RESOLUTION_640x480);

	// Mirror coordinate
	Point3f temp;
	temp.x = kinectSpacePosition.x * 100.0 * -1.0;
	temp.y = kinectSpacePosition.y * 100.0;
	temp.z = kinectSpacePosition.z * 100.0 * -1.0;

	// Transform into Ogre scene coordinate space
	_kinectCalibrationMetaphor->transform(temp, temp);
	return Ogre::Vector3(temp.x, temp.y, temp.z);
}

void AsyncKinectProcessor::_handleMoveObject(const Ogre::Vector3& position) {
	if (_activeNode != nullptr) {
		btTransform transform; //Declaration of the btTransform
		transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.

		//transform.setRotation(btQuaternion(

		Ogre::Vector3 translation       = position - _handGrabStartPosition;
		Ogre::Vector3 newObjectPosition = _objectGrabStartPosition + translation;

		OgreBulletDynamics::RigidBody* body = Ogre::any_cast<OgreBulletDynamics::RigidBody*>(_activeNode->getUserObjectBindings().getUserAny("rigid_body"));

		//transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(pos)); //Set the new position/origin
		transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(newObjectPosition)); //Set the new position/origin
		body->getBulletRigidBody()->setWorldTransform(transform);
		_activeNode->setPosition(newObjectPosition);
	}
}

bool AsyncKinectProcessor::_testObjectIntersection(Ogre::SceneNode* node, const Ogre::Vector3& position) {
	Ogre::Sphere handSphere = Ogre::Sphere(position, 3.0f);
	//_sceneMgr->_updateSceneGraph();
	bool result = node->_getWorldAABB().intersects(handSphere);
	return result;
}

void AsyncKinectProcessor::_handleObjectGrabbing(const Ogre::Vector3& position) {
	//for (int i = 0; i < 4; i++) {
	std::vector<Ogre::SceneNode*> sceneNodes = _experiment->experimentObjectSceneNodes();
	for (auto iter = sceneNodes.begin(); iter != sceneNodes.end(); ++iter) {
		/*float dist = 5.0f;
		Ogre::Vector3 posB = _testBoxes[i]->getSceneNode()->getPosition();
		const float distance = posB.distance(position);*/

		bool result = _testObjectIntersection(*iter, position);

		/*if (position.x > (posB.x - dist) && position.y > (posB.y - dist) && position.z > (posB.z - dist)) {
			if (position.x < (posB.x + dist) && position.y < (posB.y + dist) && position.z < (posB.z + dist)) {*/
		//if (distance <= dist) {
		if (result) {
				// box gefasst
				_activeNode = (*iter);
				//_testBoxes[_activeNode]->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));

				_handGrabStartPosition = position;
				//_objectGrabStartPosition = posB;
				_objectGrabStartPosition = _activeNode->getPosition();

				//btRigidBody* body = _testBoxes[_activeNode]->getBulletRigidBody();
				//_collisionMetaphor->dynamicWorld()->getBulletDynamicsWorld()->removeRigidBody(body);

				_experiment->resetOrientation(_activeNode);
				_experiment->togglePhysics(false);

				return;
		}
			/*}
		}*/
	}

	_activeNode = nullptr;
	return;
}

void AsyncKinectProcessor::_runThread(void* param) {
	// Pointer to instance
	AsyncKinectProcessor* instance = static_cast<AsyncKinectProcessor*>(param);

	// Necessary as argument in update method
	Ogre::Timer timer;

	while (!instance->_stopThread) {
		// Only run processing if active
		if (instance->_active) {
			// Calculate time since last update
			const double deltaTime = timer.getMilliseconds();

			// Reset timer
			timer.reset();

			// Update processing (time in seconds)
			instance->_update(deltaTime / 1000.0);
		}
		else {
			// Sleep in inactive state
			nixie::this_thread::sleep_for(nixie::chrono::milliseconds(100));
		}
	}
}

void AsyncKinectProcessor::_processConvertGrayScale(const cv::Mat& colorFrame, cv::Mat& grayScaleFrame) {
	grayScaleFrame = cv::Mat(480, 640, CV_8UC1, cv::Scalar(0));
	cv::cvtColor(colorFrame, grayScaleFrame, CV_RGB2GRAY);

	// Invert image, hand will get white, background black
	grayScaleFrame = 255 - grayScaleFrame;
}

void AsyncKinectProcessor::_processClipGrayScale(cv::Mat& grayScaleFrame) {
	const int clipLeft   = 65;
	const int clipRight  =  0;
	const int clipTop    = 50;
	const int clipBottom = 40;
		
	cv::rectangle(grayScaleFrame, cv::Rect(0,               0,                clipLeft, 480    ), cv::Scalar(0), CV_FILLED);
	cv::rectangle(grayScaleFrame, cv::Rect(640 - clipRight, 0,                640,      480    ), cv::Scalar(0), CV_FILLED);
	cv::rectangle(grayScaleFrame, cv::Rect(0,               480 - clipBottom, 640,      480    ), cv::Scalar(0), CV_FILLED);
	cv::rectangle(grayScaleFrame, cv::Rect(0,               0,                640,      clipTop), cv::Scalar(0), CV_FILLED);
}

void AsyncKinectProcessor::_processThresholdGrayScale(cv::Mat& grayScaleFrame) {
	cv::threshold(grayScaleFrame, grayScaleFrame, thresh, 255, cv::THRESH_BINARY);
}

void AsyncKinectProcessor::_processContourDetection(cv::Mat& grayScaleFrame, std::vector<std::vector<cv::Point> >& outputContours) {
	std::vector< std::vector< cv::Point > > colorHandContours;
	cv::findContours(grayScaleFrame, colorHandContours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);

	outputContours.clear();

	for (size_t index = 0; index < colorHandContours.size(); ++index) {
		double areaSize = cv::contourArea(colorHandContours[index]);

		if (areaSize > contourFilterArea) {
			outputContours.push_back(colorHandContours[index]);
		}
	}
}

void AsyncKinectProcessor::_processMaskContours(cv::Mat& colorMask, const std::vector<std::vector<cv::Point> >& contours) {
	colorMask = cv::Mat(480, 640, CV_8UC1, cv::Scalar(0));
	cv::drawContours(colorMask, contours, -1, cv::Scalar(255), CV_FILLED);
}

void AsyncKinectProcessor::_processTransformColorToDepth(const cv::Mat& color, const cv::Mat& depth, const cv::Mat& mask, cv::Mat& registeredColor) {
	registeredColor = cv::Mat(480, 640, CV_8UC3, cv::Scalar(0));

	unsigned short* depthValues = reinterpret_cast<unsigned short*>(depth.data);

	_kinect->sensorHandle()->NuiImageGetColorPixelCoordinateFrameFromDepthPixelFrameAtResolution(
		NUI_IMAGE_RESOLUTION_640x480,
		NUI_IMAGE_RESOLUTION_640x480,
		640 * 480,
		depthValues,
		640 * 480 * 2,
		_colorCoordinates
		);
		
	unsigned char* pixelSrcPtr = (unsigned char*)color.data;
	unsigned char* pixelDstPtr = (unsigned char*)registeredColor.data;

	for (int y = 0; y < 480; ++y) {
		for (int x = 0; x < 640; ++x) {		
			// Calculate index into depth array
			int depthIndex = x + y * 640;

			// Discard zero depth pixels => Leads to holes in hand
			const unsigned short depthValue = depthValues[depthIndex];

			if (depthValue == 0) {
				for (int channel = 0; channel < 3; ++channel) {
					pixelDstPtr[y * 640 * 3 + x * 3 + channel] = 255;
				}
				continue;
			}

			// Retrieve the depth to color mapping for the current depth pixel
			LONG colorInDepthX = _colorCoordinates[depthIndex * 2];
			LONG colorInDepthY = _colorCoordinates[depthIndex * 2 + 1];

			// Only process if in mask (i.e. current mask pixel has not value 0)
			const unsigned char maskValue = mask.at<unsigned char>(colorInDepthY, colorInDepthX);
			if (maskValue == 0) {
				for (int channel = 0; channel < 3; ++channel) {
					pixelDstPtr[y * 640 * 3 + x * 3 + channel] = 255;
				}
				continue;
			}
				
			// Make sure the depth pixel maps to a valid point in color space
			if (colorInDepthX >= 0 && colorInDepthX < 640 && colorInDepthY >= 0 && colorInDepthY < 480 ) {
				for (int channel = 0; channel < 3; ++channel) {
					pixelDstPtr[y * 640 * 3 + x * 3 + channel] = pixelSrcPtr[colorInDepthY * 640 * 3 + colorInDepthX * 3 + channel];
				}
			}
			else {
				for (int channel = 0; channel < 3; ++channel) {
					pixelDstPtr[y * 640 * 3 + x * 3 + channel] = 255;
				}
			}
		}
	}
}

void AsyncKinectProcessor::_processMaskDepth(cv::Mat& depthFrame, const cv::Mat& registeredColor, cv::Mat& mask) {
	mask.setTo(0);
	cv::cvtColor(registeredColor, mask, CV_RGB2GRAY);
	
	// Threshold gray image, clip all but the hand
	cv::threshold(mask, mask, 200, 255, cv::THRESH_BINARY);

	depthFrame.setTo(0, mask);

	if (_showDebugInformation) {
		cv::imshow("depthFrame", depthFrame);
	}
}

void AsyncKinectProcessor::_processRemoveShadowsFromDepth(cv::Mat& depthFrame, cv::Mat& depthFrame8Bit, const cv::Mat& mask) {
	depthFrame8Bit = cv::Mat(480, 640, CV_8UC1);
	depthFrame.convertTo(depthFrame8Bit, CV_8UC1, 0.00390625);

	// Get minimum and maximum values from 8-bit depth image
	double minValue, maxValue;
	cv::minMaxIdx(depthFrame8Bit, &minValue, &maxValue, 0, 0, 255 - mask);

	// Create a mask image for shadow fingers
	cv::Mat depthMask = cv::Mat(480, 640, CV_8UC1);
	cv::threshold(depthFrame8Bit, depthMask, (minValue + threshValue), 255, cv::THRESH_BINARY);

	// Use mask on 8 & 16-bit depth image to remove shadow fingers
	depthFrame.setTo(0, depthMask);
	depthFrame8Bit.setTo(0, depthMask);
}

void AsyncKinectProcessor::_processCalibration(const cv::Mat& depthFrame) {
	if (_calibrationRunning) {
		_calibrationFingerFound = _findFinger(_calibrationFinger, &depthFrame);
	}


	//if (_calibrationRunning) {
	//	//Update calibration timer
	//	_kinectCalibrationMetaphor->update(deltaTime);//(evt.timeSinceLastFrame);

	//	// Find fingers
	//	cv::Point3f finger;

	//	bool found  = _findFinger(finger, &depthFrame);

	//	bool holding = false;

	//	// Check if holding
	//	const double EPS = 1.0;

	//	if (found) {
	//		// Distance from current point to last point
	//		const double distance = sqrt( 
	//			(finger.x - _oldPoint.x) * (finger.x - _oldPoint.x) +
	//			(finger.y - _oldPoint.y) * (finger.y - _oldPoint.y) +
	//			(finger.z - _oldPoint.z) * (finger.z - _oldPoint.z)
	//			);

	//		if (distance < EPS) {
	//			holding = true;
	//		}
	//	}


	//	// Update calibration
	//	if (found) {
	//		_kinectCalibrationMetaphor->updateCalibration(finger, holding);
	//		cv::Point3f transformedFinger;
	//		_kinectCalibrationMetaphor->transform(finger, transformedFinger);
	//		_oldPoint = finger;
	//	}
	//	if (_kinectCalibrationMetaphor->isCali() == false) {
	//		_calibrationRunning = false;
	//	}
	//}
}

void AsyncKinectProcessor::_processCutoffArm(cv::Mat& depthFrame, const cv::Mat& depthFrame8Bit) {
	// Detect contours
	std::vector< std::vector< cv::Point > > depthHandContours;
	cv::findContours(depthFrame8Bit.clone(), depthHandContours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);
		
	std::vector<std::vector<cv::Point> > contours_poly( depthHandContours.size() );
	std::vector<cv::Rect> boundRect( depthHandContours.size() );

	for (size_t i = 0; i < depthHandContours.size(); ++i) { 
		cv::approxPolyDP( cv::Mat(depthHandContours[i]), contours_poly[i], 10, true );
		boundRect[i] = cv::boundingRect( cv::Mat(contours_poly[i]) );
	}

	cv::Mat debugImage = cv::Mat(480, 640, CV_8UC3, cv::Scalar(0));

	for (size_t i = 0; i < depthHandContours.size(); ++i) { 
		cv::RotatedRect rRect = cv::minAreaRect(depthHandContours[i]);

		// Half width and height of rotated fitting rectangle
		const double width_h  = rRect.size.width  / 2.0;
		const double height_h = rRect.size.height / 2.0;

		// Rotation angle
		Ogre::Radian theta = Ogre::Degree(rRect.angle);
			
		// Position offset
		Ogre::Vector3 offset = Ogre::Vector3(rRect.center.x, rRect.center.y, 0);

		// Coordinates of unrotated rectangle
		Ogre::Vector3 topLeft     = Ogre::Vector3(-width_h,  height_h, 0);
		Ogre::Vector3 topRight    = Ogre::Vector3( width_h,  height_h, 0);
		Ogre::Vector3 bottomLeft  = Ogre::Vector3(-width_h, -height_h, 0);
		Ogre::Vector3 bottomRight = Ogre::Vector3( width_h, -height_h, 0);

		// Ration for cutting of hand
		const double handCutoffRatio = 1.0;// 1.10;
			
		// Cutoff to approx. the hand (take care of order to cutoff correct dimension)
		// Note: Result encloses the rest of the arm, without the hand
		if (width_h > height_h) {
			const int cutoff = rRect.size.height * handCutoffRatio;

			topRight.x    -= cutoff;
			bottomRight.x -= cutoff;	
		}
		else {
			const int cutoff = rRect.size.width * handCutoffRatio;

			bottomLeft.y  += cutoff;
			bottomRight.y += cutoff;
		}

		// Rotation matrix
		Ogre::Matrix3 rotate(Ogre::Math::Cos(theta), -Ogre::Math::Sin(theta), 0,
								Ogre::Math::Sin(theta), Ogre::Math::Cos(theta),  0,
								0,                      0,                       1);

		// Rotate and translate points to correct position
		Ogre::Vector3 topLeftRotated     = rotate * topLeft     + offset;
		Ogre::Vector3 topRightRotated    = rotate * topRight    + offset;
		Ogre::Vector3 bottomLeftRotated  = rotate * bottomLeft  + offset;
		Ogre::Vector3 bottomRightRotated = rotate * bottomRight + offset;


		cv::line( debugImage, cv::Point2f(topLeftRotated.x,     topLeftRotated.y),     cv::Point2f(topRightRotated.x,    topRightRotated.y),    cv::Scalar(255,   255,   0), 1);
		cv::line( debugImage, cv::Point2f(topRightRotated.x,    topRightRotated.y),    cv::Point2f(bottomRightRotated.x, bottomRightRotated.y), cv::Scalar(255,   255,   0), 1);
		cv::line( debugImage, cv::Point2f(bottomRightRotated.x, bottomRightRotated.y), cv::Point2f(bottomLeftRotated.x,  bottomLeftRotated.y),  cv::Scalar(255,   255,   0), 1);
		cv::line( debugImage, cv::Point2f(bottomLeftRotated.x,  bottomLeftRotated.y),  cv::Point2f(topLeftRotated.x,     topLeftRotated.y),     cv::Scalar(255,   255,   0), 1);
			
		// Collection of points to create a polygon
		cv::Point polygon_points[1][5];
		polygon_points[0][0] = Point( topLeftRotated.x,     topLeftRotated.y);
		polygon_points[0][1] = Point( topRightRotated.x,    topRightRotated.y);
		polygon_points[0][2] = Point( bottomRightRotated.x, bottomRightRotated.y);
		polygon_points[0][3] = Point( bottomLeftRotated.x,  bottomLeftRotated.y);
 		polygon_points[0][4] = Point( topLeftRotated.x,     topLeftRotated.y);

		const Point* ppt[1] = { polygon_points[0] };
		int npt[] = { 5 };
 
		// Draw polygon on depth image to mask out the arm without the hand
		cv::fillPoly(depthFrame, ppt, npt, 1, cv::Scalar( 0 ) );
	}
}

void AsyncKinectProcessor::_processHandDetection(const cv::Mat& depthFrame) {
	cv::Mat depth8 = cv::Mat(480, 640, CV_8UC1, cv::Scalar(0));
	depthFrame.convertTo(depth8, CV_8UC1, 0.00390625);
	std::vector< std::vector< cv::Point > > depthHandContours;

	cv::findContours(depth8, depthHandContours, CV_RETR_EXTERNAL, CV_CHAIN_APPROX_SIMPLE);

	if(depthHandContours.size() > 0) {
		cv::Mat drawing = Mat::zeros(depth8.size(), CV_8UC3);

		std::vector<std::vector<int> > hull(depthHandContours.size());
		std::vector<std::vector<cv::Vec4i>> convDef(depthHandContours.size());
		std::vector<std::vector<cv::Point>> hull_points(depthHandContours.size());
		std::vector<std::vector<cv::Point>> defect_points(depthHandContours.size());

		// Count hits
		size_t hits = 0;

		for (size_t index = 0; index < depthHandContours.size(); ++index) {
			double areaSize = cv::contourArea(depthHandContours[index]);

			if (areaSize > 1000) {
				hits++;

				cv::convexHull(depthHandContours[index], hull[index], false);
				cv::convexityDefects(depthHandContours[index], hull[index], convDef[index]);

				for(size_t k = 0; k < hull[index].size(); ++k) {           
					int ind = hull[index][k];
					hull_points[index].push_back(depthHandContours[index][ind]);
				}
					
				// NOTE: Alex: See http://docs.opencv.org/modules/imgproc/doc/structural_analysis_and_shape_descriptors.html
				for(size_t k = 0; k < convDef[index].size(); ++k) {       
					// Distance of farthest defect point to original contour
					double farthestDefectDistance = convDef[index][k][3] / 256.0;

					// Only observe defects which are big enough
					if (farthestDefectDistance > 20) { 
						int ind_0 = convDef[index][k][0];
						int ind_1 = convDef[index][k][1];
						int ind_2 = convDef[index][k][2];

						// Push back farthest point
						defect_points[index].push_back(depthHandContours[index][ind_2]);

						//cv::circle(drawing, depthHandContours[index][ind_0],5, cv::Scalar(0,255,0),-1); // Start point
						//cv::circle(drawing, depthHandContours[index][ind_1],5, cv::Scalar(0,255,0),-1); // End Point
						//cv::circle(drawing, depthHandContours[index][ind_2],5, cv::Scalar(0,0,255),-1); // Farthest point of defect

						/*cv::line(drawing, depthHandContours[index][ind_2], depthHandContours[index][ind_0], cv::Scalar(0,0,255),1);
						cv::line(drawing, depthHandContours[index][ind_2], depthHandContours[index][ind_1], cv::Scalar(0,0,255),1);*/
					}
				}

				// Current hand state (open / close)
				bool state = false;
					
				// If true, state changed from last frame
				bool change = false;

				// Calculate bounding circle
				cv::Point2f handCenter;
				float handRadius;
				cv::minEnclosingCircle(depthHandContours[index], handCenter, handRadius);
				cv::circle(drawing, handCenter, handRadius, cv::Scalar(255,255,255),1);

				/*int id = 0;
				for (size_t k = 0; k < depthHandContours[index].size(); ++k) {
					if (depthHandContours[index][k].y > depthHandContours[index][id].y) {
						id = k;
					}
				}

				cv::Point2f point = depthHandContours[index][id];

				handCenter = point;
				handCenter.y -= handRadius / 2.0f;*/

				// Detect hand open / close by convexity defects
				for (size_t j = 0; j < defect_points.size(); ++j) {
					// Only observe contours with minimum amount of convexity defects
					if (defect_points[j].size() > 1) {
						// Calculate minimum vertical point on bounding circle
						//cv::Point2f adjustedHandPosition = handCenter;
						//handCenter.y += handRadius;
						handCenter.y += handRadius / 3.0f; // Adjust position for open hand, leave center for closed hand

						// Collect points for approximating bounding circle of hand without fingers
						/*std::vector<cv::Point2f> points;

						points.push_back(adjustedHandPosition);

						for (size_t k = 0; k < defect_points[j].size(); ++k) {
							points.push_back(defect_points[j][k]);
						}*/

						//cv::minEnclosingCircle(points, handCenter, handRadius);
						//cv::circle(drawing, adjustedHandPosition, 10, cv::Scalar(255,255,255),1);
							
						state = true;
					} 
				}

				// Check if state changed from last frame (and if not already triggered the timer filter)
				if (!_timerFilter && (_open && !state || !_open && state)) {
					// Calculate distance between current and last hand position
					cv::Point2f newestHandPosition;
						
					if (_lastHandPositionScreen.size() > 0) {
						newestHandPosition = _lastHandPositionScreen.front();
					}
					else {
						newestHandPosition = handCenter;
					}

					Ogre::Vector2 a, b;
					a = Ogre::Vector2(handCenter.x,         handCenter.y);
					b = Ogre::Vector2(newestHandPosition.x, newestHandPosition.y);

					double distance = a.squaredDistance(b);

					// If distance is long enough, the hand is moving fast
					if (distance > 50) {
						// Block state change with this speed, otherwise moving open hand is detected as closed
						state = _open;
						change = false;
						//_timerFilter = false;

						//std::cout << "-- DETECTION BLOCKED ( " << distance << " ) --" << std::endl;
					}
					else {
						// Start state change and start timer to reduce sudden offset
						change = true;
						//_timerFilter = true;
						//mTimer.reset();
							
						//std::cout << "PROBABLY CHANGE TO: ";

						//if (_open) {
						//	std::cout << "CLOSE ( " << distance << " ) " << std::endl;
						//}
						//else {
						//	std::cout << "OPEN ( " << distance << " ) " << std::endl;
						//}
					}
				}

				double distance = 0.0;

				// If currently hand is opening / closing take special care of current position and filtering for a short duration
				//if (_timerFilter) {
				//	// Do this special treatment only for a short time period
				//	if (mTimer.getMilliseconds() > 200) {
				//		_timerFilter = false;
				//	}

				//	// Get oldest hand position
				//	cv::Point2f oldestHandPosition;
				//		
				//	if (_lastHandPositionScreen.size() > 0) {
				//		oldestHandPosition = _lastHandPositionScreen.back();
				//	}
				//	else {
				//		oldestHandPosition = handCenter;
				//	}
				//		
				//	handCenter = oldestHandPosition;

				//	//Ogre::Vector2 a, b;
				//	//a = Ogre::Vector2(handCenter.x, handCenter.y);
				//	//b = Ogre::Vector2(oldestHandPosition.x, oldestHandPosition.y);

				//	//distance = a.squaredDistance(b);

				//	////std::cout << "FILTERED DIST: " << distance << std::endl;

				//	//float q = 0.3f;
				//	//handCenter = oldestHandPosition * (1 - q) + handCenter * q;
				//		

				//	//_lastHandPositionScreen = handCenter;

				//		
				//}
				//else {
				//	cv::Point2f newestHandPosition;
				//		
				//	if (_lastHandPositionScreen.size() > 0) {
				//		newestHandPosition = _lastHandPositionScreen.front();
				//	}
				//	else {
				//		newestHandPosition = handCenter;
				//	}

				//	//Ogre::Vector2 a, b;
				//	//a = Ogre::Vector2(handCenter.x, handCenter.y);
				//	//b = Ogre::Vector2(newestHandPosition.x, newestHandPosition.y);

				//	//distance = a.squaredDistance(b);

				//	////std::cout << "DIST: " << distance << std::endl;

				//	//// Hand is holding still => Strong filter
				//	//if (distance < 50) {
				//	//	float q = 0.1f;
				//	//	handCenter = newestHandPosition * (1 - q) + handCenter * q;
				//	//}
				//	//// Hand is moving really fast => no filter and block opening / closing
				//	//else if (distance > 100) {
				//	//	float q = 1.0f;
				//	//	handCenter = newestHandPosition * (1 - q) + handCenter * q;
				//	//}
				//	//// Hand is moving fast => Weak filter
				//	//else if (distance > 50) {
				//	//	float q = 0.8f;
				//	//	handCenter = newestHandPosition * (1 - q) + handCenter * q;
				//	//}

					/*_lastHandPositionScreen.push_front(handCenter);*/
				//}

				// --------------------------------------------------------------------------
				// Debug Stuff
				// --------------------------------------------------------------------------

				//static cv::Point2f lastHandOpenPosition  = handCenter;
				//static cv::Point2f lastHandClosePosition = handCenter;

				//cv::circle(drawing, lastHandOpenPosition,  3, cv::Scalar(0, 0, 255),   2);
				//cv::circle(drawing, lastHandClosePosition, 3, cv::Scalar(0, 255, 255), 2);

				//if (change) {
				//	if (state) {
				//		//std::cout << "OPENED: " << handCenter.x << ", " << handCenter.y << std::endl;
				//		lastHandOpenPosition = handCenter;
				//	}
				//	else {
				//		//std::cout << "CLOSED: " << handCenter.x << ", " << handCenter.y << std::endl;
				//		lastHandClosePosition = handCenter;
				//	}
				//}

				
				
				
				/*cv::drawContours( drawing, depthHandContours, index, cv::Scalar(0,255,0), 1, 8, std::vector<Vec4i>(), 0, cv::Point() );
				cv::drawContours( drawing, hull_points, index, cv::Scalar(255,0,0), 1, 8, std::vector<Vec4i>(), 0, cv::Point() );*/

				//Ogre::Vector3 pos = km->handPos(handCenter, depthFrame.at<unsigned short>(handCenter.y, handCenter.x));

				// Avoid zero pixels in depth image
				const unsigned short depthValue = depthFrame.at<unsigned short>(handCenter.y, handCenter.x);

				// Translate to tracking center
				//const Ogre::Vector3 trackingCenter = Ogre::Vector3(7.5, -10.5, 0);

				Ogre::Vector3 pos;

				if (depthValue != 0) {
					pos = _transformHandPosition(handCenter, depthFrame.at<unsigned short>(handCenter.y, handCenter.x));
					//pos.z -= 55;

					//pos -= trackingCenter;

					//// Scale to display
					//pos.x *= 1.6f;
					//pos.y *= 3.517f;

					//// Translate back to display center
					//pos += trackingCenter;
				}
				else {
					pos = _currentHandPosition;
					//std::cout << "ZERO PIXEL HIT" << std::endl;
				}

				//std::cout << "POS: " << pos.x << ", " << pos.y << ", " << pos.z << std::endl;
				
				// --------------------------------------------------------------------------


				float lowPassFilter = 1.0f;

				const double FINGER_MOVE_THRESH = 2.0f;
				const double squaredDistance = pos.squaredDistance(_oldHandPos);

				const double bottomThresh = 0.05;
				const double topThresh    = 0.7;
				// ------------------------------------------------------------------
				const double filterLength = topThresh - bottomThresh;

				double dynamicFilter = squaredDistance;
				dynamicFilter -= FINGER_MOVE_THRESH;
				dynamicFilter /= FINGER_MOVE_THRESH;
				dynamicFilter *= filterLength;
				dynamicFilter += bottomThresh;

				if (dynamicFilter < bottomThresh) {
					lowPassFilter = bottomThresh;
					//std::cout << "HIGH FILTER" << std::endl;
				}
				else if (dynamicFilter >= bottomThresh && dynamicFilter <= topThresh) {
					lowPassFilter = dynamicFilter;
					//std::cout << "DYNAMIC FILTER" << std::endl;
				}
				else {
					lowPassFilter = topThresh;
					//std::cout << "LOW FILTER" << std::endl;
				}

				// ------------------------------------------------------------------
				//std::cout << "DIST: " << squaredDistance <<  "  Filter: " << lowPassFilter << std::endl;

				/*if (squaredDistance < FINGER_MOVE_THRESH) {						
					lowPassFilter = 0.2f;
				} 
				else if (squaredDistance < 2 * FINGER_MOVE_THRESH) {					
					lowPassFilter = 0.5f;
				}
				else {					
					lowPassFilter = 0.9f;
				}*/

				

				

				

				pos = _oldHandPos * (1 - lowPassFilter) + pos * lowPassFilter;

				

				//std::cout << "POS: " << pos.x << ", " << pos.y << ", " << pos.z << std::endl;

				//km->handMove(pos, _open);

				/*_currentHandPosition = pos;
				_oldHandPos = _currentHandPosition;*/

				
				// --------------------------------------------------------------------------

				// Set current state to detected state
				if (change && !_delayHandTrigger) {
					_delayHandTrigger = true;
					mTimer2.reset();
					//std::cout << "BLOCK" << std::endl;
				}


				if (_delayHandTrigger && mTimer2.getMilliseconds() > 200) { // was 200; 300
					_delayHandTrigger = false;
					//std::cout << "RELEASED" << std::endl;
					/*_open = state;*/
				}
				else if (_delayHandTrigger) {
					//_currentHandPosition = _oldHandPos;

					if (_oldHandPositions.size() > 0) {
						_currentHandPosition = _oldHandPositions.front();
					}
					else {
						_currentHandPosition = _oldHandPos;
					}
				}

				if (!_delayHandTrigger) {
					_currentHandPosition = pos;
					_oldHandPos = _currentHandPosition;

					// Manage history of 3d hand positions
					_oldHandPositions.push_back(_currentHandPosition);
					while (_oldHandPositions.size() > 10) {
						_oldHandPositions.pop_front();
					}
				}

				_open = state;

				
				// Remove old positions to avoid overflow
				_lastHandPositionScreen.push_front(handCenter);
				while (_lastHandPositionScreen.size() > 10) {
					_lastHandPositionScreen.pop_back();
				}

				// --------------------------------------------------------------------------
				// Debug Stuff
				// --------------------------------------------------------------------------
				cv::Scalar handColor;

				if (_open) {
					handColor = cv::Scalar( 0,   0,  255);
				} 
				else {
					handColor = cv::Scalar(  0, 255,  0);
				}

				cv::circle(drawing, handCenter,           10,         handColor, -1);
				//cv::circle(drawing, handCenter,           handRadius, Scalar(255, 255, 255));
				cv::drawContours(drawing, depthHandContours, index, cv::Scalar(0,255,0), 1, 8, std::vector<Vec4i>(), 0, cv::Point() );

				

				
			}
		}

		if (hits > 1) {
			std::cout << "More then one hit found (" << hits << ") !" << std::endl;
		}

		if (_showDebugInformation) {
			//cv::drawContours( drawing, depthHandContours, -1, cv::Scalar(0,255,0), 1, 8, std::vector<Vec4i>(), 0, cv::Point() );
			cv::imshow("drawing", drawing);
			cv::waitKey(1);
		}
	}
}

void AsyncKinectProcessor::_processDepthShadowTransformation(const cv::Mat& depthFrame8Bit) {
	_mutex.lock();

	//_depth8Bit = depthFrame8Bit.clone();
	cv::flip(depthFrame8Bit, _depth8Bit, 1);

	_mutex.unlock();
}

void AsyncKinectProcessor::_addKinectMaterial(const Ogre::String& name, Ogre::Frustum* projectiveFrustum) {
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

	pass->setName("ShadowPass");

	// some settings
	pass->setLightingEnabled(false);
	pass->setDepthBias(1);
	pass->setSceneBlending(Ogre::SBT_TRANSPARENT_ALPHA);

	// add kinect-shadow
	Ogre::TextureUnitState* texState = pass->createTextureUnitState("KinectShadow");
	texState->setProjectiveTexturing(true, projectiveFrustum);
	texState->setTextureAddressingMode(Ogre::TextureUnitState::TAM_BORDER);
	texState->setTextureBorderColour(Ogre::ColourValue(0.0f, 0.0f, 0.0f, 0.0f));
	texState->setTextureFiltering(Ogre::FO_POINT, Ogre::FO_LINEAR, Ogre::FO_NONE);
}
