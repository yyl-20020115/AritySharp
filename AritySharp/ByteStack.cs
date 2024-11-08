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

public class ByteStack
{
    public const byte Zero = 0;
    private byte[] data = new byte[8];
    private int length = 0;

    public byte[] Data => this.data;
    public int Length => this.length;
    public void Clear() => this.length = 0;
    public void Push(byte value)
    {
        if (this.length >= data.Length)
        {
            var data = new byte[this.data.Length << 1];
            Array.Copy(this.data, 0, data, 0, this.data.Length);
            this.data = data;
        }
        this.data[this.length++] = value;
    }

    public byte Pop() => this.length > 0 ? this.data[--this.length] : Zero;

    public byte[] ToArray()
    {
        var copy = new byte[this.length];
        Array.Copy(this.data, 0, copy, 0, this.length);
        return copy;
    }
    public void Trim() => this.data = this.ToArray();
}
