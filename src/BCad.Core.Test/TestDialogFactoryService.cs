// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Composition;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Core.Test
{
    [ExportWorkspaceService, Shared]
    public class TestDialogFactoryService : IDialogFactoryService
    {
        public static Action<INotifyPropertyChanged> ModifyTestFileSettingsTransform = null;

        public Task<bool?> ShowDialog(string type, string id, INotifyPropertyChanged viewModel = null)
        {
            if (type == "FileSettings")
            {
                ModifyTestFileSettingsTransform?.Invoke(viewModel);
                return Task.FromResult<bool?>(true);
            }

            throw new NotImplementedException();
        }
    }
}
