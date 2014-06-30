#include <vector>

#include <OgreDepthBuffer.h>
#include "OgreBulletDynamicsRigidBody.h"

#include "globals.h"

#include "multitouch_shadows.h"

#include "HeadTrackingMetaphor.h"

#include "experiment.h"
#include "ShadowVolumeManager.h"

MultitouchShadows::MultitouchShadows(Ogre::SceneManager* sceneManager, Experiment* experiment, Ogre::RenderSystem* renderSystem) 
	:Metaphor("MultitouchShadowsMetaphor")
	,_sceneMgr(sceneManager)
	,_experiment(experiment)
	,_renderSystem(renderSystem)
	,_active(false)
	,_shadowTextureResolution(1024, 768) /// Resolution of the shadow render target texture
	,_touchMode(TouchMode::Waiting)
	,_grabbedObjectNode(0)
	,_grabbedRigidBody(0)
	,_lightPlaneHeight(25.0f)
{
	_initialize();

	_shadowVolumeMgr = new ShadowVolumeManager(sceneManager, experiment, _lightPlaneHeight);
	experiment->assignShadowVolumeManager(_shadowVolumeMgr);
}

MultitouchShadows::~MultitouchShadows() {
	// Remove listener
	//_shadowRenderTexture->removeListener(this);

	delete _shadowVolumeMgr;
}

void MultitouchShadows::toggleProcessing(bool active) {
	_active = active;
	
	_surfacePlaneNode->setVisible(_active);

	if (_active) {
		_sceneMgr->setShadowTechnique(Ogre::SHADOWTYPE_NONE);
	}
	else {
		_sceneMgr->setShadowTechnique(Ogre::SHADOWTYPE_STENCIL_ADDITIVE);
	}

	/*if (_active) {
		_navigationPanelOverlay->show();
		_navigationMarkerOverlayContainer->show();
		_navigationPanelOverlayContainer->show();
	}
	else {
		_navigationPanelOverlay->hide();
		_navigationMarkerOverlayContainer->hide();
		_navigationPanelOverlayContainer->hide();
	}*/
	_shadowVolumeMgr->toggleVisibility(active);
}

void MultitouchShadows::update(float timeSinceLastUpdate) {
	// Leave in inactive state
	if (!_active) {
		return;
	}

	// Update line links between shadows and objects
	//const Ogre::Matrix4 viewMatrix     = _shadowCasterCamera->getViewMatrix();
	//const Ogre::Matrix4 projMatrix     = _shadowCasterCamera->getProjectionMatrix();
	//const Ogre::Matrix4 viewProjMatrix = projMatrix * viewMatrix;

	//// Iterate over all objects and compute the line start and end points
	//for (size_t index = 0; index < 4; ++index) {
	//	// Construct scene node names
	//	std::stringstream sstream1, sstream2;
	//	sstream1 << "dynamic_box" <<  (index + 1) << "_node";
	//	sstream2 << "dynamic_line" << (index + 1) << "_node"; 

	//	// Obtain scene nodes
	//	Ogre::SceneNode* boxNode  = _sceneMgr->getSceneNode(sstream1.str());
	//	Ogre::SceneNode* lineNode = _sceneMgr->getSceneNode(sstream2.str());

	//	// Extract line from node
	//	DynamicLines* line = static_cast<DynamicLines*>(lineNode->getAttachedObject(0));

	//	// Compute Model Matrix
	//	Ogre::Matrix4 modelMatrix = Ogre::Matrix4(boxNode->getOrientation());
	//	modelMatrix.setScale(boxNode->getScale());
	//	modelMatrix.setTrans(boxNode->getPosition());
	//	
	//	// Compute MVP Matrix
	//	Ogre::Matrix4 MVP = viewProjMatrix * modelMatrix;

	//	// Get projected point
	//	const Ogre::Vector4 scenePosition = Ogre::Vector4(0.0f, 0.0f, 0.0f, 1.0f); // Position etc. already encoded in model matrix => use object center as position
	//	Ogre::Vector4 projectedPoint = MVP * scenePosition;
	//	projectedPoint /= projectedPoint.w;

	//	// Flip horizontally
	//	//projectedPoint.x *= -1.0f;

	//	// Get point on screen surface screen in Ogre3D world coordinates
	//	const Ogre::Vector3 surfacePoint = Ogre::Vector3(projectedPoint.x * global::tableWidth / 2.0f, projectedPoint.y * global::tableHeight / 2.0f, 0.0f);

	//	// (Re)adjust the line to point from object center to the center of the projection on the surface
	//	line->clear();
	//	line->addPoint(boxNode->getPosition().x, boxNode->getPosition().y, boxNode->getPosition().z);
	//	line->addPoint(surfacePoint);
	//	line->update();
	//}

	//viargo::vec3f leftEye = dynamic_cast<HeadTrackingMetaphor&>(Viargo.metaphor("HeadTracking")).leftEyePos();
	//viargo::vec3f rightEye = dynamic_cast<HeadTrackingMetaphor&>(Viargo.metaphor("HeadTracking")).rightEyePos();

	//viargo::vec3f center = (leftEye + rightEye) * 0.5f;
	//		
	//// Assign camera position
	//Ogre::Vector3 cameraPosition = Ogre::Vector3(center.x, center.y, center.z);
	//		
	//_shadowCasterCamera->setPosition(cameraPosition);
	////_shadowCasterCamera->lookAt(Ogre::Vector3(0.0f, 0.0f, -25.0f));
	//_shadowCasterCamera->lookAt(Ogre::Vector3(cameraPosition.x, cameraPosition.y, 0.0f));

	//_calculateFrustum(_shadowCasterCamera, cameraPosition);

	_shadowVolumeMgr->update();
}

