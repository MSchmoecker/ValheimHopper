using Jotunn;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ValheimHopper {
    public class HopperUI : MonoBehaviour {
        public static HopperUI Instance { get; private set; }
        public static bool IsOpen { get; private set; }
        private static readonly Color WhiteShade = new Color(219f / 255f, 219f / 255f, 219f / 255f);

        // Disable Field XYZ is never assigned to, and will always have its default value XX
#pragma warning disable 0649
        [SerializeField] private Text title;
        [SerializeField] private Toggle filterHopper;
        [SerializeField] private Toggle dropItems;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button pasteButton;
        [SerializeField] private Button resetButton;
#pragma warning restore 0649

        private static GameObject uiRoot;
        private Hopper target;
        private Hopper copy;

        private void Awake() {
            Instance = this;

            filterHopper.onValueChanged.AddListener(i => target.FilterItems.Set(i));
            dropItems.onValueChanged.AddListener(i => target.DropItems.Set(i));

            copyButton.onClick.AddListener(() => { copy = target; });
            pasteButton.onClick.AddListener(() => {
                target.PasteData(copy);
                UpdateText();
            });
            resetButton.onClick.AddListener(() => {
                target.ResetValues();
                UpdateText();
            });
        }

        public static void Init() {
            GameObject prefab = Plugin.AssetBundle.LoadAsset<GameObject>("HopperUI");
            HopperUI ui = Instantiate(prefab, GUIManager.CustomGUIFront.transform, false).GetComponent<HopperUI>();
            uiRoot = ui.transform.GetChild(0).gameObject;

            ApplyAllComponents(uiRoot);
            GUIManager.Instance.ApplyTextStyle(ui.title, GUIManager.Instance.AveriaSerifBold, GUIManager.Instance.ValheimOrange, 20);
            ApplyLocalization();

            uiRoot.SetActive(false);
            uiRoot.FixReferences(true);
        }

        private void Update() {
            if (IsOpen) {
                if (Plugin.hopperEditKey.Value.IsDown() || Input.GetKeyDown(KeyCode.Escape) || ZInput.GetButtonDown("Use") || ZInput.GetButtonDown("Inventory")) {
                    target = null;
                    SetGUIState(false);
                }
            } else {
                if (Plugin.hopperEditKey.Value.IsDown() && Player.m_localPlayer) {
                    TryOpenUI();
                }
            }

            if (IsOpen && target) {
                UpdateText();
            }
        }

        private static void SetGUIState(bool active) {
            if (IsOpen && active) {
                return;
            }

            IsOpen = active;
            uiRoot.SetActive(active);
            GUIManager.BlockInput(active);
        }

        private void TryOpenUI() {
            GameObject hoverPiece = Player.m_localPlayer.GetHoverObject();

            if (!hoverPiece) {
                return;
            }

            Hopper hopper = hoverPiece.GetComponentInParent<Hopper>();

            if (hopper) {
                OpenUI(hopper);
            }
        }

        public void OpenUI(Hopper hopper) {
            target = hopper;
            SetGUIState(true);
            UpdateText();
        }

        private void UpdateText() {
            title.text = Localization.instance.Localize(target.piece.m_name);
            filterHopper.SetIsOnWithoutNotify(target.FilterItems.Get());
            dropItems.SetIsOnWithoutNotify(target.DropItems.Get());
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
