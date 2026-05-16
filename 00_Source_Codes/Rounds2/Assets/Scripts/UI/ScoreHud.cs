using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rounds2.UI
{
    public sealed class ScoreHud : MonoBehaviour
    {
        private static readonly List<ScoreHud> Instances = new();
        private static string currentText = string.Empty;

        [SerializeField] private Text scoreText;

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

        public static void SetScoreText(string text)
        {
            currentText = text;
            foreach (ScoreHud hud in Instances)
            {
                hud.ApplyCurrentText();
            }
        }

        private void ApplyCurrentText()
        {
            if (scoreText != null)
            {
                scoreText.text = currentText;
            }
        }
    }
}
