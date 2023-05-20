# ARRViz - Augmented Reality Robot Visualization

This Unity project allows you to view ros topics overlayed  on a real robot in augmented reality.

This project started out as a hackathon project for Cobalt Robotics. Check out what they are doing at
[https://www.cobaltrobotics.com](https://www.cobaltrobotics.com)

[pic](picture)

## Compatibility

Tier 1:
- iphone/iPad Pro Series
- Androids with depth sensors
- Apple AR Device (unreleased headset)

Tier 2: Won't localize as well, need to scan the room more before starting.
- iPhones/iPads with ARKit support
- Androids with ARCore support

Tier 3:
- Hololens (Untested, ROS-TCP-Connector may not work)

## Setting Up

### Quick Start Guide

### Getting the app

The app can be downloaded for the iPhone/iPad via the app store [app](app store link). For android, 
you can download the apk from here.

### Building from Unity

To build the app from source, you will need to:
``` 
git clone ...
cd ...
git submodule init (I think)
```
Download Unity version 2021.3.x. [Download Page](https://unity.com/download)
Download either the ios or android build package.
Open the project with Unity 
Build the project via file/"Build Settings" Build
More info can be found on building Unity projects [here](https://docs.unity3d.com/Manual/PublishingBuilds.html)

### ROS Setup

To setup ros to work with robot_ar_viewer, you will need to install the [ROS-TCP-Endpoint Package](https://github.com/Unity-Technologies/ROS-TCP-Endpoint).
For ROS1 use the main branch, for ROS2 use main-ros2.

Once the package is built, launch:
```
ROS1
roslaunch ros_tcp_endpoint endpoint.launch

ROS2
ros2 launch ros_tcp_endpoint endpoint.py
```

The default port is 10000.

Get the ip address of the computer running the ros_tcp_endpoint and the port selected above. you'll need both to connect your robot from the phone.

#### Downsampling Topics

The ROS-TCP-Connector can only handle a limited amount of bandwidth so it is likely you will need to downsample
the point cloud topics. To easily downsample point clouds check out the ROS1 [pcl_ros Voxel Filtering](http://wiki.ros.org/pcl_ros/Tutorials/VoxelGrid%20filtering)

If the point clouds start to lag, it is likely you will need to downsample the topic.

### App Setup

To connect to the robot, input the ip address and port and then click connect. If it doesn't connect, checkout the Debugging section below. 

#### Setttings

QR Code Link: This is the link in the tf tree that should correlate to the where the QR code is placed on the robot 
or in the world. At the moment only this QR code will work:

![image](Assets/ImageTracking/robot_qr_code.png)

Add picture showing how qr code should line up with a tf frame.

QR Code Size: The size of the physical QR code. You can select sizes from 5cm to 25cm in increments of 1cm.

Base Link: This should be the base link of the robot. Often times this is just called base link.
It is expected that the base link is not offset from the ground with the z frame pointing up.

Pose Topic: The topic to use with the pose tool. The pose tool will send a PoseStamped msg over this topic. 
It can be used to send a navigation goal, initial pose, or anything other PoseStamped msg.

Topics: The topics section allows you to choose which topics are shown in the visualization. 

#### Usage

Scan the room with the phone to make sure the phone is well localized. The better it is scanned, the better 
the phone will stay localized with the robot.

Once ros is connected, the settings inputted, and the topics selected, you can now localize the phone to 
the robot. There are two ways to do this, with the barcode scanner or by manually selecting the base_link frame.

QR Code: Select the QR code mode on the bottom right, make sure the camera is in good view of the code,
and then press and hold the action button until the green overlay appears.

Manual Localize: Select the localize button, move the target to the base link on the robot and then press and hold
the action button. While holding, move the phone to set the direction of the base link.

Pose Selector: Select the Pose button, move the target to the goal location and then press and hold the
action button to select the direction.

## Debugging 

- If it doesn't connect or you don't see any topics appear, make sure there aren't any firewalls up.
Get the ip address of your device and check if you can ping it from the robot. 
- If the visualizations are lagging, select fewer topics or downsample the point clouds. 

## Contributing 

Feel free to open a pr if you have a bug fix or enhancement. See the issue tracker for a list of known bugs
and possible enhancements.

## Similar Projects 

IViz: https://github.com/KIT-ISAS/iviz
Very cool project porting over much of RViz's functionality to mobile

ARViz: https://github.com/hri-group/arviz
ROS and mixed reality for Hololens
