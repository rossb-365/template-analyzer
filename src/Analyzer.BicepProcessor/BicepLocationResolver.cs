// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using Bicep.Core.Emit;
using Microsoft.Azure.Templates.Analyzer.Types;

namespace Microsoft.Azure.Templates.Analyzer.Utilities
{
    /// <summary>
    /// An <see cref="ILineNumberResolver"/> used for resolving line numbers from a compiled JSON template to the original Bicep template.
    /// </summary>
    public class BicepLocationResolver : ILineNumberResolver
    {
        private readonly JsonLineNumberResolver jsonLineNumberResolver;
        private readonly SourceMap sourceMap;

        /// <summary>
        /// Create a new instance with the given <see cref="TemplateContext"/>.
        /// </summary>
        /// <param name="templateContext">The template context to map JSON paths against.</param>
        public BicepLocationResolver(TemplateContext templateContext)
        {
            this.jsonLineNumberResolver = new(templateContext ?? throw new ArgumentNullException(nameof(templateContext)));
            this.sourceMap = (templateContext.SourceMap as SourceMap) ?? throw new ArgumentNullException(nameof(templateContext.SourceMap));
        }

        /// <summary>
        /// Given a JSON path in an expanded JSON template from a compiled Bicep file, find the equivalent line number
        /// in the original Bicep template.
        /// </summary>
        /// <param name="pathInExpandedTemplate">The path in the expanded template
        /// to find the line number of in the original template.</param>
        /// <returns>The line number of the equivalent location in the original template,
        /// or 1 if it can't be determined.</returns>
        public int ResolveLineNumber(string pathInExpandedTemplate)
        {
            var jsonLine = this.jsonLineNumberResolver.ResolveLineNumber(pathInExpandedTemplate);

            // Source map line numbers from Bicep are 0-indexed
            jsonLine--;

            // TODO: look for mappings in other files/modules once nested templates are supported, for now just entrypoint file
            var entrypointFile = this.sourceMap.Entries.First(entry => entry.FilePath == this.sourceMap.Entrypoint);
            var match = entrypointFile.SourceMap.FirstOrDefault(mapping => mapping.TargetLine == jsonLine);

            return (match != null)
                ? match.SourceLine + 1 // convert to 1-indexing
                : 1;
        }
    }
}