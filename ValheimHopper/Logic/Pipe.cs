using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ValheimHopper.Logic.Helper;

namespace ValheimHopper.Logic {
    public class Pipe : NetworkPiece, IPushTarget, IPullTarget {
        public HopperPriority PushPriority { get; } = HopperPriority.PipePush;
        public HopperPriority PullPriority { get; } = HopperPriority.PipePull;
        public bool IsPickup { get; } = false;

        [SerializeField] private Vector3 outPos = new Vector3(0, 0, -1f);
        [SerializeField] private Vector3 outSize = new Vector3(0.5f, 0.5f, 0.5f);

        private Container container;
        private ContainerTarget containerTarget;
        private List<IPushTarget> pushTo = new List<IPushTarget>();
        private List<Hopper> nearHoppers = new List<Hopper>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private int transferFrame;
        private int objectSearchFrame;
        private int frameOffset;

        private int pushCounter;

        protected override void Awake() {
            base.Awake();

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

        private bool CanPushItem(ItemDrop.ItemData item) {
            return !nearHoppers.Any(hopper => hopper.pullFrom.Contains(this) && hopper.CanAddItem(item));
        }

        private void PushItems() {
            if (pushTo.Count == 0) {
                return;
            }

            IPushTarget to = pushTo[pushCounter % pushTo.Count];
            pushCounter++;

            if (!to.IsValid()) {
                return;
            }

            ItemDrop.ItemData item = container.GetInventory().FindFirstItem(i => to.CanAddItem(i) && CanPushItem(i));

            if (item != null) {
                to.AddItem(item, container.GetInventory(), zNetView.m_zdo.m_uid);
            }
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;
            pushTo = HopperHelper.FindTargets<IPushTarget>(transform.TransformPoint(outPos), outSize, rotation, i => i.PushPriority, this);
            nearHoppers = HopperHelper.FindTargets<Hopper>(transform.position, Vector3.one * 1.5f, rotation, i => i.PullPriority, this);
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
