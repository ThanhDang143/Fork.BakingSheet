using ThanhDV.Cathei.BakingSheet.Examples;
using ThanhDV.Cathei.BakingSheet.Implementation;
using UnityEngine;

public class Tester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Test();
        }
    }

    private async void Test()
    {
        BakingSheetController bsController = new();
        SheetContainer sheetContainer = await bsController.LoadContainerAsync<SheetContainer>("_Container");
        string desc = sheetContainer.Demos2[0].Description;
        Debug.Log(desc);
    }
}
