using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Kinect = Windows.Kinect;

public class testBody : MonoBehaviour {

    public GameObject canvas;
    BodySourceView bodySource;
    GameObject text;
    float time;

    bool T_check_flag = false;
    // Use this for initialization
    void Start () {
        bodySource = GetComponent<BodySourceView>();
        T_check_flag = false;
        canvas.SetActive(false);
        text = canvas.transform.GetChild(1).gameObject;
    }
	
	// Update is called once per frame
	void Update () {
        if (T_check_flag)
        {
            if (!bodySource.OutOfRange())//出界
            {
                canvas.SetActive(true);
                T_check_flag = false;
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
            canvas.SetActive(true);
            text.GetComponent<UnityEngine.UI.Text>().text = "請擺出T字形";
            T_check_flag = bodySource.Tcheck();
            //判斷為T
            if (T_check_flag)
            {
                Vector3 basePosition = bodySource.GetJointPosition(Kinect.JointType.SpineBase);
                float time = 3.0f;
                int dot = 1;
                while (time > 0)
                {
                    time -= Time.deltaTime;
                    dot++;
                    if (dot > 3)
                       dot = 1;
                    text.GetComponent<UnityEngine.UI.Text>().text = "讀取中";
                    for(int i = 0; i < dot; i++)
                    {
                        text.GetComponent<UnityEngine.UI.Text>().text += ".";
                    }

                    if (!bodySource.Tcheck())
                    {
                        T_check_flag = false;
                        break;
                    }

                    Vector3 tmpPosition = bodySource.GetJointPosition(Kinect.JointType.SpineBase);

                    if (System.Math.Abs(basePosition.x - tmpPosition.x) > 0.5 || System.Math.Abs(basePosition.y - tmpPosition.y) > 0.5 || System.Math.Abs(basePosition.z - tmpPosition.z) > 0.5)
                    {
                        T_check_flag = false;
                        break;
                    }
                }
            }
        }     
    }
}
