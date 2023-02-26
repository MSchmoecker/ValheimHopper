using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;
using ValheimHopper.Logic.Helper;
using Random = UnityEngine.Random;

namespace ValheimHopper.Logic {
    [DefaultExecutionOrder(5)]
    public class Hopper : MonoBehaviour, IPushTarget, IPullTarget {
        public Piece Piece { get; private set; }
        private ZNetView zNetView;
        private Container container;
        private ContainerTarget containerTarget;

        public HopperPriority PushPriority { get; } = HopperPriority.HopperPush;
        public HopperPriority PullPriority { get; } = HopperPriority.HopperPull;
        public bool IsPickup { get; } = false;

        [SerializeField] private Vector3 inPos = new Vector3(0, 0.25f * 1.5f, 0);
        [SerializeField] private Vector3 outPos = new Vector3(0, -0.25f * 1.5f, 0);
        [SerializeField] private Vector3 inSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 outSize = new Vector3(1f, 1f, 1f);

        private List<IPushTarget> pushTo = new List<IPushTarget>();
        private List<IPullTarget> pullFrom = new List<IPullTarget>();
        [SerializeField] private List<Hopper> nearHoppers = new List<Hopper>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private int transferFrame;
        private int objectSearchFrame;
        private int frameOffset;

        public ItemFilter filter;

        public ZBool FilterItemsOption { get; private set; }
        public ZBool DropItemsOption { get; private set; }
        public ZBool PickupItemsOption { get; private set; }

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            Piece = GetComponent<Piece>();
            container = GetComponent<Container>();
            containerTarget = GetComponent<ContainerTarget>();

            FilterItemsOption = new ZBool("hopper_filter_items", false, zNetView);
            DropItemsOption = new ZBool("hopper_drop_items", false, zNetView);
            PickupItemsOption = new ZBool("hopper_pickup_items", true, zNetView);

            transferFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * TransferInterval);
            objectSearchFrame = Mathf.RoundToInt((1f / Time.fixedDeltaTime) * ObjectSearchInterval);
            frameOffset = Mathf.Abs(GetInstanceID() % transferFrame);
        }

        private void Start() {
            if (!IsValid()) {
                return;
            }

            filter = new ItemFilter(zNetView, container.GetInventory());
            container.GetInventory().m_onChanged += () => {
                if (FilterItemsOption.Get()) {
                    filter.Save();
                }
            };
        }

        public void PasteData(Hopper copy) {
            FilterItemsOption.Set(copy.FilterItemsOption.Get());
            DropItemsOption.Set(copy.DropItemsOption.Get());
            PickupItemsOption.Set(copy.PickupItemsOption.Get());
            filter.Copy(copy.filter);
        }

        public void ResetValues() {
            FilterItemsOption.Reset();
            DropItemsOption.Reset();
            PickupItemsOption.Reset();
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

        public bool IsValid() {
            return this && containerTarget && containerTarget.IsValid();
        }

        private void PullItems() {
            foreach (IPullTarget from in pullFrom) {
                if (!from.IsValid()) {
                    continue;
                }

                if (!PickupItemsOption.Get() && from.IsPickup) {
                    continue;
                }

                foreach (ItemDrop.ItemData item in from.GetItems()) {
                    if (!FindFreeSlot(item, out Vector2i pos)) {
                        continue;
                    }

                    from.RemoveItem(item, container.GetInventory(), pos, zNetView.m_zdo.m_uid);
                    return;
                }
            }
        }

        private void PushItems() {
            if (pushTo.Count == 0 && DropItemsOption.Get()) {
                DropItem();
                return;
            }

            foreach (IPushTarget to in pushTo) {
                if (!to.IsValid()) {
                    continue;
                }

                ItemDrop.ItemData item = container.GetInventory().FindFirstItem(i => to.CanAddItem(i));

                if (item == null || !CanPushItem(item)) {
                    continue;
                }

                to.AddItem(item, container.GetInventory(), zNetView.m_zdo.m_uid);
                return;
            }
        }

        private void DropItem() {
            ItemDrop.ItemData firstItem = container.GetInventory().FindFirstItem(CanPushItem);

            if (firstItem != null) {
                container.GetInventory().RemoveOneItem(firstItem);
                float angle = Random.Range(0f, (float)(2f * Math.PI));
                Vector3 randomPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.2f;
                Vector3 visualOffset = ItemHelper.GetVisualItemOffset(firstItem.m_dropPrefab.name);
                Vector3 pos = transform.TransformPoint(outPos) + visualOffset + new Vector3(randomPos.x, 0, randomPos.z);
                GameObject drop = Instantiate(firstItem.m_dropPrefab, pos, firstItem.m_dropPrefab.transform.rotation);
                drop.GetComponent<ItemDrop>().m_itemData.m_stack = 1;
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
            return !nearHoppers.Any(hopper => hopper != this && hopper.pullFrom.Contains(this) && hopper.CanAddItem(item));
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;
            pullFrom = HopperHelper.FindTargets<IPullTarget>(transform.TransformPoint(inPos), inSize, rotation, i => (int)i.PullPriority);
            pushTo = HopperHelper.FindTargets<IPushTarget>(transform.TransformPoint(outPos), outSize, rotation, i => (int)i.PushPriority);
            nearHoppers = HopperHelper.FindTargets<Hopper>(transform.position, Vector3.one * 1.5f, rotation, i => (int)i.PullPriority);
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.TransformPoint(inPos), inSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.TransformPoint(outPos), outSize);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 1.5f);

            Gizmos.color = Color.cyan;
            foreach (Transform child in transform) {
                if (child.CompareTag("snappoint")) {
                    Gizmos.DrawSphere(child.position, .05f);
                }
            }
        }
    }
}