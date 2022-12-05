``` ini

BenchmarkDotNet=v0.13.2, OS=macOS 13.0.1 (22A400) [Darwin 22.1.0]
Intel Core i3-1000NG4 CPU 1.10GHz, 1 CPU, 4 logical and 2 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2


```
| Method |     Mean |     Error |    StdDev | Allocated |
|------- |---------:|----------:|----------:|----------:|
|    Mul | 1.866 μs | 0.0367 μs | 0.0698 μs |   3.45 KB |
