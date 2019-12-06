//******************************************************************//
//In this algorithm, we first read the conditions derived to control// 
//the propagtion of the chosen difference. Then, we model each con- //
//dition according to the rules in Proposition 1 and 2. Finally, we //
//attempt to figure out the minimum numder of conditions which are  //
//equal to 1.                                                       //
//****************************NOTE**********************************//
//1. The code is writen in C# and the MILP solver used is Gurobi 7.5//
//   To run it, we should use release/x64 mode and cite the         //
//   Gurobi75Net.dll.                                               //
//2. The file Conset.txt includes all the derived conditions.       //
//3. This code is used to figure out the minmum number of the cond- //
//   itions which are equal to 1.                                   //
//4. The file FinalCons.txt consists of the final conditions deriv- //
//   ed by optimizing our MILP model.                               //
//******************************************************************//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Gurobi;

namespace SearchForOptimalConMILP
{
    class Program
    {
        //read condition from the file
        static public void ReadCondition(UInt32[, ,] ConS, int[] conlen, int[] consflag)
        {
            // UInt32[,,] ConS = new UInt32[10000, 20, 6];
            int i, j, k, m;
            int ind = 0;
            int t = 0;
            m = 0;
            //int[] conlen = new int[10000];
            for (i = 0; i < 1000; i++)
            {
                for (j = 0; j < 20; j++)
                {
                    for (k = 0; k < 6; k++)
                    {
                        ConS[i, j, k] = 0;
                    }
                }
            }
            //条件置零
            for (i = 0; i < 1000; i++)
            {
                consflag[i] = 0;
            }
            StreamReader conset = new StreamReader("ConSet.txt");
            while (!conset.EndOfStream)
            {
                string temp;
                temp = conset.ReadLine();
                string[] str = temp.Split('+');
                //每一项用6个int型变量表示，012，v; 345,k;
                // Console.WriteLine(temp);
                //str.Count()个单项式
                conlen[m] = str.Count();
                for (i = 0; i < str.Count(); i++)
                {
                    if (str[i][0] == '1')
                    {
                        consflag[m] = 1;
                        conlen[m] = conlen[m] - 1;
                    }
                    else//不是常值1时进行处理。
                    {
                        string[] temp2 = str[i].Split(')');
                        //提取每一个变元，放到cons[m,i,]这个单项式中
                        for (k = 0; k < temp2.Count() - 1; k++)
                        {
                            if (temp2[k][0] == '*')
                            {
                                ind = 1;
                            }
                            else
                            {
                                ind = 0;
                            }
                            if (temp2[k][ind] == 'v')
                            {
                                if (temp2[k].Length == (3 + ind))
                                {
                                    t = temp2[k][2 + ind] - '0';
                                }
                                if (temp2[k].Length == (4 + ind))
                                {
                                    t = (temp2[k][2 + ind] - '0') * 10 + (temp2[k][3 + ind] - '0');
                                }
                                ConS[m, i, (t >> 5)] = ConS[m, i, (t >> 5)] | ((UInt32)(0x01 << (t & 0x1f)));
                            }

                            if (temp2[k][ind] == 'k')
                            {
                                if (temp2[k].Length == (3 + ind))
                                {
                                    t = temp2[k][2 + ind] - '0';
                                }
                                if (temp2[k].Length == (4 + ind))
                                {
                                    t = (temp2[k][2 + ind] - '0') * 10 + (temp2[k][3 + ind] - '0');
                                }
                                ConS[m, i, (t >> 5) + 3] = ConS[m, i, (t >> 5) + 3] | ((UInt32)(0x01 << (t & 0x1f)));
                            }
                            //  Console.Write(t + ",");
                        }
                    }
                }
                m++;
            }
            conset.Close();
        }
        //model y=x1+x2
        static public void AddCons2Vars(GRBModel m, List<GRBVar> pickvars)
        {
            double[,] coeff = new double[4, 4] { { 1, 1, -1, 0 }, { 1, -1, 1, 0 }, { -1, 1, 1, 0 }, { -1, -1, -1, 2 } };
            GRBLinExpr lincon = new GRBLinExpr();
            int i = 0, j = 0;
            for (i = 0; i < 4; i++)
            {
                lincon.Clear();
                for (j = 0; j < 3; j++)
                    lincon.AddTerm(coeff[i, j], pickvars[j]);
                lincon.AddConstant(coeff[i, 3]);
                m.AddConstr(lincon >= 0, "conslin2");
            }
        }
        //model y=x1+x2+x3
        static public void AddCons3Vars(GRBModel m, List<GRBVar> pickvars)
        {
            double[,] coeff = new double[8, 5] { {-1,-1,-1,1,2},{1,1,1,-1,0},{1,-1,1,1,0},{-1,1,1,1,0},
                                                 {1,1,-1,1,0},{-1,-1,1,-1,2},{-1,1,-1,-1,2},{1,-1,-1,-1,2}};
            GRBLinExpr lincon = new GRBLinExpr();
            int i = 0, j = 0;
            for (i = 0; i < 8; i++)
            {
                lincon.Clear();
                for (j = 0; j < 4; j++)
                    lincon.AddTerm(coeff[i, j], pickvars[j]);
                lincon.AddConstant(coeff[i, 4]);
                m.AddConstr(lincon >= 0, "conslin2");
            }
        }
        //linearize a condition f. For example, f=k1+k2*k3+k4, it would return f=u1+u2+u3, where u1=k1, u2=k2*k3, u3=k4
        //Furthermore, this function could identify if f has a constant term. If so, it would mark out that f has a constant term in the array consflag.
        static public List<GRBVar> LinearConstrain(GRBModel m, GRBVar[] Uvar, GRBVar[] UFvar, UInt32[, ,] ConS, int[] conlen, int ind, GRBVar[] keyvar, GRBVar[] ivvar, int[] loc1, GRBVar[] kvf, GRBVar[] vvf, GRBVar[] Wvar, int[] locw, List<GRBVar> LinFlagRes)
        {
            List<GRBVar> LinVar = new List<GRBVar>() { };
            List<GRBVar> LinVarFlag = new List<GRBVar>() { };
            List<GRBVar> TempVarFlag = new List<GRBVar>() { };//store the flag variables of the variables appearing in a monimial
            List<GRBVar> TempVar = new List<GRBVar>() { };//store the assignment variables of the variables appearing in a monimial
            int weight = 0;
            int i, j, k;
            int t;
            int loc = 0;
            int locf = 0;
            int loc_w = locw[0];
            loc = loc1[0];
            locf = loc1[1];
            for (j = 0; j < conlen[ind]; j++)
            {
                //for the j-th monomial
                //Cons[ind,j,0-2]stores the iv variable appearing in the monimial
                //Cons[ind,j,3-5]stores the key variable appearing in the monimial
                weight = 0;
                TempVar.Clear();
                TempVarFlag.Clear();
                for (i = 0; i < 96; i++)
                {
                    t = (int)((ConS[ind, j, (i >> 5)] >> (i & 0x1f)) & 0x01);
                    weight = weight + t;
                    if (t == 1)
                    {
                        TempVar.Add(ivvar[i]);
                        TempVarFlag.Add(vvf[i]);
                    }
                }
                for (i = 0; i < 96; i++)
                {
                    t = (int)((ConS[ind, j, (i >> 5) + 3] >> (i & 0x1f)) & 0x01);
                    weight = weight + t;
                    if (t == 1)
                    {
                        TempVar.Add(keyvar[i]);
                        TempVarFlag.Add(kvf[i]);
                    }

                }
                //if weight ==1, it is a monomial of degree one, then we only replace it with a new variable
                if (weight == 1)
                {
                    m.AddConstr(Uvar[loc] == TempVar[0], "SingleBit" + loc.ToString());
                    m.AddConstr(UFvar[locf] == TempVarFlag[0], "SingleBit" + loc.ToString());
                    LinVar.Add(Uvar[loc]);
                    LinFlagRes.Add(UFvar[locf]);
                    loc++;
                    locf++;

                }
                //if weight>1, then it is a nonlinear monomial and we should linearize it accroding to the rule of AND in our paper.
                if (weight > 1)
                {
                    GRBVar[] PickVars = new GRBVar[TempVar.Count];
                    for (i = 0; i < TempVar.Count; i++)
                    {
                        PickVars[i] = TempVar[i];

                        //Wvar[loc_w+i]= TempVar[i]^TempVarFlag[i]
                        GRBVar[] tempor = new GRBVar[2] { TempVar[i], TempVarFlag[i] };
                        m.AddGenConstrOr(Wvar[loc_w + i], tempor, "tempor" + i.ToString());//变元与变元的标志相或
                    }
                    GRBVar[] tempm = new GRBVar[TempVar.Count];


                    //Wvar[loc_w+l]=min(Wvar[loc_w],Wvar[loc_w+1],...,Wvar[loc_w+l-1]), where l is the number variables appearing in the monomial
                    for (i = 0; i < TempVar.Count; i++)
                    {
                        tempm[i] = Wvar[loc_w + i];
                    }
                    loc_w = loc_w + TempVar.Count;
                    m.AddGenConstrMin(Wvar[loc_w], tempm, 1, "linmin" + loc_w.ToString());
                    loc_w++;

                    
                    for (i = 0; i < TempVar.Count; i++)
                    {
                        tempm[i] = TempVarFlag[i];
                    }
                    //Wvar[loc_w]=max(TempVarFlag[0],TempVarFlag[1],...,TempVarFlag[l-1]), where l is the number variables appearing in the monomial
                    m.AddGenConstrMax(Wvar[loc_w], tempm, 0, "linmaxFlag" + loc_w.ToString());
                    loc_w++;

                    
                    GRBVar[] tempminFlag = new GRBVar[2] { Wvar[loc_w - 1], Wvar[loc_w - 2] };
                    //UFvar[loc] is the final flag variable of the monomial
                    m.AddGenConstrMin(UFvar[locf], tempminFlag, 1, "minFlag" + loc.ToString());
                    //Uvar[loc] is the final assignment variable of the monomial
                    m.AddGenConstrMin(Uvar[loc], PickVars, 0, "Linearity" + loc.ToString());
                    LinVar.Add(Uvar[loc]);
                    LinFlagRes.Add(UFvar[locf]);
                    loc++;
                    locf++;
                }
            }
            loc1[0] = loc;
            loc1[1] = locf;
            locw[0] = loc_w;
            return LinVar;
        }
        static void Main(string[] args)
        {
            GRBEnv env = new GRBEnv("TJLinearCharacter.log");
            GRBModel model = new GRBModel(env);

            //declare the iv variables and key variables
            GRBVar[] keyvar = model.AddVars(80, GRB.BINARY);//declare the assignment variables of key variables
            GRBVar[] ivvar = model.AddVars(80, GRB.BINARY);//declare the assignment variables of iv variables
            GRBVar[] kvf = model.AddVars(80, GRB.BINARY);//declare the flag variables of key variables
            GRBVar[] vvf = model.AddVars(80, GRB.BINARY);//declare the flag variables of iv variables

            GRBVar[] Uvar = model.AddVars(8000, GRB.BINARY);//interval assignment variables used to long XOR expression into short ones y=k1+k2+k3+k4--> u1=k1+k2, u2=k3+k4, y=u1+u2
            GRBVar[] Vvar = model.AddVars(8000, GRB.INTEGER);//interval variables
            GRBVar[] Wvar = model.AddVars(8000, GRB.BINARY);//interval variables

            GRBVar[] UFvar = model.AddVars(8000, GRB.BINARY);//UFvar[i] is the flag variables corresponding to Uvar[i]
            GRBVar[] NUvar = model.AddVars(8000, GRB.BINARY);//NUvar[i]= 1-Uvar[i]
            GRBVar[] NUFvar = model.AddVars(8000, GRB.BINARY);//NUFvar[i]= 1-UFvar[i]
            GRBVar[] Consvar = model.AddVars(8000, GRB.BINARY);//constant variables
            GRBVar[] bvar = model.AddVars(8000, GRB.BINARY);//b variables
            int i, j, k;
            int loc0 = 0, loc1 = 0, loc2 = 0, loc3 = 0, loc4 = 0, locb = 0;
            int locf = 0;
            int con = 0;
            // name all the variables used in this MILP model
            for (i = 0; i < keyvar.Length; i++)
            {
                keyvar[i].VarName = "k" + i.ToString();
                ivvar[i].VarName = "v" + i.ToString();
                kvf[i].VarName = "kf" + i.ToString();
                vvf[i].VarName = "vf" + i.ToString();
            }
            for (i = 0; i < Uvar.Length; i++)
            {
                Uvar[i].VarName = "U" + i.ToString();
                Wvar[i].VarName = "W" + i.ToString();
                UFvar[i].VarName = "UF" + i.ToString();
                Vvar[i].VarName = "V" + i.ToString();
                NUvar[i].VarName = "NU" + i.ToString();
                NUFvar[i].VarName = "NUF" + i.ToString();
                bvar[i].VarName = "b" + i.ToString();
                Consvar[i].VarName = "cons" + i.ToString();
            }

            UInt32[, ,] ConS = new UInt32[1000, 200, 6];//used to store the conditions derived to control the propagation of difference
            int[] conlen = new int[1000];
            int[] consflag = new int[1000];
            int[] rec_loc1 = new int[1000];
            int[] rec_locf = new int[1000];

            //the number of the conditions
            int consnum = 652;
            int[] loc = new int[2];
            int[] locw = new int[1];
            int[] condiassig = new int[consnum];
            int[] condiflag = new int[consnum];

            GRBLinExpr Tar = new GRBLinExpr();//declare the target linear expression, which is used as the objective function
            List<int> indlist = new List<int>() { };
            List<GRBVar> Varlist = new List<GRBVar>() { };
            List<GRBVar> VarlistCopy = new List<GRBVar>() { };
            List<GRBVar> VarFlaglist = new List<GRBVar>() { };
            //List<GRBVar> VarFlaglistCopy = new List<GRBVar>() { };
            StreamWriter sw = new StreamWriter("FinalCons.txt");
            //the index of cube variables
            List<int> cube = new List<int>() { 0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66, 69, 72, 75, 78 };

            //set the state of cube variables to \delta
            for (i = 0; i < cube.Count; i++)
            {
                model.AddConstr(vvf[cube[i]] == 1, "iniiv" + (cube[i]).ToString());
            }

            //set the contraints between an assigment variable and the corresponding flag variable
            for (i = 0; i < 80; i++)
            {
                model.AddConstr(keyvar[i] <= 1 - kvf[i], "keyflagrel");
                model.AddConstr(ivvar[i] <= 1 - vvf[i], "ivflagrel");
            }
            model.Update();

            //read the derived constraints from the file 
            ReadCondition(ConS, conlen, consflag);
            Console.Write("ReadConditionDone\n");

            //model each condition
            for (i = 0; i < consnum; i++)
            {
                loc[0] = loc1;
                loc[1] = locf;
                VarFlaglist.Clear();
                Varlist.Clear();
                //linearize a condition and return a list of involved variables. 

                //In this condition, for condition f, it would linearize it and would return the assignment/flag variable of each monomial in f;
                //In paticular, Uvar[0],Uvar[1],...,Uvar[m-1] are the assignment variables of the linearized monomials
                //UFvar[0],UFvar[1],...,UFvar[m-1] are the flag variables of the linearized monomials
                //For eaxmple, for a condition f=k1+k2*k3+k4, it can be linearized as f=u1+u2+u3 where u2=k2k3.
                //Then, Varlist[0],Varlist[1],Varlist[2] are assignment variables of u1,u2,u3
                //and VarFlaglist[0],VarFlaglist[1],...,VarFlaglist[2] are the flag variables of u1,u2,u3
                Varlist = LinearConstrain(model, Uvar, UFvar, ConS, conlen, i, keyvar, ivvar, loc, kvf, vvf, Wvar, locw, VarFlaglist);
                loc1 = loc[0];
                locf = loc[1];
                //here a condition f is linearized and so it is the XOR of several variables.
                //in the following, we would determine the assigment varible and flag variable of f
                // and we denote the assigment variable and flag variable of f by f_av and f_fv respectively for short.

                //The target expression 'Tar' is determined according to the assigment variable and flag variable of each condition f 
                //In particular, Tar= sum(f in Conset) f_av +(-10000)*f_fv, where Conset is the set of conditions.

                //If the size of f is 1(excluding the constant), namely f consists of only one variable, then we do not need extra operations.
                if (Varlist.Count == 1)
                {
                    //if consflag[i]=1, then it means that there is a constant 1 in f. 
                    //In this case, we add (-1.0*f_av) to Tar and increase con by 1 which is added to Tar in the end.
                    if (consflag[i] == 1)
                    {
                        Tar.AddTerm(-1.0, Varlist[0]);
                        con++;
                    }
                    else
                        Tar.AddTerm(1.0, Varlist[0]);
                    //add -10000*f_fv to Tar
                    Tar.AddTerm(-10000, VarFlaglist[0]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                }

                //If f is the XOR of two variables(assumin that f=u1+u2+c), where c is a constant in {0,1}.
                //then we shoule calclate f_av and f_fv according to (u1_av, u2_av) and (u1_fv, u2_fv).
                if (Varlist.Count == 2)
                {
                    VarlistCopy.Clear();
                    GRBVar[] VarFlaglistCopy = new GRBVar[3];
                    GRBVar[] Vartemplist = new GRBVar[4];
                    //copy Varlist/VarFlaglist to VarlistCopy/VarFlaglistCopy
                    for (j = 0; j < Varlist.Count(); j++)
                    {
                        VarlistCopy.Add(Varlist[j]);
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }
                    VarlistCopy.Add(Uvar[loc1]);

                    //model u1_av+u2_av. Here, VarlistCopy[0]and VarlistCoyp[1] are u1_av and u2_av respectively.
                    AddCons2Vars(model, VarlistCopy);
                    //model u1_av+u2_av+c. if consflag[i]=1, then c is 1. In this case, we add an constant variable Consvar[loc0] to the model
                    //and model u1_av+u2_av+c.
                    if (consflag[i] == 1)
                    {
                        model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                        VarlistCopy[0] = Uvar[loc1];
                        VarlistCopy[1] = Consvar[loc0 - 1];
                        VarlistCopy[2] = Uvar[loc1 + 1];
                        //Console.WriteLine("*****\n" + loc1 + "\n******\n" + loc0 + "\n*****\n");
                        loc1++;
                        AddCons2Vars(model, VarlistCopy);
                    }

                    //until now, Uvar[loc1] is corresponding to beta_d in Proposition 2 of our paper.
                    //Then, we shall show how to obtain the final assignment/flag variable of f.

                    //UF flip the variables
                    model.AddConstr(NUvar[loc3++] == 1 - VarFlaglist[0], "Flip1" + (loc3).ToString());
                    model.AddConstr(NUvar[loc3++] == 1 - VarFlaglist[1], "Flip2" + (loc3).ToString());
                    VarFlaglistCopy[0] = NUvar[loc3 - 1];
                    VarFlaglistCopy[1] = VarFlaglist[0];
                    //Vvar[loc2++]=min(1 - VarFlaglist[1], VarFlaglist[0]). Vvar[loc2] corresponds to T_j in Proposition 2.
                    model.AddGenConstrMin(Vvar[loc2++], VarFlaglistCopy, 1000, "FlagTransOxr1" + loc2.ToString());
                    VarFlaglistCopy[0] = NUvar[loc3 - 2];
                    VarFlaglistCopy[1] = VarFlaglist[1];
                    //Vvar[loc2++]=min(1 - VarFlaglist[0], VarFlaglist[1]). Vvar[loc2] corresponds to T_j in Proposition 2.
                    model.AddGenConstrMin(Vvar[loc2++], VarFlaglistCopy, 1000, "FlagTransOxr2" + loc2.ToString());
                    //UFvar[loc1]=Vvar[loc2-1]+Vvar[loc2-2], UFvar[loc1] is the flag variable of f
                    // model.AddConstr(UFvar[loc1] == Vvar[loc2 - 1] + Vvar[loc2 - 2], "FlagTransOxr3" + loc2.ToString());
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        Vartemplist[j] = Vvar[loc2 - 1 - j];
                    }
                    //UFvar[locf]=max(Vvar[loc2],Vvar[loc2-1],...,). is equivalent to UFvar[locf]= Vvar[loc2]+Vvar[loc2-1]+...+
                    //since only one of Vvar[loc2],Vvar[loc2-1],..., is equal to 1. 
                    //UFvar[locf] is the flag variable of f, corresponding to F_d in Proposition 2.
                    model.AddGenConstrMax(UFvar[locf], Vartemplist, 0, "FlagTransXor-2" + loc2.ToString());

                    //NUFvar[loc4] =1-UFvar[loc1] and increase loc4 by 1. NUFvar[loc4] corresponds to 1-F_d in Proposition 2.
                    model.AddConstr(NUFvar[loc4++] == 1 - UFvar[locf], "Flip3" + loc4.ToString());
                    //max(u1_fv,u2_fv). bvar[locb] corresponds to b_d in Proposition 2. 
                    VarFlaglistCopy[0] = VarFlaglist[0];
                    VarFlaglistCopy[1] = VarFlaglist[1];
                    model.AddGenConstrMax(bvar[locb++], VarFlaglistCopy, 0, "maxfv" + locb.ToString());

                    //NUFvar[loc4] =1-bvar[locb++] and increase loc4 by 1. NUFvar[loc4] corresponds to 1-b_d in Proposition 2.
                    model.AddConstr(NUFvar[loc4++] == 1 - bvar[locb - 1], "Flip3" + loc4.ToString());

                    VarFlaglistCopy[0] = NUFvar[loc4 - 1];//corresponding to 1-F_d
                    VarFlaglistCopy[1] = NUFvar[loc4 - 2];//corresponding to 1-b_d
                    VarFlaglistCopy[2] = Uvar[loc1];//corresponding to beta_d
                    loc1++;
                    //Uvar[loc1]=min(NUFvar[loc4-1],NUFvar[loc4-2],loc1), is the  assigment variable of f.
                    model.AddGenConstrMin(Uvar[loc1], VarFlaglistCopy, 1000, "minav" + loc1.ToString());
                    //model.Update();
                    Tar.AddTerm(1.0, Uvar[loc1]);

                    Tar.AddTerm(-10000, UFvar[locf]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                    loc1++;
                    locf++;
                }
                //in this case, assume that f=u1+u2+u3+c
                if (Varlist.Count == 3)
                {
                    VarlistCopy.Clear();
                    GRBVar[] VarFlaglistCopy = new GRBVar[4];
                    GRBVar[] VarFlaglistCopy2 = new GRBVar[4];
                    GRBVar[] Vartemplist = new GRBVar[4];
                    for (j = 0; j < Varlist.Count(); j++)
                    {
                        VarlistCopy.Add(Varlist[j]);
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }
                    VarlistCopy.Add(Uvar[loc1]);
                    //Uvar[loc1]=u1+u2+u3
                    AddCons3Vars(model, VarlistCopy);
                    //model.AddGenConstrMax(UFvar[loc1], VarFlaglistCopy, 0, "TransFlagXor" + loc1.ToString());

                    //if c==1, namely f has the constant term 1, U[loc1+1]=Uvar[loc]+1
                    if (consflag[i] == 1)
                    {
                        model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                        VarlistCopy[0] = Uvar[loc1];
                        VarlistCopy[1] = Consvar[loc0 - 1];
                        VarlistCopy[2] = Uvar[loc1 + 1];
                        loc1++;
                        AddCons2Vars(model, VarlistCopy);
                    }

                    //filp every variable, 
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        model.AddConstr(NUvar[loc3++] == 1 - VarFlaglistCopy[j], "Flip" + j.ToString());
                    }
                    //
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        for (k = 0; k < Varlist.Count; k++)
                        {
                            if (k != j)
                                Vartemplist[k] = NUvar[loc3 - 1 - (Varlist.Count - 1 - k)];
                            else
                                Vartemplist[k] = VarFlaglistCopy[k];
                        }
                        //Vvar[loc2]=min(1-VarFlaglistCopy[0],1-VarFlaglistCopy[2],...,VarFlaglistCopy[j],1-VarFlaglistCopy[j],...)
                        model.AddGenConstrMin(Vvar[loc2++], Vartemplist, 1000, "FlagTransOxr1" + loc2.ToString());
                    }
                    
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        Vartemplist[j] = Vvar[loc2 - 1 - j];
                    }
                    //Since only one of Vvar[loc2 - 1],Vvar[loc2 - 2],..., is equal to 0 and the remainings are equal to 0s, 
                    //UFvar[loc1]=max(Vvar[loc2 - 1],Vvar[loc2 - 2],...,) is equivalently to UFvar[loc1]=Vvar[loc2 - 1]+Vvar[loc2 - 2]+...+
                    //UFvar[loc1]is the fianl flag variable of f
                    model.AddGenConstrMax(UFvar[locf], Vartemplist, 0, "FlagTransXor-2" + loc2.ToString());


                    //NUFvar[loc4] =1-UFvar[loc1] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - UFvar[locf], "Flip3" + loc4.ToString());
                    //max(u1_fv,u2_fv,u3_fv)
                    VarFlaglistCopy[0] = VarFlaglist[0];
                    VarFlaglistCopy[1] = VarFlaglist[1];
                    VarFlaglistCopy[2] = VarFlaglist[2];
                    model.AddGenConstrMax(bvar[locb++], VarFlaglistCopy, 0, "maxfv" + locb.ToString());

                    //NUFvar[loc4] =1-bvar[locb++] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - bvar[locb - 1], "Flip3" + loc4.ToString());

                    VarFlaglistCopy2[0] = NUFvar[loc4 - 1];
                    VarFlaglistCopy2[1] = NUFvar[loc4 - 2];
                    VarFlaglistCopy2[2] = Uvar[loc1];
                    loc1++;
                    //Uvar[loc1]=min(NUFvar[loc4-1],NUFvar[loc4-2],loc1), is the  assigment variable of f.
                    model.AddGenConstrMin(Uvar[loc1], VarFlaglistCopy2, 1000, "min_av" + loc1.ToString());


                    Tar.AddTerm(1.0, Uvar[loc1]);
                    Tar.AddTerm(-10000, UFvar[locf]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                    loc1++;
                    locf++;
                }
                //assuming f=u1+u2+u3+u4+c
                if (Varlist.Count == 4)
                {
                    List<GRBVar> templist = new List<GRBVar>() { };
                    GRBVar[] VarFlaglistCopy = new GRBVar[5];
                    GRBVar[] VarFlaglistCopy2 = new GRBVar[5];
                    GRBVar[] Vartemplist = new GRBVar[5];

                    for (j = 0; j < Varlist.Count(); j++)
                    {
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }

                    templist.Add(Varlist[0]);
                    templist.Add(Varlist[1]);
                    templist.Add(Uvar[loc1]);
                    AddCons2Vars(model, templist);

                    templist.Clear();
                    loc1++;
                    templist.Add(Varlist[2]);
                    templist.Add(Varlist[3]);
                    templist.Add(Uvar[loc1]);
                    AddCons2Vars(model, templist);


                    templist.Clear();
                    loc1++;
                    templist.Add(Uvar[loc1 - 2]);
                    templist.Add(Uvar[loc1 - 1]);
                    templist.Add(Uvar[loc1]);
                    AddCons2Vars(model, templist);
                    
                    //if c==1
                    if (consflag[i] == 1)
                    {
                        model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                        VarlistCopy[0] = Uvar[loc1];
                        VarlistCopy[1] = Consvar[loc0 - 1];
                        VarlistCopy[2] = Uvar[loc1 + 1];
                        loc1++;
                        AddCons2Vars(model, VarlistCopy);
                    }

                   
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        model.AddConstr(NUvar[loc3++] == 1 - VarFlaglistCopy[j], "Flip" + j.ToString());
                    }
                   
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        for (k = 0; k < Varlist.Count; k++)
                        {
                            if (k != j)
                                Vartemplist[k] = NUvar[loc3 - 1 - (Varlist.Count - 1 - k)];
                            else
                                Vartemplist[k] = VarFlaglistCopy[k];

                        }
                        model.AddGenConstrMin(Vvar[loc2++], Vartemplist, 1000, "FlagTransOxr1" + loc2.ToString());
                    }

