using System.Runtime.Intrinsics.Arm;

namespace DecFloat;

public class Program
{
    static void Main(string[] args)
    {
        var a = new DecFloat("1");
        for (int i = 2; i <= 10000; i++)
        {
            a = a.Mul(new DecFloat(i + ""));
            Console.WriteLine(i + "!=" + a);
        }
        Console.ReadLine();
    }
}
