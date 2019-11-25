using UnityEngine;

public class RectTransformRotator : MonoBehaviour {

    [SerializeField] float speed;
    [SerializeField] float offset;
    [SerializeField] float updateInterval;
    
    RectTransform rt;
    float lastUpdate;

    void Start () {
        lastUpdate = Time.time;
        rt = GetComponent<RectTransform>();
    }

    void Update () {
        if(Time.time - lastUpdate > updateInterval){
            rt.localEulerAngles = new Vector3(0, 0, Time.time * speed);
            lastUpdate = Time.time;
        }
    }
	
}
