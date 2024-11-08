/*
 * Copyright (C) 2008-2009 Mihai Preda.
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

public class FunctionStack
{
    private Function[] data = new Function[8];
    private int size = 0;

    public void Clear() => size = 0;

    public void Push(Function b)
    {
        if (size >= data.Length)
        {
            var newData = new Function[data.Length << 1];
            Array.Copy(data, 0, newData, 0, data.Length);
            data = newData;
        }
        data[size++] = b;
    }

    public Function Pop() => this.size>0 ? this.data[--this.size]: Function.Empty;

    public Function[] ToArray()
    {
        var trimmed = new Function[size];
        Array.Copy(data, 0, trimmed, 0, size);
        return trimmed;
    }
}