void MultitouchShadows::_initialize() {
	 // Create the camera
	//Ogre::Vector3 cameraPosition = Ogre::Vector3(0.0f, 0.0f,  _lightPlaneHeight);
	////Ogre::Vector3 lookAtPosition = Ogre::Vector3(0.0f, 0.0f, -50.0f); // Test value
	////Ogre::Vector3 cameraPosition = Ogre::Vector3(0.0f, 0.0f, -75.0f);
	////Ogre::Vector3 lookAtPosition = Ogre::Vector3(0.0f, 0.0f, -25.0f); // Test value
	//Ogre::Vector3 lookAtPosition = Ogre::Vector3(cameraPosition.x, cameraPosition.y, 0.0f);
	//
 //   _shadowCasterCamera = _sceneMgr->createCamera("ShadowCasterCamera");
 //   _shadowCasterCamera->setPosition(cameraPosition);
	//_shadowCasterCamera->lookAt(lookAtPosition);
 //   _shadowCasterCamera->setNearClipDistance(1.0f);
	//_shadowCasterCamera->setFarClipDistance(250.0f);  

	//// Calculate and assign projection frustum
	////_calculateFrustum(_shadowCasterCamera, cameraPosition);
	//_shadowCasterCamera->setProjectionType(Ogre::PT_ORTHOGRAPHIC);
	//_shadowCasterCamera->setFrustumExtents(-global::tableWidth / 2.0f, global::tableWidth / 2.0f, global::tableHeight / 2.0f, -global::tableHeight / 2.0f);

	//// Create the RTT texture
	//Ogre::TexturePtr rtt_texture = Ogre::TextureManager::getSingleton().createManual("MultitouchShadowTexture", 
	//			Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME, 
	//			Ogre::TEX_TYPE_2D, 
	//			_shadowTextureResolution.x,
	//			_shadowTextureResolution.y, 
	//			0, 
	//			Ogre::PF_R8G8B8A8, 
	//			//Ogre::PF_BYTE_BGR, 
	//			//Ogre::PF_FLOAT16_R,
	//			Ogre::TU_RENDERTARGET);

	//_shadowRenderTexture = rtt_texture->getBuffer()->getRenderTarget();
	//
	//// Add the camera to the render target
	//Ogre::Viewport* viewport = _shadowRenderTexture->addViewport(_shadowCasterCamera);
	////viewport->setDimensions(0.0f, 0.0f, _shadowTextureResolution.x, _shadowTextureResolution.y);

	//// Register listener
	//_shadowRenderTexture->addListener(this);

	//_shadowRenderTexture->getViewport(0)->setClearEveryFrame(true);
	//_shadowRenderTexture->getViewport(0)->setBackgroundColour(Ogre::ColourValue(0.0f, 0.0f, 0.0f, 0.0f));
	////_shadowRenderTexture->getViewport(0)->setDepthClear(-9999.0f); // Camera Bottom
	//_shadowRenderTexture->getViewport(0)->setOverlaysEnabled(false);
	//_shadowRenderTexture->getViewport(0)->setShadowsEnabled(false);
	//_shadowRenderTexture->getViewport(0)->setMaterialScheme("depthSurfaceRendering");
	//_shadowRenderTexture->setAutoUpdated(true);

	// Create translucent surface plane
	Ogre::Plane plane(Ogre::Vector3::UNIT_Z, 0.0f);
    Ogre::MeshManager::getSingleton().createPlane("static_plane_surface_mesh",
                                            Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME,											
											plane, global::tableWidth, 
											global::tableHeight, 
											1, 
											1, 
											true, 
											1, 
											1, 
											1, 
											Ogre::Vector3::UNIT_Y);

    Ogre::Entity* surfacePlaneEntity = _sceneMgr->createEntity("static_plane_surface_entity", "static_plane_surface_mesh");
	surfacePlaneEntity->setMaterialName("MultitouchShadow");
	_surfacePlaneNode = _sceneMgr->getRootSceneNode()->createChildSceneNode("static_plane_surface_node");
	_surfacePlaneNode->attachObject(surfacePlaneEntity);
	_surfacePlaneNode->setPosition(Ogre::Vector3(0.0f, 0.0f, 0.0f));
	_surfacePlaneNode->setVisible(false);

	// Load material for depth rendering
	//Ogre::MaterialManager::getSingleton().load("DepthMap", Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME);
	//_depthMaterial = Ogre::MaterialManager::getSingleton().getByName("DepthMap");//Ogre::MaterialManager::getSingleton().load("DepthMap", Ogre::ResourceGroupManager::DEFAULT_RESOURCE_GROUP_NAME);

	// Initialize lines as linkage between objects and shadows
	/*DynamicLines* line = 0;

	for (size_t index = 0; index < 4; ++index) {
		std::stringstream sstream;
		sstream << "dynamic_line" << (index + 1) << "_node";

		line = new DynamicLines(Ogre::RenderOperation::OT_LINE_LIST);
		line->setMaterial("Examples/Black");
		line->update();
		_sceneMgr->getRootSceneNode()->createChildSceneNode(sstream.str())->attachObject(line);
	}*/

	// Intialize multitouch properties
	//_touchIDs[0] = -1;
	//_touchIDs[1] = -1;

	//// Initialize overlays for navigation panel
	//Ogre::OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();

	//// Main overlay, specific overlay containers are inserted here
	//_navigationPanelOverlay = overlayManager.create("NavigationPanelOverlay");
	//_navigationPanelOverlay->hide();

	// Constant sizes and positions for navigation panel
	//const Ogre::Vector2 overlaySize       = Ogre::Vector2(0.1f,  0.3f);
	//const Ogre::Vector2 overlayEdgeOffset = Ogre::Vector2(0.01f, 0.01f);
	//const Ogre::Vector2 overlayPosition   = Ogre::Vector2(0.0f + overlayEdgeOffset.x,  1.0f - overlaySize.y - overlayEdgeOffset.y); // Bottom left corner as position

	//// Assign navigation panel dimensions for multitouch detection
	//_navigationPanelBottomLeft = overlayPosition;
	//_navigationPanelTopRight   = overlayPosition + overlaySize;

	//// Constant sizes and positions for navigation marker
	//const Ogre::Vector2 overlayMarkerSize     = Ogre::Vector2(0.075f, 0.075f);
	//const Ogre::Vector2 overlayMarkerPosition = overlayPosition + overlaySize * 0.5f - overlayMarkerSize * 0.5f;

	//// Assign size for multitouch interaction
	//_navigationMarkerSize = overlayMarkerSize;

	//// Create navigation panel
	//_navigationPanelOverlayContainer = static_cast<Ogre::OverlayContainer*>(overlayManager.createOverlayElement("Panel", "NavigationPanelOverlayContainer"));
	//_navigationPanelOverlayContainer->setPosition(overlayPosition.x, overlayPosition.y);
	//_navigationPanelOverlayContainer->setDimensions(overlaySize.x, overlaySize.y);
	//_navigationPanelOverlayContainer->setMaterialName("navigation_panel");
	//_navigationPanelOverlayContainer->hide();

	//// Create navigation marker
	//_navigationMarkerOverlayContainer = static_cast<Ogre::OverlayContainer*>(overlayManager.createOverlayElement("Panel", "NavigationMarkerOverlayContainer"));
	//_navigationMarkerOverlayContainer->setPosition(overlayMarkerPosition.x, overlayMarkerPosition.y);
	//_navigationMarkerOverlayContainer->setDimensions(overlayMarkerSize.x, overlayMarkerSize.y);
	//_navigationMarkerOverlayContainer->setMaterialName("navigation_marker");
	//_navigationMarkerOverlayContainer->hide();

	//// Add containers to overlay
	//_navigationPanelOverlay->add2D(_navigationPanelOverlayContainer);
	//_navigationPanelOverlay->add2D(_navigationMarkerOverlayContainer);
	//_navigationPanelOverlay->hide();
}

