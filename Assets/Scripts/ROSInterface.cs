using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph;
using RosMessageTypes.Geometry;

public class ROSInterface : MonoBehaviour
{
    static ROSInterface _instance;
    ROSConnection ros;
    ClockMsg latestClock;

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

    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60;
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<ClockMsg>("/clock", ReceiveClock);
    }

    public ClockMsg GetLastestClock()
    {
        return latestClock;
    }

    void ReceiveClock(ClockMsg msg)
    {
        Debug.Log("got clock");
        latestClock = msg;
    }

    public void PublishPoseStampedMsg(Vector3 position, float angle, string frame, string topic)
    {
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        PublishPoseStampedMsg(position, rotation, frame, topic);
    }

    public void PublishPoseStampedMsg(Vector3 position, Quaternion rotation, string frame, string topic)
    {
        PoseStampedMsg msg = new RosMessageTypes.Geometry.PoseStampedMsg();
        msg.header.frame_id = frame;
        msg.header.stamp = latestClock.clock;
        msg.pose.position.x = position.x;
        msg.pose.position.y = position.y;
        msg.pose.position.z = position.z;
        msg.pose.orientation.x = rotation.x;
        msg.pose.orientation.y = rotation.y;
        msg.pose.orientation.z = rotation.z;
        msg.pose.orientation.w = rotation.w;
        PublishPoseStampedMsg(msg, topic);
    }
    void PublishPoseStampedMsg(RosMessageTypes.Geometry.PoseStampedMsg msg, string topic)
    {
        ros.RegisterPublisher(topic, "PoseStampedMsg");
    }
}