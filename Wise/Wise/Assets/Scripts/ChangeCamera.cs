using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeCamera : MonoBehaviour
{

    // Start is called before the first frame update

    public GameObject cam1;
    public GameObject cam2;
    public GameObject cam3;
    public GameObject cam4;
    // Update is called once per frame
    void Update()
    {
      if(Input.GetButtonDown("key1")==true )
      {//If key 1 is pressed, activate camera 2 and turn off others
        cam1.SetActive(false);
        cam2.SetActive(true);
        cam3.SetActive(false);
        cam4.SetActive(false);
        //Debug.Log("Space key was pressed down");

      }

      if(Input.GetButtonDown("key2")==true)
      {//If key 2 is pressed, activate camera 3 and turn off others
        cam1.SetActive(false);
        cam2.SetActive(false);
        cam3.SetActive(true);
        cam4.SetActive(false);
        //Debug.Log("Space key was pressed down1");

      }

      if(Input.GetButtonDown("key3")==true)
      {//If key 3 is pressed, activate camera 4 and turn off others
        cam1.SetActive(false);
        cam2.SetActive(false);
        cam3.SetActive(false);
        cam4.SetActive(true);
        //Debug.Log("Space key was pressed down2");

      }

      if(Input.GetButtonDown("key4")==true)
      {//If key 4 is pressed, activate camera 1 and turn off others
        cam1.SetActive(true);
        cam2.SetActive(false);
        cam3.SetActive(false);
        cam4.SetActive(false);
        //Debug.Log("Space key was pressed down3");

      }

    }


}
