#ifndef MULTITOUCH_SHADOWS_H
#define MULTITOUCH_SHADOWS_H

#include <OgreRoot.h>
#include <OgreOverlay.h>
#include <OgreSceneManager.h>
#include <OgreRenderTargetListener.h>

#include <viargo.h>



// ------------------------------------------------------------------------------
// Forward declarations
// ------------------------------------------------------------------------------
class Experiment;
class ShadowVolumeManager;

namespace OgreBulletDynamics {
	class RigidBody;
}
// ------------------------------------------------------------------------------

class MultitouchShadows : public Ogre::RenderTargetListener, public viargo::Metaphor {
public:
	/// ctor
	///
	MultitouchShadows(Ogre::SceneManager* sceneManager, Experiment* experiment, Ogre::RenderSystem* renderSystem);

	/// dtor
	///
	~MultitouchShadows();
	
	/// Activates or deactivates processing and visualization
	///
	void toggleProcessing(bool active);

	/// Render target listener callback
	///
	virtual void preViewportUpdate(Ogre::RenderTargetViewportEvent const & evt);

	/// Render target listener callback
	///
    virtual void postViewportUpdate(Ogre::RenderTargetViewportEvent const & evt);
	
	/// Returns the shadow caster camera
	///
	Ogre::Camera* shadowCasterCamera() const;

	/// Performs a ray cast from the normalized screen coordinates [0,1] x [0,1] with the shadow projector into the scene
	///
	Ogre::SceneNode* rayCast(const Ogre::Vector2& normalizedScreenCoordinates);

	/// Viargo metaphor callback
	///
	virtual bool onEventReceived(viargo::Event* event);

	/// Viargo metaphor callback
	///
	virtual void handleEvent(viargo::Event* event);

	/// Viargo metaphor callback
	///
	virtual void update(float timeSinceLastUpdate);

private:
	struct TouchMode {
		enum Value {
			Invalid = 0,       ///< No specific action (default)
			Waiting,           ///< No touches, wait for incomming touches
			TwoDimensional,    ///< One touch point, move two dimensional
			ThreeDimensional,  ///< Two touch points, move three dimensional
			NavigationPanel,   ///< Interaction with the navigation panel
		}; // Value
	}; // TouchMode

	/// Initialize multitouch shadows
	///
	void _initialize();

	/// Calculates projection frustum for the shadow proejction camera
	///
	void _calculateFrustum(Ogre::Camera* camera, const Ogre::Vector3& cameraPosition);

	/// Adjust the alpha blending factor of the navigation panel overlays
	///
	void _adjustNavigationOverlaysAlpha(float alpha);

	/// If true, processing and visualization is active
	///
	bool _active;

	/// Resolution of the shadow texture
	///
	Ogre::Vector2 _shadowTextureResolution;

	/// Pointer to the Ogre scene manager
	///
	Ogre::SceneManager* _sceneMgr;

	/// Pointer to the Ogre render system
	///
	Ogre::RenderSystem* _renderSystem;

	/// Pointer to the experiment controler
	///
	Experiment* _experiment;

	/// Camera for shadow casting
	///
	Ogre::Camera* _shadowCasterCamera;

	/// Render texture which receives projected shadows
	///
	Ogre::RenderTexture* _shadowRenderTexture;

	/// Material which renders depth
	///
	Ogre::MaterialPtr _depthMaterial;

	/// Pointer to the surface plane node
	///
	Ogre::SceneNode* _surfacePlaneNode;

	/// Current touch mode
	///
	TouchMode::Value _touchMode;

	/// Array of managed touch IDs
	///
	//int _touchIDs[2];
	
	/// Start position of object before dragging
	///
	Ogre::Vector3 _dragObjectStartPosition;

	/// Start position of touches before action switch
	///
	//Ogre::Vector3 _dragTouchStartPositions[2];
	Ogre::Vector3 _dragTouchStartPosition;

	/// Start angle for two-touch rotation
	///
	Ogre::Radian _shadowRotationStart;

	/// Start orientation of the object before dragging / rotating
	///
	Ogre::Quaternion _dragObjectStartOrientation;

	/// Pointer to the scene node of the object, which is currently grabbed
	///
	Ogre::SceneNode* _grabbedObjectNode;

	/// Pointer to the rigid body of the object, which is currently grabbed
	///
	OgreBulletDynamics::RigidBody* _grabbedRigidBody;

	/// Overlay for the navigation panel
	///
	Ogre::Overlay* _navigationPanelOverlay;

	/// Overlay container for the navigation panel
	///
	Ogre::OverlayContainer* _navigationPanelOverlayContainer;

	/// Overlay container for the navigation marker
	///
	Ogre::OverlayContainer* _navigationMarkerOverlayContainer;

	/// Position and dimension of the navigation panel
	///
	Ogre::Vector2 _navigationPanelBottomLeft;
	Ogre::Vector2 _navigationPanelTopRight;
	Ogre::Vector2 _navigationMarkerSize;

	/// Height of the light plane
	///
	Ogre::Real _lightPlaneHeight;

	/// Shadow volume manager calculates shadow volumes from dynamic objects
	///
	ShadowVolumeManager* _shadowVolumeMgr;

}; // MultitouchShadows

#endif // MULTITOUCH_SHADOWS_H
