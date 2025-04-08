using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera whiteCamera;
    public Camera blackCamera;
    private bool isWhite;
    private Camera currentCamera;
    private Camera freeCamera;
    private bool isFreeCameraActive;

    private void Start()
    {
        string userSide = PlayerPrefs.GetString("UserSide", "white");
        whiteCamera.enabled = userSide == "white";
        blackCamera.enabled = userSide == "black";
        isWhite = userSide == "white";
        currentCamera = isWhite ? whiteCamera : blackCamera;
        isFreeCameraActive = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            whiteCamera.enabled = !whiteCamera.enabled;
            blackCamera.enabled = !blackCamera.enabled;
            isWhite = !isWhite;
            currentCamera = isWhite ? whiteCamera : blackCamera;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isFreeCameraActive)
            {
                Destroy(freeCamera.gameObject);
                currentCamera.enabled = true;
                isFreeCameraActive = false;
            }
            else
            {
                freeCamera = Instantiate(currentCamera, currentCamera.transform.position, currentCamera.transform.rotation);
                freeCamera.enabled = true;
                currentCamera.enabled = false;
                isFreeCameraActive = true;
            }
        }

        if (isFreeCameraActive)
        {
            float moveSpeed = 10f;
            float lookSpeed = 3f;
            float rotationX = Input.GetAxis("Mouse X") * lookSpeed;
            float rotationY = -Input.GetAxis("Mouse Y") * lookSpeed;

            freeCamera.transform.Rotate(0, rotationX, 0);
            freeCamera.transform.Rotate(rotationY, 0, 0);

            float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
            float moveZ = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

            freeCamera.transform.Translate(moveX, 0, moveZ);
        }
    }
}