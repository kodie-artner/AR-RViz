using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using Unity.Robotics.Visualizations;
using UnityEngine;

// This file is a derivative of VisualizationTopicTabEntry.cs in ros_tcp_connector
public class TopicVisualizerData
{
    private RosTopicState topicState;
    public RosTopicState TopicState => topicState;
    public string Topic => topicState.Topic;
    public MessageSubtopic Subtopic => topicState.Subtopic;
    public string RosMessageName => topicState.RosMessageName;
    public string Title => Topic + (Subtopic == MessageSubtopic.Response ? " (response)" : "");

    public bool CanShowWindow => GetVisualFactory() != null;
    public bool CanShowDrawing => GetVisualFactory() != null && visualFactory.CanShowDrawing;

    private bool isVisualizingDrawing;
    public bool IsVisualizingDrawing => isVisualizingDrawing;
    private string cachedRosMessageName;
    private IVisualFactory visualFactory;
    private bool noVisualFactoryAvailable;
    private IVisual visual;
    public IVisual Visual => visual;

    public TopicVisualizerData(RosTopicState baseState)
    {
        topicState = baseState;
        cachedRosMessageName = RosMessageName;
    }

    public IVisualFactory GetVisualFactory()
    {
        if (cachedRosMessageName != RosMessageName)
        {
            // if the topic has changed, discard our cached data
            visualFactory = null;
            noVisualFactoryAvailable = false;
        }

        if (visualFactory == null && !noVisualFactoryAvailable)
        {
            SetVisualFactory(VisualFactoryRegistry.GetVisualFactory(Topic, RosMessageName, Subtopic));
        }
        return visualFactory;
    }

    public void SetVisualFactory(IVisualFactory visualFactory)
    {
        if (visual != null)
        {
            visual.SetDrawingEnabled(false);
        }

        visual = null;
        this.visualFactory = visualFactory;
        cachedRosMessageName = RosMessageName;

        if (this.visualFactory == null)
        {
            noVisualFactoryAvailable = true;
        }

        EnableVisualization(isVisualizingDrawing);
    }

    public void EnableVisualization(bool enable)
    {
        isVisualizingDrawing = enable;

        if (enable && visual == null)
        {
            visual = GetVisualFactory().GetOrCreateVisual(Topic);
        }

        if (visual != null)
        {
            visual.SetDrawingEnabled(enable);
        }
    }
}
