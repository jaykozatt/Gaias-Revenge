using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dispenser : MonoBehaviour
{
    [SerializeField] Pieces _pieces;

    Transform deploymentSocket;
    [SerializeField] Transform nextSocket;

    GameObject deployed;
    GameObject next;

    private void Awake() {
        deploymentSocket = transform;
        nextSocket = transform.GetChild(0);

        GameManager.Instance.OnGameStart += Initialise;
        GameManager.Instance.OnGameEnded += DisposeOfPieces;
    }

    private void Start() {
    }

    public void Initialise()
    {
        InstantiateNext();
        DeployNext();
    }

    public void DeployNext() {
        deployed = next;
        if (deployed != null)
        {
            deployed.transform.parent = deploymentSocket;
            deployed.transform.position = deploymentSocket.position;
            deployed.transform.localScale = Vector3.one;
            deployed.GetComponent<Placeable>().enabled = true;
        } 

        InstantiateNext();
    }

    public void InstantiateNext() {
        next = Instantiate(_pieces.GetRandom(), nextSocket.position, Quaternion.identity, nextSocket);
        next.transform.localScale = .33f * Vector3.one;
        Placeable placeable = next.GetComponent<Placeable>();
        placeable.enabled = false;
        placeable.AssignDeployer(this);
    }

    private void DisposeOfPieces()
    {
        Destroy(deployed.gameObject);
        Destroy(next.gameObject);
    }
}
