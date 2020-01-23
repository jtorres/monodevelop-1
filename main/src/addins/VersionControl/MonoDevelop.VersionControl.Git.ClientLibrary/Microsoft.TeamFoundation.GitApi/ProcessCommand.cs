//*************************************************************************************************
// ProcessCommand.cs
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

namespace Microsoft.TeamFoundation.GitApi
{
    public class ProcessCommand : Command
    {
        public ProcessCommand(IExecutionContext context, Environment environment, object userData)
            : base(context, environment, userData)
        { }

        public ProcessCommand(IExecutionContext context, Environment environment)
            : this(context, environment, null)
        { }

        public ProcessCommand(Environment environment, object userData)
            : this(ExecutionContext.Current, environment, userData)
        { }

        public ProcessCommand(Environment environment)
            : this(ExecutionContext.Current, environment, null)
        { }

        new public IProcess CreateProcess(string command)
            => base.CreateProcess(command);

        new public ExecuteResult Execute(string command, out string standardOutput)
            => base.Execute(command, out standardOutput);

        new internal ExecuteResult ExecuteProgress(string command, Internal.IOperation progress)
            => base.ExecuteProgress(command, progress);
    }
}
