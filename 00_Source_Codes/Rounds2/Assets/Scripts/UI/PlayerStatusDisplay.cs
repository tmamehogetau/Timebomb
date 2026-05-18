using FishNet.Object;
using Rounds2.Combat;
using Rounds2.Config;
using Rounds2.Player;
using UnityEngine;

namespace Rounds2.UI
{
    [DisallowMultipleComponent]
    public sealed class PlayerStatusDisplay : NetworkBehaviour
    {
        private WeaponController weapon;
        private PlayerShieldController shield;
        private Health health;
        private SpriteRenderer ammoReloadCooldownFill;
        private SpriteRenderer[] ammoSlots;
        private MeshFilter shieldCooldownPie;
        private PlayerStatusGaugeState lastState = new(-1, -1, -1f, -1f);

        private const float AmmoStackWidth = 0.12f;
        private const float AmmoStackHeight = 0.58f;
        private const float AmmoSlotGap = 0.018f;
        private const float ShieldPieRadius = 0.16f;
        private const int ShieldPieSegments = 28;
        private static readonly Vector3 GaugeRootLocalPosition = new(0f, -0.24f, 0f);
        private static readonly Vector3 AmmoStackLocalPosition = new(-0.22f, 0f, 0f);
        private static readonly Vector3 ShieldPieLocalPosition = new(0.22f, 0f, 0f);
        private static Sprite gaugeSprite;

        private void Awake()
        {
            CacheComponents();
            SetStatusGauge(new PlayerStatusGaugeState(CombatTuning.MagazineSize, CombatTuning.MagazineSize, 0f, 1f));
        }

        private void Update()
        {
            if (!IsServerInitialized)
            {
                return;
            }

            PlayerStatusGaugeState state = BuildStatusGaugeState();
            if (IsSameState(state, lastState))
            {
                return;
            }

            lastState = state;
            SetStatusGaugeObserversRpc(state.ActiveAmmoSlots, state.MagazineSize, state.ReloadFill, state.ShieldCooldownFill);
        }

        private void CacheComponents()
        {
            weapon ??= GetComponent<WeaponController>();
            shield ??= GetComponent<PlayerShieldController>();
            health ??= GetComponent<Health>();
            EnsureGauges();
        }

        private PlayerStatusGaugeState BuildStatusGaugeState()
        {
            if (health != null && health.IsDead)
            {
                return new PlayerStatusGaugeState(0, weapon != null ? weapon.MagazineSize : CombatTuning.MagazineSize, 0f, 0f);
            }

            return PlayerStatusGaugeState.Create(
                weapon != null ? weapon.CurrentAmmo : CombatTuning.MagazineSize,
                weapon != null ? weapon.MagazineSize : CombatTuning.MagazineSize,
                weapon != null && weapon.IsReloading,
                weapon != null ? weapon.ReloadRemaining : 0f,
                weapon != null ? weapon.ReloadSeconds : CombatTuning.ReloadSeconds,
                shield != null ? shield.CooldownRemaining : 0f,
                CombatTuning.ShieldCooldownSeconds);
        }

        [ObserversRpc(BufferLast = true, RunLocally = true)]
        private void SetStatusGaugeObserversRpc(int activeAmmoSlots, int magazineSize, float reloadFill, float shieldCooldownFill)
        {
            SetStatusGauge(new PlayerStatusGaugeState(activeAmmoSlots, magazineSize, reloadFill, shieldCooldownFill));
        }

        private void SetStatusGauge(PlayerStatusGaugeState state)
        {
            EnsureGauges(state.MagazineSize);
            SetAmmoSlots(state.ActiveAmmoSlots, state.MagazineSize);
            SetVerticalFill(ammoReloadCooldownFill, state.ReloadFill, AmmoStackHeight);
            SetPieFill(shieldCooldownPie, state.ShieldCooldownFill);
        }

        private void EnsureGauges(int magazineSize = CombatTuning.MagazineSize)
        {
            ammoReloadCooldownFill ??= transform.Find("StatusGaugeRoot/AmmoStackRoot/AmmoReloadCooldownFill")?.GetComponent<SpriteRenderer>();
            shieldCooldownPie ??= transform.Find("StatusGaugeRoot/ShieldCooldownPie")?.GetComponent<MeshFilter>();

            int slotCount = Mathf.Max(1, magazineSize);
            if (ammoSlots != null && ammoSlots.Length == slotCount && ammoReloadCooldownFill != null && shieldCooldownPie != null)
            {
                return;
            }

            GameObject root = EnsureChild(transform, "StatusGaugeRoot", GaugeRootLocalPosition);
            Transform stackRoot = EnsureChild(root.transform, "AmmoStackRoot", AmmoStackLocalPosition).transform;
            CreateAmmoStack(stackRoot, slotCount);
            shieldCooldownPie = CreatePie(root.transform);
        }

        private static GameObject EnsureChild(Transform parent, string name, Vector3 localPosition)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                existing.localPosition = localPosition;
                return existing.gameObject;
            }

            GameObject child = new(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            return child;
        }

