using MyceliumNetworking;
using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.Utilities;
using Unity.Properties;
using UnityEngine;
using Zorro.Core;

namespace ItemLibrary
{
    public static class ItemHandler
    {

        internal static int initialItemCount; // Assign this on lobby enter, use it to remove from range whenever we join another lobby
        public static List<Item>? ItemList;
        public static List<Item> TemporaryItemList = new List<Item>();


        internal static void AddAllItemsToDatabase()
        {

            SingletonAsset<ItemDatabase>.Instance.Objects.AddRange(ItemList);
        }

        // ShopUtils doesn't add a grace value
        internal static void OnLobbyEntered() // Add to ItemDatabase, get ItemDatabase count for it since ShopUtils adds as soon as the game starts
        {
            initialItemCount = SingletonAsset<ItemDatabase>.Instance.Objects.Length; // They are 0 based. Length will start with one however if the array has any items in it.
           
            dynamic itemCount;
            bool idIsByte = false;

            #pragma warning disable CS0183 // 'is' expression's given expression is always of the provided type
            #pragma warning disable IDE0150 // Prefer 'null' check over type check
            // ItemID patcher check, ignore warnings that call me stupid
            if (new Item().id is byte)
            {
                idIsByte = true;
                itemCount = (byte)initialItemCount;
            }
            else
            {
                itemCount = initialItemCount;
            }
            #pragma warning restore IDE0150 // Prefer 'null' check over type check
            #pragma warning restore CS0183 // 'is' expression's given expression is always of the provided type

            ItemList = new List<Item>(TemporaryItemList.Count);
            if (MyceliumNetwork.IsHost)
            {
                for (int index = 0; index < TemporaryItemList.Count; index++)
                {
                    itemCount++; // Missing compiler required member 'Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo.Create
                    // Error will appear anywhere the dynamic variable is used

                    if (idIsByte && itemCount > 255)
                    {
                        break;
                    }
                    ItemList.Add(TemporaryItemList[index]);
                    ItemList[index].id = itemCount;
                    MyceliumNetwork.SetLobbyData("ItemLibrary_" + ItemList[index].GetType().Name, ItemList[index].id);
                }
                AddAllItemsToDatabase();
                return;
            }

            for (var index = 0; index < TemporaryItemList.Count; index++) // This is scuffed. Sorry.
            {
                ItemList.Add(new Item());
            }

            for (var index = 0; index < TemporaryItemList.Count; index++)
            {
                dynamic id;
                if (idIsByte)
                {
                    id = MyceliumNetwork.GetLobbyData<byte>("ItemLibrary_" + TemporaryItemList[index].GetType().Name);
                }
                else
                {
                    id = MyceliumNetwork.GetLobbyData<int>("ItemLibrary_" + TemporaryItemList[index].GetType().Name);
                }

                int itemListIndex = ((int)id - itemCount) - 1;
                ItemList[itemListIndex] = TemporaryItemList[index];
                ItemList[itemListIndex].id = id;
            }

            AddAllItemsToDatabase();
        }

        internal static void OnLobbyLeft() // Remove from ItemDatabase
        {
            
        }

        public static void AssignItem(Item item)
        {
            MyceliumNetwork.RegisterLobbyDataKey("ItemLibrary_" + item.GetType().Name);
            TemporaryItemList.Add(item);
        }
    }
}
