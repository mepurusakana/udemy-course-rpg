using UnityEngine;

public class AnimationTriggerForwarder : MonoBehaviour
{
    public void OnIntroFinishedEvent()
    {
        var parent = GetComponentInParent<SpearController>();
        if (parent != null) parent.OnIntroFinished();
    }

    public void OnOutroFinishedEvent()
    {
        var parent = GetComponentInParent<SpearController>();
        if (parent != null) parent.OnOutroFinished();
    }
}
