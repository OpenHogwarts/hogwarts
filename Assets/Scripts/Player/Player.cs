using UnityEngine;
using System.Collections;

public class Player : Photon.MonoBehaviour {

	private Vector3 correctPlayerPos = Vector3.zero; // We lerp towards this
	private Quaternion correctPlayerRot = Quaternion.identity; // We lerp towards this

	Animator anim;
	bool gotFirstUpdate = false;
	public TextMesh nick;

	void Start () {
		anim = GetComponent<Animator>();
	}

	void Update()
	{
		if (!photonView.isMine) {
			transform.position = Vector3.Lerp(transform.position, this.correctPlayerPos, Time.deltaTime * 5);
			transform.rotation = Quaternion.Lerp(transform.rotation, this.correctPlayerRot, Time.deltaTime * 5);
		} else {
			if (!gotFirstUpdate) {
				this.GetComponent<PhotonView>().RPC("setNick", PhotonTargets.Others, PhotonNetwork.player.name);
			}

			//looks like player is falling
			if (transform.position.y < -100) {
				transform.position = GameObject.Find("SpawnPoints/FirstJoin").transform.position;
			}
		}
	}

	void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting)
		{
			// We own this player: send the others our data
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
		}
		else
		{
			// Network player, receive data
			this.correctPlayerPos = (Vector3)stream.ReceiveNext();
			this.correctPlayerRot = (Quaternion)stream.ReceiveNext();

			if (anim) {
				anim.SetFloat("Speed", (float)stream.ReceiveNext());
				anim.SetBool("Jumping", (bool)stream.ReceiveNext());
			}

			
			if(gotFirstUpdate == false) {
				transform.position = this.correctPlayerPos;
				transform.rotation = this.correctPlayerRot;
				gotFirstUpdate = true;
			}
			
		}
	}

	[RPC]
	void setNick (string name) {
		nick.text = name;
	}
}
