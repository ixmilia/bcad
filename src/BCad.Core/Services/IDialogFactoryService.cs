// Copyright (c) IxMilia.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.ComponentModel;
using System.Threading.Tasks;

namespace BCad.Services
{
    public interface IDialogFactoryService : IWorkspaceService
    {
        Task<bool?> ShowDialog(string type, string id, INotifyPropertyChanged viewModel = null);
    }
}
