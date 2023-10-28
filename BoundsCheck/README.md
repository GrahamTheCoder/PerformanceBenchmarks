There are two great lessons hiding in this example:
* Know which case you're optimising for
* Ask *why* it's slower

If you benchmark the grid of 9 main possibilities separately, and plot them on a graph you will see a clear difference in those two implementations:
* "ExampleFromAi" is very consistent since it checks x and y in all cases.
* "ObviousImplementation" will be at its fastest when bounds.Left > point.X because then it only needs to do one check.

![image](https://github.com/GrahamTheCoder/PerformanceBenchmarks/assets/2490482/8cfec4aa-b034-4582-93ae-72e569d22744)

Once you understand that "why", you can inline insideX and insideY (and put an && between them), you'll have something that really tests the uint trickery.
In my test runs, the simple implementation was faster by 11%, but digging into the result, the fixed up uint trickery was ~33% faster in 4 of the cases and the simple implementation was ~40% faster in the other 5 cases.
I actually had to run my benchmarks twice to patch up some results which came out as zero or 10x smaller than they should be, which to me is a hint not to put too much stock in the small differences.

tl;dr No reason not to use the simple implementation here
