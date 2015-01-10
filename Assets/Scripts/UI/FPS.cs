using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FPS : MonoBehaviour {

	public  float updateInterval = 0.5F;
	
	private float accum   = 0;
	private int   frames  = 0;
	private float timeleft;
	public Text fpst;
	public bool ping = false;
	
	void Start()
	{
		timeleft = updateInterval;  
	}
	
	void Update()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale/Time.deltaTime;
		++frames;

		if( timeleft <= 0.0 )
		{
			float fps = accum/frames;
			string format = System.String.Format("{0:F2} FPS",fps);
			if(ping){
			fpst.text = "Ping: "+PhotonNetwork.GetPing().ToString()+"ms / "+format;
			}else{
			fpst.text = format;
			}

			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}
}
