
#ifndef HEADTRACKING_METAPHOR_H
#define HEADTRACKING_METAPHOR_H

// base app
#include "BaseApplication.h"
//#include "Tracking/asymmetric frustum.h"
#include "Tracking/viargo_ogre_head_tracking_calibration_metaphor.h"
#include "globals.h"


class HeadTrackingMetaphor : public viargo::Metaphor {

protected:
	viargo::vec3f newPosition;
	Ogre::Vector3 incomingSensorEvents[4];
	bool positionUpdated;
	Ogre::Vector3 camValues[6]; //pos, view, up, leftEye, rightEye, Player2Pos
	int sensorEventCount;
public:

	// ctor
	HeadTrackingMetaphor(std::string name)
		: Metaphor(name),
		newPosition(0, 0, 60),
		positionUpdated(true),
		sensorEventCount(0)
	{
		incomingSensorEvents[0] = (-3.5, 68, 78);
		incomingSensorEvents[1] = (4.4, 55, 65);
		incomingSensorEvents[2] = (-8.8, 53, 65);
	}

	// we only need the Filtered Head Tracking Sensor events
	virtual bool onEventReceived(viargo::Event* event) {
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			return true;
		}

		if (typeid(*event) == typeid(viargo::CalibratedSensorPositionEvent)) {
			return true;
		}
		return false;
	}

	void sortVectors(Ogre::Vector3* incoming){ //sortiert drei Vektoren nach links, rechts, oben 
		//(Vektoren duerfen nicht in einer Ebene liegen, der up vektor hat längeren Abstand zu den beiden anderen Punkten als diese untereinander).
		Ogre::Real d1 = incoming[0].squaredDistance(incoming[1]);  //squaredDistance, da nur Laengen verglichen werden.
		Ogre::Real d2 = incoming[1].squaredDistance(incoming[2]);
		Ogre::Real d3 = incoming[2].squaredDistance(incoming[0]);

		
		


		//Schreibe den up Vector (der zu den beiden anderen längeren Abstand hat) an die letzte Stelle des Arrays

		if ((d1<=d2) && (d1<=d3)){
			//upVector steht schon an der zweiten Stelle
		}
		else if ((d2<=d3) && (d2<=d1)){
			incoming[0].swap(incoming[2]);
		}
		else if ((d3<=d1) && (d3<=d2)){
			incoming[1].swap(incoming[2]);
		}

		//Distanzen von Left bzw Right (noch unbekannt) zum Up Vector
		Ogre::Vector3 rightAndLeft[2]; 
		rightAndLeft[0] = incoming[0]-incoming[2];
		rightAndLeft[1] = incoming[1]-incoming[2];

		//Bestimmen der Reihenfolge über Vorzeichen des Kreuzproduktes
		Ogre::Vector3 normal = rightAndLeft[0].crossProduct(rightAndLeft[1]);

		if (normal.z > 0){
			incoming[0].swap(incoming[1]);
		}


	}


	void refreshCamValues(Ogre::Vector3* trackValues){	// trackValues sortiert nach left, right, up (und u.U. Player2Position)

		//Hilfsvektoren
		Ogre::Vector3 leftToRight = (trackValues[1]-trackValues[0]);
		Ogre::Vector3 leftToUp = (trackValues[2]-trackValues[0]);

		Ogre::Vector3 normal = (leftToRight.crossProduct(leftToUp)).normalisedCopy();
		Ogre::Vector3 middle = (0.5f)*(trackValues[0]+trackValues[1]);

		//Parameter hier spaeter experimentell feste Werte
		Ogre::Real DOWNWARDS = 10; // leftToRight.length()*(0.7f);
		//Ogre::Real backwards = 0; // (trackValues[2]-middle).length()*(0.25);
		//Ogre::Real viewUp = leftToRight.length()*(0.8f);

		//Punkt zwischen den Augen ("pos")
		camValues[0] = middle + DOWNWARDS*normal; // + backwards*(trackValues[2]-middle);
		if (camValues[0].z<0.1f){
			camValues[0].z=0.1f;
		}

		//Sichtvektor ("look")
		camValues[1] = (middle-trackValues[2]); //-viewUp*normal;
		camValues[1].normalise();
		//Orientierung nach Oben senkrecht zum view ("up")
		camValues[2] = leftToRight.crossProduct(camValues[1]);
		camValues[2].normalise();

		//linkes Auge und rechtes Auge
		camValues[3] = camValues[0]-leftToRight.normalisedCopy()*6.5f/2;
		camValues[4] = camValues[0]+leftToRight.normalisedCopy()*6.5f/2;
		if (camValues[3].z<0.1f){
			camValues[3].z=0.1f;
		}
		if (camValues[4].z<0.1f){
			camValues[4].z=0.1f;
		}
	}

	viargo::vec3f leftEyePos(){
		if (global::twoPlayersActive){
			return viargo::vec3f(camValues[5].x, camValues[5].y*-1, camValues[5].z);
		} else {
			return viargo::vec3f(camValues[3].x, camValues[3].y*-1, camValues[3].z);
		}
	}

	viargo::vec3f rightEyePos(){
		return viargo::vec3f(camValues[4].x,camValues[4].y*-1, camValues[4].z);
	}

	viargo::vec3f upVector(){
		return viargo::vec3f(camValues[2].x,camValues[2].y, camValues[2].z);
	}

	viargo::quat orientation(){
		Ogre::Vector3 dirRight= camValues[1].crossProduct(camValues[2]);
		Ogre::Quaternion ori;
		ori.FromRotationMatrix(Ogre::Matrix3( dirRight.x,	  dirRight.y,		dirRight.z,
			camValues[2].x, camValues[2].y, camValues[2].z,
			camValues[1].x, camValues[1].y, camValues[1].z
			));

		return viargo::quat(ori.w, ori.x, ori.y, ori.z);


	}

	// Gets called for handling of an event if onEventReceived(...) returned true
	// @param: event	the event to be handled
	virtual void handleEvent(viargo::Event* event) {
		// handle key events
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			viargo::KeyEvent& key = *((viargo::KeyEvent*)event);

		
		}

		// handle Sensor-Events
		if (typeid(*event) == typeid(viargo::CalibratedSensorPositionEvent)) {
			viargo::CalibratedSensorPositionEvent& se = *((viargo::CalibratedSensorPositionEvent*)event);

		
			// Castet die einkommende ID von String nach Integer
			std::stringstream temp;
			temp << se.sensorHandle();
			int id;
			temp >> id;

			incomingSensorEvents[id] = Ogre::Vector3( se.x(), se.y(), se.z() );
			positionUpdated = true;

		}



		else {
			// panic! we should never be here!
		}
	}

	// Gets called from Viargo.update() to handle frame-specific actions
	// @param:  timeSinceLastUpdate - the time since the last call of the function
	//								  in milliseconds
	virtual void update(float timeSinceLastUpdate) {
		if (positionUpdated) {

			//Kopiere die letzten drei (4) Vektoren in ein neues Array, damit beim späteren Rechnen nicht dazwischengepfuscht wird.
			Ogre::Vector3 toUpdate[4];
			for (int i = 0; i<3; ++i){
				toUpdate[i] = incomingSensorEvents[i];
			}

			if (global::twoPlayersActive){
				toUpdate[3] = incomingSensorEvents[3];
			}

			sortVectors(toUpdate);
			refreshCamValues(toUpdate);

			// Setzen der Viargo-Kameraeinstellungen

			viargo::Camera& viargoCamera = Viargo.camera("main");
		

			positionUpdated = false;
		}
	}

};

#endif //HEADTRACKING_METAPHOR_H 


