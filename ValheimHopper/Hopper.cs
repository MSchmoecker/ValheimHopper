using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;
using Logger = Jotunn.Logger;
using Random = UnityEngine.Random;

namespace ValheimHopper {
    [DefaultExecutionOrder(5)]
    public class Hopper : MonoBehaviour {
        public Piece Piece { get; private set; }
        private ZNetView zNetView;
        private WearNTear wearNTear;
        private Container selfContainer;
        private Collider[] tmpColliders = new Collider[1000];
        private static int pieceMask;
        private static int itemMask;

        [SerializeField] private bool autoDeconstruct;

        [SerializeField] private Vector3 inPos = new Vector3(0, 0.25f * 1.5f, 0);
        [SerializeField] private Vector3 outPos = new Vector3(0, -0.25f * 1.5f, 0);
        [SerializeField] private Vector3 inSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 outSize = new Vector3(1f, 1f, 1f);

        [SerializeField] private List<TargetIO> targetsTo = new List<TargetIO>();
        [SerializeField] private List<TargetIO> targetsFrom = new List<TargetIO>();
        [SerializeField] private List<Hopper> nearHoppers = new List<Hopper>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private float fixedDeltaTime;
        private int transferFrames;
        private int objectSearchFrames;

        private int instanceId;
        private int frameOffset;

        public ZBool FilterItemsOption { get; private set; }
        public ZBool DropItemsOption { get; private set; }
        public ZBool PickupItemsOption { get; private set; }

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            Piece = GetComponent<Piece>();
            selfContainer = GetComponent<Container>();
            wearNTear = GetComponent<WearNTear>();

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

            selfContainer.GetInventory().m_onChanged += SaveFilter;
        }

        private void SaveFilter() {
            foreach (ItemDrop.ItemData item in selfContainer.GetInventory().m_inventory) {
                Vector2i gridPos = item.m_gridPos;
                int itemHash = item.m_dropPrefab.name.GetStableHashCode();
                SetFilterItemHash(gridPos, itemHash);
            }
        }

        public void SetFilterItemHash(Vector2i gridPos, int itemHash) {
            SetFilterItemHash(gridPos.x, gridPos.y, itemHash);
        }

        public void SetFilterItemHash(int x, int y, int itemHash) {
            zNetView.GetZDO().Set($"hopper_filter_{x}_{y}", itemHash);
        }

        public int GetFilterItemHash(Vector2i gridPos) {
            return GetFilterItemHash(gridPos.x, gridPos.y);
        }

        public int GetFilterItemHash(int x, int y) {
            return zNetView.GetZDO().GetInt($"hopper_filter_{x}_{y}");
        }

        public void PasteData(Hopper copy) {
            FilterItemsOption.Set(copy.FilterItemsOption.Get());
            DropItemsOption.Set(copy.DropItemsOption.Get());
            PickupItemsOption.Set(copy.PickupItemsOption.Get());

            for (int x = 0; x < selfContainer.GetInventory().m_width; x++) {
                for (int y = 0; y < selfContainer.GetInventory().m_height; y++) {
                    SetFilterItemHash(x, y, copy.GetFilterItemHash(x, y));
                }
            }
        }

        public void ResetValues() {
            FilterItemsOption.Reset();
            DropItemsOption.Reset();
            PickupItemsOption.Reset();

            for (int x = 0; x < selfContainer.GetInventory().m_width; x++) {
                for (int y = 0; y < selfContainer.GetInventory().m_height; y++) {
                    SetFilterItemHash(x, y, 0);
                }
            }
        }

