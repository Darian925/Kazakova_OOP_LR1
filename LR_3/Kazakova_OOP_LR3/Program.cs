using System;
using System.IO;


namespace Kazakova_OOP_LR3
{
    internal class Program
    {
        static void Main(string[] args)
        {
         
            TextReader save_in = Console.In;
            TextWriter save_out = Console.Out;

           
            StreamReader new_in = new StreamReader(@"input.txt");
            StreamWriter new_out = new StreamWriter(@"output.txt");

           
            Console.SetIn(new_in);
            Console.SetOut(new_out);

          
            int t = Convert.ToInt32(Console.ReadLine());
            int N = Convert.ToInt32(Console.ReadLine());
            double X = Convert.ToDouble(Console.ReadLine());
            double Y = Convert.ToDouble(Console.ReadLine());

            double Z = 1.0; 

            if (t == 0)
            {
                //for
                for (int i = 2; i <= N; i++)
                {
                    if (i % 2 == 0)
                    {
                        Z -= Math.Sin(Math.Pow(X, i)) / i;
                    }
                    else
                    {
                        Z += Math.Cos(Math.Pow(Y, i)) / i;
                    }
                }
            }
            else if (t == 1)
            {
                //while
                int i = 2;
                while (i <= N)
                {
                    if (i % 2 == 0)
                    {
                        Z -= Math.Sin(Math.Pow(X, i)) / i;
                    }
                    else
                    {
                        Z += Math.Cos(Math.Pow(Y, i)) / i;
                    }
                    i++;
                }
            }
            else if (t == 2)
            {
                //do...while
                int i = 2;
                if (N >= 2)
                {
                    do
                    {
                        if (i % 2 == 0)
                        {
                            Z -= Math.Sin(Math.Pow(X, i)) / i;
                        }
                        else
                        {
                            Z += Math.Cos(Math.Pow(Y, i)) / i;
                        }
                        i++;
                    } while (i <= N);
                }
            }

            Console.WriteLine(String.Format("{0:0.000000}",Z));
            Console.SetOut(save_out);
            Console.SetIn(save_in);
            new_out.Close();
            new_in.Close();
        }
    }
}
