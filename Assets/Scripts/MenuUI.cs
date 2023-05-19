using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using TMPro;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

public class MenuUI : MonoBehaviour
{
    public ARTrackedImageManager trackedImageManager;
    public List<XRReferenceImageLibrary> libraries;

    // Link in urdf that corresponds to the QR Code
    public TMP_InputField qrCodeLink;
    // Length of the QR Code edge
    public Slider qrCodeSliderLength;
    public TMP_Text qrCodeLength;
    // Base link of robot. Should have z axis up and on the same plane as map
    public TMP_InputField baseLink;
    // Topic to send a PoseStamped msg on. ex. /nav_goal, /initialpose
    public TMP_InputField poseTopic;
    public Toggle isROS2Toggle;
    // IP address of the ros_tcp_endpoint node
    public TMP_InputField ipAddress;
    // Port number of the ros_tcp_endpoint node
    public TMP_InputField port;
    // Connect to ros button
    public Button connectButton;
    public TMP_InputField filterTopics;
    public GameObject scrollViewContent;
    public GameObject topicSelectPrefab;
    private ROSConnection ros;
    Dictionary<string, TopicVisualizer> topics = new Dictionary<string, TopicVisualizer>();
    int topicsShown = 0;
    float scrollViewItemHeight;
    int scrollViewPadding = 20;

    void Start()
    {
        scrollViewItemHeight = topicSelectPrefab.GetComponent<RectTransform>().sizeDelta.y;
        ros = ROSConnection.GetOrCreateInstance();
        isROS2Toggle.onValueChanged.AddListener(OnROS2Toggle);
        connectButton.onClick.AddListener(ConnectCallback);
        filterTopics.onValueChanged.AddListener(FilterCallback);
        qrCodeSliderLength.onValueChanged.AddListener(SetQrCodeLength);
        ros.ListenForTopics(OnNewTopic, notifyAllExistingTopics: true);
        SetQrCodeLength(qrCodeSliderLength.value);
    }

    void OnNewTopic(RosTopicState state)
    {
        // TODO: Not getting called when new topics are available
        TopicVisualizer topicVisualizer;
        if (!topics.TryGetValue(state.Topic, out topicVisualizer))
        {
            topicVisualizer = Instantiate(topicSelectPrefab, scrollViewContent.transform).GetComponent<TopicVisualizer>();
            topicVisualizer.Initialize(state);
            topics.Add(state.Topic, topicVisualizer);
            UpdateScrollViewHeight(++topicsShown * scrollViewItemHeight);
        }
    }

    void ConnectCallback()
    {
        // First disconnect current connection
        ros.Disconnect();
        // Set IP and Port in ROSConnection
        ros.RosIPAddress = ipAddress.text;
        int portInt;
        int.TryParse(port.text, out portInt);
        ros.RosPort = portInt;
        // Connect to ros
        ros.Connect();
        // Update button text
        connectButton.GetComponentInChildren<TMP_Text>().text =  "Trying To Connect...";
    }

    void OnROS2Toggle(bool enabled)
    {
        ros.ROS2 = enabled;
    }

    void FilterCallback(string filter)
    {
        topicsShown = 0;
        foreach (KeyValuePair<string, TopicVisualizer> item in topics)
        {
            // If no filter, show topic
            if (filter == "")
            {
                item.Value.gameObject.SetActive(true);
                topicsShown++;
            }
            // If 
            else if (!item.Key.ToLower().Contains(filter) && !item.Value.topicState.RosMessageName.ToLower().Contains(filter))
            {
                item.Value.gameObject.SetActive(false);
            }
            else
            {
                item.Value.gameObject.SetActive(true);
                topicsShown++;
            }
        }
        UpdateScrollViewHeight(topicsShown * scrollViewItemHeight);
    }

    void SetQrCodeLength(float length)
    {
        qrCodeLength.text = length.ToString();
        trackedImageManager.referenceLibrary = libraries[(int)length - 1];
        trackedImageManager.enabled = true;
    }
    void ValidateQRCodeLength(string input)
    {
        float validFloat;
        if (!float.TryParse(input, out validFloat) )
        {
            if (input != ".")
            {
                qrCodeLength.text = "";
            }
        }
        else
        {
            // Set size
        }
    }

    public void ConnectionStartedCB(NetworkStream stream)
    {
        connectButton.GetComponentInChildren<TMP_Text>().text = "Connected";
    }

    public void ConnectedEndedCB()
    {
        connectButton.GetComponentInChildren<TMP_Text>().text = "Connect";
    }

    private void UpdateScrollViewHeight(float height)
    {
        var rectTransform = scrollViewContent.GetComponent<RectTransform>();
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = height + scrollViewPadding;
        rectTransform.sizeDelta = sizeDelta;
    }
}
