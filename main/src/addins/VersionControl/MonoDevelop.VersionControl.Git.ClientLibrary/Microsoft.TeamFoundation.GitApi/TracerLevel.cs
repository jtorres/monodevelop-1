/**** Git Process Management Library ****
 *
 * Copyright (c) Microsoft Corporation
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the ""Software""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
**/

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Defines the levels of trace noise, the higher the value the more noise.
    /// </summary>
    public enum TracerLevel
    {
        /// <summary>
        /// High importance message.
        /// </summary>
        Quiet = 0,

        /// <summary>
        /// Normal or nominal importance message.
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// Low importance message.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Intended for debugging of applications consuming the `<see cref="Microsoft.TeamFoundation.GitApi"/>` library.
        /// </summary>
        Detailed = 3,

        /// <summary>
        /// Intended for debugging the `<see cref="Microsoft.TeamFoundation.GitApi"/>` library.
        /// </summary>
        Diagnostic = 4,
    }
}
