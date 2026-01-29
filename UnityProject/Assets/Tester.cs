using ThanhDV.Cathei.BakingSheet.Examples;
using ThanhDV.Cathei.BakingSheet;
using UnityEngine;

public class Tester : MonoBehaviour
{
    void Start()
    {

    }

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
