using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;
using ValheimHopper.Logic.Helper;
using Random = UnityEngine.Random;

namespace ValheimHopper.Logic {
    [DefaultExecutionOrder(5)]
    public class Hopper : NetworkPiece, IPushTarget, IPullTarget {
        public Piece Piece { get; private set; }
        private Container container;
        private ContainerTarget containerTarget;

        public HopperPriority PushPriority { get; } = HopperPriority.HopperPush;
        public HopperPriority PullPriority { get; } = HopperPriority.HopperPull;
        public bool IsPickup { get; } = false;

        [SerializeField] private Vector3 inPos = new Vector3(0, 0.25f * 1.5f, 0);
        [SerializeField] private Vector3 outPos = new Vector3(0, -0.25f * 1.5f, 0);
        [SerializeField] private Vector3 inSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 outSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 nearSize = new Vector3(1.5f, 1.5f, 1.5f);

        private List<IPushTarget> pushTo = new List<IPushTarget>();
        internal List<IPullTarget> pullFrom = new List<IPullTarget>();
        private List<Hopper> nearHoppers = new List<Hopper>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private int transferFrame;
        private int objectSearchFrame;
        private int frameOffset;

        private int pushCounter;
        private int pullCounter;

        public ItemFilter filter;

        public ZBool FilterItemsOption { get; private set; }
        public ZBool DropItemsOption { get; private set; }
        public ZBool PickupItemsOption { get; private set; }
        public ZBool LeaveLastItemOption { get; private set; }

