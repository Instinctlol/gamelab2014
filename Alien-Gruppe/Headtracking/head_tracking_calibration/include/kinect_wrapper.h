#ifndef KINECT_WRAPPER_H
#define KINECT_WRAPPER_H

#include "NuiApi.h"

#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>


class Kinect {
public:
	/// Kinect Working Mode
	///
	enum KinectMode {
		/*Depth = 0,*/
		DepthColor,
		DepthIR
	};

	/// ctor
	///
	Kinect() 
		:_initialized(false)
		,_sensor(nullptr)
		,_depthMat(nullptr)
		,_colorMat(nullptr)
		,_colorTransformedMat(nullptr)
		,_irMat(nullptr)
	{
	}

	/// Initialize Kinect with given index in given mode
	///
	bool initialize(size_t index, KinectMode mode) {
		if (_initialized) {
			return false;
		}

		_mode = mode;

		HRESULT hr;

		int iSensorCount = 0;
		hr = NuiGetSensorCount(&iSensorCount);
		if (FAILED(hr) || (int)index >= iSensorCount) {
			return false;
		}
		
		// Create the sensor so we can check status
		hr = NuiCreateSensorByIndex(index, &_sensor);
		if (FAILED(hr)) {
			return false;
		}

		// Get the status of the sensor, and if connected, then we can initialize it
		hr = _sensor->NuiStatus();
		if (S_OK != hr) {
			return false;
		}
		
		// Color initialization
		if (_mode == DepthColor) {
			hr = _sensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_COLOR | NUI_INITIALIZE_FLAG_USES_DEPTH); 
			
			if (FAILED(hr)) {
				return false;
			}

			// Create an event that will be signaled when color data is available
			_supportEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

			// Open a color image stream to receive color frames
			hr = _sensor->NuiImageStreamOpen(
				NUI_IMAGE_TYPE_COLOR,
				NUI_IMAGE_RESOLUTION_640x480,
				0,
				2,
				_supportEvent,
				&_supportStreamHandle);

			if (FAILED(hr)) {
				return false;
			}
		}
		// IR initialization
		else if (_mode == DepthIR) {
			hr = _sensor->NuiInitialize(NUI_INITIALIZE_FLAG_USES_COLOR | NUI_INITIALIZE_FLAG_USES_DEPTH); 

			if (FAILED(hr)) {
				return false;
			}

			// Create an event that will be signaled when color data is available
			_supportEvent = CreateEvent(NULL, TRUE, FALSE, NULL);
			
			// Open a IR image stream to receive IR frames
			hr =_sensor->NuiImageStreamOpen(
				NUI_IMAGE_TYPE_COLOR_INFRARED,
				NUI_IMAGE_RESOLUTION_640x480,
				0,
				2,
				_supportEvent,
				&_supportStreamHandle);

			if (FAILED(hr)) {
				return false;
			}
		}

		// Depth initialization

		// Create an event that will be signaled when color data is available
		_depthEvent = CreateEvent(NULL, TRUE, FALSE, NULL);

		// Open a depth image stream to receive depth frames (Near Mode)
		hr = _sensor->NuiImageStreamOpen(
			NUI_IMAGE_TYPE_DEPTH,
			NUI_IMAGE_RESOLUTION_640x480,
			NUI_IMAGE_STREAM_FLAG_ENABLE_NEAR_MODE /*| NUI_IMAGE_STREAM_FLAG_DISTINCT_OVERFLOW_DEPTH_VALUES*/ | NUI_IMAGE_STREAM_FLAG_SUPPRESS_NO_FRAME_DATA,
			2,
			_depthEvent,
			&_depthStreamHandle);

		if (FAILED(hr)) {
			// Open a depth image stream to receive depth frames
			hr = _sensor->NuiImageStreamOpen(
				NUI_IMAGE_TYPE_DEPTH,
				NUI_IMAGE_RESOLUTION_640x480,
				0,
				2,
				_depthEvent,
				&_depthStreamHandle);

			if (FAILED(hr)) {
				return false;
			}
		}
		else {
			hr = _sensor->NuiImageStreamSetImageFrameFlags(_depthStreamHandle, NUI_IMAGE_STREAM_FLAG_ENABLE_NEAR_MODE);

			if (FAILED(hr)) {
				return false;
			}
		}

		// Setup matrices
		_depthMat = new cv::Mat(480, 640, CV_16UC1, cv::Scalar(0));

		if (_mode == DepthColor) {
			_colorMat = new cv::Mat(480, 640, CV_8UC4, cv::Scalar(0));
			_colorTransformedMat = new cv::Mat(480, 640, CV_8UC3, cv::Scalar(0));
		}
		else if (_mode == DepthIR) {
			_irMat = new cv::Mat(480, 640, CV_16UC1, cv::Scalar(0));
		}

		_initialized = true;

