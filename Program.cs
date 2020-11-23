using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//0x1198C10

namespace M3GameLogic //Decompile Game Logic Table
{
    enum BlockStatus { start, used, end, discovered };
    enum BranchType { NaB, DoWhile, While, If, Break, ToDefine, Else, GoTo }; //NaB = Not a Branch

    class PointerCouple
    {
        public int Pointers = 0, Logic = 0;
    }

    class Arguments {
        public byte[] Data;
        public int start;
        public int bank;
        public Block CurrBlock;
        public Block Block0;
        public List<string> tmp;
        public List<int> DataList;
        public int numblock;
        public bool[] visitedBlock;
        public List<int> returningStack;
        public Arguments(byte[] Data, int start, int bank, Block CurrBlock, Block Block0, List<string> tmp, List<int> DataList, int numblock, bool[] visitedBlock, List<int> returningStack) {
            this.Data = Data; this.start = start; this.bank = bank; this.CurrBlock = CurrBlock; this.Block0 = Block0; this.tmp = tmp; this.DataList = DataList; this.numblock = numblock; this.visitedBlock = visitedBlock; this.returningStack = returningStack;
        }
    }

    class Utilities
    {
        public static bool FromHex(string s, out int a)
        {
            a = 0;
            if (!s.StartsWith("0x"))
                return false;
            return int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a);
        }
        public static string ToHex(int a, int numbers)
        {
            string tmp = "";
            for (int i = 0; i < numbers; i++)
            {
                int tmp2 = (a >> ((numbers - 1 - i) * 4)) % 0x10;
                switch (tmp2)
                {
                    case 0xA:
                        tmp += "A";
                        break;
                    case 0xB:
                        tmp += "B";
                        break;
                    case 0xC:
                        tmp += "C";
                        break;
                    case 0xD:
                        tmp += "D";
                        break;
                    case 0xE:
                        tmp += "E";
                        break;
                    case 0xF:
                        tmp += "F";
                        break;
                    default:
                        tmp += tmp2.ToString();
                        break;
                };
            }
            return tmp;
        }
        public static int ToInt4Bytes(byte[] Data, int address) //Read bytes
        {
            return (Data[address]) + (Data[address + 1] << 8) + (Data[address + 2] << 16) + (Data[address + 3] << 24);
        }
        public static int ToInt3Ints(int I1, int I2, int I3) //Read ints
        {
            return (I1) + (I2 << 8) + (I3 << 16);
        }
        public static int ToInt3IntsSigned(int I1, int I2, int I3) //Read ints
        {
            return (((I1) + (I2 << 8) + (I3 << 16)) << 8) >> 8;

        }
        public static int ToInt2Ints(int I1, int I2) //Read ints
        {
            return (I1) + (I2 << 8);
        }
        public static int ToInt2IntsSigned(int I1, int I2) //Read ints
        {
            return (((I1) + (I2 << 8)) << 16) >> 16;

        }
        public static short ToInt2Bytes(byte[] Data, int address) //Read bytes
        {
            return (short)((Data[address]) + (Data[address + 1] << 8));
        }
        public static int ToInt3Bytes(byte[] Data, int address) //Read bytes
        {
            return (Data[address]) + (Data[address + 1] << 8) + (Data[address + 2] << 16);
        }
    }

    class SingleCommand
    {
        private int pointedEntry;
        public const int ImpossibleData = 0x1000000;

        public SingleCommand(int CT, int X, int Y, int Z)
        {
            CommandType = CT;
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            NumParam = GetParamsNum();
            Data = ImpossibleData; //Impossible default value
        }

        public SingleCommand(int CT, int X, int Y, int Z, Block Block0, Block CurrBlock, BlockNode Zone, FullGraph Graph, int[] Labels, int LabelsNum, Parser parser, List<int> dataValues, List<string> tmp) : this(CT, X, Y, Z)
        {
            this.LabelsNum = LabelsNum;
                Command = TranslateToText(Block0, CurrBlock, Zone, Graph, Labels, parser, dataValues, tmp);
        }
        
        override public String ToString()
        {
            if (Command == null)
            {
                return "[" + Utilities.ToHex(CommandType, 2) + " " + Utilities.ToHex(X, 2) + " " + Utilities.ToHex(Y, 2) + " " + Utilities.ToHex(Z, 2) + "]";
            }
            else return Command;
        }

        public static SingleCommand GetSingleCommand(Byte[] Data, int start, Block Block0, Block CurrBlock, Parser parser, List<int> dataList, List<string> tmp)
        {
            SingleCommand c = new SingleCommand(Data[start], Data[start + 1], Data[start + 2], Data[start + 3]);
            if (c.CommandType == 5)
                c.PointedEntry = Block0.Number[Utilities.ToInt2Ints(c.Y, c.Z)];
            if (c.CommandType == 7 || c.CommandType == 0xC || c.CommandType == 0xD)
                c.PointedEntry = CurrBlock.Number[Utilities.ToInt2Ints(c.Y, c.Z)];
            parser.TranslateToText(c, dataList, tmp);
            return c;
        }

        public static SingleCommand GetSingleCommand(Byte[] Data, int start, Block Block0, Block CurrBlock, List<int> DataList, List<String> tmp, BlockNode Zone, FullGraph Graph, int[] Labels, int LabelsNum, Parser parser)
        {
            SingleCommand c = new SingleCommand(Data[start], Data[start + 1], Data[start + 2], Data[start + 3], Block0, CurrBlock, Zone, Graph, Labels, LabelsNum, parser, DataList, tmp);
            return c;
        }

        private int GetParamsNum()
        {
            switch (CommandType)
            {
                case 3:
                    return 1;
                case 0xD:
                    return 1;
                case 4: // 04 extended codes
                    return ExtendedCodeParams.ExtCodeParams[Utilities.ToInt2Ints(Y, Z)];
                case 0xE: // 0E extended Math codes
                    switch (X)
                    {
                        case 0x00: //-$1
                            return 1;
                        case 0x1://$1+$2
                            return 2;
                        case 0x2://$2-$1
                            return 2;
                        case 0x3://$1*$2
                            return 2;
                        case 0x4://$2/$1
                            return 2;
                        case 0x5://$2%$1
                            return 2;
                        case 0x6://$1++
                            return 1;
                        case 0x7://$1--
                            return 1;
                        case 0x8://$1&$2
                            return 2;
                        case 0x9://$1|$2
                            return 2;
                        case 0xA:// If $1 == $2
                            return 2;
                        case 0xB:// If $1 != $2
                            return 2;
                        case 0xC:// If $1 > $2
                            return 2;
                        case 0xD:// If $1 < $2
                            return 2;
                        case 0xE:// If $1 >= $2
                            return 2;
                        case 0xF:// If $1 <= $2
                            return 2;
                        case 0x11:
                            return 1;
                        case 0x12:
                            return 1;
                    }
                    break;
                default:
                    break;
            }
            return 0;
        }

        private String TranslateToText(Block Block0, Block CurrBlock, BlockNode Zone, FullGraph Graph, int[] Labels, Parser parser, List<int> dataValues, List<string> tmp)
        {
            switch (CommandType)
            {
                case 0x5:
                    return "[JUMP TO 0-" + Block0.GraphAssociatedToBlock[Utilities.ToInt2Ints(Y, Z)] + ", " + X + "]";
                case 0x7:
                    return "[JUMP TO " + CurrBlock.BlockNum + "-" + CurrBlock.GraphAssociatedToBlock[Utilities.ToInt2Ints(Y, Z)] + ", " + X + "]";
                case 0xD:
                    NumParam = 1;
                    if (Zone.CycleIn != null && Zone.CycleIn.Start == Zone)
                    {
                        if (!Zone.CycleIn.DoWhile) 
                            return "(WHILE($1 == 0))";
                        //tmp.Add("while($1) ");
                    }
                    if (Zone.CycleIn != null && Zone.CycleIn.End == Zone)
                    {
                        if (Zone.CycleIn.DoWhile)
                            return "(WHILE($1 == 0));";
                        //tmp.Add("while($1) ");
                    }
                    if (Zone.LChild == null)
                    {
                        int Branchy = Utilities.ToInt2Ints(Y, Z);
                        if (Labels[Branchy] == -1)
                            Labels[Branchy] = LabelsNum++;
                        return "(IF $1 == 0, GO TO " + CurrBlock.BlockNum + "-" + CurrBlock.GraphAssociatedToBlock[Graph.Top.Start] + "-" + Labels[Branchy] + ")";
                    }
                    return "(IF $1 == 0)";
                case 0xC:
                    if (Zone.CycleIn != null && Zone.CycleIn.RealestStart.Start == Utilities.ToInt2Ints(Y, Z))
                        return "";
                    int Branch = Utilities.ToInt2Ints(Y, Z);
                    if (Labels[Branch] == -1)
                        Labels[Branch] = LabelsNum++;
                    return "[GO TO " + CurrBlock.BlockNum + "-" + CurrBlock.GraphAssociatedToBlock[Graph.Top.Start] + "-" + Labels[Branch] + "]";
                default:
                    return parser.TranslateToText(this, dataValues, tmp);
            }
        }

        public int CommandType { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string Command { get; set; }
        public int NumParam { get; set; }
        public int Data { get; set; }
        public int LabelsNum { get; set; }
        public int PointedEntry { get => pointedEntry; set => pointedEntry = value; }
    }
    class ExtendedCodeParams
    {
        public static int[] ExtCodeParams { get; set; }
    }
    class Block
    {
        private const int MAXBLOCKS = 0x4000;
        private int blockBeginning;
        private bool[] mainCaller;
        private bool[] special;
        private bool[] fifteenth_bit;

        public Block(int blockBeginning) : this(0, blockBeginning)
        {
        }
        public Block(int num, int blockBeginning)
        {
            BlockBeginning = blockBeginning;
            EntryNum = 0;
            BlockNum = num;
            Status = new BlockStatus[MAXBLOCKS];
            Number = new int[MAXBLOCKS];
            CommandNum = new int[MAXBLOCKS];
            ReferencesTo = new int[MAXBLOCKS];
            GraphAssociatedToBlock = new int[MAXBLOCKS];
            BlockAssociatedToGraph = new int[MAXBLOCKS];
            ExternalFunction = new bool[MAXBLOCKS];
            Special = new bool[MAXBLOCKS];
            Fifteenth_bit = new bool[MAXBLOCKS];
            MainCaller = new bool[MAXBLOCKS];
            for (int i = 0; i < MAXBLOCKS; i++)
            {
                Status[i] = BlockStatus.end;
                GraphAssociatedToBlock[i] = -1;
            }
        }
        public void UpdateToDiscovered(int blockCommand)
        {
            Status[blockCommand] = BlockStatus.discovered;
            Number[blockCommand] = EntryNum;
            ReferencesTo[blockCommand]++;
            CommandNum[EntryNum] = blockCommand;
            MainCaller[EntryNum] = false;
            Special[EntryNum] = false;
            Fifteenth_bit[EntryNum] = false;
            EntryNum++;
        }
        public void UpdateToDiscovered(int blockCommand, bool main)
        {
            bool tmp = false, tmp15 = false;
            if (blockCommand < 0)
            {
                blockCommand = blockCommand & 0x7FFF;
                tmp = true;
            }
            if ((blockCommand & 0x4000) != 0)
            {
                blockCommand = blockCommand & 0x3FFF;
                tmp15 = true;
            }
            UpdateToDiscovered(blockCommand);
            MainCaller[EntryNum - 1] = main;
            Special[EntryNum - 1] = tmp;
            Fifteenth_bit[EntryNum - 1] = tmp15;
        }

        public void CheckBranch(int Branch, int Subptr, byte[] Data)
        {
            if (this.Status[Branch] != BlockStatus.start && this.Status[Branch] != BlockStatus.discovered)
            {
                if (this.Status[Branch] == BlockStatus.used || (Branch != 0 && (this.Status[Branch - 1] == BlockStatus.used || (this.Status[Branch - 1] == BlockStatus.start && (Data[Subptr - 4] != 0xC && Data[Subptr - 4] != 0x8 && Data[Subptr - 4] != 0x6 && Data[Subptr - 4] != 0x9)))))
                    this.ReferencesTo[Branch]++;
                this.UpdateToDiscovered(Branch);
            }
            else
                this.ReferencesTo[Branch]++;
        }

        public int[] ReferencesTo { get; set; }
        internal BlockStatus[] Status { get; set; }
        public int[] Number { get; set; }
        public int[] CommandNum { get; set; }
        public int EntryNum { get; set; }
        public int BlockNum { get; set; }
        public bool[] ExternalFunction { get; set; }
        public int[] GraphAssociatedToBlock { get; set; }
        public int[] BlockAssociatedToGraph { get; set; }
        public bool[] MainCaller { get => mainCaller; set => mainCaller = value; }
        public bool[] Special { get => special; set => special = value; }
        public bool[] Fifteenth_bit { get => fifteenth_bit; set => fifteenth_bit = value; }
        public int BlockBeginning { get => blockBeginning; set => blockBeginning = value; }
    }

    class Program
    {
        static void PreProcessSingleEntry(byte[] Data, Block Block0, Block CurrBlock, int CurrEntryNum, int Block0Logic, int EntryLogic)
        {
            int pointer = (CurrBlock.CommandNum[CurrEntryNum] * 4) + EntryLogic;
            int blockCommand = CurrBlock.CommandNum[CurrEntryNum];
            if (CurrBlock.Status[blockCommand] == BlockStatus.start)
                return;
            if (CurrBlock.Status[blockCommand] == BlockStatus.used || CurrBlock.Status[blockCommand] == BlockStatus.end)
                CurrBlock.ReferencesTo[blockCommand]++;
            if (blockCommand != 0 && CurrBlock.Status[blockCommand - 1] != BlockStatus.start && CurrBlock.Status[blockCommand - 1] != BlockStatus.discovered)
                CurrBlock.Status[blockCommand - 1] = BlockStatus.end;
            CurrBlock.Status[blockCommand] = BlockStatus.start;
            while (Data[pointer] != 0x9 && Data[pointer] != 0x6 && Data[pointer] != 0x8 && Data[pointer] != 0xC && (blockCommand == CurrBlock.CommandNum[CurrEntryNum] || (CurrBlock.Status[blockCommand] != BlockStatus.start && CurrBlock.Status[blockCommand] != BlockStatus.discovered))) //Termination codes
            {
                if (blockCommand != CurrBlock.CommandNum[CurrEntryNum])
                    if (CurrBlock.Status[blockCommand] != BlockStatus.used)
                        CurrBlock.Status[blockCommand] = BlockStatus.used;
                pointer += 4;
                blockCommand++;
            }
            if (CurrBlock.Status[blockCommand] != BlockStatus.start && CurrBlock.Status[blockCommand] != BlockStatus.discovered)
                CurrBlock.Status[blockCommand] = BlockStatus.end;
            else
                CurrBlock.ReferencesTo[blockCommand]++;
            blockCommand = CurrBlock.CommandNum[CurrEntryNum];
            pointer = (CurrBlock.CommandNum[CurrEntryNum] * 4) + EntryLogic;
            while (Data[pointer] != 0x9 && Data[pointer] != 0x6 && Data[pointer] != 0x8 && (blockCommand == CurrBlock.CommandNum[CurrEntryNum] || (CurrBlock.Status[blockCommand] != BlockStatus.start && CurrBlock.Status[blockCommand] != BlockStatus.discovered))) //Termination codes
            {
                if (Data[pointer] == 0xC || Data[pointer] == 0xD || Data[pointer] == 0x7 || Data[pointer] == 0x5)
                {
                    int Branch = Utilities.ToInt2Bytes(Data, pointer + 2);
                    if (Data[pointer] == 0x5)
                    {
                        int Subptr = (4 * Branch) + Block0Logic;
                        Block0.CheckBranch(Branch, Subptr, Data);
                        Block0.ExternalFunction[Branch] = true;
                    }
                    else
                    {
                        int Subptr = (4 * Branch) + EntryLogic;
                        CurrBlock.CheckBranch(Branch, Subptr, Data);
                        if (Data[pointer] == 0x7)
                            CurrBlock.ExternalFunction[Branch] = true;
                        if (Data[pointer] == 0xC)
                        {
                            break;
                        }
                    }
                }
                blockCommand++;
                pointer += 4;
            }
        }

        static FullGraph GraphOfEntry(byte[] Data, Block CurrBlock, int CurrEntryNum, int EntryLogic)
        {
            int pointer = (CurrBlock.CommandNum[CurrEntryNum] * 4) + EntryLogic;
            int blockCommand = CurrBlock.CommandNum[CurrEntryNum];
            int startBlockCommand = blockCommand;
            BlockNode[] CorrespondingNode = new BlockNode[0x10000];
            FullGraph Graph = new FullGraph(new BlockNode(blockCommand));
            int NextDepth = 0;
            BlockNode ExecutionGraph = Graph.Top;
            ExecutionGraph.InsideReferences++;
            ExecutionGraph.TopOfSubGraph = ExecutionGraph;
            do
            {
                bool MakeRChild = false;
                while (Data[pointer] != 0x9 && Data[pointer] != 0x6 && Data[pointer] != 0x8 && CorrespondingNode[blockCommand] == null)
                {
                    //Create Tree
                    if ((CurrBlock.Status[blockCommand] == BlockStatus.start || MakeRChild) && ExecutionGraph.Start != blockCommand)
                    {
                        ExecutionGraph = ExecutionGraph.MakeRChild(blockCommand, NextDepth);
                        ExecutionGraph.InsideReferences++;
                        MakeRChild = false;
                    }
                    CorrespondingNode[blockCommand] = ExecutionGraph;
                    if (Data[pointer] == 0xD)
                    {
                        int Branch = Utilities.ToInt2Ints(Data[pointer + 2], Data[pointer + 3]);
                        if (ExecutionGraph.Start != blockCommand)
                        {
                            ExecutionGraph = ExecutionGraph.MakeRChild(blockCommand, NextDepth);
                            CorrespondingNode[blockCommand] = ExecutionGraph;
                        }
                        MakeRChild = true; // We need to make a new RChild to avoid issues with if/else/while-s
                        BlockNode TMP = ExecutionGraph;
                        while (TMP != null && CorrespondingNode[Branch] != null && TMP.TopOfSubGraph != CorrespondingNode[Branch].TopOfSubGraph)
                        {
                            TMP = TMP.TopOfSubGraph.Father;
                        }
                        if (CorrespondingNode[Branch] != null && TMP != null)
                        {
                            //We have a do while! We need to proceed accordingly
                            NextDepth--;
                            CorrespondingNode[Branch].InsideReferences++;
                            ExecutionGraph.LReference = CorrespondingNode[Branch];
                            CorrespondingNode[Branch].OtherFathers.Add(ExecutionGraph);
                        }
                        else
                        {
                            ExecutionGraph.MakeLChild(Branch, NextDepth);
                            ExecutionGraph.LChild.InsideReferences++;
                            NextDepth++;
                        }
                    }
                    if (Data[pointer] == 0xC)
                    {
                        int Branch = Utilities.ToInt2Ints(Data[pointer + 2], Data[pointer + 3]);
                        if (ExecutionGraph.Start != blockCommand)
                        {
                            ExecutionGraph = ExecutionGraph.MakeRChild(blockCommand, NextDepth);
                            CorrespondingNode[blockCommand] = ExecutionGraph;
                        }
                        if (CorrespondingNode[Branch] == null)
                        {
                            ExecutionGraph.End = blockCommand;
                            blockCommand = Branch - 1; //Follow the 0xC execution
                            pointer = (blockCommand * 4) + EntryLogic;
                            if (Data[pointer + 4] == 0x9 || Data[pointer + 4] == 0x6 || Data[pointer + 4] == 0x8)
                            {
                                ExecutionGraph.MakeRChild(blockCommand + 1, NextDepth);
                                ExecutionGraph = ExecutionGraph.RChild;
                                ExecutionGraph.InsideReferences++;
                            }
                        }
                        else
                        {
                            BlockNode TMPtmp = ExecutionGraph;
                            while (TMPtmp != null && TMPtmp.TopOfSubGraph != CorrespondingNode[Branch].TopOfSubGraph)
                                TMPtmp = TMPtmp.TopOfSubGraph.Father;
                            while (TMPtmp != null && TMPtmp.TopOfSubGraph == CorrespondingNode[Branch].TopOfSubGraph && TMPtmp != CorrespondingNode[Branch])
                                TMPtmp = TMPtmp.Father;
                            //Do we have a while?
                            if (TMPtmp != null && TMPtmp.TopOfSubGraph == CorrespondingNode[Branch].TopOfSubGraph) //If we have a while, de-tree-alize the branch forward
                            {
                                ExecutionGraph.End = blockCommand;
                                BlockNode TMP = CorrespondingNode[Branch];
                                TMP.OtherFathers.Add(ExecutionGraph);
                                TMP.InsideReferences++;
                                while (TMP.LChild == null && TMP.LReference == null)
                                    TMP = TMP.RChild;
                                if (TMP.LChild != null)
                                {
                                    ExecutionGraph.RReference = TMP.LChild;
                                    TMP.LChild.Father = ExecutionGraph;
                                    TMP.LChild.TopOfSubGraph = ExecutionGraph.TopOfSubGraph;
                                    TMP.LReference = ExecutionGraph.RReference;
                                    TMP.LChild = null;
                                    ExecutionGraph.RReference.OtherFathers.Add(TMP);
                                }
                                else
                                {
                                    ExecutionGraph.RReference = TMP.LReference;
                                }
                            }
                            else
                            {
                                ExecutionGraph.RReference = CorrespondingNode[Branch];
                                ExecutionGraph.RReference.InsideReferences++;
                            }
                            ExecutionGraph.RChild = ExecutionGraph.RReference;
                            blockCommand = ExecutionGraph.Start - 1;
                            pointer = (blockCommand * 4) + EntryLogic;
                        }
                    }
                    blockCommand++;
                    pointer += 4;
                }
                if (CorrespondingNode[blockCommand] == null) //New end
                {
                    if ((CurrBlock.Status[blockCommand] == BlockStatus.start || MakeRChild) && ExecutionGraph.Start != blockCommand)
                    {
                        ExecutionGraph = ExecutionGraph.MakeRChild(blockCommand, NextDepth);
                        ExecutionGraph.InsideReferences++;
                    }
                    ExecutionGraph.End = blockCommand;
                    CorrespondingNode[blockCommand] = ExecutionGraph;
                    Graph.Bottoms.Add(ExecutionGraph);
                }
                else if ((ExecutionGraph.Father != null) && ExecutionGraph.Father.LChild == ExecutionGraph && ExecutionGraph.Father.LChild.Start == CorrespondingNode[blockCommand].Start) //Reference the old endings
                {
                    ExecutionGraph.Father.LReference = CorrespondingNode[blockCommand];
                    CorrespondingNode[blockCommand].OtherFathers.Add(ExecutionGraph.Father);
                    if (ExecutionGraph != CorrespondingNode[blockCommand])
                    {
                        ExecutionGraph.Father.LReference.InsideReferences++;
                    }
                    ExecutionGraph.Father.LChild = null;
                }
                else
                {
                    CorrespondingNode[blockCommand].OtherFathers.Add(ExecutionGraph);
                    ExecutionGraph.RChild = null;
                }
                //Do something with the bottoms
                BlockNode prev = ExecutionGraph;
                while (ExecutionGraph != null && (ExecutionGraph.LChild == null || ExecutionGraph.LChild == prev))
                {
                    prev = ExecutionGraph;
                    ExecutionGraph = ExecutionGraph.Father;
                }
                if (ExecutionGraph != null)
                {
                    ExecutionGraph = ExecutionGraph.LChild;
                    NextDepth = ExecutionGraph.Depth;
                    blockCommand = ExecutionGraph.Start;
                    pointer = (blockCommand * 4) + EntryLogic;
                }
            } while (ExecutionGraph != null);

            /*for (int i = 0; i < CurrBlock.EntryNum; i++) //DEBUG CODE
            {
                int block = CurrBlock.CommandNum[i];
                if (CorrespondingNode[block] != null)
                {
                    int NumRef = CurrBlock.ReferencesTo[block];
                    if (CorrespondingNode[block].InsideReferences == NumRef || CorrespondingNode[block] == Graph.Top)
                    {
                    }
                    else if (CorrespondingNode[block].InsideReferences > NumRef)
                    {
                        i = i;
                        System.Console.WriteLine(CorrespondingNode[block].Start + " " + i);
                    }
                    else
                    {
                        if (CorrespondingNode[block].Father != null && CorrespondingNode[block].Father.RChild == CorrespondingNode[block])
                        {
                            CorrespondingNode[block].Father.ROtherGraph = true;
                            CorrespondingNode[block].Father.RChild = null;
                        }
                        else if (CorrespondingNode[block].Father != null)
                        {
                            CorrespondingNode[block].Father.LOtherGraph = true;
                            CorrespondingNode[block].Father.LChild = null;
                        }
                        CorrespondingNode[block].Father = null;
                        for (int j = 0; j < Graph.Bottoms.Count; j++)
                        {
                            BlockNode TMP = Graph.Bottoms[j];
                            while (TMP != null && TMP.TopOfSubGraph != Graph.Top.TopOfSubGraph && TMP.TopOfSubGraph != CorrespondingNode[block].TopOfSubGraph)
                                TMP = TMP.TopOfSubGraph.Father;
                            if (TMP != null && TMP.TopOfSubGraph == CorrespondingNode[block].TopOfSubGraph)
                            {
                                while (TMP.Father != null && TMP.TopOfSubGraph == CorrespondingNode[block].TopOfSubGraph)
                                {
                                    TMP = TMP.Father;
                                }
                                if (TMP.Father == null && TMP != Graph.Top)
                                {
                                    Graph.Bottoms.RemoveAt(j);
                                }
                            }
                        }
                    }
                }
            }*/

            return Graph;
        }

        static List<FullGraph> PreProcessEntries(byte[] Data, ref PointerCouple entry, ref PointerCouple block0, Block Block0, Block CurrBlock, bool Linear)
        {
            int entries = Utilities.ToInt2Bytes(Data, entry.Pointers);
            for (int i=0; i<entries; i++) //Separate the two, so there won't be repeated things in the future
            {
                CurrBlock.UpdateToDiscovered(Utilities.ToInt2Bytes(Data, entry.Pointers + 2 * (i + 1)), true);
            }
            for (int i = 0; i < CurrBlock.EntryNum; i++)
            {
                PreProcessSingleEntry(Data, Block0, CurrBlock, i, block0.Logic, entry.Logic);
            }
            int[] DifferentTopReferences = new int[CurrBlock.EntryNum];
            List<FullGraph> CurrBlockGraph = new List<FullGraph>();
            if (!Linear)
            {
                for (int i = 0; i < CurrBlock.EntryNum; i++)
                {
                    bool Valid = (i < entries) ? true : false;
                    if (Valid || CurrBlock.ExternalFunction[CurrBlock.CommandNum[i]])
                    {
                        CurrBlockGraph.Add(GraphOfEntry(Data, CurrBlock, i, entry.Logic));
                        CurrBlockGraph[CurrBlockGraph.Count - 1].GetParenthesis();
                        CurrBlock.GraphAssociatedToBlock[CurrBlock.CommandNum[i]] = CurrBlockGraph.Count - 1;
                        CurrBlock.BlockAssociatedToGraph[CurrBlockGraph.Count -1] = CurrBlock.CommandNum[i];
                    }
                }
            }
            return CurrBlockGraph;
        }

        static string[] ReadGameLogicEntries(byte[] Data, PointerCouple CurrentBlock, Block CurrBlock, Block Block0, List<FullGraph> Tree, bool Linear, bool Expanded, ResourcesList resList)
        {
            if (Linear)
                return ReadGameLogicEntries(Data, CurrentBlock, CurrBlock, Block0, Expanded, resList);
            return ReadGameLogicEntries(Data, CurrentBlock, CurrBlock, Block0, Tree, resList);
        }

        static string[] ReadGameLogicEntries(byte[] Data, PointerCouple CurrentBlock, Block CurrBlock, Block Block0, bool Expanded, ResourcesList resList)
        {
            int NumElem = CurrBlock.EntryNum;
            List<String> ReadGameLogic = new List<string>();
            bool found = false;
            for (int i = 0; i < NumElem; i++)
            {
                if (CurrBlock.BlockNum == 0)
                {
                    if (!found || CurrBlock.MainCaller[i])
                    {
                        if (CurrBlock.MainCaller[i])
                            found = true;
                        if (Expanded)
                        {
                            List<Arguments> argsStack = new List<Arguments>();
                            string basis = (CurrBlock.MainCaller[i] ? "C-" : "") + (CurrBlock.Special[i] ? "S-" : "") + (CurrBlock.Fifteenth_bit[i] ? "Fif-" : "") + CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                            do
                            {
                                if (argsStack.Count == 0)
                                    ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, argsStack, resList));
                                else
                                {
                                    Arguments tmpArg = argsStack[argsStack.Count - 1];
                                    argsStack.RemoveAt(argsStack.Count - 1);
                                    ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(tmpArg, argsStack, resList));
                                }
                            }
                            while (argsStack.Count > 0);
                        }
                        else
                        {
                            string basis = (CurrBlock.MainCaller[i] ? "C-" : "") + (CurrBlock.Special[i] ? "S-" : "") + (CurrBlock.Fifteenth_bit[i] ? "Fif-" : "") + CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                            ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, null, resList));
                        }
                    }
                    else
                    {
                        string basis = CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                        ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, null, resList));
                    }
                }
                else
                {
                    if (CurrBlock.MainCaller[i])
                    {
                        if (Expanded)
                        {

                            List<Arguments> argsStack = new List<Arguments>();
                            string basis = (CurrBlock.MainCaller[i] ? "C-" : "") + (CurrBlock.Special[i] ? "S-" : "") + (CurrBlock.Fifteenth_bit[i] ? "Fif-" : "") + CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                            do
                            {
                                if (argsStack.Count == 0)
                                    ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, argsStack, resList));
                                else
                                {
                                    Arguments tmpArg = argsStack[argsStack.Count - 1];
                                    argsStack.RemoveAt(argsStack.Count - 1);
                                    ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(tmpArg, argsStack, resList));
                                }
                            }
                            while (argsStack.Count > 0);
                        }
                        else
                        {
                            string basis = (CurrBlock.MainCaller[i] ? "C-" : "") + (CurrBlock.Special[i] ? "S-" : "") + (CurrBlock.Fifteenth_bit[i] ? "Fif-" : "") + CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                            ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, null, resList));
                        }
                    }
                    else
                    {
                        string basis = CurrBlock.BlockNum.ToString() + "-" + i.ToString() + ": ";
                        ReadGameLogic.Add(basis + ReadGameLogicSingleEntry(Data, (CurrBlock.CommandNum[i] * 4) + CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, null, resList));
                    }
                }
            }
            return ReadGameLogic.ToArray();
        }

        static string[] ReadGameLogicEntries(byte[] Data, PointerCouple CurrentBlock, Block CurrBlock, Block Block0, List<FullGraph> Graph, ResourcesList resList)
        {
            int NumElem = Graph.Count;
            string[] ReadGameLogic = new string[NumElem];
            for (int i = 0; i < NumElem; i++)
            {
                ReadGameLogic[i] = CurrBlock.BlockNum + "-" + i + ":" + Environment.NewLine;
                ReadGameLogic[i] += ReadGameLogicSingleEntry(Data, CurrentBlock.Logic, CurrBlock.BlockNum, i, CurrBlock, Block0, Graph[i].Top, Graph[i], resList);
            }
            return ReadGameLogic;
        }

        static string ReadGameLogicSingleEntry(byte[] Data, int start, int bank, int num, Block CurrBlock, Block Block0, List<Arguments> argsStack, ResourcesList resList)
        {
            List<string> tmp = new List<string>();
            List<int> DataList = new List<int>();
            Parser parser = new Parser(Block0, CurrBlock, resList);
            bool[] visitedBlock = new bool[CurrBlock.EntryNum];
            for (int i = 0; i < visitedBlock.Length; i++)
                visitedBlock[i] = false;
            int numblock = CurrBlock.CommandNum[num];
            List<int> returningStack = new List<int>();
            if (argsStack == null)
                returningStack = null;
            return ReadGameLogicSingleEntry(Data, start, bank, CurrBlock, Block0, tmp, DataList, numblock, visitedBlock, returningStack, argsStack, parser);
        }

        static string ReadGameLogicSingleEntry(Arguments arguments, List<Arguments> argsStack, ResourcesList resList)
        { return ReadGameLogicSingleEntry(arguments.Data, arguments.start, arguments.bank, arguments.CurrBlock, arguments.Block0, arguments.tmp, arguments.DataList, arguments.numblock, arguments.visitedBlock, arguments.returningStack, argsStack, new Parser(arguments.Block0, arguments.CurrBlock, resList)); }

        static string ReadGameLogicSingleEntry(byte[] Data, int start, int bank, Block CurrBlock, Block Block0, List<string>tmp, List<int> DataList, int numblock, bool[] visitedBlock, List<int> returningStack, List<Arguments> argsStack, Parser parser)
        {
            bool end = false;
            int tmpNumBlock;
            int tmpStart;
            do
            {
                if (CurrBlock.Status[numblock] == BlockStatus.start)
                    visitedBlock[CurrBlock.Number[numblock]] = true;
                SingleCommand c = SingleCommand.GetSingleCommand(Data, start, Block0, CurrBlock, parser, DataList, tmp);
                if (argsStack != null)
                    if (c.CommandType == 0xD || c.CommandType == 0xC || c.CommandType == 8 || c.CommandType == 7)
                    {
                        tmp.Remove(tmp[tmp.Count - 1]);
                        DataList.Remove(DataList[DataList.Count - 1]);
                    }
                switch (c.CommandType)
                {
                    case 0x7:
                        if (!visitedBlock[CurrBlock.Number[CurrBlock.CommandNum[c.PointedEntry]]])
                        {
                            if (returningStack != null)
                            {
                                returningStack.Add(numblock);
                                numblock = CurrBlock.CommandNum[c.PointedEntry] - 1;
                                start = (numblock * 4) + CurrBlock.BlockBeginning;
                            }
                        }
                        break;
                    case 0x8:
                        if (returningStack != null)
                        {
                            numblock = returningStack[returningStack.Count - 1];
                            returningStack.RemoveAt(returningStack.Count - 1);
                            start = (numblock * 4) + CurrBlock.BlockBeginning;
                        }
                        else
                            end = true;
                        break;
                    case 0xD:
                        if (!visitedBlock[CurrBlock.Number[CurrBlock.CommandNum[c.PointedEntry]]])
                        {
                            if (argsStack != null)
                            {
                                tmpNumBlock = CurrBlock.CommandNum[c.PointedEntry];
                                visitedBlock[CurrBlock.Number[tmpNumBlock]] = true;
                                tmpStart = (tmpNumBlock * 4) + CurrBlock.BlockBeginning;
                                argsStack.Add(new Arguments(Data, tmpStart, bank, CurrBlock, Block0, tmp.ToList(), DataList.ToList(), tmpNumBlock, visitedBlock, returningStack.ToList()));
                            }
                        }
                        break;
                    case 5:
                        break;
                    case 6:
                        end = true;
                        break;
                    case 0xC:
                        if (argsStack != null)
                        {
                            if (visitedBlock[CurrBlock.Number[CurrBlock.CommandNum[c.PointedEntry]]])
                            {
                                end = true;
                                tmp.Add("[END]");
                                DataList.Add(SingleCommand.ImpossibleData);
                            }
                            else
                            {
                                numblock = CurrBlock.CommandNum[c.PointedEntry] - 1;
                                start = (numblock * 4) + CurrBlock.BlockBeginning;
                            }
                        }
                        else
                            end = true;
                        break;
                    case 9:
                        end = true;
                        break;
                    default:
                        break;
                }
                numblock++;
                start += 4;
            }
            while (!end && (argsStack != null || CurrBlock.Status[numblock] != BlockStatus.start));
            string tmp2 = "";
            for (int i = 0; i < tmp.Count; i++)
                tmp2 += tmp[i];
            if (argsStack == null && !end && CurrBlock.Status[numblock] == BlockStatus.start)
                tmp2 += "[DO " + CurrBlock.BlockNum + "-" + CurrBlock.Number[numblock] + "]";
            return tmp2;
        }

        static string ReadGameLogicSingleEntry(byte[] Data, int Pointer, int bank, int num, Block CurrBlock, Block Block0, BlockNode Zone, FullGraph Graph, ResourcesList resList)
        {
            List<string> tmp = new List<string>();
            List<int> DataList = new List<int>();
            Parser parser = new Parser(Block0, CurrBlock, resList);
            int[] Labels = new int[0x10000];
            for (int i = 0; i < 0x10000; i++)
                Labels[i] = -1;
            int LabelsNum = 0;
            do
            {
                for (int i = 0; i < Zone.CloseParenthesisAt.Count; i++)
                {
                    if (Zone.CloseParenthesisAt[i] == 0)
                    {
                        tmp.Add("}" + Environment.NewLine);
                        break;
                    }
                }
                int numblock = Zone.Start;
                int start = (numblock * 4) + Pointer;
                if (Zone.OtherFathers.Count != 0)
                {
                    if(Labels[numblock] == -1)
                        Labels[numblock] = LabelsNum++;
                    tmp.Add(CurrBlock.BlockNum + "-" + num + "-" + Labels[numblock] + ":" + Environment.NewLine);
                }
                if (Zone.ElseIn != null && Zone.ElseIn.Start == Zone)
                {
                    tmp.Add("ELSE ");
                }
                if (Zone.LChild != null)
                {
                    if (!Zone.LChild.OpenParenthesisAt.Contains(0))
                    {
                        Zone.OpenParenthesisAt.Add(4);
                        BlockNode TMP = Zone;
                        while (TMP.RChild == null)
                            TMP = TMP.Father;
                        TMP.RChild.CloseParenthesisAt.Add(0);
                    }
                }
                for (int i = 0; i < Zone.OpenParenthesisAt.Count; i++)
                {
                    if (Zone.OpenParenthesisAt[i] == 0)
                    {
                        tmp.Add("{" + Environment.NewLine);
                        break;
                    }
                }
                while (numblock <= Zone.End)
                {
                    SingleCommand c = SingleCommand.GetSingleCommand(Data, start, Block0, CurrBlock, DataList, tmp, Zone, Graph, Labels, LabelsNum, parser);
                    LabelsNum = c.LabelsNum;
                    numblock++;
                    start += 4;
                }
                for (int i = 0; i < Zone.CloseParenthesisAt.Count; i++)
                {
                    if (Zone.CloseParenthesisAt[i] == 4)
                    {
                        tmp.Add("}" + Environment.NewLine);
                        break;
                    }
                }
                for (int i = 0; i < Zone.OpenParenthesisAt.Count; i++)
                {
                    if (Zone.OpenParenthesisAt[i] == 4)
                    {
                        tmp.Add("{" + Environment.NewLine);
                        break;
                    }
                }
                if (Zone.GoToBlock.Count != 0)
                {
                    if (Zone.CycleIn == null || (Zone.GoToBlock[0] != Zone.CycleIn.RealestStart))
                    {
                        if (Labels[Zone.GoToBlock[0].Start] == -1)
                            Labels[Zone.GoToBlock[0].Start] = LabelsNum++;
                        tmp.Add("[GOTO " + CurrBlock.BlockNum + "-" + num + "-" + Labels[Zone.GoToBlock[0].Start] + "]" + Environment.NewLine);
                    }
                }
                if (Zone.LChild != null)
                    Zone = Zone.LChild;
                else if (Zone.RChild != null)
                    Zone = Zone.RChild;
                else
                {
                    Zone = Zone.TopOfSubGraph.Father;
                    if (Zone != null && Zone != Graph.Top)
                        Zone = Zone.RChild;
                }
            }
            while (Zone != null && Zone != Graph.Top);
            string tmp2 = "";
            for (int i = 0; i < tmp.Count; i++)
                tmp2 += tmp[i] + (tmp[i].EndsWith(Environment.NewLine) ? "" : Environment.NewLine);
            return tmp2;
        }

        static int Main(string[] args) //Decompile only for now
        {
            if (args.Count() < 1)
            {
                Console.WriteLine("Write the path to the ROM!\n");
                return -2;
            }
            byte[] Data = File.ReadAllBytes(args[0]);
            const int paramAddress = 0xD2D658;
            int[] extendedCodeParameters = new int[256];
            for (int i = 0; i < 256; i++)
            {
                extendedCodeParameters[i] = Utilities.ToInt4Bytes(Data, paramAddress + (i * 4));
            }
            ResourcesList resList = ResourcesList.readResources();
            ExtendedCodeParams.ExtCodeParams = extendedCodeParameters;
            const int baseaddress = 0x1198C10;
            int numentries = Utilities.ToInt4Bytes(Data, baseaddress)/2;
            PointerCouple[] entriespointers = new PointerCouple[numentries + 1];
            List<string>[] ConvertedEntries= new List<string>[numentries];
            entriespointers[0] = new PointerCouple();
            entriespointers[0].Pointers = Utilities.ToInt4Bytes(Data, baseaddress + 4) + baseaddress;
            entriespointers[0].Logic = Utilities.ToInt4Bytes(Data, baseaddress + 8) + baseaddress;
            Block Block0 = new Block(entriespointers[0].Logic);
            bool Linear = true;
            bool Expanded = false;
            for (int i = 0; i < args.Count() - 1; i++)
            {
                if (args[1 + i].Equals("g"))
                    Linear = false;
                if (args[1 + i].Equals("e"))
                    Expanded = true;
            }
            for (int i = 1; i < numentries + 1; i++)
            {
                entriespointers[i] = new PointerCouple();
                entriespointers[i].Pointers = Utilities.ToInt4Bytes(Data, baseaddress + 4 + (i * 8)) + baseaddress;
                entriespointers[i].Logic = Utilities.ToInt4Bytes(Data, baseaddress + 8 + (i * 8)) + baseaddress;
                Block CurrBlock;
                if (i != numentries)
                    CurrBlock = new Block(i, entriespointers[i].Logic);
                else
                {
                    CurrBlock = Block0;
                    i = 0;
                }
                ConvertedEntries[i] = new List<string>();
                ConvertedEntries[i].Add("[BLOCK " + i.ToString() + "] 0x" + entriespointers[i].Pointers.ToString("X"));
                List<FullGraph> Graph = null;
                if (entriespointers[i].Logic != baseaddress && entriespointers[i].Pointers != baseaddress)
                {
                    Graph = PreProcessEntries(Data, ref entriespointers[i], ref entriespointers[0], Block0, CurrBlock, Linear);
                    ConvertedEntries[i].AddRange(ReadGameLogicEntries(Data, entriespointers[i], CurrBlock, Block0, Graph, Linear, Expanded, resList));
                }
                if (i == 0)
                    i = numentries;
            }
            List<string> FinalProduct= new List<string>();
            
            FinalProduct.Add(Linear ? "L" + (Expanded ? "E" : "") : "G");

            for (int i=0; i<numentries; i++)
            {
                FinalProduct.AddRange(ConvertedEntries[i]);
                FinalProduct.Add("");
            }

            string extraName = (Linear ? (Expanded ? "Expanded" : "") : "Graph");
            File.WriteAllLines("GameLogic" + extraName + ".txt", FinalProduct);
            return 0;
        }
        private static void Compile(string[] args)
        {

        }
    }
}
