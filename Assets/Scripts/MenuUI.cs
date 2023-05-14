using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector;
using TMPro;

public class MenuUI : MonoBehaviour
{
    public TMP_InputField qrCodeLink;
    public TMP_InputField qrCodeLength;
    public TMP_InputField baseLink;
    public TMP_InputField poseTopic;
    public TMP_InputField ipAddress;
    public TMP_InputField port;
    public Button connectButton;
    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        connectButton.onClick.AddListener(ConnectCallback);
    }

    void ConnectCallback()
    {
        Debug.Log("connect");
        ros.RosIPAddress = ipAddress.text;
        int portInt;
        int.TryParse(port.text, out portInt);
        ros.RosPort = portInt;
    }
}
