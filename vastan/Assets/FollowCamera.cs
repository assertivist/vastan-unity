using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour {
    public Camera c;
    private Vector3 pos;
    // Use this for initialization
    void Start () {
        pos = transform.position;
    }
    
    // Update is called once per frame
    void LateUpdate () {
        if (c != null)
        transform.position = pos + c.transform.position;
    }
}
