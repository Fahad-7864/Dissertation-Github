using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // ----- Section: Variables for Skill Display  -----
    public Image skillIcon;
    public GameObject tooltipPrefab; 
    private GameObject tooltipObject; 
    public Skill skill; 
    public GameObject skillsPanel; 

    public TextMeshProUGUI cooldownText;
    public void Start()
    {
        if (skill != null)
        {
            DisplaySkill(skill);
        }
    }

    public void DisplaySkill(Skill skill)
    {
        this.skill = skill;
        skillIcon.sprite = skill.skillSprite;

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Create the tooltip GameObject from the prefab
        tooltipObject = Instantiate(tooltipPrefab, transform.position, Quaternion.identity, transform);

        // Get the RectTransform component of the skillsPanel
        RectTransform panelRect = skillsPanel.GetComponent<RectTransform>();

        // Adjust the position of the tooltip GameObject to be at the specified position relative to the panel
        Vector3 position = skillsPanel.transform.position;
        position.y += panelRect.rect.height / 2-400; 
        tooltipObject.transform.position = position;

        // Get the Text components
        Text[] texts = tooltipObject.GetComponentsInChildren<Text>();
        Text skillNameText = texts[0]; 
        Text skillDescriptionText = texts[1]; 
        // Set the skill name and description
        skillNameText.text = skill.skillName;
        skillDescriptionText.text = skill.description;
    }



    public void OnPointerExit(PointerEventData eventData)
    {
        // Destroy the tooltip GameObject
        if (tooltipObject != null)
        {
            Destroy(tooltipObject);
        }
    }



}

