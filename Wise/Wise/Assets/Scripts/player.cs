using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed=1.0f;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      //If the left key is pressed, the object moves left

      if (Input.GetKey(KeyCode.LeftArrow))
      {
        //Debug.Log(this.transform.position);
         if(this.transform.position.x >= -24.4)
         {
           this.transform.position = this.transform.position - new Vector3(100f,0f,0f) * Time.deltaTime;
         }
      }
      //If the right key is pressed, the object moves to the right
      if(Input.GetKey(KeyCode.RightArrow))
      {
        //Debug.Log(this.transform.position);
         if(this.transform.position.x <= 23.1)
         {
           this.transform.position = this.transform.position + new Vector3 (100f,0f,0f) *  Time.deltaTime;
         }
      }
      //If the up key is pressed, the object moves to the right
      if (Input.GetKey(KeyCode.UpArrow))
      {
        //Debug.Log(this.transform.position);
        if(this.transform.position.z <=31)
        {
          this.transform.position = this.transform.position + new Vector3(0f,0f,100f) * Time.deltaTime;
        }
      }
      //If the down key is pressed, the object moves to the right
      if (Input.GetKey(KeyCode.DownArrow))
      {
        //Debug.Log(this.transform.position);
        if(this.transform.position.z >= -15)
        {
          this.transform.position = this.transform.position - new Vector3(0f,0f,100f) * Time.deltaTime;
        }
      }
    }
}
