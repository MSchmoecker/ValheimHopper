using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MultiUserChest;

namespace ValheimHopper {
    public class Hopper : MonoBehaviour {
        private ZNetView zNetView;
        private Container selfContainer;
        private Collider[] tmpColliders = new Collider[1000];
        private static int pieceMask;
        private static int itemMask;

        [SerializeField] private Vector3 inPos = new Vector3(0, 0.25f * 1.5f, 0);
        [SerializeField] private Vector3 outPos = new Vector3(0, -0.25f * 1.5f, 0);
        [SerializeField] private Vector3 inSize = new Vector3(1f, 1f, 1f);
        [SerializeField] private Vector3 outSize = new Vector3(1f, 1f, 1f);

        private List<Container> chestsTo = new List<Container>();
        private List<Container> chestsFrom = new List<Container>();
        private List<Smelter> smelters = new List<Smelter>();

        private const float TransferInterval = 0.2f;
        private const float ObjectSearchInterval = 3f;

        private float fixedDeltaTime;
        private int transferFrames;
        private int objectSearchFrames;
        private int instanceId;

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
        }

        private void FixedUpdate() {
            if ((FixedFrameCount() + instanceId) % transferFrames == 0) {
                TransferItems();
            }

            if ((FixedFrameCount() + instanceId + 1) % objectSearchFrames == 0) {
                chestsTo = FindContainer(outPos, outSize, false);
                chestsFrom = FindContainer(inPos, inSize, true);
                smelters = FindSmelters(outPos, outSize);
            }
        }

        private int FixedFrameCount() {
            return Mathf.RoundToInt(Time.fixedTime / fixedDeltaTime);
        }

        private void TransferItems() {
            if (!zNetView || !zNetView.IsValid() || zNetView.GetZDO() == null || !zNetView.IsOwner()) {
                return;
            }

            bool drained = DrainItemsFromChests();

            if (!drained) {
                PickupItems();
            }

            bool pushed = PushItemsIntoChests();

            if (!pushed) {
                PushItemsIntoSmelter();
            }
        }

        private bool PushItemsIntoChests() {
            foreach (Container to in chestsTo) {
                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(i => to.GetInventory().CanAddItem(i, 1));

                if (item != null) {
                    to.AddItemToChest(item, selfContainer, new Vector2i(-1, -1), 1);
                    return true;
                }
            }

            return false;
        }

        private bool DrainItemsFromChests() {

            foreach (Container from in chestsFrom) {
                ItemDrop.ItemData item = from.GetInventory().FindFirstItem(i => selfContainer.GetInventory().CanAddItem(i, 1));

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
                    bool isAllowedOre = smelter.IsItemAllowed(i) && smelter.GetQueueSize() < smelter.m_maxOre;
                    bool isFuelItem = smelter.m_fuelItem != null && smelter.m_fuelItem.m_itemData.m_shared.m_name == i.m_shared.m_name;
                    bool isAllowedFuel = isFuelItem && smelter.GetFuel() < smelter.m_maxFuel - 1;

                    Vector3 pos = transform.position + outPos;
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
            List<ItemDrop> items = FindItemDrops(inPos, inSize);

            foreach (ItemDrop item in items) {
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
            if (selfContainer.GetInventory().CanAddItem(item.m_itemData.m_dropPrefab, 1)) {
                ItemDrop.ItemData itemData = item.m_itemData.Clone();
                itemData.m_stack = 1;
                selfContainer.GetInventory().AddItem(item.m_itemData);
                item.RemoveOne();
                return true;
            }

            return false;
        }

        private List<ItemDrop> FindItemDrops(Vector3 relativePos, Vector3 size) {
            Vector3 center = transform.TransformPoint(relativePos);
            int count = Physics.OverlapBoxNonAlloc(center, size / 2f, tmpColliders, Quaternion.identity, itemMask);

            List<ItemDrop> items = new List<ItemDrop>();

            for (int i = 0; i < count; i++) {
                ItemDrop item = tmpColliders[i].GetComponentInParent<ItemDrop>();

                if (item) {
                    items.Add(item);
                }
            }

            return items;
        }

        private List<Container> FindContainer(Vector3 relativePos, Vector3 size, bool allowHopper) {
            Vector3 center = transform.TransformPoint(relativePos);
            int count = Physics.OverlapBoxNonAlloc(center, size / 2f, tmpColliders, Quaternion.identity, pieceMask);
            List<Container> chests = new List<Container>();

            for (int i = 0; i < count; i++) {
                Container container = tmpColliders[i].GetComponentInParent<Container>();

                if (!container || container.gameObject == gameObject) {
                    continue;
                }

                if (!allowHopper && container.GetComponent<Hopper>()) {
                    continue;
                }

                chests.Add(container);
            }

            return chests;
        }

        private List<Smelter> FindSmelters(Vector3 relativePos, Vector3 size) {
            Vector3 center = transform.TransformPoint(relativePos);
            int count = Physics.OverlapBoxNonAlloc(center, size / 2f, tmpColliders, Quaternion.identity, pieceMask);
            List<Smelter> smelters = new List<Smelter>();

            for (int i = 0; i < count; i++) {
                Smelter smelter = tmpColliders[i].GetComponentInParent<Smelter>();

                if (smelter) {
                    smelters.Add(smelter);
                }
            }

            return smelters;
        }

        private void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(inPos, inSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(outPos, outSize);
        }
    }
}
