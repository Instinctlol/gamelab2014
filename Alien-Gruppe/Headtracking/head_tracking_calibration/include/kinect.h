#ifndef KINECT_H
#define KINECT_H

// base app
#include <fstream>
#include "BaseApplication.h"
#include "globals.h"
#include "DynamicRenderable.h"
#include "DynamicLines.h"
#include "SdkTrays.h"
#include "OgreBulletCollisionMetaphor.h"
#include <opencv2/imgproc/imgproc.hpp>
#include "background_subtraction.h"
#include "viargo_ogre_kinect_tracking_calibration_metaphor.h"
#include <opencv2/highgui/highgui.hpp>

#include "kinect_wrapper.h"
#include "DefaultCameraMetaphor.h"
#include "MouseMetaphor.h"
#include "TouchMetaphor.h"
#include "background_subtraction.h"
#include "viargo_ogre_kinect_tracking_calibration_metaphor.h"

using namespace cv;
using namespace std;

class KinectMetaphor {

protected:
	Ogre::SceneManager*  mSceneMgr;
	OgreBulletCollisionMetaphor* _mCollisionMetaphor;
	std::deque< OgreBulletDynamics::RigidBody* >* _rigidBodies;
	OgreBulletDynamics::RigidBody *rb[4];
	Ogre::SceneNode *schatten[4];
	Ogre::SceneNode *hand;
	Ogre::Vector3 oldHandPos;
	Ogre::Vector3 newHandPos;
	float oldRadius;
	int updateCounter;
	ViargoOgreKinectTrackingCalibrationMetaphor* _metaphor;
	Ogre::SceneNode *handposNode;
	bool handOpen;
	int activeNode;
	Ogre::Entity *handEnt;

	bool objectGrabbing;
	Ogre::Vector3 objectGrabStartPosition;
	Ogre::Vector3 handGrabStartPosition;

public:

	KinectMetaphor(Ogre::SceneManager* sceneMgr, OgreBulletCollisionMetaphor* collisionMetaphor, ViargoOgreKinectTrackingCalibrationMetaphor* CaliMetaphor) {
		_mCollisionMetaphor = collisionMetaphor;
		_rigidBodies = collisionMetaphor->rigidBodies();
		mSceneMgr = sceneMgr;

		objectGrabbing = false;

		for (unsigned int i=0; i < _rigidBodies->size(); i++) {
			if(_rigidBodies->at(i)->getName() == "rbbox1") {
				rb[0] =  _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox2") {
				rb[1] = _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox3") {
				rb[2] = _rigidBodies->at(i);
			} else if (_rigidBodies->at(i)->getName() == "rbbox4") {
				rb[3] = _rigidBodies->at(i);
			}
		}
		_metaphor = CaliMetaphor;
		oldRadius = 0.0f;
		handOpen = true;
		activeNode = -1;
		updateCounter = 0;

		Ogre::Entity *entity = mSceneMgr->createEntity("handpos", "cube.mesh");		
		entity->setVisible(true);
		entity->setCastShadows(false);
		entity->setMaterialName("Examples/CubeTest");
		handEnt = entity;
		Ogre::SceneNode *node = mSceneMgr->getRootSceneNode()->createChildSceneNode("handposNode");  
		node->attachObject(entity);
		node->scale(0.1f, 0.1f, 0.0001f);
		//node->scale(50.0f, 50.0f, 50.0f);
		handposNode = mSceneMgr->getSceneNode("handposNode");
	}

	// dtor
	~KinectMetaphor() {
		 
	}

