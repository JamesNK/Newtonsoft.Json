#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(NET35 || NET20 || NETFX_CORE || ASPNETCORE50)
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;

namespace Newtonsoft.Json.Tests.TestObjects
{
    [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}"), CompilationMapping(SourceConstructFlags.SumType)]
    public class Shape
    {
        // Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal readonly int _tag;

        [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
        internal static readonly Shape _unique_Empty;

        static Shape()
        {
            _unique_Empty = new Shape(3);
        }

        [CompilerGenerated]
        internal Shape(int _tag)
        {
            this._tag = _tag;
        }

        [CompilationMapping(SourceConstructFlags.UnionCase, 1)]
        public static Shape NewCircle(double _radius)
        {
            return new Circle(_radius);
        }

        [CompilationMapping(SourceConstructFlags.UnionCase, 2)]
        public static Shape NewPrism(double _width, double item2, double _height)
        {
            return new Prism(_width, item2, _height);
        }

        [CompilationMapping(SourceConstructFlags.UnionCase, 0)]
        public static Shape NewRectangle(double _width, double _length)
        {
            return new Rectangle(_width, _length);
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public static Shape Empty
        {
            [CompilationMapping(SourceConstructFlags.UnionCase, 3)]
            get { return _unique_Empty; }
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsCircle
        {
            [CompilerGenerated]
            get { return (this.Tag == 1); }
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsEmpty
        {
            [CompilerGenerated]
            get { return (this.Tag == 3); }
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsPrism
        {
            [CompilerGenerated]
            get { return (this.Tag == 2); }
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public bool IsRectangle
        {
            [CompilerGenerated]
            get { return (this.Tag == 0); }
        }

        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public int Tag
        {
            [CompilerGenerated]
            get { return this._tag; }
        }

        [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
        public class Circle : Shape
        {
            // Fields
            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double _radius;

            // Methods
            [CompilerGenerated, DebuggerNonUserCode]
            internal Circle(double _radius) : base(1)
            {
                this._radius = _radius;
            }

            // Properties
            [CompilationMapping(SourceConstructFlags.Field, 1, 0), CompilerGenerated, DebuggerNonUserCode]
            public double radius
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this._radius; }
            }
        }


        [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
        public class Prism : Shape
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double _height;

            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double _width;

            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double item2;

            [CompilerGenerated, DebuggerNonUserCode]
            internal Prism(double _width, double item2, double _height) : base(2)
            {
                this._width = _width;
                this.item2 = item2;
                this._height = _height;
            }

            [CompilationMapping(SourceConstructFlags.Field, 2, 2), CompilerGenerated, DebuggerNonUserCode]
            public double height
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this._height; }
            }

            [CompilationMapping(SourceConstructFlags.Field, 2, 1), CompilerGenerated, DebuggerNonUserCode]
            public double Item2
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this.item2; }
            }

            [CompilationMapping(SourceConstructFlags.Field, 2, 0), CompilerGenerated, DebuggerNonUserCode]
            public double width
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this._width; }
            }
        }

        [Serializable, DebuggerDisplay("{__DebugDisplay(),nq}")]
        public class Rectangle : Shape
        {
            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double _length;

            [DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
            internal readonly double _width;

            [CompilerGenerated, DebuggerNonUserCode]
            internal Rectangle(double _width, double _length) : base(0)
            {
                this._width = _width;
                this._length = _length;
            }

            [CompilationMapping(SourceConstructFlags.Field, 0, 1), CompilerGenerated, DebuggerNonUserCode]
            public double length
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this._length; }
            }

            [CompilationMapping(SourceConstructFlags.Field, 0, 0), CompilerGenerated, DebuggerNonUserCode]
            public double width
            {
                [CompilerGenerated, DebuggerNonUserCode]
                get { return this._width; }
            }
        }

        public static class Tags
        {
            // Fields
            public const int Circle = 1;
            public const int Empty = 3;
            public const int Prism = 2;
            public const int Rectangle = 0;
        }
    }
}
#endif