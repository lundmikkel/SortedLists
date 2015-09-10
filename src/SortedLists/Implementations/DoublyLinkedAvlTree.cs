namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;

    public class DoublyLinkedAvlTree<T> : ISortedList<T> where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private readonly Node _first, _last;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariant()
        {
            // Check the balance invariant holds.
            Contract.Invariant(contractHelperConfirmBalance(_root));

            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _first.Next == _last && _last.Previous == _first);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_first.Previous == null && _first.Right == null && _first.Left == null && _first.Balance == 0);
            Contract.Invariant(_last.Next == null && _last.Right == null && _last.Left == null && _last.Balance == 0);
            Contract.Invariant(_first != _last);

            // Check enumerator is sorted
            Contract.Invariant(this.IsSorted<T>());

            // Check that doubly linked lists are sorted in both direction
            Contract.Invariant(enumerateFrom(_first.Next).IsSorted<T>());
            Contract.Invariant(enumerateBackwardsFrom(_last.Previous).Reverse().IsSorted<T>());

            // Check in-order traversal is sorted
            Contract.Invariant(contractHelperInOrderNodes(_root).IsSorted());

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

        private class Node : IComparable<Node>
        {
            #region Fields

            public T Key;

            public Node Left, Right;
            public Node Previous, Next;

            public int Count, Balance;

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            private void invariant()
            {
                Contract.Invariant(Next == null && Previous == null || Count == count(Left) + 1 + count(Right));
                Contract.Invariant(Next == null && Previous == null || Count == subtree(this).Count());
            }

            private IEnumerable<Node> subtree(Node root)
            {
                if (root.Left != null)
                    foreach (var node in subtree(root.Left))
                        yield return node;

                yield return root;

                if (root.Right != null)
                    foreach (var node in subtree(root.Right))
                        yield return node;
            }

            #endregion

            #region Constructor

            public Node(T key, Node previous)
            {
                Contract.Requires(key != null);
                Contract.Requires(previous != null);
                Contract.Requires(previous.Next != null);
                Contract.Ensures(key != null);
                Contract.Ensures(Next != null && Previous != null);

                Key = key;
                Count = 1;
                insertAfter(previous);
            }

            public Node()
            { }

            #endregion

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

                previous.Next = this;
                Previous = previous;

                Next = next;
                next.Previous = this;
            }

            public void Remove()
            {

                Previous.Next = Next;
                Next.Previous = Previous;
            }

            public int CompareTo(Node other)
            {
                return Key.CompareTo(other.Key);
            }

            public void Swap(Node successor)
            {
                Contract.Requires(successor != null);

                var tmp = Key;
                Key = successor.Key;
                successor.Key = tmp;
            }

            public void UpdateCount()
            {
                Count = count(Left) + 1 + count(Right);
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
                            root.Left.Balance = (sbyte) ((root.Balance == +1) ? -1 : 0);
                            root.Right.Balance = (sbyte) ((root.Balance == -1) ? +1 : 0);
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
                            root.Left.Balance = (sbyte) (root.Balance == +1 ? -1 : 0);
                            root.Right.Balance = (sbyte) (root.Balance == -1 ? +1 : 0);
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

            root.UpdateCount();
            node.UpdateCount();

            return node;
        }

        #endregion

        #region Constructor

        public DoublyLinkedAvlTree()
        {
            Contract.Ensures(_first != null);
            Contract.Ensures(_last != null);
            Contract.Ensures(_first.Next == _last);
            Contract.Ensures(_last.Previous == _first);

            _first = new Node();
            _last = new Node();

            _first.Next = _last;
            _last.Previous = _first;
        }

        #endregion

        #region ISortedList

        #region Properties

        public int Count { get { return count(_root); } }

        public bool IsEmpty { get { return _root == null; } }

        public bool AllowsDuplicates { get { return false; } }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i] { get { return indexer(_root, i).Key; } }

        public T First { get { return _first.Next.Key; } }

        public T Last { get { return _last.Previous.Key; } }

        #endregion

        #region Enumerable

        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            return enumerateFrom(indexer(_root, index));
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
            return enumerateBackwardsFrom(indexer(_root, index));
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
            bool itemFound;
            var index = indexOf(_root, item, out itemFound);
            return itemFound ? index : ~index;
        }

        public bool Contains(T item)
        {
            return contains(_root, item);
        }

        #endregion

        #region Extensible

        public bool Add(T item)
        {
            var intervalWasAdded = false;
            var rotationNeeded = false;

            _root = add(item, _root, _first, ref rotationNeeded, ref intervalWasAdded);

            return intervalWasAdded;
        }

        public bool Remove(T item)
        {
            var intervalWasRemoved = false;
            var rotationNeeded = false;
            _root = remove(item, _root, ref intervalWasRemoved, ref rotationNeeded);

            return intervalWasRemoved;
        }

        public void Clear()
        {
            Contract.Ensures(_root == null);
            Contract.Ensures(_first.Next == _last);
            Contract.Ensures(_last.Previous == _first);

            _root = null;

            _first.Next = _last;
            _last.Previous = _first;
        }

        #endregion

        #endregion

        #region Methods

        [Pure]
        private static int count(Node node)
        {
            return node == null ? 0 : node.Count;
        }

        private static Node indexer(Node node, int index)
        {
            while (true)
            {
                var leftCount = count(node.Left);

                if (index < leftCount)
                    node = node.Left;
                else if (index > leftCount)
                {
                    node = node.Right;
                    index -= leftCount + 1;
                }
                else
                    return node;
            }
        }

        [Pure]
        private IEnumerable<T> enumerateFrom(Node node)
        {
            Contract.Requires(node != null);

            // Iterate until the _sentinel node
            while (node != _last)
            {
                yield return node.Key;
                node = node.Next;
            }
        }

        [Pure]
        private IEnumerable<T> enumerateBackwardsFrom(Node node)
        {
            Contract.Requires(node != null);

            // Iterate until the _sentinel node
            while (node != _first)
            {
                yield return node.Key;
                node = node.Previous;
            }
        }

        private static int indexOf(Node node, T item, out bool itemFound)
        {
            itemFound = false;
            var index = 0;

            while (node != null)
            {
                var compareLow = item.CompareTo(node.Key);

                if (compareLow < 0)
                    node = node.Left;
                else if (compareLow > 0)
                {
                    index += 1 + count(node.Left);
                    node = node.Right;
                }
                else
                {
                    itemFound = true;
                    index += count(node.Left);
                    break;
                }
            }

            return index;
        }

        private static bool contains(Node node, T item)
        {
            while (node != null)
            {
                var compareTo = item.CompareTo(node.Key);

                if (compareTo < 0)
                    node = node.Left;
                else if (compareTo > 0)
                    node = node.Right;
                else
                    return true;
            }
            return false;
        }

        private Node add(T item, Node root, Node previous, ref bool rotationNeeded, ref bool intervalWasAdded)
        {
            if (root == null)
            {
                rotationNeeded = true;
                intervalWasAdded = true;
                var node = new Node(item, previous);
                return node;
            }

            var compare = item.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = add(item, root.Left, root.Previous, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance--;
            }
            else if (compare > 0)
            {
                root.Right = add(item, root.Right, root, ref rotationNeeded, ref intervalWasAdded);

                // Adjust node balance, if node was added
                if (rotationNeeded)
                    root.Balance++;
            }
            else
            {
                // Interval was not added
                return root;
            }

            if (intervalWasAdded)
                ++root.Count;

            // Tree might be unbalanced after node was added, so we rotate
            if (rotationNeeded)
                root = rotateForAdd(root, ref rotationNeeded);

            return root;
        }

        private Node remove(T item, Node root, ref bool intervalWasRemoved, ref bool rotationNeeded)
        {
            if (root == null)
                return null;

            var compare = item.CompareTo(root.Key);

            if (compare < 0)
            {
                root.Left = remove(item, root.Left, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance++;
            }
            else if (compare > 0)
            {
                root.Right = remove(item, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else if (root.Left != null && root.Right != null)
            {
                var successor = root.Next;

                // Swap root and successor nodes
                root.Swap(successor);

                // Remove the successor node
                root.Right = remove(successor.Key, root.Right, ref intervalWasRemoved, ref rotationNeeded);

                if (rotationNeeded)
                    root.Balance--;
            }
            else
            {
                rotationNeeded = true;
                intervalWasRemoved = true;
                root.Remove();

                // Return Left if not null, otherwise Right - one must be null
                return root.Left ?? root.Right;
            }

            if (intervalWasRemoved)
                --root.Count;

            if (rotationNeeded)
                root = rotateForRemove(root, ref rotationNeeded);

            return root;
        }

        #endregion
    }
}