	virtual void findHand(cv::Mat *mat) {
		Mat imgB = cv::Mat(480, 640, CV_8UC1);
		mat->convertTo(imgB, CV_8UC1, 0.00390625);
		Rect myROI(0, 0, 640, 400);
		Mat img = imgB(myROI);
		imshow("bla2", img);

		Mat drawing = Mat::zeros( img.size(), CV_8UC3 );
		vector<vector<Point> > contours;
		vector<vector<Point> > bigContours;
		vector<Vec4i> hierarchy;

		findContours(img,contours, hierarchy, cv::RETR_LIST, cv::CHAIN_APPROX_SIMPLE, Point());

		if(contours.size()>0)
		{
			vector<std::vector<int> >hull( contours.size() );
			vector<vector<Vec4i>> convDef(contours.size() );
			vector<vector<Point>> hull_points(contours.size());
			vector<vector<Point>> defect_points(contours.size());


        for( size_t i = 0; i < contours.size(); i++ )
        {
            if(contourArea(contours[i])>1000)
            {
                convexHull( contours[i], hull[i], false );
                convexityDefects( contours[i],hull[i], convDef[i]);

                for(size_t k=0;k<hull[i].size();k++)
                {           
                    int ind=hull[i][k];
                    hull_points[i].push_back(contours[i][ind]);
                }

                for(size_t k=0;k<convDef[i].size();k++)
                {           
                    if(convDef[i][k][3]>20*256) // filter defects by depth
                    {
                    int ind_0=convDef[i][k][0];
                    int ind_1=convDef[i][k][1];
                    int ind_2=convDef[i][k][2];
                    defect_points[i].push_back(contours[i][ind_2]);
                    cv::circle(drawing,contours[i][ind_0],5,Scalar(0,255,0),-1);
                    cv::circle(drawing,contours[i][ind_1],5,Scalar(0,255,0),-1);
                    cv::circle(drawing,contours[i][ind_2],5,Scalar(0,0,255),-1);
                    cv::line(drawing,contours[i][ind_2],contours[i][ind_0],Scalar(0,0,255),1);
                    cv::line(drawing,contours[i][ind_2],contours[i][ind_1],Scalar(0,0,255),1);
                    }
                }
				// find circle
				bool c = false;
				for (size_t j = 0; j < defect_points.size(); j++) {

						if (defect_points[j].size() > 3) {
							int id = 0;
							for (size_t k = 0; k < defect_points[j].size(); k++) {
								if (defect_points[j][k].y < defect_points[j][id].y) {
									id = k;
								}
							}
							Point2f cent = defect_points[j][id];
							cent.y += 40;
							// Filter
							float q = 0.2f;

							newHandPos = handPos(cent, mat->at<unsigned short>(cent.y, cent.x));
							if (newHandPos.squaredDistance(oldHandPos) < 60 || updateCounter > 2) {
								oldHandPos = oldHandPos * (1 - q) + newHandPos * q;
								updateCounter = 0;
							} else {
								updateCounter++;
							}
							cv::circle(drawing, cent, 40, Scalar(255,255,255));
							handMove(oldHandPos, true);

							std::cout << "OPEN" << std::endl;

							c = true;
						} 
					
				}

				// Keine Figner zu sehen (zumindest nicht eindeutig)
				if (!c) {		// finde obersten Hüllepunkt
					float hoechstesY = 1000.0f;
					int id = 0;
					for (size_t k = 0; k < hull_points[i].size(); k++) {
						if (hull_points[i][k].y < hoechstesY) {
							hoechstesY = hull_points[i][k].y;
							id = k;
						}
					}
					Point2f cent = hull_points[i][id];
					cent.y += 60;
					float q = 0.4f;
					newHandPos = handPos(cent, mat->at<unsigned short>(cent.y-60, cent.x));
					if (newHandPos.squaredDistance(oldHandPos) < 60 || updateCounter > 2) {
						oldHandPos = oldHandPos * (1 - q) + newHandPos * q;
						updateCounter = 0;
					} else {
						updateCounter++;
					}
					cv::circle(drawing, cent, 40, Scalar(255,255,255));
					handMove(oldHandPos, false);

					std::cout << "CLOSE" << std::endl;
				}
				
                drawContours( drawing, contours, i, Scalar(0,255,0), 1, 8, vector<Vec4i>(), 0, Point() );
                drawContours( drawing, hull_points, i, Scalar(255,0,0), 1, 8, vector<Vec4i>(), 0, Point() );
            }
        }
    }
    imshow( "Hull demo", drawing );

	}

	virtual Ogre::Vector3 handPos(Point2f pos, unsigned short depthValue) {
		Point3f ausgabe;
		Point3f ausgabe2;
		Vector4 position = NuiTransformDepthImageToSkeleton(pos.x, pos.y, depthValue, NUI_IMAGE_RESOLUTION_640x480);
		ausgabe.x = position.x * 100.0 * -1.0; // Mirrored 
		ausgabe.y = position.y * 100.0;
		ausgabe.z = position.z * 100.0 * -1.0;
		_metaphor->transform(ausgabe, ausgabe2);
		//ausgabe2.z -= 50;
		return Ogre::Vector3 (ausgabe2.x, ausgabe2.y, ausgabe2.z);
	}

	virtual void handMove(Ogre::Vector3 pos, bool open) {
		
		//cout << "Ausgabe nachher x: " << ausgabe2.x << " y: " << ausgabe2.y << " z: " << ausgabe2.z << endl;
		handposNode->setPosition(pos);
		if (handOpen && open) { // Hand ist und war vorher auf
			//tue nichts
		} else if (handOpen && !open) { // hand war auf, ist jetzt zu
			handOpen = false;
			
			handEnt->setMaterialName("Examples/CubeTestZu");
			checkObjectGrabbed(pos);
		} else if (!handOpen && !open) { // hand war zu und ist zu
			// bewege Obejkt
			moveObject(pos);
		} else if (!handOpen && open) { // hand war zu und ist jetzt offen
			// objekt fallen lassen
			handEnt->setMaterialName("Examples/CubeTest");
			rb[0]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
			rb[1]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
			rb[2]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
			rb[3]->getBulletRigidBody()->setLinearFactor(btVector3(1,1,1));
			activeNode = -1;
			handOpen = true;
		}

	}

	virtual void moveObject(Ogre::Vector3 pos) {
		if (activeNode != -1) {
			btTransform transform; //Declaration of the btTransform
			transform.setIdentity(); //This function put the variable of the object to default. The ctor of btTransform doesnt do it.

			Ogre::Vector3 translation = pos - handGrabStartPosition;
			Ogre::Vector3 newObjectPosition = objectGrabStartPosition + translation;

			//transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(pos)); //Set the new position/origin
			transform.setOrigin(OgreBulletCollisions::OgreBtConverter::to(newObjectPosition)); //Set the new position/origin
			rb[activeNode]->getBulletRigidBody()->setWorldTransform(transform);
		}
	}

	virtual void checkObjectGrabbed(Ogre::Vector3 pos) {
		for (int i = 0; i < 4; i++) {
			float dist = 5.0f;
			Ogre::Vector3 posB = rb[i]->getSceneNode()->getPosition();
			if (pos.x > (posB.x - dist) && pos.y > (posB.y - dist) && pos.z > (posB.z - dist)) {
				if (pos.x < (posB.x + dist) && pos.y < (posB.y + dist) && pos.z < (posB.z + dist)) {
					// box gefasst
					activeNode = i;
					rb[activeNode]->getBulletRigidBody()->setLinearFactor(btVector3(0,0,0));

					handGrabStartPosition = pos;
					objectGrabStartPosition = posB;

					return;
				}
			}
		}
		activeNode = -1;
		return;
	}

};


#endif