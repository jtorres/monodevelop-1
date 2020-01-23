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
    /// Submodule specific options related to `<see cref="StatusOptions"/>`.
    /// </summary>
    public enum StatusIgnoreSubmodule
    {
        /// <summary>
        /// Consider the submodule modified when it either contains untracked or modified files or
        /// its HEAD differs from the commit recorded in the super-project and can be used to
        /// override any settings of the ignore option
        /// </summary>
        None,

        /// <summary>
        /// Submodules are not considered dirty when they only contain untracked content (but they
        /// are still scanned for modified content).
        /// </summary>
        Untracked,

        /// <summary>
        /// Ignores all changes to the work tree of submodules, only changes to the commits stored in
        /// the super-project are shown.
        /// </summary>
        Dirty,

        /// <summary>
        /// Hides all changes to submodules (and suppresses the output of submodule summaries when
        /// the config option status.submoduleSummary is set).
        /// </summary>
        All,
    }
}
