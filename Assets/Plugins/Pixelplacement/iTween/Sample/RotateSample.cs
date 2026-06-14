using UnityEngine;

public class RotateSample : MonoBehaviour
{	
	private void Start()
	{
		ITween.RotateBy(gameObject, ITween.Hash("x", .25, "easeType", "easeInOutBack", "loopType", "pingPong", "delay", .4));
	}
}