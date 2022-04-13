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
                    Vector2i gridPos = firstItem.m_gridPos;
                    ContainerHandler.RemoveItemFromChest(from, selfContainer, gridPos, new Vector2i(-1, -1));
                });
            }

            if (chestsTo.Count > 0) {
                Container to = chestsTo[0];

                IterateInventory(selfContainer.GetInventory(), firstItem => {
                    Vector2i gridPos = firstItem.m_gridPos;
                    ContainerHandler.AddItemToChest(to, selfContainer, gridPos, new Vector2i(-1, -1));
                });
            }

            //ContainerHandler.AddItemToChest(to, from.GetInventory(), from.m_nview.GetZDO().m_uid, new Vector2i(x, y), new Vector2i(-1, -1), 1, false);
            // if (item == null || !to.CanAddItem(item, 1)) {
            //     continue;
            // }
        }

        private List<Container> FindContainer(Vector3 relativePos, bool allowHopper) {
            int count = Physics.OverlapBoxNonAlloc(transform.position + relativePos, Vector3.one / 2f, tmpColliders);
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

        private static void IterateInventory(Inventory target, Action<ItemDrop.ItemData> firstItem) {
            for (int y = 0; y < target.m_height; y++) {
                for (int x = 0; x < target.m_width; x++) {
                    ItemDrop.ItemData item = target.GetItemAt(x, y);

                    if (item == null) {
                        continue;
                    }

                    firstItem(item);
                    return;
                }
            }
        }
    }
}