                    for (j = 0; j < Varlist.Count; j++)
                    {
                        Vartemplist[j] = Vvar[loc2 - 1 - j];
                    }
                    model.AddGenConstrMax(UFvar[locf], Vartemplist, 0, "FlagTransXor-2" + loc2.ToString());

                    //NUFvar[loc4] =1-UFvar[loc1] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4] == 1 - UFvar[locf], "Flip3" + loc4.ToString());
                    loc4++;
                    //max(u1_fv,u2_fv,u3_fv,u4_fv)
                    VarFlaglistCopy[0] = VarFlaglist[0];
                    VarFlaglistCopy[1] = VarFlaglist[1];
                    VarFlaglistCopy[2] = VarFlaglist[2];
                    VarFlaglistCopy[3] = VarFlaglist[3];
                    model.AddGenConstrMax(bvar[locb++], VarFlaglistCopy, 0, "maxfv" + locb.ToString());

                    //NUFvar[loc4] =1-bvar[locb++] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4] == 1 - bvar[locb - 1], "Flip4" + loc4.ToString());
                    loc4++;
                    //VarFlaglistCopy.Initialize();

                    VarFlaglistCopy2[0] = NUFvar[loc4 - 1];
                    VarFlaglistCopy2[1] = NUFvar[loc4 - 2];
                    VarFlaglistCopy2[2] = Uvar[loc1];
                    loc1++;

