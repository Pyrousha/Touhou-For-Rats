using UnityEngine;

public interface IInteractable
{
    public Transform Transform { get; set; }

    public int Priority { get; set; }

    void TryInteract(HeroDialogueInteract player);
}
