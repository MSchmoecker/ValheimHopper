using System.Collections.Generic;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class Pipe : MonoBehaviour, IPushTarget, IPullTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.PipePush;
        public HopperPriority PullPriority { get; } = HopperPriority.PipePull;
        public bool IsPickup { get; } = false;

        [SerializeField] private Vector3 outPos = new Vector3(0, 0, -1f);
        [SerializeField] private Vector3 outSize = new Vector3(0.5f, 0.5f, 0.5f);

        private Container container;
        private ContainerTarget containerTarget;
        private ZNetView zNetView;
        private List<IPushTarget> pushTo = new List<IPushTarget>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private int transferFrame;
        private int objectSearchFrame;
        private int frameOffset;

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            container = GetComponent<Container>();
            containerTarget = GetComponent<ContainerTarget>();

            transferFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * TransferInterval);
            objectSearchFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * ObjectSearchInterval);
            frameOffset = Mathf.Abs(GetInstanceID() % transferFrame);
        }

        private void FixedUpdate() {
            if (!IsValid() || !zNetView.IsOwner()) {
                return;
            }

            int frame = HopperHelper.GetFixedFrameCount();
            int globalFrame = (frame + frameOffset) / transferFrame;

            if ((frame + frameOffset) % transferFrame == 0) {
                if (globalFrame % 2 == 1) {
                    PushItems();
                }
            }

            if ((frame + frameOffset + 1) % objectSearchFrame == 0) {
                FindIO();
            }
        }

        public bool IsValid() {
            return this && containerTarget && containerTarget.IsValid();
        }

        public bool InRange(Vector3 position) {
            return true;
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return containerTarget.CanAddItem(item);
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            containerTarget.AddItem(item, source, sender);
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            return containerTarget.GetItems();
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            containerTarget.RemoveItem(item, destination, destinationPos, sender);
        }

        private void PushItems() {
            foreach (IPushTarget to in pushTo) {
                if (!to.IsValid()) {
                    continue;
                }

                ItemDrop.ItemData item = container.GetInventory().FindFirstItem(i => to.CanAddItem(i));

                if (item == null) {
                    continue;
                }

                to.AddItem(item, container.GetInventory(), zNetView.m_zdo.m_uid);
                return;
            }
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;
            pushTo = HopperHelper.FindTargets<IPushTarget>(transform.TransformPoint(outPos), outSize, rotation, i => (int)i.PushPriority);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.TransformPoint(outPos), outSize);

            Gizmos.color = Color.cyan;
            foreach (Transform child in transform) {
                if (child.CompareTag("snappoint")) {
                    Gizmos.DrawSphere(child.position, .05f);
                }
            }
        }
    }
}
