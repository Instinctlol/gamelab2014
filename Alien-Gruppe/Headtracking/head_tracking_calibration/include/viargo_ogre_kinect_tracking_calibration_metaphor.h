#ifndef __VIARGO_OGRE_KINECT_TRACKING_CALIBRATION_METAPHOR_H__
#define __VIARGO_OGRE_KINECT_TRACKING_CALIBRATION_METAPHOR_H__

#include <vector>

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

// --------------------------------------------------------------------------------------------
// --------------------------------------------------------------------------------------------

class ViargoOgreKinectTrackingCalibrationMetaphor {
public:
	/// ctor
	///
	ViargoOgreKinectTrackingCalibrationMetaphor(const Ogre::Vector2& windowSize, int pattern = 4, const Ogre::Vector4& offsets = Ogre::Vector4(0.1f, 0.1f, 0.1f, 0.1f));
	
	/// dtor
	///
	virtual ~ViargoOgreKinectTrackingCalibrationMetaphor();

	/// Initializes calibration
	///
	void startCalibration();

	void updateCalibration(const cv::Point3f& point, bool holding);
	
	/// Called by engine
	///
	void update(float timeSinceLastUpdate);

	/// Transforms data by calibration
	///
	void transform(const cv::Point3f& in, cv::Point3f& out);

	void storeTransformation(const std::string& name);
	void loadTransformation(const std::string& name);

	bool isCali();

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
	void _handleFilteredPositionEvent(const cv::Point3f& point, bool holding);
	
	/// Updates calibration
	///
	void _updateCalibration(const cv::Point3f& position);

	/// Finalizes calibration
	///
	void _finalizeCalibration();
	
	/// Update overlay screen to current calibration progress
	///
	/// @param state If true, marker is holding still (green overlay), if false marker is moving (yellow overlay)
	///
	void _updateOverlays(bool state);

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

	float _transformationMatrix2[16];

