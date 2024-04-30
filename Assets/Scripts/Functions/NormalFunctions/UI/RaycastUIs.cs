using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

public class RaycastUIs
{
    static GameObject previousclickslot;
    static GameObject previoushoverslot;
    static StringBuilder status = new();
    static bool IsDropDownOff = true;

    public static void UIHit(ref GameObject[] hitobject)
    {
        if (IsDropDownOff)
        {
            switch (hitobject[0].tag)
            {
                case "Slot":
                    SlotOnHover(hitobject[0], hitobject[1]);
                    SlotOnClick(hitobject[0], hitobject[1]);
                    break;
            }
        }
    }

    private static void SlotOnHover(GameObject hitslot, GameObject statusshow)
    {
        if ((hitslot != previoushoverslot) && (previoushoverslot != null))
        {
            previoushoverslot.transform.GetChild(0).gameObject.SetActive(false);
        }
        previoushoverslot = hitslot;
        hitslot.transform.GetChild(0).gameObject.SetActive(true);
        if (statusshow.TryGetComponent<TMP_Text>(out _))
        {
            ShowSlotStats(hitslot, statusshow);
        }
    }

    private static void SlotOnClick(GameObject hitslot, GameObject slotdropdown)
    {
        if (Input.GetMouseButtonDown(0))
        {
            if ((hitslot != previousclickslot) && (previousclickslot != null))
            {
                previousclickslot.transform.GetChild(1).gameObject.SetActive(false);
            }
            previousclickslot = hitslot;
            hitslot.transform.GetChild(1).gameObject.SetActive(true);
            if (slotdropdown.CompareTag("DropDown"))
            {
                LoadSlotToDropdown(previousclickslot, slotdropdown);
                RefillDropdownEvents(previousclickslot, slotdropdown);
                ShowSlotDropdown(slotdropdown);
            }
        }
    }

    private static void LoadSlotToDropdown(GameObject hitslot, GameObject slotdropdown)
    {
        Slot thisslot = hitslot.GetComponent<SlotComponent>().thisSlot;

        TMP_Dropdown landscapedropdown = slotdropdown.transform
            .GetChild(0)
            .GetComponent<TMP_Dropdown>();
        if (thisslot.Landscape != null)
        {
            landscapedropdown.value = (int)thisslot.Landscape.LandscapeType + 1;
        }
        else
        {
            landscapedropdown.value = 0;
        }

        TMP_Dropdown constructiondropdown = slotdropdown.transform
            .GetChild(1)
            .GetComponent<TMP_Dropdown>();
        if (thisslot.Construction != null)
        {
            constructiondropdown.value = (int)thisslot.Construction.ConstructionType + 1;
        }
        else
        {
            constructiondropdown.value = 0;
        }

        TMP_Dropdown chessdropdown = slotdropdown.transform
            .GetChild(2)
            .GetComponent<TMP_Dropdown>();
        if (thisslot.Chess != null)
        {
            chessdropdown.value = (int)thisslot.Chess.ChessType + 1;
        }
        else
        {
            chessdropdown.value = 0;
        }
    }

    private static void ShowSlotDropdown(GameObject slotdropdown)
    {
        IsDropDownOff = false;
        if (!slotdropdown.activeSelf)
        {
            slotdropdown.SetActive(true);
            slotdropdown.transform.GetChild(0).gameObject.SetActive(true);
            slotdropdown.GetComponent<RectTransform>().position = new Vector3(
                Input.mousePosition.x,
                Input.mousePosition.y - 20f
            );
        }
        else
        {
            slotdropdown.GetComponent<RectTransform>().position = Input.mousePosition;
        }
        ShootTheMouseRay.endClickActions += () =>
        {
            slotdropdown.SetActive(false);
            IsDropDownOff = true;
        };
    }

    private static void ShowSlotStats(GameObject hitslot, GameObject statusshow)
    {
        TMP_Text showtext = statusshow.GetComponent<TMP_Text>();
        if (!statusshow.activeSelf)
        {
            statusshow.SetActive(true);
        }
        Slot thisslot = hitslot.GetComponent<SlotComponent>().thisSlot;
        status.Clear();
        if (thisslot.Landscape != null)
        {
            status.Append("地形:").Append(thisslot.Landscape.LandscapeName).Append("\n");
        }
        if (thisslot.Construction != null)
        {
            status.Append("建筑:").Append(thisslot.Construction.ConstructionName).Append("\n");
        }
        if (thisslot.Chess != null)
        {
            status.Append("单位:").Append(thisslot.Chess.ChessName);
        }
        showtext.text = status.ToString();
        ShootTheMouseRay.endHoverActions += () =>
        {
            showtext.text = "";
            statusshow.SetActive(false);
        };
    }

