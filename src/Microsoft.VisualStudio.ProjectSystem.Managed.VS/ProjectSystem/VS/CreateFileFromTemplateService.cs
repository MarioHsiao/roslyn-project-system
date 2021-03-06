﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.ProjectSystem.VS
{
    /// <summary>
    /// This service creates a file from a given file template.
    /// </summary>
    [Export(typeof(ICreateFileFromTemplateService))]
    internal class CreateFileFromTemplateService : ICreateFileFromTemplateService
    {
        private readonly IUnconfiguredProjectVsServices _projectVsServices;
        private readonly IDteServices _dteServices;
        private readonly ProjectProperties _properties;

        [ImportingConstructor]
        public CreateFileFromTemplateService(IUnconfiguredProjectVsServices projectVsServices, IDteServices dteServices, ProjectProperties properties)
        {
            Requires.NotNull(projectVsServices, nameof(projectVsServices));
            Requires.NotNull(dteServices, nameof(dteServices));
            Requires.NotNull(properties, nameof(properties));

            _projectVsServices = projectVsServices;
            _dteServices = dteServices;
            _properties = properties;
        }

        /// <summary>
        /// Create a file with the given template file and add it to the parent node.
        /// </summary>
        /// <param name="templateFile">The name of the template zip file.</param>
        /// <param name="parentNode">The node to which the new file will be added.</param>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <returns>true if file is added successfully.</returns>
        public async Task<bool> CreateFileAsync(string templateFile, IProjectTree parentNode, string fileName)
        {
            Requires.NotNull(templateFile, nameof(templateFile));
            Requires.NotNull(parentNode, nameof(parentNode));
            Requires.NotNull(fileName, nameof(fileName));

            string templateLanguage = await GetTemplateLanguageAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(templateLanguage))
                return false;

            await _projectVsServices.ThreadingService.SwitchToUIThread();

            string templateFilePath = _dteServices.Solution.GetProjectItemTemplate(templateFile, templateLanguage);

            if (templateFilePath != null)
            {
                var parentId = parentNode.GetHierarchyId();
                var result = new VSADDRESULT[1];
                _projectVsServices.VsProject.AddItemWithSpecific(parentId, VSADDITEMOPERATION.VSADDITEMOP_RUNWIZARD, fileName, 0, new string[] { templateFilePath }, IntPtr.Zero, 0, Guid.Empty, null, Guid.Empty, result);

                if (result[0] == VSADDRESULT.ADDRESULT_Success)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<string> GetTemplateLanguageAsync()
        {
            ConfigurationGeneral general = await _properties.GetConfigurationGeneralPropertiesAsync()
                                                            .ConfigureAwait(false);

            return (string)await general.TemplateLanguage.GetValueAsync()
                                                         .ConfigureAwait(false);
        }
    }
}
