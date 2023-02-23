using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;
using ValheimHopper.Logic;
using Random = UnityEngine.Random;

namespace ValheimHopper {
    [DefaultExecutionOrder(5)]
    public class Hopper : MonoBehaviour, IPushTarget, IPullTarget {
        public Piece Piece { get; private set; }
        private ZNetView zNetView;
        private Container selfContainer;
        private Collider[] tmpColliders = new Collider[1000];
        private static int pieceMask;
        private static int itemMask;

        public int PushPriority { get; } = 15;
        public int PullPriority { get; } = 15;
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

        private float fixedDeltaTime;
        private int transferFrames;
        private int objectSearchFrames;

        private int instanceId;
        private int frameOffset;

        public ItemFilter filter;

        public ZBool FilterItemsOption { get; private set; }
        public ZBool DropItemsOption { get; private set; }
        public ZBool PickupItemsOption { get; private set; }

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            Piece = GetComponent<Piece>();
            selfContainer = GetComponent<Container>();

            FilterItemsOption = new ZBool("hopper_filter_items", false, zNetView);
            DropItemsOption = new ZBool("hopper_drop_items", false, zNetView);
            PickupItemsOption = new ZBool("hopper_pickup_items", true, zNetView);

            if (pieceMask == 0) {
                pieceMask = LayerMask.GetMask("piece", "piece_nonsolid");
            }

            if (itemMask == 0) {
                itemMask = LayerMask.GetMask("item");
            }

            fixedDeltaTime = Time.fixedDeltaTime;
            transferFrames = Mathf.RoundToInt((1f / fixedDeltaTime) * TransferInterval);
            objectSearchFrames = Mathf.RoundToInt((1f / fixedDeltaTime) * ObjectSearchInterval);
            instanceId = GetInstanceID();
            frameOffset = Mathf.Abs(instanceId % transferFrames);
        }

        private void Start() {
            if (!IsValid()) {
                return;
            }

            filter = new ItemFilter(zNetView, selfContainer.GetInventory());
            selfContainer.GetInventory().m_onChanged += () => {
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

            int frame = FixedFrameCount();
            int globalFrame = (frame + frameOffset) / transferFrames;

            if ((frame + frameOffset) % transferFrames == 0) {
                if (globalFrame % 2 == 0) {
                    PullItems();
                }

                if (globalFrame % 2 == 1) {
                    PushItems();
                }
            }

            if ((frame + frameOffset + 1) % objectSearchFrames == 0) {
                FindIO();
            }
        }

        private int FixedFrameCount() {
            return Mathf.RoundToInt(Time.fixedTime / fixedDeltaTime);
        }

        public bool IsValid() {
            return this && gameObject && Helper.IsValidNetView(zNetView) && zNetView.HasOwner();
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
                    if (!CanAddItem(item, out Vector2i pos)) {
                        continue;
                    }

                    from.RemoveItem(item, selfContainer.GetInventory(), pos, zNetView.m_zdo.m_uid);
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

                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(i => to.CanAddItem(i));

                if (item == null || !CanPushItem(item)) {
                    continue;
                }

                to.AddItem(item, selfContainer.GetInventory(), zNetView.m_zdo.m_uid);
                return;
            }
        }

        private void DropItem() {
            ItemDrop.ItemData firstItem = selfContainer.GetInventory().FindFirstItem(CanPushItem);

            if (firstItem != null) {
                selfContainer.GetInventory().RemoveOneItem(firstItem);
                float angle = Random.Range(0f, (float)(2f * Math.PI));
                Vector3 randomPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 0.2f;
                Vector3 visualOffset = ItemHelper.GetVisualItemOffset(firstItem.m_dropPrefab.name);
                Vector3 pos = transform.TransformPoint(outPos) + visualOffset + new Vector3(randomPos.x, 0, randomPos.z);
                GameObject drop = Instantiate(firstItem.m_dropPrefab, pos, firstItem.m_dropPrefab.transform.rotation);
                drop.GetComponent<ItemDrop>().m_itemData.m_stack = 1;
            }
        }

        public bool CanAddItem(ItemDrop.ItemData item) {
            return CanAddItem(item, out _);
        }

        public void AddItem(ItemDrop.ItemData item, Inventory source, ZDOID sender) {
            CanAddItem(item, out Vector2i pos);
            selfContainer.AddItemToChest(item, source, pos, sender, 1);
        }

        public IEnumerable<ItemDrop.ItemData> GetItems() {
            return selfContainer.GetInventory().GetItemInOrder();
        }

        public void RemoveItem(ItemDrop.ItemData item, Inventory destination, Vector2i destinationPos, ZDOID sender) {
            selfContainer.RemoveItemFromChest(item, destination, destinationPos, sender, 1);
        }

        public bool CanAddItem(ItemDrop.ItemData itemToAdd, out Vector2i pos) {
            pos = new Vector2i(0, 0);

            if (!selfContainer.GetInventory().CanAddItem(itemToAdd, 1)) {
                return false;
            }

            int itemHash = itemToAdd.m_dropPrefab.name.GetStableHashCode();

            for (int y = 0; y < selfContainer.m_height; y++) {
                for (int x = 0; x < selfContainer.m_width; x++) {
                    ItemDrop.ItemData item = selfContainer.GetInventory().GetItemAt(x, y);
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
            return !nearHoppers.Any(hopper => hopper.pullFrom.Contains(this) && hopper.CanAddItem(item, out _));
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;
            pullFrom = FindTargets<IPullTarget>(inPos, inSize, rotation, i => i.PullPriority);
            pushTo = FindTargets<IPushTarget>(outPos, outSize, rotation, i => i.PushPriority);

            AddNearHoppers();
        }

        private void AddNearHoppers() {
            nearHoppers.Clear();
            int count = Physics.OverlapSphereNonAlloc(transform.position, 1f, tmpColliders, pieceMask);

            for (int i = 0; i < count; i++) {
                Hopper hopper = tmpColliders[i].GetComponentInParent<Hopper>();

                if (hopper && hopper != this && !nearHoppers.Contains(hopper)) {
                    nearHoppers.Add(hopper);
                }
            }
        }

        private List<T> FindTargets<T>(Vector3 pos, Vector3 size, Quaternion rotation, Func<T, int> orderBy) where T : ITarget {
            Vector3 globalPos = transform.TransformPoint(pos);
            List<T> targets = new List<T>();

            int count = Physics.OverlapBoxNonAlloc(globalPos, size / 2f, tmpColliders, rotation, pieceMask | itemMask);

            for (int i = 0; i < count; i++) {
                targets.AddRange(tmpColliders[i].GetComponentsInParent<T>().Where(t => t.InRange(globalPos)));
            }

            return targets.Distinct().OrderByDescending(orderBy).ToList();
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.TransformPoint(inPos), inSize);
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
