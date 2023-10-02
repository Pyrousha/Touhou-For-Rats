using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HeroDialogueInteract : MonoBehaviour
{
    public List<IInteractable> Interactables { get; private set; } = new List<IInteractable>();
    private IInteractable highestPrioInteractable = null;

    // Update is called once per frame
    void Update()
    {
        if (DialogueUI.Instance.isOpen == false)
        {
            if (highestPrioInteractable != null)
            {
                Interactables[0].TryInteract(this);
            }
        }
    }

    public void AddInteractable(IInteractable dialogueActivator)
    {
        if (Interactables.Contains(dialogueActivator))
            return;

        Interactables.Add(dialogueActivator);

        //Sort List
        Interactables = Interactables.OrderByDescending(a => a.Priority).ToList();

        highestPrioInteractable = Interactables[0];
    }

    public void RemoveInteractable(IInteractable dialogueActivator)
    {
        int index = Interactables.IndexOf(dialogueActivator);
        if (index >= 0)
        {
            //List contains this interactable
            Interactables.RemoveAt(index);
        }

        if (Interactables.Count > 0)
            highestPrioInteractable = Interactables[0];
        else
            highestPrioInteractable = null;
    }
}
