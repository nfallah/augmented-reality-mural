using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationManager : MonoBehaviour
{
    // TODO: location sychronization via spatial anchors... to be done next year :)
    private void Start()
    {
        LocationService l = new();
        l.Start(0.05f, 0.05f);
        //Debug.Log(l.lastData);
        //Debug.Log(l.isEnabledByUser);
    }
}
