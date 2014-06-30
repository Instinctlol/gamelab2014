#include <OgreCamera.h>
#include <OgreOverlay.h>
#include <OgreOverlayContainer.h>
#include <OgreOverlayManager.h>
#include <OgreMaterialManager.h>
#include <OgreMaterial.h>
#include <OgreTechnique.h>

#include <opencv2\opencv.hpp>

#include "tracking/viargo_ogre_head_tracking_calibration_metaphor.h"

#ifdef _DEBUG
	#pragma comment(lib, "opencv_core248d.lib")
	#pragma comment(lib, "opencv_highgui248d.lib")
	#pragma comment(lib, "opencv_imgproc248d.lib")
	#pragma comment(lib, "opencv_calib3d248d.lib")
	#pragma comment(lib, "opencv_features2d248d.lib")
#else
	#pragma comment(lib, "opencv_core248.lib")
	#pragma comment(lib, "opencv_highgui248.lib")
	#pragma comment(lib, "opencv_imgproc248.lib")
	#pragma comment(lib, "opencv_calib3d248.lib")
	#pragma comment(lib, "opencv_features2d248.lib")
	#pragma comment(lib, "opencv_gpu248.lib")
#endif

namespace viargo {

// 2 Second pause time
const float ViargoOgreHeadTrackingCalibrationMetaphor::_PAUSE_TIME = 2000.0f;

ViargoOgreHeadTrackingCalibrationMetaphor::ViargoOgreHeadTrackingCalibrationMetaphor(const std::string& name, const Ogre::Vector2& windowSize, int pattern, const Ogre::Vector4& offsets, bool start) 
	:Metaphor(name, start)
	,Device(name)
	,_windowSize(windowSize)
	,_patternSize(pattern)
	,_offsets(offsets)
	,_markerSize(Ogre::Vector2(0.01f, 0.01f))
	,_calibrating(false)
	,_calibrated(false)
	,_pause(false)
	,_markerHoldsStill(false)
	,_calibrationProgress(0)
	,_currentPauseTime(0.0f)
	,_sensorId("")
	,_transformationMatrix(0)
	,_queueUpdateOverlays(false)
	,_updateOverlaysState(false)
	,_overlaysActive(false)
{
	// Setup transformation matrix, try to load from file if exists
	_transformationMatrix = new cv::Mat(cv::Mat::eye(3, 4, CV_32FC1));

	_loadCalibrationData();

	// Create colored materials for markers from texture
	Ogre::MaterialPtr mat = Ogre::MaterialManager::getSingleton().create("headtrackingCalibrationRedMat","Essential"); 
	mat->getTechnique(0)->getPass(0)->createTextureUnitState("red.jpg");
	mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
	mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
	mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

	mat = Ogre::MaterialManager::getSingleton().create("headtrackingCalibrationGreenMat","Essential"); 
	mat->getTechnique(0)->getPass(0)->createTextureUnitState("green.jpg");
	mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
	mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
	mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

	mat = Ogre::MaterialManager::getSingleton().create("headtrackingCalibrationBlackMat","Essential"); 
	mat->getTechnique(0)->getPass(0)->createTextureUnitState("black.jpg");
	mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
	mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
	mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

	mat = Ogre::MaterialManager::getSingleton().create("headtrackingCalibrationYellowMat","Essential"); 
	mat->getTechnique(0)->getPass(0)->createTextureUnitState("yellow.jpg");
	mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
	mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
	mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

	// Construct pattern and overlays
	_buildPattern();
}

ViargoOgreHeadTrackingCalibrationMetaphor::~ViargoOgreHeadTrackingCalibrationMetaphor() {
}

bool ViargoOgreHeadTrackingCalibrationMetaphor::onEventReceived(viargo::Event* event) {
	// Accept filtered head tracking events
	{
		FilteredSensorPositionEvent* ev = dynamic_cast<FilteredSensorPositionEvent*>(event);

		if (ev != 0) {
			return true;
		}
	}

	// Accept keyboard events
	{
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			return true;
		}
	}

	return false;
}

