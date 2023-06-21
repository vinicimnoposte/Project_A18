using UnityEngine;
using TMPro;

public class NPCInteraction : MonoBehaviour
{
    public string npcName;  // Nome do NPC
    public string[] dialogue;  // Diálogos do NPC

    private bool canInteract = false;  // Flag para verificar se o jogador pode interagir
    private bool isInteracting = false;  // Flag para verificar se o jogador está interagindo

    public TextMeshProUGUI dialogueText;  // Referência ao componente TextMeshProUGUI na UI

    private int currentDialogueIndex = 0;  // Índice do diálogo atual

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canInteract = false;
        }
    }

    private void Update()
    {
        if (canInteract && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartInteraction();
        }
        else if (isInteracting && Input.GetKeyDown(KeyCode.E))
        {
            ContinueInteraction();
        }
    }

    private void StartInteraction()
    {
        isInteracting = true;
        dialogueText.gameObject.SetActive(true);
        dialogueText.text = npcName + ": " + dialogue[currentDialogueIndex];
    }

    private void ContinueInteraction()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogue.Length)
        {
            dialogueText.text = npcName + ": " + dialogue[currentDialogueIndex];
        }
        else
        {
            EndInteraction();
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;
        dialogueText.gameObject.SetActive(false);
        currentDialogueIndex = 0;
    }
}
