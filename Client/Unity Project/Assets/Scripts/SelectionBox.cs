using UnityEngine;

public class SelectionBox : MonoBehaviour
{
    private bool activated;

    private void Start()
    {
        if (gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    public void Activate()
    {
        if (activated == true)
        {
            return;
        }

        activated = true;

        if (gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    public void Deactivate()
    {
        if (activated == false)
        {
            return;
        }

        activated = false;
        
        if (gameObject.activeSelf == true)
        {
            gameObject.SetActive(false);
        }
    }

    public void UpdatePos(Vector3 start, Vector3 end)
    {
        if (activated == false)
        {
            return;
        }

        if (MathUtils.Vector3Equals(start, end) == true)
        {
            if (gameObject.activeSelf == true)
            {
                gameObject.SetActive(false);
            }

            return;
        }

        if (gameObject.activeSelf == false)
        {
            gameObject.SetActive(true);
        }

        Vector3 center = (start + end) / 2f;
        transform.position = center;

        Vector3 sizes = end - start;
        transform.localScale = sizes;
    }
}