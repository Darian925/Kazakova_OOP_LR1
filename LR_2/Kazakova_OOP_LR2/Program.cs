using System;
using System.IO;

namespace Kazakova_OOP_LR2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TextWriter save_out=Console.Out;
            TextReader save_in=Console.In;
            var new_out=new StreamWriter(@"output.txt");
            var new_in = new StreamReader(@"input.txt");
            Console.SetOut(new_out);
            Console.SetIn(new_in);
            double a1, a2, a3, a4, a5;
            double s, k;
            a1= Convert.ToDouble(Console.ReadLine());
            a2 = Convert.ToDouble(Console.ReadLine());
            a3 = Convert.ToDouble(Console.ReadLine());
            a4 = Convert.ToDouble(Console.ReadLine());
            a5 = Convert.ToDouble(Console.ReadLine());
            if ((a1 - a2 < 0) || (a2 - a3 < 0) || (a3 - a4 <= 0) || (a1 < 0) || (a4 - a5 == 0))
                Console.WriteLine("ERROR");
            else
            {
                s = Math.Sqrt(a1 - a2)+(Math.Sqrt(a2-a3)/Math.Sqrt(a3-a4)) +(Math.Sqrt(a1)/(a4-a5)) ;
                Console.WriteLine(String.Format("{0:0.000}",s));
            }

            if ((a2 - a3 <= 0) || (a2 - a4 == 0))
                Console.WriteLine("ERROR");
            else 
            {
                k = Math.Sqrt(1 / (a2 - a3))+Math.Sqrt(2)/((a2-a4)*(a2-a4));
                Console.WriteLine(String.Format("{0:0.000}", k));
            }
            Console.SetOut(save_out);new_out.Close();
            Console.SetIn(save_in);new_in.Close();
        }
    }
}
