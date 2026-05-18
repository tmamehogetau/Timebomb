using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Rounds2.UI
{
    public sealed class DraftHud : MonoBehaviour
    {
        private const int CardChoiceCount = 3;
        private static readonly List<DraftHud> Instances = new();
        private static bool creatingRuntimeHud;

        public static event Action<int> ChoiceClicked;

        private readonly Text[] cardTexts = new Text[CardChoiceCount];
        private readonly GameObject[] cardPanels = new GameObject[CardChoiceCount];

        [SerializeField] private GameObject draftRoot;
        [SerializeField] private Text draftText;

        private void Awake()
        {
            if (!Instances.Contains(this))
            {
                Instances.Add(this);
            }
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
        }

        public static void SetDraftText(string text)
        {
            EnsureRuntimeInstance();

            foreach (DraftHud hud in Instances)
            {
                hud.ApplyText(text);
            }
        }

        private static void EnsureRuntimeInstance()
        {
            if (Instances.Count > 0 || creatingRuntimeHud || !Application.isPlaying || Application.isBatchMode)
            {
                return;
            }

            creatingRuntimeHud = true;
            GameObject canvasObject = new("DraftHudRuntimeCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            EnsureEventSystem();

            DraftHud hud = canvasObject.AddComponent<DraftHud>();
            hud.draftRoot = CreateDraftRoot(canvasObject.transform);
            hud.draftText = CreateHeaderText(hud.draftRoot.transform);
            Transform row = CreateCardRow(hud.draftRoot.transform);
            for (int i = 0; i < CardChoiceCount; i++)
            {
                hud.cardPanels[i] = CreateCardPanel(row, i + 1, out Text cardText);
                hud.cardTexts[i] = cardText;
            }

            hud.ApplyText(string.Empty);
            creatingRuntimeHud = false;
        }

        private static GameObject CreateDraftRoot(Transform parent)
        {
            GameObject rootObject = new("DraftCardRoot");
            rootObject.transform.SetParent(parent, false);
            RectTransform rootRect = rootObject.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = new Vector2(0f, 126f);
            rootRect.sizeDelta = new Vector2(980f, 158f);

            VerticalLayoutGroup layout = rootObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.spacing = 8f;
            return rootObject;
        }

        private static Text CreateHeaderText(Transform parent)
        {
            GameObject textObject = new("DraftText");
            textObject.transform.SetParent(parent, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(980f, 26f);

            Text text = textObject.AddComponent<Text>();
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.94f, 0.55f, 1f);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            return text;
        }

        private static Transform CreateCardRow(Transform parent)
        {
            GameObject rowObject = new("DraftCardRow");
            rowObject.transform.SetParent(parent, false);
            RectTransform rowRect = rowObject.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(980f, 122f);

            HorizontalLayoutGroup layout = rowObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.spacing = 10f;
            return rowObject.transform;
        }

        private static GameObject CreateCardPanel(Transform parent, int choiceNumber, out Text cardText)
        {
            GameObject panelObject = new($"DraftCardPanel{choiceNumber}");
            panelObject.transform.SetParent(parent, false);
            RectTransform panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(300f, 118f);

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.07f, 0.08f, 0.1f, 0.88f);

            Button button = panelObject.AddComponent<Button>();
            button.targetGraphic = panelImage;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 0.96f, 0.65f, 1f);
            colors.pressedColor = new Color(0.82f, 0.78f, 0.46f, 1f);
            colors.selectedColor = new Color(1f, 0.96f, 0.65f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            button.colors = colors;
            int choiceIndex = choiceNumber - 1;
            button.onClick.AddListener(() => ChoiceClicked?.Invoke(choiceIndex));

            Outline outline = panelObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.94f, 0.55f, 0.55f);
            outline.effectDistance = new Vector2(1f, -1f);

            GameObject textObject = new("DraftCardText");
            textObject.transform.SetParent(panelObject.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);

            cardText = textObject.AddComponent<Text>();
            cardText.alignment = TextAnchor.UpperLeft;
            cardText.color = new Color(0.96f, 0.98f, 1f, 1f);
            cardText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cardText.fontSize = 15;
            cardText.fontStyle = FontStyle.Bold;
            cardText.horizontalOverflow = HorizontalWrapMode.Wrap;
            cardText.verticalOverflow = VerticalWrapMode.Truncate;
            cardText.lineSpacing = 0.9f;
            return panelObject;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new("DraftHudEventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private void ApplyText(string text)
        {
            if (draftText == null)
            {
                return;
            }

            bool hasText = !string.IsNullOrEmpty(text);
            if (draftRoot != null)
            {
                draftRoot.SetActive(hasText);
            }

            draftText.enabled = hasText;
            if (!HasCardPanels())
            {
                draftText.text = text;
                return;
            }

            string[] lines = hasText ? text.Split('\n') : System.Array.Empty<string>();
            draftText.text = lines.Length > 0 ? lines[0] : string.Empty;
            for (int i = 0; i < CardChoiceCount; i++)
            {
                string cardText = lines.Length > i + 1 ? lines[i + 1] : string.Empty;
                ApplyCardText(i, cardText, hasText);
            }
        }

        private bool HasCardPanels()
        {
            return cardTexts[0] != null && cardPanels[0] != null;
        }

        private void ApplyCardText(int index, string text, bool visible)
        {
            bool hasText = visible && !string.IsNullOrEmpty(text);
            cardPanels[index].SetActive(hasText);
            cardTexts[index].text = text.Replace(" | ", "\n");
            cardTexts[index].enabled = hasText;
        }
    }
}