void MultitouchShadows::_calculateFrustum(Ogre::Camera* camera, const Ogre::Vector3& cameraPosition) {
	// Dimensions of screen surface
	const float width = global::tableWidth;
	const float height = global::tableHeight;

	// Orthogonal distance of the camera to the screen surface
	const float distance = Ogre::Math::Abs(/*-50.0f - */cameraPosition.z);

	// Ratio for intercept theorem
	float ratio = distance / camera->getNearClipDistance();

	// Compute size for focal
	float imageLeft   = (-1.0f * width  / 2.0f) - cameraPosition.x;
	float imageRight  = (        width  / 2.0f) - cameraPosition.x;
	float imageTop    = (        height / 2.0f) - cameraPosition.y;
	float imageBottom = (-1.0f * height / 2.0f) - cameraPosition.y;

	// Intercept theorem
	float nearLeft   = imageLeft   / ratio;
	float nearRight  = imageRight  / ratio;
	float nearTop    = imageTop    / ratio;
	float nearBottom = imageBottom / ratio;

	// Assign properties to camera
	camera->setFrustumExtents(nearLeft, nearRight, nearTop, nearBottom);
}

void MultitouchShadows::preViewportUpdate(Ogre::RenderTargetViewportEvent const & evt) {
	if (!_active) {
		return;
	}

	// Make surface invisible
	_sceneMgr->getSceneNode("static_plane_surface_node")->setVisible(false);

	// Make fishtank invisible
	_sceneMgr->getSceneNode("static_walls_floor_node")->setVisible(false);
	_sceneMgr->getSceneNode("static_walls_back_node")->setVisible(false);
	_sceneMgr->getSceneNode("static_walls_front_node")->setVisible(false);
	_sceneMgr->getSceneNode("static_walls_left_node")->setVisible(false);
	_sceneMgr->getSceneNode("static_walls_right_node")->setVisible(false);

	// Deactivate main light
	_sceneMgr->getLight("MainLight")->setVisible(false);

	// Make lines invisible
	for (size_t index = 0; index < 4; ++index) {
		std::stringstream sstream;
		sstream << "dynamic_line" << (index + 1) << "_node";
		_sceneMgr->getSceneNode(sstream.str())->setVisible(false);
	}

	// Resize objects to fake perspective distortion regarding size of projection
	bool nodeFound = true;
	size_t counter = 1;
	while (nodeFound) {
		// Construct the name
		std::stringstream sstream;
		sstream << "dynamic_box" << counter << "_node";

		// Check if node exists
		if (_sceneMgr->hasSceneNode(sstream.str())) {
			Ogre::SceneNode* node = _sceneMgr->getSceneNode(sstream.str());

			// Old scale factor
			const Ogre::Vector3 oldScale = node->getScale();

			// Store scale in scene node, to be able to reset it after rendering
			node->getUserObjectBindings().setUserAny("original_scale", Ogre::Any(oldScale));

			// Flat object
			//node->getParentSceneNode()->setScale(oldScale.x, oldScale.y, 0.01f);
			//node->getParentSceneNode()->setScale(1.0f, 1.0f, 0.01f);

			// Create ray scene query from ray
			Ogre::Ray ray = Ogre::Ray(Ogre::Vector3(node->getPosition().x, node->getPosition().y, _lightPlaneHeight), Ogre::Vector3(0.0f, 0.0f, -1.0f));
			Ogre::RaySceneQuery* query = _sceneMgr->createRayQuery(ray);
			query->setSortByDistance(true);
	 
			// Execute query
			Ogre::RaySceneQueryResult& result = query->execute();
			Ogre::RaySceneQueryResult::iterator iter = result.begin();

			Ogre::RaySceneQueryResult::iterator correctResult = result.end();

			// Iterate over results
			for ( ; iter != result.end(); ++iter) {
				if (iter->movable) {
					Ogre::MovableObject* movable  = iter->movable;
					Ogre::SceneNode*     hitNode  = static_cast<Ogre::SceneNode*>(movable->getParentNode());
					
					// Check for correct node
					if (hitNode == node) {
						correctResult = iter;
						break;
					}
				}
			}

			if (correctResult != result.end()) {
				// Theorem of Intersection
				const Ogre::Real h  = correctResult->distance;
				const Ogre::Real h1 = Ogre::Math::Abs(_lightPlaneHeight);
				const Ogre::Real h2 = h - h1;
				//const Ogre::Real h2 = Ogre::Math::Abs(node->getPosition().z);
				
				// New scale factor, computed by proportions
				Ogre::Real scaleFactor = h1 / h; //(h1 + h2);

				// FIXME
				/*if (scaleFactor > 1.0f) {
					scaleFactor = 1.0f;
				}*/

				// Assign new scale factor
				node->setScale(scaleFactor * oldScale.x, scaleFactor * oldScale.y, scaleFactor * oldScale.z);
				//node->getParentSceneNode()->setScale(scaleFactor * oldScale.x, scaleFactor * oldScale.y, 0.01f);
				//node->getParentSceneNode()->setScale(scaleFactor, scaleFactor, scaleFactor);
			}
			else {
				std::cout << "Error in shadow ray scene query !" << std::endl;
			}
		}
		else {
			// Break the loop
			nodeFound = false;
		}

		counter++;
	}

	

	//_sceneMgr->_suppressRenderStateChanges(true);
	//
	//_renderSystem->_setViewport(_shadowRenderTexture->getViewport(0));
	//// like said above, _suppressRenderStateChanges(true)
	//// also suppresses the camera matrices
	//_renderSystem->_setProjectionMatrix(_shadowCasterCamera->getProjectionMatrixRS());
	//_renderSystem->_setViewMatrix(_shadowCasterCamera->getViewMatrix(true));

	//// use your depth pass (you could do this manually 
	//// using just the render system, but _setPass()
	//// is easier if you have a built pass already)
	//_sceneMgr->_setPass(_depthMaterial->getBestTechnique()->getPass(0), true, false);
}

