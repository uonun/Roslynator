// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Roslynator.VisualStudio
{
    public partial class RefactoringsControl : UserControl
    {
        public RefactoringsControl()
        {
            InitializeComponent();

            DataContext = this;
        }

        public ObservableCollection<RefactoringModel> Refactorings { get; } = new ObservableCollection<RefactoringModel>();
    }
}
