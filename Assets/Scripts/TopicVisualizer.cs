using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Robotics.ROSTCPConnector;

public class TopicVisualizer : MonoBehaviour
{
    public RosTopicState topicState;
    public TMP_Text topic;
    public Toggle toggle;
    TopicVisualizerData topicVisualizerData;

    public void Initialize(RosTopicState state)
    {
        topic = GetComponentInChildren<TMP_Text>();
        toggle = GetComponentInChildren<Toggle>();
        toggle.onValueChanged.AddListener(OnToggleChanged);

        if (topic == null)
        {
            throw new NullReferenceException("topic object is null.");
        }
        if (toggle == null)
        {
            throw new NullReferenceException("toggle object is null.");
        }
        topicState = state;       
        topicVisualizerData = new TopicVisualizerData(state);
        topic.text = state.Topic;
        toggle.interactable = topicVisualizerData.CanShowDrawing;
    }

    void OnToggleChanged(bool enabled)
    {
        if (enabled)
        {
            topicVisualizerData.EnableVisualization(true);
        }
        else 
        {
            topicVisualizerData.EnableVisualization(false);
        }
    }
}
