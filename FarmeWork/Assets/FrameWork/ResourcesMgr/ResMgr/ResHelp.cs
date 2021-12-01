using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SObject = System.Object;
using UObject = UnityEngine.Object;

namespace ShipFarmeWork.Resource
{
    /// <summary>
    /// 资源接口类
    /// </summary>
    public static class ResHelp
    {
        public static GameObject CommonGame = new GameObject("CommonGame");

        // 获取资源(一般用于图片等资源)
        public static UnityEngine.Object GetAsset(string bundle, string asset, System.Type assetType, GameObject requester)
        {
            return ResourceManager.Instance.GetAsset(bundle, asset, assetType, requester);
        }

        // 获取资源(一般用于图片等资源)
        public static T GetAsset<T>(string bundle, string asset, GameObject requester) where T : UObject
        {
            return ResourceManager.Instance.GetAsset<T>(bundle, asset, requester);
        }

        // 请求bundle包集合中的所有资源
        public static bool RequestAllAssetsFromBundles(string[] bundles, GameObject requester, System.Action<string> bundleCompleteCallback)
        {
            return ResourceManager.Instance.RequestAllAssetsFromBundles(bundles, requester, bundleCompleteCallback);
        }

        // 请求bundle包中的某些资源
        public static bool RequestAssetsFromBundle(string bundle, string[] assets, System.Type[] types, GameObject requester, System.Action<string, string[], System.Type[]> assetsCompleteCallback)
        {
            return ResourceManager.Instance.RequestAssetsFromBundle(bundle, assets, types, requester, assetsCompleteCallback);
        }

        // 获取bundle下的所有资源(一般用于图片等资源)
        public static UnityEngine.Object[] GetAssets(string bundle, GameObject requester, System.Type assetType)
        {
            return ResourceManager.Instance.GetAssets(bundle, requester, assetType);
        }

        // 实例化资源(加载预制体用的)
        public static GameObject CloneAsset(string bundle, string asset, GameObject requester, Transform parent, bool worldPositionStays = false)
        {
            GameObject game = ResourceManager.Instance.CloneAsset(bundle, asset, requester);
            if (game != null && parent != null)
            {
                game.SetActive(true);
                game.transform.SetParent(parent, worldPositionStays);
            }
            return game;
        }

        // 设置池子的容量
        public static void SetClonePoolCapacity(string bundle, string asset, int capacity)
        {
            ResourceManager.Instance.SetClonePoolCapacity(bundle, asset, capacity);
        }

        // 返还单个资源
        public static void ReturnCloneOne(string bundle, string assetName, GameObject clone)
        {
            ResourceManager.Instance.ReturnCloneOne(bundle, assetName, clone);
        }
        // 返还资源
        public static void ReturnClones(string bundle, string assetName, GameObject[] clones)
        {
            ResourceManager.Instance.ReturnClones(bundle, assetName, clones);
        }

        //通过包名返还bundle包
        public static void ReturnBundleByName(SObject requester, string bundle, bool unloadAll = true)
        {
            ResourceManager.Instance.ReturnBundleByName(requester, bundle, unloadAll);
        }
        //返还请求者请求的所有资源
        public static void ReturnAllByRequester(SObject requester, bool cancelRequest = true, bool unloadAll = true)
        {
            ResourceManager.Instance.ReturnAllByRequester(requester, cancelRequest, unloadAll);
        }
    }
}