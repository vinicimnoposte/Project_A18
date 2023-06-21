using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public GameObject iconPrefab;
    public KeyCode interactionKey = KeyCode.E;

    private GameObject iconInstance;
    private bool canInteract;
    public Vector3 anchor;
    private void Start()
    {
        anchor = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        // Instancia o ícone acima do objeto, desativado inicialmente
        iconInstance = Instantiate(iconPrefab, anchor, Quaternion.identity, gameObject.transform);
        iconInstance.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o jogador entrou no alcance do objeto
        if (other.CompareTag("Player"))
        {
            canInteract = true;
            iconInstance.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verifica se o jogador saiu do alcance do objeto
        if (other.CompareTag("Player"))
        {
            canInteract = false;
            iconInstance.SetActive(false);
        }
    }
}
