using UnityEngine;
using Photon.Pun;

public class ObjectTriggerCheck : MonoBehaviour
{
    [SerializeField] Material standardTile;
    [SerializeField] Material standardOutsideTile;
    [SerializeField] Material standardDropZone;
    [SerializeField] Material standardTable;
    [SerializeField] Material standardTapeTable;
    [SerializeField] Material standardTrash;

    PhotonView PV;

    void Awake()
    {
        PV = GetComponentInParent<PhotonView>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) return;
        
        if (PlayerLiftController.DropableTile(other.gameObject)) PlayerManager.myPlayerLiftController.latestTile = other.gameObject;
        Highlight(other.gameObject, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (PV.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) return;
        
        Highlight(other.gameObject, false);
    }

    void Highlight(GameObject obj, bool highlight)
    {
        Material mat;
        int i;
        switch (obj.tag)
        {
            case "PlaceableTile":
                mat = standardTile;
                i = 1;
                break;

            case "PlaceableOutsideTile":
                mat = standardOutsideTile;
                i = 1;
                break;

            case "DropZone":
                mat = standardDropZone;
                i = 0;
                break;

            case "Table":
                mat = standardTable;
                i = 1;
                break;

            case "TapeTable":
                mat = standardTapeTable;
                i = 0;
                break;

            case "Trash":
                mat = standardTrash;
                i = 0;
                break;

            default:
                return;
        }
        if (highlight) mat = FindHighlightedMaterial(obj, mat);
        SetMaterial(obj, mat, i);
    }

    Material FindHighlightedMaterial(GameObject obj, Material material)
    {
        Material matTemp = new Material(material);
        switch (obj.tag)
        {
            case "Table":
                matTemp.SetColor("_Color", matTemp.color.gamma);
                break;

            // DropZone, Trash or PlaceableOutsideTile
            case "DropZone":
            case "Trash":
            case "PlaceableOutsideTile":
                matTemp.SetColor("_Color", new Color(matTemp.color.r+0.5f, matTemp.color.g+0.5f, matTemp.color.b+0.5f, matTemp.color.a));
                break;

            default:
                matTemp.SetColor("_Color", matTemp.color.linear);
                break;
        }
        return matTemp;
    }

    void SetMaterial(GameObject obj, Material mat, int i)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        Material[] mats = renderer.materials;
        mats[i] = mat;
        renderer.materials = mats;
    }
}
