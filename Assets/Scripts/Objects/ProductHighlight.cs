using UnityEngine;

public class ProductHighlight : MonoBehaviour, IHighlight
{
    [SerializeField] Renderer rend;
    
    string type;
    Material[] standardMats;
    Material[] highlightedMats;
    
    void Start()
    {
        type = gameObject.CompareTag("ProductController") ? GetComponent<ProductController>().type : GetComponent<ProductManager>().type;
        SetMats();
    }

    public void Highlight(bool highlight)
    {
        rend.materials = highlight ? highlightedMats : standardMats;
    }

    Material[] FindHighlightedMaterial(Material[] mats)
    {
        Material[] matsTemp = CopyMaterialArray(mats);
        float offset;
        if (type == "Bear" || type == "Car") offset = 0.8f;
        else if (type == "Ball") offset = 0.3f;
        else if (type == "Laptop") offset = 0.15f;
        else offset = 0.5f;
        foreach (Material mat in matsTemp)
        {
            mat.SetColor("_Color", new Color(mat.color.r+offset, mat.color.g+offset, mat.color.b+offset, mat.color.a));
        }
        return matsTemp;
    }

    void SetMats()
    {
        Material[] mats = rend.materials;
        switch (type)
        {
            case "Ball":
                standardMats = CopyMaterialArray(mats);
                break;
                
            case "Bear":
                standardMats = CopyMaterialArray(mats);
                break;
                
            case "Boat":
                standardMats = CopyMaterialArray(mats);
                break;
                
            case "Book":
                standardMats = CopyMaterialArray(mats);
                break;
                
            case "Car":
                standardMats = CopyMaterialArray(mats);
                break;
                
            case "Laptop":
                standardMats = CopyMaterialArray(mats);
                break;
                
        }
        highlightedMats = FindHighlightedMaterial(standardMats);
    }

    Material[] CopyMaterialArray(Material[] arr)
    {
        Material[] newArr = new Material[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            newArr[i] = new Material(arr[i]);
        }
        return newArr;
    }
}
