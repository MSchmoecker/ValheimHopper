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

            List<Container> chestsFrom = FindContainer(new Vector3(0, 0.25f * 1.5f, 0), true);
            List<Container> chestsTo = FindContainer(new Vector3(0, -0.25f * 1.5f, 0), false);

            if (chestsFrom.Count > 0) {
                Container from = chestsFrom[0];

                IterateInventory(from.GetInventory(), firstItem => {
                    if (selfContainer.GetInventory().CanAddItem(firstItem, 1)) {
                        Vector2i gridPos = firstItem.m_gridPos;
                        from.RemoveItemFromChest(selfContainer, gridPos, new Vector2i(-1, -1));
                        return true;
                    }

                    return false;
                });
            }

            if (chestsTo.Count > 0) {
                Container to = chestsTo[0];

                IterateInventory(selfContainer.GetInventory(), firstItem => {
                    if (to.GetInventory().CanAddItem(firstItem, 1)) {
                        Vector2i gridPos = firstItem.m_gridPos;
                        to.AddItemToChest(selfContainer, gridPos, new Vector2i(-1, -1));
                        return true;
                    }

                    return false;
                });
            }
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

        private static void IterateInventory(Inventory target, Func<ItemDrop.ItemData, bool> firstItem) {
            for (int y = 0; y < target.m_height; y++) {
                for (int x = 0; x < target.m_width; x++) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item == null) {
                        continue;
                    }

                    bool used = firstItem(item);

                    if (used) {
                        return;
                    }
                }
            }
        }
    }
}