                    //Uvar[loc1]=min(NUFvar[loc4-1],NUFvar[loc4-2],loc1), is the  assigment variable of f.
                    model.AddGenConstrMin(Uvar[loc1], VarFlaglistCopy2, 1000, "min_av" + loc1.ToString());

                    //Tar=Tar+assigment variable +(-10000)*flag variable
                    Tar.AddTerm(1.0, Uvar[loc1]);
                    Tar.AddTerm(-10000, UFvar[locf]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                    loc1++;
                    locf++;
                }
                //assuming f=u1+u2+u3+u4+u5+c
                if (Varlist.Count == 5)
                {

                    List<GRBVar> templist = new List<GRBVar>() { };
                    GRBVar[] VarFlaglistCopy = new GRBVar[6];
                    GRBVar[] VarFlaglistCopy2 = new GRBVar[6];
                    GRBVar[] Vartemplist = new GRBVar[6];

                    for (j = 0; j < Varlist.Count(); j++)
                    {
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }

                    templist.Add(Varlist[0]);
                    templist.Add(Varlist[1]);
                    templist.Add(Uvar[loc1]);
                    AddCons2Vars(model, templist);

                    templist.Clear();
                    loc1++;
                    templist.Add(Varlist[2]);
                    templist.Add(Varlist[3]);
                    templist.Add(Varlist[4]);
                    templist.Add(Uvar[loc1]);
                    AddCons3Vars(model, templist);


                    templist.Clear();
                    loc1++;
                    templist.Add(Uvar[loc1 - 2]);
                    templist.Add(Uvar[loc1 - 1]);
                    templist.Add(Uvar[loc1]);
                    AddCons2Vars(model, templist);


                    if (consflag[i] == 1)
                    {
                        model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                        VarlistCopy[0] = Uvar[loc1];
                        VarlistCopy[1] = Consvar[loc0 - 1];
                        VarlistCopy[2] = Uvar[loc1 + 1];
                        loc1++;
                        AddCons2Vars(model, VarlistCopy);
                    }

                    for (j = 0; j < Varlist.Count; j++)
                    {
                        model.AddConstr(NUvar[loc3++] == 1 - VarFlaglistCopy[j], "Flip" + j.ToString());
                    }
                    //flip and calculate Tj
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        for (k = 0; k < Varlist.Count; k++)
                        {
                            if (k != j)
                                Vartemplist[k] = NUvar[loc3 - 1 - (Varlist.Count - 1 - k)];
                            else
                                Vartemplist[k] = VarFlaglistCopy[k];

                        }
                        model.AddGenConstrMin(Vvar[loc2++], Vartemplist, 1000, "FlagTransOxr1" + loc2.ToString());
                    }
                    //
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        Vartemplist[j] = Vvar[loc2 - 1 - j];
                    }
                    model.AddGenConstrMax(UFvar[locf], Vartemplist, 0, "FlagTransXor-2" + loc2.ToString());


