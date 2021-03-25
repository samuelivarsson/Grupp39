using Photon.Pun;
using UnityEngine;

public class ProductController : MonoBehaviour
{
    //List<GameObject> droppedDeliveries = new List<GameObject>();
    //int gatheredPoints = 0;
    bool spaceKeyWasPressed;

    Rigidbody rb;
    PhotonView PV;
    [SerializeField] ScoreController scoreController;
    [SerializeField] Transform player;
    GameObject[] players;
    Transform hand;
    bool isLifted;

    bool canPickUp;
    Transform latestPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        hand = player.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckLiftAndDrop();
    }

    private void CheckLiftAndDrop () 
    {
        if (gameObject.transform.parent.tag != "Package") 
        {
            if (Input.GetKeyDown(KeyCode.Space) && isLifted)
            {
                Drop();
            }
            if (Input.GetKeyDown(KeyCode.Space) && !isLifted && canPickUp)
            {
                Lift();
            }
        }
    }

    public void Lift()
    {
        gameObject.transform.parent = latestPlayer;
        gameObject.transform.localPosition = hand.transform.localPosition;
        isLifted = true;
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        playerController.setIsLifting(true);
        PV.RPC("OnLift", RpcTarget.OthersBuffered, latestPlayer.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void OnLift(int viewID)
    {
        GameObject player = PhotonView.Find(viewID).gameObject;
        gameObject.transform.parent = player.transform;
        gameObject.transform.localPosition = hand.transform.localPosition;
    }

    public void Drop()
    {
        gameObject.transform.parent = null;
        isLifted = false;
        canPickUp = false;
        PlayerController playerController = latestPlayer.GetComponent<PlayerController>();
        playerController.setIsLifting(false);
        PV.RPC("OnDrop", RpcTarget.OthersBuffered);
    }

    [PunRPC]
    void OnDrop()
    {
        gameObject.transform.parent = null;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("DropDown"))
        {
            //droppedDeliveries.Add(gameObject);
            //gatheredPoints++;
            Destroy(gameObject);
            scoreController.IncrementScore(1);
            //Debug.Log("Points:"+ gatheredPoints);
        }
    }
    
    public void setCanPickUp(bool _canPickUp)
    {
        canPickUp = _canPickUp;
    }

    public void setLatestPlayer(Transform player)
    {
        latestPlayer = player;
    }

    public void setIsLifted(bool _isLifted)
    {
        isLifted = _isLifted;
    }

    public bool getIsLifted()
    {
        return isLifted;
    }
}
