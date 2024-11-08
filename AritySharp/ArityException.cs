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
 * Thrown when a {@link Function} is evaluated with a wrong number of arguments
 * (when the number of arguments is not equal to the function's arity).
 */

public class ArityException(string mes) : Exception(mes)
{
    public ArityException(int nArgs) : this($"Didn't expect {nArgs} arguments") { }
}
