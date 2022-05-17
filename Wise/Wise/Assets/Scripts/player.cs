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
      //If the left key is pressed, the object moves by

      if (Input.GetKey(KeyCode.LeftArrow))
      {
         if(this.transform.position.x >= -23.1)
         {
           this.transform.position+= Vector3.left * speed * Time.deltaTime;
         }

      }

      if(Input.GetKey(KeyCode.RightArrow))
      {
        Debug.Log(this.transform.position);
         if(this.transform.position.x <= 23.5)
         {

           this.transform.position = this.transform.position + new Vector3 (240f,0f,0f) *  Time.deltaTime;

         }
      }
    }
}
