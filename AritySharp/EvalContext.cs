/*
 * Copyright (C) 2008 Mihai Preda.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace AritySharp;

/**
   To evaluate CompiledFunctions from multiple threads in parallel,
   you need to create one EvalContext instance per thread,
   and pass it to the eval() methods of CompiledFunction.
 */

public class EvalContext
{
    public const int MAX_STACK_SIZE = 128; //if stack ever grows above this likely something is wrong
    public readonly double[] StackRe = new double[MAX_STACK_SIZE];
    public readonly Complex[] StackComplex = new Complex[MAX_STACK_SIZE];
    public int StackBase = 0;

    public readonly double[] Args1 = new double[1];
    public readonly double[] Args2 = new double[2];
    public readonly Complex[] Args1c;
    public readonly Complex[] Args2c;

    /** Constructs a new EvalContext, ready to be used with CompiledFunction.eval().
     */
    public EvalContext()
    {
        for (int i = 0; i < MAX_STACK_SIZE; ++i)
            StackComplex[i] = new ();
        Args1c = [new ()];
        Args2c = [new (), new ()];
    }
}
