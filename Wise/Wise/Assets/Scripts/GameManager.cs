using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] koma;
    public GameObject [] WhitePoll1;
    public GameObject [] BlackPoll1;
    public GameObject player;
    public GameObject checkButtons;
    public int Count1=0;
    public int turns=2;
    void Start()
    {
      checkButtons.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.Return))
      {
        //Debug.Log("enter");
        checkButtons.SetActive(true);
      }


    }


    public void NoButton()
    {
      //If the "No button is pressed, dispear the double check screen"
      checkButtons.SetActive(false);
    }
    public void YesButton()
    {
      checkButtons.SetActive(false);

      if(player.transform.position.x>=22  && player.transform.position.x<=26 && player.transform.position.z>=-19 && player.transform.position.z<=-15)
      {
        if(turns%2==0)
        {
          Debug.Log(Count1);
          WhitePoll1[Count1].SetActive(true);
          Count1++;
          turns++;
        }
        else
        {
          BlackPoll1[Count1].SetActive(true);
          Count1++;
          turns++;
        }
      }
    }
}
