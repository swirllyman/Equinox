using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

    Transform lookPos;
    bool setup = false;
    public bool worldUp = false;
    public bool scale = false;
    public float maxDistance = 40.0f;
    public int scaleFactor = 3;

    Vector3 startScale;
    // Use this for initialization
    void Start()
    {
        startScale = transform.localScale;

        GameObject camObject = GameObject.FindWithTag("MainCamera");
        Camera myCam;
        if (camObject != null)
        {
            myCam = camObject.GetComponent<Camera>();
            lookPos = myCam.transform;
            setup = true;
        }
        else
        {
            myCam = FindObjectOfType<Camera>();
            if (myCam == null)
            {
                StartCoroutine(LateStart());
            }
            else
            {
                lookPos = myCam.transform;
                setup = true;
            }
        }
    }
    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1.0f);
        Camera myCam = FindObjectOfType<Camera>();
        if (myCam == null)
        {
            myCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
            if(myCam != null)
            {
                setup = true;
                lookPos = myCam.transform;
            }
        }
        else
        {
            lookPos = myCam.transform;
            setup = true;
        }
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (setup)
        {
            if (worldUp)
            {
                transform.LookAt(new Vector3(lookPos.position.x, transform.position.y, lookPos.position.z));
            }
            else
            {
                transform.LookAt(lookPos);
            }
        }

        if (scale)
        {
            float currentDistance = Vector3.Distance(transform.position, lookPos.position);
            transform.localScale = Vector3.Lerp(startScale, startScale * scaleFactor, currentDistance / maxDistance);
        }
	}
}
