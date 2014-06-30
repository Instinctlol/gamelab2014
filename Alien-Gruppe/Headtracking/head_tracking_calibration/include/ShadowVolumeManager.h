#ifndef __ShadowVolumeManager_h_
#define __ShadowVolumeManager_h_

#include "BaseApplication.h"


#include <sstream>
#include <algorithm>
#include <vector>


class Experiment;

class ShadowVolumeManager {
public:
	// create a ShadowVolumeManager object _after_ all scene objects are included
    ShadowVolumeManager(Ogre::SceneManager* sceneManager, Experiment* experiment, float lightHeight);
    ~ShadowVolumeManager();

	void buildShadowVolumes();
	void purgeAll();

	// call it in <code>bool frameStarted(const Ogre::FrameEvent& evt)<\code>
    void update();

	/// Toggles visibility of shadow volumes
	/// @param visible If true, shadow volumes will be set to visible. Otherwise, shadow volumes will be invisible
	///
	void toggleVisibility(bool visible);

private:
    Ogre::SceneManager* _sceneManager;
	Experiment* _experiment;
    float _lightHeight;

protected:
    struct NodeData {
        Ogre::SceneNode* node;
        Ogre::Quaternion oldOri;
		Ogre::Vector3 oldPosition;
        Ogre::ManualObject* shadowVolumeObject;
		Ogre::ManualObject* shadowVolumeObjectTop;
        std::vector<Ogre::Vector3> meshVerices;
    };
    std::vector<NodeData> _nodeData;

    // returns <code>true<\code> iff the node, or some of it children have interactive objects attached
	// TODO: alex: fit it to your system!!!
  //  bool isNodeInteractive(Ogre::SceneNode* node) { // TODO: dimi: fit me!
  //      //return (node == _sceneManager->getRootSceneNode() || node->getName()[0] == '_');

		//const Ogre::String nodeName = node->getName();
		//
		//if (node == _sceneManager->getRootSceneNode() ||
		//	nodeName == "dynamic_box1_node" ||
		//	nodeName == "dynamic_box2_node" ||
		//	nodeName == "dynamic_box3_node" ||
		//	nodeName == "dynamic_box4_node") {
		//	return true;
		//}
		//else {
		//	return false;
		//}

		///*if (nodeName.substr(0, 11) == "dynamic_box" || node == _sceneManager->getRootSceneNode()) {
		//	return true;
		//}
		//else {
		//	return false;
		//}*/
  //  }

    void traverseSceneGraph();

    //std::string getUniqueManualObjectName() const {
    //    static int counter = 0;
    //    std::ostringstream oss;
    //    oss << "ShadowVolumeObject" << (counter++);
    //    return oss.str();
    //}

    // std::string getUniqueSceneNodeName() const {
    //    static int counter = 0;
    //    std::ostringstream oss;
    //    oss << "ShadowVoluneNode" << (counter++);
    //    return oss.str();
    //}

    void updateShadowVolume(NodeData& nd);

    // -----------------------------------------------------------------------------------------------
    // modified version from http://www.ogre3d.org/tikiwiki/tiki-index.php?page=RetrieveVertexData
    // -----------------------------------------------------------------------------------------------
    static std::vector<Ogre::Vector3> getMeshVertexData(const Ogre::MeshPtr mesh);

	// --------------------------------------------------------------------------------------------
    // Implementation of Andrew's monotone chain 2D convex hull algorithm.
	// --------------------------------------------------------------------------------------------
    static bool lex2DCompare(const Ogre::Vector3& lop, const Ogre::Vector3& rop);
    static bool lex2DDuplicate(const Ogre::Vector3& lop, const Ogre::Vector3& rop);
    static bool crossLEQZero(const Ogre::Vector3& O, const Ogre::Vector3& A, const Ogre::Vector3& B);

    class Transform {
	    public:
		    Transform(const Ogre::Quaternion& quat) : q(quat){}
			void operator() (Ogre::Vector3& p) {
				const float f = 1e4f;
				const float invf = 1.0f/f;
				p=f*(q*p); 
				p = invf * Ogre::Vector3(std::ceil(p.x), std::ceil(p.y), std::ceil(p.z));
			}
        private:
            Ogre::Quaternion q;
    };

    // Note: the last point in the returned list is the same as the first one.
    static std::vector<Ogre::Vector3> get2DConvexHull(const std::vector<Ogre::Vector3>& vxlist, const Ogre::Quaternion& ori);
};

#endif // #ifndef __ShadowVolumeManager_h_