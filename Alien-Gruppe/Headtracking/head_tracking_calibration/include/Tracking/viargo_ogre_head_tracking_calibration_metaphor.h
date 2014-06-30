#ifndef __VIARGO_OGRE_HEAD_TRACKING_CALIBRATION_METAPHOR_H__
#define __VIARGO_OGRE_HEAD_TRACKING_CALIBRATION_METAPHOR_H__

#include <vector>

#include <viargo.h>
#include "event/sensorevent.h"

#include "tracking/viargo_head_tracking_filter_metaphor.h"

// Forward declaration
namespace Ogre {
	class Overlay;
	class OverlayContainer;
	class Camera;
}

namespace cv {
	template <typename T> class Point3_;
	template <typename T> class Point_;
	typedef Point3_<float> Point3f;
	typedef Point_<float> Point2f;
	class Mat;
}

namespace viargo {

// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------

/// Calibrated head tracking position data
///
class CalibratedSensorPositionEvent : public SensorEvent {
public:
	// Constructor
	CalibratedSensorPositionEvent(const std::string& device, const std::string& sensorHandle, float x, float y, float z, bool holding = false) 
		:SensorEvent(device, sensorHandle)
		,_x(x)
		,_y(y)
		,_z(z) 
		,_holding(holding)
	{
	}

	// Destructor
	virtual ~CalibratedSensorPositionEvent() {}

	// Getter
	float x() const  {return _x;}
	float y() const  {return _y;}
	float z() const  {return _z;}
	vec3 pos() const {return vec3(_x, _y, _z);}

	/// Returns holding status
	///
	bool holding() const { return _holding; }

private:
	float _x;
	float _y;
	float _z;

	/// If true, current position was nearly the same values for specific period of time
	///
	bool _holding;
};

// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------

class ViargoOgreHeadTrackingCalibrationMetaphor : public Metaphor, public Device {
public:
	/// ctor
	///
	ViargoOgreHeadTrackingCalibrationMetaphor(const std::string& name, const Ogre::Vector2& windowSize, int pattern = 4, const Ogre::Vector4& offsets = Ogre::Vector4(0.1f, 0.1f, 0.1f, 0.1f), bool start = true);
	
	/// dtor
	///
	virtual ~ViargoOgreHeadTrackingCalibrationMetaphor();

	/// Called by all events
	///
	bool onEventReceived(viargo::Event* event);

	/// Called by responsible events
	///
	void handleEvent(viargo::Event* event);

	/// Called by engine
	///
	void update(float timeSinceLastUpdate);

private:
	/// If true, calibration is accessible
	///
	bool _calibrated;

	/// If true, calibration is currently in progress
	///
	bool _calibrating;

	/// If true, calibration is making a pause to give user time to reposition marker
	///
	bool _pause;

	/// Current collected time for pause
	///
	float _currentPauseTime;

	/// Pause time
	///
	static const float _PAUSE_TIME;

	/// If true, marker holds still
	///
	bool _markerHoldsStill;

	/// Size of window (unit is not realy important, just note that calibrated positions will have this given unit at the end)
	///
	Ogre::Vector2 _windowSize;

	/// Size of markers
	///
	Ogre::Vector2 _markerSize;

	/// X/Y Pattern size
	///
	int _patternSize;

	/// X/Y Offsets
	///
	Ogre::Vector4 _offsets;

	/// Overlay for markers
	///
	Ogre::Overlay* _overlay;

	/// Background overlay to hide scene
	///
	Ogre::OverlayContainer* _backgroundOverlayContainer;

	/// Collection of markers as overlay containers
	///
	std::vector<Ogre::OverlayContainer*> _markers;

	/// Positions of visualizations on screen in screenspace
	///
	std::vector<cv::Point2f> _screenPositions;

	/// Positions of visualizations on screen in worldspace
	///
	std::vector<cv::Point3f> _screenPositions3D;

	/// Positions of marker in real world on visualizations
	///
	std::vector<cv::Point3f> _worldPositions;

	/// Progress of calibration
	///
	int _calibrationProgress;

	/// Sensor id of calibration marker
	///
	std::string _sensorId;

	/// Temporary list of points, used to calculate average over time during calibrating specific marker point
	///
	std::vector<cv::Point3f> _collectingPoints;

	/// Amount of points, that need to be collected for one point
	///
	static const unsigned int _COLLECTING_POINTS = 100;

	/// Computed transformation matrix
	///
	cv::Mat* _transformationMatrix;

	// ---------------------------------------------------------------------------------

	/// Build overlay pattern
	///
	void _buildPattern();

	/// Handle filtered ehad tracking data
	///
	void _handleFilteredPositionEvent(FilteredSensorPositionEvent* event);

	/// Handle key board data
	///
	void _handleKeyboardEvent(const viargo::KeyEvent& key);

	/// Initializes calibration
	///
	void _startCalibration();

	/// Updates calibration
	///
	void _updateCalibration(const cv::Point3f& position);

	/// Finalizes calibration
	///
	void _finalizeCalibration();

	/// Transforms data by calibration
	///
	void _transform(const cv::Point3f& in, cv::Point3f& out);

	/// Update overlay screen to current calibration progress
	///
	/// @param state If true, marker is holding still (green overlay), if false marker is moving (yellow overlay)
	///
	void _updateOverlays(bool state);

	bool _queueUpdateOverlays;
	bool _updateOverlaysState;
	bool _overlaysActive;

	/// Build extended positions, needed for opencv affine transformation solver
	///
	void _buildExtendedPositions(std::vector<cv::Point3f>& positions);

	/// Loads calibration data from file
	///
	void _loadCalibrationData();

	/// Stores calibration data to file
	///
	void _storeCalibrationData();

	/// Solve rigid body transformtaion
	///
	void _solveRigidBodyTransformation(const std::vector<cv::Point3f>& positionsIn, const std::vector<cv::Point3f>& positionsOut);

	/// Helper to multiply matrix with vector in opencv
	///
	void _openCVMultiply(const cv::Mat& mat, const cv::Point3f& point, cv::Point3f& out);

}; // ViargoOgreHeadTrackingCalibrationMetaphor 

}; // namespace viargo

#endif // __VIARGO_OGRE_HEAD_TRACKING_CALIBRATION_METAPHOR_H__