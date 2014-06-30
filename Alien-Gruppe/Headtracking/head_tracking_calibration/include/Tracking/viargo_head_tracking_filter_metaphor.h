#ifndef __VIARGO_HEAD_TRACKER_FILTER_METAPHOR_H__
#define __VIARGO_HEAD_TRACKER_FILTER_METAPHOR_H__

#include <viargo.h>
#include "event/sensorevent.h"

#include <queue>

namespace viargo {

// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------

/// Filtered head tracking position data
///
class FilteredSensorPositionEvent : public SensorEvent {
public:
	// Constructor
	FilteredSensorPositionEvent(const std::string& device, const std::string& sensorHandle, float x, float y, float z, bool holding = false) 
		:SensorEvent(device, sensorHandle)
		,_x(x)
		,_y(y)
		,_z(z) 
		,_holding(holding)
	{
	}

	// Destructor
	virtual ~FilteredSensorPositionEvent() {}

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

/// Filters ehad tracking data
///
class ViargoHeadTrackingFilterMetaphor : public Metaphor, public Device {

public:
	/// ctor
	///
	ViargoHeadTrackingFilterMetaphor(const std::string& name, bool start = true);

	/// dtor
	///
	virtual ~ViargoHeadTrackingFilterMetaphor();

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
	/// Minimum distance current position has to be from latest one, to be classified as 'still holding'
	///
	static const double _MARKER_HOLDS_STILL_MIN_DISTANCE;

	/// If marker appears for first time and in specific time is in this distance from old positions
	/// we classify the marker as 'noise' and ignore him
	///
	static const double _MARKER_IS_NOISE_MIN_DISTANCE;

	/// Low pass filter factor
	///
	static const double _LOW_PASS_FILTER_FACTOR;

	/// Low pass filter factor, applying when marker holds still to decrease noise
	///
	static const double _LOW_PASS_2_FILTER_FACTOR;

	/// Minimum amount of 'ok' positions, to classify as active
	///
	static const int _MARKER_ACTIVATION_AMOUNT = 10;	

	/// Minimum amount of 'still hold' positions, to set the flag 'still holding marker'
	///
	static const int _MARKER_HOLDS_STILL_AMOUNT = 100;

	/// Minimum amount of dist<EPS positions, to erase the marker
	///
	static const int _MARKER_DISAPPEARED_AMOUNT = 50;

	/// Minimum amount of dist > NOISE positions, to reset marker
	///
	static const int _MARKER_NOISE_AMOUNT = 100;

	/// Maximal amount of sensor IDs 
	///
	static const int _MARKER_MAXIMAL_SENSOR_ID = 10;

	/// Epsilon
	///
	static const double _EPS;


protected:
	/// Counts positions, classified as still holding
	//
	int _counterHoldsStill[_MARKER_MAXIMAL_SENSOR_ID];

	/// Counts positions, before activating a marker
	//
	int _counterActivation[_MARKER_MAXIMAL_SENSOR_ID];

	/// Counts positions, before deactivating a marker
	//
	int _counterDisappeared[_MARKER_MAXIMAL_SENSOR_ID];

	/// Counts positions, before deactivating a marker
	//
	int _counterNoise[_MARKER_MAXIMAL_SENSOR_ID];

	/// Flag if marker is hold still
	///
	bool _holdStill[_MARKER_MAXIMAL_SENSOR_ID];
	
	/// Flag that marker is active
	///
	bool _isActive[_MARKER_MAXIMAL_SENSOR_ID];

	/// Queue for all positions
	///
	std::deque<viargo::vec3d> _positions[_MARKER_MAXIMAL_SENSOR_ID];

}; // class ViargoHeadTrackingFilterMetaphor

} // namespace viargo

#endif // __VIARGO_HEAD_TRACKER_FILTER_METAPHOR_H__