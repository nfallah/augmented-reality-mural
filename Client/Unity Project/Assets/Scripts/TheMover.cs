using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheMover : MonoBehaviour
{
    public bool isEnabled;

    private Vector3? lastPos, lastScale, lastRotation;
    public IEnumerator Move(float waitTime)
    {
        while (isEnabled)
        {
            uint id = GetComponent<IDContainer>().id;

            if ((lastPos != null && !MathUtils.Vector3Equals(lastPos.Value, transform.position)) ||
                (lastScale != null && !MathUtils.Vector3Equals(lastScale.Value, transform.localScale)) ||
                (lastRotation != null && !MathUtils.Vector3Equals(lastRotation.Value, transform.eulerAngles)))
            {
                //Debug.Log("Moved!");
                ModifyContainer c = new ModifyContainer(id, transform.position, transform.localScale, transform.eulerAngles,
                    JustMonika.Instance.GetChunk(GetComponent<IDContainer>().lastPos));
                GetComponent<IDContainer>().lastPos = transform.position; // Update pos to new.
                Command command = new Command(c, JustMonika.Instance.GetChunk(transform.position));
                ClientManager.Instance.SendCommand(command);
            }

            lastPos = transform.position;
            lastScale = transform.localScale;
            lastRotation = transform.eulerAngles;
            yield return new WaitForSeconds(waitTime);
        }
        lastPos = null;
        lastScale = null;
        lastRotation = null;
    }
}