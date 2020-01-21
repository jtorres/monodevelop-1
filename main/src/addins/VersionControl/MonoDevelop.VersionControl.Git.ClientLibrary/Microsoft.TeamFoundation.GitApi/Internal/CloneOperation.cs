//*************************************************************************************************
// CloneOperation.cs
// 
// Copyright (c) Microsoft Corporation.  All rights reserved.
//*************************************************************************************************

using System;
using System.IO;

namespace Microsoft.TeamFoundation.GitApi.Internal
{
    internal sealed class CloneOperation : Operation
    {
        static readonly OperationParser[] parsers = {
            new ReceivingObjectsParser(),
            new CheckingOutFilesParser(),
            new ResolvingDeltasParser(),
            new RemoteMessageParser(),
            new WarningAndErrorParser()
        };

        internal CloneOperation(ExecutionContext context, OperationCallback progressCallback)
            : base(context, progressCallback)
        {
        }

#if SUBMODULE_IMPL
        private readonly Regex SubmoduleCheckoutRegex = new Regex(PatternSubmoduleCheckout, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        private readonly Regex SubmoduleRegisteredRegex = new Regex(PatternSubmoduleRegistered, RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        public IReadOnlyList<Api.SubmoduleOperationStatus> Submodules
        {
            get
            {
                if (_submodules == null)
                    return null;

                return System.Linq.Enumerable.ToList(_submodules.Values);
            }
        }
        private Dictionary<string, Api.SubmoduleOperationStatus> _submodules;
#endif

        protected override bool ParseOutput (OperationOutput output)
        {
            const string SubmodulePrefix = "Submodule";
            if (output == OperationOutput.OutputClosed)
            {
                return false;
            }

            string line;
            if (TryParse(output.Message, parsers, out var progress))
            {
                Update(progress);
                return true;
            } else if ((line = CleanLine(output.Message)) != null)
            {
                Update(new GenericOperationMessage(line));
                return true;
            }
            return false;

            /*
            using (var reader = new StreamReader(readableStream, System.Text.Encoding.UTF8, false, 4 * 1024, true))
            {
                while ((line = reader.ReadLine()) != null)
                {
                  
                    else if (line.StartsWith(SubmodulePrefix))
                    {
#if SUBMODULE_IMPL
                        if (_submodules == null)
                        {
                            _submodules = new Dictionary<string, Api.SubmoduleOperationStatus>(StringComparer.Ordinal);
                        }

                        Match match;
                        if ((match = SubmoduleRegisteredRegex.Match(line)).Success)
                        {
                            string module = match.Groups[1].Value;
                            string url = match.Groups[2].Value;
                            string path = match.Groups[3].Value;

                            Api.SubmoduleOperationStatus submodule = new Api.SubmoduleOperationStatus()
                            {
                                Name = module,
                                Url = url,
                                Path = path,
                            };

                            _submodules[module] = submodule;

                            var status = new Api.CloneSubmoduleStatus(this, submodule);

                            Update(status);
                        }
                        else if ((match = SubmoduleCheckoutRegex.Match(line)).Success)
                        {
                            string module = match.Groups[1].Value;
                            string sha1 = match.Groups[2].Value;
                            Api.ObjectId objectId = Api.ObjectId.FromString(sha1);

                            Api.SubmoduleOperationStatus submodule = default(Api.SubmoduleOperationStatus);

                            submodule = _submodules[module];

                            submodule = new Api.SubmoduleOperationStatus(submodule)
                            {
                                Head = objectId,
                            };

                            _submodules[module] = submodule;

                            var status = new Api.CloneSubmoduleStatus(this, submodule);

                            Update(status);
                        }
#endif
                    }
                    
                }*/
        }
    }
}