void MultitouchShadows::postViewportUpdate(Ogre::RenderTargetViewportEvent const & evt) {
	if (!_active) {
		return;
	}

	//_sceneMgr->_suppressRenderStateChanges(false);

	_sceneMgr->getSceneNode("static_plane_surface_node")->setVisible(true);

	_sceneMgr->getSceneNode("static_walls_floor_node")->setVisible(true);
	_sceneMgr->getSceneNode("static_walls_back_node")->setVisible(true);
	_sceneMgr->getSceneNode("static_walls_front_node")->setVisible(true);
	_sceneMgr->getSceneNode("static_walls_left_node")->setVisible(true);
	_sceneMgr->getSceneNode("static_walls_right_node")->setVisible(true);

	_sceneMgr->getLight("MainLight")->setVisible(true);

	for (size_t index = 0; index < 4; ++index) {
		std::stringstream sstream;
		sstream << "dynamic_line" << (index + 1) << "_node";
		_sceneMgr->getSceneNode(sstream.str())->setVisible(true);
	}

	// Re-resize objects to correct size
	bool nodeFound = true;
	size_t counter = 1;
	while (nodeFound) {
		// Construct the name
		std::stringstream sstream;
		sstream << "dynamic_box" << counter << "_node";

		// Check if node exists
		if (_sceneMgr->hasSceneNode(sstream.str())) {
			Ogre::SceneNode* node = _sceneMgr->getSceneNode(sstream.str());

			// Get temporary stored old scale factor
			const Ogre::Vector3 oldScale = Ogre::any_cast<Ogre::Vector3>(node->getUserObjectBindings().getUserAny("original_scale"));
			
			// Assign original scale
			node->setScale(oldScale);
			//node->setScale(1.0f, 1.0f, 1.0f);
		}
		else {
			// Break the loop
			nodeFound = false;
		}

		counter++;
	}
}

Ogre::Camera* MultitouchShadows::shadowCasterCamera() const {
	return _shadowCasterCamera;
}

static bool zCompare(const Ogre::SceneNode* lop, const Ogre::SceneNode* rop) {
	return (lop->getPosition().z < rop->getPosition().z);
}

Ogre::SceneNode* MultitouchShadows::rayCast(const Ogre::Vector2& normalizedScreenCoordinates) {
	// Create camera ray
	//Ogre::Ray ray = _shadowCasterCamera->getCameraToViewportRay(normalizedScreenCoordinates.x, normalizedScreenCoordinates.y);

	const float nx = (normalizedScreenCoordinates.x * 2.0f - 1.0f) * global::tableWidth  / 2.0f;
	const float ny = (normalizedScreenCoordinates.y * 2.0f - 1.0f) * -1.0f * global::tableHeight / 2.0f;

	Ogre::Ray ray = Ogre::Ray(Ogre::Vector3(nx, ny, 100.0f), Ogre::Vector3(0.0f, 0.0f, -1.0f));

	// Create ray scene query from ray
	Ogre::RaySceneQuery* query = _sceneMgr->createRayQuery(ray);
	query->setSortByDistance(true);
	 
	// Execute query
	Ogre::RaySceneQueryResult& result = query->execute();
	Ogre::RaySceneQueryResult::iterator iter = result.begin();

	// Collection of hit nodes
	std::vector<Ogre::SceneNode*> hitNodes;

	// Iterate over results
	for ( ; iter != result.end(); ++iter) {
		if (iter->movable) {
			Ogre::MovableObject* movable  = iter->movable;
			Ogre::SceneNode*     node     = static_cast<Ogre::SceneNode*>(movable->getParentNode());
			const Ogre::String   nodeName = node->getName();

			const Ogre::String shadowObjectTopPrefix = "shadow_volume_object_top";

			if (movable->getName().substr(0, shadowObjectTopPrefix.length()) == shadowObjectTopPrefix) {
				hitNodes.push_back(node->getParentSceneNode());
			}

			//if (nodeName.substr(0, 11) == "dynamic_box") {
			//	hitNodes.push_back(node);
			//}
		}
	}

	std::sort(hitNodes.begin(), hitNodes.end(), zCompare);

	if (hitNodes.size() > 0) {
		//return hitNodes.back(); // Camera bottom
		return hitNodes.front(); // Camera top
	}
	
	// Return 0 as error value
	return 0;
}

bool MultitouchShadows::onEventReceived(viargo::Event* event) {
	if (typeid(*event) == typeid(viargo::MultiTouchEvent)) {
		return true;
	}

	return false;
}

