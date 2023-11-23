using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerHotkeys : MonoBehaviour
{
    public static PlayerHotkeys Instance;
    public static bool isClickingATarget;
    public GameObject broom;
    public GameObject lumos;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Chat.Instance.isWritting) return;

        if (InputSystemAgent.Key["Esc"]?.wasPressedThisFrame??false) Menu.Instance.togglePanel("ConfigPanel");
        if (InputSystemAgent.GetKeyDown("F1"))
            GameObject.Find("Canvas/TopMenu/Config").GetComponent<ConfigMenu>().dev.SetActive(true);
        if (InputSystemAgent.GetKeyDown("F2"))
        {
            var now = DateTime.Now.ToString("_d-MMM-yyyy-HH-mm-ss-f");
            ScreenCapture.CaptureScreenshot("./Screenshots/" + string.Format("Screenshot{0}.png", now));

            Chat.Instance.LocalMsg("<color=\"#e8bf00\">[Sistema]</color> Captura guardada como Screenshot" + now +
                                   ".png");
        }

        // Display/hide the UI
        if (InputSystemAgent.GetKeyDown("Z")) Menu.Instance.gameObject.SetActive(!Menu.Instance.gameObject.GetActive());

        if (InputSystemAgent.GetKeyDown("B")) Menu.Instance.togglePanel("BagPanel");
        if (InputSystemAgent.GetKeyDown("C")) Menu.Instance.togglePanel("CharacterPanel");
        if (InputSystemAgent.GetKeyDown("T")) Chat.Instance.input2.ActivateInputField();

        if (InputSystemAgent.GetKeyDown("Q")) toggleLight();

        if (InputSystemAgent.GetKeyDown("E")) toggleBroomStick();


        if (Player.Instance.target)
        {
            // unselect target
            if (InputSystemAgent.GetKeyUp("LMaus") && !EventSystem.current.IsPointerOverGameObject())
            {
                if (isClickingATarget)
                    isClickingATarget = false;
                else
                    Player.Instance.target = null;
            }

            if (InputSystemAgent.GetKeyDown("1"))
                PlayerCombat.Instance.spellCast(0);
            else if (InputSystemAgent.GetKeyDown("2"))
                PlayerCombat.Instance.spellCast(1);
            else if (InputSystemAgent.GetKeyDown("3")) PlayerCombat.Instance.spellCast(2);
        }
    }

    public void toggleLight()
    {
        lumos.SetActive(!lumos.activeSelf);
    }

    public void toggleBroomStick()
    {
        if (!broom.activeSelf)
        {
            if (!PlayerCombat.Instance.castingSpell && !Player.Instance.isInCombat &&
                !PlayerPanel.Instance.castingPanel.isCasting) StartCoroutine(nameof(Broom));
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("Broomstick", false);
            gameObject.GetComponent<ThirdPersonUserControl>().enabled = true;
            gameObject.GetComponent<ThirdPersonCharacter>().enabled = true;
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            gameObject.GetComponent<BroomstickControl>().enabled = false;
            broom.SetActive(!broom.activeSelf);
            transform.localEulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
    }

    private IEnumerator Broom()
    {
        PlayerPanel.Instance.castingPanel.Cast("Escoba voladora", 1);
        yield return new WaitForSeconds(1);
        gameObject.GetComponent<Animator>().SetBool("Broomstick", true);
        gameObject.GetComponent<ThirdPersonUserControl>().enabled = false;
        gameObject.GetComponent<ThirdPersonCharacter>().enabled = false;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
        gameObject.GetComponent<BroomstickControl>().enabled = true;
        broom.SetActive(!broom.activeSelf);
    }
}