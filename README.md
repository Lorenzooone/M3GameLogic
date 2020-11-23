# M3GameLogic
This is a Game logic compiler/decompiler for Mother 3.
For now it just supports decompiling.
It requires passing in input the path to the ROM.

Standard output is fully working and goes to GameLogic.txt.

The e option makes it expand the branches.

To use it, type your path to the ROM and then "e", the output goes to GameLogicExpanded.txt.

The g option makes it use a graph logic, which isn't fully working and is currently broken, but could be useful to analyze some code.

To use it, type your path to the ROM and then "g", the output goes to GameLogicGraph.txt.

To interpret what's written, look at https://datacrystal.romhacking.net/wiki/MOTHER_3:Game_logic
