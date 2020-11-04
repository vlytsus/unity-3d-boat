using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WaweManager : MonoBehaviour
{
    public static WaweManager instance;// = new WaweManager();

    public float amplitude = 1f;
    public float lenght = 2f;
    public float speed = 1f;
    public float offset = 0f;

    private void Awake() {

        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this);
        }

    }

    // Update is called once per frame
    void Update() {
        offset += Time.deltaTime * speed;
    }

    public static float GetWaweHeight(float _x) {

        return instance.amplitude * Mathf.Sin(_x / instance.lenght + instance.offset);
    }
}
