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
        }

        private bool PushItemsIntoChests() {
            List<Container> chestsTo = FindContainer(new Vector3(0, -0.25f * 1.5f, 0), false);

            foreach (Container to in chestsTo) {
                bool pushed = IterateInventory(selfContainer.GetInventory(), firstItem => {
                    if (to.GetInventory().CanAddItem(firstItem, 1)) {
                        Vector2i gridPos = firstItem.m_gridPos;
                        to.AddItemToChest(selfContainer, gridPos, new Vector2i(-1, -1));
                        return true;
                    }

                    return false;
                });

                if (pushed) {
                    return true;
                }
            }

            return false;
        }

        private bool DrainItemsFromChests() {
            List<Container> chestsFrom = FindContainer(new Vector3(0, 0.25f * 1.5f, 0), true);

            foreach (Container from in chestsFrom) {
                bool movedItem = IterateInventory(from.GetInventory(), firstItem => {
                    if (selfContainer.GetInventory().CanAddItem(firstItem, 1)) {
                        Vector2i gridPos = firstItem.m_gridPos;
                        from.RemoveItemFromChest(selfContainer, gridPos, new Vector2i(-1, -1));
                        return true;
                    }

                    return false;
                });

                if (movedItem) {
                    return true;
                }
            }

            return false;
        }

        private List<ItemDrop> FindItemDrops(Vector3 relativePos) {
            Vector3 center = transform.position + relativePos;
            int count = Physics.OverlapBoxNonAlloc(center, Vector3.one / 2f, tmpColliders, Quaternion.identity, itemMask);

            List<ItemDrop> items = new List<ItemDrop>();

            for (int i = 0; i < count; i++) {
                ItemDrop item = tmpColliders[i].gameObject.GetComponentInParent<ItemDrop>();

                if (item) {
                    items.Add(item);
                }
            }

            return items;
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

        private List<Container> FindContainer(Vector3 relativePos, bool allowHopper) {
            Vector3 center = transform.position + relativePos;
            int count = Physics.OverlapBoxNonAlloc(center, Vector3.one / 2f, tmpColliders, Quaternion.identity, pieceMask);
            List<Container> chests = new List<Container>();

            for (int i = 0; i < count; i++) {
                Container container = tmpColliders[i].gameObject.GetComponentInParent<Container>();

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

        private static bool IterateInventory(Inventory target, Func<ItemDrop.ItemData, bool> firstItem) {
            for (int y = 0; y < target.m_height; y++) {
                for (int x = 0; x < target.m_width; x++) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item == null) {
                        continue;
                    }

                    bool used = firstItem(item);

                    if (used) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
