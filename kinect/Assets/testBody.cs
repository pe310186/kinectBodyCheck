using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kinect = Windows.Kinect;

public class testBody : MonoBehaviour {

    BodySourceView bodySource;
    // Use this for initialization
    void Start () {
        bodySource = GetComponent<BodySourceView>();
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("LeftHand " + bodySource.GetJointPosition(Kinect.JointType.HandLeft));
        Debug.Log("Head " + bodySource.GetJointPosition(Kinect.JointType.Head));
    }
}
