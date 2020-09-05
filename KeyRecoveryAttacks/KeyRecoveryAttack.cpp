//实现密钥穷举攻击和立方攻击的实验对比
#include<stdio.h>
#include<stdlib.h>
#include<time.h>
typedef unsigned int u32;
typedef unsigned char u8;
void ECRYPT_keystream_words(u32 keystream[],u32 iv[], u32 key[], u32 roundnum)               
{
    u32 z1;
	//z = Trivium_update_funcion_word(ctx);
	//�����ú���ֱ��д
  u32 roundnum_word;
  u32 roundnum_bit;
  u32 t1,t2,t3,i;
  u32 s0,s1,s2,s3,s4,s5,s6,s7,s8,s9;
  //u32 temp1,temp2,temp3,temp4,temp5,temp6,s91,s92,s171,s175,s176,s264,s286,s287,s69;
  u32 temp1,temp2,temp3,temp4,temp5,temp6;
  u32 z;
 s0 = key[0]^(key[1]<<8)^(key[2]<<16)^(key[3]<<24);
 s1 = key[4]^(key[5]<<8)^(key[6]<<16)^(key[7]<<24);
 s2 = key[8]^(key[9]<<8);

 s3 = iv[0]^(iv[1]<<8)^(iv[2]<<16)^(iv[3]<<24);
 s4 = iv[4]^(iv[5]<<8)^(iv[6]<<16)^(iv[7]<<24);
 s5 = iv[8]^(iv[9]<<8);

 s6 = 0;
 s7 = 0;
 s8 = 0;
 s9 = 0x00007000;
  
 // 32*36 = 1152
  roundnum_word=roundnum/32;
  roundnum_bit=roundnum%32;
  for(i=0;i<roundnum_word;i++)
  {

	temp1 = (s2<<30)|(s1>>2);//���֮���൱��ֱ�Ӽ���
	temp2 = (s2<<3)|(s1>>29);
	temp3 = (s5<<27)|(s4>>5);
	temp4 = (s5<<12)|(s4>>20);
	temp5 = (s8<<30)|(s7>>2);
	temp6 = (s9<<17)| (s8>>15);

	t1 = temp1^temp2;//((s2<<30)|(s1>>2))^((s2<<3)|(s1>>29))^(((s2<<5)|(s1>>27))&((s2<<4)|(s1>>28)))^(s5<<18)|(s4>>14);

	t2 = temp3^temp4;//((s5<<27)|(s4>>5))^((s5<<12)|(s4>>20))^((s5<<14)|(s4>>18))&((s5<<13)|(s4>>19))^((s8<<9)|(s7>>23));

	t3 = temp5^temp6;//((s8<<30)|(s7>>2))^((s9<<17)| (s8>>15))^

	//z = t1^t2^t3;
	
	//����t1,t2,t3
//	t1 = t1 + s91s92 + s171
	temp1 = (s2<<5)|(s1>>27);//(((s2<<5)|(s1>>27))&((s2<<4)|(s1>>28)))^(s5<<18)|(s4>>14);
	temp2 = (s2<<4)|(s1>>28);
	temp3 = (s5<<18)|(s4>>14);

	t1 ^= (temp1&temp2)^temp3;

//	t2 = t2 + s175s176 + s264
	temp1 = (s5<<14)|(s4>>18);//(((s5<<14)|(s4>>18))&((s5<<13)|(s4>>19)))^((s8<<9)|(s7>>23));
	temp2 = (s5<<13)|(s4>>19);
	temp3 = (s8<<9)|(s7>>23);

	t2 ^= (temp1&temp2)^temp3;

//	t3 = t3 + s286s287 + s69
	temp1 = (s9<<19)|(s8>>13);//(()&())^()
	temp2 = (s9<<18)|(s8>>14);
	temp3 = (s2<<27)|(s1>>5);

	t3 ^= (temp1&temp2)^temp3;

	// update register 1
	s2 = (s1)&(0x1FFFFFFF);
	s1 =s0;
	s0 = t3;

//	update register 2
	s5 =s4&(0x000FFFFF);
	s4 =s3;
	s3 = t1;

//	update register 3
	s9 =s8&(0x00007FFF);
	s8 =s7;
	s7 =s6;
	s6 = t2;
  }
  if(roundnum_bit!=0)
  {
	temp1 = (s2<<30)|(s1>>2);//���֮���൱��ֱ�Ӽ���
	temp2 = (s2<<3)|(s1>>29);
	temp3 = (s5<<27)|(s4>>5);
	temp4 = (s5<<12)|(s4>>20);
	temp5 = (s8<<30)|(s7>>2);
	temp6 = (s9<<17)| (s8>>15);

	t1= temp1^temp2;
	t2= temp3^temp4;
	t3= temp5^temp6;
	z1 = temp1^temp2^temp3^temp4^temp5^temp6;
	//z1=z;
	//����t1,t2,t3
	//t1 = t1 + s91s92 + s171
	temp1 = (s2<<5)|(s1>>27);
	temp2 = (s2<<4)|(s1>>28);
	temp3 = (s5<<18)|(s4>>14);

	t1 ^= (temp1&temp2)^temp3;

	//t2 = t2 + s175s176 + s264
	temp1 = (s5<<14)|(s4>>18);
	temp2 = (s5<<13)|(s4>>19);
	temp3 = (s8<<9)|(s7>>23);

	t2 ^= (temp1&temp2)^temp3;

	//t3 = t3 + s286s287 + s69
	temp1 = (s9<<19)|(s8>>13);
	temp2 = (s9<<18)|(s8>>14);
	temp3 = (s2<<27)|(s1>>5);

	t3 ^= (temp1&temp2)^temp3;

	// update register 1
	s2 = (s1)&(0x1FFFFFFF);
	s1 =s0;
	s0 = t3;

	//update register 2
	s5 =s4&(0x000FFFFF);
	s4 =s3;
	s3 = t1;

	//update register 3
	s9 =s8&(0x00007FFF);
	s8 =s7;
	s7 =s6;
	s6 = t2;
  }
	temp1 = (s2<<30)|(s1>>2);//���֮���൱��ֱ�Ӽ���
	temp2 = (s2<<3)|(s1>>29);
	temp3 = (s5<<27)|(s4>>5);
	temp4 = (s5<<12)|(s4>>20);
	temp5 = (s8<<30)|(s7>>2);
	temp6 = (s9<<17)| (s8>>15);
	z = temp1^temp2^temp3^temp4^temp5^temp6;
	if(roundnum_bit!=0)
		z=(z1<<roundnum_bit)|(z>>(32-roundnum_bit));
	//keystream[0]=reverse_word(z);
	temp1=0;
	temp1 = ((z&0x000000FF)<<24)^((z&0x0000FF00)<<8)^((z&0x00FF0000)>>8)^((z&0xFF000000)>>24);
	temp1 = ((temp1&0x01010101)<<7)^((temp1&0x02020202)<<5)^((temp1&0x04040404)<<3)^((temp1&0x08080808)<<1)^((temp1&0x10101010)>>1)^((temp1&0x20202020)>>3)^((temp1&0x40404040)>>5)^((temp1&0x80808080)>>7);
	keystream[0]=temp1;
}

