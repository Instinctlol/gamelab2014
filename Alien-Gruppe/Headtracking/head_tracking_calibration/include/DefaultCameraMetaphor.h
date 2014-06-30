
#ifndef VIARGO_DEFAULT_CAMERA_METAPHOR_H
#define VIARGO_DEFAULT_CAMERA_METAPHOR_H

// base app
#include "BaseApplication.h"

// bullet physics
#include "OgreBulletDynamicsRigidBody.h"
#include "Shapes/OgreBulletCollisionsStaticPlaneShape.h" // for static planes
#include "Shapes/OgreBulletCollisionsBoxShape.h"		  // for Boxes


class DefaultCameraMetaphor : public viargo::Metaphor {

public:

	// ctor
	DefaultCameraMetaphor(std::string name, std::string camName = "main", float topSpeed = 100.0f, 
		viargo::vec3f defaultUp = viargo::vec3f::unitY)

		:Metaphor(name),
		_cameraName(camName),
		_defaultUp(defaultUp),
		_topSpeed(topSpeed),
		_velocity(0.0f),
		_goingForward(false),
		_goingBack(false),
		_goingLeft(false),
		_goingRight(false),
		_goingUp(false),
		_goingDown(false),
		_fastMove(false)
	{
	
	}
	
	// we only need the keyboard & mouse events
	virtual bool onEventReceived(viargo::Event* event) {
		
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			return true;
		}

		if (typeid(*event) == typeid(viargo::MouseEvent)) {
			return true;
		}
		
		return false;
	}
	
	// Gets called for handling of an event if onEventReceived(...) returned true
	// @param: event	the event to be handled
	virtual void handleEvent(viargo::Event* event) {
	
		// handle key events
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			viargo::KeyEvent& ke = *((viargo::KeyEvent*)event);
			// key pressed
			if (ke.action() == viargo::KeyEvent::KEY_PRESSED) {
				if (ke.key() == viargo::KeyboardKey::KEY_W || ke.key() == viargo::KeyboardKey::KEY_UP) 
					_goingForward = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_S || ke.key() == viargo::KeyboardKey::KEY_DOWN) 
					_goingBack = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_A || ke.key() == viargo::KeyboardKey::KEY_LEFT) 
					_goingLeft = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_D || ke.key() == viargo::KeyboardKey::KEY_RIGHT) 
					_goingRight = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_PAGEUP) 
					_goingUp = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_PAGEDOWN) 
					_goingDown = true;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_SHIFT) 
					_fastMove = true;
			}
			// key released
			else if (ke.action() == viargo::KeyEvent::KEY_RELEASED) {
				if (ke.key() == viargo::KeyboardKey::KEY_W || ke.key() == viargo::KeyboardKey::KEY_UP) 
					_goingForward = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_S || ke.key() == viargo::KeyboardKey::KEY_DOWN) 
					_goingBack = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_A || ke.key() == viargo::KeyboardKey::KEY_LEFT) 
					_goingLeft = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_D || ke.key() == viargo::KeyboardKey::KEY_RIGHT) 
					_goingRight = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_PAGEUP) 
					_goingUp = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_PAGEDOWN) 
					_goingDown = false;
				
				else if (ke.key() == viargo::KeyboardKey::KEY_SHIFT) 
					_fastMove = false;
			}

			// nop
			else {
				// panic! we should never be here!
			}

		
		}

		// mouse events
		else if (typeid(*event) == typeid(viargo::MouseEvent)) {
		
			viargo::MouseEvent& me = *((viargo::MouseEvent*)event);
			viargo::Camera& cam = Viargo.camera(_cameraName);

			// mouse moved
			if (me.action() == viargo::MouseEvent::MOTION) {
				cam.rotate(viargo::quatf(_defaultUp, -me.x() * 0.015f));
				cam.rotate(viargo::quatf(cam.strafe(), -me.y() * 0.015f));
			}
		}

		// nop
		else {
			// panic! we should never be here!
		}
	}

	// Gets called from Viargo.update() to handle frame-specific actions
	// @param:  timeSinceLastUpdate - the time since the last call of the function
	//								  in milliseconds
	virtual void update(float timeSinceLastUpdate) {
		// build our acceleration vector based on keyboard input composite
		viargo::vec3f accel = viargo::vec3f::zero;
		viargo::Camera& cam = Viargo.camera(_cameraName);

		if (_goingForward) 
			accel += cam.direction();
		else if (_goingBack) 
			accel -= cam.direction();
		
		if (_goingRight) 
			accel += cam.strafe();
		else if (_goingLeft) 
			accel -= cam.strafe();
		
		if (_goingUp) 
			accel += cam.up();
		else if (_goingDown) 
			accel -= cam.up();

		// if accelerating, try to reach top speed in a certain time
		float topSpeed = _fastMove ? _topSpeed * 20 : _topSpeed;
		if (accel != viargo::vec3f::zero) {
			_velocity += viargo::normalize(accel) * (topSpeed * timeSinceLastUpdate * 0.01f);
		}
		// if not accelerating, try to stop in a certain time
		else {
			_velocity -= _velocity * (timeSinceLastUpdate * 0.01f);
		}

		float tooSmall = std::numeric_limits<float>::epsilon();

		// keep camera velocity below top speed and above epsilon
		if (viargo::squaredLength(_velocity) > topSpeed * topSpeed) {
			_velocity = viargo::normalize(_velocity) * topSpeed;
		}
		else if (viargo::squaredLength(_velocity) < tooSmall * tooSmall) {
			_velocity = viargo::vec3f::zero;
		}

		if (_velocity != viargo::vec3f::zero) {
			cam.setPosition(cam.position() + _velocity * timeSinceLastUpdate / 1000.f);
			cam.setTarget(cam.target() + _velocity * timeSinceLastUpdate / 1000.f);
		}
	}

protected:
	std::string _cameraName;
	viargo::vec3f _defaultUp;
	float _topSpeed;
	viargo::vec3f _velocity;
	bool _goingForward;
	bool _goingBack;
	bool _goingLeft;
	bool _goingRight;
	bool _goingUp;
	bool _goingDown;
	
	bool _fastMove;
};

#endif //VIARGO_DEFAULT_CAMERA_METAPHOR_H 


