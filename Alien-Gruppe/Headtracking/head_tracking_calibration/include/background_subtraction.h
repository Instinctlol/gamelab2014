#pragma once

#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>

#ifdef max
#undef max
#endif
#ifdef min
#undef min
#endif

class ImageBackgroundSubtractor {
public:
	ImageBackgroundSubtractor(int type)
		:_type(type)
	{
		assert(type == CV_8UC1 || type == CV_16UC1 || type == CV_8UC3);

		_minimumInitialValue = (type == CV_8UC1) ? 255 : 65536;
		_maximumInitialValue = 0;

		if (type == CV_8UC3) {
			_minimumMat = cv::Mat(480, 640, type, cv::Scalar(_minimumInitialValue, _minimumInitialValue, _minimumInitialValue));
		}
		else {
			_minimumMat = cv::Mat(480, 640, type, cv::Scalar(_minimumInitialValue));
		}
		
		_maximumMat = cv::Mat(480, 640, type, cv::Scalar(_maximumInitialValue));
		
		_maskMatMin = cv::Mat(480, 640, CV_8UC1);
		_maskMatMax = cv::Mat(480, 640, CV_8UC1);
		_maskMat = cv::Mat(480, 640, CV_8UC1);
	}

	~ImageBackgroundSubtractor() {
	}

	void addSample(const cv::Mat& matrix, int ignoreValue = -1) {
		_minimumMat = cv::min(_minimumMat, matrix);
		_maximumMat = cv::max(_maximumMat, matrix);

		if (ignoreValue != -1) {
			cv::compare(_minimumMat, ignoreValue, _maskMat, cv::CMP_EQ);
			_minimumMat.setTo(cv::Scalar(_minimumInitialValue), _maskMat);

			cv::compare(_maximumMat, ignoreValue, _maskMat, cv::CMP_EQ);
			_maximumMat.setTo(cv::Scalar(_maximumInitialValue), _maskMat);
		}
	}

	void subtractBackground(cv::Mat& matrix, int threshold = 0, int defaultValue = 0) {
		cv::inRange(matrix, _minimumMat - threshold, _maximumMat + threshold, _maskMat);
		matrix.setTo(cv::Scalar(defaultValue), _maskMat);
	}

	void store(const std::string& filename) {
		cv::FileStorage fs(filename, cv::FileStorage::WRITE);
		fs << "minimumMat" << _minimumMat;
		fs << "maximumMat" << _maximumMat;
		fs.release();
	}

	bool load(const std::string& filename) {
		cv::FileStorage fs(filename, cv::FileStorage::READ);

		if (fs["minimumMat"].empty() || fs["maximumMat"].empty()) {
			return false;
		}

		fs["minimumMat"] >> _minimumMat;
		fs["maximumMat"] >> _maximumMat;
		fs.release();

		return true;
	}

protected:
	cv::Mat _minimumMat;
	cv::Mat _maximumMat;

	cv::Mat _maskMat;
	cv::Mat _maskMatMin;
	cv::Mat _maskMatMax;

	int _type;

	int _minimumInitialValue;
	int _maximumInitialValue;

}; // DepthSubtractor