using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace IxMilia.BCad.Collections
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
                    Recalculate();
                }
            }
            public Node Right
            {
                get { return right; }
                set
                {
                    right = value;
                    Recalculate();
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
                Recalculate();
            }

            public Node Clone()
            {
                return new Node(Key, Value, Left, Right);
            }

            public int BalanceFactor
            {
                get { return GetHeight(Left) - GetHeight(Right); }
            }

            private IEnumerable<T> GetValues<T>(Func<Node, T> selector, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (Left != null)
                {
                    foreach (var value in Left.GetValues(selector, cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return value;
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                yield return selector(this);

                if (Right != null)
                {
                    foreach (var value in Right.GetValues(selector, cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return value;
                    }
                }
            }

            public IEnumerable<TValue> GetValues(CancellationToken cancellationToken)
            {
                return GetValues(node => node.Value, cancellationToken);
            }

            public IEnumerable<TKey> GetKeys(CancellationToken cancellationToken)
            {
                return GetValues(node => node.Key, cancellationToken);
            }

            public void ForEach(Action<TKey, TValue> action)
            {
                if (Left != null)
                    Left.ForEach(action);
                action(Key, Value);
                if (Right != null)
                    Right.ForEach(action);
            }

            public void Recalculate()
            {
                RecalculateHeight();
                RecalculateCount();
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

        private void InsertMutable(TKey key, TValue value)
        {
            if (root == null)
                root = new Node(key, value, null, null);
            else
            {
                var requiresBalance = true;
                var current = root;
                var path = new Stack<Node>();
                while (current != null)
                {
                    var comp = key.CompareTo(current.Key);
                    if (comp < 0)
                    {
                        // go left
                        path.Push(current);
                        if (current.Left == null)
                        {
                            // place it here
                            current.Left = new Node(key, value, null, null);
                            break;
                        }
                        else
                        {
                            // go deeper
                            current = current.Left;
                        }
                    }
                    else if (comp > 0)
                    {
                        // go right
                        path.Push(current);
                        if (current.Right == null)
                        {
                            // place it here
                            current.Right = new Node(key, value, null, null);
                            break;
                        }
                        else
                        {
                            // go deeper
                            current = current.Right;
                        }
                    }
                    else
                    {
                        // replace.  parent is correct
                        current.Value = value;
                        requiresBalance = false;
                        break;
                    }
                }

                if (requiresBalance)
                {
                    // traverse up the stack, rebalancing as we go
                    while (path.Count > 0)
                    {
                        current = path.Pop();
                        current.Recalculate();
                        var balanceFactor = current.BalanceFactor;
                        if (Math.Abs(balanceFactor) <= 1)
                        {
                            // nothing to do, this node is balanced
                        }
                        else
                        {
                            var parent = path.Count > 0 ? path.Peek() : null;
                            if (balanceFactor == 2)
                                LeftRebalanceMutable(current, parent);
                            else if (balanceFactor == -2)
                                RightRebalanceMutable(current, parent);
                            else
                                throw new Exception("Unexpected balance: " + balanceFactor);
                        }
                    }
                }
            }
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

        public IEnumerable<TKey> GetKeys(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (root == null)
            {
                return new TKey[0];
            }
            else
            {
                return root.GetKeys(cancellationToken);
            }
        }

        public IEnumerable<TValue> GetValues(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (root == null)
            {
                return new TValue[0];
            }
            else
            {
                return root.GetValues(cancellationToken);
            }
        }

        public static ReadOnlyTree<TKey, TValue> FromEnumerable(IEnumerable<TValue> values, Func<TValue, TKey> keyGenerator)
        {
            var tree = new ReadOnlyTree<TKey, TValue>();
            foreach (var value in values)
            {
                var key = keyGenerator(value);
                tree.InsertMutable(key, value);
            }

            return tree;
        }

        private static Node BalanceAndReSpine(Node current, Stack<Node> ancestors, bool insert)
        {
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

        private void RightRebalanceMutable(Node node, Node parent)
        {
            var bf = node.Right.BalanceFactor;
            if (bf == -1)
                RightRightRebalanceMutable(node, parent);
            else
                RightLeftRebalanceMutable(node, parent);
        }

        private static Node RightRightRebalance(Node root)
        {
            var newLeftChild = root.Clone();
            newLeftChild.Right = root.Right.Left;
            var newRoot = root.Right.Clone();
            newRoot.Left = newLeftChild;
            return newRoot;
        }

        private void RightRightRebalanceMutable(Node root, Node parent)
        {
            // stealing Wikipedia's diagram: (AVL tree)
            //   3                4
            //  / \             /   \
            // A   4           3     5
            //    / \     =>  / \   / \
            //   B   5       A   B C   D
            //      / \
            //     C   D
            var Three = root;
            var Four = Three.Right;
            var B = Four.Left;
            Three.Right = B;
            Four.Left = Three;
            if (parent != null)
            {
                if (root.Key.CompareTo(parent.Key) < 0)
                    parent.Left = Four;
                else
                    parent.Right = Four;
            }
            else
            {
                this.root = Four;
            }
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

        private void RightLeftRebalanceMutable(Node root, Node parent)
        {
            // stealing Wikipedia's diagram: (AVL tree)
            //   3              3
            //  / \            / \
            // A   5          A   4
            //    / \    =>      / \
            //   4   D          B   5
            //  / \                / \
            // B   C              C   D
            var Three = root;
            var Five = Three.Right;
            var Four = Five.Left;
            var C = Four.Right;
            Five.Left = C;
            Four.Right = Five;
            Three.Right = Four;
            RightRightRebalanceMutable(Three, parent);
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

        private void LeftRebalanceMutable(Node node, Node parent)
        {
            // node is heavy on the left, rotate right
            var bf = node.Left.BalanceFactor;
            if (bf == 1)
                LeftLeftRebalanceMutable(node, parent);
            else
                LeftRightRebalanceMutable(node, parent);
        }

        private static Node LeftLeftRebalance(Node root)
        {
            var newRightChild = root.Clone();
            newRightChild.Left = root.Left.Right;
            var newRoot = root.Left.Clone();
            newRoot.Right = newRightChild;
            return newRoot;
        }

        private void LeftLeftRebalanceMutable(Node root, Node parent)
        {
            // stealing Wikipedia's diagram: (AVL tree)
            //       5             4
            //      / \          /   \
            //     4   D        3     5
            //    / \      =>  / \   / \
            //   3   C        A   B C   D
            //  / \
            // A   B
            var Five = root;
            var Four = Five.Left;
            var C = Four.Right;
            Five.Left = C;
            Four.Right = Five;
            if (parent != null)
            {
                if (root.Key.CompareTo(parent.Key) < 0)
                    parent.Left = Four;
                else
                    parent.Right = Four;
            }
            else
            {
                this.root = Four;
            }
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

        private void LeftRightRebalanceMutable(Node root, Node parent)
        {
            // stealing Wikipedia's diagram: (AVL tree)
            //     5               5
            //    / \             / \
            //   3   D           4   D
            //  / \      =>     / \
            // A   4           3   C
            //    / \         / \
            //   B   C       A   B
            var Five = root;
            var Three = Five.Left;
            var Four = Three.Right;
            var B = Four.Left;
            Three.Right = B;
            Four.Left = Three;
            Five.Left = Four;
            LeftLeftRebalanceMutable(Five, parent);
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
