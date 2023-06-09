using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using TMPro;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

// MenuUI is the handles all the logic in the menu.
public class MenuUI : MonoBehaviour
{
    // The public fields need to be set in the inspector
    public ARTrackedImageManager trackedImageManager;
    public AROcclusionManager occlusionManager;
    public List<XRReferenceImageLibrary> libraries;

    // Link in urdf that corresponds to the QR Code
    public TMP_Dropdown qrCodeDropdown;
    public string qrCodeLink
    {
        get => qrCodeDropdown.options[qrCodeDropdown.value].text;
    }

    // Length of the QR Code edge
    public Slider qrCodeSliderLength;
    public TMP_Text qrCodeLength;

    // Base link of robot. Should have z axis up and on the same plane as map
    public TMP_Dropdown baseLinkDropdown;
    public string baseLink
    {
        get => baseLinkDropdown.options[baseLinkDropdown.value].text;
    }

    // Topic to send a PoseStamped msg on. ex. /nav_goal, /initialpose
    public TMP_InputField poseTopic;

    // Link to be used as frame_id in PoseStampedMsg
    public TMP_Dropdown poseLinkDropdown;
    public string poseLink
    {
        get => poseLinkDropdown.options[poseLinkDropdown.value].text;
    }

    public Toggle occlusionToggle;
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
    private Dictionary<string, TopicVisualizer> topics = new Dictionary<string, TopicVisualizer>();
    private int topicsShown = 0;
    private float scrollViewItemHeight;
    private int scrollViewPadding = 20;
    private string connectButtonText = null;
    private float timer;
    private float interval = 1f;

    void Start()
    {
        scrollViewItemHeight = topicSelectPrefab.GetComponent<RectTransform>().sizeDelta.y;
        ros = ROSConnection.GetOrCreateInstance();
        occlusionToggle.onValueChanged.AddListener(OnOcclusionToggle);
        isROS2Toggle.onValueChanged.AddListener(OnROS2Toggle);
        connectButton.onClick.AddListener(ConnectCallback);
        filterTopics.onValueChanged.AddListener(FilterCallback);
        qrCodeSliderLength.onValueChanged.AddListener(SetQrCodeLength);
        ros.ListenForTopics(OnNewTopic, notifyAllExistingTopics: true);
        SetQrCodeLength(qrCodeSliderLength.value);
        // TODO: don't use this long term
        InvokeRepeating("UpdateUI", 1, 1);
    }

    void Update()
    {
        if (connectButtonText != null)
        {
            connectButton.GetComponentInChildren<TMP_Text>().text = connectButtonText;
            connectButtonText = null;
            UpdateUI();
        }
    }

    void OnNewTopic(RosTopicState state)
    {
        TopicVisualizer topicVisualizer;
        if (!topics.TryGetValue(state.Topic, out topicVisualizer))
        {
            topicVisualizer = Instantiate(topicSelectPrefab, scrollViewContent.transform)
                .GetComponent<TopicVisualizer>();
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
        ros.Connect(ConnectionStartedCB, ConnectedEndedCB);
        // Update button text
        connectButtonText = "Trying To Connect...";
    }

    public void UpdateUI()
    {
        Debug.Log("updating");
        if (TFSystem.instance == null)
        {
            return;
        }
        List<string> options = TFSystem.instance.GetTransformNames().ToList();
        options.Insert(0, "");

        // Get current values. The order won't change when new tf is added
        int qrCodeValue = qrCodeDropdown.value;
        int baseLinkValue = baseLinkDropdown.value;
        int poseLinkValue = poseLinkDropdown.value;

        qrCodeDropdown.ClearOptions();
        baseLinkDropdown.ClearOptions();
        poseLinkDropdown.ClearOptions();
        qrCodeDropdown.AddOptions(options);
        baseLinkDropdown.AddOptions(options);
        poseLinkDropdown.AddOptions(options);

        qrCodeDropdown.value = qrCodeValue;
        baseLinkDropdown.value = baseLinkValue;
        poseLinkDropdown.value = poseLinkValue;
    }

    void OnROS2Toggle(bool enabled)
    {
        ros.ROS2 = enabled;
    }

    void OnOcclusionToggle(bool enabled)
    {
        occlusionManager.enabled = enabled;
    }

    void FilterCallback(string filter)
    {
        topicsShown = 0;
        foreach (KeyValuePair<string, TopicVisualizer> item in topics)
        {
            // If no filter, show all topics
            if (filter == "")
            {
                item.Value.gameObject.SetActive(true);
                topicsShown++;
            }
            else if (
                !item.Key.ToLower().Contains(filter.ToLower())
                && !item.Value.topicState.RosMessageName.ToLower().Contains(filter.ToLower())
            )
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
        // Offset by 5 because we start at 5cm
        trackedImageManager.referenceLibrary = libraries[(int)length - 5];
    }

    public void ConnectionStartedCB(NetworkStream stream)
    {
        // Can't set text directly because we aren't on the main thread
        connectButtonText = "Connected";
        ros.OnConnectionStartedCallback(stream);
    }

    public void ConnectedEndedCB()
    {
        // Can't set text directly because we aren't on the main thread
        connectButtonText = "Failed To Connect";
        ros.OnConnectionLostCallback();
    }

    private void UpdateScrollViewHeight(float height)
    {
        var rectTransform = scrollViewContent.GetComponent<RectTransform>();
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.y = height + scrollViewPadding;
        rectTransform.sizeDelta = sizeDelta;
    }
}