        private void CreateAmmoStack(Transform parent, int slotCount)
        {
            DisableExistingAmmoSlots(parent);
            SpriteRenderer back = CreateSpriteRenderer(parent, "AmmoReloadCooldownFrame", Vector3.zero, new Color(0.12f, 0.14f, 0.16f, 0.78f), sortingOrder: 4);
            back.transform.localScale = new Vector3(AmmoStackWidth + 0.06f, AmmoStackHeight + 0.06f, 1f);

            ammoReloadCooldownFill = CreateSpriteRenderer(parent, "AmmoReloadCooldownFill", Vector3.zero, new Color(0.2f, 0.95f, 0.45f, 0.8f), sortingOrder: 5);
            ammoReloadCooldownFill.transform.localScale = new Vector3(AmmoStackWidth + 0.035f, 0f, 1f);

            ammoSlots = new SpriteRenderer[slotCount];
            float slotHeight = (AmmoStackHeight - AmmoSlotGap * (ammoSlots.Length - 1)) / ammoSlots.Length;
            for (int i = 0; i < ammoSlots.Length; i++)
            {
                float y = -AmmoStackHeight * 0.5f + slotHeight * 0.5f + i * (slotHeight + AmmoSlotGap);
                ammoSlots[i] = CreateSpriteRenderer(parent, $"AmmoSlot{i + 1}", new Vector3(0f, y, -0.02f), new Color(1f, 0.86f, 0.18f, 1f), sortingOrder: 6);
                ammoSlots[i].transform.localScale = new Vector3(AmmoStackWidth, slotHeight, 1f);
            }
        }

        private static void DisableExistingAmmoSlots(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (!child.name.StartsWith("AmmoSlot", System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (child.GetComponent<SpriteRenderer>() is SpriteRenderer renderer)
                {
                    renderer.enabled = false;
                }
            }
        }

        private static MeshFilter CreatePie(Transform parent)
        {
            GameObject pieObject = EnsureChild(parent, "ShieldCooldownPie", ShieldPieLocalPosition);
            MeshFilter filter = pieObject.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = pieObject.AddComponent<MeshFilter>();
            }

            MeshRenderer renderer = pieObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = pieObject.AddComponent<MeshRenderer>();
            }

            renderer.material = new Material(Shader.Find("Sprites/Default"))
            {
                color = new Color(0.35f, 0.95f, 1f, 0.92f)
            };
            renderer.sortingOrder = 6;
            return filter;
        }

        private static SpriteRenderer CreateSpriteRenderer(Transform parent, string name, Vector3 localPosition, Color color, int sortingOrder)
        {
            GameObject child = EnsureChild(parent, name, localPosition);
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = child.AddComponent<SpriteRenderer>();
            }

            renderer.sprite = GetGaugeSprite();
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void SetAmmoSlots(int activeSlots, int magazineSize)
        {
            if (ammoSlots == null)
            {
                return;
            }

            int clampedActive = Mathf.Clamp(activeSlots, 0, ammoSlots.Length);
            for (int i = 0; i < ammoSlots.Length; i++)
            {
                bool visible = i < magazineSize && i < clampedActive;
                if (ammoSlots[i] != null)
                {
                    ammoSlots[i].enabled = visible;
                }
            }
        }

        private static void SetVerticalFill(SpriteRenderer renderer, float fill, float height)
        {
            if (renderer == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(fill);
            float scaledHeight = height * clamped;
            renderer.transform.localScale = new Vector3(renderer.transform.localScale.x, scaledHeight, 1f);
            renderer.transform.localPosition = new Vector3(0f, (scaledHeight - height) * 0.5f, renderer.transform.localPosition.z);
        }

        private static void SetPieFill(MeshFilter filter, float fill)
        {
            if (filter == null)
            {
                return;
            }

            filter.mesh = BuildPieMesh(Mathf.Clamp01(fill));
        }

        private static Mesh BuildPieMesh(float fill)
        {
            int segments = Mathf.Max(1, Mathf.CeilToInt(ShieldPieSegments * fill));
            Vector3[] vertices = new Vector3[segments + 2];
            int[] triangles = new int[segments * 3];
            vertices[0] = Vector3.zero;

            for (int i = 0; i <= segments; i++)
            {
                float t = segments == 0 ? 0f : (float)i / segments;
                float angle = Mathf.Lerp(90f, 90f - 360f * fill, t) * Mathf.Deg2Rad;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * ShieldPieRadius, Mathf.Sin(angle) * ShieldPieRadius, 0f);
            }

            for (int i = 0; i < segments; i++)
            {
                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + 2;
            }

            Mesh mesh = new();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Sprite GetGaugeSprite()
        {
            if (gaugeSprite != null)
            {
                return gaugeSprite;
            }

            Texture2D texture = new(4, 4, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.white;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            gaugeSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 4f);
            return gaugeSprite;
        }

        private static bool IsSameState(PlayerStatusGaugeState left, PlayerStatusGaugeState right)
        {
            return left.ActiveAmmoSlots == right.ActiveAmmoSlots
                && left.MagazineSize == right.MagazineSize
                && Mathf.Abs(left.ReloadFill - right.ReloadFill) < 0.001f
                && Mathf.Abs(left.ShieldCooldownFill - right.ShieldCooldownFill) < 0.001f;
        }
    }
}
