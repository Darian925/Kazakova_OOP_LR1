using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kazakova_OOP_LR1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine("Название и номер лабораторной работы: Лабораторная работа номер 1. Разработка консольного приложения ");
            Console.WriteLine("ФИО студента: Казакова Дарья Владимировна");
            Console.WriteLine("Группа студента и шифр специальности: ИДБ-23-01 , 09.03.01");
            Console.WriteLine("Дата рождения студента: 31.03.2005");
            Console.WriteLine("Населенный пункт постоянного места жительства студента: Москва");
            Console.WriteLine("Любимый предмет в школе: Изо");
            Console.WriteLine("Краткое описание хобби,увлечений,интересов: Манга и компьютерные игры ");

            //Задание 2
            //Вариант 8
            int a = 1, G_1 = 5, Zvcw = 7 ;
            double A0 = (35/G_1)*Zvcw+ G_1*a-((a+Zvcw)/a) ;
            Console.WriteLine("Результат:A0={0}",A0);
            Console.ReadKey();
        }
    }
}
