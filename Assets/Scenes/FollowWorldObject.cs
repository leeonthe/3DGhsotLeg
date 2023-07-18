using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowWorldObject : MonoBehaviour
{
    private GameObject target = null;
    private bool startedFollowing = false;
    private Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (startedFollowing)
        {
            if(target != null)
            {
                Vector3 position = target.transform.position;
                Vector3 screenPos = mainCam.WorldToScreenPoint(position);
                transform.position = screenPos;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void setTarget(GameObject t)
    {
        target = t;
        startedFollowing = true;
    }
}
