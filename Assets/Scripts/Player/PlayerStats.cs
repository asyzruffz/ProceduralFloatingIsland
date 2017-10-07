using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {

    public GameObject HPMANACanvas;

    Text hpText;
    Text manaText;
    Image hpImage;
    Image manaImage;

    public float maxHealth = 100;
    public float maxMana = 100;
    public float maxDamage = 0;
    public float maxArmor = 0;

    public float currentHealth = 60;
    public float currentMana = 100;
    public float currentDamage = 0;
    public float currentArmor = 0;

    void Start () {
        //if (HPMANACanvas != null)
        //{
        //    hpText = HPMANACanvas.transform.GetChild(1).GetChild(0).GetComponent<Text>();

        //    manaText = HPMANACanvas.transform.GetChild(2).GetChild(0).GetComponent<Text>();

        //    hpImage = HPMANACanvas.transform.GetChild(1).GetComponent<Image>();
        //    manaImage = HPMANACanvas.transform.GetChild(1).GetComponent<Image>();

        //    UpdateHPBar();
        //    UpdateManaBar();
        //}
    }
    
    void UpdateHPBar() {
    //    hpText.text = (currentHealth + "/" + maxHealth);
    //    float fillAmount = currentHealth / maxHealth;
    //    hpImage.fillAmount = fillAmount;
    }

    void UpdateManaBar() {
    //    manaText.text = (currentMana + "/" + maxMana);
    //    float fillAmount = currentMana / maxMana;
    //    manaImage.fillAmount = fillAmount;
    }

    public void IncreaseValueFrom (Item item) {
        for (int i = 0; i < item.itemAttributes.Count; i++) {
            if (item.itemAttributes[i].attributeName == "Health") {
                currentHealth += item.itemAttributes[i].attributeValue;
                currentHealth = Mathf.Clamp (currentHealth, 0, maxHealth);
            }
            if (item.itemAttributes[i].attributeName == "Mana") {
                currentMana += item.itemAttributes[i].attributeValue;
                currentMana = Mathf.Clamp (currentMana, 0, maxMana);
            }
            if (item.itemAttributes[i].attributeName == "Armor") {
                currentArmor += item.itemAttributes[i].attributeValue;
                currentArmor = Mathf.Clamp (currentArmor, 0, maxArmor);
            }
            if (item.itemAttributes[i].attributeName == "Damage") {
                currentDamage += item.itemAttributes[i].attributeValue;
                currentDamage = Mathf.Clamp (currentDamage, 0, maxDamage);
            }
        }

        UpdateManaBar();
        UpdateHPBar();
    }

    public void IncreaseMaxValueFrom (Item item) {
        for (int i = 0; i < item.itemAttributes.Count; i++) {
            if (item.itemAttributes[i].attributeName == "Health")
                maxHealth += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Mana")
                maxMana += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                maxArmor += item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
                maxDamage += item.itemAttributes[i].attributeValue;
        }

        UpdateManaBar();
        UpdateHPBar();
    }

    public void DecreaseMaxValueFrom (Item item) {
        for (int i = 0; i < item.itemAttributes.Count; i++) {
            if (item.itemAttributes[i].attributeName == "Health")
                maxHealth -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Mana")
                maxMana -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Armor")
                maxArmor -= item.itemAttributes[i].attributeValue;
            if (item.itemAttributes[i].attributeName == "Damage")
                maxDamage -= item.itemAttributes[i].attributeValue;
        }

        UpdateManaBar ();
        UpdateHPBar ();
    }
}
