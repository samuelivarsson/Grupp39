using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PackageController : MonoBehaviour, LiftablePackage
{
    public bool isLifted {get; set;} = false;
    public bool isTaped {get; set;} = false;
    public bool canTape { get; set; } = false;
    public int productCount {get; set;} = 0;
    
    public Transform pic1 {get; set;}
    public Transform pic2 {get; set;}
    public Transform pic3 {get; set;}
    public bool bpic1 {get; set;} = false;
    public bool bpic2 {get; set;} = false;
    public bool bpic3 {get; set;} = false;

    [SerializeField] public Image timebar;

    PhotonView PV;
    Rigidbody rb;
    TapeTimer tapeTimer;

    const int scoreMultiplier = 100;

    public static Vector3 tileOffset = new Vector3(1.5f, 0f, 1.5f);
    public static Vector3 tapeOffset = new Vector3(1.5f, 0.28f, 1.5f);
    public static Vector3 tableOffset = new Vector3(1.5f, 0.3f, 1.5f);

    void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        pic1 = gameObject.transform.GetChild(0);
        pic2 = gameObject.transform.GetChild(1);
        pic3 = gameObject.transform.GetChild(2);
    }

    // Start is called before the first frame update
    void Start()
    {
        timebar.enabled = false;
    }

    public bool OrderDelivery(GameObject latestTile)
    {
        if (!isTaped)
        {
            print("Package isn't taped!");
            PopupInfo.Instance.Popup("Paketet måste tejpas innan den kan levereras", 7);
            return false;
        }
        if (!latestTile.CompareTag("DropZone"))
        {
            print("This is not a drop zone!");
            return false;
        }
        string num = latestTile.name.Substring(latestTile.name.Length - 1);
        GameObject taskObj = GameObject.FindGameObjectWithTag("Task"+num);
        if (taskObj == null)
        {
            // Task was already destroyed and no new task has spawned yet
            print("No task was found with name Task"+num);
            return false;
        }
        TaskTimer taskTimer = taskObj.GetComponent<TaskTimer>();
        if (taskTimer.timeLeft <= 0)
        {
            // Task hasn't been destroyed yet, but the time was up
            print("Time is up!");
            return false;
        }

        TaskController taskController = taskObj.GetComponent<TaskController>();
        PhotonView taskPV = taskObj.GetComponent<PhotonView>();
        if (HasRequiredProducts(taskController))
        {
            ScoreController.Instance.IncrementScore(taskController.productAmount * scoreMultiplier);
            foreach (ProductController productController in GetComponentsInChildren<ProductController>())
            {
                PhotonView productPV = productController.GetComponent<PhotonView>();
                if (productPV.IsMine)
                {
                    // Destroy product
                    PhotonNetwork.Destroy(productController.gameObject);
                }
                else productPV.RPC("DestroyProduct", RpcTarget.OthersBuffered);
            }
            if (PV.IsMine)
            {
                // Destroy package
                PhotonNetwork.Destroy(gameObject);
            }
            else PV.RPC("DestroyPackage", RpcTarget.OthersBuffered);
            if (PhotonNetwork.IsMasterClient)
            {
                // Destroy task
                PhotonNetwork.Destroy(taskObj);
                TaskManager.Instance.GenerateNewTask(taskController.taskNr);
            }
            else taskPV.RPC("DestroyTask", RpcTarget.OthersBuffered);
            print("The package contained the required products!");
            return true;
        }
        else
        {
            print("The package did not contain the required products!");
            PopupInfo.Instance.Popup("Paketet innehöll inte alla produkter", 7);
            return false;
        }
    }

    [PunRPC]
    void DestroyPackage()
    {
        if (PV.IsMine) PhotonNetwork.Destroy(gameObject);
    }

    bool HasRequiredProducts(TaskController task)
    {
        string[] packageProducts = GetProductTypes();
        string[] requiredProducts = task.requiredProducts;

        if (packageProducts.Length <= 0) return false;

        return packageProducts.OrderBy(x=>x).SequenceEqual(requiredProducts.OrderBy(x=>x));
    }

    string[] GetProductTypes()
    {
        ProductController[] productControllers = GetComponentsInChildren<ProductController>();
        string[] packageProducts = new string[productControllers.Length];
        
        for (int i = 0; i < productControllers.Length; i++)
        {
            packageProducts[i] = productControllers[i].type;
        }

        return packageProducts;
    }
}
