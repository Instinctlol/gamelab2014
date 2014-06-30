#ifndef ASYNC_KINECT_PROCESSOR_H
#define ASYNC_KINECT_PROCESSOR_H

#include <OgreRoot.h>
#include <OgreSceneManager.h>
#include <OgreSceneNode.h>
#include <OgreTimer.h>

#include <opencv2/opencv.hpp>

#include "OgreBulletDynamicsRigidBody.h"

#include "threading/nixie_tiny_thread.h"

// ------------------------------------------------------------------------------
// Forward declarations
// ------------------------------------------------------------------------------
class Experiment;
class Kinect;
class KinectMetaphor;
class ViargoOgreKinectTrackingCalibrationMetaphor;
class OgreBulletCollisionMetaphor;
// ------------------------------------------------------------------------------

//#define VOID_SHADOWS_SHOW_OPENCV_TRACKBARS
//#define VOID_SHADOWS_SHOW_DEBUG_OUTPUT

class AsyncKinectProcessor {
public:
	/// ctor
	///
	AsyncKinectProcessor(Ogre::SceneManager* sceneManager, Experiment* experiment);

	/// dtor
	///
	~AsyncKinectProcessor();

	/// Toggles kinect hand processing
	///
	void toggleProcessing(bool active);

	/// Returns true, if hand is currently opened
	///
	bool isHandOpen() const;

	/// Returns last detected hand position in Ogre Scene coordinates
	///
	Ogre::Vector3 position() const;

	/// Starts Kinect calibration process
	///
	void startCalibration();

	/// Trigger debug information output
	///
	void toggleShowDebugInformation();

	/// Update kinect hand logic (from the Ogre rendering thread)
	///
	void update(double deltaTime);

private:
	/// Kinect wrapper
	///
	Kinect* _kinect;

	/// Async thread
	///
	nixie::thread* _thread;

	/// Async mutex
	///
	nixie::mutex _mutex;

	/// 8-Bit processed depth frame for shadow texturing
	///
	cv::Mat _depth8Bit;

	/// If true, kinect processing is active
	///
	bool _active;

	/// If true, kinect was correctly initialized. If false, processing will not be issued, no matter of active state
	///
	bool _kinectInitialized;

	/// If true, hand is currently open
	///
	mutable bool _open;

	/// Last state of the hand
	///
	mutable bool _lastState;

	/// If true, processing will output additional debug information
	///
	mutable bool _showDebugInformation;
	
	/// Current detected hand position in Ogre coordinates
	///
	mutable Ogre::Vector3 _currentHandPosition;

	/// Old Ogre 3D Scene Hand positions
	///
	std::deque<Ogre::Vector3> _oldHandPositions;
	
	/// Array of color -> depth coordinate transformations
	///
	LONG* _colorCoordinates;

	/// If true, kinect is currently in calibration mode
	///
	bool _calibrationRunning;

	/// Position of the finger during calibration
	///
	mutable cv::Point3f _calibrationFinger;

	/// If true, finger was found during calibration
	///
	mutable bool _calibrationFingerFound;

	/// Collection of last hand positions in screen coordinates
	///
	std::deque<cv::Point2f> _lastHandPositionScreen;

	/// Pointer to the Ogre scene manager
	///
	Ogre::SceneManager* _sceneMgr;

	/// Pointer to the collision metaphor
	///
	//OgreBulletCollisionMetaphor* _collisionMetaphor;
	Experiment* _experiment;

	/// Pointer to the kinect calibration metaphor
	///
	ViargoOgreKinectTrackingCalibrationMetaphor* _kinectCalibrationMetaphor;

	/// Pointer to the four scene boxes
	///
	//OgreBulletDynamics::RigidBody* _testBoxes[4];

	/// Pointer to the scene node that represents the hand
	///
	Ogre::SceneNode* _handNode;

	/// Pointer to the entity that represents the hand
	///
	Ogre::Entity* _handEntity;
		
	/// Currently grabbed node index
	///
	//int _activeNode;
	Ogre::SceneNode* _activeNode;

	/// If true, user is currently grabbing an object
	///
	bool _objectGrabbing;

	/// Start position of a grabbed object
	///
	Ogre::Vector3 _objectGrabStartPosition;

