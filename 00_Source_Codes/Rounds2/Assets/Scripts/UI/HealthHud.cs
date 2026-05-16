using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rounds2.UI
{
    public sealed class HealthHud : MonoBehaviour
    {
        private static readonly List<HealthHud> Instances = new();
        private static string currentText = string.Empty;

        [SerializeField] private Text healthText;

        private void OnEnable()
        {
            if (!Instances.Contains(this))
            {
                Instances.Add(this);
            }

            ApplyCurrentText();
        }

        private void OnDisable()
        {
            Instances.Remove(this);
        }

        public static void SetHealthText(string text)
        {
            currentText = text;
            foreach (HealthHud hud in Instances)
            {
                hud.ApplyCurrentText();
            }
        }

        private void ApplyCurrentText()
        {
            if (healthText != null)
            {
                healthText.text = currentText;
            }
        }
    }
}
