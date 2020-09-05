using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace VerifyCondition
{
    class Program
    {
        static public void ReadCondition(UInt32[,,] ConS, int[] conlen, int[] consflag)
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
                    for (k = 0; k < 8; k++)
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
                                if (temp2[k].Length == (5 + ind))
                                {
                                    t = (temp2[k][2 + ind] - '0') * 100 + (temp2[k][3 + ind] - '0') * 10 + (temp2[k][4 + ind] - '0');
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
                                if (temp2[k].Length == (5 + ind))
                                {
                                    t = (temp2[k][2 + ind] - '0') * 100 + (temp2[k][3 + ind] - '0') * 10 + (temp2[k][4 + ind] - '0');
                                }
                                ConS[m, i, (t >> 5) + 4] = ConS[m, i, (t >> 5) + 4] | ((UInt32)(0x01 << (t & 0x1f)));
                            }
                            //  Console.Write(t + ",");
                        }
                    }
                }
                m++;
            }
            conset.Close();
            //输出条件，进行验证

            for (i = 0; i < 1; i++)
            {
                for (m = 0; m < conlen[i]; m++)
                {

                    for (j = 0; j < 4; j++)
                    {
                        for (k = 0; k < 32; k++)
                        {
                            if (((ConS[i, m, j] >> k) & 0x01) == 1)
                            {
                                Console.Write("v(" + (32 * j + k).ToString() + ")");
                            }
                        }
                    }

                    for (j = 0; j < 4; j++)
                    {
                        for (k = 0; k < 32; k++)
                        {
                            if (((ConS[i, m, j + 4] >> k) & 0x01) == 1)
                            {
                                Console.Write("k(" + (32 * j + k).ToString() + ")");
                            }
                        }
                    }

                    Console.Write("+");
                }
                Console.WriteLine();
            }


        }
        static void Main(string[] args)
        {
           
           //弱密钥条件,和IV条件
           // verify the condition C_1, C_2, C_3
           /*int [] keysetto0=new int[]{1,2,4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,26,28,29,31,32,34,35,37,38,40,41,43,44,46,47,49,50,52,53,55,56,58,59,61,62,64,65,66,68,70,71,73,74,76,77,79};
           int [] keysetto1=new int[]{67};
           int [] ivsetto0=new int[]{1,2,4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,26,28,29,31,32,34,35,37,38,40,41,43,44,46,47,49,50,52,53,55,56,58,59,61,62,64,65,67,68,70,71,73,74,76,77,79};

           List<int> cubevar=new List<int>(){0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66, 69,72,75,78};*/
           
           // verify the condition C_4, C_5, C_6
           int [] keysetto0=new int[]{0,2,3,5,6,8,9,11,12,14,15,17,18,20,21,23,24,26,27,29,30,32,33,35,36,38,39,41,42,44,45,47,48,50,51,53,54,56,57,59,60,62,63,65,66,68,69,71,72,74,75,77,78};
           int [] keysetto1=new int[]{67};
           int [] ivsetto0=new int[]{0,2,3,5,6,8,9,11,12,14,15,17,18,20,21,23,24,26,27,29,30,32,33,35,36,38,39,41,42,44,45,47,48,50,51,53,54,56,57,59,60,62,63,65,66,68,69,71,72,74,75,77,78};

           List<int> cubevar=new List<int>(){1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 37, 40, 43, 46, 49, 52, 55, 58, 61, 64, 67, 70, 73, 76, 79};


           //
            UInt32[,,] ConS = new UInt32[2000, 800, 8];//used to store the conditions derived to control the propagation of difference
            int[] conlen = new int[2000];
            int[] consflag = new int[2000];
            int[] rec_locf = new int[2000];
            int[] conval=new int[2000];

            StreamWriter res=new StreamWriter("CondiVal.txt");
            //
            int consnum=656;
            int i=0,j=0,k=0;
            int sum=0;
            int tmp1=0,tmp2=0,tmp=0;
            int[] val=new int [256];

            for(i=0;i<256;i++)
            {
                val[i]=2;
            }
            for(i=0;i<keysetto0.Count();i++)
            {
                val[128+keysetto0[i]]=0;
            }
            for(i=0;i<keysetto1.Count();i++)
            {
                val[128+keysetto1[i]]=1;
            }
            //val[128+66]=1;
            /*for(i=0;i<80;i++)
            {
                if(!cubevar.Contains(i))
                {
                    val[i]=0;
                }
            }*/
            for(i=0;i<53;i++)
            {
                val[ivsetto0[i]]=0;
            }
            
            ReadCondition(ConS, conlen, consflag);
            Console.Write("ReadConditionDone\n");

            for(i=0;i<consnum;i++)
            {
                conval[i]=0;
                sum=0;
                for(j=0;j<conlen[i];j++)
                {
                    tmp1=1;
                    //IV变元
                    for(k=0;k<80;k++)
                    {
                        tmp=(int)(ConS[i,j,(k>>5)]>>(k&0x1f))&0x01;
                        if(tmp==1)
                            tmp1*=val[k];
                    }

                    for(k=0;k<80;k++)
                    {
                        tmp=(int)(ConS[i,j,4+(k>>5)]>>(k&0x1f))&0x01;
                        if(tmp==1)
                            tmp1*=val[k+128];
                    }
                    if(tmp1==2)
                    {
                        sum=2;
                        break;
                    }
                    else
                    {
                        sum+=tmp1;
                        sum&=0x01;
                    }
                    if(i==112)
                       Console.Write(conlen[i]+" sum="+sum+"\n");
                }
               
                if(sum!=2)
                    sum^=consflag[i];
                conval[i]=sum;
                if(i==112)
                    Console.Write(conlen[i]+" Finalsum="+sum+"\n");
            }

            for(i=0;i<consnum;i++)
            {
                 //Console.Write(conval[i]+",");
                // res.Write(conval[i]+",");
                if(conval[i]==1)
                {
                    Console.Write(i+"\n");
                    res.Write(i+"\n");
                }
            }
            res.Close();
        }
    }
}
