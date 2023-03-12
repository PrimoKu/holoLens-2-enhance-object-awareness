using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        this.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(target) {
            this.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            this.transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            this.transform.LookAt(target.transform, this.transform.up);
        } else {
            this.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
        }

    }
}