    private static void RefillDropdownEvents(GameObject hitslot, GameObject slotdropdown)
    {
        Slot thisslot = hitslot.GetComponent<SlotComponent>().thisSlot;

        TMP_Dropdown landscapedropdown = slotdropdown.transform
            .GetChild(0)
            .GetComponent<TMP_Dropdown>();
        landscapedropdown.onValueChanged.AddListener(
            (int index) =>
            {
                int landscapeindex = landscapedropdown.value;
                if (
                    landscapeindex != 0
                    && landscapeindex <= Enum.GetValues(typeof(LandscapeType)).Length
                )
                {
                    thisslot.InitializeOrSwapLandscape((LandscapeType)landscapeindex - 1);
                    if (thisslot.Landscape == null)
                    {
                        GameObject CreatedObject = MonoBehaviour.Instantiate(
                            EssenitalDatumLoader.GameObjectDictionary[Prefab.Landscape]
                        );
                        thisslot.Landscape.UnitGameObject = CreatedObject;
                        Landscape targetlandscape = CreatedObject
                            .GetComponent<LandscapeComponent>()
                            .thislandscape;
                        targetlandscape = thisslot.Landscape;
                        targetlandscape.PutToSlotPosition(ref hitslot);
                        CreatedObject.transform.SetParent(hitslot.transform);
                        CreatedObject.transform.localPosition = new Vector3(0,0,0.2f);
                    }
                    thisslot.Landscape.LoadLandscapeSprite();
                }
                else if (landscapeindex == 0)
                {
                    thisslot.Landscape = null;
                }
                landscapedropdown.onValueChanged.RemoveAllListeners();
            }
        );

        TMP_Dropdown constructiondropdown = slotdropdown.transform
            .GetChild(1)
            .GetComponent<TMP_Dropdown>();
        constructiondropdown.onValueChanged.AddListener(
            (int index) =>
            {
                int constructionindex = constructiondropdown.value;
                if (
                    constructionindex != 0
                    && constructionindex <= Enum.GetValues(typeof(ConstructionType)).Length
                )
                {
                    thisslot.InitializeOrSwapConstruction((ConstructionType)constructionindex - 1);
                    if (thisslot.Construction == null)
                    {
                        GameObject CreatedObject = MonoBehaviour.Instantiate(
                            EssenitalDatumLoader.GameObjectDictionary[Prefab.Construction]
                        );
                        thisslot.Construction.UnitGameObject = CreatedObject;
                        Construction targetconstruction = CreatedObject
                            .GetComponent<ConstructionComponent>()
                            .thisconstruction;
                        targetconstruction = thisslot.Construction;
                        targetconstruction.PutToSlotPosition(ref hitslot);
                        CreatedObject.transform.SetParent(hitslot.transform);
                        CreatedObject.transform.localPosition = new Vector3(0,0,0.4f);
                    }
                    thisslot.Construction.LoadConstructionSprite();
                }
                else
                {
                    thisslot.Construction = null;
                }
                constructiondropdown.onValueChanged.RemoveAllListeners();
            }
        );

        TMP_Dropdown chessdropdown = slotdropdown.transform
            .GetChild(2)
            .GetComponent<TMP_Dropdown>();
        chessdropdown.onValueChanged.AddListener(
            (int index) =>
            {
                int chessindex = chessdropdown.value;
                if (chessindex != 0 && chessindex <= Enum.GetValues(typeof(ChessType)).Length)
                {
                    thisslot.InitializeOrSwapChess((ChessType)chessindex - 1);
                    if (thisslot.Chess == null)
                    {
                        GameObject CreatedObject = MonoBehaviour.Instantiate(
                            EssenitalDatumLoader.GameObjectDictionary[Prefab.Chess]
                        );
                        thisslot.Chess.UnitGameObject = CreatedObject;
                        Chess targetchess = CreatedObject.GetComponent<ChessComponent>().thischess;
                        targetchess = thisslot.Chess;
                        targetchess.PutToSlotPosition(ref hitslot);
                        CreatedObject.transform.SetParent(hitslot.transform);
                        CreatedObject.transform.localPosition = new Vector3(0,0,0.6f);
                    }
                    thisslot.Chess.LoadChessSprite();
                }
                else
                {
                    thisslot.Chess = null;
                }
                chessdropdown.onValueChanged.RemoveAllListeners();
            }
        );
    }
}
