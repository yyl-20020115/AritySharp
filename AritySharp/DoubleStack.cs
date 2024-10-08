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
            double[] newRe = new double[newSize];
            double[] newIm = new double[newSize];
            Array.Copy(re, 0, newRe, 0, re.Length);
            Array.Copy(im, 0, newIm, 0, re.Length);
            re = newRe;
            im = newIm;
        }
        re[size] = a;
        im[size] = b;
        ++size;
    }

    public void Pop(int cnt)
    {
        if (cnt > size)
            throw new Exception("pop " + cnt + " from " + size);
        size -= cnt;
    }

    public void Pop() => --size;

    public double[] GetRe()
    {
        var trimmed = new double[size];
        Array.Copy(re, 0, trimmed, 0, size);
        return trimmed;
    }

    public double[]? GetIm()
    {
        var allZero = true;
        for (int i = 0; i < size; ++i)
        {
            if (im[i] != 0)
            {
                allZero = false;
                break;
            }
        }
        if (allZero) return null;

        var trimmed = new double[size];
        Array.Copy(im, 0, trimmed, 0, size);
        return trimmed;
    }
}
