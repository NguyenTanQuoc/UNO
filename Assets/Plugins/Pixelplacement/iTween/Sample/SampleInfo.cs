using UnityEngine;
using System.Collections;

public class SampleInfo : MonoBehaviour
{
void OnGUI(){
		GUILayout.Label("ITween can spin, shake, punch, move, handle audio, fade color and transparency \nand much more with each task needing only one line of code.");
		GUILayout.BeginHorizontal();
		GUILayout.Label("ITween works with C#, JavaScript and Boo. For full documentation and examples visit:");
		if(GUILayout.Button("http://ITween.pixelplacement.com")){
			Application.OpenURL("http://ITween.pixelplacement.com");
		}
		GUILayout.EndHorizontal();
	}
}

