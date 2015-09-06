namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using C5;

    public class RedBlackBinarySearchTree<T> : ISortedList<T> where T : IComparable<T>
    {
        #region Fields

        private const bool Red = true;
        private const bool Black = false;

        private Node _root;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // TODO: Very time consuming
            /*
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
            */
        }

        // is the tree rooted at x a BST with all keys strictly between min and max
        // (if min or max is null, treat as empty constraint)
        // Credit: Bob Dondero's elegant solution
        private bool isBST(Node x, Func<T> min, Func<T> max)
        {
            if (x == null)
                return true;
            if (min != null && x.Key.CompareTo(min()) <= 0)
                return false;
            if (max != null && x.Key.CompareTo(max()) >= 0)
                return false;
            return isBST(x.Left, min, () => x.Key) && isBST(x.Right, () => x.Key, max);
        }

        private bool isSizeConsistent(Node x)
        {
            if (x == null)
                return true;
            if (x.Count != count(x.Left) + count(x.Right) + 1)
                return false;
            return isSizeConsistent(x.Left) && isSizeConsistent(x.Right);
        }

        private bool isRankConsistent() {
            for (var i = 0; i < Count; ++i)
                if (i != IndexOf(this[i]))
                    return false;

            foreach (T key in this)
                if (key.CompareTo(this[IndexOf(key)]) != 0)
                    return false;

            return true;
        }

        private bool is23(Node x)
        {
            if (x == null)
                return true;
            if (isRed(x.Right))
                return false;
            if (x != _root && isRed(x) && isRed(x.Left))
                return false;
            return is23(x.Left) && is23(x.Right);
        }

        private bool isBalanced()
        {
            var black = 0;     // number of black links on path from root to min
            var x = _root;
            while (x != null)
            {
                if (!isRed(x)) black++;
                x = x.Left;
            }
            return isBalanced(_root, black);
        }

        // does every path from the root to a leaf have the given number of black links?
        private bool isBalanced(Node x, int black)
        {
            if (x == null) return black == 0;
            if (!isRed(x)) black--;
            return isBalanced(x.Left, black) && isBalanced(x.Right, black);
        }

        #endregion

        #region Inner Class

        private class Node
        {
            public T Key;
            public Node Left, Right;
            public bool Color;
            public int Count;

            public Node(T key)
            {
                Key = key;
                Color = Red;
                Count = 1;
            }

            [ContractInvariantMethod]
            private void invariants()
            {
                Contract.Invariant(Key != null);
                Contract.Invariant(Count > 0);
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

        #endregion

        #region Tree Helper Methods

        /// <summary>
        /// Makes a left-leaning link lean to the right.
        /// </summary>
        /// <param name="h">The node.</param>
        /// <returns>The new parent node.</returns>
        private Node rotateRight(Node h)
        {
            Contract.Requires(h != null);
            Contract.Requires(isRed(h.Left));

            // Rotate
            var x = h.Left;
            h.Left = x.Right;
            x.Right = h;

            // Fix colors
            x.Color = x.Right.Color;
            x.Right.Color = Red;

            // Fix count
            x.Count = h.Count;
            h.Count = count(h.Left) + count(h.Right) + 1;

            return x;
        }

        /// <summary>
        /// Makes a right-leaning link lean to the left.
        /// </summary>
        /// <param name="h">The node.</param>
        /// <returns>The new parent node.</returns>
        private Node rotateLeft(Node h)
        {
            Contract.Requires(h != null);
            Contract.Requires(isRed(h.Right));

            // Rotate
            var x = h.Right;
            h.Right = x.Left;
            x.Left = h;

            // Fix colors
            x.Color = x.Left.Color;
            x.Left.Color = Red;

            // Fix count
            x.Count = h.Count;
            h.Count = count(h.Left) + count(h.Right) + 1;

            return x;
        }

        /// <summary>
        /// Flip the colors of a node and its two children
        /// </summary>
        /// <param name="h">The node.</param>
        private void flipColors(Node h)
        {
            Contract.Requires(h != null);
            Contract.Requires(h.Left != null);
            Contract.Requires(h.Right != null);
            // Node must have opposite color of its two children
            Contract.Requires(!isRed(h) && isRed(h.Left) && isRed(h.Right) || (isRed(h) && !isRed(h.Left) && !isRed(h.Right)));

            h.Color = !h.Color;
            h.Left.Color = !h.Left.Color;
            h.Right.Color = !h.Right.Color;
        }

        // Assuming that h is red and both h.Left and h.Left.Left
        // are black, make h.Left or one of its children red.
        private Node moveRedLeft(Node h)
        {
            Contract.Requires(h != null);
            Contract.Requires(isRed(h) && !isRed(h.Left) && !isRed(h.Left.Left));

            flipColors(h);
            if (isRed(h.Right.Left))
            {
                h.Right = rotateRight(h.Right);
                h = rotateLeft(h);
                flipColors(h);
            }
            return h;
        }

        // Assuming that h is red and both h.Right and h.Right.Left
        // are black, make h.Right or one of its children red.
        private Node moveRedRight(Node h)
        {
            Contract.Requires(h != null);
            Contract.Requires(isRed(h) && !isRed(h.Right) && !isRed(h.Right.Left));
            flipColors(h);
            if (isRed(h.Left.Left))
            {
                h = rotateRight(h);
                flipColors(h);
            }
            return h;
        }

        /// <summary>
        /// Restores red-black tree invariant for a node.
        /// </summary>
        /// <param name="h">The node.</param>
        /// <returns>The potentially new node.</returns>
        private Node balance(Node h)
        {
            Contract.Requires(h != null);

            if (isRed(h.Right)) h = rotateLeft(h);
            if (isRed(h.Left) && isRed(h.Left.Left)) h = rotateRight(h);
            if (isRed(h.Left) && isRed(h.Right)) flipColors(h);

            h.Count = count(h.Left) + count(h.Right) + 1;
            return h;
        }

        #endregion

        #region Constructor

        public RedBlackBinarySearchTree()
        { }

        #endregion

        #region ISortedList

        #region Properties

        public int Count { get { return count(_root); } }

        public bool IsEmpty { get { return _root == null; } }

        public bool AllowsDuplicates { get { return false; } }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i] { get { return indexer(_root, i).Key; } }

        public T First { get { return min(_root).Key; } }

        public T Last { get { return max(_root).Key; } }

        #endregion

        #region Enumerable

        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            return enumerateRange(_root, index, Count);
        }

        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            return enumerateRange(_root, inclusiveFrom, exclusiveTo);
        }

        public IEnumerable<T> EnumerateBackwards()
        {
            return enumerateBackwards(_root);
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            return enumerateBackwardsFromIndex(_root, index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return enumerate(_root).GetEnumerator();
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
            var itemWasAdded = false;

            _root = add(_root, item, ref itemWasAdded);
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
            _root = null;
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

        private static Node max(Node node)
        {
            Contract.Requires(node != null);
            Contract.Ensures(Contract.Result<Node>().Right == null);

            while (node.Right != null)
                node = node.Right;

            return node;
        }

        private IEnumerable<T> enumerate(Node node)
        {
            // TODO: Speed up

            if (node == null)
                yield break;
            
            foreach (var item in enumerate(node.Left))
                yield return item;
            
            yield return node.Key;
            
            foreach (var item in enumerate(node.Right))
                yield return item;
        }

        private IEnumerable<T> enumerateBackwards(Node node)
        {
            // TODO: Speed up

            if (node == null)
                yield break;

            foreach (var item in enumerateBackwards(node.Right))
                yield return item;
            
            yield return node.Key;

            foreach (var item in enumerateBackwards(node.Left))
                yield return item;
        }

        private IEnumerable<T> enumerateRange(Node node, int inclusiveFrom, int exclusiveTo)
        {
            if (node == null)
                yield break;

            var index = count(node.Left);

            if (inclusiveFrom < index)
                foreach (var item in enumerateRange(node.Left, inclusiveFrom, exclusiveTo))
                    yield return item;

            if (inclusiveFrom <= index && index < exclusiveTo)
                yield return node.Key;

            if (index + 1 < exclusiveTo)
                foreach (var item in enumerateRange(node.Right, inclusiveFrom - index - 1, exclusiveTo - index - 1))
                    yield return item;
        }

        private IEnumerable<T> enumerateBackwardsFromIndex(Node node, int index)
        {
            if (node == null)
                yield break;

            var currentIndex = count(node.Left);

            if (currentIndex < index)
                foreach (var item in enumerateBackwardsFromIndex(node.Right, index - currentIndex - 1))
                    yield return item;

            if (currentIndex <= index)
                yield return node.Key;

            foreach (var item in enumerateBackwardsFromIndex(node.Left, index))
                    yield return item;
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

        private Node add(Node node, T key, ref bool itemWasAdded)
        {
            if (node == null)
            {
                itemWasAdded = true;
                return new Node(key);
            }

            var compareTo = key.CompareTo(node.Key);

            if (compareTo < 0)
                node.Left = add(node.Left, key, ref itemWasAdded);
            else if (compareTo > 0)
                node.Right = add(node.Right, key, ref itemWasAdded);
            else
            {
                return node;
            }

            // Fix-up any right-leaning links
            if (isRed(node.Right) && !isRed(node.Left))
                node = rotateLeft(node);
            if (isRed(node.Left) && isRed(node.Left.Left))
                node = rotateRight(node);
            if (isRed(node.Left) && isRed(node.Right))
                flipColors(node);

            node.Count = count(node.Left) + count(node.Right) + 1;

            return node;
        }

        private Node remove(Node h, T item, ref bool itemWasRemoved)
        {
            if (item.CompareTo(h.Key) < 0)
            {
                if (!isRed(h.Left) && !isRed(h.Left.Left))
                    h = moveRedLeft(h);

                h.Left = remove(h.Left, item, ref itemWasRemoved);
            }
            else
            {
                if (isRed(h.Left))
                    h = rotateRight(h);

                if (item.CompareTo(h.Key) == 0 && (h.Right == null))
                {
                    itemWasRemoved = true;
                    return null;
                }

                if (!isRed(h.Right) && !isRed(h.Right.Left))
                    h = moveRedRight(h);

                if (item.CompareTo(h.Key) == 0)
                {
                    itemWasRemoved = true;
                    var x = min(h.Right);
                    h.Key = x.Key;
                    h.Right = deleteMin(h.Right); 
                }
                else
                    h.Right = remove(h.Right, item, ref itemWasRemoved);
            }
            return balance(h);
        }

        // delete the key-value pair with the minimum key rooted at h
        private Node deleteMin(Node h)
        {
            if (h.Left == null)
                return null;

            if (!isRed(h.Left) && !isRed(h.Left.Left))
                h = moveRedLeft(h);

            h.Left = deleteMin(h.Left);
            return balance(h);
        }

        #endregion
    }
}
