using System;
using Microsoft.TeamFoundation.GitApi.Internal;

namespace Microsoft.TeamFoundation.GitApi
{
    /// <summary>
    /// Base type for all Git object model types.
    /// </summary>
    public abstract class Base
    {
        protected Base()
        {
        }

        internal void SetContext(IExecutionContext context)
        {
            Context = context as ExecutionContext;
        }

        private ExecutionContext _context;

        /// <summary>
        /// Gets the instance of `<see cref="IFileSystem"/>` associated with this instance of
        /// `<see cref="Base"/>` based on its associated `<see cref="IExecutionContext"/>`.
        /// </summary>
        protected IFileSystem FileSystem
        {
            get { return Context.FileSystem; }
        }

        /// <summary>
        /// Gets the instance of `<see cref="GitOptions"/>` associated with this instance of
        /// `<see cref="Base"/>` based on its associated `<see cref="IExecutionContext"/>`.
        /// </summary>
        protected GitOptions Git
        {
            get { return Context.Git; }
        }

        /// <summary>
        /// Gets the instance of `<see cref="GitApi.Tracer"/>` associated with this
        /// instance of `<see cref="Base"/>` based on its associated `<see cref="IExecutionContext"/>`.
        /// </summary>
        protected ITracer Tracer
        {
            get { return Context.Tracer; }
        }

        /// <summary>
        /// Gets the instance of `<see cref="IExecutionContext"/>` associated with this
        /// instance of `<see cref="Base"/>`.
        /// </summary>
        internal ExecutionContext Context
        {
            get
            {
                if (_context == null)
                    throw new NullReferenceException(nameof(Context));

                return _context;
            }
            private set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(Context));

                _context = value;
            }
        }

        /// <summary>
        /// Gets the instance of `<see cref="GitApi.ParseHelper"/>` associated with this
        /// instance of `<see cref="Base"/>` based on its associated `<see cref="IExecutionContext"/>`.
        /// </summary>
        internal ParseHelper ParseHelper
        {
            get { return Context.ParseHelper; }
        }

        /// <summary>
        /// Gets the instance of `<see cref="Internal.PathHelper"/>` associated with this
        /// instance of `<see cref="Base"/>` based on its associated `<see cref="ExecutionContext"/>`.
        /// </summary>
        internal PathHelper PathHelper
        {
            get { return Context.PathHelper; }
        }

        /// <summary>
        /// Gets the instance of `<see cref="IWhere"/>` associated with this instance of
        /// `<see cref="Base"/>` based on its associated `<see cref="IExecutionContext"/>`.
        /// </summary>
        internal IWhere Where
        {
            get { return Context.Where; }
        }
    }
}
