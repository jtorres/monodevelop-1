using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.CodeAnalysis.Text;

namespace MonoDevelop.Ide.Composition
{
	internal class InlineRenameService : IInlineRenameService
	{
		public IInlineRenameSession ActiveSession => null;

		public InlineRenameSessionInfo StartInlineSession (Document document, TextSpan triggerSpan, CancellationToken cancellationToken = default (CancellationToken))
		{
			throw new NotImplementedException ();
		}
	}
}
