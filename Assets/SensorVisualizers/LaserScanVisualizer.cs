using System;
using System.Collections.Generic;
using System.Linq;
using RosMessageTypes.Sensor;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using UnityEngine;

public class LaserScanVisualizer : DrawingVisualizer<LaserScanMsg>
{
    public string RosMessageName => MessageRegistry.GetRosMessageName<LaserScanMsg>();

    VisualizerSettingsGeneric<LaserScanMsg> m_Settings;
    public VisualizerSettingsGeneric<LaserScanMsg> Settings
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

    public Action CreateGUI(LaserScanMsg message, MessageMetadata meta)
    {
        return null;
    }

    [SerializeField]
    bool m_UseIntensitySize;
    public bool UseIntensitySize
    {
        get => m_UseIntensitySize;
        set => m_UseIntensitySize = value;
    }

    [HideInInspector, SerializeField]
    float m_PointRadius = 0.05f;
    public float PointRadius
    {
        get => m_PointRadius;
        set => m_PointRadius = value;
    }

    [HideInInspector, SerializeField]
    float m_MaxIntensity = 100.0f;
    public float MaxIntensity
    {
        get => m_MaxIntensity;
        set => m_MaxIntensity = value;
    }

    public enum ColorModeType
    {
        Distance,
        Intensity,
        Angle,
    }

    [SerializeField]
    ColorModeType m_ColorMode;
    public ColorModeType ColorMode
    {
        get => m_ColorMode;
        set => m_ColorMode = value;
    }

    public override void Draw(Drawing3d drawing, LaserScanMsg message, MessageMetadata meta)
    {
        drawing.SetTFTrackingSettings(m_TFTrackingSettings, message.header);

        PointCloudDrawing pointCloud = drawing.AddPointCloud(message.ranges.Length);
        // negate the angle because ROS coordinates are right-handed, unity coordinates are left-handed
        float angle = -message.angle_min;
        ColorModeType mode = m_ColorMode;
        if (mode == ColorModeType.Intensity && message.intensities.Length != message.ranges.Length)
            mode = ColorModeType.Distance;
        for (int i = 0; i < message.ranges.Length; i++)
        {
            Vector3 point = Quaternion.Euler(0, Mathf.Rad2Deg * angle, 0) * Vector3.forward * message.ranges[i];

            Color32 c = Color.white;
            switch (mode)
            {
                case ColorModeType.Distance:
                    c = Color.HSVToRGB(
                        Mathf.InverseLerp(message.range_min, message.range_max, message.ranges[i]),
                        1,
                        1
                    );
                    break;
                case ColorModeType.Intensity:
                    c = new Color(1, message.intensities[i] / m_MaxIntensity, 0, 1);
                    break;
                case ColorModeType.Angle:
                    c = Color.HSVToRGB((1 + angle / (Mathf.PI * 2)) % 1, 1, 1);
                    break;
            }

            float radius = m_PointRadius;
            if (m_UseIntensitySize && message.intensities.Length > 0)
            {
                radius = Mathf.InverseLerp(0, m_MaxIntensity, message.intensities[i]);
            }
            drawing.DrawCuboid(point, Vector3.one * radius, c);
            //pointCloud.AddPoint(point, c, radius);

            angle -= message.angle_increment;
        }
        //pointCloud.Bake();
    }

    public void Redraw()
    {
        // settings have changed - update the visualization
        //TODO
    }
}
