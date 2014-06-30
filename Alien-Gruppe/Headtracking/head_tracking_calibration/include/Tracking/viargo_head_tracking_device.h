#ifndef VIARGO_HEADTRACKING_DEVICE_H
#define VIARGO_HEADTRACKING_DEVICE_H

#include <viargo.h>

#include "device/networkdevice.h"

namespace viargo {
class BinaryDataReader;

class ViargoHeadtrackingDevice : public NetworkDevice {
public:
	/// ctor
	///
	/// For localhost, insert own network id, e.g., 128.176.181.25, or let empty
	ViargoHeadtrackingDevice(const std::string& name, const std::string& ip = "", int port = 2424);

	// Destructor
	virtual ~ViargoHeadtrackingDevice();

	// Implementation of DataParser interface
	void parseData(const char* data, int size);

private:
	void decodePosition(const std::string& trackerName, int sensorHandle, viargo::BinaryDataReader& bdr);
	void decodeAcceleration(const std::string& trackerName, int sensorHandle, viargo::BinaryDataReader& bdr);
	void decodeVelocity(const std::string& trackerName, int sensorHandle, viargo::BinaryDataReader& bdr);

}; // ViargoHeadtrackingDevice

} // namespace viargo

#endif // VIARGO_HEADTRACKING_DEVICE_H