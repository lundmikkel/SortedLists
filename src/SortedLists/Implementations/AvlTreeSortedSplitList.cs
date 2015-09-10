namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;
    using SCG = System.Collections.Generic;

    public class AvlTreeSortedSplitList<T> : ISortedList<T> where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private readonly Node _first, _last;
        private readonly int _deepness;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // TODO: Code contracts from similar classes!

            // Check the balance invariant holds.
            Contract.Invariant(contractHelperConfirmBalance(_root));

            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _first.Next == _root && _last.Previous == _root);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_first.Previous == null && _first.Right == null && _first.Left == null && _first.Balance == 0);
            Contract.Invariant(_last.Next == null && _last.Right == null && _last.Left == null && _last.Balance == 0);
            Contract.Invariant(_first != _last);

            // Check enumerator is sorted
            Contract.Invariant(this.IsSorted<T>());

            // Check that doubly linked lists are sorted in both direction
            Contract.Invariant(enumerateFrom(_first.Next).IsSorted<T>());
            Contract.Invariant(enumerateBackwardsFrom(_last.Previous).Reverse().IsSorted<T>());

        }

        /// <summary>
        /// Checks that the contractHelperHeight of the tree is balanced.
        /// </summary>
        /// <returns>True if the tree is balanced, else false.</returns>
        [Pure]
        private static bool contractHelperConfirmBalance(Node root)
        {
            var result = true;
            contractHelperHeight(root, ref result);
            return result;
        }

        /// <summary>
        /// Get the contractHelperHeight of the tree.
        /// </summary>
        /// <param name="node">The node you wish to check the contractHelperHeight on.</param>
        /// <param name="result">Reference to a bool that will be set to false if an in-balance is discovered.</param>
        /// <returns>Height of the tree.</returns>
        [Pure]
        private static int contractHelperHeight(Node node, ref bool result)
        {
            if (node == null)
                return 0;

            var heightLeft = contractHelperHeight(node.Left, ref result);
            var heightRight = contractHelperHeight(node.Right, ref result);

            if (node.Balance != heightRight - heightLeft)
                result = false;

            return Math.Max(heightLeft, heightRight) + 1;
        }

        [Pure]
        private static IEnumerable<Node> contractHelperInOrderNodes(Node root)
        {
            if (root == null)
                yield break;

            foreach (var node in contractHelperInOrderNodes(root.Left))
                yield return node;

            yield return root;

            foreach (var node in contractHelperInOrderNodes(root.Right))
                yield return node;
        }

        #endregion

        #region Inner Class

        private class Node
        {
            #region Fields

            public List<T> List;
            public Node Left, Right, Previous, Next;
            public int Count, Balance;

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                // TODO
                Contract.Invariant(Next == null && Previous == null || Count == count(Left) + List.Count + count(Right));

                // The list is never empty
                //Contract.Invariant(List == null || List.Count > 0);
                // The list is sorted
                Contract.Invariant(List == null || List.IsSorted());

                // The count is the left and right trees' count plus the items in the list
                Contract.Invariant(List == null || Count == count(Left) + List.Count + count(Right));

                // TODO: If I point to you, you point to me!
            }

            #endregion

            #region Constructor

            // Only for _first and _last
            public Node() { }

            public Node(T item, Node previous)
                : this(previous)
            {
                List.Add(item);
                ++Count;
            }

            public Node(Node previous)
            {
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                // TODO Contract.Ensures(key != null);
                Contract.Ensures(Next != null && Previous != null);

                List = new List<T>();
                Count = 0;

                var next = previous.Next;
                previous.Next = next.Previous = this;
                Previous = previous;
                Next = next;
            }

            #endregion

            public void Remove()
            {
                Previous.Next = Next;
                Next.Previous = Previous;
            }

            public void SwapLists(Node successor)
            {
                Contract.Requires(successor != null);

                var tmp = List;
                List = successor.List;
                successor.List = tmp;

                // TODO: Do we need to update both?
                UpdateCount();
                successor.UpdateCount();
            }

            public void UpdateCount()
            {
                Count = count(Left) + List.Count + count(Right);
            }
        }

        #endregion

        #region AVL Tree Methods

        private static Node rotateForAdd(Node root, ref bool rotationNeeded)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // Node is balanced after the node was added
                case 0:
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
                            root.Balance = 0;
                            break;
                    }

                    rotationNeeded = false;
                    break;
            }

            return root;
        }

        private static Node rotateForRemove(Node root, ref bool rotationNeeded)
        {
            Contract.Requires(root != null);

            Contract.Requires(root.Balance != -2 || root.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != -1 || root.Left.Left != null);
            Contract.Requires(root.Balance != -2 || root.Left.Balance != +1 || root.Left.Right != null);

            Contract.Requires(root.Balance != +2 || root.Right != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != -1 || root.Right.Left != null);
            Contract.Requires(root.Balance != +2 || root.Right.Balance != +1 || root.Right.Right != null);

            switch (root.Balance)
            {
                // High will not change for parent, so we can stop here
                case -1:
                case +1:
                    rotationNeeded = false;
                    break;

                // Node is unbalanced, so we rotate
                case -2:
                    switch (root.Left.Balance)
                    {
                        // Left Left Case
                        case -1:
                            root = rotateRight(root);
                            root.Balance = root.Right.Balance = 0;
                            break;

                        case 0:
                            root = rotateRight(root);
                            root.Right.Balance = -1;
                            root.Balance = +1;
                            rotationNeeded = false;
                            break;

                        // Left Right Case
                        case +1:
                            root.Left = rotateLeft(root.Left);
                            root = rotateRight(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = (root.Balance == +1) ? -1 : 0;
                            root.Right.Balance = (root.Balance == -1) ? +1 : 0;
                            root.Balance = 0;
                            break;
                    }
                    break;

                // Node is unbalanced, so we rotate
                case +2:
                    switch (root.Right.Balance)
                    {
                        // Right Right Case
                        case +1:
                            root = rotateLeft(root);
                            root.Balance = root.Left.Balance = 0;
                            break;

                        case 0:
                            root = rotateLeft(root);
                            root.Left.Balance = 1;
                            root.Balance = -1;
                            rotationNeeded = false;
                            break;

                        // Right Left Case
                        case -1:
                            root.Right = rotateRight(root.Right);
                            root = rotateLeft(root);

                            // root.Balance is either -1, 0, or +1
                            root.Left.Balance = root.Balance == +1 ? -1 : 0;
                            root.Right.Balance = root.Balance == -1 ? +1 : 0;
                            root.Balance = 0;
                            break;
                    }
                    break;
            }

            return root;
        }

        private static Node rotateRight(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Left != null);

            // Rotate
            var node = root.Left;
            root.Left = node.Right;
            node.Right = root;

            // Fix count
            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        private static Node rotateLeft(Node root)
        {
            Contract.Requires(root != null);
            Contract.Requires(root.Right != null);

            // Rotate
            var node = root.Right;
            root.Right = node.Left;
            node.Left = root;

            // Fix count
            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        #endregion

        #region Constructor

        public AvlTreeSortedSplitList(int deepness = 512)
        {
            Contract.Ensures(_first != null);
            Contract.Ensures(_last != null);
            Contract.Ensures(_first.Next == _root);
            Contract.Ensures(_last.Previous == _root);
            // TODO

            _first = new Node();
            _last = new Node();

            _first.Next = _last;
            _last.Previous = _first;

            // Create root node
            _root = new Node(_first);

            _deepness = deepness;
        }

        #endregion

        #region ISortedList

        #region Properties

        public int Count { get { return count(_root); } }

        public bool IsEmpty { get { return Count == 0; } }

        public bool AllowsDuplicates { get { return false; } }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i] { get { return indexer(ref i).List[i]; } }

        public T First { get { return _first.Next.List[0]; } }

        public T Last
        {
            get
            {
                var list = _last.Previous.List;
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
            return enumerateBackwardsFrom(_last.Previous);
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFrom(indexer(ref index), index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerateFrom(_first.Next).GetEnumerator();
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
                        if (node != _first)
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
            if (IsEmpty)
            {
                _root.List.Add(item);
                _root.Count++;
                return true;
            }

            var itemWasAdded = false;
            var rotationNeeded = false;

            _root = add(item, _root, _first, ref rotationNeeded, ref itemWasAdded);

            return itemWasAdded;
        }

        public bool Remove(T item)
        {
            if (IsEmpty)
                return false;

            var itemWasRemoved = false;
            var rotationNeeded = false;
            _root = remove(item, _root, ref itemWasRemoved, ref rotationNeeded);

            return itemWasRemoved;
        }

        public void Clear()
        {
            Contract.Ensures(_root != null);
            Contract.Ensures(_first.Next == _root);
            Contract.Ensures(_last.Previous == _root);
            // TODO: Contract for nodes following each other nodesConnected(_first, _root)

            _first.Next = _last;
            _last.Previous = _first;

            _root = new Node(_first);
        }

        #endregion

        #endregion

        #region Methods

        [Pure]
        private static int count(Node node)
        {
            return node == null ? 0 : node.Count;
        }

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

            // Iterate until the _last node
            while (node != _last)
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

            // Iterate until the _first node
            while (node != _first)
            {
                var list = node.List;
                for (int i = list.Count - 1; i >= 0; --i)
                    yield return list[i];

                node = node.Previous;
            }
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

        private Node add(T item, Node root, Node previous, ref bool rotationNeeded, ref bool itemWasAdded)
        {
            if (root == null)
            {
                var rootIsMinNode = previous == _first;
                // The node to hold the item
                var listNode = rootIsMinNode ? previous.Next : previous;
                var itemIndex = indexOf(listNode.List, item);

                Contract.Assert(!rootIsMinNode || itemIndex == ~0);

                // Duplicate found
                if (0 <= itemIndex)
                    return null;

                itemIndex = ~itemIndex;

                // Check if list fits the item
                if (listNode.List.Count < _deepness)
                {
                    listNode.List.Insert(itemIndex, item);
                }
                else if (itemIndex == listNode.List.Count && previous.Next == _last)
                {
                    root = new Node(item, previous);
                    rotationNeeded = true;
                }
                else
                {
                    var mid = _deepness >> 1;

                    // Create new list if we are on the last list
                    // or if the next list can not accommodate the second half of the list
                    if (listNode.Next == _last || mid <= previous.Next.List.Count)
                    {
                        root = new Node(previous);
                        rotationNeeded = true;

                        // If root is the min node, make root the list node
                        if (rootIsMinNode)
                        {
                            listNode.SwapLists(root);
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

            var compare = item.CompareTo(root.List[0]);

            if (compare < 0)
            {
                root.Left = add(item, root.Left, root.Previous, ref rotationNeeded, ref itemWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                root.Right = add(item, root.Right, root, ref rotationNeeded, ref itemWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance++;
            }
            else
            {
                // Item was not added
                return root;
            }

            if (itemWasAdded)
            {
                root.UpdateCount();

                // Tree might be unbalanced after node was added, so we rotate
                if (rotationNeeded)
                    root = rotateForAdd(root, ref rotationNeeded);
            }

            return root;
        }

        private Node remove(T item, Node root, ref bool itemWasRemoved, ref bool rotationNeeded)
        {
            // TODO: Make it work

            if (root == null)
            {
                
            }

            var compare = item.CompareTo(root.List[0]);

            if (compare < 0)
            {
                root.Left = remove(item, root.Left, ref itemWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance++;
            }
            else if (compare > 0)
            {
                root.Right = remove(item, root.Right, ref itemWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else if (root.Left != null && root.Right != null)
            {
                var successor = root.Next;

                // Swap lists in root and successor node
                root.SwapLists(successor);

                // Remove the successor node
                root.Right = remove(successor.List[0], root.Right, ref itemWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else
            {
                rotationNeeded = true;
                itemWasRemoved = true;
                root.Remove();

                // Return Left if not null, otherwise Right - one must be null
                return root.Left ?? root.Right;
            }

            if (itemWasRemoved)
            {
                root.UpdateCount();

                if (rotationNeeded)
                    root = rotateForRemove(root, ref rotationNeeded);
            }

            return root;
        }

        #endregion
    }
}
