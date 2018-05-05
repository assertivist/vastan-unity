using UnityEngine;
using System.Collections;

public class DestroySound : MonoBehaviour {

    // Use this for initialization
    void Start () {
        AudioSource mysound = GetComponent<AudioSource>();
        Destroy(gameObject, mysound.clip.length);
    }
    
    // Update is called once per frame
    void Update () {
    
    }
}
