using UnityEngine;
using System.Collections;

public class Util : MonoBehaviour {

	public static Vector3 formatMoney (int money) {

		int x = money / 10000;
		int y = (money - (x * 10000)) / 100;
		int z = (money - (x * 10000) - (y * 100));

		return new Vector3(x, y, z);
	}
}