int main()
{

    //首先是遍历密钥
    u32 key[10]={0x32,0xcb,0xb4,0x50,0x23,0xa5,0x60,0xd6,0x33,0xd1};
	u32 keybak[10]={0};
    u32 iv[10]={0};
    u32 keystream[10]={0},keystreamRight[10]={0};
    u32 roundnum=968;
    u32 sum=0;
    int freekeybits[80]={0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63,66,67,69,72,75,78};
	u32 cube[40]={0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 66,69};
	int dim=24;
    int i,j,k;
    //choose a key randomly
    
	//
	int keysetto0[80]={1,2,4,5,7,8,10,11,13,14,16,17,19,20,22,23,25,26,28,29,31,32,34,35,37,38,40,41,43,44,46,47,49,50,52,53,55,56,58,59,61,62,64,65,68,70,71,73,74,76,77,79};
	int keysetto1[80]={67};
	int key0num=52;
	int key1num=1;
	u32 tttt=0,loc;
	int remkeyset[100]={0};
    int lin[15]={4,6,9,11,15,17,18,19,20};
	int remkeynum=0;
    int linlen=8;
    int tmpkey;
	int knownkb;

	int t=0;
	int time1[1000]={0};
	int time2[1000]={0};
	int keynum=500;
	FILE *fp;
	fopen_s(&fp,"keyrec","a");
	fprintf(fp,"Tested Keys!!!!!\n\n");
	srand((unsigned int) time(NULL));
	for(t=0;t<keynum;t++)
	{
		printf("%d-th key\n",t);
		
		for(i=0;i<10;i++)
		{
			key[i]=rand()%256;
			//keybak[i]=key[i];
		}
		for(i=0;i<10;i++)
		{
			fprintf(fp,"0x%x,",key[i]);
		}
		fprintf(fp,"\n");
		for(i=0;i<key0num;i++)
		{
			tttt=0xff;
			tttt=tttt^(0x01<<(keysetto0[i]&0x07));
			key[keysetto0[i]>>3] &=tttt;
		}
		for(i=0;i<0;i++)
		{
			tttt=(0x01<<(keysetto1[i]&0x7));
			key[keysetto1[i]>>3] |=tttt;
		}
		printf("k66=%d k67=%d\n", (key[66>>3]>>(66&0x07))&0x01,((key[67>>3]>>(67&0x07))&0x01));
		for(i=0;i<10;i++)
		{
			iv[i]=0;
			keybak[i]=key[i];
		}
		// generate key stream bits
		ECRYPT_keystream_words(keystreamRight,iv,key,roundnum);

		//brute-force attack
		clock_t t1,t2;
		t1=clock();
		for(i=0;i<0x10000000;i++)
		{
			for(j=0;j<10;j++)
			{
				key[j]=0;
				//key[j]=keybak[j];
			}
			for(j=0;j<0;j++)
			{
				tttt=(0x01<<(keysetto1[j]&0x7));
				key[keysetto1[j]>>3] |=tttt;
			}
			for(j=0;j<28;j++)
			{
				key[freekeybits[j]>>3]|=((i>>j)&0x01)<<(freekeybits[j]&0x07);
			}
			ECRYPT_keystream_words(keystream,iv,key,roundnum);
			if(keystream[0]==keystreamRight[0])
			{
				printf("i=%x Succeed!!!\n",i);
				loc=i;
				break;
			}
		}
		t2=clock();
		printf("time used %d ms\n", t2-t1);
		time1[t]=t2-t1;
		//立方集求和
		t1=clock();
		sum=0;
		for(i=0;i<0x1000000;i++)
		{
			for(j=0;j<10;j++)
			{
				iv[j]=0;
			}
			for(j=0;j<dim;j++)
			{
				iv[cube[j]>>3]|=((i>>j)&0x01)<<(cube[j]&0x07);
			}
			ECRYPT_keystream_words(keystream,iv,key,roundnum);
			sum^=keystream[0];
		}
		int a1=sum&1;//968
		int a2=(sum>>9)&0x01;//977
		
		
		for(i=0;i<10;i++)
		{
			iv[i]=0;
		}
		//
		int k66,k67,flag=0;
		for(i=0;i<0x4000000;i++)
		//for(i=1;i<2;i++)
		{
			
			for(k=0;k<4;k++)
			{
				k67=(k>>1)&0x01;
				k66=(k)&0x01;
				//case 1
				if((k66==0) && (k67==0) &&(flag==0))
				{
					//建立的方程是968轮的, 二次方程, 无单挂的项
					int remkeyset1[26]={0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 69, 72, 75, 78};
					int remkeynum1=26;
					int lin1[9]={0, 4, 5, 9, 11, 12, 16, 18, 20};
					int qua1[161][2]={{0,3},{0,4},{0,6},{0,7},{0,14},{0,15},{0,19},{0,20},{0,23},{0,24},\
									  {1,3},{1,7},{1,8},{1,9},{1,10},{1,14},{1,16},{1,18},{1,20},{1,22},{1,24},\
									  {2,3},{2,5},{2,6},{2,8},{2,9},{2,11},{2,14},{2,16},{2,17},{2,23},{2,24},\
									  {3,7},{3,8},{3,10},{3,11},{3,12},{3,14},{3,15},{3,18},{3,19},{3,20},{3,21},{3,25},{4,5},{4,9},{4,13},{4,15},{4,16},{4,17},{4,18},{4,20},{4,22},{5,7},{5,8},{5,9},{5,10},{5,14},{5,15},{5,16},{5,17},{5,20},{5,21},{6,7},{6,8},{6,9},{6,12},{6,14},{6,16},{6,17},{6,18},{6,23},{6,24},{6,25},{7,8},{7,10},{7,12},{7,13},{7,15},{7,16},{7,19},{7,20},{7,23},{7,24},{8,9},{8,11},{8,13},{8,15},{8,21},{8,22},{8,23},{8,24},{9,10},{9,11},{9,14},{9,15},{9,16},{9,17},{9,19},{9,20},{9,21},{9,22},{9,23},{10,11},{10,12},{10,13},{10,16},{10,17},{10,19},{10,20},{10,22},{10,25},{11,12},{11,13},{11,17},{11,18},{11,19},{11,20},{11,21},{11,23},{11,25},{12,13},{12,15},{12,16},{12,17},{12,19},{12,21},{12,23},{12,24},{12,25},{13,15},{13,16},{13,20},{13,24},{14,15},{14,18},{14,22},{14,23},{14,25},{15,18},{15,19},{15,22},{15,24},{15,25},{16,17},{16,18},{16,19},{16,22},{16,23},{17,19},{17,21},{17,23},{17,24},{18,22},{18,24},{19,21},{19,22},{21,23},{22,23},{22,25},{23,24}};
					
					int linlen1=9;
					int qualen1=161;
					int tmpsum=0;
					int d1,d2;
					int pinv[80]={0, 0, 0, 1, 0, 0, 2, 0, 0, 3, 0, 0, 4, 0, 0, 5, 0, 0, 6, 0, 0, 7, 0, 0, 8, 0, 0, 9, 0, 0, 10, 0, 0, 11, 0, 0, 12, 0, 0, 13, 0, 0, 14, 0, 0, 15, 0, 0, 16, 0, 0, 17, 0, 0, 18, 0, 0, 19, 0, 0, 20, 0, 0, 21, 0, 0, 0, 0, 0, 22, 0, 0, 23, 0, 0, 24, 0, 0, 25, 0};
					int bini[26]={0};
					for(j=0;j<26;j++)
					{
						bini[j]=(i>>j)&0x01;
					}

					tmpsum=0;
					for(j=0;j<linlen1;j++)
					{
						tmpsum^=((i>>lin1[j])&0x01);
						//printf("tmpsum1=%d\n",tmpsum);
					}
					
					for(j=0;j<qualen1;j++)
					{
						//d1=(i>>qua1[j][0])&0x01;
						//d2=(i>>qua1[j][1])&0x01;
						d1=bini[qua1[j][0]];
						d2=bini[qua1[j][1]];
						tmpsum^=(d1&d2);
						//printf("tmpsum1=%d\n",tmpsum);
					}
					tmpsum^=1;
					tmpsum^=a1;
					if(tmpsum==0)
					{
						for(j=0;j<10;j++)
						{
							key[j]=0;
						}
						for(j=0;j<remkeynum1;j++)
						{
							key[remkeyset1[j]>>3]|=(((i>>j)&(0x01))<<(remkeyset1[j]&0x07));
						}
						//key[7]=keybak[7];
						ECRYPT_keystream_words(keystream,iv,key,roundnum);
						if(keystream[0]==keystreamRight[0])
						{
							printf("Succeed!!! Case 1\n");
							flag=1;
							break;
						}
					}
				}
				//case 2
				if((k66==1)&&(k67==0)&&(flag==0)&&(i<0x2000000))
				{
					int remkeyset2[100]={0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 63,69,72,75,78};
					int lin2[15]={4,6,9,11,15,17,18,19,20};
					int linlen2=8;
					int remkeynum2=25;
					int looplenth=0x01<<remkeynum2;
					//建立的方程式968, 线性方程
					//for(i=0;i<looplenth;i++)
					{
						for(j=0;j<10;j++)
						{
							key[j]=0;
						}
						tttt=(0x01<<(66&0x7));
						key[66>>3] |=tttt;
						for(j=0;j<remkeynum2;j++)
						{
							key[remkeyset2[j]>>3]|=(((i>>j)&(0x01))<<(remkeyset2[j]&0x07));
						}
						for(j=0;j<0;j++)
						{
							key[j]=keybak[j];
						}
						tmpkey=0;
						for(j=0;j<linlen2;j++)
							tmpkey^=((i>>lin[j])&0x01);
						if((tmpkey^a1)==1)
						{
							key[60>>3]|=(0x01)<<(60&0x07);
							//printf("k(60)=1\n");
						}
						//key[7]=keybak[7];
						ECRYPT_keystream_words(keystream,iv,key,roundnum);
						if(keystream[0]==keystreamRight[0])
						{
							printf("Succeed!!! Case 2\n");
							flag=1;
							break;
						}
					}
				}
				//case 3
				if((k66==0)&&(k67==1) &&(flag==0)&&(i<0x2000000))
				{
					//建立的方程是977
					int remkeyset3[25]={0, 3, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60, 63, 69, 72, 75, 78};
					int remkeynum3=25;
					int looplenth;
					int keylin3[100]={3,9,12,15,18,24,27,30,54,69,72,78};
					int lin3[100]={1, 2, 3, 4, 5, 7, 8, 9, 17, 21, 22, 24};
					int linlen3=12;
					int keyqua3[200][2]={{0,30},{0,57},{3,30},{3,57},{12,30},{12,57},{15,30},{15,57},{27,30},{27,57},\
									  {30,33},{30,36},{30,42},{30,48},{30,51},{30,57},{30,60},{30,63},{30,69},{30,78},\
									  {57,33},{57,36},{57,42},{57,48},{57,51},{57,60},{57,63},{57,69},{57,78}};
					int qua3[200][2]={{0, 9},{0, 18},{1, 9},{1, 18},{3, 9},{3, 18},{4, 9},{4, 18},{8, 9},{8, 18},{9, 10},\
									  {9, 11},{9, 13},{9, 15},{9, 16},{9, 18},{9, 19},{9, 20},{9, 21},{9, 24},{18, 10},\
									  {18, 11},{18, 13},{18, 15},{18, 16},{18, 19},{18, 20},{18, 21},{18, 24}};
					int qualen3=29;
					int t1,t2;
					//int pinv[80]={0, 0, 0, 1, 0, 0, 0, 0, 0, 2, 0, 0, 3, 0, 0, 4, 0, 0, 5, 0, 0, 6, 0, 0, 7, 0, 0, 8, 0, 0, 9, 0, 0, 10, 0, 0, 11, 0, 0, 12, 0, 0, 13, 0, 0, 14, 0, 0, 15, 0, 0, 16, 0, 0, 17, 0, 0, 18, 0, 0, 19, 0, 0, 20, 0, 0, 0, 0, 0, 21, 0, 0, 22, 0, 0, 23, 0, 0, 24, 0};
					//需要将k66和k67的值也放进去
					//i=0;
					//for(i=0;i<looplenth;i++)
					{
						for(j=0;j<10;j++)
						{
							key[j]=0;
						}
						tttt=(0x01<<(67&0x7));
						key[67>>3] |=tttt;
						for(j=0;j<remkeynum3;j++)
						{
							key[remkeyset3[j]>>3]|=(((i>>j)&(0x01))<<(remkeyset3[j]&0x07));
						}
						tmpkey=0;
						//建立方程
						for(j=0;j<linlen3;j++)
							tmpkey^=((i>>lin3[j])&0x01);
						for(j=0;j<qualen3;j++)
						{
							t1= (i>>(qua3[j][0]))&0x01;
							t2= (i>>(qua3[j][1]))&0x01;
							tmpkey=tmpkey^(t1&t2);
						}

						if((tmpkey^a2)==1)
						{
							key[6>>3]|=(0x01)<<(6&0x07);
							//printf("k(60)=1\n");
						}
					

						//key[7]=keybak[7];
						ECRYPT_keystream_words(keystream,iv,key,roundnum);
						if(keystream[0]==keystreamRight[0])
						{
							printf("Succeed!!! Case 3\n");
							flag=1;
							break;
						}
					}
				}

				//case 4
				if((k66==1) && (k67==1) &&(flag==0))
				//if(0)
				{
					//建立的方程是968轮的, 二次方程, 无单挂的项
					int remkeyset4[26]={0, 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, 39, 42, 45, 48, 51, 54, 57, 60,63,69,72,75,78};
					int remkeynum4=26;
					int looplenth=0x01<<remkeynum4;
					//建立的方程式968, 线性方程
					//for(i=0;i<looplenth;i++)
					{
						for(j=0;j<10;j++)
						{
							key[j]=0;
						}
						tttt=(0x01<<(66&0x7));
						key[66>>3] |=tttt;
						tttt=(0x01<<(67&0x7));
						key[67>>3] |=tttt;
						for(j=0;j<remkeynum4;j++)
						{
							key[remkeyset4[j]>>3]|=(((i>>j)&(0x01))<<(remkeyset4[j]&0x07));
						}
						ECRYPT_keystream_words(keystream,iv,key,roundnum);
						if(keystream[0]==keystreamRight[0])
						{
							printf("Succeed!!! Case 4\n");
							flag=1;
							break;
						}
					}
				}

			}

			if(flag==1)
				break;

		}
		//loc=((loc&0x3f00000)>>1)|((loc&0xfffff));
		
		t2=clock();
		printf("%d ms\n", t2-t1);
		time2[t]=t2-t1;
	}
	
	FILE *fpa;
	fopen_s(&fpa,"aaa","a");
	for(t=0;t<keynum;t++)
	{
		fprintf(fpa,"%d,",time1[t]);
	}
	fprintf(fpa,"\n");
    
	for(t=0;t<keynum;t++)
	{
		fprintf(fpa,"%d,",time2[t]);
	}
	fprintf(fpa,"\n");
	fclose(fpa);
	fclose(fp);
	system("pause");
    return 0;

}