#include "ShadowVolumeManager.h"
#include "experiment.h"

// create a ShadowVolumeManager object _after_ all scene objects are included
    ShadowVolumeManager::ShadowVolumeManager(Ogre::SceneManager* sceneManager, Experiment* experiment, float lightHeight)
        : _sceneManager(sceneManager)
        , _lightHeight(lightHeight)
		,_experiment(experiment)
    {
 //       traverseSceneGraph();

		// Set initial visibility for all shadow volumes to invisible
		//toggleVisibility(false);
    }

    ShadowVolumeManager::~ShadowVolumeManager()
    {
    }

	void ShadowVolumeManager::buildShadowVolumes() {
		traverseSceneGraph();

		// Set initial visibility for all shadow volumes to invisible
		toggleVisibility(false);
	}

	void ShadowVolumeManager::purgeAll() {
		for (size_t i = 0; i < _nodeData.size(); ++i) {
            NodeData& nd = _nodeData[i];
			delete nd.shadowVolumeObject;
			delete nd.shadowVolumeObjectTop;
		}

		_nodeData.clear();
	}

	static bool compareQuat(const Ogre::Quaternion& q1, const Ogre::Quaternion& q2, float EPS) {
		if (Ogre::Math::Abs(q1.x - q2.x) < EPS &&
			Ogre::Math::Abs(q1.y - q2.y) < EPS &&
			Ogre::Math::Abs(q1.z - q2.z) < EPS &&
			Ogre::Math::Abs(q1.w - q2.w) < EPS) 
		{
				return true;
		}
		else {
			return false;
		}
	}

	static bool compareVec3(const Ogre::Vector3& v1, const Ogre::Vector3& v2, float EPS) {
		if (Ogre::Math::Abs(v1.x - v2.x) < EPS &&
			Ogre::Math::Abs(v1.y - v2.y) < EPS &&
			Ogre::Math::Abs(v1.z - v2.z) < EPS) 
		{
				return true;
		}
		else {
			return false;
		}
	}

	// call it in <code>bool frameStarted(const Ogre::FrameEvent& evt)<\code>
    void ShadowVolumeManager::update() { // updates everything @ every frame... we should check for changes, if its to slow,
					// ...but for 5-10 items it woudn't be that slow :)
		/*static int counter = 0;

		counter++;

		if (counter > 3) {
			counter = 0;
		}
		else {
			return;
		}*/

		static const float EPS = 1e-2f;

        for (size_t i = 0; i < _nodeData.size(); ++i) {
            NodeData& nd = _nodeData[i];
            Ogre::Quaternion currentOri = nd.node->_getDerivedOrientation();
			Ogre::Vector3 currentPos = nd.node->getPosition();

			//if (nd.node->getName() == "dynamic_box1_node") std::cout <<  nd.node->_getDerivedOrientation() << "\n";
			//if (nd.oldOri != currentOri || nd.oldPosition != currentPos) {
			if (!compareVec3(nd.oldPosition, currentPos, EPS) || !compareQuat(nd.oldOri, currentOri, EPS)) {
                nd.oldOri = currentOri;
				nd.oldPosition = currentPos;
                //nd.node->getChild(0)->setOrientation(currentOri.Inverse());
				nd.shadowVolumeObject->getParentNode()->setOrientation(currentOri.Inverse());
                updateShadowVolume(nd);
            }
        }
    }

	/// Toggles visibility of shadow volumes
	/// @param visible If true, shadow volumes will be set to visible. Otherwise, shadow volumes will be invisible
	///
	void ShadowVolumeManager::toggleVisibility(bool visible) {
		 for (size_t i = 0; i < _nodeData.size(); ++i) {
            NodeData& nd = _nodeData[i];
			Ogre::SceneNode* originalNode = nd.node;
			const Ogre::String shadowVolumeNodeName = "shadow_volume_node" + originalNode->getName();
			Ogre::SceneNode* shadowVolumeNode = static_cast<Ogre::SceneNode*>(originalNode->getChild(shadowVolumeNodeName));
			if (shadowVolumeNode) {
				shadowVolumeNode->setVisible(visible);
			}
			else {
				std::cout << "Error in void toggleVisibility(bool visible), node has no shadow volume !" << std::endl;
			}
		 }
	}

    void ShadowVolumeManager::traverseSceneGraph() {
		 std::vector<Ogre::SceneNode*> nodes = _experiment->experimentObjectSceneNodes();

		 for (auto iter = nodes.begin(); iter != nodes.end(); ++iter) {
			Ogre::SceneNode* node = *iter;
            NodeData nd;
            nd.node = node;
            nd.oldOri = Ogre::Quaternion::ZERO;
			nd.oldPosition = node->getPosition();
            nd.meshVerices = getMeshVertexData((static_cast<Ogre::Entity*>(node->getAttachedObject(0)))->getMesh());

            // create and init the shadow volume object...
            Ogre::ManualObject* m = _sceneManager->createManualObject("shadow_volume_object_" + node->getName());
            m->setDynamic(true);
            m->begin("ShadowInteractionTest/ShadowVolumeMaterialLines", Ogre::RenderOperation::OT_LINE_LIST);//Ogre::RenderOperation::OT_TRIANGLE_STRIP);
            m->position(0,0,0); // dummy
            m->position(1,0,0); // dummy
            m->position(0,1,0); // dummy
            m->end();
			m->begin("ShadowInteractionTest/ShadowVolumeMaterialBody", Ogre::RenderOperation::OT_TRIANGLE_STRIP);
            m->position(0,0,0); // dummy
            m->position(1,0,0); // dummy
            m->position(0,1,0); // dummy
            m->end();
			nd.shadowVolumeObject = m;

			// manual object 2 (cup)
			m = _sceneManager->createManualObject("shadow_volume_object_top" + node->getName());
			m->setDynamic(true);
			m->begin("ShadowInteractionTest/ShadowVolumeMaterial", Ogre::RenderOperation::OT_TRIANGLE_FAN);
            m->position(0,0,0); // dummy
            m->position(1,0,0); // dummy
            m->position(0,1,0); // dummy
            m->end();
			nd.shadowVolumeObjectTop = m;
            
            _nodeData.push_back(nd);
            updateShadowVolume(_nodeData.back());

            // ...and add it to the scene
			Ogre::SceneNode* shadowNode = node->createChildSceneNode("shadow_volume_node" + node->getName());
			shadowNode->attachObject(_nodeData.back().shadowVolumeObject);
			shadowNode->attachObject(_nodeData.back().shadowVolumeObjectTop);


			node->getUserObjectBindings().setUserAny("old_position", Ogre::Any(node->getPosition()));

            //node->createChildSceneNode(node->getName() + "_shadow_volume_node"/*getUniqueSceneNodeName()*/)->attachObject(_nodeData.back().shadowVolumeObject);
        }

        //// recursion
        //Ogre::SceneNode::ChildNodeIterator it = node->getChildIterator();
        //while (it.hasMoreElements()) {
        //    Ogre::SceneNode* child = dynamic_cast<Ogre::SceneNode*>(it.getNext());
        //    if (child)
        //        traverseSceneGraph(child);
        //    else
        //        std::cout << "----- Found child node, which is not a SceneNode -------\n";
        //}
    }

    void ShadowVolumeManager::updateShadowVolume(NodeData& nd) {
        std::vector<Ogre::Vector3> contourVertices = get2DConvexHull(nd.meshVerices, nd.node->_getDerivedOrientation());
        Ogre::Vector3 lightPosition = nd.node->_getDerivedPosition();
        lightPosition.x = 0;
        lightPosition.y = 0;
        //lightPosition.z = _lightHeight - lightPosition.z;
        Ogre::ManualObject* m = nd.shadowVolumeObject;
		Ogre::ManualObject* mTop = nd.shadowVolumeObjectTop;

        m->estimateVertexCount(contourVertices.size() + 10);
        

		// -----------------
		// Screen surface plane
		const Ogre::Plane surfacePlane = Ogre::Plane(Ogre::Vector3(0.0f, 0.0f, 1.0f), -lightPosition.z);
		if (lightPosition.z > 0)
			lightPosition.z = -_lightHeight - lightPosition.z;
		else
			lightPosition.z = _lightHeight - lightPosition.z;

		// Fill in collection of vertices on the screen surface
		std::vector<Ogre::Vector3> intersectionVertices;
		for (size_t i = 0; i < contourVertices.size(); ++i) {
			const Ogre::Ray ray = Ogre::Ray(contourVertices[i], lightPosition - contourVertices[i]);
			const std::pair<bool, Ogre::Real> result = ray.intersects(surfacePlane);

			if (!result.first) {
				//std::cout << "ERROR !!" << std::endl;
				intersectionVertices.push_back(contourVertices[i]);//lightPosition);
			}
			else {
				intersectionVertices.push_back(ray.getPoint(result.second));
			}
        }
		// -----------------
		// body - lines
		m->beginUpdate(0);

        for (size_t i = 0; i < contourVertices.size(); ++i) {
            m->position(contourVertices[i]);
			m->position(intersectionVertices[i]);
        }

		for (size_t i = 0; i < intersectionVertices.size() - 1; ++i) {
			m->position(intersectionVertices[i]);
			m->position(intersectionVertices[i + 1]);
        }

        m->end();

		// -----------------
		// body - fill
		m->beginUpdate(1);

        for (size_t i = 0; i < contourVertices.size(); ++i) {
            m->position(contourVertices[i]);
			m->position(intersectionVertices[i]);
        }
        m->end();

		// -----------------
		// cup
		mTop->estimateVertexCount(intersectionVertices.size());

		mTop->beginUpdate(0);

        for (size_t i = 0; i < intersectionVertices.size(); ++i) {
			mTop->position(intersectionVertices[i]);
        }

        mTop->end();
    }

    // -----------------------------------------------------------------------------------------------
    // modified version from http://www.ogre3d.org/tikiwiki/tiki-index.php?page=RetrieveVertexData
    // -----------------------------------------------------------------------------------------------
    std::vector<Ogre::Vector3> ShadowVolumeManager::getMeshVertexData(const Ogre::MeshPtr mesh) {
        bool added_shared = false;
        size_t current_offset = 0;
        size_t shared_offset = 0;
        size_t next_offset = 0;
        size_t index_offset = 0;

        size_t vertex_count = 0, index_count = 0;
	    std::vector<Ogre::Vector3> vertices;

        // Calculate how many vertices and indices we're going to need
        for ( unsigned short i = 0; i < mesh->getNumSubMeshes(); ++i)
        {
            Ogre::SubMesh* submesh = mesh->getSubMesh(i);
            // We only need to add the shared vertices once
            if(submesh->useSharedVertices)
            {
                if( !added_shared )
                {
                    vertex_count += mesh->sharedVertexData->vertexCount;
                    added_shared = true;
                }
            }
            else
            {
                vertex_count += submesh->vertexData->vertexCount;
            }
            // Add the indices
            //index_count += submesh->indexData->indexCount;
        }

        // Allocate space for the vertices and indices
        vertices.resize(vertex_count);
        //indices = new unsigned long[index_count];

        added_shared = false;

        // Run through the submeshes again, adding the data into the arrays
        for (unsigned short i = 0; i < mesh->getNumSubMeshes(); ++i)
        {
            Ogre::SubMesh* submesh = mesh->getSubMesh(i);

            Ogre::VertexData* vertex_data = submesh->useSharedVertices ? mesh->sharedVertexData : submesh->vertexData;

            if ((!submesh->useSharedVertices) || (submesh->useSharedVertices && !added_shared))
            {
                if(submesh->useSharedVertices)
                {
                    added_shared = true;
                    shared_offset = current_offset;
                }

                const Ogre::VertexElement* posElem =
                    vertex_data->vertexDeclaration->findElementBySemantic(Ogre::VES_POSITION);

                Ogre::HardwareVertexBufferSharedPtr vbuf =
                    vertex_data->vertexBufferBinding->getBuffer(posElem->getSource());

                unsigned char* vertex =
                    static_cast<unsigned char*>(vbuf->lock(Ogre::HardwareBuffer::HBL_READ_ONLY));

                // There is _no_ baseVertexPointerToElement() which takes an Ogre::Real or a double
                //  as second argument. So make it float, to avoid trouble when Ogre::Real will
                //  be comiled/typedefed as double:
                //Ogre::Real* pReal;
                float* pReal;

                for( size_t j = 0; j < vertex_data->vertexCount; ++j, vertex += vbuf->getVertexSize())
                {
                    posElem->baseVertexPointerToElement(vertex, &pReal);
                    Ogre::Vector3 pt(pReal[0], pReal[1], pReal[2]);
                    vertices[current_offset + j] = pt;//(orient * (pt * scale)) + position;
                }

                vbuf->unlock();
                next_offset += vertex_data->vertexCount;
            }

            //Ogre::IndexData* index_data = submesh->indexData;
            //size_t numTris = index_data->indexCount / 3;
            //Ogre::HardwareIndexBufferSharedPtr ibuf = index_data->indexBuffer;

            //bool use32bitindexes = (ibuf->getType() == Ogre::HardwareIndexBuffer::IT_32BIT);

            //unsigned long* pLong = static_cast<unsigned long*>(ibuf->lock(Ogre::HardwareBuffer::HBL_READ_ONLY));
            //unsigned short* pShort = reinterpret_cast<unsigned short*>(pLong);

            // size_t offset = (submesh->useSharedVertices)? shared_offset : current_offset;

            // if ( use32bitindexes )
            // {
                // for ( size_t k = 0; k < numTris*3; ++k)
                // {
                    // indices[index_offset++] = pLong[k] + static_cast<unsigned long>(offset);
                // }
            // }
            // else
            // {
                // for ( size_t k = 0; k < numTris*3; ++k)
                // {
                    // indices[index_offset++] = static_cast<unsigned long>(pShort[k]) +
                                              // static_cast<unsigned long>(offset);
                // }
            // }

            //ibuf->unlock();
            current_offset = next_offset;
        }

	    return vertices;
    }

	// --------------------------------------------------------------------------------------------
    // Implementation of Andrew's monotone chain 2D convex hull algorithm.
	// --------------------------------------------------------------------------------------------
    bool ShadowVolumeManager::lex2DCompare(const Ogre::Vector3& lop, const Ogre::Vector3& rop) {
	    return (lop.x < rop.x) || (lop.x == rop.x && lop.y < rop.y) || (lop.x == rop.x && lop.y == rop.y && lop.z > rop.z);
    }
    bool ShadowVolumeManager::lex2DDuplicate(const Ogre::Vector3& lop, const Ogre::Vector3& rop) {
	    return (lop.x == rop.x) && (lop.y == rop.y);
    }
    bool ShadowVolumeManager::crossLEQZero(const Ogre::Vector3& O, const Ogre::Vector3& A, const Ogre::Vector3& B) {
	    return ((A.x - O.x) * (B.y - O.y) - (A.y - O.y) * (B.x - O.x)) <= 0.0f;
    }

    // Note: the last point in the returned list is the same as the first one.
    std::vector<Ogre::Vector3> ShadowVolumeManager::get2DConvexHull(const std::vector<Ogre::Vector3>& vxlist, const Ogre::Quaternion& ori) {
	    // copy and transform all vertex positions
	    std::vector<Ogre::Vector3> P(vxlist);
	    Transform transform(ori);

	    // Sort points lexicographically and remove the duplicates
		const float eps = 0.1f;
	    std::for_each(P.begin(), P.end(), transform);
		for (size_t i = 0; i < P.size() - 1; ++i) {
			for (size_t j = i+1; j < P.size(); ++j) {
				if ((std::abs(P[i].x - P[j].x) <= eps) && (std::abs(P[i].y - P[j].y) <= eps)) {
					if (P[i].z < P[j].z)
						std::swap(P[i], P[j]);
					P.erase(P.begin() + j);
					j--;
				}
			}
		}
	    
		std::sort(P.begin(), P.end(), lex2DCompare);
	    //P.erase(std::unique(P.begin(), P.end(), lex2DDuplicate), P.end());
		/*for (size_t i = 0; i < P.size(); ++i) {
			while (i + 1 < P.size() && lex2DDuplicate(P[i], P[i+1])) P.erase(P.begin() + i + 1);
		}*/

        int n = (int)P.size();
	    int k = 0;
	    std::vector<Ogre::Vector3> H(2 * n);

	    // Build lower hull
	    for (int i = 0; i < n; ++i) {
		    while (k >= 2 && crossLEQZero(H[k-2], H[k-1], P[i])) k--;
		    H[k++] = P[i];
	    }

	    // Build upper hull
	    for (int i = n-2, t = k+1; i >= 0; i--) {
		    while (k >= t && crossLEQZero(H[k-2], H[k-1], P[i])) k--;
		    H[k++] = P[i];
	    }

	    H.resize(k);
	    return H;
    }