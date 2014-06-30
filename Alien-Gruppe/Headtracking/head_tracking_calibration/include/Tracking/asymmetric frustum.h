#pragma once

#include "globals.h"

void getAsymmetricFrustum(viargo::vec3f cameraPosition, float& nearLeft, float& nearRight, float& nearTop, float& nearBottom, float& focal, float nearDist) {
	// TODO: Do not forget to adjust camera.LookAt(..) such that camera looks perpendicular to image plane on (cam.x, cam.y, 0.0)
	
	// Constants
	//static float width = 47.5f; // Screen surface width in cm
	//static float height = 29.5f; // Screen surface height in cm

	static float width = global::tableWidth; // displayWidth; // Screen surface width in cm
	static float height = global::tableHeight; // displayHeight; // Screen surface height in cm

	// Focal length = orthogonal distance to image plane
	focal = cameraPosition.z;

	// Ratio for intercept theorem
	float ratio = focal / nearDist;

	// Compute size for focal
	float imageLeft   = (-width/2.0)  - cameraPosition.x;
	float imageRight  = (width/2.0)   - cameraPosition.x;
	float imageTop    = (height/2.0)  - cameraPosition.y;
	float imageBottom = (-height/2.0) - cameraPosition.y;

	// Intercept theorem
	nearLeft   = imageLeft   / ratio;
	nearRight  = imageRight  / ratio;
	nearTop    = imageTop    / ratio;
	nearBottom = imageBottom / ratio;
}
