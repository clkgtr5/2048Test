using UnityEngine;
using System.Collections;
using FairyGUI;
public class init : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GRoot.inst.SetContentScaleFactor(410,700);
        UIPackage.AddPackage("UI/2048_pkg");
        GComponent mainpanel = UIPackage.CreateObject("2048_pkg", "mainpanel").asCom;
        GRoot.inst.AddChild(mainpanel);

       

        //for (int i = 0; i < gameLines * gameLines; i++)
        //{
        //    _blocks[i] = _mainView.GetChild("l" + (i + 1).ToString());  //获取contianers l1 ~ l16
        //                                                                // Debug.Log("Block_Name: "+_blocks[i].name);
        //}


        GObject holder = mainpanel.GetChild("Control");
        // Debug.Log("holder: "+holder.id); 
        SwipeGesture gesture1 = new SwipeGesture(holder);
        //Debug.Log("swipeleft");
        gesture1.onMove.Add(OnSwipeMove);
        //gesture1.onEnd.Add(OnSwipeEnd);

        mainpanel.onClick.Add(() => { print("onclick"); });
    }
	
	// Update is called once per frame
	void Update () {
	
	}
    void OnSwipeMove(EventContext context)
    {
        Debug.Log("swipemove");
        SwipeGesture gesture = (SwipeGesture)context.sender;

        Vector3 v = new Vector3();

        if (Mathf.Abs(gesture.delta.x) > Mathf.Abs(gesture.delta.y)) //delta 是触摸的距离
        {
            v.x = Mathf.Round(gesture.delta.x);

            if (Mathf.Abs(v.y) < 2) //消除手抖的影响
                return;
            if (v.x < 0)
            {//swipeleft
             // Debug.Log("swipeleft");
               // swipeLeft();
              //  rearranage();
            }
            else if (v.x > 0)
            { //swiperight

            }
        }

        else
        {
            v.y = Mathf.Round(gesture.delta.y);
            if (Mathf.Abs(v.x) < 2)
                return;

            if (v.y > 0)
            { // swipeup


            }
            else if (v.y < 0)
            { // siwpedown


            }
        }
        //_mainPanel.Rotate(v, Space.World);
    }

}
