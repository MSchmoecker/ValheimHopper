using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NoChestBlock;

namespace ValheimHopper {
    public class Hopper : MonoBehaviour {
        private ZNetView zNetView;
        private Container selfContainer;
        private Collider[] tmpColliders = new Collider[1000];
        private static int pieceMask = LayerMask.GetMask("piece", "piece_nonsolid");
        private static int itemMask = LayerMask.GetMask("item");

        private void Awake() {
            zNetView = GetComponent<ZNetView>();
            selfContainer = GetComponent<Container>();

            if (!zNetView) {
                return;
            }

            InvokeRepeating(nameof(TransferItems), 1f, 1f);
        }

        private void TransferItems() {
            if (zNetView.GetZDO() == null || !zNetView.IsOwner()) {
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
            List<Container> chestsTo = FindContainer(new Vector3(0, -0.25f * 1.5f, 0), false);

            foreach (Container to in chestsTo) {
                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(i => to.GetInventory().CanAddItem(i, 1));

                if (item != null) {
                    to.AddItemToChest(selfContainer, item.m_gridPos, new Vector2i(-1, -1));
                    return true;
                }
            }

            return false;
        }

        private bool DrainItemsFromChests() {
            List<Container> chestsFrom = FindContainer(new Vector3(0, 0.25f * 1.5f, 0), true);

            foreach (Container from in chestsFrom) {
                ItemDrop.ItemData item = from.GetInventory().FindFirstItem(i => selfContainer.GetInventory().CanAddItem(i, 1));

                if (item != null) {
                    from.RemoveItemFromChest(selfContainer, item.m_gridPos, new Vector2i(-1, -1));
                    return true;
                }
            }

            return false;
        }

        private void PushItemsIntoSmelter() {
            List<Smelter> smelters = FindSmelters(new Vector3(0, -0.25f * 1.5f, 0));

            foreach (Smelter smelter in smelters) {
                ItemDrop.ItemData item = selfContainer.GetInventory().FindFirstItem(i => {
                    bool isAllowedOre = smelter.IsItemAllowed(i) && smelter.GetQueueSize() < smelter.m_maxOre;
                    bool isFuelItem = smelter.m_fuelItem.m_itemData.m_shared.m_name == i.m_shared.m_name;
                    bool isAllowedFuel = isFuelItem && smelter.GetFuel() < smelter.m_maxFuel - 1;
                    return isAllowedOre || isAllowedFuel;
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
            List<ItemDrop> items = FindItemDrops(new Vector3(0, 0.25f * 1.5f, 0));

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
                selfContainer.GetInventory().AddItem(item.m_itemData.m_dropPrefab, 1);
                item.RemoveOne();
                return true;
            }

            return false;
        }

        private List<ItemDrop> FindItemDrops(Vector3 relativePos) {
            Vector3 center = transform.position + relativePos;
            int count = Physics.OverlapBoxNonAlloc(center, Vector3.one / 2f, tmpColliders, Quaternion.identity, itemMask);

            List<ItemDrop> items = new List<ItemDrop>();

            for (int i = 0; i < count; i++) {
                ItemDrop item = tmpColliders[i].GetComponentInParent<ItemDrop>();

                if (item) {
                    items.Add(item);
                }
            }

            return items;
        }

        private List<Container> FindContainer(Vector3 relativePos, bool allowHopper) {
            Vector3 center = transform.position + relativePos;
            int count = Physics.OverlapBoxNonAlloc(center, Vector3.one / 2f, tmpColliders, Quaternion.identity, pieceMask);
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

        private List<Smelter> FindSmelters(Vector3 relativePos) {
            Vector3 center = transform.position + relativePos;
            int count = Physics.OverlapBoxNonAlloc(center, Vector3.one / 2f, tmpColliders, Quaternion.identity, pieceMask);
            List<Smelter> smelters = new List<Smelter>();

            for (int i = 0; i < count; i++) {
                Smelter smelter = tmpColliders[i].GetComponentInParent<Smelter>();

                if (smelter) {
                    smelters.Add(smelter);
                }
            }

            return smelters;
        }
    }
}