                    //NUFvar[loc4] =1-UFvar[loc1] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - UFvar[locf], "Flip3" + loc4.ToString());
                    //max(u1_fv,u2_fv,u3_fv,u4_fv,u5_fv)
                    VarFlaglistCopy[0] = VarFlaglist[0];
                    VarFlaglistCopy[1] = VarFlaglist[1];
                    VarFlaglistCopy[2] = VarFlaglist[2];
                    VarFlaglistCopy[3] = VarFlaglist[3];
                    VarFlaglistCopy[4] = VarFlaglist[4];
                    model.AddGenConstrMax(bvar[locb++], VarFlaglistCopy, 0, "maxfv" + locb.ToString());

                    //NUFvar[loc4] =1-bvar[locb++] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - bvar[locb - 1], "Flip4" + loc4.ToString());

                    VarFlaglistCopy2[0] = NUFvar[loc4 - 1];
                    VarFlaglistCopy2[1] = NUFvar[loc4 - 2];
                    VarFlaglistCopy2[2] = Uvar[loc1];
                    loc1++;
                    
                    //Uvar[loc1]=min(NUFvar[loc4-1],NUFvar[loc4-2],loc1), is the  assigment variable of f.
                    model.AddGenConstrMin(Uvar[loc1], VarFlaglistCopy2, 1000, "min_av" + loc1.ToString());
                    Tar.AddTerm(1.0, Uvar[loc1]);

