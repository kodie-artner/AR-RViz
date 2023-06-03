using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Sensor;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class PointCloud2Visualizer : DrawingVisualizer<PointCloud2Msg>
{
    public string RosMessageName => MessageRegistry.GetRosMessageName<PointCloud2Msg>();

    VisualizerSettingsGeneric<PointCloud2Msg> m_Settings;
    public VisualizerSettingsGeneric<PointCloud2Msg> Settings
    {
        get => m_Settings;
        set => m_Settings = value;
    }
    public override string Name => (string.IsNullOrEmpty(m_Topic) ? "" : $"({m_Topic}) ") + Settings.name;

    [SerializeField]
    protected TFTrackingSettings m_TFTrackingSettings = new TFTrackingSettings { tfTopic = "/tf" };
    public TFTrackingSettings TFTrackingSettings
    {
        get => m_TFTrackingSettings;
        set => m_TFTrackingSettings = value;
    }

    public Action CreateGUI(PointCloud2Msg message, MessageMetadata meta)
    {
        return null;
    }

    public enum ColorMode
    {
        HSV,
        SeparateRGB,
        CombinedRGB,
    }

    [HideInInspector, SerializeField]
    ColorMode m_ColorModeSetting;
    public ColorMode ColorModeSetting
    {
        get => m_ColorModeSetting;
        set => m_ColorModeSetting = value;
    }
    public string[] Channels
    {
        get => m_Channels;
        set => m_Channels = value;
    }
    string[] m_Channels;

    public string XChannel
    {
        get => m_XChannel;
        set => m_XChannel = value;
    }
    string m_XChannel = "x";
    public string YChannel
    {
        get => m_YChannel;
        set => m_YChannel = value;
    }
    string m_YChannel = "y";
    public string ZChannel
    {
        get => m_ZChannel;
        set => m_ZChannel = value;
    }
    string m_ZChannel = "z";

    public float pointsize = 0.01f;
    float lastMinZ = 1000;
    float lastMaxZ = 0;
    float currentMinZ = 1000;
    float currentMaxZ = 0;

    public override void Draw(Drawing3d drawing, PointCloud2Msg message, MessageMetadata meta)
    {
        drawing.SetTFTrackingSettings(m_TFTrackingSettings, message.header);
        var pointCloud = drawing.AddPointCloud((int)(message.data.Length / message.point_step));

        Channels = message.fields.Select(field => field.name).ToArray();

        Dictionary<string, int> channelToIdx = new Dictionary<string, int>();
        for (int i = 0; i < message.fields.Length; i++)
        {
            channelToIdx.Add(message.fields[i].name, i);
        }

        TFFrame frame = TFSystem.instance.GetTransform(message.header);

        int xChannelOffset = (int)message.fields[channelToIdx[m_XChannel]].offset;
        int yChannelOffset = (int)message.fields[channelToIdx[m_YChannel]].offset;
        int zChannelOffset = (int)message.fields[channelToIdx[m_ZChannel]].offset;

        int maxI = message.data.Length / (int)message.point_step;
        int count = 0;
        currentMinZ = 1000;
        currentMaxZ = 0;

        for (int i = 0; i < maxI; i++)
        {
            count++;
            int iPointStep = i * (int)message.point_step;
            var x = BitConverter.ToSingle(message.data, iPointStep + xChannelOffset);
            var y = BitConverter.ToSingle(message.data, iPointStep + yChannelOffset);
            var z = BitConverter.ToSingle(message.data, iPointStep + zChannelOffset);
            Vector3<FLU> rosPoint = new Vector3<FLU>(x, y, z);
            Vector3 unityPoint = rosPoint.toUnity;

            Color depthColor = Color.HSVToRGB(Mathf.InverseLerp(lastMinZ, lastMaxZ, z), 1, 1);

            drawing.DrawCuboid(unityPoint, Vector3.one * pointsize, depthColor);
            //pointCloud.AddPoint(unityPoint, depthColor, pointsize);
            if (z < currentMinZ)
            {
                currentMinZ = z;
            }
            else if (z > currentMaxZ)
            {
                currentMaxZ = z;
            }
        }
        //pointCloud.Bake();
        lastMinZ = currentMinZ;
        lastMaxZ = currentMaxZ;
    }

    public void Redraw()
    {
        // settings have changed - update the visualization
        //TODO
    }
}