	/// Start position of the hand in a grab operation
	///
	Ogre::Vector3 _handGrabStartPosition;

	/// Position of the Kinect sensor in Ogre3D scene coordinates
	///
	Ogre::Vector3 _kinectSensorPosition;

	/// Direction of the Kinect sensor in Ogre3D scene coordinates
	/// NOTE: This is a direction vector, not a position
	///
	Ogre::Vector3 _kinectSensorDirection;


	Ogre::Timer mTimer; 
	bool _timerFilter;

	bool _delayHandTrigger;
	Ogre::Timer mTimer2; 

	Ogre::Vector3 _oldHandPos;
	cv::Point3f _oldPoint;

	//KinectMetaphor *km;
	
	

	int thresh;
	int contourFilterArea;
	int threshValue;

	/// Ogre frustum for projective texture
	///
	Ogre::Frustum* _projectiveFurstum;

	/// Texture buffer for kinect shadow image
	///
	Ogre::HardwarePixelBufferSharedPtr	_kinectShadowBuffer;

	/// If true, threading will be stopped
	///
	mutable bool _stopThread;
	
	/// Thread loop
	///
	static void _runThread(void* param);

	/// Update kinect processing
	///
	void _update(double deltaTime);

	/// Initialize all processing
	///
	void _initialize();

	/// Detects finger in depth image (used for calibration)
	///
	bool _findFinger(cv::Point3f& point, const cv::Mat* depth);

	/// Transforms hand position from screen coordinates into 3D Ogre scene coordinates
	///
	Ogre::Vector3 _transformHandPosition(const cv::Point2f& position, unsigned short depthValue);
	
	/// Handle object movement
	///
	void _handleMoveObject(const Ogre::Vector3& position);

	/// Handle object grabbing
	///
	void _handleObjectGrabbing(const Ogre::Vector3& position);

	/// Test for intersection of hand and object
	///
	bool _testObjectIntersection(Ogre::SceneNode* node, const Ogre::Vector3& position);

	// ---------------------------------------------------------------------------------------------------------------------
	// Processing steps
	// ---------------------------------------------------------------------------------------------------------------------

	/// Converts RGB-Color frame into inverted gray scale frame
	///
	void _processConvertGrayScale(const cv::Mat& colorFrame, cv::Mat& grayScaleFrame);

	/// Clip sides from gray scale image
	///
	void _processClipGrayScale(cv::Mat& grayScaleFrame);

	/// Threshold gray scale image
	///
	void _processThresholdGrayScale(cv::Mat& grayScaleFrame);

	/// Detect contours in gray scale frame and apply area filter to detect users arm
	///
	void _processContourDetection(cv::Mat& grayScaleFrame, std::vector<std::vector<cv::Point> >& outputContours);

	/// Draw arm contour(s) into color mask
	///
	void _processMaskContours(cv::Mat& colorMask, const std::vector<std::vector<cv::Point> >& contours);

	/// Color depth image registration
	///
	void _processTransformColorToDepth(const cv::Mat& color, const cv::Mat& depth, const cv::Mat& mask, cv::Mat& registeredColor);

	/// Mask depth image to segment hand
	///
	void _processMaskDepth(cv::Mat& depthFrame, const cv::Mat& registeredColor, cv::Mat& mask);

	/// Remove "shadow" artifacts from depth image
	///
	void _processRemoveShadowsFromDepth(cv::Mat& depthFrame, cv::Mat& depthFrame8Bit, const cv::Mat& mask);

	/// Run kinect calibration
	///
	void _processCalibration(const cv::Mat& depthFrame);

	/// Cutoff arm from hand in depth frame
	///
	void _processCutoffArm(cv::Mat& depthFrame, const cv::Mat& depthFrame8Bit);

	/// Detect 3D hand position and open / closed state
	///
	void _processHandDetection(const cv::Mat& depthFrame);

	/// Extract and process depth frame into a shadow texture
	///
	void _processDepthShadowTransformation(const cv::Mat& depthFrame8Bit);

	// ---------------------------------------------------------------------------------------------------------------------

	/// Add an additional material pass for projective shadows from kinect
	///
	void _addKinectMaterial(const Ogre::String& name, Ogre::Frustum* projectiveFrustum);

}; // ViargoHeadtrackingDevice


#endif // ASYNC_KINECT_PROCESSOR_H