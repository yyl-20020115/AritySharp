/*
 * Copyright (C) 2008 Mihai Preda.
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
 * A constant presented as a function, always evaluates to the same value.
 */
public class Constant(Complex o) : Function
{
    private readonly Complex value = new (o);

    /** Returns the complex constant. */
    public override Complex EvalComplex() => value;

    //@Override
    /**
     * Returns the complex constant as a real value.
     * 
     * @see Complex.asReas()
     */
    public override double Eval() => value.AsReal;

    public override string ToString() => value.ToString();

    public override int Arity => 0;
}
