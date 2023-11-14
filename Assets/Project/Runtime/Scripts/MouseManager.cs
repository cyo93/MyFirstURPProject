using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }


    void Start(){
    Cursor.lockState = CursorLockMode.Locked;
}
    // Update is called once per frame
    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}
