using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Rounds2.Tests.EditMode
{
    public sealed class PlayerAimIndicatorPrefabTests
    {
        [Test]
        public void PlayerPrefabHasForwardAimIndicator()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            Transform indicator = playerPrefab.transform.Find("AimIndicator");

            Assert.IsNotNull(indicator);
            Assert.Greater(indicator.localPosition.x, 0f);

            SpriteRenderer renderer = indicator.GetComponent<SpriteRenderer>();
            Assert.IsNotNull(renderer);
            Assert.Greater(indicator.localScale.x, indicator.localScale.y);
            Assert.Greater(renderer.sortingOrder, playerPrefab.GetComponent<SpriteRenderer>().sortingOrder);
        }

        [Test]
        public void AimIndicatorStartsAtMuzzle()
        {
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            Transform muzzle = playerPrefab.transform.Find("Muzzle");
            Transform indicator = playerPrefab.transform.Find("AimIndicator");

            Assert.IsNotNull(muzzle);
            Assert.IsNotNull(indicator);

            float indicatorStartX = indicator.localPosition.x - indicator.localScale.x * 0.5f;
            Assert.AreEqual(muzzle.localPosition.x, indicatorStartX, 0.001f);
        }
    }
}