		return true;
	}

	/// dtor
	///
	~Kinect() {
		if (_initialized) {
			_sensor->NuiShutdown();

			_sensor->Release();
			_sensor = 0;
			
			if (_supportEvent != INVALID_HANDLE_VALUE) {
				CloseHandle(_supportEvent);
			}

			if (_depthEvent != INVALID_HANDLE_VALUE) {
				CloseHandle(_depthEvent);
			}

			delete _depthMat;

			if (_mode == DepthColor) {
				delete _colorMat;
				delete _colorTransformedMat;
			}
			else if (_mode == DepthIR) {
				delete _irMat;
			}
		}
	}

	/// Returns true if all frames are ready
	///
	bool hasFrame() {
		std::vector<HANDLE> allHandles;

		allHandles.push_back(_depthEvent);

		if (_mode == DepthColor || _mode == DepthIR) {
			allHandles.push_back(_supportEvent);
		}

		DWORD dwEvent = WaitForMultipleObjects(allHandles.size(), &allHandles[0], TRUE, 0);

		if (dwEvent == WAIT_OBJECT_0) {
			return true;
		}
		else {
			return false;
		}
	}
	
	/// Blocks and waits for receiving new frame
	///
	bool waitFrame() {
		std::vector<HANDLE> allHandles;

		allHandles.push_back(_depthEvent);

		if (_mode == DepthColor || _mode == DepthIR) {
			allHandles.push_back(_supportEvent);
		}

		DWORD dwEvent = WaitForMultipleObjects(allHandles.size(), &allHandles[0], TRUE, INFINITE);

		if (dwEvent == WAIT_OBJECT_0) {
			bool result = true;

			result &= _processDepth();

			if (_mode == DepthColor) {
				result &= _processColor();
			}
			else if (_mode == DepthIR) {
				result &= _processIR();
			}

			return result;
		}
		else {
			return false;
		}
	}

	cv::Mat* depthFrame() const {
		return _depthMat;
	}

	cv::Mat* colorFrame() const {
		return _colorTransformedMat;
	}
	
	cv::Mat* irFrame() const {
		return _irMat;
	}

	INuiSensor* sensorHandle() const {
		return _sensor;
	}

private:
	bool _processColor() {
		HRESULT hr;
		NUI_IMAGE_FRAME imageFrame;

		// Attempt to get the color frame
		hr = _sensor->NuiImageStreamGetNextFrame(_supportStreamHandle, 0, &imageFrame);
		if (FAILED(hr)) {
			return false;
		}

		INuiFrameTexture * pTexture = imageFrame.pFrameTexture;
		NUI_LOCKED_RECT LockedRect;

		// Lock the frame data so the Kinect knows not to modify it while we're reading it
		pTexture->LockRect(0, &LockedRect, NULL, 0);

		static cv::Mat colorTransformMat(480, 640, CV_8UC4);

		// Make sure we've received valid data
		if (LockedRect.Pitch != 0) {
			unsigned char* colorBufferRun = (unsigned char*)LockedRect.pBits;

			const unsigned int img_size = 640 * 480 * 4; 
			memcpy(_colorMat->data, colorBufferRun, img_size);
			cv::cvtColor(*_colorMat, *_colorTransformedMat, CV_BGRA2BGR);
		}

		// We're done with the texture so unlock it
		pTexture->UnlockRect(0);

		// Release the frame
		_sensor->NuiImageStreamReleaseFrame(_supportStreamHandle, &imageFrame);

		return true;
	}

	bool _processDepth() {
		HRESULT hr;
		NUI_IMAGE_FRAME imageFrame;
		
		// Attempt to get the color frame
		hr = _sensor->NuiImageStreamGetNextFrame(_depthStreamHandle, 0, &imageFrame);

		if (FAILED(hr)) {
			return 0;
		}

		INuiFrameTexture * pTexture = imageFrame.pFrameTexture;
		NUI_LOCKED_RECT LockedRect;

		// Lock the frame data so the Kinect knows not to modify it while we're reading it
		pTexture->LockRect(0, &LockedRect, NULL, 0);

		// Make sure we've received valid data
		if (LockedRect.Pitch != 0) {
			unsigned char* depthBufferRun = (unsigned char*)LockedRect.pBits;

			const unsigned int img_size = 640 * 480 * sizeof(unsigned short); 
			memcpy(_depthMat->data, depthBufferRun, img_size);
		}

		// We're done with the texture so unlock it
		pTexture->UnlockRect(0);

		// Release the frame
		_sensor->NuiImageStreamReleaseFrame(_depthStreamHandle, &imageFrame);

		return true;
	}

	bool _processIR() {
		HRESULT hr;
		NUI_IMAGE_FRAME imageFrame;
		
		// Attempt to get the color frame
		hr = _sensor->NuiImageStreamGetNextFrame(_supportStreamHandle, 0, &imageFrame);
		if (FAILED(hr)) {
			return 0;
		}

		INuiFrameTexture * pTexture = imageFrame.pFrameTexture;
		NUI_LOCKED_RECT LockedRect;

		// Lock the frame data so the Kinect knows not to modify it while we're reading it
		pTexture->LockRect(0, &LockedRect, NULL, 0);

		// Make sure we've received valid data
		if (LockedRect.Pitch != 0) {
			unsigned char* IRBufferRun = (unsigned char*)LockedRect.pBits;

			const unsigned int img_size = 640 * 480 * sizeof(unsigned short); 
			memcpy(_irMat->data, IRBufferRun, img_size);
		}

		// We're done with the texture so unlock it
		pTexture->UnlockRect(0);

		// Release the frame
		_sensor->NuiImageStreamReleaseFrame(_supportStreamHandle, &imageFrame);

		return true;
	}


	/// Handle to Kinect Sensor
	///
	INuiSensor* _sensor;

	/// If true, Kinect is initialized correctly
	///
	bool _initialized;

	/// Kinect working mode
	///
	KinectMode _mode;

	HANDLE _depthEvent;
	HANDLE _depthStreamHandle;
	HANDLE _supportEvent;
	HANDLE _supportStreamHandle;

	cv::Mat* _depthMat;
	cv::Mat* _colorMat;
	cv::Mat* _colorTransformedMat;
	cv::Mat* _irMat;

}; // Kinect

#endif // KINECT_WRAPPER_H