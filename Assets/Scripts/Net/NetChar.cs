using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetChar : Photon.MonoBehaviour {

	//Este script sera el que controlar lo que se envia y recibe de los jugadores a traves de photon
	//Tened en cuenta que se envia en serie, es decir, se recibira en el mismo orden que se envia
	//El PhotonView debera observar este script en vez del objeto en si, para poder manejar mas cosas que la posicion y el movimiento
	//Vancete 05/01

	Vector3 rpos;
	Quaternion rrot;
	public Animator anim;
	public TextMesh nick;
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		if (photonView.isMine) {
			
		} else {
			transform.position = rpos;
			transform.rotation = rrot;
		}
	}

	//Enviaremos en primer lugar el nombre del jugador, para aplicarlo despues al textmesh
	//Despues posicion y rotacion
	//Por ultimo (por ahora), las animaciones
	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if (stream.isWriting) {
			stream.SendNext(PhotonNetwork.playerName);
			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("Speed"));
			stream.SendNext(anim.GetBool("Jumping"));
		} else {
			nick.text = (string)stream.ReceiveNext();
			rpos = (Vector3)stream.ReceiveNext();
			rrot = (Quaternion)stream.ReceiveNext();
			anim.SetFloat("Speed", (float)stream.ReceiveNext());
			anim.SetBool("Jumping", (bool)stream.ReceiveNext());
		}
	}
}
