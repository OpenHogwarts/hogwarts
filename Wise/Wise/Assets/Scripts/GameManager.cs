using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] verticalWhite;
    public GameObject whiteMessage;
    public GameObject blackMessage;
    public GameObject [] WhitePoll1;
    public GameObject [] WhitePoll2;
    public GameObject [] WhitePoll3;
    public GameObject [] WhitePoll4;
    public GameObject [] WhitePoll5;
    public GameObject [] WhitePoll6;
    public GameObject [] WhitePoll7;
    public GameObject [] WhitePoll8;
    public GameObject [] WhitePoll9;
    public GameObject [] WhitePoll10;
    public GameObject [] WhitePoll11;
    public GameObject [] WhitePoll12;
    public GameObject [] WhitePoll13;
    public GameObject [] WhitePoll14;
    public GameObject [] WhitePoll15;
    public GameObject [] WhitePoll16;
    public GameObject [] BlackPoll1;
    public GameObject [] BlackPoll2;
    public GameObject [] BlackPoll3;
    public GameObject [] BlackPoll4;
    public GameObject [] BlackPoll5;
    public GameObject [] BlackPoll6;
    public GameObject [] BlackPoll7;
    public GameObject [] BlackPoll8;
    public GameObject [] BlackPoll9;
    public GameObject [] BlackPoll10;
    public GameObject [] BlackPoll11;
    public GameObject [] BlackPoll12;
    public GameObject [] BlackPoll13;
    public GameObject [] BlackPoll14;
    public GameObject [] BlackPoll15;
    public GameObject [] BlackPoll16;
    public GameObject player;
    public GameObject checkButtons;
    private int Count1=0;
    private int Count2=0;
    private int Count3=0;
    private int Count4=0;
    private int Count5=0;
    private int Count6=0;
    private int Count7=0;
    private int Count8=0;
    private int Count9=0;
    private int Count10=0;
    private int Count11=0;
    private int Count12=0;
    private int Count13=0;
    private int Count14=0;
    private int Count15=0;
    private int Count16=0;
    private int turns=2;

    void Start()
    {
      checkButtons.SetActive(false);
      blackMessage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
      if(turns%2==0)
      {
          whiteMessage.SetActive(true);
          blackMessage.SetActive(false);
      }
      else
      {
        blackMessage.SetActive(true);
        whiteMessage.SetActive(false);
      }
      //Shows who's turn is it

      if(Input.GetKeyDown(KeyCode.Return))
      {

        checkButtons.SetActive(true);
        Debug.Log(player.transform.position);
      }
      if(Count1==4)
      {
        for(int i=0;i<4;i++)
        {
          if(WhitePoll1[i].activeInHierarchy==false)
          {
            i=10;
            Debug.Log(i);
          }
          else
          {
            Debug.Log("White Wins");
          }
        }
      }

    }

    public void NoButton()
    {
      //If the "No button is pressed, dispear the double check screen"
      checkButtons.SetActive(false);
    }
    public void YesButton()
    {
      //If you click "Yes" while the double check process
      checkButtons.SetActive(false);
//Poll1
      if(player.transform.position.x>=22  && player.transform.position.x<=26 && player.transform.position.z>=-19 && player.transform.position.z<=-15)
      {
        if(turns%2==0)
        {
          //when the turn is even number, the white tip will be placed
          WhitePoll1[Count1].SetActive(true);
          Count1++;
          turns++;
        }
        else
        {
          //when the turn is odd number, the black tip will be placed
          BlackPoll1[Count1].SetActive(true);
          Count1++;
          turns++;
        }
      }
//Poll 2
      if(player.transform.position.x>=2.5  && player.transform.position.x<=10 && player.transform.position.z>=-16.7 && player.transform.position.z<=-13.3)
      {
        if(turns%2==0)
        {
          WhitePoll2[Count2].SetActive(true);
          Count2++;
          turns++;
          //Debug.Log(Count2);
        }
        else
        {
          BlackPoll2[Count2].SetActive(true);
          Count2++;
          turns++;
          //Debug.Log(Count2);
        }
      }
      //Poll3
      if(player.transform.position.x>=-15.7  && player.transform.position.x<=-7.2 && player.transform.position.z>=-16.7 && player.transform.position.z<=-14)
      {
        if(turns%2==0)
        {
          WhitePoll3[Count3].SetActive(true);
          Count3++;
          turns++;
        }
        else
        {
          BlackPoll3[Count3].SetActive(true);
          Count3++;
          turns++;
          //Debug.Log(Count3);
        }
      }
      //Poll4
      if(player.transform.position.x>=-26.3  && player.transform.position.x<=-21.1 && player.transform.position.z>=-16.7 && player.transform.position.z<=-11.5)
      {
        if(turns%2==0)
        {
          WhitePoll4[Count4].SetActive(true);
          Count4++;
          turns++;
        }
        else
        {
          BlackPoll4[Count4].SetActive(true);
          Count4++;
          turns++;
          //Debug.Log(Count3);
        }
      }
      //Poll5
      if(player.transform.position.x>=20.3  && player.transform.position.x<=25.5 && player.transform.position.z>=-6 && player.transform.position.z<=-0.6)
      {
        if(turns%2==0)
        {
          WhitePoll5[Count5].SetActive(true);
          Count5++;
          turns++;
        }
        else
        {
          BlackPoll5[Count5].SetActive(true);
          Count5++;
          turns++;
          //Debug.Log(Count5);
        }
      }
      //Poll6
      if(player.transform.position.x>=2.2  && player.transform.position.x<=7.5 && player.transform.position.z>=-8.9 && player.transform.position.z<=-1)
      {
        if(turns%2==0)
        {
          WhitePoll6[Count6].SetActive(true);
          Count6++;
          turns++;
        }
        else
        {
          BlackPoll6[Count6].SetActive(true);
          Count6++;
          turns++;
          //Debug.Log(Count6);
        }
      }
      //Poll7
      if(player.transform.position.x>=-13.6  && player.transform.position.x<=-6.9 && player.transform.position.z>=-6.9 && player.transform.position.z<=0.6)
      {
        if(turns%2==0)
        {
          WhitePoll7[Count7].SetActive(true);
          Count7++;
          turns++;
        }
        else
        {
          BlackPoll7[Count7].SetActive(true);
          Count7++;
          turns++;
          //Debug.Log(Count6);
        }
      }
        //Poll8
        if(player.transform.position.x>=-26  && player.transform.position.x<=-19.2 && player.transform.position.z>=-6.9 && player.transform.position.z<=0.6)
        {
          if(turns%2==0)
          {
            WhitePoll8[Count8].SetActive(true);
            Count8++;
            turns++;
          }
          else
          {
            BlackPoll8[Count8].SetActive(true);
            Count8++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll9
        if(player.transform.position.x>=20.5  && player.transform.position.x<=24.4 && player.transform.position.z>=7.9 && player.transform.position.z<=13)
        {
          if(turns%2==0)
          {
            WhitePoll9[Count9].SetActive(true);
            Count9++;
            turns++;
          }
          else
          {
            BlackPoll9[Count9].SetActive(true);
            Count9++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //poll10
        if(player.transform.position.x>=4.2  && player.transform.position.x<=9.3 && player.transform.position.z>=4.6 && player.transform.position.z<=14.9)
        {
          if(turns%2==0)
          {
            WhitePoll10[Count10].SetActive(true);
            Count10++;
            turns++;
          }
          else
          {
            BlackPoll10[Count10].SetActive(true);
            Count10++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll11
        if(player.transform.position.x>=-14.9  && player.transform.position.x<=-7.9 && player.transform.position.z>=4.6 && player.transform.position.z<=14.9)
        {
          if(turns%2==0)
          {
            WhitePoll11[Count11].SetActive(true);
            Count11++;
            turns++;
          }
          else
          {
            BlackPoll11[Count11].SetActive(true);
            Count11++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll12
        if(player.transform.position.x>=-25.5  && player.transform.position.x<=-20 && player.transform.position.z>=9.8 && player.transform.position.z<=16.6)
        {
          if(turns%2==0)
          {
            WhitePoll12[Count12].SetActive(true);
            Count12++;
            turns++;
          }
          else
          {
            BlackPoll12[Count12].SetActive(true);
            Count12++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll13
        if(player.transform.position.x>=19.7  && player.transform.position.x<=24.5 && player.transform.position.z>=25.6 && player.transform.position.z<=32.5)
        {
          if(turns%2==0)
          {
            WhitePoll13[Count13].SetActive(true);
            Count13++;
            turns++;
          }
          else
          {
            BlackPoll13[Count13].SetActive(true);
            Count13++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll14
        if(player.transform.position.x>=2.4  && player.transform.position.x<=9.4 && player.transform.position.z>=25.6 && player.transform.position.z<=32.5)
        {
          if(turns%2==0)
          {
            WhitePoll14[Count14].SetActive(true);
            Count14++;
            turns++;
          }
          else
          {
            BlackPoll14[Count14].SetActive(true);
            Count14++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll15
        if(player.transform.position.x>=-15.1  && player.transform.position.x<=-6.6 && player.transform.position.z>=25.6 && player.transform.position.z<=32.5)
        {
          if(turns%2==0)
          {
            WhitePoll15[Count15].SetActive(true);
            Count15++;
            turns++;
          }
          else
          {
            BlackPoll15[Count15].SetActive(true);
            Count15++;
            turns++;
            //Debug.Log(Count6);
          }
        }
        //Poll16
        if(player.transform.position.x>=-25.4  && player.transform.position.x<=-19.9 && player.transform.position.z>=25.6 && player.transform.position.z<=32.5)
        {
          if(turns%2==0)
          {
            WhitePoll16[Count16].SetActive(true);
            Count16++;
            turns++;
          }
          else
          {
            BlackPoll16[Count16].SetActive(true);
            Count16++;
            turns++;
            //Debug.Log(Count6);
          }
        }
    }
}
