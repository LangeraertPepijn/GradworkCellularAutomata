
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField]
    private float _mouseSensitivity = 100.0f;
    [SerializeField]
    private Transform _playerBody = null;

    private float xRotation = 0f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X")*_mouseSensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*_mouseSensitivity*Time.deltaTime;
        Debug.Log(mouseY);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        transform.localRotation=Quaternion.Euler(xRotation,0f,0f);
        _playerBody.Rotate(Vector3.up*mouseX);
    }
}
