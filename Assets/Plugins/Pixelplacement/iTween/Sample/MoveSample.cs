using UnityEngine;

public class MoveSample : MonoBehaviour
{	
	private void Start()
	{
		ITween.MoveBy(gameObject, ITween.Hash("x", 2, "easeType", "easeInOutExpo", "loopType", "pingPong", "delay", .1));
	}
}