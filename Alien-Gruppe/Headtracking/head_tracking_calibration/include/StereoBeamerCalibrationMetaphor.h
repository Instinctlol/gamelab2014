#ifndef WBCALIBRATION_METAPHOR_H
#define WBCALIBRATION_METAPHOR_H

// base app
#include "BaseApplication.h"
#include "StereoRenderTargetListener.h"

#include <OgreRenderTargetListener.h>
#include <iostream>
#include <fstream>

class StereoBeamerCalibrationMetaphor : public viargo::Metaphor {

protected:
	Ogre::SceneManager* _mSceneMgr;
	StereoRenderTargetListener::StereoMode _stereoMode;
	Ogre::OverlayContainer* _container1BG;
	Ogre::OverlayContainer* _container2BG;
	Ogre::OverlayContainer* _container1_1;
	Ogre::OverlayContainer* _container1_2;
	Ogre::OverlayContainer* _container1_3;
	Ogre::OverlayContainer* _container1_4;
	Ogre::OverlayContainer* _container2_1;
	Ogre::OverlayContainer* _container2_2;
	Ogre::OverlayContainer* _container2_3;
	Ogre::OverlayContainer* _container2_4;
	Ogre::Vector2 _markerSize;
	bool _calibrated;

	Ogre::Vector2 _positions[8];

private:
	enum ScreenEdge {
		SE_TOP_LEFT,
		SE_TOP_RIGHT,
		SE_BOTTOM_LEFT,
		SE_BOTTOM_RIGHT,
	};


	// moves the position of a screen edge
	bool setScreenEdgePosition(ScreenEdge se, bool isLeft, const Ogre::Vector2& position) {

		std::string materialName = std::string("RTTViewerMaterial") + (isLeft ? "Left" : "Right");
	
		// get the material
		Ogre::MaterialPtr lmaterial = Ogre::MaterialManager::getSingleton().getByName(materialName);
		if (lmaterial.isNull()) return false;
	
		// get the name...
		Ogre::Pass* lpass = lmaterial->getTechnique(0)->getPass(0);
	
		// and the shader uniforms
		Ogre::GpuProgramParametersSharedPtr lparams = lpass->getFragmentProgramParameters();
		if (lparams.isNull()) return false;

		// set the param
		std::string uniformName;
		switch (se) {
			case SE_TOP_LEFT:		uniformName = "tctl"; break;
			case SE_TOP_RIGHT:		uniformName = "tctr"; break;
			case SE_BOTTOM_LEFT:	uniformName = "tcbl"; break;
			case SE_BOTTOM_RIGHT:	uniformName = "tcbr"; break;
			default: 
				return false;
		}
		lparams->setNamedConstant(uniformName, Ogre::Vector4(position.x, position.y, 0.0f, 0.0f));
		return true;
	}


public:
	/// Own render target listener to update visibility of beamer calibration overlays
	///
	class BeamerCalibrationRenderTargetListener : public Ogre::RenderTargetListener {
	public:
		BeamerCalibrationRenderTargetListener(bool isLeft) :_isLeft(isLeft) {
			Ogre::OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();
			_overlay1 = overlayManager.getByName("WBCalibrationMarker1");
			_overlay2 = overlayManager.getByName("WBCalibrationMarker2");
		}
		virtual ~BeamerCalibrationRenderTargetListener() {}

		virtual void preRenderTargetUpdate(const Ogre::RenderTargetEvent& evt) {
			Ogre::OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();

			if (_overlay1 == 0 || _overlay2 == 0) {
				std::cout << "Bad things will happen..." << std::endl;
				return;
			}

			// Subsequently switch visibility for overlays for beamer calibration (left / right)
			if (_isLeft) {
				_overlay1->show();
				_overlay2->hide();
			}
			else {
				_overlay1->hide();
				_overlay2->show();
			}
		}

		virtual void postRenderTargetUpdate(const Ogre::RenderTargetEvent& evt) {}

	private:
		bool _isLeft;
		Ogre::Overlay* _overlay1;
		Ogre::Overlay* _overlay2;
	}; // BeamerCalibrationRenderTargetListener

