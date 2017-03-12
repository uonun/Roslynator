// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Roslynator.CSharp.Refactorings;

namespace Roslynator.VisualStudio
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1D9ECCF3-5D2F-4112-9B25-264596873DC9")]
    public partial class RefactoringsOptionsPage : UIElementDialogPage
    {
        private const string RefactoringCategory = "Refactoring";

        private RefactoringsControl _refactoringsControl = new RefactoringsControl();

        protected override UIElement Child
        {
            get { return _refactoringsControl; }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);

            _refactoringsControl.Refactorings.Clear();

            SaveValuesToView(_refactoringsControl.Refactorings);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                LoadValuesFromView(_refactoringsControl.Refactorings);

                Apply();
            }

            base.OnApply(e);
        }

        private static void SetIsEnabled(string id, bool isEnabled)
        {
            if (!ConfigFileSettings.Current.Refactorings.ContainsKey(id))
            {
                RefactoringSettings.Current.SetRefactoring(id, isEnabled);
            }
            else
            {
                Debug.WriteLine("Cannot " + ((isEnabled) ? "enable" : "disable") + $" refactoring {id}, value is overridden by config file");
            }
        }
    }
}
