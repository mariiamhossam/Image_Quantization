using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageQuantization
{
    class priority_Q
    {
        public static List<KeyValuePair<RGBPixel, double>> list = new List<KeyValuePair<RGBPixel, double>>();
        public static int Count
        {
            get
            {
                return list.Count;
            }
        }
        public priority_Q()
        {
            list = new List<KeyValuePair<RGBPixel, double>>();
        }

        // O(log v)
        public static void Enqueue(RGBPixel vertex, double x)
        {
            list.Add(new KeyValuePair<RGBPixel, double>(vertex, x));

            int p = 0;

            for (int i = Count - 1; i > 0;)
            {
                int pos = (i - 1) / 2;
                if (list[pos].Value <= x)
                {
                    p = i;
                    break;

                }
                list[i] = list[pos];
                i = pos;
            }

            if (Count > 0)
            {
                KeyValuePair<RGBPixel, double> temp = new KeyValuePair<RGBPixel, double>(vertex, x);
                list[p] = temp;
            }
        }

        public static KeyValuePair<RGBPixel, double> Dequeue()
        {
            KeyValuePair<RGBPixel, double> min = Top();
            double top = list[Count - 1].Value;
            RGBPixel ver = list[Count - 1].Key;
            list.RemoveAt(Count - 1);

            int i = 0;
            while (i * 2 < Count - 2)
            {
                int a = i * 2 + 1;
                int b = i * 2 + 2;
                int c = 0;
                if (b < Count && list[b].Value < list[a].Value)
                    c = b;
                else
                    c = a;

                if (list[c].Value >= top)
                {
                    break;
                }
                list[i] = list[c];
                i = c;
            }
            if (Count > 0)
            {
                list[i] = new KeyValuePair<RGBPixel, double>(ver, top);
            }
            
           return new KeyValuePair<RGBPixel,double>(min.Key, min.Value);
        }
        
        public static KeyValuePair<RGBPixel, double> Top()
        {
            
           return new KeyValuePair<RGBPixel,double>(list[0].Key, list[0].Value);
        }

        public static void Clear()
        {
            list.Clear();
        }
    }
}
