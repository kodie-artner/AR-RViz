using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Visualization;
using Unity.Robotics.Visualizations;
using Unity.Robotics.ROSTCPConnector;

// A simple visualizer that places a (user configured) prefab to show the position and
// orientation of a Pose message
namespace RosMessageTypes.Visualization
{
    public class InteractiveMarkerVisualizer : BaseVisualFactory<InteractiveMarkerUpdateMsg>
    {   
        // TODO get from resources
        public InteractiveMarker interactiveMarkerPrefab;
        // The BaseVisualFactory's job is just to create visuals for topics as appropriate.
        protected override IVisual CreateVisual(string topic)
        {
            return new InteractiveMarkerVisual(topic, interactiveMarkerPrefab);
        }

        // The job of the visual itself is to subscribe to a topic, and draw
        // representations of the messages it receives.
        class InteractiveMarkerVisual : IVisual
        {
            InteractiveMarker interactiveMarkerPrefab;
            InteractiveMarkerUpdateMsg m_LastMessage;
            bool m_IsDrawingEnabled;
            public bool IsDrawingEnabled => m_IsDrawingEnabled;
            Dictionary<string, InteractiveMarker> markers_;
            string topicNS;
            string feedbackTopic;


            public InteractiveMarkerVisual(string topic, InteractiveMarker interactiveMarkerPrefab)
            {
                this.interactiveMarkerPrefab = interactiveMarkerPrefab;
                markers_ = new Dictionary<string, InteractiveMarker>();
                if (topic.EndsWith("/update"))
                {
                    topicNS = topic.Substring(0, topic.Length - "/update".Length);
                    feedbackTopic = topicNS + "/feedback";
                }
                else
                {
                    throw new Exception("Invalid topic name for interactive marker");
                }

                ROSConnection.GetOrCreateInstance().Subscribe<InteractiveMarkerInitMsg>(topic + "_full", AddInitMessage);
                ROSConnection.GetOrCreateInstance().Subscribe<InteractiveMarkerUpdateMsg>(topic, AddMessage);
                ROSConnection.GetOrCreateInstance().RegisterPublisher<InteractiveMarkerFeedbackMsg>(feedbackTopic);
            }

            void AddMessage(InteractiveMarkerUpdateMsg message)
            {
                m_LastMessage = message;
                foreach (InteractiveMarkerMsg markerMsg in message.markers)
                {
                    if (markers_.ContainsKey(markerMsg.name))
                    {
                        markers_[markerMsg.name].UpdateLatestMsg(markerMsg);
                    }
                    else
                    {
                        Debug.Log("Marker not found, something is up");
                    }
                }
            }

            void AddInitMessage(InteractiveMarkerInitMsg msg)
            {
                Debug.Log("Recieved Interactive Marker init");
                RemoveAllMarkers();
                foreach (InteractiveMarkerMsg markerMsg in msg.markers)
                {
                    if (!markers_.ContainsKey(markerMsg.name))
                    {
                        InteractiveMarker interactiveMarker = Instantiate(interactiveMarkerPrefab);
                        interactiveMarker.SetFeedbackTopic(feedbackTopic);

                        interactiveMarker.gameObject.SetActive(m_IsDrawingEnabled);
                        markers_[markerMsg.name] = interactiveMarker;
                    }
                    markers_[markerMsg.name].Initialize(markerMsg);
                }
            }

            private void RemoveAllMarkers()
            {
                // foreach (KeyValuePair<string, InteractiveMarker> entry in markers_)
                // {
                //     Destroy(entry.Value.gameObject);

                //     // Remove the entry from the dictionary
                //     markers_.Remove(entry.Key);
                // }
            }

            public void SetDrawingEnabled(bool enabled)
            {
                m_IsDrawingEnabled = enabled;

                // detroy all game objects
                foreach (KeyValuePair<string, InteractiveMarker> entry in markers_)
                {
                    if (enabled)
                    {
                        entry.Value.gameObject.SetActive(true);
                    }
                    else
                    {
                        entry.Value.gameObject.SetActive(false);
                    }
                }
            }
    
            public void Redraw()
            {

            }

            public void OnGUI()
            {

            }
            }
        // Indicates that this visualizer should have a "3d" drawing checkbox in the topics list
        public override bool CanShowDrawing => true;
    }
}
