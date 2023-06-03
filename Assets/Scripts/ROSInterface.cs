using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

// ROSInterface is a interface between ROSConnection and other scripts for common calls to ROS.
public class ROSInterface : MonoBehaviour
{
    private static ROSInterface _instance;
    private ROSConnection ros;
    private string poseTopicRegistered = "";
    private Transform mapTransform;

    public static ROSInterface GetOrCreateInstance()
    {
        if (_instance == null)
        {
            // Prefer to use the ROSInterface in the scene, if any
            _instance = FindObjectOfType<ROSInterface>();
            if (_instance != null)
                return _instance;

            GameObject instance = new GameObject("ROSInterface");
            _instance = instance.AddComponent<ROSInterface>();
        }
        return _instance;
    }

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        mapTransform = GameObject.FindGameObjectWithTag("Map").transform;
    }

    public void PublishPoseStampedMsg(Vector3 position, float angle, string frame, string topic)
    {
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        PublishPoseStampedMsg(position, rotation, frame, topic);
    }

    public void PublishPoseStampedMsg(Vector3 position, Quaternion rotation, string frame, string topic)
    {
        // Convert unity transform into ros and frame transform.
        // TODO: Do this with math just multiplying transforms
        TFFrame frameTF = TFSystem.instance.GetTransform(frame_id: frame, time: 0);
        GameObject frameObj = new GameObject();
        frameObj.transform.SetParent(mapTransform);
        frameObj.transform.localPosition = frameTF.translation;
        frameObj.transform.localRotation = frameTF.rotation;
        // Create pose object with unity coordinate frame
        GameObject poseObj = new GameObject();
        poseObj.transform.position = position;
        poseObj.transform.rotation = rotation;

        // Get poseObj in the header frame
        poseObj.transform.SetParent(frameObj.transform);

        PoseStampedMsg msg = new RosMessageTypes.Geometry.PoseStampedMsg();
        msg.header.frame_id = frame;
        // TODO: Get time from Tf header, update TFSystem to make this easily accessible
        //msg.header.stamp = ;
        msg.pose.position = poseObj.transform.localPosition.To<FLU>();
        msg.pose.orientation = poseObj.transform.localRotation.To<FLU>();
        PublishPoseStampedMsg(msg, topic);

        // Destroy temporary game objects
        Destroy(frameObj);
        Destroy(poseObj);
    }

    void PublishPoseStampedMsg(RosMessageTypes.Geometry.PoseStampedMsg msg, string topic)
    {
        ros.Publish(topic, msg);
    }

    public void RegisterPoseTopicIfNotRegistered(string topic)
    {
        // Check if Pose topic is set and register the topic is so
        if (topic != "" && poseTopicRegistered != topic)
        {
            ros.RegisterPublisher<PoseStampedMsg>(topic);
            poseTopicRegistered = topic;
        }
    }
}
