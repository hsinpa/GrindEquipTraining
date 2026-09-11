using UnityEngine;

public class GrinderSpin : MonoBehaviour
{
    [Tooltip("Config")]
    public float rotationSpeed = 1080f;

    [SerializeField]
    private GameObject[] spinObjects;

    void Update()
    {

        if (spinObjects != null)
        {
            foreach (GameObject p_gameobject in spinObjects)
            {
                p_gameobject.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime, Space.Self);   
            }
        }
    }
}