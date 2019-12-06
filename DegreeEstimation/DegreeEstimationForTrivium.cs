//******************************************************************//
//In this algorithm, based on the division property with the flag t-//
//echnique, we estimate the upper bound of the degree of the superp-//
//oly of a given cube under specific conditions impose on key/iv va-//
//riables.                                                          //
//Althogh the division property with flag technique is not accurate,//
//it is secure to use it to estimate the upper bound of the degree //
//of a superpoly of a cube.                                        //
//****************************NOTE**********************************//
//1. The code is writen in C# and the MILP solver used is Gurobi 7.5//
//   To run it, we should use release/x64 mode and cite the         //
//   Gurobi75Net.dll.                                               //
//******************************************************************//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurobi;
using System.IO;
namespace gorubi
{
    class Program
    {
        static void Main(string[] args)
        {

            int round;
            List<int> deglist = new List<int>();
            //StreamWriter MaxDegT = new StreamWriter("MaxdegreeTerms.txt");
            int startround = 960;
            //the target round runs from 960 to 1152.
            for (round = startround; round < 1153; round++)
            {
                GRBEnv env = new GRBEnv("Trvium.log");

                GRBModel model = new GRBModel(env);
                Console.Write("****************** The " + round.ToString() + "-th round ******************\n");
                //设置gurobi不输出中间结果
                model.Parameters.LogToConsole = 0;
                int[] pos = new int[] { 65, 92, 161, 176, 242, 287 };//6个输出位置 65,92, 161, 176, 242, 287
                //int[] pos = new int[3] {  };//6个输出位置
                GRBVar[] IV = model.AddVars(80, GRB.BINARY);
                GRBVar[] Key = model.AddVars(80, GRB.BINARY);
                for (int i = 0; i < 80; i++)
                {
                    IV[i].VarName = "IV" + i.ToString();//IV变量,命名为IV0-IV79
                    Key[i].VarName = "Key" + i.ToString();//IV变量,命名为Key0-Key79
                }
                GRBVar[] s = model.AddVars(288, GRB.BINARY);
                for (int i = 0; i < 288; i++)
                    s[i].VarName = "var" + i.ToString();//288个寄存器,命名为var0-var288
                char[] FlagS = new char[288];//288个寄存器的Flag

                GRBVar[] NewVars = model.AddVars(30 * round, GRB.BINARY);
                for (int i = 0; i < NewVars.Length; i++)
                    NewVars[i].VarName = "y" + i.ToString();//每过一次更新许需要加30个变量，总共为30*round,命名为y0-y300*round
                char[] FlagNewVars = new char[30 * round];//新加变量的Flag

                List<uint> cube = new List<uint>() { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66, 69, 72, 75, 78 };
                List<uint> ivbits_set_to_1 = new List<uint>() { };
                List<uint> ivbits_set_to_0 = new List<uint>() { };
                for (uint i = 0; i < 80; i = i + 1)
                    ivbits_set_to_0.Add(i);
                for (int i = 0; i < cube.Count; i++)
                {
                    ivbits_set_to_0.Remove(cube[i]);
                }
                for (int i = 0; i < cube.Count; i++)
                    Console.Write(cube[i] + " ");
                Console.WriteLine();

                List<UInt32> Noncube = new List<uint>() { 0x0, 0x0, 0x0 };//Noncube stores the value of the non-cube variables

                //for each iv bit which is set to 1, set the corresponding bit of Noncube to 1.
                for (int i = 0; i < ivbits_set_to_1.Count; i++)
                {
                    Noncube[(int)ivbits_set_to_1[i] >> 5] |= (uint)(0x01 << ((int)(ivbits_set_to_1[i] & 0x1f)));
                }

                GRBLinExpr ChooseIV = new GRBLinExpr();//
                for (int i = 0; i < cube.Count; i++)
                    ChooseIV.AddTerm(1.0, IV[cube[i]]);

                //the bits set to constants
                List<int> chokey = new List<int>() { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 17, 19, 20, 22, 23, 25, 26, 28, 29, 31, 32, 34, 35, 37, 38, 40, 41, 43, 44, 46, 47, 49, 50, 52, 53, 55, 56, 58, 59, 61, 62, 64, 65, 66,67, 68, 70, 71, 73, 74, 76, 77, 79 };
                
                //pick up the key variables which are not fixed.
                //i.e. keydeg= k_i1+k_i2+...+k_in, where k_i1,k_i2,...,k_im are the key bits which are not fixed
                GRBLinExpr keydeg = new GRBLinExpr();
                for (int i = 0; i < 80; i++)
                {
                    if (!chokey.Contains(i))
                        keydeg.AddTerm(1.0, Key[i]);
                }

                //set maximizing the linear expression keydeg as the objective function of our model
                //Hence, we could obtain the upper bound of the degree of the superpoly of the chosen cube.
                model.SetObjective(keydeg, GRB.MAXIMIZE);

                //in this function, we set the conditions which are imposed to the key bits and iv bits
                //before running, it needs to set some parameters, such as the key bits set to 0/1 and so on, in this function,
                initial(model, s, FlagS, cube, Noncube, IV, Key); 

                int VarNumber = 0;

                //describe the propagation of the division property with flag through Trivium
                for (int i = 1; i <= round; i++)
                    Triviumcore(model, s, FlagS, NewVars, FlagNewVars, ref VarNumber);

                for (int i = 0; i < 288; i++)
                {
                    if (!pos.Contains(i))
                    {
                        model.AddConstr(s[i] == 0, "a" + i.ToString());
                    }
                }
                GRBLinExpr expr = new GRBLinExpr();
                for (int i = 0; i < pos.Count(); i++)
                    expr.AddTerm(1.0, s[pos[i]]);
                model.AddConstr(expr == 1, "t1");

                //solve the MILP model.
                model.Optimize();
                int currentdeg = 0;

                //outout the solution
                int NO = 0;
                // is the model is feasible the upper bound of the degree of the superpoly is large than 0.
                // In this case, we output a possible term of degree d, where d is the upper bound of the degree of the superpoly.
                if (model.SolCount > 0)
                {
                    StreamWriter MaxDegT = new StreamWriter("MaxdegreeTerms.txt", true);
                    currentdeg = (int)model.ObjVal;
                    NO++;
                    Console.WriteLine("****************No." + round + "********************\n");
                    //MaxDegT.WriteLine("****************No." + NO + "********************\n");
                    MaxDegT.WriteLine("*****************round" + round + "**********************\n");
                    MaxDegT.Write("Upper bound of degree of superpoly: ");
                    Console.WriteLine(model.ObjVal);
                    MaxDegT.WriteLine(model.ObjVal);
                    GRBLinExpr ChoIV = new GRBLinExpr();//选择能到达最大次数的IV;
                    // for (int i = 0; i < cube.Count; i++)
                    int sumy = 0;
                    for (int i = 0; i < 80; i++)
                    {
                        Console.Write(Key[i].X + ",");
                    }
                    Console.WriteLine();

                    MaxDegT.Write("One possible terms of degree " + model.ObjVal + " is : ");
                    for (int i = 0; i < 80; i++)
                    {
                        if (Key[i].X > 0.781)
                        {
                            sumy++;
                            MaxDegT.Write(i + ",");
                            Console.Write(i + ",");
                        }
                    }
                    MaxDegT.Write("\n\n***********************************************\n");
                    Console.Write("\n***********************************************\n");
                    for (int i = 0; i < cube.Count(); i++)
                    {
                        // Console.Write(IV[i].X + ",");
                        if (s[cube[i]].X > 0.781)
                        {
                            sumy++;
                            MaxDegT.Write(i + ",");
                            Console.Write(i + ",");
                        }
                    }

                    Console.WriteLine();
                    MaxDegT.WriteLine();
                    MaxDegT.Close();
                }

                if (model.SolCount > 0)
                    deglist.Add((int)model.ObjVal);
                else//if the model is imfeasible, then the degree of the superpoly is upper bounded by 0.
                {
                    deglist.Add(0);
                    StreamWriter MaxDegT = new StreamWriter("MaxdegreeTerms.txt", true);
                    MaxDegT.WriteLine("*****************round" + round + "**********************\n");
                    MaxDegT.WriteLine("Upper bound of degree of superpoly: 0");
                    MaxDegT.Write("\n***********************************************\n");
                    MaxDegT.WriteLine();
                    MaxDegT.Close();
                }
                model.Dispose();
                env.Dispose();
            }
            //MaxDegT.Close();
            for (int i = startround; i < round; i++)
            {
                Console.Write(i.ToString() + "  " + deglist[i - startround].ToString() + "\n");
            }
           
            Console.ReadLine();
        }

