using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    /// 

    public struct RGBPixel
    {
        public byte red, green, blue;

    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public struct graph
    {
        public RGBPixel v1;
        public RGBPixel v2;
        public double weight;
        public graph(RGBPixel v1, RGBPixel v2, double weight)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.weight = weight;
        }

    }

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        public static double Mst_sum = 0;
        public static List<KeyValuePair<RGBPixel, double>> list = new List<KeyValuePair<RGBPixel, double>>();

        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {

            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];


            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }
        /////////finding dist colors//////////////

        public static List<RGBPixel> find_diff_colors(RGBPixel[,] ImageMatrix)
        {
            int w = GetWidth(ImageMatrix);
            int h = GetHeight(ImageMatrix);

            List<RGBPixel> disc_colors1 = new List<RGBPixel>();
            int[,,] arr = new int[256, 256, 256];


            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    if (arr[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue] != 1)
                    {
                        disc_colors1.Add(ImageMatrix[i, j]);
                        arr[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue] = 1;

                    }

                }
            }

            return disc_colors1;
        }

        ////////return edges with it's cost////////////////////////
        public static List<graph> construct_graph(RGBPixel[,] ImageMatrix)
        {
            List<graph> edges = new List<graph>();
            List<RGBPixel> vertices = new List<RGBPixel>();
            vertices = find_diff_colors(ImageMatrix);
            int count = 1;
            double red = 0;
            double green = 0;
            double blue = 0;
            double weight = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = count; j < vertices.Count; j++)
                {
                    graph g = new graph();
                    red = (vertices[i].red - vertices[j].red) * (vertices[i].red - vertices[j].red);
                    green = (vertices[i].green - vertices[j].green) * (vertices[i].green - vertices[j].green);
                    blue = (vertices[i].blue - vertices[j].blue) * (vertices[i].blue - vertices[j].blue);
                    weight = Math.Sqrt(red + green + blue);  //O(1)
                    g.v1 = vertices[i];
                    g.v2 = vertices[j];
                    g.weight = weight;
                    edges.Add(g);
                }
                count++;
            }
            
            return edges;
        }
        public static double calcdis(RGBPixel x, RGBPixel y)
        {
            double red, green, blue, weight;
            red = (x.red - y.red) * (x.red - y.red);
            green = (x.green - y.green) * (x.green - y.green);
            blue = (x.blue - y.blue) * (x.blue - y.blue);
            weight = Math.Sqrt(red + green + blue);
            return weight;
        }
 
        public static List<graph> MST(RGBPixel[,] ImageMatrix, List<RGBPixel> vertices)
        {

            bool[,,] is_visted = new bool[256, 256, 256]; 
            double[,,] weight = new double[256, 256, 256];
            RGBPixel[,,] parent = new RGBPixel[256, 256, 256];
            bool[,,] hasparent = new bool[256, 256, 256]; 
            List<graph> mst = new List<graph>();

            RGBPixel r = new RGBPixel();
            for (int i = 0; i < vertices.Count; i++) //θ(D)
            {
                weight[vertices[i].red, vertices[i].green, vertices[i].blue] = int.MaxValue;
                parent[vertices[i].red, vertices[i].green, vertices[i].blue] = r;
                hasparent[vertices[i].red, vertices[i].green, vertices[i].blue] = false;
            }

            priority_Q.Enqueue(vertices[0], calcdis(vertices[0], vertices[1])); //O(log v)
            while (priority_Q.Count > 0)
            {
                RGBPixel top1 = priority_Q.Dequeue().Key;
                if (is_visted[top1.red, top1.green, top1.blue])
                {
                    continue;
                }
                is_visted[top1.red, top1.green, top1.blue] = true;
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (top1.red == vertices[i].red && top1.green == vertices[i].green && top1.blue == vertices[i].blue)
                        continue;
                    double w = calcdis(top1, vertices[i]);
                    if (!is_visted[vertices[i].red, vertices[i].green, vertices[i].blue] && weight[vertices[i].red, vertices[i].green, vertices[i].blue] > w)
                    {
                        weight[vertices[i].red, vertices[i].green, vertices[i].blue] = w;
                        priority_Q.Enqueue(vertices[i], weight[vertices[i].red, vertices[i].green, vertices[i].blue]);
                        parent[vertices[i].red, vertices[i].green, vertices[i].blue] = top1;
                        hasparent[vertices[i].red, vertices[i].green, vertices[i].blue] = true;
                    }
                }
            }
            double distance = 0;
            //double sum = 0;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (hasparent[vertices[i].red, vertices[i].green, vertices[i].blue])
                {
                    distance = weight[vertices[i].red, vertices[i].green, vertices[i].blue];
                    Mst_sum += distance;
                    mst.Add(new graph(parent[vertices[i].red, vertices[i].green, vertices[i].blue], vertices[i], distance));
                }
            }
            return mst;
        }
        public static double MST_SUM(double sum)
        {
            return sum;
        }

        public static List<List<RGBPixel>> Clustering(RGBPixel[,] ImageMatrix, int k, List<RGBPixel> dis_colors, List<graph> mst_graph)
        {
            List<List<RGBPixel>> cluster = new List<List<RGBPixel>>();
            bool[,,] is_visited = new bool[256, 256, 256];
            double w = -50;
            int indx = 0;
            for (int i = 0; i < k - 1; i++)
            {
                for (int j = 0; j < mst_graph.Count; j++)
                {
                    if (mst_graph[j].weight > w)
                    {
                        w = mst_graph[j].weight;
                        indx = j;
                    }

                }
                graph g = mst_graph[indx];
                g.weight = -1;
                mst_graph[indx] = g;
                w = -50;
            }

            List<RGBPixel>[,,] adj = new List<RGBPixel>[256, 256, 256];
            foreach (RGBPixel i in dis_colors)
            {
                adj[i.red, i.green, i.blue] = new List<RGBPixel>();
            }

            RGBPixel x = new RGBPixel();

            for (int i = 0; i < mst_graph.Count; i++)
            {
                if (mst_graph[i].weight != -1)
                {
                    x.red = mst_graph[i].v1.red;
                    x.green = mst_graph[i].v1.green;
                    x.blue = mst_graph[i].v1.blue;

                    adj[x.red, x.green, x.blue].Add(mst_graph[i].v2);
                    x.red = mst_graph[i].v2.red;
                    x.green = mst_graph[i].v2.green;
                    x.blue = mst_graph[i].v2.blue;
                    adj[x.red, x.green, x.blue].Add(mst_graph[i].v1);
                }
            }
            int cnt = 0;
            foreach (RGBPixel r in dis_colors)
            {
                if (is_visited[r.red, r.green, r.blue] == false)
                {
                    cluster.Add(new List<RGBPixel>());
                    cluster = DFS(ImageMatrix, r, adj, cnt, is_visited, cluster);
                    cnt++;
                }
            }
            return cluster;
        }
        public static List<List<RGBPixel>> DFS(RGBPixel[,] ImageMatrix, RGBPixel r, List<RGBPixel>[,,] adj, int cnt, bool[,,] is_visited, List<List<RGBPixel>> cluster)
        {
            cluster[cnt].Add(r);
            is_visited[r.red, r.green, r.blue] = true;

            foreach (RGBPixel i in adj[r.red, r.green, r.blue])
            {
                if (is_visited[i.red, i.green, i.blue] == false)
                {
                    DFS(ImageMatrix, i, adj, cnt, is_visited, cluster);
                }
            }
            return cluster;
        }

        //////Pallete Formation//////
        public static RGBPixel[,,] Colors_Pallete(List<List<RGBPixel>> Cluster)
        {
            double total_b = 0;
            double total_g = 0;
            double total_r = 0;
            RGBPixel value = new RGBPixel();
            RGBPixel[,,] Average_color = new RGBPixel[256, 256, 256];
            foreach (var k in Cluster)
            {
                total_b = 0;
                total_g = 0;
                total_r = 0;
                foreach (var element in k)
                {
                    total_r += element.red;
                    total_g += element.green;
                    total_b += element.blue;
                }
                total_r = total_r / k.Count;
                total_g = total_g / k.Count;
                total_b = total_b / k.Count;
                value.red = (byte)total_r;
                value.blue = (byte)total_b;
                value.green = (byte)total_g;

                foreach (var element in k)
                {
                    Average_color[element.red, element.green, element.blue] = value;
                }
            }
            return Average_color;
        }
        //////Quantization/////////
        public static void Quantization(ref RGBPixel[,] ImageMatrix, RGBPixel[,,] Colors_Pallete)
        {
            int w = GetWidth(ImageMatrix);
            int h = GetHeight(ImageMatrix);
            RGBPixel value = new RGBPixel();
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    value = Colors_Pallete[ImageMatrix[i, j].red, ImageMatrix[i, j].green, ImageMatrix[i, j].blue];
                    ImageMatrix[i, j].red = value.red;
                    ImageMatrix[i, j].green = value.green;
                    ImageMatrix[i, j].blue = value.blue;
                }
            }
        }
    }

}
