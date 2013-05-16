using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BCad.Collections
{
    public class ReadOnlyTree<TKey, TValue> where TKey : IComparable
    {
        private class Node
        {
            private Node left;
            private Node right;

            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public Node Left
            {
                get { return left; }
                set
                {
                    left = value;
                    RecalculateHeight();
                    RecalculateCount();
                }
            }
            public Node Right
            {
                get { return right; }
                set
                {
                    right = value;
                    RecalculateHeight();
                    RecalculateCount();
                }
            }
            public int Height { get; private set; }
            public int Count { get; private set; }

            public Node(TKey key, TValue value, Node left, Node right)
            {
                Key = key;
                Value = value;
                this.left = left;
                this.right = right;
                RecalculateHeight();
                RecalculateCount();
            }

            public Node Clone()
            {
                return new Node(Key, Value, Left, Right);
            }

            public int BalanceFactor
            {
                get { return GetHeight(Left) - GetHeight(Right); }
            }

            public void ForEach(Action<TKey, TValue> action)
            {
                if (Left != null)
                    Left.ForEach(action);
                action(Key, Value);
                if (Right != null)
                    Right.ForEach(action);
            }

            private void RecalculateHeight()
            {
                Height = Math.Max(GetHeight(Left), GetHeight(Right)) + 1;
            }

            private void RecalculateCount()
            {
                Count = GetCount(Left) + GetCount(Right) + 1;
            }

            public override string ToString()
            {
                return string.Format("{0}", Key);
            }

            private static int GetHeight(Node node)
            {
                if (node == null)
                    return 0;
                return node.Height;
            }

            private static int GetCount(Node node)
            {
                if (node == null)
                    return 0;
                return node.Count;
            }
        }

        private Node root;

        public ReadOnlyTree()
            : this(null)
        {
        }

        private ReadOnlyTree(Node root)
        {
            this.root = root;
        }

        private ReadOnlyTree(TKey key, TValue value, bool isRed)
        {
            root = new Node(key, value, null, null);
        }

        public int Count
        {
            get
            {
                if (root == null)
                    return 0;
                return root.Count;
            }
        }

        /// <summary>
        /// Insert the given values and return the new resultant tree.
        /// </summary>
        public ReadOnlyTree<TKey, TValue> Insert(TKey key, TValue value)
        {
            var current = root;
            var newNode = new Node(key, value, null, null);
            var path = new Stack<Node>();
            while (current != null)
            {
                var comp = key.CompareTo(current.Key);
                if (comp < 0)
                {
                    // go left
                    path.Push(current);
                    current = current.Left;
                }
                else if (comp > 0)
                {
                    // go right
                    path.Push(current);
                    current = current.Right;
                }
                else
                {
                    // replace.  parent is correct
                    newNode.Left = current.Left;
                    newNode.Right = current.Right;
                    break;
                }
            }

            // re-spine the tree
            var newRoot = BalanceAndReSpine(newNode, path, true);
            return new ReadOnlyTree<TKey, TValue>(newRoot);
        }

        /// <summary>
        /// Delete the specified value and return the resultant tree.
        /// </summary>
        public ReadOnlyTree<TKey, TValue> Delete(TKey key)
        {
            var current = root;
            var path = new Stack<Node>();
            while (current != null)
            {
                var comp = key.CompareTo(current.Key);
                if (comp < 0)
                {
                    // go left
                    path.Push(current);
                    current = current.Left;
                }
                else if (comp > 0)
                {
                    // go right
                    path.Push(current);
                    current = current.Right;
                }
                else
                {
                    // found it
                    break;
                }
            }

            if (current == null)
                return this; // key not found, no change

            Node toDelete = current;
            Node newRoot;
            if (current.Left == null && current.Right == null)
            {
                // just delete this node
                if (path.Count == 0)
                {
                    // deleting the only node
                    newRoot = null;
                }
                else
                {
                    var parent = path.Pop().Clone();
                    if (current.Key.CompareTo(parent.Key) < 0)
                        parent.Left = null;
                    else
                        parent.Right = null;
                    parent = Rebalance(parent, false);
                    newRoot = BalanceAndReSpine(parent, path, false);
                }
            }
            else if (current.Left == null && current.Right != null)
            {
                // the right child takes the node's place, and rebalance to the top
                var newNode = current.Right.Clone();
                newRoot = BalanceAndReSpine(newNode, path, false);
            }
            else if (current.Left != null && current.Right == null)
            {
                // the left child takes the node's place, and rebalance to the top
                var newNode = current.Left.Clone();
                newRoot = BalanceAndReSpine(newNode, path, false);
            }
            else
            {
                // both children are present.  replace node with immediate successor and custom respine
                path.Push(toDelete);
                var immediateSuccessor = current.Right;
                path.Push(immediateSuccessor);
                while (immediateSuccessor.Left != null)
                {
                    immediateSuccessor = immediateSuccessor.Left;
                    path.Push(immediateSuccessor);
                }

                var child = immediateSuccessor;
                var childKey = child.Key;
                while (path.Count > 0)
                {
                    var newNode = path.Pop().Clone();
                    if (newNode.Key.CompareTo(immediateSuccessor.Key) == 0)
                    {
                        // the immediate successor gets slipped out
                        newNode = newNode.Right;
                    }
                    else if (newNode.Key.CompareTo(toDelete.Key) == 0)
                    {
                        // this is the node being deleted; replace its values
                        newNode = immediateSuccessor.Clone();
                        newNode.Left = toDelete.Left;
                        if (newNode.Key.CompareTo(toDelete.Right.Key) != 0)
                        {
                            newNode.Right = child;
                        }
                    }
                    else if (childKey.CompareTo(newNode.Key) < 0)
                    {
                        // this was the left child
                        newNode.Left = child;
                    }
                    else
                    {
                        // this was the right child
                        newNode.Right = child;
                    }

                    child = Rebalance(newNode, false);
                    if (child != null)
                    {
                        childKey = child.Key;
                        Debug.Assert(Math.Abs(child.BalanceFactor) <= 1);
                    }
                }

                newRoot = child;
            }

            return new ReadOnlyTree<TKey, TValue>(newRoot);
        }

        public void ForEach(Action<TKey, TValue> action)
        {
            if (root != null)
                root.ForEach(action);
        }

        public List<TKey> GetKeys()
        {
            var list = new List<TKey>(Count);
            ForEach((key, _value) => list.Add(key));
            return list;
        }

        public List<TValue> GetValues()
        {
            var list = new List<TValue>(Count);
            ForEach((_key, value) => list.Add(value));
            return list;
        }

        private static Node BalanceAndReSpine(Node current, Stack<Node> ancestors, bool insert)
        {
            // TODO: add flag for mutable re-balancing; useful for batch inserts/creation
            while (ancestors.Count > 0)
            {
                // re-create parent
                var newParent = ancestors.Pop().Clone();
                if (current.Key.CompareTo(newParent.Key) < 0)
                    newParent.Left = current;
                else
                    newParent.Right = current;
                current = Rebalance(newParent, insert);
            }

            return current;
        }

        private static Node Rebalance(Node node, bool insert)
        {
            if (node == null)
                return null;

            Node result;
            var balanceFactor = node.BalanceFactor;
            if (Math.Abs(balanceFactor) <= 1)
            {
                result = node;
            }
            else
            {
                if (balanceFactor == 2)
                    result = LeftRebalance(node, insert);
                else if (balanceFactor == -2)
                    result = RightRebalance(node, insert);
                else
                    throw new Exception("Unexpected balance: " + balanceFactor);
            }

            Debug.Assert(Math.Abs(result.BalanceFactor) <= 1);
            return result;
        }

        private static Node RightRebalance(Node node, bool insert)
        {
            var r = node.Right;
            var bf = r.BalanceFactor;
            if (bf == -1 || (!insert && bf == 0))
                return RightRightRebalance(node);
            else
                return RightLeftRebalance(node);
        }

        private static Node RightRightRebalance(Node root)
        {
            var newLeftChild = root.Clone();
            newLeftChild.Right = root.Right.Left;
            var newRoot = root.Right.Clone();
            newRoot.Left = newLeftChild;
            return newRoot;
        }

        private static Node RightLeftRebalance(Node root)
        {
            var newRightGrandchild = root.Right.Clone();
            newRightGrandchild.Left = root.Right.Left.Right;
            var newRightChild = root.Right.Left.Clone();
            newRightChild.Right = newRightGrandchild;
            var newRoot = root.Clone();
            newRoot.Right = newRightChild;
            return RightRightRebalance(newRoot);
        }

        private static Node LeftRebalance(Node node, bool insert)
        {
            var l = node.Left;
            var bf = l.BalanceFactor;
            if (bf == 1 || (!insert && bf == 0))
                return LeftLeftRebalance(node);
            else
                return LeftRightRebalance(node);
        }

        private static Node LeftLeftRebalance(Node root)
        {
            var newRightChild = root.Clone();
            newRightChild.Left = root.Left.Right;
            var newRoot = root.Left.Clone();
            newRoot.Right = newRightChild;
            return newRoot;
        }

        private static Node LeftRightRebalance(Node root)
        {
            var newLeftGrandchild = root.Left.Clone();
            newLeftGrandchild.Right = root.Left.Right.Left;
            var newLeftChild = root.Left.Right.Clone();
            newLeftChild.Left = newLeftGrandchild;
            var newRoot = root.Clone();
            newRoot.Left = newLeftChild;
            return LeftLeftRebalance(newRoot);
        }

        public bool KeyExists(TKey key)
        {
            TValue temp;
            return TryFind(key, out temp);
        }

        public bool TryFind(TKey key, out TValue value)
        {
            var current = root;
            while (current != null)
            {
                var comp = key.CompareTo(current.Key);
                if (comp < 0)
                    current = current.Left;
                else if (comp > 0)
                    current = current.Right;
                else
                {
                    value = current.Value;
                    return true;
                }
            }

            value = default(TValue);
            return false;
        }

        public TValue GetValue(TKey key)
        {
            TValue value;
            if (TryFind(key, out value))
                return value;
            throw new IndexOutOfRangeException();
        }
    }
}
