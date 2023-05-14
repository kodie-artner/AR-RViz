using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.Visualizations;
using UnityEngine;

// Represents a single line in the VisualizationTopicsTab
// and saves and loads the options for that line, plus the associated hud windows etc.
public class TopicVisualizerData
{
    RosTopicState m_TopicState;
    public RosTopicState TopicState => m_TopicState;
    public string Topic => m_TopicState.Topic;
    public MessageSubtopic Subtopic => m_TopicState.Subtopic;
    public string RosMessageName => m_TopicState.RosMessageName;
    public string Title => Topic + (Subtopic == MessageSubtopic.Response ? " (response)" : "");

    // a service topic is represented by two lines, one for the request and one for the response. m_ServiceResponseTopic is the response.
    TopicVisualizerData m_ServiceResponseTopic;

    bool m_IsVisualizingDrawing;
    public bool IsVisualizingDrawing => m_IsVisualizingDrawing;

    string m_CachedRosMessageName;

    IVisualFactory m_VisualFactory;
    bool m_NoVisualFactoryAvailable;

    IVisual m_Visual;
    public IVisual Visual => m_Visual;

    public IVisualFactory GetVisualFactory()
    {
        if (m_CachedRosMessageName != RosMessageName)
        {
            // if the topic has changed, discard our cached data
            m_VisualFactory = null;
            m_NoVisualFactoryAvailable = false;
        }

        if (m_VisualFactory == null && !m_NoVisualFactoryAvailable)
        {
            SetVisualFactory(VisualFactoryRegistry.GetVisualFactory(Topic, RosMessageName, Subtopic));
        }
        return m_VisualFactory;
    }

    public void SetVisualFactory(IVisualFactory visualFactory)
    {
        if (m_Visual != null)
            m_Visual.SetDrawingEnabled(false);
        m_Visual = null;

        m_VisualFactory = visualFactory;
        m_CachedRosMessageName = RosMessageName;
        if (m_VisualFactory == null)
            m_NoVisualFactoryAvailable = true;

        EnableVisualization(m_IsVisualizingDrawing);
    }

    public bool CanShowWindow => GetVisualFactory() != null;
    public bool CanShowDrawing => GetVisualFactory() != null && m_VisualFactory.CanShowDrawing;



    public void EnableVisualization(bool enable)
    {
        m_IsVisualizingDrawing = enable;

        if (enable && m_Visual == null)
        {
            m_Visual = GetVisualFactory().GetOrCreateVisual(Topic);
        }

        if (m_Visual != null)
        {
            m_Visual.SetDrawingEnabled(enable);
        }
    }

    public TopicVisualizerData(RosTopicState baseState)
    {
        m_TopicState = baseState;

        // if (baseState.ServiceResponseTopic != null || true)
        // {
        //     m_ServiceResponseTopic = new TopicVisualizerData(baseState.ServiceResponseTopic);
        // }

        m_CachedRosMessageName = RosMessageName;
    }
}
