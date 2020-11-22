using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M3GameLogic
{
    abstract class ParsingCommand
    {
        public int returnValue = SingleCommand.ImpossibleData;
        public abstract String returnText(params string[] parameters);
    }
    class Hex : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out returnValue))
                return Utilities.ToHex(returnValue, 2);
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0];
        }
    }
    class Hex7 : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out returnValue))
                return "0x" + Utilities.ToHex(returnValue, 7);
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0];
        }
    }
    class IntCommand : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out returnValue))
                return returnValue.ToString();
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0];
        }
    }
    class Int2Int : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out int result))
                if (int.TryParse(parameters[1], out int result2))
                {
                    returnValue = Utilities.ToInt2Ints(result, result2);
                    return returnValue.ToString();
                }
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0] + " " + parameters[1];
        }
    }
    class Int2IntS : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out int result))
                if (int.TryParse(parameters[1], out int result2))
                {
                    returnValue = Utilities.ToInt2IntsSigned(result, result2);
                    return returnValue.ToString();
                }
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0] + " " + parameters[1];
        }
    }
    class Int3Int : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out int result))
                if (int.TryParse(parameters[1], out int result2))
                    if (int.TryParse(parameters[2], out int result3))
                    {
                        returnValue = Utilities.ToInt3Ints(result, result2, result3);
                        return returnValue.ToString();
                    }
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0] + " " + parameters[1] + " " + parameters[2];
        }
    }
    class Int3IntS : ParsingCommand
    {
        public override string returnText(params string[] parameters)
        {
            if (int.TryParse(parameters[0], out int result))
                if (int.TryParse(parameters[1], out int result2))
                    if (int.TryParse(parameters[2], out int result3))
                    {
                        returnValue = Utilities.ToInt3IntsSigned(result, result2, result3);
                        return returnValue.ToString();
                    }
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0] + " " + parameters[1] + " " + parameters[2];
        }
    }
    class Neg : ParsingCommand
    {
        public virtual string operation
        {
            get { return "-"; }
        }
        public virtual string operation2
        {
            get { return ""; }
        }
        public virtual int operate(int a)
        {
            return -a;
        }

        public override string returnText(params string[] parameters)
        {
            int result;
            int valid = 0;

            if (int.TryParse(parameters[0], out result))
            {
                valid |= 1;
            }
            else
            {
                try
                {
                    result = Convert.ToInt32(parameters[0], 16);
                    valid |= 1;
                }
                catch (System.FormatException)
                {
                }
            }
            if (valid == 1)
            {
                returnValue = operate(result);
                return returnValue.ToString();
            }
            returnValue = SingleCommand.ImpossibleData;
            return operation + parameters[0] + operation2;
        }
    }
    class Inc : Neg
    {
        public override string operation
        {
            get { return ""; }
        }
        public override string operation2
        {
            get { return "++"; }
        }
        public override int operate(int a)
        {
            return a++;
        }
    }
    class Dec : Neg
    {
        public override string operation
        {
            get { return ""; }
        }
        public override string operation2
        {
            get { return "--"; }
        }
        public override int operate(int a)
        {
            return a--;
        }
    }
    class Sum : ParsingCommand
    {
        public virtual string operation
        {
            get { return "+"; }
        }
        public virtual int operate(int a, int b)
        {
            return a + b;
        }

        public override string returnText(params string[] parameters)
        {
            int[] results = new int[2];
            int valid = 0;

            for (int i = 0; i < 2; i++)
            {
                if (int.TryParse(parameters[i], out int result))
                {
                    results[i] = result;
                    valid |= (1 << i);
                }
                else
                {
                    try
                    {
                        result = Convert.ToInt32(parameters[i], 16);
                        results[i] = result;
                        valid |= (1 << i);
                    }
                    catch (System.FormatException)
                    {

                    }
                }
            }
            if (valid == 3)
            {
                returnValue = operate(results[0], results[1]);
                return returnValue.ToString();
            }
            returnValue = SingleCommand.ImpossibleData;
            return parameters[0] + " " + operation + " " + parameters[1];
        }
    }
    class Sub : Sum
    {
        public override string operation
        {
            get { return "-"; }
        }
        public override int operate(int a, int b)
        {
            return a - b;
        }
    }
    class Mul : Sum
    {
        public override string operation
        {
            get { return "*"; }
        }
        public override int operate(int a, int b)
        {
            return a * b;
        }
    }
    class Div : Sum
    {
        public override string operation
        {
            get { return "/"; }
        }
        public override int operate(int a, int b)
        {
            return a / b;
        }
    }
    class Mod : Sum
    {
        public override string operation
        {
            get { return "%"; }
        }
        public override int operate(int a, int b)
        {
            return a % b;
        }
    }
    class And : Sum
    {
        public override string operation
        {
            get { return "AND"; }
        }
        public override int operate(int a, int b)
        {
            return a & b;
        }
    }
    class Or : Sum
    {
        public override string operation
        {
            get { return "OR"; }
        }
        public override int operate(int a, int b)
        {
            return a | b;
        }
    }
    class Eq : Sum
    {
        public override string operation
        {
            get { return "=="; }
        }
        public override int operate(int a, int b)
        {
            return a == b ? 1 : 0;
        }
    }
    class Neq : Sum
    {
        public override string operation
        {
            get { return "!="; }
        }
        public override int operate(int a, int b)
        {
            return a != b ? 1 : 0;
        }
    }
    class Gt : Sum
    {
        public override string operation
        {
            get { return ">"; }
        }
        public override int operate(int a, int b)
        {
            return a > b ? 1 : 0;
        }
    }
    class Lt : Sum
    {
        public override string operation
        {
            get { return "<"; }
        }
        public override int operate(int a, int b)
        {
            return a < b ? 1 : 0;
        }
    }
    class Ge : Sum
    {
        public override string operation
        {
            get { return ">="; }
        }
        public override int operate(int a, int b)
        {
            return a >= b ? 1 : 0;
        }
    }
    class Le : Sum
    {
        public override string operation
        {
            get { return "<="; }
        }
        public override int operate(int a, int b)
        {
            return a <= b ? 1 : 0;
        }
    }
    class BlockCommand : ParsingCommand
    {
        Block Block;
        public BlockCommand(Block Block)
        {
            this.Block = Block;
        }
        public override string returnText(params string[] parameters)
        {
            returnValue = Utilities.ToInt2Ints(int.Parse(parameters[0]), int.Parse(parameters[1]));
            return Block.Number[returnValue].ToString();
        }
    }
    class BlockNum : ParsingCommand
    {
        Block Block;
        public BlockNum(Block Block)
        {
            this.Block = Block;
        }
        public override string returnText(params string[] parameters)
        {
            returnValue = Block.BlockNum;
            return returnValue.ToString();
        }
    }
    class Extra : DataCommand
    {
        SingleCommand Command;

        public Extra(List<int> DataList, List<string> Strings, SingleCommand C) : base(DataList, Strings)
        {
            Command = C;
        }

        public override string returnText(params string[] parameters)
        {
            string result = "";
            for (int i = 0; i < Command.NumParam; i++)
                result += ", " + calledRetText(i.ToString());
            return result;
        }
    }
    class DataCommand : ParsingCommand
    {
        List<int> DataList;
        List<string> Strings;

        public DataCommand(List<int> DataList, List<string> Strings)
        {
            this.DataList = DataList;
            this.Strings = Strings;
        }
        public string calledRetText(params string[] parameters)
        {
            int slot = int.Parse(parameters[0]);
            if (DataList.Count - slot - 1 >= 0)
            {
                if (DataList[DataList.Count - slot - 1] == SingleCommand.ImpossibleData)
                    return Strings[DataList.Count - slot - 1];
                return DataList[DataList.Count - slot - 1].ToString();
            }
            return "$" + (slot + 1);
        }
        public override string returnText(params string[] parameters)
        {
            return calledRetText(parameters);
        }
    }
    class Parser
    {
        ResourcesList resLists;
        List<int> dataValues;
        List<string> dataStrings;
        int latestValue;
        IDictionary<string, ParsingCommand> parsingCommands = new Dictionary<string, ParsingCommand>(){
        {"hex", new Hex()},
        {"hex7", new Hex7()},
        {"int", new IntCommand()},
        {"int2int", new Int2Int()},
        {"int2ints", new Int2IntS()},
        {"int3int", new Int3Int()},
        {"int3ints", new Int3IntS()},
        {"neg", new Neg()},
        {"sum", new Sum()},
        {"sub", new Sub()},
        {"mul", new Mul()},
        {"div", new Div()},
        {"mod", new Mod()},
        {"inc", new Inc()},
        {"dec", new Dec()},
        {"and", new And()},
        {"or", new Or()},
        {"eq", new Eq()},
        {"neq", new Neq()},
        {"gt", new Gt()},
        {"lt", new Lt()},
        {"ge", new Ge()},
        {"le", new Le()},
        };

        public Parser(Block Block0, Block CurrBlock, ResourcesList resourcesList, List<int> dataValues, List<String> dataStrings)
        {
            ResLists = resourcesList;
            parsingCommands.Add("block0", new BlockCommand(Block0));
            parsingCommands.Add("block", new BlockCommand(CurrBlock));
            parsingCommands.Add("blocknum", new BlockNum(CurrBlock));
            DataValues = dataValues;
            DataStrings = dataStrings;
            parsingCommands.Add("data", new DataCommand(DataValues, DataStrings));
        }
        
        public List<int> DataValues { get => dataValues; set => dataValues = value; }
        public List<string> DataStrings { get => dataStrings; set => dataStrings = value; }
        internal ResourcesList ResLists { get => resLists; set => resLists = value; }

        public string TranslateToText(SingleCommand Command)
        {
            if (parsingCommands.ContainsKey("extra"))
                parsingCommands.Remove("extra");
            parsingCommands.Add("extra", new Extra(DataValues, DataStrings, Command));
            String commandText = ResLists.getWantedInfo("MAIN", Command.CommandType.ToString());
            if (commandText != "")
            {
                string finalText = elaborateText(Command, commandText);
                if(Command.NumParam != 0)
                {
                    DataStrings.RemoveRange(Math.Max(DataStrings.Count - Command.NumParam, 0), Math.Min(Command.NumParam, DataStrings.Count));
                    DataValues.RemoveRange(Math.Max(DataValues.Count - Command.NumParam, 0), Math.Min(Command.NumParam, DataValues.Count));
                }
                DataStrings.Add(finalText);
                DataValues.Add(latestValue);
                return finalText;
            }
            return "";
        }

        private string[] specialSplit(string text)
        {
            List<string> newStrings = new List<string>();
            String[] splitString = Regex.Split(text, @"([\[\]\,])");
            for (int i = 0; i < splitString.Length; i++)
            {
                if (splitString[i] == "[") //Cover nested stuff
                {
                    string product = splitString[i++];
                    int countLayer = 1;
                    while (countLayer > 0)
                    {
                        string value = splitString[i++];
                        if (value == "[")
                            countLayer++;
                        else if (value == "]")
                            countLayer--;
                        product += value;
                    }
                    i--;
                    newStrings.Add(product);
                }
                else if(splitString[i] != "," && splitString[i].Trim() != "")
                    newStrings.Add(splitString[i]);
            }
            return newStrings.ToArray();
        }

        private int findSpecialEndIndex(string text)
        {
            int countLayer = 1;
            String[] splitString = Regex.Split(text, @"([\(\)])");
            int i = 0;
            string product = "";
            while (countLayer > 0)
            {
                string value = splitString[i++];
                if (value == "(")
                    countLayer++;
                else if (value == ")")
                    countLayer--;
                product += value;
            }
            return product.Length - 1;
        }

        private string elaborateText(SingleCommand Command, string text)
        {
            while (text.Contains('\\'))
            {
                string subText = text.Substring(text.LastIndexOf('\\') + 1);
                string commandText = subText.Substring(0, subText.IndexOf('('));
                int endLength = findSpecialEndIndex(subText.Substring(subText.IndexOf('(') + 1));
                string paramsText = subText.Substring(subText.IndexOf('(') + 1, endLength);

                string[] parameters = specialSplit(paramsText);
                for (int i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = parameters[i].Trim();
                    switch (parameters[i])
                    {
                        case "X":
                            parameters[i] = Command.X.ToString();
                            break;
                        case "Y":
                            parameters[i] = Command.Y.ToString();
                            break;
                        case "Z":
                            parameters[i] = Command.Z.ToString();
                            break;
                        case "O":
                            parameters[i] = Command.CommandType.ToString();
                            break;
                        case "default":
                            break;
                    }
                }

                string newText = "";
                if (parsingCommands.ContainsKey(commandText))
                {
                    newText = parsingCommands[commandText].returnText(parameters);
                    if(Command.CommandType == 1 || Command.CommandType == 0xE)
                        latestValue = parsingCommands[commandText].returnValue; //Only these commands return actual values that we can interpret...
                    else
                        latestValue = SingleCommand.ImpossibleData;
                }
                else
                {
                    newText = ResLists.getWantedInfo(commandText, parameters);
                    latestValue = SingleCommand.ImpossibleData;
                }
                text = text.Substring(0, text.LastIndexOf('\\')) + newText + text.Substring(text.LastIndexOf('\\') + 1 + subText.IndexOf('(') + 1 + endLength + 1);
            }
            return text;
        }
    }
}
