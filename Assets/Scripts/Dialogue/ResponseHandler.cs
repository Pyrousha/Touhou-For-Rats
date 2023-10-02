using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResponseHandler : MonoBehaviour
{
    [SerializeField] private RectTransform responsesParent;
    [SerializeField] private RectTransform responseBox;
    [SerializeField] private RectTransform responseButtonTemplate;
    [SerializeField] private RectTransform responseContainer;
    [SerializeField] private Transform triangleIndicator;

    private DialogueUI dialogueUI;
    private ResponseEvent[] responseEvents;

    private List<GameObject> tempResponseButtons = new List<GameObject>();

    private GameObject[] responseObjects;
    private Response[] responseArray;
    private int responseIndex;
    private bool responsesEnabled = false;

    private void Start()
    {
        dialogueUI = GetComponent<DialogueUI>();
    }

    public void AddResponseEvents(ResponseEvent[] responseEvents)
    {
        this.responseEvents = responseEvents;
    }

    private void Update()
    {
        if (responsesEnabled == false)
            return;

        //Press Down
        if (InputHandler.Instance.Down.Down)
        {
            responseIndex++;
            if (responseIndex > (responseObjects.Length - 1))
                responseIndex = 0;
            SetIndicator();
        }

        //Press up
        if (InputHandler.Instance.Up.Down)
        {
            responseIndex--;
            if (responseIndex < 0)
                responseIndex = (responseObjects.Length - 1);
            SetIndicator();
        }

        //Press Space
        if (InputHandler.Instance.Interact_Shoot.Down)
            OnPickedResponse(responseArray[responseIndex], responseIndex);
    }

    public void ShowResponses(Response[] responses)
    {
        float responseBoxHeight = 0;

        responseObjects = new GameObject[responses.Length];
        responseArray = new Response[responses.Length];

        List<RectTransform> rectsToUpdate = new List<RectTransform>() { responsesParent };

        for (int i = 0; i < responses.Length; i++)
        {
            Response response = responses[i];
            int responseIndex = i;

            responseArray[i] = response;

            GameObject responseButton = Instantiate(responseButtonTemplate.gameObject, responseContainer);
            responseButton.gameObject.SetActive(true);

            responseObjects[i] = responseButton;

            TMP_Text[] responseTexts = responseButton.GetComponentsInChildren<TMP_Text>();
            foreach (TMP_Text txt in responseTexts)
                txt.text = response.ResponseText;

            responseButton.GetComponent<Button>().onClick.AddListener(() => OnPickedResponse(response, responseIndex));

            tempResponseButtons.Add(responseButton);

            responseBoxHeight += responseButtonTemplate.sizeDelta.y;

            rectsToUpdate.Add(responseButton.GetComponent<RectTransform>());
        }

        responseIndex = 0;

        responseBox.sizeDelta = new Vector2(responseBox.sizeDelta.x, responseBoxHeight);
        responseBox.gameObject.SetActive(true);

        foreach (RectTransform rect in rectsToUpdate)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        LayoutRebuilder.ForceRebuildLayoutImmediate(responseContainer);

        SetIndicator();

        responsesEnabled = true;
    }

    private void SetIndicator()
    {
        triangleIndicator.parent = responseObjects[responseIndex].transform.Find("IncidatorLocation");
        triangleIndicator.localPosition = Vector3.zero;
    }

    private void OnPickedResponse(Response response, int responseIndex)
    {
        triangleIndicator.parent = responseBox;

        responsesEnabled = false;

        responseBox.gameObject.SetActive(false);

        while (tempResponseButtons.Count > 0)
        {
            Destroy(tempResponseButtons[^1]);
            tempResponseButtons.RemoveAt(tempResponseButtons.Count - 1);
        }

        if (responseEvents != null && responseIndex <= responseEvents.Length)
        {
            responseEvents[responseIndex].OnPickedResponse?.Invoke();
        }

        responseEvents = null;
        responseArray = null;
        responseObjects = null;

        if (response.DialogueObject)
        {
            dialogueUI.ShowDialogue(response.DialogueObject, null);
        }
        else
        {
            dialogueUI.CloseDialogueBox();
        }
    }
}
