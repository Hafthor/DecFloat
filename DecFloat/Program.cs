namespace DecFloat;

public class Program {
    public static void Main() {
        // compute factorial of 1000
        var a = DecFloat.One;
        for (int i = 2; i <= 1000; i++) {
            a = a.Mul(new DecFloat(i + ""));
            Console.WriteLine(i + "!=" + a);
        }
        Console.ReadLine();
    }
}