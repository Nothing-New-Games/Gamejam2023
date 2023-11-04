using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using WatchDog;
using UnityEngine.UI;
using TMPro;
using Sirenix.Serialization;
using System;

public class SlotHandler : MonoBehaviour, IPointerClickHandler
{
    [TabGroup("Customization")]
    public Vector3 MenuOffset = new(75, -120, 0);
    [TabGroup("Customization")]
    public Sprite DefaultSprite;

    public int EmptyItemAlpha = 100;


    private RectTransform RightClickRectTransform;

    [Serialize, ShowInInspector]
    private Loot ContainedLoot;
    [MinValue(1), TabGroup("Debug")]
    public int MaxQuantity = 999;

    public Loot GetLoot {  get { return ContainedLoot; } }
    public int GetQuantity { get { return ContainedLoot.Quantity; ; } }

    private Button[] buttons;
    private Image UIImage;
    private TextMeshProUGUI QuantityDisplay;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && ContainedLoot != null)
        {
            RightClickRectTransform.transform.position = Input.mousePosition + MenuOffset;

            #region Arrange Right Click Menu Buttons
            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.Equip))
                buttons.First(button => button.name == "Equip").gameObject.SetActive(true);
            else buttons.First(button => button.name == "Equip").gameObject.SetActive(false);

            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.Consume))
                buttons.First(button => button.name == "Consume").gameObject.SetActive(true);
            else buttons.First(button => button.name == "Consume").gameObject.SetActive(false);

            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.CobineWith))
                buttons.First(button => button.name == "CobineWith").gameObject.SetActive(true);
            else buttons.First(button => button.name == "CobineWith").gameObject.SetActive(false);

            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.Drop))
                buttons.First(button => button.name == "Drop").gameObject.SetActive(true);
            else buttons.First(button => button.name == "Drop").gameObject.SetActive(false);

            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.Examine))
                buttons.First(button => button.name == "Examine").gameObject.SetActive(true);
            else buttons.First(button => button.name == "Examine").gameObject.SetActive(false);

            if (ContainedLoot.Item.AvailableRightClickOptions.Any(option => option == ItemMenuOptions.Destroy))
                buttons.First(button => button.name == "Destroy").gameObject.SetActive(true);
            else buttons.First(button => button.name == "Destroy").gameObject.SetActive(false);
            #endregion

            RightClickRectTransform.gameObject.SetActive(true);
            GetComponent<Button>().Select();
        }
    }

    private void Start()
    {
        UIImage = GetComponent<Image>();
        image = GetComponent<Image>();
        QuantityDisplay = GetComponentInChildren<TextMeshProUGUI>();
        QuantityDisplay.gameObject.SetActive(false);
        UpdateSprite();

        RightClickRectTransform = RightClickMenuHandler.GetInstance.GetComponent<RectTransform>();

        buttons = RightClickRectTransform.GetComponentsInChildren<Button>();
    }


    void Update()
    {
        if (IsMouseOutsideElement(RightClickRectTransform) && RightClickRectTransform.gameObject.activeInHierarchy)
        {
            RightClickRectTransform.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            RightClickMenuHandler.RightClickMenuDisappearedCallback.Invoke(new());
        }

        if (ContainedLoot == null)
            UIImage.color = new(UIImage.color.r, UIImage.color.g, UIImage.color.b, (float)EmptyItemAlpha / 255);

        if (ContainedLoot != null && ContainedLoot.Quantity > 1)
        {
            QuantityDisplay.gameObject.SetActive(true);
            QuantityDisplay.text = ContainedLoot.Quantity.ToString();
        }
        else QuantityDisplay.gameObject.SetActive(false);

        
    }

    private bool IsMouseOutsideElement(RectTransform element)
    {
        // Check if the mouse position is outside the bounds of the specified UI element
        Vector2 mousePosition = Input.mousePosition;
        Vector3[] corners = new Vector3[4];
        element.GetWorldCorners(corners);

        if (mousePosition.x < corners[0].x || mousePosition.x > corners[2].x ||
            mousePosition.y < corners[0].y || mousePosition.y > corners[2].y)
        {
            return true;
        }
        return false;
    }

    Image image;
    private void UpdateSprite()
    {
        if (ContainedLoot == null)
        {
            image.sprite = DefaultSprite;
            return;
        }

        image.sprite = ContainedLoot.Item.ContainerSprite;
    }

    /// <summary>
    /// Attempts to add the item to the container.
    /// </summary>
    /// <param name="newLoot">Item to be added to the slot</param>
    /// <returns>
    /// <see langword="false"/> if it cannot be added. 
    /// <para><see langword="true"/> if the add was successful.</para>
    /// </returns>
    public bool AddToSlot(Loot newLoot)
    {
        if (newLoot == null)
        {
            ContainedLoot = newLoot;
            UpdateSprite();
            return true;
        }
        else if (ContainedLoot == newLoot)
        {
            if (newLoot.Quantity + ContainedLoot.Quantity <= MaxQuantity)
                ContainedLoot.Quantity += newLoot.Quantity;
            else return false;

            return true;
        }

        return false;
    }
    public int RemoveFromSlot(int quantityToRemove)
    {
        if (ContainedLoot != null)
        {
            if (ContainedLoot.Quantity >= quantityToRemove)
            {
                ContainedLoot.Quantity -= quantityToRemove;
                if (ContainedLoot.Quantity == 0)
                    ContainedLoot = null;

                UpdateSprite();
                return ContainedLoot.Quantity;
            }
        }

        return quantityToRemove;
    }
}
