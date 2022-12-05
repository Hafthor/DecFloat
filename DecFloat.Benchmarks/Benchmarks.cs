namespace DecFloat.Benchmarks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser(false)]
public class Benchmarks
{
    public static void Main()
    {
        BenchmarkRunner.Run<Benchmarks>();
    }

    [Benchmark]
    public void E()
    {
        DecFloat.E(100); //  3.046 ms, 5.91 MB
    }

    [Benchmark]
    public void Pi()
    {
        DecFloat.Pi(100); //  811.518 ms, 1774.35 MB
    }

    [Benchmark]
    public void Fact()
    {
        DecFloat.Fact(69); // 1.039us, 2.81KB
    }

    private static readonly DecFloat three = new DecFloat("3");
    [Benchmark]
    public void Log2()
    {
        three.Log2(30); // 2.593s, 2.812 GB - 100 was 77.29 s, 142.04 GB
    }

    private static readonly DecFloat threeonefour = new DecFloat("3.14");
    [Benchmark]
    public void Exp2()
    {
        threeonefour.Exp2(30); // 683ms, 643 MB - 100 was 285.6 s, 662.77 GB
    }

    private static readonly DecFloat five = new DecFloat("5");
    [Benchmark]
    public void Sqrt()
    {
        five.Sqrt(100); //  53.37 ms, 102.35 MB
    }

    private static readonly DecFloat twentytwo = new DecFloat("22");
    private static readonly DecFloat seven = new DecFloat("7");
    [Benchmark]
    public void Div()
    {
        twentytwo.Div(seven, 100); // 10.48 us, 20.49 KB
    }

    private static readonly DecFloat sqrt5 = five.Sqrt(100);
    [Benchmark]
    public void Mul()
    {
        sqrt5.Mul(sqrt5); // 1.866 us, 3.45 KB
    }
}

