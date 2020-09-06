// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using MDSDK.JPEG2000.Utils;
using System.Diagnostics;

namespace MDSDK.JPEG2000.CodestreamSyntax
{
    class TagTree
    {
        abstract class Node
        {
            public abstract bool TryGetValue(uint x, uint y, uint limit, BitReader input, out uint value);
        }

        class BranchNode : Node
        {
            public TagTree TagTree { get; }

            public int Level { get; }

            public Node[,] ChildNodes { get; } = new Node[2, 2];

            public uint MinimumOfChildNodeValues { get; private set; }

            public bool MinimumOfChildNodeValuesIsFullyDecoded { get; private set; }

            public BranchNode(TagTree tagTree, BranchNode parentNode, int level)
            {
                TagTree = tagTree;
                
                Level = level;
                
                if (parentNode != null)
                {
                    // Start with already decoded minimum
                    Debug.Assert(parentNode.MinimumOfChildNodeValuesIsFullyDecoded);
                    MinimumOfChildNodeValues = parentNode.MinimumOfChildNodeValues;
                }
            }

            public override bool TryGetValue(uint x, uint y, uint limit , BitReader input, out uint value)
            {
                while (!MinimumOfChildNodeValuesIsFullyDecoded && (MinimumOfChildNodeValues <= limit))
                {
                    if (input.ReadBit() == 0)
                    {
                        MinimumOfChildNodeValues++;
                    }
                    else
                    {
                        MinimumOfChildNodeValuesIsFullyDecoded = true;
                    }
                }

                if (MinimumOfChildNodeValues <= limit)
                {
                    var childBitIndex = TagTree.LeafNodeLevel - Level - 1;
                    var i = (x >> childBitIndex) & 1;
                    var j = (y >> childBitIndex) & 1;
                    var childNode = ChildNodes[i, j];
                    if (childNode == null)
                    {
                        childNode = TagTree.CreateNode(this, Level + 1);
                        ChildNodes[i, j] = childNode;
                    }
                    return childNode.TryGetValue(x, y, limit, input, out value);
                }
                else
                {
                    value = 0xFFFF;
                    return false;
                }
            }
        }

        class LeafNode : Node
        {
            public uint Value { get; set; }

            public bool ValueIsFullyDecoded { get; set; }

            public LeafNode(BranchNode parentNode)
            {
                if (parentNode != null)
                {
                    // Start with already decoded minimum
                    Debug.Assert(parentNode.MinimumOfChildNodeValuesIsFullyDecoded);
                    Value = parentNode.MinimumOfChildNodeValues;
                }
            }

            public override bool TryGetValue(uint x, uint y, uint limit, BitReader input, out uint value)
            {
                while (!ValueIsFullyDecoded && (Value <= limit))
                {
                    if (input.ReadBit() == 0)
                    {
                        Value++;
                    }
                    else
                    {
                        ValueIsFullyDecoded= true;
                    }
                }

                if (ValueIsFullyDecoded)
                {
                    value = Value;
                    return true;
                }
                else
                {
                    value = 0xFFFF;
                    return false;
                }
            }
        }

        public int LeafNodeLevel { get; }

        public TagTree(uint nCodeBlocksX, uint nCodeBlocksY)
        {
            while (((1U << LeafNodeLevel) < nCodeBlocksX) || ((1U << LeafNodeLevel) < nCodeBlocksY))
            {
                LeafNodeLevel++;
            }
        }

        private Node CreateNode(BranchNode parentNode, int level)
        {
            if (level == LeafNodeLevel)
            {
                return new LeafNode(parentNode);
            }
            else
            {
                return new BranchNode(this, parentNode, level);
            }
        }

        private Node _rootNode;

        public bool TryGetValue(uint x, uint y, uint limit, BitReader input, out uint value)
        {
            if (_rootNode == null)
            {
                _rootNode = CreateNode(null, 0);
            }
            
            return _rootNode.TryGetValue(x, y, limit, input, out value);
        }
    }
}
