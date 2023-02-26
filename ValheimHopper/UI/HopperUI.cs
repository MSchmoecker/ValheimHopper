using Jotunn;
using Jotunn.GUI;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;
using ValheimHopper.Logic;

namespace ValheimHopper.UI {
    public class HopperUI : MonoBehaviour {
        public static HopperUI Instance { get; private set; }
        public static bool IsOpen { get; private set; }
        private static readonly Color WhiteShade = new Color(219f / 255f, 219f / 255f, 219f / 255f);

        // Disable Field XYZ is never assigned to, and will always have its default value XX
#pragma warning disable 0649
        [SerializeField] private Text title;
        [SerializeField] private Toggle filterHopper;
        [SerializeField] private Toggle dropItems;
        [SerializeField] private Toggle pickupItems;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button pasteButton;
        [SerializeField] private Button resetButton;
#pragma warning restore 0649

        private static GameObject uiRoot;
        private Hopper target;
        private Hopper copy;

        private void Awake() {
            Instance = this;

            dropItems.onValueChanged.AddListener(i => target.DropItemsOption.Set(i));
            pickupItems.onValueChanged.AddListener(i => target.PickupItemsOption.Set(i));

            filterHopper.onValueChanged.AddListener(active => {
                target.FilterItemsOption.Set(active);

                if (active) {
                    target.filter.Save();
                } else {
                    target.filter.Clear();
                }
            });

            copyButton.onClick.AddListener(() => { copy = target; });
            pasteButton.onClick.AddListener(() => { target.PasteData(copy); });
            resetButton.onClick.AddListener(() => { target.ResetValues(); });
        }

        public static void Init() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("HopperUI");
            HopperUI ui = Instantiate(prefab, GUIManager.CustomGUIFront.transform, false).GetComponent<HopperUI>();
            uiRoot = ui.transform.GetChild(0).gameObject;

            ApplyAllComponents(uiRoot);
            GUIManager.Instance.ApplyTextStyle(ui.title, GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange, 20);
            ApplyLocalization();

            uiRoot.AddComponent<DragWindowCntrl>();
            uiRoot.SetActive(false);
            uiRoot.FixReferences(true);
        }

        private void LateUpdate() {
            if (!Player.m_localPlayer) {
                target = null;
                SetGUIState(false);
                return;
            }

            InventoryGui gui = InventoryGui.instance;

            if (!gui || !gui.IsContainerOpen() || !gui.m_currentContainer) {
                target = null;
                SetGUIState(false);
                return;
            }

            if (gui.m_currentContainer.TryGetComponent(out Hopper hopper)) {
                target = hopper;
                SetGUIState(true);
                UpdateText();
            } else {
                target = null;
                SetGUIState(false);
            }
        }

        private static void SetGUIState(bool active) {
            if (IsOpen == active) {
                return;
            }

            IsOpen = active;
            uiRoot.SetActive(active);
        }

        private void UpdateText() {
            title.text = Localization.instance.Localize(target.Piece.m_name);
            filterHopper.SetIsOnWithoutNotify(target.FilterItemsOption.Get());
            dropItems.SetIsOnWithoutNotify(target.DropItemsOption.Get());
            pickupItems.SetIsOnWithoutNotify(target.PickupItemsOption.Get());
        }

        private static void ApplyAllComponents(GameObject root) {
            foreach (Text text in root.GetComponentsInChildren<Text>()) {
                GUIManager.Instance.ApplyTextStyle(text, GUIManager.Instance.AveriaSerif, WhiteShade, 16, false);
            }

            foreach (InputField inputField in root.GetComponentsInChildren<InputField>()) {
                GUIManager.Instance.ApplyInputFieldStyle(inputField, 16);
            }

            foreach (Toggle toggle in root.GetComponentsInChildren<Toggle>()) {
                GUIManager.Instance.ApplyToogleStyle(toggle);
            }

            foreach (Button button in root.GetComponentsInChildren<Button>()) {
                GUIManager.Instance.ApplyButtonStyle(button);
            }
        }

        private static void ApplyLocalization() {
            foreach (Text text in uiRoot.GetComponentsInChildren<Text>()) {
                text.text = Localization.instance.Localize(text.text);
            }
        }
    }
}