                    Tar.AddTerm(-10000, UFvar[locf]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                    loc1++;
                    locf++;
                }
                //Console.WriteLine("5: " +loc1);
                //assuming that f=u1+u2+u3+u4+u5...+um+c, where m>5.
                if (Varlist.Count > 5)
                {
                    int length;
                    length = Varlist.Count;
                    List<GRBVar> curvarlist = new List<GRBVar>() { };
                    GRBVar[] VarFlaglistCopy = new GRBVar[Varlist.Count + 1];
                    GRBVar[] VarFlaglistCopy2 = new GRBVar[Varlist.Count + 1];
                    GRBVar[] Vartemplist = new GRBVar[Varlist.Count + 1];

                    for (j = 0; j < Varlist.Count; j++)
                    {
                        curvarlist.Add(Varlist[j]);
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }

                    //model f=u1+u2+...+un. The method used here is a slight differnt from that presented in the paper. 
                    //The method used here is more convenient for coding.
                    while (length > 3)
                    {
                        List<GRBVar> nextvarlist = new List<GRBVar>() { };
                        List<GRBVar> templist = new List<GRBVar>() { };
                        //
                        templist.Add(curvarlist[0]);
                        templist.Add(curvarlist[1]);
                        templist.Add(curvarlist[2]);
                        templist.Add(Uvar[loc1]);
                        AddCons3Vars(model, templist);

                        for (j = 3; j < curvarlist.Count; j++)
                        {
                            nextvarlist.Add(curvarlist[j]);
                        }
                        nextvarlist.Add(Uvar[loc1]);
                        curvarlist.Clear();
                        for (j = 0; j < nextvarlist.Count; j++)
                        {
                            curvarlist.Add(nextvarlist[j]);
                        }
                        length = curvarlist.Count;
                        loc1++;
                    }

                    if (length == 2)
                    {
                        curvarlist.Add(Uvar[loc1]);
                        AddCons2Vars(model, curvarlist);
                        //if c==1, then we need xor one more constant
                        if (consflag[i] == 1)
                        {
                            model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                            VarlistCopy[0] = Uvar[loc1];
                            VarlistCopy[1] = Consvar[loc0 - 1];
                            VarlistCopy[2] = Uvar[loc1 + 1];
                            loc1++;
                            AddCons2Vars(model, VarlistCopy);
                        }
                    }

                    if (length == 3)
                    {
                        curvarlist.Add(Uvar[loc1]);
                        AddCons3Vars(model, curvarlist);
                        //if c==1, then we need xor one more constant
                        if (consflag[i] == 1)
                        {
                            model.AddConstr(Consvar[loc0++] == 1, "Flip" + j.ToString());
                            VarlistCopy[0] = Uvar[loc1];
                            VarlistCopy[1] = Consvar[loc0 - 1];
                            VarlistCopy[2] = Uvar[loc1 + 1];
                            loc1++;
                            AddCons2Vars(model, VarlistCopy);
                        }
                    }
                    
                    

                    for (j = 0; j < Varlist.Count; j++)
                    {
                        model.AddConstr(NUvar[loc3++] == 1 - VarFlaglistCopy[j], "Flip" + j.ToString());
                    }

                    for (j = 0; j < Varlist.Count; j++)
                    {
                        for (k = 0; k < Varlist.Count; k++)
                        {
                            if (k != j)
                                Vartemplist[k] = NUvar[loc3 - 1 - (Varlist.Count - 1 - k)];
                            else
                                Vartemplist[k] = VarFlaglistCopy[k];

                        }
                        model.AddGenConstrMin(Vvar[loc2++], Vartemplist, 1000, "FlagTransOxr1" + loc2.ToString());
                    }
                    for (j = 0; j < Varlist.Count; j++)
                    {
                        Vartemplist[j] = Vvar[loc2 - 1 - j];
                    }
                    model.AddGenConstrMax(UFvar[locf], Vartemplist, 0, "FlagTransXor-2" + loc2.ToString());

                    //NUFvar[loc4] =1-UFvar[loc1] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - UFvar[locf], "Flip3" + loc4.ToString());
                    //bvar[locb]=max(u1_fv,u2_fv,u3_fv,u4_fv,...,un_fv) and increase locb by 1
                    for (j = 0; j < VarFlaglist.Count; j++)
                    {
                        VarFlaglistCopy[j] = VarFlaglist[j];
                    }
                    model.AddGenConstrMax(bvar[locb++], VarFlaglistCopy, 0, "maxfv" + locb.ToString());

                    //NUFvar[loc4] =1-bvar[locb++] and increase loc4 by 1
                    model.AddConstr(NUFvar[loc4++] == 1 - bvar[locb - 1], "Flip4" + loc4.ToString());

                    VarFlaglistCopy2[0] = NUFvar[loc4 - 1];
                    VarFlaglistCopy2[1] = NUFvar[loc4 - 2];
                    VarFlaglistCopy2[2] = Uvar[loc1];
                    loc1++;
                    //Uvar[loc1]=min(NUFvar[loc4-1],NUFvar[loc4-2],loc1), is the  assigment variable of f.
                    model.AddGenConstrMin(Uvar[loc1], VarFlaglistCopy2, 1000, "min_av" + loc1.ToString());

                    Tar.AddTerm(1.0, Uvar[loc1]);
                    Tar.AddTerm(-10000, UFvar[locf]);
                    rec_loc1[i] = loc1;
                    rec_locf[i] = locf;
                    loc1++;
                    locf++;
                }
            }
            
            //
            GRBLinExpr keyspace = new GRBLinExpr();
            for (i = 0; i < 80; i++)
            {
                keyspace.AddTerm(1.0, kvf[i]);
            }
            Tar.AddConstant((double)con);
            
            //Add the condition that Tar>=0. It gurantees that there is not any condition whose flag variable is equal to 1.
            model.AddConstr(Tar ==3, "finalcons");
            model.SetObjective(keyspace, GRB.MAXIMIZE);
            //Set the objective function of the model.
            //model.SetObjective(Tar, GRB.MINIMIZE);
            model.Optimize();

            //output the states of key variables and iv variables
            if (model.SolCount > 0)
            {
                sw.WriteLine("****************************Conditions on single key/iv variables***************************\n");
                Console.Write("Free Key bits:\n");
                sw.Write("Free Key bits:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((keyvar[i].X == 0) && (kvf[i].X == 1))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                Console.Write("Key bits set to 1:\n");
                sw.Write("Key bits set to 1:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((keyvar[i].X == 1) && (kvf[i].X == 0))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                Console.Write("Key bits set 0:\n");
                sw.Write("Key bits set 0:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((keyvar[i].X == 0) && (kvf[i].X == 0))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                //for (i = 0; i < loc2; i++)
                //{
                //    if (Vvar[i].X > 1)
                //    {
                //        Console.Write(Vvar[i].VarName + " " + Vvar[i].X + "\n");
                //    }

                //}

                Console.WriteLine("*******************************************************\n");
                sw.WriteLine("*******************************************************\n");

                Console.Write("Free Iv bits:\n");
                sw.Write("Free Iv bits:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((ivvar[i].X == 0) && (vvf[i].X == 1))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                Console.Write("Iv bits set to 1:\n");
                sw.Write("Iv bits set to 1:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((ivvar[i].X == 1) && (vvf[i].X == 0))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                Console.Write("Iv bits set to 0:\n");
                sw.Write("Iv bits set to 0:\n");
                for (i = 0; i < 80; i++)
                {
                    if ((ivvar[i].X == 0) && (vvf[i].X == 0))
                    {
                        Console.Write(i + ",");
                        sw.Write(i + ",");
                    }
                }
                Console.WriteLine();
                sw.WriteLine();

                double aa = Tar.Value;
                Console.WriteLine("*****************The number of 1's*****************");
                Console.WriteLine(aa);
                Console.WriteLine("***************************************************");
                sw.WriteLine("***********************The assigment/flag variable of key and iv variables***********************");
                for (i = 0; i < 80; i++)
                {
                    Console.Write(keyvar[i].VarName + "= " + keyvar[i].X + " " + kvf[i].VarName + "= " + kvf[i].X + "\n");
                    sw.Write(keyvar[i].VarName + "= " + keyvar[i].X + " " + kvf[i].VarName + "= " + kvf[i].X + "\n");
                }
                Console.WriteLine("*******************************");
                for (i = 0; i < 80; i++)
                {
                    Console.Write(ivvar[i].VarName + "= " + ivvar[i].X + " " + vvf[i].VarName + "= " + vvf[i].X + "\n");
                    sw.Write(ivvar[i].VarName + "= " + ivvar[i].X + " " + vvf[i].VarName + "= " + vvf[i].X + "\n");
                }
                Console.WriteLine();
                //
                StreamReader rw = new StreamReader("ConSet.txt");
                sw.WriteLine("***************The concrete conditions****************");
                for (j = 0; j < consnum; j++)
                {
                    string onecon = rw.ReadLine();
                    sw.WriteLine(onecon + "=" + Uvar[rec_loc1[j]].X);
                }

                sw.Close();
            }
        }
    }
}
