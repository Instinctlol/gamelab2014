#include "tracking/viargo_head_tracking_filter_metaphor.h"

namespace viargo {

// -----------------------------------------------------------------------------------------
// Static members
// -----------------------------------------------------------------------------------------
const double ViargoHeadTrackingFilterMetaphor::_MARKER_IS_NOISE_MIN_DISTANCE    = 100.0;
const double ViargoHeadTrackingFilterMetaphor::_MARKER_HOLDS_STILL_MIN_DISTANCE = 1.0;
const double ViargoHeadTrackingFilterMetaphor::_EPS                             = 0.00001;
const double ViargoHeadTrackingFilterMetaphor::_LOW_PASS_FILTER_FACTOR          = 0.0;// 0.1;
const double ViargoHeadTrackingFilterMetaphor::_LOW_PASS_2_FILTER_FACTOR        = 0.0;//0.5;
// -----------------------------------------------------------------------------------------

ViargoHeadTrackingFilterMetaphor::ViargoHeadTrackingFilterMetaphor(const std::string& name, bool start)
	:Metaphor(name, start)
	,Device(name)
{
	// Initialize data
	for (int i = 0; i < _MARKER_MAXIMAL_SENSOR_ID; i++) {
		_counterHoldsStill[i]  = 0;
		_counterActivation[i]  = 0;
		_counterDisappeared[i] = 0;
		_counterNoise[i]       = 0;
		_holdStill[i]          = false;
		_isActive[i]           = false;
	}
}

ViargoHeadTrackingFilterMetaphor::~ViargoHeadTrackingFilterMetaphor() {
}

bool ViargoHeadTrackingFilterMetaphor::onEventReceived(viargo::Event* event) {
	SensorPositionEvent* ev = dynamic_cast<SensorPositionEvent*>(event);

	if (ev != 0) {
		return true;
	}

	return false;
}

void ViargoHeadTrackingFilterMetaphor::handleEvent(viargo::Event* event) {
	SensorPositionEvent* ev = dynamic_cast<SensorPositionEvent*>(event);
	if (ev == 0) {
		return;
	}

	// Current sensor ID
	std::stringstream sensorStr(ev->sensorHandle()); 
	int id;
	sensorStr >> id;

	// Current position in cm
	vec3d pos(ev->x() * 100.0, ev->y() * 100.0, ev->z() * 100.0);

	// Cleanup list
	while (_positions[id].size() > 10) {
		_positions[id].pop_back();
	}
	
	// Check if marker is new
	if (_positions[id].size() == 0) {
		// Add first position for current marker
		_positions[id].push_front(pos);
	}
	// Check activation possibility for inactive markers
	else if (!_isActive[id]) {
		// Last position
		vec3d posOld = _positions[id][0];

		// Distance between last and current position
		double dist = viargo::Math::abs<double>( (pos.x - posOld.x) + (pos.y - posOld.y) + (pos.z - posOld.z));

		// Marker moved, increment activation possibility
		if (dist > _EPS) {
			_counterActivation[id] ++;
		}
		else {
			// Marker did not move, punish possibility with decrement
			_counterActivation[id] --;
			if (_counterActivation[id] < 0) {
				_counterActivation[id] = 0;
			}
		}

		// If enough positions were okay, activate marker
		if (_counterActivation[id] > _MARKER_ACTIVATION_AMOUNT) {
			_counterActivation[id] = 0;
			_isActive[id]          = true;
		}

		// Add position unfiltered
		_positions[id].push_front(pos);
	}
	// If active but did not move about EPS, check disappearance possibility
	else if (_isActive[id]) {
		// Last position
		vec3d posOld = _positions[id][0];

		// Distance between last and current position
		double dist = viargo::Math::abs<double>( (pos.x - posOld.x) + (pos.y - posOld.y) + (pos.z - posOld.z));

		// Marker did not move under EPS, increment disapearance possibility
		if (dist < _EPS) {
			_counterDisappeared[id] ++;
		}
		else {
			// Marker moved, reset counter
			_counterDisappeared[id] = 0;
		}

		// If enough positions were under EPS, deactivate marker and reset all dependent state
		if (_counterDisappeared[id] > _MARKER_DISAPPEARED_AMOUNT) {
			_counterDisappeared[id] = 0;
			_counterActivation[id]  = 0;
			_counterHoldsStill[id]  = 0;
			_counterNoise[id]       = 0;
			_isActive[id]           = false;
			_holdStill[id]          = false;
			_positions[id].clear();
		}

		// Test if marker does not move about rough distance, but more than EPS
		if (dist < _MARKER_HOLDS_STILL_MIN_DISTANCE && dist > _EPS) {
			_counterHoldsStill[id] ++;
		}
		else {
			// Marker moved, reset counter
			_counterHoldsStill[id] = 0;

			// Reset flag
			_holdStill[id] = false;
		}

		// If enough positions were hold still, set flag
		if (_counterHoldsStill[id] > _MARKER_HOLDS_STILL_AMOUNT) {
			_holdStill[id] = true;
		}

		// Test if marker did move too much, indicates noise
		if (dist > _MARKER_IS_NOISE_MIN_DISTANCE) {
			_counterNoise[id] ++;
		}
		else {
			// Marker not moved so much, reset counter
			_counterNoise[id] = 0;
		}

		// If too much noise, we have to reset the state in order to avoid 'dead lock' when moving marker by intent too fast
		if (_counterNoise[id] > _MARKER_NOISE_AMOUNT) {
			_counterDisappeared[id] = 0;
			_counterActivation[id]  = 0;
			_counterHoldsStill[id]  = 0;
			_counterNoise[id]       = 0;
			_isActive[id]           = false;
			_holdStill[id]          = false;
			_positions[id].clear();
		}

		// If marker is holding still, flat out position
		if (_holdStill[id]) {
			// Apply stronger filter
			pos = pos + _LOW_PASS_2_FILTER_FACTOR * (posOld-pos);
		}
		else {
			// Apply filter
			pos = pos + _LOW_PASS_FILTER_FACTOR * (posOld-pos);
		}

		// Add position, if not affected by noise
		if (_counterNoise[id] == 0) {
			_positions[id].push_front(pos);
		}
		
	}

	// Fire event if current marker is active
	if (_isActive[id]) {
		viargo::FilteredSensorPositionEvent* filteredEvent = new viargo::FilteredSensorPositionEvent(ev->device(), ev->sensorHandle(), (float)pos.x, (float)pos.y, (float)pos.z, _holdStill[id]);
		//broadcastEvent(filteredEvent);
		Viargo.metaphor("HeadtrackingCalibration-Metaphor").handleEvent(filteredEvent);
		filteredEvent->drop();
	}
}

void ViargoHeadTrackingFilterMetaphor::update(float timeSinceLastUpdate) {
}

} // namespace viargo
