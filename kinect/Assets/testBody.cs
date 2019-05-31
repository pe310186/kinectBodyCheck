using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Kinect = Windows.Kinect;

public class testBody : MonoBehaviour {

    public GameObject canvas;
    BodySourceView bodySource;
    GameObject text;

    bool T_check_flag = false;
    // Use this for initialization
    void Start () {
        bodySource = GetComponent<BodySourceView>();
        T_check_flag =  bodySource.Tcheck();
        canvas.SetActive(false);
        text = canvas.transform.GetChild(1).gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log("LeftHand " + bodySource.GetJointPosition(Kinect.JointType.HandLeft));
        //Debug.Log("Head " + bodySource.GetJointPosition(Kinect.JointType.Head));
        print(T_check_flag);
        if (T_check_flag)
        {
            if (!bodySource.OutOfRange())//出界
            {
                canvas.SetActive(true);
                text.GetComponent<UnityEngine.UI.Text>().text = "請擺出T字形";
                print("out of range");
            }
            else
            {
                canvas.SetActive(false);
                text.GetComponent<UnityEngine.UI.Text>().text = "";
                print("剛好");
            }
        }
        else
        {
            text.GetComponent<UnityEngine.UI.Text>().text = "請擺出T字形";
            canvas.SetActive(true);
            T_check_flag = bodySource.Tcheck();
        }
        
        
    }
}
