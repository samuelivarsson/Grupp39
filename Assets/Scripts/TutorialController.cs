using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
    bool basicInformation = false;
    bool changeTask = true;
    bool movement = false;
    bool pickUp = false;
    bool packedProduct = false;
    bool taped = false;
    bool delivered = false;

    GameObject player;
    List<TaskController> taskToShow = new List<TaskController>();
    PlayerPackController playerPC;

    int basicIndex = 0;
    string[] basicInformationStrings = new string[] { "Välkommen till Lagerutmaningen! Klicka på Enter för att fortsätta",
        "På pallarna i högra hörnet hittar du paket, Klicka på Enter för att fortsätta",
        "På hyllorna till vänster hittar du de olika produkterna som kan paketeras, Klicka på Enter för att fortsätta",
        "Bänken till höger är en tejpstation där paketen ska tejpas, Klicka på Enter för att fortsätta",
        "På högra sidan av skärmen hittar du alla ordrar, färgen på ordern matchar vilken lastbil den ska levereras till, Klicka på Enter för att fortsätta",
        "Uppe till vänster på skärmen ser du hur många poäng du har, Klicka på Enter för att fortsätta",
        "I mitten av skärem högst upp ser du hur många liv du har kvar, du tappar liv genom att missa en order, Klicka på Enter för att fortsätta",
        "Nu kan du grunden i spelet, Klicka på Enter för att testa spelet"};


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("PlayerController");
        playerPC = player.GetComponent<PlayerPackController>();
    }

    // Update is called once per frame
    void Update()
    {

        if(changeTask)
        {
            ChangeTasks();
            changeTask = false;
            basicInformation = true;
        }
        else if(basicInformation)
        {
            BasicInformation();
        }
        else if(movement)
        {
            PopupInfo.Instance.Popup("Använd piltagenterna för att gå med karaktären", 1000000);

            if (Input.GetAxisRaw("Horizontal") != 0 && Input.GetAxisRaw("Vertical") != 0)
            {
                movement = false;
                pickUp = true;
            }
        }
        else if(pickUp)
        {
            PopupInfo.Instance.Popup("Gå fram till en produkt eller paket och klicka på mellanslagstagenten för att plocka upp den", 1000000);

            if (CheckPickUP())
            {
                pickUp = false;
                packedProduct = true;
            }
        }
        else if(packedProduct)
        {
            PopupInfo.Instance.Popup("Hämta ett paket och placera det på bordet, hämta sedan en produkt, gå fram till paketet och klicka på X-tagenten", 1000000);

            if(CheckProductPacked())
            {
                packedProduct = false;
                taped = true;
            }
        }
        else if(taped)
        {
            PopupInfo.Instance.Popup("Flytta paketet till en tejpstation och klicka på C-tagenten för att tejpa paketet", 1000000);

            if(CheckPackagedTaped())
            {
                taped = false;
                delivered = true;
            }
        }
        else if(delivered)
        {
            PopupInfo.Instance.Popup("Nu är paketet redo att levereras, du kan nu släppa lådan bakom den rosa lastbilen för att leverera!", 1000000);
        }
    }

    private void BasicInformation()
    {
        if(Input.GetKeyUp(KeyCode.Return)) basicIndex++;
        if (basicIndex > basicInformationStrings.Length)
        {
            basicInformation = false;
            movement = true;
            return;
        }
        PopupInfo.Instance.Popup(basicInformationStrings[basicIndex], 10000);
    }

    private bool CheckPackagedTaped()
    {
        foreach (var controller in GameObject.FindGameObjectsWithTag("PackageController"))
        {
            var isTaped = controller.GetComponent<PackageController>().isTaped;

            if (isTaped)
            {
                PopupInfo.Instance.Popup("Nu måste du vänta lite eftersom det tar 5 sekunder att tejpa ett paket", 100000);
                if(!playerPC.isTaping)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CheckProductPacked()
    {

        foreach(var controller in GameObject.FindGameObjectsWithTag("PackageController"))
        {
            var productCount = controller.GetComponent<PackageController>().productCount;

            if(productCount > 0)
            {
                return true;
            }
        }

        return false;
    }


    private bool CheckPickUP()
    {
        PlayerLiftController liftController = player.GetComponent<PlayerLiftController>();

        if(liftController.liftingID != -1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ChangeTasks()
    {

        for (int i = 0; i < 4; i++)
        {
            GameObject task = GameObject.FindGameObjectWithTag("Task" + i);
            if(i == 2 || i == 3)
            {
                taskToShow.Add(task.GetComponent<TaskController>());
            }
            else
            {
                task.SetActive(false);
            }
        }

        // TODO: Hämta translatedProducts och ändra till en produkt och skriv om.
        // Ta bort timer
        
    }


    // TODO: Fixa tasksen så de är lätta och har oändligt med tid, se till att det bara finns en task
    // TODO: Lägga in en soptunna

    // TODO: Lägga till en delay innan nästa instruktion kommer upp och ge användaren beröm för det hen har gjort. "Bra jobbat, du har nu paketerat ditt första paket"
}