	// ctor
	StereoBeamerCalibrationMetaphor(std::string name, Ogre::SceneManager* sceneMgr, StereoRenderTargetListener::StereoMode mode, Ogre::RenderTexture* leftTexture, Ogre::RenderTexture* rightTexture)
		:Metaphor(name)
		,_mSceneMgr(sceneMgr)
		,_stereoMode(mode)
		,_calibrated(false)
	{
		_markerSize = Ogre::Vector2(0.05f, 0.05f);

		// Setup standard positions
		_positions[0] = Ogre::Vector2(0, 0); // Top left
		_positions[1] = Ogre::Vector2(1, 0); // Top right
		_positions[2] = Ogre::Vector2(0, 1); // Bottom left
		_positions[3] = Ogre::Vector2(1, 1); // Bottom right
		_positions[4] = Ogre::Vector2(0, 0); // Top left
		_positions[5] = Ogre::Vector2(1, 0); // Top right
		_positions[6] = Ogre::Vector2(0, 1); // Bottom left
		_positions[7] = Ogre::Vector2(1, 1); // Bottom right

		loadCalibration();
		setupCalibration();

		// If side-by-side stereo, setup markers for better visibility
		if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
			// Create red and green materials for markers from texture
			Ogre::MaterialPtr mat = Ogre::MaterialManager::getSingleton().create("redMat","Essential"); 
			mat->getTechnique(0)->getPass(0)->createTextureUnitState("red.jpg");
			mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
			mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
			mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

			mat = Ogre::MaterialManager::getSingleton().create("greenMat","Essential"); 
			mat->getTechnique(0)->getPass(0)->createTextureUnitState("green.jpg");
			mat->getTechnique(0)->getPass(0)->setDepthCheckEnabled(false);
			mat->getTechnique(0)->getPass(0)->setDepthWriteEnabled(false);
			mat->getTechnique(0)->getPass(0)->setLightingEnabled(false);

			// Create overlays for image calibration
			Ogre::OverlayManager& overlayManager = Ogre::OverlayManager::getSingleton();
				
			Ogre::Overlay* overlay = 0;

			// Overlay 1 (left)
			overlay = overlayManager.create( "WBCalibrationMarker1" );

			_container1BG = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker1BG" ) );
			_container1BG->setPosition( 0.0, 0.0 );
			_container1BG->setDimensions( 1.0, 1.0 );
			_container1BG->setMaterialName( "BaseWhite" );

			_container1_1 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker1_1" ) );
			_container1_1->setPosition( 0.0, 0.0 );
			_container1_1->setDimensions( _markerSize.x, _markerSize.y );
			_container1_1->setMaterialName( "redMat" );

			_container1_2 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker1_2" ) );
			_container1_2->setPosition( 1.0 - _markerSize.x, 0.0 );
			_container1_2->setDimensions( _markerSize.x, _markerSize.y );
			_container1_2->setMaterialName( "redMat" );

			_container1_3 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker1_3" ) );
			_container1_3->setPosition( 0.0, 1.0 - _markerSize.y );
			_container1_3->setDimensions( _markerSize.x, _markerSize.y );
			_container1_3->setMaterialName( "redMat" );

			_container1_4 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker1_4" ) );
			_container1_4->setPosition( 1.0 - _markerSize.x, 1.0 - _markerSize.y );
			_container1_4->setDimensions( _markerSize.x, _markerSize.y );
			_container1_4->setMaterialName( "redMat" );

			overlay->add2D( _container1BG );
			overlay->add2D( _container1_1 );
			overlay->add2D( _container1_2 );
			overlay->add2D( _container1_3 );
			overlay->add2D( _container1_4 );

			overlay->hide();
			_container1BG->hide();
			_container1_1->hide();
			_container1_2->hide();
			_container1_3->hide();
			_container1_4->hide();

			// Overlay 2 (right)
			overlay = overlayManager.create( "WBCalibrationMarker2" );

			_container2BG = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker2BG" ) );
			_container2BG->setPosition( 0.0, 0.0 );
			_container2BG->setDimensions( 1.0, 1.0 );
			_container2BG->setMaterialName( "BaseWhite" );

			_container2_1 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker2_1" ) );
			_container2_1->setPosition( 0.0, 0.0 );
			_container2_1->setDimensions( _markerSize.x, _markerSize.y );
			_container2_1->setMaterialName( "greenMat" );

			_container2_2 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker2_2" ) );
			_container2_2->setPosition( 1.0 - _markerSize.x, 0.0 );
			_container2_2->setDimensions( _markerSize.x, _markerSize.y );
			_container2_2->setMaterialName( "greenMat" );

			_container2_3 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker2_3" ) );
			_container2_3->setPosition( 0.0, 1.0 - _markerSize.y );
			_container2_3->setDimensions( _markerSize.x, _markerSize.y );
			_container2_3->setMaterialName( "greenMat" );

			_container2_4 = static_cast<Ogre::OverlayContainer*>( overlayManager.createOverlayElement( "Panel", "WBCalibrationMarker2_4" ) );
			_container2_4->setPosition( 1.0 - _markerSize.x, 1.0 - _markerSize.y );
			_container2_4->setDimensions( _markerSize.x, _markerSize.y );
			_container2_4->setMaterialName( "greenMat" );

			overlay->add2D( _container2BG );
			overlay->add2D( _container2_1 );
			overlay->add2D( _container2_2 );
			overlay->add2D( _container2_3 );
			overlay->add2D( _container2_4 );

			overlay->hide();
			_container2BG->hide();
			_container2_1->hide();
			_container2_2->hide();
			_container2_3->hide();
			_container2_4->hide();
			
			// Add render target listener to both textrues to update overlay visibility
			leftTexture->addListener(new BeamerCalibrationRenderTargetListener(true));
			rightTexture->addListener(new BeamerCalibrationRenderTargetListener(false));
		}
	}

	// we only need the keyboard events
	virtual bool onEventReceived(viargo::Event* event) {
		
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			return true;
		}

		return false;
	}
	
	// Gets called for handling of an event if onEventReceived(...) returned true
	// @param: event	the event to be handled
	virtual void handleEvent(viargo::Event* event) {
		//std::cout << "\n--------------------- WBCalibration ----------------------------------\n";

		static int mode = 0;

		// handle key events
		if (typeid(*event) == typeid(viargo::KeyEvent)) {
			std::cout << "KeyEvent gefunden";
			viargo::KeyEvent& key = *((viargo::KeyEvent*)event);

			float factor = 0.02f; // Adjust precision
			 
			// hier kommt die Kalibrierung rein!!!

			// *** Diese Funktion kann eingesetzt werden, um die Verzerrung der Workbench zu kalibrieren.
			// am besten Viargo-Metapher dafür schreiben
			// mit Tasten, dann Vektoren abspeichern.
			// dann beim Initialisieren laden
			// siehe RTTCaveWallMaterial.material
			// *****

			// NOTE: Alex: Why not linear interpolation in shader ? Looks like splines or cubic interpolation on deformations ?!
			
			bool updateCalibration = false;
			if (key.action() == viargo::KeyEvent::KEY_PRESSED) {
				if (key.key() == viargo::KeyboardKey::KEY_0) {
					mode = 0;
					std::cout << "No mode" << std::endl;

					// Setup standard positions
					_positions[0] = Ogre::Vector2(0, 0); // Top left
					_positions[1] = Ogre::Vector2(1, 0); // Top right
					_positions[2] = Ogre::Vector2(0, 1); // Bottom left
					_positions[3] = Ogre::Vector2(1, 1); // Bottom right
					_positions[4] = Ogre::Vector2(0, 0); // Top left
					_positions[5] = Ogre::Vector2(1, 0); // Top right
					_positions[6] = Ogre::Vector2(0, 1); // Bottom left
					_positions[7] = Ogre::Vector2(1, 1); // Bottom right

					// Overlay visibility
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(false);
					}
				}
				// Left quad
				else if (key.key() == viargo::KeyboardKey::KEY_1) {
					mode = 1;
					std::cout << "Mode: Left: Top Left" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_2) {
					mode = 2;
					std::cout << "Mode: Left: Top Right" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_3) {
					mode = 3;
					std::cout << "Mode: Left: Bottom Left" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_4) {
					mode = 4;
					std::cout << "Mode: Left: Bottom Right" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				// Right quad
				else if (key.key() == viargo::KeyboardKey::KEY_5) {
					mode = 5;
					std::cout << "Mode: Right: Top Left" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_6) {
					mode = 6;
					std::cout << "Mode: Right: Top Right" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_7) {
					mode = 7;
					std::cout << "Mode: Right: Bottom Left" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_8) {
					mode = 8;
					std::cout << "Mode: Right: Bottom Right" << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(true);
					}
				}
				else if (key.key() == viargo::KeyboardKey::KEY_9) {
					mode = 0;
					std::cout << "Store calibration..." << std::endl;

					// Overlay visibility and position
					if (_stereoMode == StereoRenderTargetListener::SM_SIDEBYSIDE) {
						showMarkers(false);
					}

					storeCalibration();
					_calibrated = true;

					setupCalibration();
				}
				else if (key.key() == viargo::KeyboardKey::KEY_UP) {	
					_positions[mode - 1].y += 0.1 * factor;
					updateCalibration = true;
				}
				else if (key.key() == viargo::KeyboardKey::KEY_DOWN) {
					_positions[mode - 1].y -= 0.1 * factor;
					updateCalibration = true;
				}
				else if (key.key() == viargo::KeyboardKey::KEY_LEFT) {	
					_positions[mode - 1].x += 0.1 * factor;
					updateCalibration = true;
				}
				else if (key.key() == viargo::KeyboardKey::KEY_RIGHT) {
					_positions[mode - 1].x -= 0.1 * factor;
					updateCalibration = true;
				}
			}

			setupCalibration(updateCalibration);
		}

		else {
			// panic! we should never be here!
		}
	}

	// Gets called from Viargo.update() to handle frame-specific actions
	// @param:  timeSinceLastUpdate - the time since the last call of the function
	//								  in milliseconds
	virtual void update(float timeSinceLastUpdate) {
		
	}

	void setupCalibration(bool force = false) {
		if (force || _calibrated) {
			if (_stereoMode != StereoRenderTargetListener::SM_SIDEBYSIDE) {
				_positions[4] = _positions[0];
				_positions[5] = _positions[1];
				_positions[6] = _positions[2];
				_positions[7] = _positions[3];
			}

			setScreenEdgePosition(SE_TOP_LEFT, true, _positions[0]);		
			setScreenEdgePosition(SE_TOP_LEFT, false, _positions[4]);	
			setScreenEdgePosition(SE_TOP_RIGHT, true, _positions[1]);		
			setScreenEdgePosition(SE_TOP_RIGHT, false, _positions[5]);	
			setScreenEdgePosition(SE_BOTTOM_LEFT, true, _positions[2]);		
			setScreenEdgePosition(SE_BOTTOM_LEFT, false, _positions[6]);
			setScreenEdgePosition(SE_BOTTOM_RIGHT, true, _positions[3]);		
			setScreenEdgePosition(SE_BOTTOM_RIGHT, false, _positions[7]);	
		}
	}

	void showMarkers(bool show) {
		if (show) {
			_container1BG->show();
			_container1_1->show();
			_container1_2->show();
			_container1_3->show();
			_container1_4->show();

			_container2BG->show();
			_container2_1->show();
			_container2_2->show();
			_container2_3->show();
			_container2_4->show();
		}
		else {
			_container1BG->hide();
			_container1_1->hide();
			_container1_2->hide();
			_container1_3->hide();
			_container1_4->hide();

			_container2BG->hide();
			_container2_1->hide();
			_container2_2->hide();
			_container2_3->hide();
			_container2_4->hide();
		}
	}


	void loadCalibration() {
		std::ifstream calibrationFile("calibration.bin", std::ios_base::in);
		if (calibrationFile.good()) {
			calibrationFile.read((char*)&_positions[0], 8 * sizeof(Ogre::Vector2));
			_calibrated = true;
		}
	}

	void storeCalibration() {
		std::ofstream calibrationFile("calibration.bin", std::ios_base::out);
		if (calibrationFile.good()) {
			calibrationFile.write((char*)&_positions[0], 8 * sizeof(Ogre::Vector2));
		}
	}
};

#endif //VIARGO_DEFAULT_TOUCH_STEUERUNG_METAPHOR_H


