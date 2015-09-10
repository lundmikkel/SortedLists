namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using C5;
    using SCG = System.Collections.Generic;

    public class RedBlackTreeSortedSplitList<T> : ISortedList<T> where T : IComparable<T>
    {
        #region Fields

        private const bool Red = true, Black = false;

        private Node _root;
        private readonly Node _sentinel;
        private readonly int _deepness;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _sentinel.Next == _root && _sentinel.Previous == _root);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_sentinel.Right == null && _sentinel.Left == null && _sentinel.Count == 0);

            // Root always has a node, and its list is never empty
            Contract.Invariant(_root != null && _root.List != null);

            // Does this binary tree satisfy symmetric order?
            Contract.Invariant(isBST(_root, null, null));
            // are the size fields correct?
            Contract.Invariant(isSizeConsistent(_root));
            // Does the tree have no red right links, and at most one (left) red links in a row on any path?
            Contract.Invariant(is23(_root));
            // do all paths from root to leaf have same number of black edges?
            Contract.Invariant(isBalanced());
            // check that ranks are consistent
            Contract.Invariant(isRankConsistent());
        }

        // is the tree rooted at root a BST with all keys strictly between min and max
        // (if min or max is null, treat as empty constraint)
        // Credit: Bob Dondero's elegant solution
        private bool isBST(Node root, Func<T> min, Func<T> max)
        {
            if (root == null)
                return true;
            if (min != null && root.List[0].CompareTo(min()) <= 0)
                return false;
            if (max != null && root.List[0].CompareTo(max()) >= 0)
                return false;
            return isBST(root.Left, min, () => root.List[0]) && isBST(root.Right, () => root.List[0], max);
        }

        private bool isSizeConsistent(Node root)
        {
            if (root == null)
                return true;
            if (root.Count != count(root.Left) + count(root.Right) + root.List.Count)
                return false;
            return isSizeConsistent(root.Left) && isSizeConsistent(root.Right);
        }

        private bool isRankConsistent()
        {
            for (var i = 0; i < Count; ++i)
                if (i != IndexOf(this[i]))
                    return false;

            foreach (T key in this)
                if (key.CompareTo(this[IndexOf(key)]) != 0)
                    return false;

            return true;
        }

        private bool is23(Node root)
        {
            if (root == null)
                return true;
            if (isRed(root.Right))
                return false;
            if (root != _root && isRed(root) && isRed(root.Left))
                return false;
            return is23(root.Left) && is23(root.Right);
        }

        private bool isBalanced()
        {
            var black = 0;     // number of black links on path from root to min
            var node = _root;
            while (node != null)
            {
                if (!isRed(node)) black++;
                node = node.Left;
            }
            return isBalanced(_root, black);
        }

        // does every path from the root to a leaf have the given number of black links?
        private bool isBalanced(Node root, int black)
        {
            if (root == null) return black == 0;
            if (!isRed(root)) black--;
            return isBalanced(root.Left, black) && isBalanced(root.Right, black);
        }

        #endregion

        #region Inner Class

        [DebuggerDisplay("Count = {Count}, List Count = {List == null ? 0 : List.Count}")]
        //[DebuggerTypeProxy(typeof(NodeDebugView))]
        private class Node
        {
            public List<T> List;
            public Node Left, Right;
            public Node Previous, Next;
            public bool Color;
            public int Count;

            public Node() { }

            public Node(T item, Node previous) : this(previous)
            {
                List.Add(item);
                ++Count;
            }

            public Node(Node previous)
            {
                List = new List<T>();
                Color = Red;
                Count = 0;
                insertAfter(previous);
            }

            [ContractInvariantMethod]
            private void invariants()
            {
                // The list is never empty
                //Contract.Invariant(List == null || List.Count > 0);
                // The list is sorted
                Contract.Invariant(List == null || List.IsSorted());

                // The count is the left and right trees' count plus the items in the list
                Contract.Invariant(List == null || Count == count(Left) + List.Count + count(Right));

                // TODO: If I point to you, you point to me!
            }

            private void insertAfter(Node previous)
            {
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(Contract.OldValue(previous.Next) == previous.Next.Next);
                Contract.Ensures(previous.Next == this);
                Contract.Ensures(this.Previous == previous);
                Contract.Ensures(Contract.OldValue(previous.Next).Previous == this);
                Contract.Ensures(this.Next == Contract.OldValue(previous.Next));

                var next = previous.Next;

                previous.Next = next.Previous = this;
                Previous = previous;
                Next = next;
            }

            public void RemoveLinks()
            {
                Previous.Next = Next;
                Next.Previous = Previous;
            }

            public void swapLists(Node root)
            {
                var list = root.List;
                root.List = List;
                List = list;

                UpdateCount();
                root.UpdateCount();
            }

            public void UpdateCount()
            {
                Count = count(Left) + List.Count + count(Right);
            }
        }

        private sealed class NodeDebugView
        {
            private readonly Node _node;

            public NodeDebugView(Node node)
            {
                _node = node;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Items
            {
                get { return _node == null ? new T[0] : _node.List.ToArray(); }
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Check if a node is red.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>True if the node is red.</returns>
        [Pure]
        private bool isRed(Node node)
        {
            return node != null && node.Color == Red;
        }

        /// <summary>
        /// Returns the number of nodes in the subtree rooted at the node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The number of nodes in the subtree rooted at the node. 0 if null.</returns>
        [Pure]
        private static int count(Node node)
        {
            Contract.Ensures(Contract.Result<int>() >= 0);

            return node == null ? 0 : node.Count;
        }

        [Pure]
        private IEnumerable<T> enumerateFrom(Node node, int index = -1)
        {
            Contract.Requires(node != null);

            // Iterate current node partially
            if (index >= 0)
            {
                var list = node.List;
                for (int i = index; i < list.Count; ++i)
                    yield return list[i];
                node = node.Next;
            }

            // Iterate until the _sentinel node
            while (node != _sentinel)
            {
                foreach (var item in node.List)
                    yield return item;
                node = node.Next;
            }
        }

        [Pure]
        private IEnumerable<T> enumerateBackwardsFrom(Node node, int index = -1)
        {
            Contract.Requires(node != null);

            // Iterate current node partially
            if (index >= 0)
            {
                var list = node.List;
                for (int i = index; i >= 0; --i)
                    yield return list[i];
                node = node.Previous;
            }

            // Iterate until the sentinel node
            while (node != _sentinel)
            {
                var list = node.List;
                for (int i = list.Count - 1; i >= 0; --i)
                    yield return list[i];

                node = node.Previous;
            }
        }

        #endregion

        #region Tree Helper Methods

        /// <summary>
        /// Makes a left-leaning link lean to the right.
        /// </summary>
        /// <param name="root">The node.</param>
        /// <returns>The new parent node.</returns>
        private Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(isRed(root.Left));

            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;

            // Fix colors
            node.Color = node.Right.Color;
            node.Right.Color = Red;

            // Fix count
            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        /// <summary>
        /// Makes a right-leaning link lean to the left.
        /// </summary>
        /// <param name="root">The node.</param>
        /// <returns>The new parent node.</returns>
        private Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(isRed(root.Right));

            // Rotate
            var node = root.Right;
            root.Right = node.Left;
            node.Left = root;

            // Fix colors
            node.Color = node.Left.Color;
            node.Left.Color = Red;

            // Fix count
            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        /// <summary>
        /// Flip the colors of a node and its two children
        /// </summary>
        /// <param name="node">The node.</param>
        private void flipColors(Node node)
        {
            Contract.Requires(node != null);
            Contract.Requires(node.Left != null);
            Contract.Requires(node.Right != null);
            // Node must have opposite color of its two children
            Contract.Requires(!isRed(node) && isRed(node.Left) && isRed(node.Right) || (isRed(node) && !isRed(node.Left) && !isRed(node.Right)));

            node.Color = !node.Color;
            node.Left.Color = !node.Left.Color;
            node.Right.Color = !node.Right.Color;
        }

        // Assuming that root is red and both root.Left and root.Left.Left
        // are black, make root.Left or one of its children red.
        private Node moveRedLeft(Node node)
        {
            Contract.Requires(node != null);
            Contract.Requires(isRed(node) && !isRed(node.Left) && !isRed(node.Left.Left));

            flipColors(node);
            if (isRed(node.Right.Left))
            {
                node.Right = rotateRight(node.Right);
                node = rotateLeft(node);
                flipColors(node);
            }
            return node;
        }

        // Assuming that root is red and both root.Right and root.Right.Left
        // are black, make root.Right or one of its children red.
        private Node moveRedRight(Node node)
        {
            Contract.Requires(node != null);
            Contract.Requires(isRed(node) && !isRed(node.Right) && !isRed(node.Right.Left));
            flipColors(node);
            if (isRed(node.Left.Left))
            {
                node = rotateRight(node);
                flipColors(node);
            }
            return node;
        }

        /// <summary>
        /// Restores red-black tree invariant for a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The potentially new node.</returns>
        private Node balance(Node node)
        {
            Contract.Requires(node != null);

            if (isRed(node.Right)) node = rotateLeft(node);
            if (isRed(node.Left) && isRed(node.Left.Left)) node = rotateRight(node);
            if (isRed(node.Left) && isRed(node.Right)) flipColors(node);

            node.UpdateCount();
            return node;
        }

        #endregion

        #region Constructor

        public RedBlackTreeSortedSplitList(int deepness = 512)
        {
            Contract.Ensures(_sentinel != null);
            Contract.Ensures(_root != null);
            Contract.Ensures(_sentinel.Next == _root);
            Contract.Ensures(_sentinel.Previous == _root);

            // Create sentinel node
            _sentinel = new Node();
            _sentinel.Next = _sentinel.Previous = _sentinel;

            // Create root node
            _root = new Node(_sentinel);

            _deepness = deepness;
        }

        #endregion

        #region ISortedList

        #region Properties

        public int Count { get { return count(_root); } }

        public bool IsEmpty { get { return Count == 0; } }

        public bool AllowsDuplicates { get { return false; } }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i]
        {
            get
            {
                return indexer(ref i).List[i];
            }
        }

        public T First { get { return _sentinel.Next.List[0]; } }

        public T Last
        {
            get
            {
                var list = _sentinel.Previous.List;
                return list[list.Count - 1];
            }
        }

        #endregion

        #region Enumerable

        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            return enumerateFrom(indexer(ref index), index);
        }

        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            foreach (var item in EnumerateFromIndex(inclusiveFrom))
            {
                if (inclusiveFrom++ < exclusiveTo)
                    yield return item;
                else
                    yield break;
            }
        }

        public IEnumerable<T> EnumerateBackwards()
        {
            return enumerateBackwardsFrom(_sentinel.Previous);
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFrom(indexer(ref index), index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerateFrom(_sentinel.Next).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Find

        public int IndexOf(T item)
        {
            if (IsEmpty)
                return ~0;

            var node = _root;
            var index = 0;

            while (true)
            {
                var compareLow = item.CompareTo(node.List[0]);

                if (compareLow < 0)
                {
                    if (node.Left == null)
                    {
                        // Item can only be in previous node's list, so we move back
                        node = node.Previous;
                        // Subtract the list count to adjust after moving back
                        if (node != _sentinel)
                            index -= node.List.Count;

                        break;
                    }

                    // Recurse subtree
                    node = node.Left;
                }
                else if (compareLow > 0)
                {
                    if (node.Right == null)
                    {
                        // Item can only be in this node's list, so we count the items in left tree
                        index += count(node.Left);

                        break;
                    }

                    // Fix index
                    index += count(node.Left) + node.List.Count;
                    // Recurse subtree
                    node = node.Right;
                }
                else
                    // Item was first in a list
                    return count(node.Left) + index;
            }

            // Search the list
            var itemIndex = indexOf(node.List, item);
            var itemFound = itemIndex >= 0;
            index += itemFound ? itemIndex : ~itemIndex;

            return itemFound ? index : ~index;
        }

        public bool Contains(T item)
        {
            if (IsEmpty)
                return false;

            var node = _root;

            while (node != null)
            {
                var compareTo = item.CompareTo(node.List[0]);

                if (compareTo < 0)
                {
                    // Found a leaf, search previous list
                    if (node.Left == null)
                        return indexOf(node.Previous.List, item) >= 0;

                    node = node.Left;
                }
                else if (compareTo > 0)
                {
                    // Found list, search it
                    if (node.Right == null)
                        return indexOf(node.List, item) >= 0;

                    node = node.Right;
                }
                else
                    return true;
            }

            return false;
        }

        #endregion

        #region Extensible

        public bool Add(T item)
        {
            if (Count < _deepness)
            {
                var index = indexOf(_root.List, item);
                if (index >= 0)
                    return false;

                _root.List.Insert(~index, item);
                _root.Count++;
                return true;
            }

            var itemWasAdded = false;

            _root = add(item, _root, _sentinel, ref itemWasAdded);
            _root.Color = Black;

            return itemWasAdded;
        }

        public bool Remove(T item)
        {
            if (IsEmpty)
                return false;

            // If both children of root are black, set root to red
            if (!isRed(_root.Left) && !isRed(_root.Right))
                _root.Color = Red;

            var itemWasRemoved = false;
            _root = remove(_root, item, ref itemWasRemoved);
            _root.Color = Black;

            return itemWasRemoved;
        }

        public void Clear()
        {
            Contract.Ensures(_root != null);
            Contract.Ensures(_sentinel.Next == _root);
            Contract.Ensures(_sentinel.Previous == _root);

            _sentinel.Next = _sentinel.Previous = _sentinel;
            _root = new Node(_sentinel);
        }

        #endregion

        #endregion

        #region Methods

        private Node indexer(ref int index)
        {
            var node = _root;
            while (true)
            {
                var leftCount = count(node.Left);

                if (index < leftCount)
                    node = node.Left;
                else if (index >= leftCount + node.List.Count)
                {
                    index -= leftCount + node.List.Count;
                    node = node.Right;
                }
                else
                {
                    index -= leftCount;
                    return node;
                }
            }
        }

        private static Node min(Node node)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>().Left == null);

            while (node.Left != null)
                node = node.Left;

            return node;
        }

        private static int indexOf(SCG.IList<T> list, T item)
        {
            if (list == null)
                return ~0;

            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = list[mid].CompareTo(item);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                else
                    return mid;
            }

            return ~low;
        }

        private Node add(T item, Node root, Node previous, ref bool itemWasAdded)
        {
            if (root == null)
            {
                var rootIsMinNode = previous == _sentinel;
                // The node to hold the item
                var listNode = rootIsMinNode ? _sentinel.Next : previous;
                var itemIndex = indexOf(listNode.List, item);

                Contract.Assert(previous != _sentinel || itemIndex == ~0);

                // Duplicate found
                if (0 <= itemIndex)
                    return null;

                itemIndex = ~itemIndex;

                // Check if list fits the item
                if (listNode.List.Count < _deepness)
                {
                    listNode.List.Insert(itemIndex, item);
                }
                else if (itemIndex == listNode.List.Count && previous.Next == _sentinel)
                    root = new Node(item, previous);
                else
                {
                    var mid = _deepness >> 1;

                    // Create new list if we are on the last list
                    // or if the next list can not accommodate the second half of the list
                    if (listNode.Next == _sentinel || mid <= previous.Next.List.Count)
                    {
                        root = new Node(previous);

                        // If root is the min node, make root the list node
                        if (rootIsMinNode)
                        {
                            listNode.swapLists(root);
                            listNode = root;
                        }
                    }

                    // Move second half to next list before inserting, to avoid resizing list above deepness
                    listNode.Next.List.InsertRange(0, listNode.List.GetRange(mid, mid));
                    listNode.Next.Count += mid;

                    listNode.List.RemoveRange(mid, mid);
                    listNode.Count -= mid;

                    // Insert item in the appropriate list
                    if (itemIndex <= mid)
                    {
                        listNode.List.Insert(itemIndex, item);
                        listNode.Count++;
                    }
                    else
                    {
                        listNode.Next.List.Insert(itemIndex - mid, item);
                        listNode.Next.Count++;
                    }
                }

                itemWasAdded = true;
                return root;
            }

            var compareTo = item.CompareTo(root.List[0]);

            if (compareTo < 0)
                root.Left = add(item, root.Left, root.Previous, ref itemWasAdded);
            else if (compareTo > 0)
                root.Right = add(item, root.Right, root, ref itemWasAdded);
            else
                return root;

            // Fix-up any right-leaning links
            if (isRed(root.Right) && !isRed(root.Left))
                root = rotateLeft(root);
            if (isRed(root.Left) && isRed(root.Left.Left))
                root = rotateRight(root);
            if (isRed(root.Left) && isRed(root.Right))
                flipColors(root);

            if (itemWasAdded)
                root.UpdateCount();

            return root;
        }

        private Node remove(Node root, T item, ref bool itemWasRemoved)
        {
            if (item.CompareTo(root.List[0]) < 0)
            {
                if (!isRed(root.Left) && !isRed(root.Left.Left))
                    root = moveRedLeft(root);

                root.Left = remove(root.Left, item, ref itemWasRemoved);
            }
            else
            {
                if (isRed(root.Left))
                    root = rotateRight(root);

                if (item.CompareTo(root.List[0]) == 0 && (root.Right == null))
                {
                    itemWasRemoved = true;
                    root.RemoveLinks();
                    return null;
                }

                if (!isRed(root.Right) && !isRed(root.Right.Left))
                    root = moveRedRight(root);

                if (item.CompareTo(root.List[0]) == 0)
                {
                    itemWasRemoved = true;
                    var node = min(root.Right);
                    root.List = node.List;
                    root.Right = deleteMin(root.Right);
                }
                else
                    root.Right = remove(root.Right, item, ref itemWasRemoved);
            }
            return itemWasRemoved ? balance(root) : root;
        }

        // delete the List-value pair with the minimum List rooted at root
        private Node deleteMin(Node root)
        {
            if (root.Left == null)
            {
                root.RemoveLinks();
                return null;
            }

            if (!isRed(root.Left) && !isRed(root.Left.Left))
                root = moveRedLeft(root);

            root.Left = deleteMin(root.Left);
            return balance(root);
        }

        #endregion
    }
}
