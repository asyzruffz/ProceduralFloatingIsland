using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class PlayerInventory : MonoBehaviour
{
    public GameObject inventory;
    public GameObject characterSystem;
    public GameObject craftSystem;

    private Inventory craftSystemInventory;
    private CraftSystem cS;
    private Inventory mainInventory;
    private Inventory characterSystemInventory;
    private Tooltip toolTip;

    private InputManager inputManagerDatabase;
    private PlayerStats stats;

    int normalSize = 3;

    void Start () {
        if (inputManagerDatabase == null)
            inputManagerDatabase = (InputManager)Resources.Load ("InputManager");
        if (inventory == null) {
            inventory = GameObject.FindGameObjectWithTag ("MainInventory");
            inventory.SetActive (false);
        }

        stats = GetComponent<PlayerStats> ();

        if (craftSystem != null)
            cS = craftSystem.GetComponent<CraftSystem> ();

        if (GameObject.FindGameObjectWithTag ("Tooltip") != null)
            toolTip = GameObject.FindGameObjectWithTag ("Tooltip").GetComponent<Tooltip> ();
        if (inventory != null)
            mainInventory = inventory.GetComponent<Inventory> ();
        if (characterSystem != null)
            characterSystemInventory = characterSystem.GetComponent<Inventory> ();
        if (craftSystem != null)
            craftSystemInventory = craftSystem.GetComponent<Inventory> ();
    }

    void Update () {
        if (Input.GetKeyDown (inputManagerDatabase.CharacterSystemKeyCode)) {
            if (!characterSystem.activeSelf) {
                characterSystemInventory.openInventory ();
            } else {
                if (toolTip != null)
                    toolTip.deactivateTooltip ();
                characterSystemInventory.closeInventory ();
            }
        }

        if (Input.GetButtonDown ("Inventory")) {
            if (!inventory.activeSelf) {
                mainInventory.openInventory ();
            } else {
                if (toolTip != null)
                    toolTip.deactivateTooltip ();
                mainInventory.closeInventory ();
            }
        }

        if (Input.GetKeyDown (inputManagerDatabase.CraftSystemKeyCode)) {
            if (!craftSystem.activeSelf)
                craftSystemInventory.openInventory ();
            else {
                if (cS != null)
                    cS.backToInventory ();
                if (toolTip != null)
                    toolTip.deactivateTooltip ();
                craftSystemInventory.closeInventory ();
            }
        }
    }

    public void OnEnable()
    {
        Inventory.ItemEquip += OnBackpack;
        Inventory.UnEquipItem += UnEquipBackpack;

        Inventory.ItemEquip += OnGearItem;
        Inventory.ItemConsumed += OnConsumeItem;
        Inventory.UnEquipItem += OnUnEquipItem;

        Inventory.ItemEquip += EquipWeapon;
        Inventory.UnEquipItem += UnEquipWeapon;

        Inventory.InventoryOpen += OnOpenInventory;
        Inventory.AllInventoriesClosed += OnCloseAllInventories;
    }

    public void OnDisable()
    {
        Inventory.ItemEquip -= OnBackpack;
        Inventory.UnEquipItem -= UnEquipBackpack;

        Inventory.ItemEquip -= OnGearItem;
        Inventory.ItemConsumed -= OnConsumeItem;
        Inventory.UnEquipItem -= OnUnEquipItem;

        Inventory.UnEquipItem -= UnEquipWeapon;
        Inventory.ItemEquip -= EquipWeapon;

        Inventory.InventoryOpen -= OnOpenInventory;
        Inventory.AllInventoriesClosed -= OnCloseAllInventories;
    }

    void EquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {
            //add the weapon if you unequip the weapon
        }
    }

    void UnEquipWeapon(Item item)
    {
        if (item.itemType == ItemType.Weapon)
        {
            //delete the weapon if you unequip the weapon
        }
    }

    void OnBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
        {
            for (int i = 0; i < item.itemAttributes.Count; i++)
            {
                if (mainInventory == null)
                    mainInventory = inventory.GetComponent<Inventory>();
                mainInventory.sortItems();
                if (item.itemAttributes[i].attributeName == "Slots")
                    changeInventorySize(item.itemAttributes[i].attributeValue);
            }
        }
    }

    void UnEquipBackpack(Item item)
    {
        if (item.itemType == ItemType.Backpack)
            changeInventorySize(normalSize);
    }

    void changeInventorySize(int size)
    {
        dropTheRestItems(size);

        if (mainInventory == null)
            mainInventory = inventory.GetComponent<Inventory>();
        if (size == 3)
        {
            mainInventory.width = 3;
            mainInventory.height = 1;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        if (size == 6)
        {
            mainInventory.width = 3;
            mainInventory.height = 2;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        else if (size == 12)
        {
            mainInventory.width = 4;
            mainInventory.height = 3;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        else if (size == 16)
        {
            mainInventory.width = 4;
            mainInventory.height = 4;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
        else if (size == 24)
        {
            mainInventory.width = 6;
            mainInventory.height = 4;
            mainInventory.updateSlotAmount();
            mainInventory.adjustInventorySize();
        }
    }

    void dropTheRestItems(int size)
    {
        if (size < mainInventory.ItemsInInventory.Count)
        {
            for (int i = size; i < mainInventory.ItemsInInventory.Count; i++)
            {
                GameObject dropItem = (GameObject)Instantiate(mainInventory.ItemsInInventory[i].itemModel);
                dropItem.AddComponent<PickUpItem>();
                dropItem.GetComponent<PickUpItem>().item = mainInventory.ItemsInInventory[i];
                dropItem.transform.localPosition = GameObject.FindGameObjectWithTag("Player").transform.localPosition;
            }
        }
    }
    
    void OnConsumeItem(Item item)
    {
        stats.IncreaseValueFrom (item);
    }

    void OnGearItem(Item item)
    {
        stats.IncreaseMaxValueFrom (item);
    }

    void OnUnEquipItem(Item item)
    {
        stats.DecreaseMaxValueFrom (item);
    }

    void OnOpenInventory () {
        if (GameController.Instance) {
            GameController.Instance.TogglePause (true);
        }
    }

    void OnCloseAllInventories () {
        if (GameController.Instance) {
            GameController.Instance.TogglePause (false);
        }
    }

    public void CloseAllPlayerPanels () {
        mainInventory.closeInventory ();
        //characterSystemInventory.closeInventory ();
        //craftSystemInventory.closeInventory ();
    }
}
