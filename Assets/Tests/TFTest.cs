using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using RosMessageTypes.Tf2;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.SceneManagement;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

namespace Tests
{
    public class TFTest
    {
        ROSConnection ros;
        Transform mapTransform;
        Localizer localizer;
        float tolerance = 0.01f;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("OneTimeSetUp Base");
            SceneManager.LoadScene("Scenes/MainScene");

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<TFMessageMsg>("/tf");
            ros.RegisterPublisher<TFMessageMsg>("/tf_static");
        }

        [SetUp]
        public void SetUp()
        {
            Debug.Log("SetUp Base");
        }

        [TearDown]
        public void TearDown()
        {
            ros.Disconnect();
        }

        [UnityTest]
        public IEnumerator TestSuiteWithEnumeratorPasses()
        {
            mapTransform = GameObject.FindGameObjectWithTag("Map").transform;
            localizer = new Localizer();
            // Setup tf
            TFMessageMsg msg = new TFMessageMsg();
            msg.transforms = new TransformStampedMsg[2];
            msg.transforms[1] = GetTransformMsg(
                "map",
                "link_1",
                new Vector3<FLU>(new Vector3(5, 0, 0)),
                new Quaternion<FLU>(new Quaternion(0, 0, 0, 1))
            );
            msg.transforms[0] = GetTransformMsg(
                "link_1",
                "link_2",
                new Vector3<FLU>(new Vector3(9, 0, 0)),
                new Quaternion<FLU>(new Quaternion(0, 0, 0, 1))
            );
            TFSystem.instance.TFTopics["/tf"].ReceiveTF(msg);

            // Set map position
            localizer.SetMapTransform(new Vector3(0, 0, 0), 0, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(-14, 0, 0));

            localizer.SetMapTransform(new Vector3(0, 5, 0), 0, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(-14, 5, 0));

            localizer.SetMapTransform(new Vector3(5, 0, 0), 0, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(-9, 0, 0));

            localizer.SetMapTransform(new Vector3(0, 0, 0), 180, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(14, 0, 0));

            localizer.SetMapTransform(new Vector3(0, 0, 0), 90, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(0, 0, 14));

            localizer.SetMapTransform(new Vector3(5, 0, 0), 90, "link_2");
            Debug.Log(mapTransform.position);
            Vector3Equal(mapTransform.position, new Vector3(5, 0, 14));
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestBaseFrameIsWhereIsExpected()
        {
            mapTransform = GameObject.FindGameObjectWithTag("Map").transform;
            localizer = new Localizer();
            // Setup tf
            TFMessageMsg msg = new TFMessageMsg();
            msg.transforms = new TransformStampedMsg[4];
            msg.transforms[0] = GetTransformMsg(
                "map",
                "odom",
                new Vector3<FLU>(new Vector3(5, 0, 0)),
                new Quaternion<FLU>(new Quaternion(0, -0.4281827f, 0, -0.9036922f))
            );
            msg.transforms[1] = GetTransformMsg(
                "odom",
                "base_frame",
                new Vector3<FLU>(new Vector3(3.2f, 0, 8)),
                new Quaternion<FLU>(new Quaternion(0, -0.3048106f, 0, -0.952413f))
            );
            msg.transforms[2] = GetTransformMsg(
                "base_frame",
                "head_frame",
                new Vector3<FLU>(new Vector3(0, 1.5f, 0)),
                new Quaternion<FLU>(new Quaternion(0, 0, 0, 1))
            );
            msg.transforms[3] = GetTransformMsg(
                "head_frame",
                "qr_link",
                new Vector3<FLU>(new Vector3(0, 0.15f, 0)),
                new Quaternion<FLU>(new Quaternion(0.938f, 0, 0, 0.3466353f))
            );
            TFSystem.instance.TFTopics["/tf"].ReceiveTF(msg);

            GameObject tracker = new GameObject();
            tracker.transform.position = new Vector3(4, 1.65f, 9.5f);
            tracker.transform.rotation = new Quaternion(0.68490f, 0.23684f, -0.64090f, 0.25310f);

            localizer.SetMapTransformWithTracker(tracker, "qr_link");

            TFFrame T_head_frame = TFSystem.instance.GetTransform(frame_id: "qr_link", time: 0);
            GameObject head_frame = new GameObject();
            head_frame.transform.SetParent(mapTransform);
            head_frame.transform.localPosition = T_head_frame.translation;
            head_frame.transform.localRotation = T_head_frame.rotation;

            Vector3Equal(head_frame.transform.position, tracker.transform.position);
            QuaternionEqual(head_frame.transform.rotation, tracker.transform.rotation);
            yield return null;
        }

        void Vector3Equal(Vector3 first, Vector3 second)
        {
            Assert.That(first.x, Is.EqualTo(second.x).Within(tolerance));
            Assert.That(first.y, Is.EqualTo(second.y).Within(tolerance));
            Assert.That(first.z, Is.EqualTo(second.z).Within(tolerance));
        }

        void QuaternionEqual(Quaternion first, Quaternion second)
        {
            float angle = Quaternion.Angle(first, second);
            Assert.That(angle, Is.EqualTo(0).Within(tolerance));
        }

        TransformStampedMsg GetTransformMsg(
            string frame_id,
            string child_frame_id,
            Vector3<FLU> position,
            Quaternion<FLU> rotation
        )
        {
            TransformStampedMsg transformMsg = new TransformStampedMsg();
            transformMsg.header.frame_id = frame_id;
            transformMsg.child_frame_id = child_frame_id;
            transformMsg.transform.translation = new Vector3Msg(position.x, position.y, position.z);
            transformMsg.transform.rotation = new QuaternionMsg(rotation.x, rotation.y, rotation.z, rotation.w);
            return transformMsg;
        }
    }
}
