using UnityEngine;
using System.Collections;

public class DestroyMe : MonoBehaviour
{
	
	public float duration = .5f;
	
	// Use this for initialization
	void Start ()
	{
		Destroy (gameObject, duration);
	}
}
