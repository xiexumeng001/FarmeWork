using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ShipFarmeWork.Logic.CityMap
{
    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
    }

    /// <summary>
    /// 二叉堆对象
    /// 二叉堆排序的逻辑(基于完全二叉树):
    ///     咱们用的是小根堆,就是跟节点永远不大于叶节点的
    ///     当添加一个节点:此节点此时在数组最末尾,然后此节点和它的根节点比较,如果此节点比根节点小,就与根节点替换,循环与新的根节点比较,直到比根节点大
    ///                       这样每次添加新的节点都能保证最上面的根节点为最小节点
    ///     当移除根节点:把最后一个节点替换为最上面的根节点,然后最上面的根节点与两个叶节点比较,找出最小的开始替换,然后继续往下替换,直到他比最小的叶节点小
    ///                       这样就能保证 叶节点永远比根节点大了,因为他下沉的路上,最小的都作为各自的根节点了
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BinaryHeap<T> where T : IHeapItem<T>
    {


        public T[] items;
        int currentItemCount;  //当前堆中的元素数量

        public BinaryHeap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        //加入新元素,向上排序
        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            //向上排序
            SortUp(item);
            currentItemCount++;
        }

        //移除根节点,向下排序
        public T RemoveFirst()
        {
            T firstItem = items[0];//根节点
            currentItemCount--;
            items[0] = items[currentItemCount];//填充最后一个元素
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }
        //更新树重新排序
        public void UpdateItem(T item)
        {
            SortUp(item);
        }
        //返回当前元素数量
        public int Count()
        {
            return currentItemCount;
        }
        //是否存在元素
        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }
        //向下排序,寻找子节点
        void SortDown(T item)
        {
            while (true)
            {
                //把左右叶节点中最小的往上提一提
                int childIndexLeft = item.HeapIndex * 2 + 1; //左叶
                int childIndexRight = item.HeapIndex * 2 + 2; //右叶
                int swapIndex = 0;
                //如果还存在子节点
                if (childIndexLeft < currentItemCount)
                {
                    swapIndex = childIndexLeft;
                    if (childIndexRight < currentItemCount)
                    {
                        if (items[childIndexLeft].CompareTo(items[childIndexRight]) > 0)
                        {
                            swapIndex = childIndexRight; //得到小的节点
                        }
                    }
                    if (item.CompareTo(items[swapIndex]) > 0)
                    { //和小的节点比较
                        Swap(item, items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                { //如果没有子节点了,返回
                    return;
                }
            }
        }
        //向上排序,寻找父节点
        void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;
            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) < 0)
                {
                    Swap(item, parentItem); //当前节点更小就交换
                }
                else { break; }
                parentIndex = (item.HeapIndex - 1) / 2; //继续向上
            }

        }
        //交换数据和指针
        void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int itemAIndex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAIndex;
        }


    }


    public class Node : IHeapItem<Node>
    {
        int heapIndex;
        public int HeapIndex
        {
            get { return heapIndex; }
            set { heapIndex = value; }
        }

        public int F;

        public Node(int f)
        {
            F = f;
        }

        public int CompareTo(Node nodeToCompare)
        {
            //比较大小
            return F.CompareTo(nodeToCompare.F);
            //int compare = F.CompareTo(nodeToCompare.F);
            //if (compare == 0)
            //{
            //    compare = F.CompareTo(nodeToCompare.F);
            //}
            //return compare;
        }
    }
}