        private void FixedUpdate() {
            if (!IsValid() || !zNetView.IsOwner()) {
                return;
            }

            if (autoDeconstruct) {
                if (Player.m_localPlayer) {
                    wearNTear.Destroy();
                }

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

        private bool IsValid() {
            return zNetView && zNetView.IsValid() && zNetView.GetZDO() != null;
        }

        private void PullItems() {
            bool drained = DrainItemsFromChests();

            if (!drained && PickupItemsOption.Get()) {
                PickupItems();
            }
        }

        private void PushItems() {
            if (targetsTo.Count == 0) {
                if (DropItemsOption.Get()) {
                    DropItem();
                }
            } else {
                bool pushed = PushItemsIntoChests();

                if (!pushed) {
                    PushItemsIntoSmelters();
                }
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

        private bool PushItemsIntoChests() {
            foreach (TargetIO to in targetsTo) {
                if (!to.piece || !to.container) {
                    continue;
                }

                bool FindPushItem(ItemDrop.ItemData i) {
                    if (!CanPushItem(i)) {
                        return false;
                    }

                    return to.container.GetInventory().CanAddItem(i, 1);
                }

                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(FindPushItem);

                if (item != null) {
                    to.container.AddItemToChest(item, selfContainer, new Vector2i(-1, -1), 1);
                    return true;
                }
            }

            return false;
        }

        private bool CanAddItem(ItemDrop.ItemData itemToAdd, out Vector2i pos) {
            pos = new Vector2i(-1, -1);

            if (!selfContainer.GetInventory().CanAddItem(itemToAdd, 1)) {
                return false;
            }

            if (!FilterItemsOption.Get()) {
                return true;
            }

            int itemHash = itemToAdd.m_dropPrefab.name.GetStableHashCode();

            for (int y = 0; y < selfContainer.m_height; y++) {
                for (int x = 0; x < selfContainer.m_width; x++) {
                    ItemDrop.ItemData item = selfContainer.GetInventory().GetItemAt(x, y);

                    if (GetFilterItemHash(x, y) == itemHash && (item == null || item.m_stack + 1 <= item.m_shared.m_maxStackSize)) {
                        pos = new Vector2i(x, y);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CanPushItem(ItemDrop.ItemData item) {
            return !nearHoppers.Any(h => h.targetsFrom.Exists(t => t.piece == Piece) && h.CanAddItem(item, out _));
        }

        private bool DrainItemsFromChests() {
            foreach (TargetIO from in targetsFrom) {
                if (!from.piece || !from.container) {
                    continue;
                }

                foreach (ItemDrop.ItemData item in from.container.GetInventory().FindItemInOrder()) {
                    if (CanAddItem(item, out Vector2i pos)) {
                        from.container.RemoveItemFromChest(item, selfContainer, pos, 1);
                        return true;
                    }
                }
            }

            return false;
        }

        private void PushItemsIntoSmelters() {
            foreach (TargetIO to in targetsTo) {
                if (!to.piece || !to.smelter) {
                    continue;
                }

                Smelter smelter = to.smelter;

                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(i => {
                    if (!CanPushItem(i)) {
                        return false;
                    }

                    bool isAllowedOre = smelter.IsItemAllowed(i) && smelter.GetQueueSize() < smelter.m_maxOre;
                    bool isFuelItem = smelter.m_fuelItem != null && smelter.m_fuelItem.m_itemData.m_shared.m_name == i.m_shared.m_name;
                    bool isAllowedFuel = isFuelItem && smelter.GetFuel() < smelter.m_maxFuel - 1;

                    Vector3 pos = transform.TransformPoint(outPos);
                    Switch oreSwitch = smelter.m_addOreSwitch;
                    Switch fuelSwitch = smelter.m_addWoodSwitch;

                    bool oreRange = oreSwitch == null || Vector3.Distance(oreSwitch.transform.position, pos) <= 1f;
                    bool fuelRange = fuelSwitch == null || Vector3.Distance(fuelSwitch.transform.position, pos) <= 1f;

                    return isAllowedOre && oreRange || isAllowedFuel && fuelRange;
                });

                if (item != null) {
                    selfContainer.GetInventory().RemoveItem(item, 1);

                    if (smelter.IsItemAllowed(item)) {
                        smelter.m_nview.InvokeRPC("AddOre", item.m_dropPrefab.name);
                    } else {
                        smelter.m_nview.InvokeRPC("AddFuel");
                    }

                    break;
                }
            }
        }

        private bool PickupItems() {
            foreach (ItemDrop item in FindItemDrops(inPos, inSize)) {
                if (!item.m_nview || !item.m_nview.IsValid()) {
                    continue;
                }

                if (selfContainer.GetInventory().CanAddItem(item.m_itemData, 1)) {
                    if (!item.m_nview.IsOwner()) {
                        item.RequestOwn();
                        continue;
                    }

                    bool pickuped = PickupItem(item);

                    if (pickuped) {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool PickupItem(ItemDrop item) {
            if (!selfContainer.GetInventory().CanAddItem(item.m_itemData, 1)) {
                return false;
            }

            bool hasRemoved = item.RemoveOne();

            if (!hasRemoved) {
                return false;
            }

            ItemDrop.ItemData itemData = item.m_itemData.Clone();
            itemData.m_stack = 1;
            selfContainer.GetInventory().AddItem(itemData);
            return true;
        }

        private IEnumerable<ItemDrop> FindItemDrops(Vector3 relativePos, Vector3 size) {
            Vector3 center = transform.TransformPoint(relativePos);
            int count = Physics.OverlapBoxNonAlloc(center, size / 2f, tmpColliders, Quaternion.identity, itemMask);

            for (int i = 0; i < count; i++) {
                ItemDrop item = tmpColliders[i].GetComponentInParent<ItemDrop>();

                if (item) {
                    yield return item;
                }
            }
        }

        private void FindIO() {
            Quaternion rotation = transform.rotation;

            targetsFrom.Clear();
            targetsTo.Clear();
            nearHoppers.Clear();

            AddNearPieces();
            AddIOPieces(inPos, inSize, rotation, targetsFrom);
            AddIOPieces(outPos, outSize, rotation, targetsTo);
        }

        private void AddNearPieces() {
            int count = Physics.OverlapSphereNonAlloc(transform.position, 1f, tmpColliders, pieceMask);

            for (int i = 0; i < count; i++) {
                Piece piece = tmpColliders[i].GetComponentInParent<Piece>();

                if (!piece || piece.gameObject == gameObject) {
                    continue;
                }

                Hopper hopper = piece.GetComponent<Hopper>();

                if (hopper && !nearHoppers.Contains(hopper)) {
                    nearHoppers.Add(hopper);
                }
            }
        }

        private void AddIOPieces(Vector3 pos, Vector3 size, Quaternion rotation, List<TargetIO> targetList) {
            int count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(pos), size / 2f, tmpColliders, rotation, pieceMask);

            for (int i = 0; i < count; i++) {
                Piece piece = tmpColliders[i].GetComponentInParent<Piece>();

                if (!piece || piece.gameObject == gameObject || targetList.Exists(t => t.piece == piece)) {
                    continue;
                }

                targetList.Add(new TargetIO(piece));
            }
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
