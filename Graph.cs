using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3GameLogic
{
    class LogicBlock
    {
        public LogicBlock(BlockNode start, BlockNode end)
        {
            Start = start;
            End = end;
        }

        internal BlockNode End { get; set; }
        internal BlockNode Start { get; set; }
    }
    class Cycle : LogicBlock
    {
        public Cycle(BlockNode start, BlockNode realStart, BlockNode realestStart, BlockNode end, BlockNode exit, List<BlockNode> LinearizedGraph, bool doWhile) : base(start, end)
        {
            Exit = exit;
            DoWhile = DoWhile;
            RealStart = realStart;
            RealestStart = realestStart;
            UpdateCycleFrom(LinearizedGraph);
        }

        private void UpdateCycleFrom(List<BlockNode> LinearizedGraph)
        {
            for (int i = Start.IndexInLinearizedGraph; i < Exit.IndexInLinearizedGraph; i++)
            {
                LinearizedGraph[i].CycleIn = this;
            }
        }

        internal BlockNode Exit { get; set; }
        public bool DoWhile { get; set; }
        internal BlockNode RealStart { get; set; }
        internal BlockNode RealestStart { get; set; }
    }
    class Else : LogicBlock
    {
        public Else(BlockNode start, BlockNode end, BlockNode exit, List<BlockNode> LinearizedGraph) : base(start, end)
        {
            Exit = exit;
            Start.ElseIn = this;
            End.ElseIn = this;
        }

        private void UpdateElseFrom(List<BlockNode> LinearizedGraph)
        {
            for (int i = Start.IndexInLinearizedGraph; i < Exit.IndexInLinearizedGraph; i++)
            {
                LinearizedGraph[i].ElseIn = this;
            }
        }

        internal BlockNode Exit { get; set; }
    }

    class BlockNode
    {
        public BlockNode(int start) : this(start, -1, 0, null, 0, null)
        {
        }

        public BlockNode(int start, int depth, BlockNode TopSub) : this(start, -1, 0, null, depth, TopSub)
        {
        }

        public BlockNode(int start, BlockNode father, int depth, BlockNode TopSub) : this(start, -1, 0, father, depth, TopSub)
        {
        }

        public BlockNode(int start, int end, BlockNode father, int depth, BlockNode TopSub) : this(start, end, 0, father, depth, TopSub)
        {
        }

        public BlockNode(int start, int end, int insideReferences, BlockNode father, int depth, BlockNode TopSub)
        {
            Father = father;
            Start = start;
            End = end;
            InsideReferences = insideReferences;
            Depth = depth;
            IndexInLinearizedGraph = 0;
            RReference = null;
            LReference = null;
            CycleIn = null;
            ElseIn = null;
            OpenParenthesisAt = new List<int>();
            CloseParenthesisAt = new List<int>();
            OpenParenthesisAtConnectedTo = new List<BlockNode>();
            GoToBlock = new List<BlockNode>();
            CloseParenthesisAtConnectedTo = new List<BlockNode>();
            TopOfSubGraph = TopSub;
            LChild = null;
            RChild = null;
            LOtherGraph = false;
            ROtherGraph = false;
            Branch = BranchType.NaB;
            OtherFathers = new List<BlockNode>();
        }

        public BlockNode MakeRChild(int blockCommand, int NextDeepness)
        {
            this.End = blockCommand - 1;
            this.RChild = new BlockNode(blockCommand, this, NextDeepness, this.TopOfSubGraph);
            return this.RChild;
        }

        public BlockNode MakeLChild(int Branch, int NextDeepness)
        {
            this.LChild = new BlockNode(Branch, this, NextDeepness, null);
            this.LChild.TopOfSubGraph = this.LChild;
            return this.LChild;
        }

        public void UpdateOpenParenthesis(BlockNode Destination, int Value)
        {
            this.OpenParenthesisAt.Add(Value);
            this.OpenParenthesisAtConnectedTo.Add(Destination);
        }

        public void UpdateCloseParenthesis(BlockNode Source, int Value)
        {
            this.CloseParenthesisAt.Add(Value);
            this.CloseParenthesisAtConnectedTo.Add(Source);
        }

        public int End { get; set; }
        public int InsideReferences { get; set; }
        public int Start { get; set; }
        internal BlockNode Father { get; set; }
        internal BlockNode LChild { get; set; }
        internal BlockNode RChild { get; set; }
        internal BranchType Branch { get; set; }
        public int Depth { get; set; }
        internal BlockNode RReference { get; set; }
        internal BlockNode LReference { get; set; }
        internal BlockNode TopOfSubGraph { get; set; }
        public bool LOtherGraph { get; set; }
        public bool ROtherGraph { get; set; }
        public List<int> OpenParenthesisAt { get; set; }
        public List<int> CloseParenthesisAt { get; set; }
        internal List<BlockNode> OtherFathers { get; set; }
        internal List<BlockNode> OpenParenthesisAtConnectedTo { get; set; }
        internal List<BlockNode> CloseParenthesisAtConnectedTo { get; set; }
        public int IndexInLinearizedGraph { get; set; }
        internal List<BlockNode> GoToBlock { get; set; }
        internal Cycle CycleIn { get; set; }
        internal Else ElseIn { get; set; }
    }
    class FullGraph
    {
        public FullGraph(BlockNode top)
        {
            Top = top;
            LinearizedGraph = new List<BlockNode>();
            Bottoms = new List<BlockNode>();
        }

        public void GetParenthesis()
        {
            MakeLinearizedGraph(Top);
            foreach (BlockNode Bottom in Bottoms)
            {
                BlockNode Destination = Bottom;
                //UpdateParenthesisTopBottom(Destination, Bottom.TopOfSubGraph);
                while (Destination != null && Destination.OtherFathers.Count() == 0)
                {
                    Destination = Destination.Father;
                }
                if (Destination != null)
                {
                    for (int i = 0; i < Destination.OtherFathers.Count(); i++)
                    {
                        BlockNode Objective = Destination;
                        BlockNode Source = Destination.OtherFathers[i];
                        BlockNode Common = Source;
                        while (Common != null && Common.TopOfSubGraph != Destination.TopOfSubGraph)
                            Common = Common.TopOfSubGraph.Father;
                        if (Common != null)
                        {
                            if (Common.Depth == Destination.Depth)
                            {
                                if (Common.IndexInLinearizedGraph >= Destination.IndexInLinearizedGraph && Common.CycleIn == Destination.CycleIn)
                                {
                                    bool DoWhile = true;
                                    if (Source.RReference != null)
                                    {
                                        while (Destination.LChild == null && Destination.LReference == null)
                                            Destination = Destination.RChild;
                                        DoWhile = false;
                                        if (Destination.RChild.IndexInLinearizedGraph > Source.IndexInLinearizedGraph && Destination.LChild != null)
                                            Destination = Destination.LChild;
                                        else
                                            Destination = Destination.RChild;
                                        UpdateParenthesisIfElseDoWhile(Common, Destination);
                                        if (Common.RChild != null)
                                            new Cycle(Destination.Father, Destination, Objective, Common, Common.RChild, LinearizedGraph, DoWhile);
                                        else
                                            new Cycle(Destination.Father, Destination, Objective, Common, Common.RReference, LinearizedGraph, DoWhile);
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (BlockNode Bottom in Bottoms)
            {
                BlockNode Destination = Bottom;
                UpdateParenthesisTopBottom(Destination, Bottom.TopOfSubGraph);
                while (Destination != null && Destination.OtherFathers.Count() == 0)
                {
                    Destination = Destination.Father;
                }
                if (Destination != null)
                {
                    for (int i = 0; i < Destination.OtherFathers.Count(); i++)
                    {
                        BlockNode Source = Destination.OtherFathers[i];
                        BlockNode Common = Source;
                        while (Common != null && Common.TopOfSubGraph != Destination.TopOfSubGraph)
                        {
                            Common = Common.TopOfSubGraph.Father;
                        }
                        if (Common == null)
                            Source.GoToBlock.Add(Destination);
                        else
                        {
                            if (Common.Depth == Destination.Father.Depth)
                            {
                                if (Common.CycleIn != Destination.Father.CycleIn)
                                {
                                    Source.GoToBlock.Add(Destination);
                                }
                                else if (Common.IndexInLinearizedGraph < Destination.IndexInLinearizedGraph)
                                {
                                    UpdateParenthesisIfElseDoWhile(Destination.Father, Common.RChild);
                                    if (Source != Common)
                                        new Else(Common.RChild, Destination.Father, Destination, LinearizedGraph);
                                }
                            }
                            else
                            {
                                Source.GoToBlock.Add(Destination);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateParenthesisIfElseDoWhile(BlockNode Destination, BlockNode Common)
        {
            UpdateDepthFrom(Destination, Common);
            if (Destination.RChild != null)
            {
                Common.UpdateOpenParenthesis(Destination.RChild, 0);
                Destination.RChild.UpdateCloseParenthesis(Common, 0);
            }
            else
            {
                Common.UpdateOpenParenthesis(Destination, 0);
                Destination.UpdateCloseParenthesis(Common, 4);
            }
        }

        private void UpdateParenthesisTopBottom(BlockNode Destination, BlockNode Common)
        {

            UpdateDepthFrom(Destination, Common);
            UpdateDepth(Common.LChild);
            Common.UpdateOpenParenthesis(Destination, 0);
            Destination.UpdateCloseParenthesis(Common, 4);
        }

        private void MakeLinearizedGraph(BlockNode TMP)
        {
            while (TMP != null)
            {
                LinearizedGraph.Add(TMP);
                TMP.IndexInLinearizedGraph = LinearizedGraph.Count - 1;
                TMP.Depth = 0;
                if (TMP.LChild != null)
                    MakeLinearizedGraph(TMP.LChild);
                TMP = TMP.RChild;
            }
        }

        private void UpdateDepth(BlockNode TMP)
        {
            while (TMP != null)
            {
                TMP.Depth++;
                if (TMP.LChild != null)
                    UpdateDepth(TMP.LChild);
                TMP = TMP.RChild;
            }
        }

        private void UpdateDepthFrom(BlockNode Destination, BlockNode Source)
        {
            if (Destination.RChild != null)
            {
                for (int i = Source.IndexInLinearizedGraph; i < Destination.RChild.IndexInLinearizedGraph; i++)
                {
                    LinearizedGraph[i].Depth++;
                }
            }
            else
            {
                for (int i = Source.IndexInLinearizedGraph; i <= Destination.IndexInLinearizedGraph; i++)
                {
                    LinearizedGraph[i].Depth++;
                }
            }
            UpdateDepth(Destination.LChild);
        }

        internal BlockNode Top { get; set; }
        internal List<BlockNode> Bottoms { get; set; }
        internal List<BlockNode> LinearizedGraph { get; set; }
    }
}
