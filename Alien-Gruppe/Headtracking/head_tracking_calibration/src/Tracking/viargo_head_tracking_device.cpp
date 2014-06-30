#include <string>

#include "core/logger.h"
#include "device/comm/binarydatareader.h"
#include "tracking/viargo_head_tracking_device.h"

namespace viargo {

ViargoHeadtrackingDevice::ViargoHeadtrackingDevice(const std::string& name, const std::string& ip, int port)
	:NetworkDevice(name, NetworkDevice::CT_UDP, ip, port){ 
}

ViargoHeadtrackingDevice::~ViargoHeadtrackingDevice() {
}

void ViargoHeadtrackingDevice::parseData(const char* data, int size) {
	BinaryDataReader bdr((const viargo::System::byte*)data, 
			size, EndiannessConvertor::ED_LITTLE_ENDIAN);
	
	// CONSTANTS
	static const char MAGIC_KEY[] = "VALI";
	static const char EVENT_TYPE_POS[] = "POS";
	static const char EVENT_TYPE_ACC[] = "ACC";
	static const char EVENT_TYPE_VEL[] = "VEL";

	static const unsigned int BUFFER_SIZE = 100;

	// Buffer
	char* buffer = new char[BUFFER_SIZE];

	// Read magic number
	bdr.read(buffer, 5);

	// Check magic number
	if (strcmp(buffer, MAGIC_KEY) != 0) {
		delete[] buffer;
		return;
	}

	// Read event type
	bdr.read(buffer, 4);

	// Store event type
	const std::string eventType = std::string(buffer);

	// Read tracker name
	int charIndex = 0;

	do {
		buffer[charIndex] = bdr.readChar();
		charIndex++;
	}
	while (charIndex < BUFFER_SIZE && buffer[charIndex - 1] != '\0');

	// Store tracker name
	const std::string trackerName = std::string(buffer);

	// Release buffer
	delete[] buffer;

	// Read sensor handle
	int sensorHandle = bdr.readInt();

	if (eventType == EVENT_TYPE_POS) {
		decodePosition(trackerName, sensorHandle, bdr);
	}
	else if (eventType == EVENT_TYPE_ACC) {
		decodeAcceleration(trackerName, sensorHandle, bdr);
	}
	else if (eventType == EVENT_TYPE_VEL) {
		decodeVelocity(trackerName, sensorHandle, bdr);
	}
	else {
		LogError("Malformed input data!" );
	}
}

void ViargoHeadtrackingDevice::decodePosition(const std::string& trackerName, int sensorHandle, BinaryDataReader& bdr) {
	double posX = 0.0;
	double posY = 0.0;
	double posZ = 0.0;

	double quat1 = 0.0;
	double quat2 = 0.0;
	double quat3 = 0.0;
	double quat4 = 0.0;

	// Get values
	posX = bdr.readDouble();
	posY = bdr.readDouble();
	posZ = bdr.readDouble();

	quat1 = bdr.readDouble();
	quat2 = bdr.readDouble();
	quat3 = bdr.readDouble();
	quat4 = bdr.readDouble();

	// Convert sensorHandle to string
	std::ostringstream oss;
	oss << sensorHandle;

	viargo::SensorPositionEvent* event = new viargo::SensorPositionEvent(trackerName, oss.str(), (float)posX, (float)posY, (float)posZ);

	Viargo.metaphor("HeadtrackingFilter-Metaphor").handleEvent(event);
	event->drop();

	//broadcastEvent(event);
}

void ViargoHeadtrackingDevice::decodeAcceleration(const std::string& trackerName, int sensorHandle, BinaryDataReader& bdr) {
	// Do nothing...
}

void ViargoHeadtrackingDevice::decodeVelocity(const std::string& trackerName, int sensorHandle, BinaryDataReader& bdr) {
	// Do nothing...
}

}