        protected override void Awake() {
            base.Awake();

            Piece = GetComponent<Piece>();
            container = GetComponent<Container>();
            containerTarget = GetComponent<ContainerTarget>();

            FilterItemsOption = new ZBool("hopper_filter_items", false, zNetView);
            DropItemsOption = new ZBool("hopper_drop_items", false, zNetView);
            PickupItemsOption = new ZBool("hopper_pickup_items", true, zNetView);
            LeaveLastItemOption = new ZBool("hopper_leave_last_item", false, zNetView);

            transferFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * TransferInterval);
            objectSearchFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * ObjectSearchInterval);
            frameOffset = Mathf.Abs(GetInstanceID() % transferFrame);
        }

        private void Start() {
            filter = new ItemFilter(zNetView, container.GetInventory());
            container.GetInventory().m_onChanged += () => {
                if (IsValid() && FilterItemsOption.Get()) {
                    filter.Save();
                }
            };
        }

        public void PasteData(Hopper copy) {
            FilterItemsOption.Set(copy.FilterItemsOption.Get());
            DropItemsOption.Set(copy.DropItemsOption.Get());
            PickupItemsOption.Set(copy.PickupItemsOption.Get());
            LeaveLastItemOption.Set(copy.LeaveLastItemOption.Get());
            filter.Copy(copy.filter);
        }

        public void ResetValues() {
            FilterItemsOption.Reset();
            DropItemsOption.Reset();
            PickupItemsOption.Reset();
            LeaveLastItemOption.Reset();
            filter.Clear();
        }

        private void FixedUpdate() {
            if (!IsValid() || !zNetView.IsOwner()) {
                return;
            }

            int frame = HopperHelper.GetFixedFrameCount();
            int globalFrame = (frame + frameOffset) / transferFrame;

            if ((frame + frameOffset) % transferFrame == 0) {
                if (globalFrame % 2 == 0) {
                    PullItems();
                }

                if (globalFrame % 2 == 1) {
                    PushItems();
                }
            }

            if ((frame + frameOffset + 1) % objectSearchFrame == 0) {
                FindIO();
            }
        }

        private void PullItems() {
            if (pullFrom.Count == 0) {
                return;
            }

            IPullTarget from = pullFrom[pullCounter % pullFrom.Count];
            pullCounter++;

            if (!from.IsValid()) {
                return;
            }

            if (!PickupItemsOption.Get() && from.IsPickup) {
                return;
            }

            foreach (ItemDrop.ItemData item in from.GetItems()) {
                if (!FindFreeSlot(item, out Vector2i pos)) {
                    continue;
                }

                from.RemoveItem(item, container.GetInventory(), pos, zNetView.m_zdo.m_uid);
                return;
            }
        }

        private void PushItems() {
            if (pushTo.Count == 0) {
                if (DropItemsOption.Get()) {
                    DropItem();
                }

                return;
            }

            IPushTarget to = pushTo[pushCounter % pushTo.Count];
            pushCounter++;

            if (!to.IsValid()) {
                return;
            }

            ItemDrop.ItemData item = container.GetInventory().FindLastItem(i => to.CanAddItem(i) && CanPushItem(i));

            if (item != null) {
                to.AddItem(item, container.GetInventory(), zNetView.m_zdo.m_uid);
            }
        }

        private void DropItem() {
            ItemDrop.ItemData firstItem = container.GetInventory().FindLastItem(CanPushItem);

            if (firstItem != null) {
                container.GetInventory().RemoveOneItem(firstItem);
                float angle = Random.Range(0f, (float)(2f * Math.PI));
                Vector3 randomPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.2f;
                Vector3 visualOffset = ItemHelper.GetVisualItemOffset(firstItem.m_dropPrefab.name);
                Vector3 pos = transform.TransformPoint(outPos) + visualOffset + new Vector3(randomPos.x, 0, randomPos.z);
                ItemDrop.DropItem(firstItem, 1, pos, firstItem.m_dropPrefab.transform.rotation);
            }
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return FindFreeSlot(item, out _);
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            FindFreeSlot(item, out Vector2i pos);
            container.AddItemToChest(item, source, pos, sender, 1);
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            return containerTarget.GetItems();
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            containerTarget.RemoveItem(item, destination, destinationPos, sender);
        }

        private bool FindFreeSlot(ItemDrop.ItemData itemToAdd, out Vector2i pos) {
            pos = new Vector2i(0, 0);

            if (!container.GetInventory().CanAddItem(itemToAdd, 1)) {
                return false;
            }

            int itemHash = itemToAdd.m_dropPrefab.name.GetStableHashCode();

            for (int y = 0; y < container.m_height; y++) {
                for (int x = 0; x < container.m_width; x++) {
                    ItemDrop.ItemData item = container.GetInventory().GetItemAt(x, y);
                    bool canAdd = item == null ||
                                  item.m_stack + 1 <= item.m_shared.m_maxStackSize && item.m_shared.m_name == itemToAdd.m_shared.m_name;

                    if (!canAdd) {
                        continue;
                    }

                    if (FilterItemsOption.Get()) {
                        int filterHash = filter.GetItemHash(x, y);
                        bool isFiltered = filterHash == 0 || filterHash == itemHash;

                        if (isFiltered) {
                            pos = new Vector2i(x, y);
                            return true;
                        }
                    } else {
                        pos = new Vector2i(x, y);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool InRange(Vector3 position) {
            return true;
        }

        private bool CanPushItem(ItemDrop.ItemData item) {
            return (!LeaveLastItemOption.Get() || container.GetInventory().CountItems(item.m_shared.m_name) > 1) &&
                   !nearHoppers.Any(hopper => hopper != this && hopper.pullFrom.Contains(this) && hopper.CanAddItem(item));
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;
            pullFrom = HopperHelper.FindTargets<IPullTarget>(transform.TransformPoint(inPos), inSize, rotation, i => i.PullPriority, this);
            pushTo = HopperHelper.FindTargets<IPushTarget>(transform.TransformPoint(outPos), outSize, rotation, i => i.PushPriority, this);
            nearHoppers = HopperHelper.FindTargets<Hopper>(transform.position, nearSize, rotation, i => i.PullPriority, this);
            pullFrom.RemoveAll(pull => pushTo.Exists(push => push.NetworkHashCode() == pull.NetworkHashCode()));
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.TransformPoint(inPos), inSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.TransformPoint(outPos), outSize);
            Gizmos.color =  new Color(0.0f, 1f, 0.0f, 0.5f);
            Gizmos.DrawWireCube(transform.position, nearSize);

            Gizmos.color = Color.cyan;
            foreach (Transform child in transform) {
                if (child.CompareTag("snappoint")) {
                    Gizmos.DrawSphere(child.position, .05f);
                }
            }
        }
    }
}
