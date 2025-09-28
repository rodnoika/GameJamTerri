using UnityEngine;

public class SpikeTrap : TrapBase
{
    public Animator Animator;
    public string TriggerName = "Activate";
    public AudioClip SpikeSfx;

    protected override void ActivateTrap(Transform target)
    {
        if (Animator && !string.IsNullOrEmpty(TriggerName))
            Animator.SetTrigger(TriggerName);

        if (SpikeSfx)
            AudioSource.PlayClipAtPoint(SpikeSfx, transform.position);
    }
}
