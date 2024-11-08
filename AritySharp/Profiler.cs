/*
 * Copyright (C) 2007-2009 Mihai Preda.
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace AritySharp;

/**
 * Runs unit-tests.
 * <p>
 * Usage: java -jar arity.jar
 */
public class Profiler
{
    private static readonly string[] PROFILE_CASES = [
        "(100.5 + 20009.999)*(7+4+3)/(5/2)^3!)*2", "fun1(x)=(x+2)*(x+3)",
        "otherFun(x)=(fun1(x-1)*x+1)*(fun1(2-x)+10)", "log(x+30.5, 3)^.7*sin(x+.5)" 
        ];

    /**
     * Takes a single command-line argument, an expression; compiles and prints it.
     * 
     * @throws SyntaxException
     *           if there are errors compiling the expression.
     */
    public static void Main(string[] argv)
    {
        int size = argv.Length;
        if (size == 0)
        {
            Console.WriteLine("Unit testing implementation has been removed / converted to JUnit tests in this version!");
        }
        else if (argv[0]==("-profile"))
        {
            if (size == 1)
            {
                Profile();
            }
            else
            {
                var symbols = new Symbols();
                for (int i = 1; i < size - 1; ++i)
                {
                    var fan = symbols.CompileWithName(argv[i]);
                    symbols.Define(fan);
                }
                Profile(symbols, argv[size - 1]);
            }
        }
        else
        {
            var symbols = new Symbols();
            foreach (string arg in argv)
            {
                var fan = symbols.CompileWithName(arg);
                symbols.Define(fan);
                var f = fan.Function;
                Console.WriteLine($"{arg} : {f}");
            }
        }
    }

    static void Profile(Symbols symbols, string str)
    {
        var f = symbols.Compile(str);
        Console.WriteLine($"\n{str}: {f}");

        var t1 = DateTime.Now.Microsecond;
        for (int i = 0; i < 1000; ++i)
        {
            symbols.Compile(str);
        }
        var t2 = DateTime.Now.Microsecond;
        Console.WriteLine($"compilation time: {t2 - t1} us");

        var args = new double[f.Arity];
        t1 = DateTime.Now.Microsecond;
        for (int i = 0; i < 100000; ++i)
        {
            f.Eval(args);
        }
        t2 = DateTime.Now.Microsecond;
        long delta = t2 - t1;
        Console.WriteLine($"execution time: {(delta > 100 ? "" + delta / 100.0 + " us" : "" + delta + " ns")}");
    }

    private static void Profile()
    {
        var symbols = new Symbols();
        try
        {
            foreach (var cas in PROFILE_CASES)
            {
                symbols.Define(symbols.CompileWithName(cas));
                Profile(symbols, cas);
            }
        }
        catch (SyntaxException)
        {
            throw;
        }
    }
}
