using System.Collections.Generic;
using UnityEngine;

public class EnvironmentButton : MonoBehaviour , IInteractable
{
    [SerializeField]  UnityEventInteractions events = new();

    [SerializeField] DetectedPlayer_SetInteractable detectedPlayer = null;

    private List<Collider> colliders = new List<Collider>();

    private Animator animator = null;

    private const string triggerKey = "Trigger";

    private bool pressed = false;
    private IInteractable selfInteractable => this;
    public bool IsActive => pressed;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        detectedPlayer.Awake(selfInteractable);
    }

    

    public void PlayButtonEvent()
    {
        pressed = true;
        animator.SetTrigger(triggerKey);
        selfInteractable.Activate();
    }

    void IInteractable.Interact()
    {
        if (pressed) return;
        PlayButtonEvent();
    }

    void IActivable.Activate()
    {
        events.OnFirstClick?.Invoke();
    }

    void IActivable.Deactivate()
    {
        events.OnLastClick?.Invoke();
    }
}

[System.Serializable]
public class DetectedPlayer_SetInteractable
{
    [SerializeField] public ColliderM colliderM = null;

    [SerializeField] private IInteractable selfInteractable = null;
    private void Enter(Collider other)
    {
        if (other.TryGetComponent(out ICanInteract CharacterInteractable))
            CharacterInteractable.Interactable = selfInteractable;
    }
    private void Exit(Collider other)
    {
        if (other.TryGetComponent(out ICanInteract CharacterInteractable))
            CharacterInteractable.Interactable = null;
    }

    public void Awake(IInteractable inter)
    {
        selfInteractable = inter;
        colliderM.TriggerEnter += Enter;
        colliderM.TriggerExit += Exit;
    }
}