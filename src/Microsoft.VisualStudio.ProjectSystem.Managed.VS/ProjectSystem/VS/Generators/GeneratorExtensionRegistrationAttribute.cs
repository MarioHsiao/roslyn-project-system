﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;
using System;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Generators
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    internal sealed class GeneratorExtensionRegistrationAttribute : RegistrationAttribute
    {
        private readonly string _extension;
        private readonly string _generator;
        private readonly string _contextGuid;

        public GeneratorExtensionRegistrationAttribute(string extension, string generator, string contextGuid)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (generator == null)
                throw new ArgumentNullException(nameof(generator));
            if (contextGuid == null)
                throw new ArgumentNullException(nameof(contextGuid));

            _extension = extension;
            _generator = generator;
            _contextGuid = contextGuid;
        }

        public override void Register(RegistrationContext context)
        {
            using (Key childKey = context.CreateKey($"Generators\\{_contextGuid}\\{_extension}"))
            {
                childKey.SetValue(string.Empty, _generator);
            }
        }

        public override void Unregister(RegistrationContext context)
        {
            context.RemoveKey(_extension);
        }
    }
}
