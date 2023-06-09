using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosMessageTypes.Std;
using RosMessageTypes.Visualization;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

// InteractiveMarker is a class for handing InteractiveMarker behaviors.
public class InteractiveMarker : MonoBehaviour
{
    const float timeDelay = 2;
    Vector3 expandedScale = new Vector3(1.1f, 1.1f, 1.1f);
    string feedbackTopic;
    ROSConnection ros;
    float lastMoveTime = 0;
    string name = "";
    string frame_id = "";
    Transform map;
    public bool publish = false;
    private Drawing3d drawing;

    // Start is called before the first frame update
    void Awake()
    {
        ros = ROSConnection.GetOrCreateInstance();
    }

    void Start()
    {
        drawing = Drawing3dManager.CreateDrawing();
        drawing.transform.SetParent(this.transform);
        drawing.transform.localPosition = Vector3.zero;
        drawing.transform.localRotation = Quaternion.identity;
        DrawAxisVectors();
    }

    // Update is called once per frame
    void Update()
    {
        if (publish)
        {
            PublishFeedback();
        }
    }

    public void Expand(bool expand)
    {
        if (expand)
        {
            transform.localScale = expandedScale;
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    public void DrawAxisVectors()
    {
        Vector3 x,
            y,
            z;
        float size = 0.15f;

        x = new Vector3<FLU>(1, 0, 0).toUnity * size;
        y = new Vector3<FLU>(0, 1, 0).toUnity * size;
        z = new Vector3<FLU>(0, 0, 1).toUnity * size;

        float thickness = 0.1f * size;
        float arrowheadScale = 2;
        float arrowheadGradient = .5f;
        drawing.DrawArrow(Vector3.zero, x, Color.red, thickness, arrowheadScale, arrowheadGradient);
        drawing.DrawArrow(Vector3.zero, y, Color.green, thickness, arrowheadScale, arrowheadGradient);
        drawing.DrawArrow(Vector3.zero, z, Color.blue, thickness, arrowheadScale, arrowheadGradient);
        drawing.DrawCuboid(Vector3.zero, new Vector3(.03f, .03f, .03f), Color.white);
    }

    public void UpdatePosition(Vector3 position)
    {
        transform.position = position;
        lastMoveTime = Time.time;
        PublishFeedback();
    }

    public void UpdateRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
        lastMoveTime = Time.time;
        PublishFeedback();
    }

    public void UpdateTransform(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        lastMoveTime = Time.time;
        PublishFeedback();
    }

    private void PublishFeedback()
    {
        InteractiveMarkerFeedbackMsg msg = new InteractiveMarkerFeedbackMsg();
        // TODO: Update msg
        msg.client_id = SystemInfo.deviceUniqueIdentifier;
        msg.marker_name = name;
        msg.header.frame_id = frame_id;
        msg.event_type = 1;
        msg.pose.position = transform.position.To<FLU>();
        msg.pose.orientation = transform.rotation.To<FLU>();

        ros.Publish(feedbackTopic, msg);
    }

    public void SetFeedbackTopic(string topic)
    {
        feedbackTopic = topic;
    }

    public void Initialize(InteractiveMarkerMsg msg)
    {
        map = GameObject.FindGameObjectWithTag("Map").transform;
        name = msg.name;
        frame_id = msg.header.frame_id;
        Transform frame_transform = map.Find(frame_id);
        if (frame_transform == null)
        {
            // Instantiate a frame object
            GameObject frameUpdater = new GameObject(msg.header.frame_id);
            frame_transform = frameUpdater.transform;
            frameUpdater.AddComponent<FrameUpdater>().Init(msg.header.frame_id, map);
        }
        transform.SetParent(frame_transform);
        UpdateLatestMsg(msg);
    }

    public void UpdateLatestMsg(InteractiveMarkerMsg msg)
    {
        if (Time.time - lastMoveTime > timeDelay)
        {
            transform.localPosition = msg.pose.position.From<FLU>();
            transform.localRotation = msg.pose.orientation.From<FLU>();
        }
        else
        {
            Debug.Log("Just moved, not applying update msg");
        }
    }
}
