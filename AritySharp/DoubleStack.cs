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

public class DoubleStack
{
    private double[] re = new double[8];
    private double[] im = new double[8];
    private int size = 0;

    public void Clear() => this.size = 0;

    public void Push(double a, double b)
    {
        if (size >= re.Length)
        {
            int newSize = re.Length << 1;
            var newRe = new double[newSize];
            var newIm = new double[newSize];
            Array.Copy(re, 0, newRe, 0, re.Length);
            Array.Copy(im, 0, newIm, 0, re.Length);
            this.re = newRe;
            this.im = newIm;
        }
        this.re[size] = a;
        this.im[size] = b;
        ++this.size;
    }

    public void Pop(int cnt)
    {
        if (cnt > size)
            throw new Exception($"pop {cnt} from {size}");
        this.size -= cnt;
    }

    public void Pop() => --this.size;

    public double[] Reals
    {
        get
        {
            var values = new double[size];
            Array.Copy(re, 0, values, 0, size);
            return values;
        }
    }

    public double[] Imaginaries
    {
        get
        {
            var values = new double[this.size];
            Array.Copy(im, 0, values, 0, size);
            return values;
        }
    }
}
