using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.Visualization;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class FrameUpdater : MonoBehaviour
{
    string frame_id;

    public void Init(string frame_id, Transform parent)
    {
        this.frame_id = frame_id;
        transform.SetParent(parent);
    }

    // Update is called once per frame
    void Update()
    {
        if (frame_id == null) return;
        TFFrame frame = TFSystem.instance.GetTransform(frame_id, 0);
        // Add check to make sure frame exists
        transform.localPosition = frame.translation;
        transform.localRotation = frame.rotation;
    }
}