void ViargoOgreHeadTrackingCalibrationMetaphor::handleEvent(viargo::Event* event) {
	// Headtracking event
	{
		FilteredSensorPositionEvent* ev = dynamic_cast<FilteredSensorPositionEvent*>(event);
//std::cout << "VIARGO: Filtered Head Tracking Event..." << std::endl;

		if (ev != 0) {
			_handleFilteredPositionEvent(ev);
			return;
		}
	}

	// Keyboard event
	{
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			viargo::KeyEvent& key = *((viargo::KeyEvent*)event);
			_handleKeyboardEvent(key);
			return;
		}
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::update(float timeSinceLastUpdate) {
	// Manage pause
	if (_pause) {
		_currentPauseTime += timeSinceLastUpdate;

		if (_currentPauseTime >= _PAUSE_TIME) {
			_pause = false;
			_currentPauseTime = 0.0f;
		}
	}

	if (_queueUpdateOverlays) {
		if (_overlaysActive) {
			_updateOverlays(_updateOverlaysState);
		}
		else {
			_overlay->hide();
			_backgroundOverlayContainer->hide();
		}

		_queueUpdateOverlays = false;
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_buildPattern() {
	// Horizontal and vertical offets
	float horizontalOffset = _offsets.x + _offsets.z;
	float verticalOffset   = _offsets.y + _offsets.w;

	// Step in horizontal and vertical direction
	double widthStep  = (1.0 - horizontalOffset) / (_patternSize  - 1);
	double heightStep = (1.0 - verticalOffset)   / (_patternSize - 1);

	// Clear old positions
	_screenPositions.clear();
	_screenPositions3D.clear();

	// Build new 2d screen positions
	for (int j = 0; j < _patternSize; j++) {
		for (int i = 0; i < _patternSize; i++) {
			double positionX = _offsets.x + i * widthStep;
			double positionY = _offsets.y + j * heightStep;

			// Add to list
			_screenPositions.push_back(cv::Point2d(positionX, positionY));
		}
	}

	// Transform 2d screen positions into world space relative to window center
	for (unsigned int i = 0; i < _screenPositions.size(); i++) {
		cv::Point3f worldPosition = cv::Point3f(0, 0, 0);

		// NOTE: Alex: Window size is actually not needed, because there is no scaling part in the matrix till now

		// Scale to window size and correct position for [0,0] in window center
		worldPosition.x = _screenPositions[i].x * _windowSize.x - _windowSize.x / 2.0f;
		worldPosition.y = _screenPositions[i].y * _windowSize.y - _windowSize.y / 2.0f;
		
		_screenPositions3D.push_back(worldPosition);
	}

	// Build pattern as overlays
	Ogre::OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();

	// Main overlay, specific overlay containers are inserted here
	_overlay = overlayManager.create("HeadTrackingCalibrationOverlay");
	_overlay->hide();

	_backgroundOverlayContainer = static_cast<Ogre::OverlayContainer*>(overlayManager.createOverlayElement("Panel", "HeadTrackingCalibrationBG"));
	_backgroundOverlayContainer->setPosition(0.0, 0.0);
	_backgroundOverlayContainer->setDimensions(1.0, 1.0);
	_backgroundOverlayContainer->setMaterialName("headtrackingCalibrationBlackMat");
	_backgroundOverlayContainer->hide();

	// Add background first
	_overlay->add2D(_backgroundOverlayContainer);

	char overlayName[100];

	// Build overlay for each marker
	for (unsigned int i = 0; i < _screenPositions.size(); i++) {
		Ogre::Vector2 screenPostion = Ogre::Vector2(_screenPositions[i].x, _screenPositions[i].y);

		sprintf(overlayName, "HeadTrackingCalibration_%d", i);

		Ogre::OverlayContainer* container = static_cast<Ogre::OverlayContainer*>(overlayManager.createOverlayElement("Panel", std::string(overlayName)));
		container->setPosition(screenPostion.x - _markerSize.x / (Ogre::Real)2.0, screenPostion.y - _markerSize.y / (Ogre::Real)2.0);
		container->setDimensions(_markerSize.x, _markerSize.y);
		container->setMaterialName("headtrackingCalibrationRedMat");
		container->hide();

		// Add overlay item
		_overlay->add2D(container);

		// Add to list
		_markers.push_back(container);
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_handleFilteredPositionEvent(FilteredSensorPositionEvent* event) {
	if (_calibrating && _sensorId == "") {
		_sensorId = event->sensorHandle();
	}
	else if (_calibrating && _sensorId != event->sensorHandle()) {
		std::cout << ">> HeadTrackingCalibration: Found more than one active marker, will reset calibration." << std::endl;
		std::cout << ">>  Please restart calibration, and make sure to only use one active marker." << std::endl;

		// Reset all data, stop calibration
		_collectingPoints.clear();
		_worldPositions.clear();

		_calibrationProgress = 0;
		_currentPauseTime    = 0.0f;

		_calibrated  = false;
		_pause       = false;
		_calibrating = false;

		_sensorId = "";
	}

	// Process calibration, marker is holding
	if (!_pause && _calibrating && event->holding()) {
		cv::Point3f position = cv::Point3f(event->x(), event->y(), event->z());
		_updateCalibration(position);

		// If previously marker was not held still, redisplay overlays with correct colors
		if (!_markerHoldsStill) {
			//_updateOverlays(true);
			_queueUpdateOverlays = true;
			_updateOverlaysState = true;
			_markerHoldsStill = true;
		}
	}
	// Marker moving
	else if (!_pause && _calibrating && !event->holding()) {
		// If previously marker was held still, redisplay overlays with correct colors
		if (_markerHoldsStill) {
			//_updateOverlays(false);
			_queueUpdateOverlays = true;
			_updateOverlaysState = false;
			_markerHoldsStill = false;
		}
	}
	// Transform
	else if (_calibrated) {
		// Get current position
		cv::Point3f positionIn = cv::Point3f(event->x(), event->y(), event->z());
		cv::Point3f positionOut = cv::Point3f(0, 0, 0);

		// Transform by matrix
		_transform(positionIn, positionOut);
		
		cv::Point3f transformed = cv::Point3f(positionOut.x, -1.0f * positionOut.y, -1.0f * positionOut.z);

		// Broadcast event
		CalibratedSensorPositionEvent* calibratedEvent = new CalibratedSensorPositionEvent(event->device(), event->sensorHandle(), transformed.x, transformed.y, transformed.z, event->holding());
		//broadcastEvent(calibratedEvent);

		Viargo.metaphor("HeadTracking").handleEvent(calibratedEvent);
		calibratedEvent->drop();

		//std::cout << "POS: " << transformed.x << ", " << transformed.y << ", " << transformed.z << std::endl;
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_handleKeyboardEvent(const viargo::KeyEvent& key) {
	if (key.action() == viargo::KeyEvent::KEY_PRESSED) {
		// Start Calibration process
		if (key.key() == viargo::KeyboardKey::KEY_C) {
			_startCalibration();
		}
		// Jump back one position
		else if (key.key() == viargo::KeyboardKey::KEY_MINUS) {
			// TODO (if needed)
		}
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_startCalibration() {
	// Reset all data
	_collectingPoints.clear();
	_worldPositions.clear();

	_calibrationProgress = 0;
	_currentPauseTime    = 0.0f;

	_sensorId = "";

	_calibrated  = false;
	_pause       = false;
	_markerHoldsStill = false;

	// Mark start
	_calibrating = true;

	// Show overlays, assume moving marker
	//_updateOverlays(false);
	_overlaysActive = true;
	_queueUpdateOverlays = true;
	_updateOverlaysState = false;
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_updateCalibration(const cv::Point3f& position) {
	// Collect current point
	_collectingPoints.push_back(position);

	// Check if collected enough points
	if (_collectingPoints.size() >= _COLLECTING_POINTS) {
		// Calculate average
		cv::Point3f position(0.0, 0.0, 0.0);

		for (unsigned int i = 0; i < _collectingPoints.size(); i++) {
			position.x += _collectingPoints[i].x;
			position.y += _collectingPoints[i].y;
			position.z += _collectingPoints[i].z;
		}

		position.x /= _collectingPoints.size();
		position.y /= _collectingPoints.size();
		position.z /= _collectingPoints.size();

		// Add to collection of real world positions
		_worldPositions.push_back(position);

		// Clear temporary list
		_collectingPoints.clear();

		// Test if finished calibration
		if (_calibrationProgress + 1 == _patternSize * _patternSize) {
			_finalizeCalibration();
		}
		else {
			// Pause for a while
			_pause = true;
			_currentPauseTime = 0.0f;

			// Increment current pass
			_calibrationProgress++;

			// Update overlays with new progress, show current marker yellow
			//_updateOverlays(false);
			_queueUpdateOverlays = true;
			_updateOverlaysState = false;
		}
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_finalizeCalibration() {
	// Build extended positions (calculates average normal plane and adds translated points to list, needed for opencv affine transformation solver)
	_buildExtendedPositions(_screenPositions3D);
	_buildExtendedPositions(_worldPositions);

	// SVD 
	_solveRigidBodyTransformation(_worldPositions, _screenPositions3D);

	// Store transformation matrix in file
	_storeCalibrationData();

	// Reset state
	_calibrating = false;
	_calibrated = true;

	// Hide overlays
	//_overlay->hide();
	//_backgroundOverlayContainer->hide();
	_overlaysActive = false;
	_queueUpdateOverlays = true;

	for (unsigned int i = 0; i < _markers.size(); i++) {
		Ogre::OverlayContainer* currentMarkerOverlay = _markers[i];
		currentMarkerOverlay->hide();
	}

	_worldPositions.clear();
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_updateOverlays(bool state) {
	_overlay->show();
	_backgroundOverlayContainer->show();

	// Update overlay for each marker
	for (unsigned int i = 0; i < _markers.size(); i++) {
		Ogre::OverlayContainer* currentMarkerOverlay = _markers[i];

		if (_calibrationProgress == i) {
			if (state) {
				currentMarkerOverlay->setMaterialName("headtrackingCalibrationGreenMat");
			}
			else {
				currentMarkerOverlay->setMaterialName("headtrackingCalibrationYellowMat");
			}
		}
		else {
			currentMarkerOverlay->setMaterialName("headtrackingCalibrationRedMat");
		}

		currentMarkerOverlay->show();
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_transform(const cv::Point3f& in, cv::Point3f& out) {
	if (_transformationMatrix != 0 && _transformationMatrix->data != 0) {
		cv::Point3f coordinateOut;

		// Multiplicate by transformation matrix
		/*coordinateOut.x = (float)_transformationMatrix->at<double>(0,0) * in.x + 
			(float)_transformationMatrix->at<double>(0,1) * in.y +
			(float)_transformationMatrix->at<double>(0,2) * in.z +
			(float)_transformationMatrix->at<double>(0,3);

		coordinateOut.y = (float)_transformationMatrix->at<double>(1,0) * in.x + 
			(float)_transformationMatrix->at<double>(1,1) * in.y +
			(float)_transformationMatrix->at<double>(1,2) * in.z +
			(float)_transformationMatrix->at<double>(1,3);

		coordinateOut.z = (float)_transformationMatrix->at<double>(2,0) * in.x + 
			(float)_transformationMatrix->at<double>(2,1) * in.y +
			(float)_transformationMatrix->at<double>(2,2) * in.z +
			(float)_transformationMatrix->at<double>(2,3);*/

		coordinateOut.x = _transformationMatrix->at<float>(0,0) * in.x + 
			_transformationMatrix->at<float>(0,1) * in.y +
			_transformationMatrix->at<float>(0,2) * in.z +
			_transformationMatrix->at<float>(0,3);

		coordinateOut.y = (float)_transformationMatrix->at<float>(1,0) * in.x + 
			_transformationMatrix->at<float>(1,1) * in.y +
			_transformationMatrix->at<float>(1,2) * in.z +
			_transformationMatrix->at<float>(1,3);

		coordinateOut.z = (float)_transformationMatrix->at<float>(2,0) * in.x + 
			_transformationMatrix->at<float>(2,1) * in.y +
			_transformationMatrix->at<float>(2,2) * in.z +
			_transformationMatrix->at<float>(2,3);

		out.x = coordinateOut.x;
		out.y = coordinateOut.y;
		out.z = coordinateOut.z;
	}

	//std::cout << "POS: [ " << out.x << ", " << out.y << ", " << out.z << " ]" << std::endl;
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_buildExtendedPositions(std::vector<cv::Point3f>& positions) {
	std::vector<cv::Point3f> normals;

	// Calculate normals of World positions
	for (int c = 0; c < _patternSize; c++) {
		for (int r = 0; r < _patternSize; r++) {
			// Origin point
			cv::Point3f org = positions[c * _patternSize + r];

			// Direction vectors
			cv::Point3f a(0.0f, 0.0f, 0.0f);
			cv::Point3f b(0.0f, 0.0f, 0.0f);

			// Cross product
			cv::Point3f ab(0.0f, 0.0f, 0.0f);

			// Check current case and compute direction vectors a,b
			if (r < _patternSize - 1 && // a = down; b = right
				c < _patternSize - 1) {
					a = positions[(c + 1) * _patternSize + r] - org;
					b = positions[c * _patternSize + r + 1] - org;
			}
			else if (r < _patternSize - 1 && // a = right; b = up
				c == _patternSize - 1) {
					a = positions[c * _patternSize + r + 1] - org;
					b = positions[(c - 1) * _patternSize + r] - org;
			}
			else if (r == _patternSize - 1 && // a = up; b = left
				c == _patternSize - 1) {
					a = positions[(c - 1) * _patternSize + r] - org;
					b = positions[c * _patternSize + r - 1] - org;
			}
			else if (r == _patternSize - 1 && // a = left; b = down
				c < _patternSize - 1) { 
					a = positions[c * _patternSize + r - 1] - org;
					b = positions[(c + 1) * _patternSize + r] - org;
			}

			// Take cross product
			ab.x = a.y*b.z - b.y*a.z; 
			ab.y = b.x*a.z - a.x*b.z; 
			ab.z = b.y*a.x - b.x*a.y;

			// Normalize to 1mm
			float length = sqrt(ab.x*ab.x + ab.y*ab.y + ab.z*ab.z);

			ab.x /= length;
			ab.y /= length;
			ab.z /= length;

			ab.x *= 100;
			ab.y *= 100;
			ab.z *= 100;

			// Add direction to origin
			org += ab;

			// Insert into list
			normals.push_back(ab);
		}
	}

	// Take average
	cv::Point3f avg(0.0f, 0.0f, 0.0f);

	for each(cv::Point3f p in normals) {
		avg += p;
	}

	avg.x = avg.x / normals.size();
	avg.y = avg.y / normals.size();
	avg.z = avg.z / normals.size();

	normals.clear();

	for each (cv::Point3f p in positions) {
		normals.push_back(p + avg);
	}

	for each(cv::Point3f p in normals) {
		positions.push_back(p);
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_loadCalibrationData() {
	// Load transformation matrix from file
	cv::FileStorage fs("headtracking_calibration.yml", cv::FileStorage::READ);

	if (!fs["transformation"].empty()) {
		fs["transformation"] >> *_transformationMatrix;
		_calibrated = true;
	}
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_storeCalibrationData() {
	assert(_transformationMatrix != 0);

	// Store transformation matrix in file
	cv::FileStorage fs("headtracking_calibration.yml", cv::FileStorage::WRITE);
	fs << "transformation" << *_transformationMatrix;

	std::ofstream fileOutStream;
	fileOutStream.open("head_tracking_matrix.txt");
	for (size_t y = 0; y < 4; ++y) {
		for (size_t x = 0; x < 4; ++x) {
			fileOutStream << _transformationMatrix->at<float>(y,x) << " ";
		}
	}
	fileOutStream.close();
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_solveRigidBodyTransformation(const std::vector<cv::Point3f>& positionsIn, const std::vector<cv::Point3f>& positionsOut) {
	// Base on http://nghiaho.com/?page_id=671 and
	//  http://www.kwon3d.com/theory/jkinem/rotmat.html and
	//  http://igl.ethz.ch/projects/ARAP/svd_rot.pdf

	// Assertions
	assert(positionsIn.size() == positionsOut.size());
	assert(positionsIn.size() > 3);

	int size = positionsIn.size();
	delete _transformationMatrix;
	// 3x3 Part of rotation + right column for translation
	_transformationMatrix = new cv::Mat(3, 4, CV_32FC1, cv::Scalar::all(0));

	// 1. Find centroids of point clouds (without last normal)
	cv::Point3f centerIn, centerOut;

	centerOut = cv::Point3f(0,0,0);
	centerIn  = cv::Point3f(0,0,0);

	for (int i = 0; i < size / 2; i++) {
		centerIn  += positionsIn[i];
		centerOut += positionsOut[i];
	}

	centerIn.x  /= size / 2;
	centerIn.y  /= size / 2;
	centerIn.z  /= size / 2;

	// 2. Find rotation from in to out
	float H[9];
	for (int j = 0; j < 9; j++) {
		H[j] = 0.0f;
	}

	// H = [SUM_{size} of (P_A_i - center_A) * (P_B_i - center_B)^T]
	for (int i = 0; i < size; i++) {
		// openCV Points
		cv::Point3f cvA = positionsIn[i]  - centerIn;
		cv::Point3f cvB = positionsOut[i] - centerOut;

		// Convert to float array to access via index
		float A[3], B[3];

		A[0] = cvA.x;
		A[1] = cvA.y;
		A[2] = cvA.z;

		B[0] = cvB.x;
		B[1] = cvB.y;
		B[2] = cvB.z;
		
		// Calculate matrix part
		for (int r = 0; r < 3; r++) {
			for (int c = 0; c < 3; c++) {
				float value = A[c] * B[r];

				// Add to matrix
				H[r * 3 + c] += value;
			}
		}
	}

	// Convert float matrix to openCV Matrix
	cv::Mat cvH = cv::Mat(3, 3, CV_32FC1, cv::Scalar::all(0));

	for (int c = 0; c < 3; c++)
		for (int r = 0; r < 3; r++)
			cvH.at<float>(r,c) = H[r * 3 + c];

	// SVD Decompostion
	cv::SVD svd(cvH, cv::SVD::MODIFY_A | cv::SVD::FULL_UV);

	// Rotation matrix
	cv::Mat V = svd.vt.t();
	cv::Mat U = svd.u;
	cv::Mat D = cv::Mat::eye(3, 3, CV_32FC1); // Avoiding reflection matrix (?)
	D.at<float>(2, 2) = (float)cv::determinant(V * U.t());

	cv::Mat R = V * D * U.t();

	// 3. Find optimal translation from in to out
	cv::Point3f T;
	_openCVMultiply(R, centerIn, T);
	T = centerOut - T;

	// 4. Assign to transformation matrix
	// Rotation part
	for (int c = 0; c < 3; c++) {
		for (int r = 0; r < 3; r++) {
			_transformationMatrix->at<float>(r, c) = R.at<float>(r, c);
		}
	}

	// Translation part
	_transformationMatrix->at<float>(0, 3) = T.x;
	_transformationMatrix->at<float>(1, 3) = T.y;
	_transformationMatrix->at<float>(2, 3) = T.z;
}

void ViargoOgreHeadTrackingCalibrationMetaphor::_openCVMultiply(const cv::Mat& mat, const cv::Point3f& point, cv::Point3f& out) {
	cv::Point3f coordinateOut;

	// Multiplicate by transformation matrix
	coordinateOut.x = mat.at<float>(0,0) * point.x + 
		mat.at<float>(0,1) * point.y +
		mat.at<float>(0,2) * point.z;

	coordinateOut.y = mat.at<float>(1,0) * point.x + 
		mat.at<float>(1,1) * point.y +
		mat.at<float>(1,2) * point.z;

	coordinateOut.z = mat.at<float>(2,0) * point.x + 
		mat.at<float>(2,1) * point.y +
		mat.at<float>(2,2) * point.z;



	out.x = coordinateOut.x;
	out.y = coordinateOut.y;
	out.z = coordinateOut.z;
}

} // namespace viargo