        //initialize the model and set the conditions imposed to the key/iv bits.
        static void initial(GRBModel model, GRBVar[] s, char[] FlagS, List<uint> cube, List<UInt32> Noncube, GRBVar[] IV, GRBVar[] Key)
        {
           //key bits set to 0
            List<int> chosenkey = new List<int>() { 1, 2, 4, 5, 7, 8, 10, 11, 13, 14, 16, 17, 19, 20, 22, 23, 25, 26, 28, 29, 31, 32, 34, 35, 37, 38, 40, 41, 43, 44, 46, 47, 49, 50, 52, 53, 55, 56, 58, 59, 61, 62, 64, 65, 67, 68, 70, 71, 73, 74, 76, 77, 79 };
            //key bits set to 1
            List<int> onekey = new List<int>() { 66};
            for (int i = 0; i < 80; i++)
            {
                //set key bits in chosenkey to constant 0
                if(chosenkey.Contains(i))
                {
                    model.AddConstr(s[i] == 0, "z" + i.ToString());
                    FlagS[i] = '0';
                }
                else
                {
                    //set the key bits in onekey to constant 1
                    if(onekey.Contains(i))
                    {
                        Console.WriteLine("******"+i+"********");
                        model.AddConstr(s[i] == 0, "z" + i.ToString());
                        FlagS[i] = '1';
                    }
                    else// treat the remaining key bits as variables.
                    {
                        model.AddConstr(s[i] == Key[i], "z" + i.ToString());
                        FlagS[i] = '2';
                    }
                }
                
            }
            
            for (int i = 80; i < 93; i++)
            {
                model.AddConstr(s[i] == 0, "z" + i.ToString());
                FlagS[i] = '0';
            }


            if (Noncube.Count == 0)//if the noncube variables are not set to constants
            {
                for (uint i = 93; i < 173; i++)
                {

                    if (cube.Contains(i - 93))
                    {
                        model.AddConstr(s[i] == 1, "IV" + i.ToString());
                    }
                    else
                    {
                        model.AddConstr(s[i] == 0, "z" + i.ToString());
                    }
                    FlagS[i] = '2';
                }
            }
            else//if the non-cube variables are set to constants
            {
                for (uint i = 93; i < 173; i++)
                {
                    //treat the iv bits in cube as variable
                    if (cube.Contains(i - 93))
                    {
                        model.AddConstr(s[i] == 1, "z" + i.ToString());
                        FlagS[i] = '2';
                    }
                    else
                    {
                        //model.AddConstr(IV[i - 93] == 0, "IV" + i.ToString());
                        model.AddConstr(s[i] == 0, "z" + i.ToString());
                        int pos1 = (int)((i - 93) >> 5);
                        int pos2 = (int)(((i - 93) & 0x1f));
                        //the flag of iv bits which are set to 1 is set to '1'
                        if (((Noncube[pos1] >> pos2) & 0x01) == 1)
                        {
                            FlagS[i] = '1';
                        }
                        else //the flag of iv bits which are set to 0 is set to '0'
                        {
                            FlagS[i] = '0';
                        }
                    }
                }
            }
            //initialize the state bits which are loaded with constants
            //namely, set the last 4 bits of the second register and the first 108 bits in the third register to constant 0
            //set the last 3 bits of the third register to 1.
            for (int i = 173; i < 285; i++)
            {
                model.AddConstr(s[i] == 0, "z" + i.ToString());
                FlagS[i] = '0';
            }
            for (int i = 285; i < 288; i++)
            {
                model.AddConstr(s[i] == 0, "z" + i.ToString());
                FlagS[i] = '1';
            }


        }

