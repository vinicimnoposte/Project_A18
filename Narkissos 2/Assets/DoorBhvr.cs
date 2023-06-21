using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBhvr : MonoBehaviour
{
    public Animator animator;
    public bool open;
    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(Input.GetKeyDown(KeyCode.E) && open == false)
            {
                StartCoroutine(OpenDoor(gameObject, 0.3f));
                open = !open;
            }
            if(Input.GetKeyDown(KeyCode.E) && open == true)
            {
                StartCoroutine(OpenDoor(gameObject, 0.3f));
                open = !open;
            }
            
        }
    }

    public IEnumerator OpenDoor(GameObject doorToOpen, float time)
    {
        animator.SetBool("Open", open);
        yield return new WaitForSeconds(time);
    }
}
