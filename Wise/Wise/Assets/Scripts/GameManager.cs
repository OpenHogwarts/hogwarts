using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject [,] verticalWhite;
    public GameObject [,] verticalBlack;
    public GameObject [] diagnalCheck;
    public GameObject [] diagnalCheck2;
    public GameObject [] diagnalCheck3;
    public GameObject [] diagnalCheck4;
    public GameObject [] diagnalCheckB;
    public GameObject [] diagnalCheckB2;
    public GameObject [] diagnalCheckB3;
    public GameObject [] diagnalCheckB4;
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
    private int Count=0;
    private int lines=0;
    private int horiWhite=0;
    private int horiWhite2=0;
    private int horiWhite3=0;
    private int horiWhite4=0;
    private int horiWhite5=0;
    private int horiBlack=0;
    private int horiBlack2=0;
    private int horiBlack3=0;
    private int horiBlack4=0;
    private int horiBlack5=0;
    private int diagnal=0;
    private int diagnal2=0;
    private int diagnal3=0;
    private int diagnal4=0;
    private int diagnal5=0;
    private int diagnal6=0;
    private int diagnalBlack=0;
    private int diagnalBlack2=0;
    private int diagnalBlack3=0;
    private int diagnalBlack4=0;
    private int diagnalBlack5=0;
    private int diagnalBlack6=0;
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
      verticalWhite= new GameObject[16,4];
      verticalBlack= new GameObject[16,4];
      //2D Array for white tips only
      for(int i=0; i<16; i++)
      {
        for(int j=0;j<4;j++)
        {
          if(i==0)
          {
            verticalWhite[i,j]=WhitePoll1[j];
          }
          if(i==1)
          {
            verticalWhite[i,j]=WhitePoll2[j];
          }
          if(i==2)
          {
            verticalWhite[i,j]=WhitePoll3[j];
          }
          if(i==3)
          {
            verticalWhite[i,j]=WhitePoll4[j];
          }
          if(i==4)
          {
            verticalWhite[i,j]=WhitePoll5[j];
          }
          if(i==5)
          {
            verticalWhite[i,j]=WhitePoll6[j];
          }
          if(i==6)
          {
            verticalWhite[i,j]=WhitePoll7[j];
          }
          if(i==7)
          {
            verticalWhite[i,j]=WhitePoll8[j];
          }
          if(i==8)
          {
            verticalWhite[i,j]=WhitePoll9[j];
          }
          if(i==9)
          {
            verticalWhite[i,j]=WhitePoll10[j];
          }
          if(i==10)
          {
            verticalWhite[i,j]=WhitePoll11[j];
          }
          if(i==11)
          {
            verticalWhite[i,j]=WhitePoll12[j];
          }
          if(i==12)
          {
            verticalWhite[i,j]=WhitePoll13[j];
          }
          if(i==13)
          {
            verticalWhite[i,j]=WhitePoll14[j];
          }
          if(i==14)
          {
            verticalWhite[i,j]=WhitePoll15[j];
          }
          if(i==15)
          {
            verticalWhite[i,j]=WhitePoll16[j];
          }
        }
      }
      for(int i=0; i<16; i++)
      {
        for(int j=0;j<4;j++)
        {
          if(i==0)
          {
            verticalBlack[i,j]=BlackPoll1[j];
          }
          if(i==1)
          {
            verticalBlack[i,j]=BlackPoll2[j];
          }
          if(i==2)
          {
            verticalBlack[i,j]=BlackPoll3[j];
          }
          if(i==3)
          {
            verticalBlack[i,j]=BlackPoll4[j];
          }
          if(i==4)
          {
            verticalBlack[i,j]=BlackPoll5[j];
          }
          if(i==5)
          {
            verticalBlack[i,j]=BlackPoll6[j];
          }
          if(i==6)
          {
            verticalBlack[i,j]=BlackPoll7[j];
          }
          if(i==7)
          {
            verticalBlack[i,j]=BlackPoll8[j];
          }
          if(i==8)
          {
            verticalBlack[i,j]=BlackPoll9[j];
          }
          if(i==9)
          {
            verticalBlack[i,j]=BlackPoll10[j];
          }
          if(i==10)
          {
            verticalBlack[i,j]=BlackPoll11[j];
          }
          if(i==11)
          {
            verticalBlack[i,j]=BlackPoll12[j];
          }
          if(i==12)
          {
            verticalBlack[i,j]=BlackPoll13[j];
          }
          if(i==13)
          {
            verticalBlack[i,j]=BlackPoll14[j];
          }
          if(i==14)
          {
            verticalBlack[i,j]=BlackPoll15[j];
          }
          if(i==15)
          {
            verticalBlack[i,j]=BlackPoll16[j];
          }
        }
      }
    }

    // Update is called once per frame
    void Update()
    {
      //Shows who's turn is it
      if(turns%2==0)
      {//When the turn is even, it's white's turn
          whiteMessage.SetActive(true);
          blackMessage.SetActive(false);
      }
      else
      {////When the turn is odd, it's black's turn
        blackMessage.SetActive(true);
        whiteMessage.SetActive(false);
      }


      if(Input.GetKeyDown(KeyCode.Return))
      {

        checkButtons.SetActive(true);
        Debug.Log(player.transform.position);
      }
      //Checking the white's vertical winning condition
      for(int i=0; i<16; i++)
      {
        if(Count==4)
        {
          Debug.Log("White Wins");
          Count=0;
        }
        else
        {
          Count=0;
        }
        for(int j=0;j<4;j++)
        {
          if(verticalWhite[i,j].activeInHierarchy==true)
          {
            Count++;
            //Debug.Log(Count);
          }
         }
        }
        //Checking the white's horizontal winning condition
        for(int i=0; i<4; i++)
        {
          if(horiWhite==4)
          {
            Debug.Log("White Wins");
            horiWhite=0;
          }
          else
          {
            horiWhite=0;
          }
          for(int j=0;j<4;j++)
          {
            if(verticalWhite[j,i].activeInHierarchy==true)
            {
              horiWhite++;
              //Debug.Log(Count);
            }
           }
        }

        for(int i=0; i<4; i++)
        {
            if(horiWhite2==4)
            {
              Debug.Log("White Wins");
              horiWhite2=0;
            }
            else
            {
              horiWhite2=0;
            }
            for(int j=0;j<=12;j+=4)
            {
              if(verticalWhite[j,i].activeInHierarchy==true)
              {
                horiWhite2++;
                //Debug.Log(horiWhite2);
              }
            }
        }

        for(int i=0; i<4; i++)
        {
            if(horiWhite3==4)
            {
              Debug.Log("White Wins");
              horiWhite3=0;
            }
            else
            {
              horiWhite3=0;
            }
            for(int j=1;j<=13;j+=4)
            {
               if(verticalWhite[j,i].activeInHierarchy==true)
               {
                 horiWhite3++;
                 //Debug.Log(horiWhite2);
               }
            }
        }

        for(int i=0; i<4; i++)
        {
            if(horiWhite4==4)
            {
              Debug.Log("White Wins");
              horiWhite4=0;
            }
            else
            {
              horiWhite4=0;
            }
              for(int j=2;j<=14;j+=4)
              {
                if(verticalWhite[j,i].activeInHierarchy==true)
                {
                  horiWhite4++;
                  //Debug.Log(horiWhite2);
                }
              }
        }
        for(int i=0; i<4; i++)
        {
            if(horiWhite5==4)
            {
              Debug.Log("White Wins");
              horiWhite5=0;
            }
            else
            {
              horiWhite5=0;
            }
              for(int j=3;j<=15;j+=4)
              {
                if(verticalWhite[j,i].activeInHierarchy==true)
                {
                  horiWhite5++;
                  //Debug.Log(horiWhite2);
                }
              }
        }

       //Checking the white's diagnal winning condition
        for(int i=0;i<16;i++)
        {
          if(i==4 || i==8 || i==12)
          {
            diagnal=0;
          }

          if(diagnalCheck[i].activeInHierarchy==true)
          {
            diagnal++;
            //Debug.Log(diagnal);
          }

          if(diagnal==4)
          {
            Debug.Log("White wins");
          }

        }

        for(int i=0;i<16;i++)
        {
          if(i==4 || i==8 || i==12)
          {
            diagnal2=0;
          }

          if(diagnalCheck2[i].activeInHierarchy==true)
          {
            diagnal2++;
            //Debug.Log(diagnal2);
          }

          if(diagnal2==4)
          {
            Debug.Log("White wins");
          }
        }

        for(int i=0;i<16;i++)
        {
          if(i==4 || i==8 || i==12)
          {
            diagnal3=0;
          }

          if(diagnalCheck3[i].activeInHierarchy==true)
          {
            diagnal3++;
            //Debug.Log(diagnal3);
          }

          if(diagnal3==4)
          {
            Debug.Log("White wins");
          }
        }

        for(int i=0;i<16;i++)
        {
          if(i==4 || i==8 || i==12)
          {
            diagnal4=0;
          }

          if(diagnalCheck4[i].activeInHierarchy==true)
          {
            diagnal4++;
            //Debug.Log(diagnal4);
          }

          if(diagnal4==4)
          {
            Debug.Log("White wins");
          }

        }
        for(int i=0;i<4;i++)
        {
          if(diagnal5==4)
          {
            Debug.Log("White Wins");
            diagnal5=0;
          }
          else
          {
            diagnal5=0;
          }
          for(int j=0;j<=15;j+=5)
          {
            if(verticalWhite[j,i].activeInHierarchy==true)
            {
              diagnal5++;
              //Debug.Log(diagnal5);
            }
          }
        }

        for(int i=0;i<4;i++)
        {
          if(diagnal6==4)
          {
            Debug.Log("White Wins");
            diagnal6=0;
          }
          else
          {
            diagnal6=0;
          }
          for(int j=3;j<=12;j+=3)
          {
            if(verticalWhite[j,i].activeInHierarchy==true)
            {
              diagnal6++;
              //Debug.Log(diagnal6);
            }
          }
        }
        //Checking the black's vertical winning condition
        for(int i=0; i<16; i++)
        {
          if(lines==4)
          {
            Debug.Log("Black Wins");
            lines=0;
          }
          else
          {
            lines=0;
          }
          for(int j=0;j<4;j++)
          {
            if(verticalBlack[i,j].activeInHierarchy==true)
            {
              lines++;
              //Debug.Log(lines);
            }
          }
        }

          //Checking the black's horizontal winning condition
          for(int i=0; i<4; i++)
          {
            if(horiBlack==4)
            {
              Debug.Log("Black Wins");
              horiBlack=0;
            }
            else
            {
              horiBlack=0;
            }
            for(int j=0;j<4;j++)
            {
              if(verticalBlack[j,i].activeInHierarchy==true)
              {
                horiBlack++;
                //Debug.Log(Count);
              }
             }
          }

          for(int i=0; i<4; i++)
          {
              if(horiBlack2==4)
              {
                Debug.Log("Black Wins");
                horiBlack2=0;
              }
              else
              {
                horiBlack2=0;
              }
              for(int j=0;j<=12;j+=4)
              {
                if(verticalBlack[j,i].activeInHierarchy==true)
                {
                  horiBlack2++;
                  //Debug.Log(horiWhite2);
                }
              }
          }
          for(int i=0; i<4; i++)
          {
            if(horiBlack3==4)
            {
              Debug.Log("Black Wins");
              horiBlack3=0;
            }
            else
            {
              horiBlack3=0;
            }
              for(int j=1;j<=13;j+=4)
              {
                 if(verticalBlack[j,i].activeInHierarchy==true)
                 {
                   horiBlack3++;
                   //Debug.Log(horiWhite2);
                 }
              }
          }

          for(int i=0; i<4; i++)
          {
            if(horiBlack4==4)
            {
              Debug.Log("Black Wins");
              horiBlack4=0;
            }
            else
            {
              horiBlack4=0;
            }
              for(int j=2;j<=14;j+=4)
              {
                  if(verticalBlack[j,i].activeInHierarchy==true)
                  {
                    horiBlack4++;
                    //Debug.Log(horiWhite2);
                  }
              }
          }

          for(int i=0; i<4; i++)
          {
            if(horiBlack5==4)
            {
              Debug.Log("Black Wins");
              horiBlack5=0;
            }
            else
            {
              horiBlack5=0;
            }
              for(int j=3;j<=15;j+=4)
              {
                  if(verticalBlack[j,i].activeInHierarchy==true)
                  {
                    horiBlack5++;
                    //Debug.Log(horiWhite2);
                  }
              }
          }
            //Check the black's diagnal winning condition
            for(int i=0;i<4;i++)
            {
              if(diagnalBlack==4)
              {
                Debug.Log("Black Wins");
                diagnalBlack=0;
              }
              else
              {
                diagnalBlack=0;
              }
              for(int j=0;j<=15;j+=5)
              {
                if(verticalBlack[j,i].activeInHierarchy==true)
                {
                  diagnalBlack++;
                  //Debug.Log(diagnal5);
                }
              }
            }

            for(int i=0;i<4;i++)
            {
              if(diagnalBlack2==4)
              {
                Debug.Log("Black Wins");
                diagnalBlack2=0;
              }
              else
              {
                diagnalBlack2=0;
              }
              for(int j=3;j<=12;j+=3)
              {
                if(verticalBlack[j,i].activeInHierarchy==true)
                {
                  diagnalBlack2++;
                  //Debug.Log(diagnal6);
                }
              }
            }

            for(int i=0;i<16;i++)
            {
              if(i==4 || i==8 || i==12)
              {
                diagnalBlack3=0;
              }

              if(diagnalCheckB[i].activeInHierarchy==true)
              {
                diagnalBlack3++;
                //Debug.Log(diagnal2);
              }

              if(diagnalBlack3==4)
              {
                Debug.Log("Black wins");
              }
            }

            for(int i=0;i<16;i++)
            {
              if(i==4 || i==8 || i==12)
              {
                diagnalBlack4=0;
              }

              if(diagnalCheckB2[i].activeInHierarchy==true)
              {
                diagnalBlack4++;
                //Debug.Log(diagnal2);
              }

              if(diagnalBlack4==4)
              {
                Debug.Log("Black wins");
              }
            }

            for(int i=0;i<16;i++)
            {
              if(i==4 || i==8 || i==12)
              {
                diagnalBlack5=0;
              }

              if(diagnalCheckB3[i].activeInHierarchy==true)
              {
                diagnalBlack5++;
                //Debug.Log(diagnal2);
              }

              if(diagnalBlack5==4)
              {
                Debug.Log("Black wins");
              }
            }

            for(int i=0;i<16;i++)
            {
              if(i==4 || i==8 || i==12)
              {
                diagnalBlack6=0;
              }

              if(diagnalCheckB4[i].activeInHierarchy==true)
              {
                diagnalBlack6++;
                //Debug.Log(diagnal2);
              }

              if(diagnalBlack6==4)
              {
                Debug.Log("Black wins");
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
      if(player.transform.position.x>=20.3  && player.transform.position.x<=25.5 && player.transform.position.z>=-6.7 && player.transform.position.z<=-0.6)
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
      if(player.transform.position.x>=-13.6  && player.transform.position.x<=-6.9 && player.transform.position.z>=-7.3 && player.transform.position.z<=0.6)
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
        if(player.transform.position.x>=20.5  && player.transform.position.x<=24.9 && player.transform.position.z>=7.9 && player.transform.position.z<=13)
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
        if(player.transform.position.x>=-14.9  && player.transform.position.x<=-7.8 && player.transform.position.z>=4.6 && player.transform.position.z<=14.9)
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
        if(player.transform.position.x>=19.7  && player.transform.position.x<=24.9 && player.transform.position.z>=25.6 && player.transform.position.z<=32.6)
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
        if(player.transform.position.x>=-25.4  && player.transform.position.x<=-19.9 && player.transform.position.z>=25.6 && player.transform.position.z<=32.8)
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