void MultitouchShadows::handleEvent(viargo::Event* event) {
	if (!_active) {
		return;
	}

	static const bool FLIP_X_AXIS = false;
	static const bool FLIP_Y_AXIS = false;

	// Scale of height adjustment
	static const float touchBallonScale = 1.0f;

	if (typeid(*event) == typeid(viargo::MultiTouchEvent)) {
		viargo::MultiTouchEvent* mt = static_cast<viargo::MultiTouchEvent*>(event);

		// Amount of current active touches
		const size_t touchesSize = mt->touches().size();

		// Array of touches, initialized with null pointers
		const size_t maxTouchesSize = 2;
		std::vector<viargo::Touch*> touches;

		// Collect two touch points
		mt->touches().resetIterator();
		size_t countTouches = 0;
		while (mt->touches().hasNext()) {
			// Current touch
			viargo::Touch* currentTouch = &mt->touches().next();

			touches.push_back(currentTouch);
			
			// Leave after collecting two touch points
			if (++countTouches == maxTouchesSize) {
				break;
			}
		}
		
		// Check for new actions
		TouchMode::Value newModeCandidate = TouchMode::Invalid;

		// Check for navigation panel touch (takes priority over other modes)
		//if (_touchMode == TouchMode::Waiting && touchesSize == 1) {
		//	viargo::Touch* touch = touches.front();

		//	// Start position of touch
		//	viargo::vec3f touchPositionViargo = touch->current.position;
		//	
		//	const Ogre::Vector2 touchPositionOgre   = (Ogre::Vector2(touchPositionViargo.x, -1.0f * touchPositionViargo.y) + Ogre::Vector2(1.0f)) * 0.5f;

		//	// Check for touch in the navigation panel
		//	//const Ogre::Real    navigationPanelRadius   = (_navigationPanelTopRight.x - _navigationPanelBottomLeft.x) / 2.0f - _navigationMarkerSize.x; // !!
		//	//const Ogre::Vector2 navigationPanelCenter   = (_navigationPanelTopRight + _navigationPanelBottomLeft) / 2.0f;
		//	//const Ogre::Real    navigationPanelDistance = navigationPanelCenter.distance(touchPositionOgre);

		//	if (touchPositionOgre.x >= _navigationPanelBottomLeft.x &&
		//		touchPositionOgre.x <= _navigationPanelTopRight.x   &&
		//		touchPositionOgre.y >= _navigationPanelBottomLeft.y &&
		//		touchPositionOgre.y <= _navigationPanelTopRight.y) 
		//	{
		//		newModeCandidate = TouchMode::NavigationPanel;
		//	}

		//	/*if (navigationPanelDistance <= navigationPanelRadius) {
		//		newModeCandidate = TouchMode::NavigationPanel;
		//	}*/
		//}

		// Only process other modes if navigation panel was not touched
		if (newModeCandidate == TouchMode::Invalid) {
			if ((_touchMode == TouchMode::Waiting || _touchMode == TouchMode::ThreeDimensional) && touchesSize == 1) {
				newModeCandidate = TouchMode::TwoDimensional;
			}
			else if ((_touchMode == TouchMode::Waiting || _touchMode == TouchMode::TwoDimensional) && touchesSize == 2) {
				newModeCandidate = TouchMode::ThreeDimensional;
			}
			else if ((_touchMode == TouchMode::TwoDimensional || _touchMode == TouchMode::ThreeDimensional || _touchMode == TouchMode::NavigationPanel) && touchesSize == 0) {
				newModeCandidate = TouchMode::Waiting;
			}
		}
		
		/*switch (newModeCandidate) { 
			case TouchMode::Waiting: { std::cout << "Waiting" << std::endl; break; }
			case TouchMode::TwoDimensional: { std::cout << "TwoDimensional" << std::endl; break; }
			case TouchMode::ThreeDimensional: { std::cout << "ThreeDimensional" << std::endl; break; }
			case TouchMode::Invalid: { break; }
		}*/

		// -----------------------------------------------------------------
		// React to actions
		// -----------------------------------------------------------------

		// Start navigation panel interaction
		//if (_touchMode == TouchMode::Waiting && newModeCandidate == TouchMode::NavigationPanel) {
		//	viargo::Touch* touch = touches.front();

		//	// Start position of touch
		//	viargo::vec3f touchPositionViargo = touch->current.position;
		//	
		//	const Ogre::Vector2 touchPositionOgre   = (Ogre::Vector2(touchPositionViargo.x, -1.0f * touchPositionViargo.y) + Ogre::Vector2(1.0f)) * 0.5f;

		//	// Store start touch position
		//	_dragTouchStartPosition = Ogre::Vector3(touchPositionOgre.x, touchPositionOgre.y, 0.0f);

		//	// Apply new mode
		//	_touchMode = newModeCandidate;

		//	// Adjust transparency
		//	_adjustNavigationOverlaysAlpha(1.0f);
		//}

		//// Navigation panel interaction
		//if (_touchMode == TouchMode::NavigationPanel && newModeCandidate == TouchMode::Invalid) {
		//	viargo::Touch* touch = touches.front();

		//	// Start position of touch
		//	viargo::vec3f touchPositionViargo = touch->current.position;

		//	const Ogre::Vector2 touchPositionOgre   = (Ogre::Vector2(touchPositionViargo.x, -1.0f * touchPositionViargo.y) + Ogre::Vector2(1.0f)) * 0.5f;

		//	const Ogre::Real    navigationPanelRadius   = (_navigationPanelTopRight.x - _navigationPanelBottomLeft.x) / 2.0f - _navigationMarkerSize.x; // !
		//	const Ogre::Vector2 navigationPanelCenter   = (_navigationPanelTopRight + _navigationPanelBottomLeft) / 2.0f;
		//	const Ogre::Real    navigationPanelDistance = navigationPanelCenter.distance(touchPositionOgre);

		//	Ogre::Vector2 newOverlayMarkerPosition = Ogre::Vector2(navigationPanelCenter.x, touchPositionOgre.y);

		//	// Check for not exceeding the panel bounds
		//	if (newOverlayMarkerPosition.y < _navigationPanelBottomLeft.y) {
		//		newOverlayMarkerPosition.y = _navigationPanelBottomLeft.y;
		//	}

		//	if (newOverlayMarkerPosition.y > _navigationPanelTopRight.y) {
		//		newOverlayMarkerPosition.y = _navigationPanelTopRight.y;
		//	}
		//				
		//	//if (navigationPanelDistance > navigationPanelRadius) {
		//	//	// Normalized direction vector from panel center to touch point
		//	//	Ogre::Vector2 directionVector = (touchPositionOgre - navigationPanelCenter).normalisedCopy();

		//	//	// Compute position on the line between center and touch, which lies on the edge of the panel
		//	//	newOverlayMarkerPosition = navigationPanelCenter + navigationPanelRadius * directionVector;
		//	//}
		//	//else {
		//	//	newOverlayMarkerPosition = Ogre::Vector2(touchPositionOgre.x, touchPositionOgre.y);
		//	//}

		//	// Subtract offset to adjust to rectangular overlay
		//	//newOverlayMarkerPosition -= (_navigationMarkerSize * 0.5f);

		//	// Assign new position
		//	_navigationMarkerOverlayContainer->setPosition(newOverlayMarkerPosition.x, newOverlayMarkerPosition.y);			

		//	// Calculate normalized information of the current navigation marker
		//	float markerNavigationCenterDistance = touchPositionOgre.distance(navigationPanelCenter) / navigationPanelRadius;  
		//	Ogre::Radian markerNavigationAngle   = Ogre::Vector2(0.0f, 1.0f).angleBetween(touchPositionOgre - navigationPanelCenter); // in [0, 2 * PI)
		//	const bool sign                      = (touchPositionOgre - navigationPanelCenter).x > 0;

		//	// Change sign 
		//	if (!sign) {
		//		markerNavigationAngle = Ogre::Radian(2.0f * Ogre::Math::PI) - markerNavigationAngle;
		//	}

		//	// Normalize distance to [0, 1]
		//	markerNavigationCenterDistance = (markerNavigationCenterDistance > 1.0f) ? 1.0f : markerNavigationCenterDistance;

		//	// Compute final angles for arcball camera
		//	//const Ogre::Radian cameraLockedAlphaAngle = Ogre::Radian(1.0f / 2.0f * Ogre::Math::PI);  // 90°
		//	const Ogre::Radian cameraLockedAlphaAngle = Ogre::Radian(1.0f / 4.0f * Ogre::Math::PI);    // 45°
		//	const Ogre::Radian cameraAlphaAngle       = Ogre::Radian(1.0f / 2.0f * Ogre::Math::PI) - Ogre::Radian(markerNavigationCenterDistance) * cameraLockedAlphaAngle;
		//	const Ogre::Radian cameraBetaAngle        = markerNavigationAngle;

		//	// Adjust shadow camera position
		//	const Ogre::Vector3 basePosition = Ogre::Vector3(0.0f, 0.0f, -25.0f);
		//	const float fixedRadius          = 200.0f; // global::tableWidth / 2.0f;

		//	// Calculate new camera position
		//	Ogre::Vector3 cameraPosition = Ogre::Vector3(0.0f, -fixedRadius, 0.0f);
		//	
		//	//cameraPosition = Ogre::Quaternion(-cameraAlphaAngle, Ogre::Vector3::UNIT_X) * cameraPosition;  // Camera from top
		//	cameraPosition = Ogre::Quaternion(cameraAlphaAngle, Ogre::Vector3::UNIT_X) * cameraPosition; // camera from bottom
		//	cameraPosition = Ogre::Quaternion(cameraBetaAngle,  Ogre::Vector3::UNIT_Z) * cameraPosition;
		//	cameraPosition += basePosition;
		//				
		//	// Assign camera position			
		//	_shadowCasterCamera->setPosition(cameraPosition);
		//	//_shadowCasterCamera->lookAt(Ogre::Vector3(0.0f, 0.0f, -25.0f));
		//	_shadowCasterCamera->lookAt(Ogre::Vector3(cameraPosition.x, cameraPosition.y, 0.0f)); // Perspective correct, look orthogonal at surface

		//	_calculateFrustum(_shadowCasterCamera, cameraPosition);
		//}

		// Start two dimensional grabbing
		if (_touchMode == TouchMode::Waiting && newModeCandidate == TouchMode::TwoDimensional) {
			viargo::Touch* touch = touches.front();

			// Start position of touch
			viargo::vec3f touchPositionViargo = touch->current.position;

			if (FLIP_X_AXIS)
				touchPositionViargo.x = -1.0f * touchPositionViargo.x;

			if (FLIP_Y_AXIS)
				touchPositionViargo.y = -1.0f * touchPositionViargo.y;

			const Ogre::Vector2 touchPositionOgre   = (Ogre::Vector2(touchPositionViargo.x, -1.0f * touchPositionViargo.y) + Ogre::Vector2(1.0f)) * 0.5f;

			// Get object under the touch point
			Ogre::SceneNode* node = rayCast(touchPositionOgre);

			if (node) {
				// Store node
				_grabbedObjectNode = node;

				// Get index of dynamic box
				std::stringstream sstream;
				sstream << node->getName().substr(11, 1);

				size_t boxIndex = 0;
				sstream >> boxIndex;
				boxIndex--;

				// Store rigid body
				_grabbedRigidBody = Ogre::any_cast<OgreBulletDynamics::RigidBody*>(_grabbedObjectNode->getUserObjectBindings().getUserAny("rigid_body"));//_experiment->rigidBodies()->at(boxIndex);
				
				// Reset orientation of the object
				_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));

				// Store object start position
				_dragObjectStartPosition = node->getPosition();

				// Compute approximative scene coordinate position of touch
				const float nx = touchPositionViargo.x * global::tableWidth  / 2.0f;
				const float ny = touchPositionViargo.y * global::tableHeight / 2.0f;
				
				// Store touch offset
				_dragTouchStartPosition = Ogre::Vector3(nx, ny, 0.0f);

				// Deactivate physics for this object
				//_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));
				//_experiment->dynamicWorld()->getBulletDynamicsWorld()->removeRigidBody(_grabbedRigidBody->getBulletRigidBody());
				_experiment->togglePhysics(false);
				_experiment->resetOrientation(_grabbedObjectNode);
				_experiment->resyncPhysics(_grabbedRigidBody);

				// Apply new mode
				_touchMode = newModeCandidate;
			}
		}

		// Release grabbed object (no matter of previous mode)
		if (newModeCandidate == TouchMode::Waiting) {
			if (_grabbedRigidBody != 0) {
				//_experiment->dynamicWorld()->getBulletDynamicsWorld()->addRigidBody(_grabbedRigidBody->getBulletRigidBody());
				_experiment->togglePhysics(true);

				_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(1, 1, 1));
				_grabbedRigidBody = 0;
			}

			// Re-adjust transparency
			if (_touchMode == TouchMode::NavigationPanel) {
				_adjustNavigationOverlaysAlpha(0.5f);
			}

			// Apply new mode
			_touchMode = newModeCandidate;
		}

		// Move object two dimensionally
		if (_touchMode == TouchMode::TwoDimensional && newModeCandidate == TouchMode::Invalid) {
			viargo::Touch* touch = touches.front();

			// Start start position of touch
			viargo::vec3f touchPositionViargo = touch->current.position;

			if (FLIP_X_AXIS)
				touchPositionViargo.x = -1.0f * touchPositionViargo.x;

			if (FLIP_Y_AXIS)
				touchPositionViargo.y = -1.0f * touchPositionViargo.y;
			
			if (_grabbedRigidBody != 0) {
				// Compute approximative scene coordinate position of touch
				const float nx = touchPositionViargo.x * global::tableWidth  / 2.0f;
				const float ny = touchPositionViargo.y * global::tableHeight / 2.0f;

				const Ogre::Vector3 touchPosition = Ogre::Vector3(nx, ny, 0.0f);

				Ogre::Vector3 touchOffset = touchPosition - _dragTouchStartPosition;

				if (FLIP_X_AXIS)
					touchOffset.x = -1.0f * touchOffset.x;

				if (FLIP_Y_AXIS)
					touchOffset.y = -1.0f * touchOffset.y;

				// -----------------------------------------------------------

				// World Direction: Camera -> Touch Point
				//const Ogre::Vector3 v1 = Ogre::Vector3(_dragTouchStartPosition.x, _dragTouchStartPosition.y, _dragTouchStartPosition.z) - _shadowCasterCamera->getPosition();

				//// World Direction: Camera -> Object
				//const Ogre::Vector3 v2 = _dragObjectStartPosition - _shadowCasterCamera->getPosition();

				//// World Direction: Old Touch -> New Touch
				//const Ogre::Vector3 t  = Ogre::Vector3(nx, ny, 0.0f) - Ogre::Vector3(_dragTouchStartPosition.x, _dragTouchStartPosition.y, _dragTouchStartPosition.z);

				//// Intersection Theorem Ratio
				//const Ogre::Real delta = t.length() / v1.length();

				//// Offset Vector Length
				//const Ogre::Real offsetLength = delta * v2.length();

				//// World Offset Vector
				//const Ogre::Vector3 offset = offsetLength * (t.normalisedCopy());

				//// New Position
				//const Ogre::Vector3 newPosition = _dragObjectStartPosition + offset;

				// -----------------------------------------------------------

				const Ogre::Vector3 newPosition   = _dragObjectStartPosition + touchOffset;
				//const Ogre::Vector3 newPosition   = _dragObjectStartPosition + (touchPosition - _dragTouchStartPosition);

				//btTransform transform; 
				//transform.setIdentity(); 
				//transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(newPosition)); 
				//_grabbedRigidBody->getBulletRigidBody()->setWorldTransform(transform); 
				_grabbedObjectNode->setPosition(newPosition);
				_experiment->resetOrientation(_grabbedObjectNode);
				_experiment->resyncPhysics(_grabbedRigidBody);
			}
		}

		// Start three dimensional grabbing (if previous mode was two dimensional grabbing)
		if (_touchMode == TouchMode::TwoDimensional && newModeCandidate == TouchMode::ThreeDimensional) {
			viargo::Touch* touch1 = touches[0];
			viargo::Touch* touch2 = touches[1];

			// Start position of touch
			viargo::vec3f touch1PositionViargo = touch1->current.position;
			viargo::vec3f touch2PositionViargo = touch2->current.position;

			if (FLIP_X_AXIS) {
				touch1PositionViargo.x = -1.0f * touch1PositionViargo.x;
				touch2PositionViargo.x = -1.0f * touch2PositionViargo.x;
			}

			if (FLIP_Y_AXIS) {
				touch1PositionViargo.y = -1.0f * touch1PositionViargo.y;
				touch2PositionViargo.y = -1.0f * touch2PositionViargo.y;
			}
			
			// Compute approximative scene coordinate position of touches
			const float nx1 = touch1PositionViargo.x * global::tableWidth  / 2.0f;
			const float ny1 = touch1PositionViargo.y * global::tableHeight / 2.0f;

			const float nx2 = touch2PositionViargo.x * global::tableWidth  / 2.0f;
			const float ny2 = touch2PositionViargo.y * global::tableHeight / 2.0f;

			const Ogre::Vector3 touch1Position      = Ogre::Vector3(nx1, ny1, 0.0f);
			const Ogre::Vector3 touch2Position      = Ogre::Vector3(nx2, ny2, 0.0f);
			const Ogre::Vector3 centerTouchPosition = (touch1Position + touch2Position) / 2.0f;

			const float distance = touch1Position.distance(touch2Position);
				
			// Store touch offset
			_dragTouchStartPosition = centerTouchPosition + Ogre::Vector3(0.0f, 0.0f, distance * touchBallonScale);

			// Store again object start position
			_dragObjectStartPosition = _grabbedObjectNode->getPosition();

			// Get start angle for rotation
			const Ogre::Vector3 touchDirection = touch1Position - touch2Position;
			_shadowRotationStart = Ogre::Math::ATan2(touchDirection.y, touchDirection.x);

			// Store initial orientation of the object
			_dragObjectStartOrientation = _grabbedObjectNode->getOrientation();

			// Apply new mode
			_touchMode = newModeCandidate;
		}

		// Start three dimensional grabbing (if previous mode was waiting)
		if (_touchMode == TouchMode::Waiting && newModeCandidate == TouchMode::ThreeDimensional) {
			viargo::Touch* touch1 = touches[0];
			viargo::Touch* touch2 = touches[1];

			// Start position of touch
			viargo::vec3f touch1PositionViargo = touch1->current.position;
			viargo::vec3f touch2PositionViargo = touch2->current.position;

			if (FLIP_X_AXIS) {
				touch1PositionViargo.x = -1.0f * touch1PositionViargo.x;
				touch2PositionViargo.x = -1.0f * touch2PositionViargo.x;
			}

			if (FLIP_Y_AXIS) {
				touch1PositionViargo.y = -1.0f * touch1PositionViargo.y;
				touch2PositionViargo.y = -1.0f * touch2PositionViargo.y;
			}
			
			// Compute approximative scene coordinate position of touches
			const float nx1 = touch1PositionViargo.x * global::tableWidth  / 2.0f;
			const float ny1 = touch1PositionViargo.y * global::tableHeight / 2.0f;

			const float nx2 = touch2PositionViargo.x * global::tableWidth  / 2.0f;
			const float ny2 = touch2PositionViargo.y * global::tableHeight / 2.0f;

			const Ogre::Vector3 touch1Position      = Ogre::Vector3(nx1, ny1, 0.0f);
			const Ogre::Vector3 touch2Position      = Ogre::Vector3(nx2, ny2, 0.0f);
			const Ogre::Vector3 centerTouchPosition = (touch1Position + touch2Position) / 2.0f;

			const Ogre::Vector2 touchPositionOgre   = (Ogre::Vector2(centerTouchPosition.x, -1.0f * centerTouchPosition.y) + Ogre::Vector2(1.0f)) * 0.5f;

			// Get start angle for rotation
			const Ogre::Vector3 touchDirection = touch1Position - touch2Position;
			_shadowRotationStart = Ogre::Math::ATan2(touchDirection.y, touchDirection.x);

			// Get object under the touch point
			Ogre::SceneNode* node = rayCast(touchPositionOgre);

			if (node) {
				// Store node
				_grabbedObjectNode = node;

				// Store initial orientation of the object
				_dragObjectStartOrientation = _grabbedObjectNode->getOrientation();

				// Get index of dynamic box
				std::stringstream sstream;
				sstream << node->getName().substr(11, 1);

				size_t boxIndex = 0;
				sstream >> boxIndex;
				boxIndex--;

				// Store rigid body
				_grabbedRigidBody = Ogre::any_cast<OgreBulletDynamics::RigidBody*>(_grabbedObjectNode->getUserObjectBindings().getUserAny("rigid_body"));//_experiment->rigidBodies()->at(boxIndex);
				
				// Reset orientation of the object
				_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));

				// Store object start position
				_dragObjectStartPosition = node->getPosition();

				const float distance = touch1Position.distance(touch2Position);
				
				// Store touch offset
				_dragTouchStartPosition = centerTouchPosition + Ogre::Vector3(0.0f, 0.0f, distance/* * touchBallonScale*/);
				
				// Deactivate physics for this object
				//_grabbedRigidBody->getBulletRigidBody()->setLinearFactor(btVector3(0, 0, 0));
				//_experiment->dynamicWorld()->getBulletDynamicsWorld()->removeRigidBody(_grabbedRigidBody->getBulletRigidBody());
				_experiment->togglePhysics(false);
				_experiment->resetOrientation(_grabbedObjectNode);
				_experiment->resyncPhysics(_grabbedRigidBody);

				// Apply new mode
				_touchMode = newModeCandidate;
			}
		}

		// Move object three dimensionally
		if (_touchMode == TouchMode::ThreeDimensional && newModeCandidate == TouchMode::Invalid) {
			viargo::Touch* touch1 = touches[0];
			viargo::Touch* touch2 = touches[1];

			// Start position of touch
			viargo::vec3f touch1PositionViargo = touch1->current.position;
			viargo::vec3f touch2PositionViargo = touch2->current.position;

			if (FLIP_X_AXIS) {
				touch1PositionViargo.x = -1.0f * touch1PositionViargo.x;
				touch2PositionViargo.x = -1.0f * touch2PositionViargo.x;
			}

			if (FLIP_Y_AXIS) {
				touch1PositionViargo.y = -1.0f * touch1PositionViargo.y;
				touch2PositionViargo.y = -1.0f * touch2PositionViargo.y;
			}
			
			if (_grabbedRigidBody != 0) {
				// Compute approximative scene coordinate position of touches
				const float nx1 = touch1PositionViargo.x * global::tableWidth  / 2.0f;
				const float ny1 = touch1PositionViargo.y * global::tableHeight / 2.0f;

				const float nx2 = touch2PositionViargo.x * global::tableWidth  / 2.0f;
				const float ny2 = touch2PositionViargo.y * global::tableHeight / 2.0f;

				const Ogre::Vector3 touch1Position      = Ogre::Vector3(nx1, ny1, 0.0f);
				const Ogre::Vector3 touch2Position      = Ogre::Vector3(nx2, ny2, 0.0f);
				const Ogre::Vector3 centerTouchPosition = (touch1Position + touch2Position) / 2.0f;

				const float distance = touch1Position.distance(touch2Position);

				Ogre::Vector3 touchOffset = (centerTouchPosition + Ogre::Vector3(0.0f, 0.0f, distance * touchBallonScale) - _dragTouchStartPosition);

				if (FLIP_X_AXIS)
					touchOffset.x = -1.0f * touchOffset.x;

				if (FLIP_Y_AXIS)
					touchOffset.y = -1.0f * touchOffset.y;




				// -----------------------------------------------------------

				//// World Direction: Camera -> Touch Point
				//const Ogre::Vector3 v1 = Ogre::Vector3(_dragTouchStartPosition.x, _dragTouchStartPosition.y, 0.0f) - _shadowCasterCamera->getPosition();

				//// World Direction: Camera -> Object
				//const Ogre::Vector3 v2 = _dragObjectStartPosition - _shadowCasterCamera->getPosition();

				//// World Direction: Old Touch -> New Touch
				//const Ogre::Vector3 t  = centerTouchPosition - Ogre::Vector3(_dragTouchStartPosition.x, _dragTouchStartPosition.y, 0.0f);

				//// Intersection Theorem Ratio
				//const Ogre::Real delta2 = t.length() / v1.length();

				//// Offset Vector Length
				//const Ogre::Real offsetLength = delta2 * v2.length();

				//// World Offset Vector
				//const Ogre::Vector3 offset = offsetLength * (t.normalisedCopy());

				//// New Object Position
				//Ogre::Vector3 newPosition = _dragObjectStartPosition + offset;



				// Old distance between the two touch points
				//const Ogre::Real oldDistance = _dragTouchStartPosition.z;

				//// New distance between the two touch points
				//const Ogre::Real newDistance = distance;

				//// World Direction Vector Camera -> Object Old
				////const Ogre::Vector3 a1 = _dragObjectStartPosition - _shadowCasterCamera->getPosition();
				//const Ogre::Vector3 a1 = newPosition - _shadowCasterCamera->getPosition();

				//// Intersection Theorem Ratio
				//const Ogre::Real delta = oldDistance / newDistance;

				//// Calculated distance
				//const Ogre::Real calcDistance = delta * a1.length();

				//// New Object Position
				////const Ogre::Vector3 a2 = _shadowCasterCamera->getPosition() + calcDistance * a1.normalisedCopy();
				//newPosition = _shadowCasterCamera->getPosition() + calcDistance * a1.normalisedCopy();

				// Get start angle for rotation

				const Ogre::Vector3 touchDirection = touch1Position - touch2Position;
				const Ogre::Radian shadowRotation  = Ogre::Math::ATan2(touchDirection.y, touchDirection.x);
				const Ogre::Radian rotationDiff    = shadowRotation - _shadowRotationStart;


				Ogre::Quaternion orientation = Ogre::Quaternion(rotationDiff, Ogre::Vector3::UNIT_Z);
				Ogre::Quaternion newOrientation = orientation * _dragObjectStartOrientation;
				


				// ---------- NOTE: ALEX: Comment this out to allow orientation change (and comment the orientation reset down there out) -----------------------
				//_grabbedObjectNode->setOrientation(newOrientation);
				// ----------------------------------------------------------------------------------------------------------------------------------------------

				


				
				//const Ogre::Vector3 newPosition = a2 + offset;

				// -----------------------------------------------------------




				
				const Ogre::Vector3 newPosition = _dragObjectStartPosition + touchOffset;
				//Ogre::Vector3 newPosition = _dragObjectStartPosition + (centerTouchPosition + Ogre::Vector3(0.0f, 0.0f, distance * touchBallonScale) - _dragTouchStartPosition);

			/*	btTransform transform; 
				transform.setIdentity(); 
				transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(newPosition)); 
				_grabbedRigidBody->getBulletRigidBody()->setWorldTransform(transform); */
				_grabbedObjectNode->setPosition(newPosition);

				_experiment->resetOrientation(_grabbedObjectNode);
				_experiment->resyncPhysics(_grabbedRigidBody);
			}
		}

		// Start two dimensional grabbing (if previous mode was three dimensional grabbing)
		if (_touchMode == TouchMode::ThreeDimensional && newModeCandidate == TouchMode::TwoDimensional) {
			viargo::Touch* touch = touches.front();

			// Start position of touch
			viargo::vec3f touchPositionViargo = touch->current.position;

			if (FLIP_X_AXIS)
				touchPositionViargo.x = -1.0f * touchPositionViargo.x;

			if (FLIP_Y_AXIS)
				touchPositionViargo.y = -1.0f * touchPositionViargo.y;

			// Store object start position
			_dragObjectStartPosition = _grabbedObjectNode->getPosition();

			// Compute approximative scene coordinate position of touch
			const float nx = touchPositionViargo.x * global::tableWidth  / 2.0f;
			const float ny = touchPositionViargo.y * global::tableHeight / 2.0f;
				
			// Store touch offset
			_dragTouchStartPosition = Ogre::Vector3(nx, ny, 0.0f);
				
			// Apply new mode
			_touchMode = newModeCandidate;
		}
	}
}

void MultitouchShadows::_adjustNavigationOverlaysAlpha(float alpha) {
	Ogre::MaterialManager::getSingleton().getByName("navigation_panel")->getTechnique(0)->getPass(0)->getTextureUnitState(0)->setAlphaOperation(Ogre::LBX_MODULATE, Ogre::LBS_TEXTURE, Ogre::LBS_MANUAL, 1.0, alpha);
	Ogre::MaterialManager::getSingleton().getByName("navigation_marker")->getTechnique(0)->getPass(0)->getTextureUnitState(0)->setAlphaOperation(Ogre::LBX_MODULATE, Ogre::LBS_TEXTURE, Ogre::LBS_MANUAL, 1.0, alpha);
}