	void _buildTransformationMatrix(cv::Point3f origin, cv::Point3f xAxis, cv::Point3f yAxis) {
		cv::Point3f zAxis(0, 0, 0);

		// Caluclate direction vectors
		xAxis.x -= origin.x;
		xAxis.y -= origin.y;
		xAxis.z -= origin.z;

		yAxis.x -= origin.x;
		yAxis.y -= origin.y;
		yAxis.z -= origin.z;
		
		// Store lenght information for scaling
		float lx = sqrt(xAxis.x * xAxis.x + xAxis.y * xAxis.y + xAxis.z * xAxis.z);
		float ly = sqrt(yAxis.x * yAxis.x + yAxis.y * yAxis.y + yAxis.z * yAxis.z);

		// Normalize
		float length = sqrt(xAxis.x * xAxis.x + xAxis.y * xAxis.y + xAxis.z * xAxis.z);
		xAxis.x = xAxis.x / length;
		xAxis.y = xAxis.y / length;
		xAxis.z = xAxis.z / length;

		// Normalize
		length = sqrt(yAxis.x * yAxis.x + yAxis.y * yAxis.y + yAxis.z * yAxis.z);
		yAxis.x = yAxis.x / length;
		yAxis.y = yAxis.y / length;
		yAxis.z = yAxis.z / length;

		// Calculate z-axis through cross product
		zAxis.x = yAxis.y * xAxis.z - yAxis.z * xAxis.y;
		zAxis.y = yAxis.z * xAxis.x - yAxis.x * xAxis.z;
		zAxis.z = yAxis.x * xAxis.y - yAxis.y * xAxis.x;

		// Normalize
		length = sqrt(zAxis.x * zAxis.x + zAxis.y * zAxis.y + zAxis.z * zAxis.z);
		zAxis.x = zAxis.x / length;
		zAxis.y = zAxis.y / length;
		zAxis.z = zAxis.z / length;

		// Calculate y-axis through cross product
		yAxis.x = xAxis.y * zAxis.z - xAxis.z * zAxis.y;
		yAxis.y = xAxis.z * zAxis.x - xAxis.x * zAxis.z;
		yAxis.z = xAxis.x * zAxis.y - xAxis.y * zAxis.x;

		// Normalize
		length = sqrt(yAxis.x * yAxis.x + yAxis.y * yAxis.y + yAxis.z * yAxis.z);
		yAxis.x = yAxis.x / length;
		yAxis.y = yAxis.y / length;
		yAxis.z = yAxis.z / length;

		// Set matrix to identity
		for (int i = 0; i < 16; i ++) {
			_transformationMatrix2[i] = 0.0f;
		}

		_transformationMatrix2[0]  = 1.0f;
		_transformationMatrix2[5]  = 1.0f;
		_transformationMatrix2[10] = 1.0f;
		_transformationMatrix2[15] = 1.0f;

		// Rotation and scale
		/*_transformationMatrix2[0] = xAxis.x * (1.0f / lx);
		_transformationMatrix2[1] = xAxis.y * (1.0f / lx);
		_transformationMatrix2[2] = xAxis.z * (1.0f / lx);

		_transformationMatrix2[4] = yAxis.x * (1.0f / ly);
		_transformationMatrix2[5] = yAxis.y * (1.0f / ly);
		_transformationMatrix2[6] = yAxis.z * (1.0f / ly);

		_transformationMatrix2[8]  = zAxis.x;
		_transformationMatrix2[9]  = zAxis.y;
		_transformationMatrix2[10] = zAxis.z;*/

		_transformationMatrix2[0] = xAxis.x;
		_transformationMatrix2[1] = xAxis.y;
		_transformationMatrix2[2] = xAxis.z;

		_transformationMatrix2[4] = yAxis.x;
		_transformationMatrix2[5] = yAxis.y;
		_transformationMatrix2[6] = yAxis.z;

		_transformationMatrix2[8]  = -zAxis.x;
		_transformationMatrix2[9]  = -zAxis.y;
		_transformationMatrix2[10] = -zAxis.z;

		// Translation (you get these values, when you multiplicate the orthogonal rotation matrix with the translation matrix)
		/*float translationX = (- origin.x * xAxis.x - origin.y * xAxis.y - origin.z * xAxis.z) * (1.0f / lx);
		float translationY = (- origin.x * yAxis.x - origin.y * yAxis.y - origin.z * yAxis.z) * (1.0f / ly);
		float translationZ = (- origin.x * zAxis.x - origin.y * zAxis.y - origin.z * zAxis.z);*/

		float translationX = (- origin.x * xAxis.x - origin.y * xAxis.y - origin.z * xAxis.z);
		float translationY = (- origin.x * yAxis.x - origin.y * yAxis.y - origin.z * yAxis.z);
		float translationZ = (- origin.x * zAxis.x - origin.y * zAxis.y - origin.z * zAxis.z);

		_transformationMatrix2[3]  = translationX;
		_transformationMatrix2[7]  = translationY;
		_transformationMatrix2[11] = translationZ;

		// Correct translation to window center
		cv::Point3f originSceneCoordinate = _screenPositions3D[2];
		
		float TX = originSceneCoordinate.x;
		float TY = originSceneCoordinate.y;
		float TZ = 0.0f;//13.5f;

		_transformationMatrix2[3]  += TX;
		_transformationMatrix2[7]  += TY;
		_transformationMatrix2[11] += TZ;

		// Get mysterious offset
		cv::Point3f out;
		transformate2(&origin, &out);

		_transformationMatrix2[11] -= out.z;
		
		// Save to file
		//Serializer::save("calibration.dat", _transformationMatrix, sizeof(float), 16);
	}

	void transformate2(cv::Point3f* in, cv::Point3f* out) {
		cv::Point3f coordinateOut;
		cv::Point3f temp;
		float w;

		// Multiplicate by transformation matrix
		coordinateOut.x = _transformationMatrix2[0] * in->x + 
			_transformationMatrix2[1] * in->y +
			_transformationMatrix2[2] * in->z +
			_transformationMatrix2[3];

		coordinateOut.y = _transformationMatrix2[4] * in->x + 
			_transformationMatrix2[5] * in->y +
			_transformationMatrix2[6] * in->z +
			_transformationMatrix2[7];

		coordinateOut.z = _transformationMatrix2[8] * in->x + 
			_transformationMatrix2[9] * in->y +
			_transformationMatrix2[10] * in->z +
			_transformationMatrix2[11];

		w = _transformationMatrix2[12] * in->x + 
			_transformationMatrix2[13] * in->y +
			_transformationMatrix2[14] * in->z +
			_transformationMatrix2[15];

		if (w != 1 && w != 0) {
			coordinateOut.x = in->x / w;
			coordinateOut.y = in->y / w;
			coordinateOut.z = in->z / w;
		} 
		
		out->x = coordinateOut.x;
		out->y = coordinateOut.y;
		out->z = coordinateOut.z;
	}

}; // ViargoOgreKinectTrackingCalibrationMetaphor 

#endif // __VIARGO_OGRE_KINECT_TRACKING_CALIBRATION_METAPHOR_H__