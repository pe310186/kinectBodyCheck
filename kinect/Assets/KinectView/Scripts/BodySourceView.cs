using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodySourceView : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private GameObject cube = null;
    private double right_hand_x = 0;
    private double left_hand_x = 0;
    private double head_y = 0;

    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };


    private Vector3 bodyScales;
    private Vector3 bodyOffsets;

    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }

        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);
 
            }
        }
        //Tcheck();

    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X*10, joint.Position.Y*10, joint.Position.Z*10);
    }

    public bool Tcheck()
    {
        if (BodySourceManager != null)
        {

            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

            if (_BodyManager == null)
                return false;

            Kinect.Body[] data = _BodyManager.GetData();
            if (data == null)
                return false;
            foreach (var body in data)
            {
                if (body == null)
                {
                    continue;
                }
                if (body.IsTracked)
                {
                    Vector3 newBodyScales = new Vector3(1, 1, 1);
                    Vector3 newBodyOffsets = new Vector3(0, 0, 0);

                    double average = 0;
                    double rightHandX = 0;
                    double leftHandX = 0;
                    double headY = 0;
                    double hipY = 0;


                    List<Vector3> jointPositions = new List<Vector3>();

                    for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                    {
                        Kinect.Joint sourceJoint = body.Joints[jt];
                        Kinect.Joint? targetJoint = null;

                        if (_BoneMap.ContainsKey(jt))
                        {
                            targetJoint = body.Joints[_BoneMap[jt]];
                        }

                        string jointType = jt.ToString();

                        if (jointType == "HandRight")
                        {
                            //print(sourceJoint.Position.Y);
                        }


                        if (jointType == "SpineBase")//身體深度判定(大約1.5m    Z: 2 +-0.2)
                        {
                            if (sourceJoint.Position.Z > 2.2)
                            {
                                //print("離太遠");
                                return false;
                            }
                            if (sourceJoint.Position.Z < 1.8)
                            {
                                //print("離太近");
                                return false;
                            }
                            if (sourceJoint.Position.X < -0.2)
                            {
                                //print("太左邊");
                            }
                            if (sourceJoint.Position.X > 0.2)
                            {
                                //print("太右邊");
                            }

                            //print("剛剛好");
                        }

                        //Y 平均 +-0.05
                        if (jointType == "HandRight" || jointType == "WristRight" || jointType == "ElbowRight" || jointType == "HandLeft" || jointType == "WristLeft" || jointType == "ElbowLeft")
                        {
                            //深度判斷
                            if (sourceJoint.Position.Z > 2.2)
                            {
                                //print("手不平");
                                return false;
                            }
                            else if (sourceJoint.Position.Z < 1.8)
                            {
                                //print("手不平");
                                return false;
                            }
                            else
                            {
                                //print("手不平");
                            }
                            average += sourceJoint.Position.Y;
                            jointPositions.Add(new Vector3(sourceJoint.Position.X, sourceJoint.Position.Y, sourceJoint.Position.Z));


                        }




                        if (jointType == "HandRight")
                        {
                            rightHandX = sourceJoint.Position.X;
                            right_hand_x = rightHandX;
                        }

                        if (jointType == "HandLeft")
                        {
                            leftHandX = sourceJoint.Position.X;
                            left_hand_x = leftHandX;
                        }



                        if (jointType == "Neck")
                        {
                            headY = sourceJoint.Position.Y;
                            head_y = headY;
                        }

                        if (jointType == "SpineBase")
                        {
                            hipY = sourceJoint.Position.Y;
                        }


                    }

                    


                    average /= 6;
                    for (int i = 0; i < jointPositions.Count; i++)
                    {
                        if (jointPositions[i].y < average - 0.05 || jointPositions[i].y > average + 0.05)
                        {
                            //print("手不平");
                            return false;
                        }
                    }

                    //x scale and offset

                    double xLength = System.Math.Abs(rightHandX - leftHandX);
                    newBodyScales.x = (float)(2.0f / xLength);
                    newBodyOffsets.x = -(float)((rightHandX + leftHandX) / 2.0f);


                    //y scale and offset


                    double yLength = System.Math.Abs(headY - hipY);
                    newBodyScales.y = (float)(2.0f / yLength);
                    newBodyOffsets.y = -(float)((headY + hipY) / 2.0f);


                    // z scale and offset
                    newBodyScales.z = 2.0f / (2.2f - 1.8f);
                    newBodyOffsets.z = -2.0f;


                    print("T字形");

                    bodyScales = newBodyScales;
                    bodyOffsets = newBodyOffsets;



                    return true;
                }
            }
        }
        return false;
    }

    public Vector3 GetJointPosition(Kinect.JointType jointType)
    {
        if (bodyScales != null && bodyOffsets != null && BodySourceManager != null)
        {
            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

            if (_BodyManager != null)
            {

                Kinect.Body[] data = _BodyManager.GetData();
                if (data != null)
                {
                    foreach (var body in data)
                    {
                        if (body == null)
                        {
                            continue;
                        }
                        if (body.IsTracked)
                        {
                            var joints = body.Joints;
                            var joint = joints[jointType];

                            Vector3 res = new Vector3((bodyOffsets.x + joint.Position.X) * bodyScales.x, (bodyOffsets.y + joint.Position.Y) * bodyScales.y, (bodyOffsets.z + joint.Position.Z) * bodyScales.z);

                            return res;

                        }

                    }
                }
            }
        }
        return Vector3.zero;
    }

    public bool OutOfRange()
    {
        if (BodySourceManager != null)
        {
            _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

            if (_BodyManager != null)
            {

                Kinect.Body[] data = _BodyManager.GetData();
                if (data != null)
                {
                    foreach (var body in data)
                    {
                        if (body == null)
                        {
                            continue;
                        }
                        if (body.IsTracked)
                        {
                            Kinect.JointType jointType = Kinect.JointType.SpineBase;
                            var joints = body.Joints;
                            var joint = joints[jointType];

                            //print(joint.Position.X);

                            //z check
                            if (joint.Position.Z < 1.8f || joint.Position.Z > 2.2f)
                                return false;

                            //x check
                            if (joint.Position.X > right_hand_x || joint.Position.X < left_hand_x)
                                return false;

                            jointType = Kinect.JointType.SpineBase;


                            joint = joints[jointType];

                            //y check
                            if (joint.Position.Y > head_y)
                                return false;

                            return true;
                        }

                    }
                }
            }
        }

        return false;
    }
}
