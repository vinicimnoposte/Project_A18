using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MissionSystem : MonoBehaviour
{
    public List<GameObject> missionObjects;
    public KeyCode interactKey = KeyCode.E;
    public int missionsCompleted = 0;
    public int totalMissions = 3;

    public TextMeshProUGUI missionsCompletedText;

    private bool[] collectedObjects;
    [SerializeField] private List<int> missionOrder;

    private void Start()
    {
        collectedObjects = new bool[missionObjects.Count];
        GenerateMissionOrder();
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            CheckMissionCompletion();
            
        }
    }

    private void GenerateMissionOrder()
    {
        missionOrder = new List<int>();

        for (int i = 0; i < missionObjects.Count; i++)
        {
            missionOrder.Add(i);
        }

        // Embaralhar a ordem dos itens de missão
        for (int i = 0; i < missionOrder.Count; i++)
        {
            int randomIndex = Random.Range(i, missionOrder.Count);
            int temp = missionOrder[i];
            missionOrder[i] = missionOrder[randomIndex];
            missionOrder[randomIndex] = temp;
        }
    }

    private void CheckMissionCompletion()
    {
        for (int i = 0; i < missionObjects.Count; i++)
        {
            int missionIndex = missionOrder[i];
            if (!collectedObjects[missionIndex] && IsPlayerCloseTo(missionObjects[missionIndex]))
            {
                CollectObject(missionIndex);
                break;
            }
        }

        if (missionsCompleted == totalMissions)
        {
            CompleteAllMissions();
        }
    }

    private bool IsPlayerCloseTo(GameObject missionObject)
    {
        float distance = Vector3.Distance(transform.position, missionObject.transform.position);
        return distance <= 2f; // Define a distância mínima para interagir com o objeto
    }

    private void CollectObject(int index)
    {
        collectedObjects[index] = true;
        missionsCompleted++;
        missionObjects[index].SetActive(false);
        Debug.Log("Object collected. Mission accomplished!: " + missionsCompleted + " of " + totalMissions);

        // Atualiza o texto das missões completadas no Canvas
        missionsCompletedText.text = "Missions Completed: " + missionsCompleted + " of " + totalMissions;
    }

    private void CompleteAllMissions()
    {
        // Aqui você pode adicionar a lógica para concluir todas as missões
        Debug.Log("Parabéns! Todas as missões foram concluídas.");
    }
}
