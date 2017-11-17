//
// EventArgs.cs
//
// Author:
//       Jose Medrano <jose.medrano@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corp.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
namespace MonoDevelop.Components
{
	class MouseEventArgs : EventArgs
	{
		public double X {
			get;
			private set;
		}

		public double Y {
			get;
			private set;
		}

		public double Button {
			get;
			private set;
		}

		public MouseEventArgs (double x, double y, int button)
		{
			X = x;
			Y = y;
			Button = button;
		}
	}

	class DataMouseEventArgs : HandledMouseEventArgs
	{
		public object Data {
			get;
			private set;
		}

		public DataMouseEventArgs (object data, double x, double y, int button) : base (x, y, button)
		{
			Data = data;
		}
	}

	class HandledMouseEventArgs : MouseEventArgs
	{
		public bool Handled { get; set; }

		public HandledMouseEventArgs (double x, double y, int button) : base (x, y, button)
		{

		}
	}

	class EventButtonEventArgs : EventArgs
	{
		public bool Handled { get; set; }

		public Gdk.EventButton EventButton { get; private set; }

		public EventButtonEventArgs (Gdk.EventButton eventButton)
		{
			this.EventButton = eventButton;
		}
	}
}