        //describe the propagation of division property through the round function of Trivium
        static void Triviumcore(GRBModel model, GRBVar[] s, Char[] FlagS, GRBVar[] NewVar, char[] FlagNewVar, ref int VarNumber)
        {

            int[] posA = new int[5] { 65, 170, 90, 91, 92 };
            for (int i = 0; i < 4; i++)
            {
                model.AddConstr(NewVar[VarNumber + 2 * i] + NewVar[VarNumber + 2 * i + 1] == s[posA[i]], "c" + (VarNumber + i).ToString());
                FlagNewVar[VarNumber + 2 * i] = FlagS[posA[i]];
                FlagNewVar[VarNumber + 2 * i + 1] = FlagS[posA[i]];

            }
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 5], "c" + (VarNumber + 5).ToString());
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 7], "c" + (VarNumber + 6).ToString());
            FlagNewVar[VarNumber + 8] = FlagMul(FlagNewVar[VarNumber + 5], FlagNewVar[VarNumber + 7]);
            if (FlagNewVar[VarNumber + 8] == '0')
                model.AddConstr(NewVar[VarNumber + 8] == 0, "t" + (VarNumber / 10).ToString());
            model.AddConstr(NewVar[VarNumber + 9] == s[posA[4]] + NewVar[VarNumber + 8] + NewVar[VarNumber + 1] + NewVar[VarNumber + 3], "c" + (VarNumber + 7).ToString());
            FlagNewVar[VarNumber + 9] = FlagAdd(FlagAdd(FlagS[posA[4]], FlagNewVar[VarNumber + 8]), FlagAdd(FlagNewVar[VarNumber + 1], FlagNewVar[VarNumber + 3]));
            VarNumber = VarNumber + 10;

            int[] posB = new int[5] { 161, 263, 174, 175, 176 };
            for (int i = 0; i < 4; i++)
            {
                model.AddConstr(NewVar[VarNumber + 2 * i] + NewVar[VarNumber + 2 * i + 1] == s[posB[i]], "c" + (VarNumber + i).ToString());
                FlagNewVar[VarNumber + 2 * i] = FlagS[posB[i]];
                FlagNewVar[VarNumber + 2 * i + 1] = FlagS[posB[i]];
            }
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 5], "c" + (VarNumber + 5).ToString());
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 7], "c" + (VarNumber + 6).ToString());
            FlagNewVar[VarNumber + 8] = FlagMul(FlagNewVar[VarNumber + 5], FlagNewVar[VarNumber + 7]);
            if (FlagNewVar[VarNumber + 8] == '0')
                model.AddConstr(NewVar[VarNumber + 8] == 0, "t" + (VarNumber / 10).ToString());
            model.AddConstr(NewVar[VarNumber + 9] == s[posB[4]] + NewVar[VarNumber + 8] + NewVar[VarNumber + 1] + NewVar[VarNumber + 3], "c" + (VarNumber + 7).ToString());
            FlagNewVar[VarNumber + 9] = FlagAdd(FlagAdd(FlagS[posB[4]], FlagNewVar[VarNumber + 8]), FlagAdd(FlagNewVar[VarNumber + 1], FlagNewVar[VarNumber + 3]));
            VarNumber = VarNumber + 10;

            int[] posC = new int[5] { 242, 68, 285, 286, 287 };
            for (int i = 0; i < 4; i++)
            {
                model.AddConstr(NewVar[VarNumber + 2 * i] + NewVar[VarNumber + 2 * i + 1] == s[posC[i]], "c" + (VarNumber + i).ToString());
                FlagNewVar[VarNumber + 2 * i] = FlagS[posC[i]];
                FlagNewVar[VarNumber + 2 * i + 1] = FlagS[posC[i]];
            }
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 5], "c" + (VarNumber + 5).ToString());
            model.AddConstr(NewVar[VarNumber + 8] >= NewVar[VarNumber + 7], "c" + (VarNumber + 6).ToString());
            FlagNewVar[VarNumber + 8] = FlagMul(FlagNewVar[VarNumber + 5], FlagNewVar[VarNumber + 7]);
            if (FlagNewVar[VarNumber + 8] == '0')
                model.AddConstr(NewVar[VarNumber + 8] == 0, "t" + (VarNumber / 10).ToString());
            model.AddConstr(NewVar[VarNumber + 9] == s[posC[4]] + NewVar[VarNumber + 8] + NewVar[VarNumber + 1] + NewVar[VarNumber + 3], "c" + (VarNumber + 7).ToString());
            FlagNewVar[VarNumber + 9] = FlagAdd(FlagAdd(FlagS[posC[4]], FlagNewVar[VarNumber + 8]), FlagAdd(FlagNewVar[VarNumber + 1], FlagNewVar[VarNumber + 3]));
            VarNumber = VarNumber + 10;

            for (int i = 287; i > 0; i--)
            {
                s[i] = s[i - 1];
                FlagS[i] = FlagS[i - 1];
            }
            s[0] = NewVar[VarNumber - 10 + 9]; FlagS[0] = FlagNewVar[VarNumber - 10 + 9];
            s[287] = NewVar[VarNumber - 10 + 6]; FlagS[287] = FlagNewVar[VarNumber - 10 + 6];
            s[286] = NewVar[VarNumber - 10 + 4]; FlagS[286] = FlagNewVar[VarNumber - 10 + 4];
            s[69] = NewVar[VarNumber - 10 + 2]; FlagS[69] = FlagNewVar[VarNumber - 10 + 2];
            s[243] = NewVar[VarNumber - 10 + 0]; FlagS[243] = FlagNewVar[VarNumber - 10 + 0];
            s[177] = NewVar[VarNumber - 20 + 9]; FlagS[177] = FlagNewVar[VarNumber - 20 + 9];
            s[176] = NewVar[VarNumber - 20 + 6]; FlagS[176] = FlagNewVar[VarNumber - 20 + 6];
            s[175] = NewVar[VarNumber - 20 + 4]; FlagS[175] = FlagNewVar[VarNumber - 20 + 4];
            s[264] = NewVar[VarNumber - 20 + 2]; FlagS[264] = FlagNewVar[VarNumber - 20 + 2];
            s[162] = NewVar[VarNumber - 20 + 0]; FlagS[162] = FlagNewVar[VarNumber - 20 + 0];
            s[93] = NewVar[VarNumber - 30 + 9]; FlagS[93] = FlagNewVar[VarNumber - 30 + 9];
            s[92] = NewVar[VarNumber - 30 + 6]; FlagS[92] = FlagNewVar[VarNumber - 30 + 6];
            s[91] = NewVar[VarNumber - 30 + 4]; FlagS[91] = FlagNewVar[VarNumber - 30 + 4];
            s[171] = NewVar[VarNumber - 30 + 2]; FlagS[171] = FlagNewVar[VarNumber - 30 + 2];
            s[66] = NewVar[VarNumber - 30 + 0]; FlagS[66] = FlagNewVar[VarNumber - 30 + 0];
        }

        //propagation rule on the XOR operation  of flag 
        static char FlagAdd(char FlagA, char FlagB)
        {
            if (FlagA == '0')
            {
                return FlagB;
            }
            else if (FlagA == '1')
            {
                if (FlagB == '0')
                    return FlagA;
                else if (FlagB == '1')
                    return '0';
                else
                    return '2';
            }
            else
            {
                return '2';
            }

        }
        //propagation rule on the AND operation of flag 
        static char FlagMul(char FlagA, char FlagB)
        {
            if (FlagA == '0')
            {
                return '0';
            }
            else if (FlagA == '1')
            {
                return FlagB;
            }
            else
            {
                if (FlagB == '0')
                    return '0';
                else
                    return FlagA;
            }
        }
    }
}
