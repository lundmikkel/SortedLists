namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;

    public class DoublyLinkedRedBlackBinarySearchTree<T> : ISortedList<T> where T : IComparable<T>
    {
        #region Fields

        private const bool Red = true;
        private const bool Black = false;

        private Node _root;
        private readonly Node _sentinel;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // First and last point to each other if the collection is empty
            Contract.Invariant(!IsEmpty || _sentinel.Next == _sentinel && _sentinel.Previous == _sentinel);
            // First and last are empty but the next and previous pointers respectively
            Contract.Invariant(_sentinel.Right == null && _sentinel.Left == null && _sentinel.Count == 0);

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
            if (min != null && root.Key.CompareTo(min()) <= 0)
                return false;
            if (max != null && root.Key.CompareTo(max()) >= 0)
                return false;
            return isBST(root.Left, min, () => root.Key) && isBST(root.Right, () => root.Key, max);
        }

        private bool isSizeConsistent(Node root)
        {
            if (root == null)
                return true;
            if (root.Count != count(root.Left) + count(root.Right) + 1)
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

        private class Node
        {
            public T Key;
            public Node Left, Right;
            public Node Previous, Next;
            public bool Color;
            public int Count;

            public Node() { }

            public Node(T key, Node previous)
            {
                Key = key;
                Color = Red;
                Count = 1;
                insertAfter(previous);
            }

            [ContractInvariantMethod]
            private void invariants()
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
        private IEnumerable<T> enumerateFrom(Node node)
        {
            Contract.Requires(node != null);

            // Iterate until the _sentinel node
            while (node != _sentinel)
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
            while (node != _sentinel)
            {
                yield return node.Key;
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
            node.Count = root.Count;
            root.Count = count(root.Left) + count(root.Right) + 1;

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
            node.Count = root.Count;
            root.Count = count(root.Left) + count(root.Right) + 1;

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

            node.Count = count(node.Left) + count(node.Right) + 1;
            return node;
        }

        #endregion

        #region Constructor

        public DoublyLinkedRedBlackBinarySearchTree()
        {
            Contract.Ensures(_sentinel != null);
            Contract.Ensures(_sentinel.Next == _sentinel);
            Contract.Ensures(_sentinel.Previous == _sentinel);

            _sentinel = new Node();
            _sentinel.Next = _sentinel.Previous = _sentinel;
        }

        #endregion

        #region ISortedList

        #region Properties

        public int Count { get { return count(_root); } }

        public bool IsEmpty { get { return _root == null; } }

        public bool AllowsDuplicates
        {
            get
            {
                // TODO: Allows for duplicates
                return false;
            }
        }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i] { get { return indexer(_root, i).Key; } }

        public T First { get { return _sentinel.Next.Key; } }

        public T Last { get { return _sentinel.Previous.Key; } }

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
            return enumerateBackwardsFrom(_sentinel.Previous);
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFrom(indexer(_root, index));
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
            bool itemFound;
            var index = indexOf(_root, item, out itemFound);
            return itemFound ? index : ~index;
        }

        public bool Contains(T item)
        {
            var node = _root;

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

        #endregion

        #region Extensible

        public bool Add(T item)
        {
            var itemWasAdded = false;

            _root = add(item, _root, _sentinel, ref itemWasAdded);
            _root.Color = Black;

            return itemWasAdded;
        }

        public bool Remove(T item)
        {
            bool itemWasRemoved = false;

            if (_root == null)
                return false;

            // If both children of root are black, set root to red
            if (!isRed(_root.Left) && !isRed(_root.Right))
                _root.Color = Red;

            _root = remove(_root, item, ref itemWasRemoved);

            if (!IsEmpty)
                _root.Color = Black;

            return itemWasRemoved;
        }

        public void Clear()
        {
            Contract.Ensures(_root == null);
            Contract.Ensures(_sentinel.Next == _sentinel);
            Contract.Ensures(_sentinel.Previous == _sentinel);

            _root = null;
            _sentinel.Next = _sentinel.Previous = _sentinel;
        }

        #endregion

        #endregion

        #region Methods

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

        private static Node min(Node node)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>().Left == null);

            while (node.Left != null)
                node = node.Left;

            return node;
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

        private Node add(T key, Node root, Node previous, ref bool itemWasAdded)
        {
            if (root == null)
            {
                itemWasAdded = true;
                return new Node(key, previous);
            }

            var compareTo = key.CompareTo(root.Key);

            if (compareTo < 0)
                root.Left = add(key, root.Left, root.Previous, ref itemWasAdded);
            else if (compareTo > 0)
                root.Right = add(key, root.Right, root, ref itemWasAdded);
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
                ++root.Count;

            return root;
        }

        private Node remove(Node root, T item, ref bool itemWasRemoved)
        {
            if (item.CompareTo(root.Key) < 0)
            {
                if (!isRed(root.Left) && !isRed(root.Left.Left))
                    root = moveRedLeft(root);

                root.Left = remove(root.Left, item, ref itemWasRemoved);
            }
            else
            {
                if (isRed(root.Left))
                    root = rotateRight(root);

                if (item.CompareTo(root.Key) == 0 && (root.Right == null))
                {
                    itemWasRemoved = true;
                    root.RemoveLinks();
                    return null;
                }

                if (!isRed(root.Right) && !isRed(root.Right.Left))
                    root = moveRedRight(root);

                if (item.CompareTo(root.Key) == 0)
                {
                    itemWasRemoved = true;
                    var node = min(root.Right);
                    root.Key = node.Key;
                    root.Right = deleteMin(root.Right);
                }
                else
                    root.Right = remove(root.Right, item, ref itemWasRemoved);
            }
            return itemWasRemoved ? balance(root) : root;
        }

        // delete the key-value pair with the minimum key rooted at root
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
