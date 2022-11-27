namespace DecFloat;

public class Program
{
    static void Main(string[] args)
    {
        // compute factorial of 1000
        var a = DecFloat.One;
        for (int i = 2; i <= 1000; i++)
        {
            a = a.Mul(new DecFloat(i + ""));
            Console.WriteLine(i + "!=" + a);
        }
        Console.ReadLine();
    }
}