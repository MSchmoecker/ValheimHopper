using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;

namespace ValheimHopper {
    [DefaultExecutionOrder(5)]
    public class Hopper : MonoBehaviour {
        public ZNetView zNetView;
        private Container selfContainer;
        private Collider[] tmpColliders = new Collider[1000];
        private static int pieceMask;
        private static int itemMask;

        [SerializeField] private Vector3 inPos = new Vector3(0, 0.25f * 1.5f, 0);
        [SerializeField] private Vector3 outPos = new Vector3(0, -0.25f * 1.5f, 0);
        [SerializeField] private Vector3 inSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 outSize = new Vector3(1f, 1f, 1f);

        [SerializeField] private List<Container> chestsTo = new List<Container>();
        [SerializeField] private List<Container> chestsFrom = new List<Container>();
        [SerializeField] private List<Smelter> smelters = new List<Smelter>();
        [SerializeField] private List<Hopper> nearHoppers = new List<Hopper>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private float fixedDeltaTime;
        private int transferFrames;
        private int objectSearchFrames;

        private int instanceId;
        private int frameOffset;

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            selfContainer = GetComponent<Container>();

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

            if (zNetView && zNetView.IsValid()) {
                zNetView.Register<bool>("Hopper_SetLeaveOneItemRPC", SetLeaveOneItemRPC);
            }
        }

        private void FixedUpdate() {
            if (!IsValid()) {
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
            return zNetView && zNetView.IsValid() && zNetView.GetZDO() != null && zNetView.IsOwner();
        }

        private void PullItems() {
            bool drained = DrainItemsFromChests();

            if (!drained) {
                PickupItems();
            }
        }

        private void PushItems() {
            bool pushed = PushItemsIntoChests();

            if (!pushed) {
                PushItemsIntoSmelter();
            }
        }

        private bool PushItemsIntoChests() {
            foreach (Container to in chestsTo) {
                bool FindPushItem(ItemDrop.ItemData i) {
                    if (!CanPushItem(i)) {
                        return false;
                    }

                    return to.GetInventory().CanAddItem(i, 1);
                }

                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(FindPushItem);

                if (item != null) {
                    to.AddItemToChest(item, selfContainer, new Vector2i(-1, -1), 1);
                    return true;
                }
            }

            return false;
        }

        private bool CanAddItem(ItemDrop.ItemData item) {
            return selfContainer.GetInventory().CanAddItem(item, 1);
        }

        private bool CanPushItem(ItemDrop.ItemData item) {
            if (item.m_stack == 1 && zNetView.GetZDO().GetBool("hopper_leave_one_item")) {
                return false;
            }

            if (nearHoppers.Any(h => h.chestsFrom.Contains(selfContainer) && h.CanAddItem(item))) {
                return false;
            }

            return true;
        }

        private bool DrainItemsFromChests() {
            foreach (Container from in chestsFrom) {
                ItemDrop.ItemData item = from.GetInventory().FindFirstItem(CanAddItem);

                if (item != null) {
                    from.RemoveItemFromChest(item, selfContainer, new Vector2i(-1, -1), 1);
                    return true;
                }
            }

            return false;
        }

        private void PushItemsIntoSmelter() {
            foreach (Smelter smelter in smelters) {
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
            Vector3 pos = transform.position;
            int count = Physics.OverlapBoxNonAlloc(pos, Vector3.one, tmpColliders, Quaternion.identity, pieceMask);

            chestsFrom.Clear();
            chestsTo.Clear();
            smelters.Clear();
            nearHoppers.Clear();

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

            count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(inPos), inSize / 2f, tmpColliders, transform.rotation, pieceMask);

            for (int i = 0; i < count; i++) {
                Piece piece = tmpColliders[i].GetComponentInParent<Piece>();

                if (!piece || piece.gameObject == gameObject) {
                    continue;
                }

                Container container = piece.GetComponent<Container>();

                if (container && !chestsFrom.Contains(container)) {
                    chestsFrom.Add(container);
                }
            }

            count = Physics.OverlapBoxNonAlloc(transform.TransformPoint(outPos), outSize / 2f, tmpColliders, transform.rotation, pieceMask);

            for (int i = 0; i < count; i++) {
                Piece piece = tmpColliders[i].GetComponentInParent<Piece>();

                if (!piece || piece.gameObject == gameObject) {
                    continue;
                }

                Container container = piece.GetComponent<Container>();
                Smelter smelter = piece.GetComponent<Smelter>();

                if (container && !chestsTo.Contains(container)) {
                    chestsTo.Add(container);
                }

                if (smelter && !smelters.Contains(smelter)) {
                    smelters.Add(smelter);
                }
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
                    Gizmos.DrawSphere(child.position, .1f);
                }
            }
        }

        public void SetLeaveOneItem(bool leaveOneItem) {
            if (zNetView.IsOwner()) {
                zNetView.GetZDO().Set("hopper_leave_one_item", leaveOneItem);
            } else {
                zNetView.InvokeRPC("Hopper_SetLeaveOneItemRPC", leaveOneItem);
            }
        }

        private void SetLeaveOneItemRPC(long sender, bool leaveOneItem) {
            if (!zNetView.IsOwner()) {
                return;
            }

            zNetView.GetZDO().Set("hopper_leave_one_item", leaveOneItem);
        }
    }
}
