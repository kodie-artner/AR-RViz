using System.Collections;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Rosgraph;
using UnityEngine;

namespace Unity.Robotics.Visualizations{
    public class ARLocator : MonoBehaviour
    {
        TFSystem tfSystem;
        public Transform mapTransform;
        public string qrFrame = "center_of_screen";
        public float angle = 0;
        Vector3 trackerPosition;
        float trackerHeading;
        ClockMsg latestClock;
        ROSConnection ros;

        //public GameObject robot;
        // Start is called before the first frame update
        void Start()
        {
            // Subscribe to /clock
            ros = ROSConnection.GetOrCreateInstance();
            ros.Subscribe<ClockMsg>("clock", ReceiveClock);
        }

        // Update is called once per frame
        void Update()
        {
            GameObject tracker = GameObject.FindGameObjectWithTag("tracker");
            if (Input.touchCount > 0 && tracker != null)
            {
                tracker.GetComponent<MeshRenderer>().enabled = true;
                if (GetTrackingCube()) 
                {
                    SetRobotPositionInUnity();
                }        
            }
            else if (tracker != null)
            {
                tracker.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        void ReceiveClock(ClockMsg msg)
        {
            latestClock = msg;
        }
        
        bool GetTrackingCube()
        {
            GameObject tracker = GameObject.FindGameObjectWithTag("tracker");
            if (tracker != null)
        {       
                // project on plane
                Vector3 heading = Vector3.ProjectOnPlane(tracker.transform.forward, Vector3.up);
                // turn into vector2 and rotate 180 deg
                Vector2 heading2d = new Vector2(heading.x, heading.z) * -1; 
                trackerHeading = Vector2.SignedAngle(heading2d, Vector2.up);
                trackerPosition = tracker.transform.position;
                return true;
            }
            return false;
        }

        void SetRobotPositionInUnity(){
            TFFrame robot_tf = TFSystem.instance.GetTransform(frame_id: "base_footprint", time: latestClock.clock);
            Debug.Log("base_footprint position" + robot_tf.translation);
        
            TFFrame T_map_screen = TFSystem.instance.GetTransform(frame_id: qrFrame, time: latestClock.clock);

            Debug.Log("tf: " + T_map_screen.translation);
            Quaternion tracker_rotation = Quaternion.Euler(0, trackerHeading, 0);
            
            mapTransform.position =  trackerPosition - tracker_rotation * Quaternion.Inverse(robot_tf.rotation) * T_map_screen.translation;
            mapTransform.rotation =  tracker_rotation * Quaternion.Inverse(robot_tf.rotation);
            
            Debug.Log("Visualizer: " +  mapTransform.position + " angle: " + mapTransform.rotation.eulerAngles);
        }
    }
}