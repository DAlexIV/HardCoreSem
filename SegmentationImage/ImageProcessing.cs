using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegmentationImage
{
    class ImageProcessing
    {
        int number_of_dots = 1000;
        double[,] blur_kern = { { 0.0625, 0.125, 0.0625 }, { 0.125, 0.25, 0.125 }, { 0.0625, 0.125, 0.0625 } };
        struct Blob
        {
            int rad;
            int x, y;
        }
        
        
        public int CountBlur(byte[] originalImage, int height, int width, int x, int y)
        {
            int start = originalImage[x * width + y]
            int r
        }
        
        public int[] MatrixSv(byte[] originalImage, int height, int width, double[,] ma, int nsize)
        {
            double[,] kern = ma; //Convolution kernel
            int[,] rf = ByteMeths.LinArrToMatrix(ByteMeths.BToInt(originalImage), height, width, nsize); //Reference image
            int[,] proc = new int[height + nsize - 1, width + nsize - 1]; //Processed image

            //Fills processed image
            for (int i = nsize / 2; i < height + nsize / 2; ++i)
            {
                for (int k = nsize / 2; k < width + nsize / 2; ++k)
                {
                    double tmp = 0;
                    for (int j = -nsize / 2 ; j <= nsize / 2; ++j)
                        for (int l = -nsize / 2; l <= nsize / 2; ++l)
                            tmp += rf[i + j, k + l] * kern[j + nsize / 2, l + nsize / 2];
                    proc[i, k] = Math.Abs((int)tmp);
                }
            }

            return ByteMeths.MatrixToLinArr(proc, nsize);
        }

        public byte[] Grsc(byte[] orig)
        {
            byte[] tmp = new byte[orig.Length / 4];
            for (int i = 0; i < tmp.Length; ++i)
            {
                byte tmps = (byte)(0.299 * orig[4 * i + 2] + 0.587 * orig[4 * i + 1] + 0.114 * orig[4 * i]);
                tmp[i] = tmps;
            }
            return tmp;
        }
        // converts image to gray-scale
        public byte[] setBinary(byte[] originalImage, int H, int W)
        {
            int magic_const = Math.Max(H, W) / 10;
            int[] gs = ByteMeths.BToInt(Grsc(originalImage)); //Converts image to graycale
            int[,] matrForF = ByteMeths.LinArrToMatrix(gs, H, W, magic_const);
            //Blob[] blbs = FindBlobs(matrForF, H, W, magic_const);
            //Sets convolution masks for x and y
            
            double[,] ykern = new double[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            double[,] xkern = new double[3, 3] { { 1, 0, -1 }, { 1, 0, -1 }, { 1, 0, -1 } };

            //double[,] ykern = new double[5, 5] { { -1, -1, -1, -1, -1 }, { -1, 0, 0, 0, -1 }, { 0, 0, 0, 0, 0 }, { 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1 } };
            //double[,] xkern = new double[5, 5] { { -1, -1, 0, 1, 1 }, { -1, 0, 0, 0, 1 }, { -1, 0, 0, 0, 1 }, { -1, 0, 0, 0, 1 }, { -1, -1, 0, 1, 1 } };

            //Uses them to fill array
            byte[] xk = ByteMeths.IntToB(MatrixSv(ByteMeths.IntToB(gs), H, W, xkern, xkern.GetLength(0)));
            byte[] yk = ByteMeths.IntToB(MatrixSv(ByteMeths.IntToB(gs), H, W, ykern, xkern.GetLength(1)));

            List<int[,]> mom = new List<int[,]>();
            int[] otk = new int[gs.Length];
            int[] outa = new int[gs.Length];

            for (int i = 0; i < gs.Length; ++i)
            {
                int[,] m = new int[2,2];
                m[0, 0] = ((int)xk[i]) * ((int)xk[i]);
                m[0, 1] = ((int)xk[i]) * ((int)yk[i]);
                m[0, 1] = ((int)xk[i]) * ((int)yk[i]);
                m[1, 1] = ((int)yk[i]) * ((int)yk[i]);
                mom.Add(m);
            }
                      
            for (int i = 0; i < gs.Length; ++i)
            {
                otk[i] = (int) (mom[i][0, 0] * mom[i][1, 1] -
                    mom[i][1, 0] * mom[i][0, 1] - 
                    0.06 * Math.Pow(mom[i][0, 0] + mom[i][1, 1], 2));
            }
            

            for (int i = 0; i < number_of_dots;  ++i)
            {
                int locm = 0;
                int ilocm = 0;
                bool _was_First = true;
                for (int k = 0; k < gs.Length; ++k)
                {
                    if (_was_First)
                    {
                        if (otk[k] != 0)
                        {
                            locm = otk[k];
                            ilocm = k;
                            _was_First = false;
                        }
                    }
                    else
                    {
                        if (otk[k] > locm)
                        {
                            locm = otk[k];
                            ilocm = k;
                        }
                    }
                }
                outa[ilocm] = locm;
                otk[ilocm] = int.MinValue;

            }
                //for (int i = 0; i < gs.Length; ++i)
                //outa[i] = (byte)((Math.Pow(mom[i].m[0,0], 2) * Math.Pow(mom[i].m[1,1], 2) - 0.04 * Math.Pow((mom[i].m[0,0]+mom[i].m[1,1]), 2)));
                //return outa;
                return ByteMeths.IntToB(outa);
        }

        // applies erosion to the image
        public byte[] setErosion(byte[] originalImage, int H, int W)
        {
            int[] gs = ByteMeths.BToInt(Grsc(originalImage)); //Converts image to graycale
            int[,] matrForF = ByteMeths.LinArrToMatrix(gs, H, W, 5);
            //Sets convolution masks for x and y
            
            //double[,] ykern = new double[3, 3] { { 1, 1, 1 }, { 0, 0, 0 }, { -1, -1, -1 } };
            //double[,] xkern = new double[3, 3] { { 1, 0, -1 }, { 1, 0, -1 }, { 1, 0, -1 } };

            double[,] ykern = new double[5, 5] { { -1, -1, -1, -1, -1 }, { -1, 0, 0, 0, -1 }, { 0, 0, 0, 0, 0 }, { 1, 0, 0, 0, 1 }, { 1, 1, 1, 1, 1 } };
            double[,] xkern = new double[5, 5] { { -1, -1, 0, 1, 1 }, { -1, 0, 0, 0, 1 }, { -1, 0, 0, 0, 1 }, { -1, 0, 0, 0, 1 }, { -1, -1, 0, 1, 1 } };

            //Uses them to fill array
            byte[] xk = ByteMeths.IntToB(MatrixSv(ByteMeths.IntToB(gs), H, W, xkern, xkern.GetLength(0)));
            byte[] yk = ByteMeths.IntToB(MatrixSv(ByteMeths.IntToB(gs), H, W, ykern, xkern.GetLength(1)));

            List<int[,]> mom = new List<int[,]>();
            int[] otk = new int[gs.Length];
            int[] outa = new int[gs.Length];

            for (int i = 0; i < gs.Length; ++i)
            {
                int[,] m = new int[2,2];
                m[0, 0] = ((int)xk[i]) * ((int)xk[i]);
                m[0, 1] = ((int)xk[i]) * ((int)yk[i]);
                m[0, 1] = ((int)xk[i]) * ((int)yk[i]);
                m[1, 1] = ((int)yk[i]) * ((int)yk[i]);
                mom.Add(m);
            }
                      
            for (int i = 0; i < gs.Length; ++i)
            {
                otk[i] = (int) (mom[i][0, 0] * mom[i][1, 1] -
                    mom[i][1, 0] * mom[i][0, 1] - 
                    0.06 * Math.Pow(mom[i][0, 0] + mom[i][1, 1], 2));
            }
            

            for (int i = 0; i < number_of_dots;  ++i)
            {
                int locm = 0;
                int ilocm = 0;
                bool _was_First = true;
                for (int k = 0; k < gs.Length; ++k)
                {
                    if (_was_First)
                    {
                        if (otk[k] != 0)
                        {
                            locm = otk[k];
                            ilocm = k;
                            _was_First = false;
                        }
                    }
                    else
                    {
                        if (otk[k] > locm)
                        {
                            locm = otk[k];
                            ilocm = k;
                        }
                    }
                }
                outa[ilocm] = locm;
                otk[ilocm] = int.MinValue;

            }
                //for (int i = 0; i < gs.Length; ++i)
                //outa[i] = (byte)((Math.Pow(mom[i].m[0,0], 2) * Math.Pow(mom[i].m[1,1], 2) - 0.04 * Math.Pow((mom[i].m[0,0]+mom[i].m[1,1]), 2)));
                //return outa;
                return ByteMeths.IntToB(outa);
        }

        // applies dilatation to the image
        public byte[] setDilatation(byte[] originalImage)
        {
            /* 
             * TODO: создать новый byte[]
             * TODO: применить операцию дилатации к бинарному изображению
             * структурный элемент - кольцо 5х5             
             */
            return null;
        }

        // detectes edges at the image
        public byte[] setEdges(byte[] originalImage)
        {
            /* 
             * TODO: создать новый byte[]
             * TODO: придумать, как выделить края
             * видимо, знания морфологических операций не помешают ^_^
             */
            return null;
        }
    }
}
