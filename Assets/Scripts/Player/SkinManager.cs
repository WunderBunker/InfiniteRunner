using UnityEngine;

//GESTION DU SKIN D'UN BATEAU
public class SkinManager : MonoBehaviour
{
    [SerializeField] MaterialList _boatPlanks;
    [SerializeField] MaterialList _boatSail;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //On applique les cosmétiqeus courrament enregistrés
        PlayerSave vSave = SaveManager.GetPlayerSave();
        ChangePlankMaterial(vSave.Skin.PlankId);
        ChangeSailMaterial(vSave.Skin.SailId);
    }

    public void ChangePlankMaterial(string pId)
    {
        foreach (IdentifiedMaterial lMat in _boatPlanks.MatArray)
            if (lMat.Id == pId)
            {
                var vMats = transform.Find("Body").GetComponent<MeshRenderer>().materials;
                vMats[2] = new(lMat.Material);
                transform.Find("Body").GetComponent<MeshRenderer>().materials = vMats;
                break;
            }
    }

    public void ChangeSailMaterial(string pId)
    {
        foreach (IdentifiedMaterial lMat in _boatSail.MatArray)
            if (lMat.Id == pId)
            {
                var vMats = transform.Find("Sail").GetComponent<MeshRenderer>().materials;
                vMats[1] = new(lMat.Material);
                transform.Find("Sail").GetComponent<MeshRenderer>().materials = vMats;
                break;
            }
    }

}
