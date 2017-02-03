// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Roslynator.CSharp.Refactorings
{
    public sealed class RefactoringSettings
    {
        public RefactoringSettings()
        {
            DisabledRefactorings = new RefactoringIdentifierSet();
        }

        public static RefactoringSettings Current { get; } = new RefactoringSettings();

        public RefactoringIdentifierSet DisabledRefactorings { get; set; }

        public bool PrefixFieldIdentifierWithUnderscore { get; set; } = true;

        public void Reset()
        {
            PrefixFieldIdentifierWithUnderscore = true;
            DisabledRefactorings.Clear();
        }

        public bool IsRefactoringEnabled(string identifier)
        {
            return !DisabledRefactorings.Contains(identifier);
        }

        public bool IsAnyRefactoringEnabled(string identifier, string identifier2)
        {
            return IsRefactoringEnabled(identifier)
                || IsRefactoringEnabled(identifier2);
        }

        public bool IsAnyRefactoringEnabled(string identifier, string identifier2, string identifier3)
        {
            return IsRefactoringEnabled(identifier)
                || IsRefactoringEnabled(identifier2)
                || IsRefactoringEnabled(identifier3);
        }

        public bool IsAnyRefactoringEnabled(string identifier, string identifier2, string identifier3, string identifier4)
        {
            return IsRefactoringEnabled(identifier)
                || IsRefactoringEnabled(identifier2)
                || IsRefactoringEnabled(identifier3)
                || IsRefactoringEnabled(identifier4);
        }

        public bool IsAnyRefactoringEnabled(string identifier, string identifier2, string identifier3, string identifier4, string identifier5)
        {
            return IsRefactoringEnabled(identifier)
                || IsRefactoringEnabled(identifier2)
                || IsRefactoringEnabled(identifier3)
                || IsRefactoringEnabled(identifier4)
                || IsRefactoringEnabled(identifier5);
        }

        public bool IsAnyRefactoringEnabled(
            string identifier,
            string identifier2,
            string identifier3,
            string identifier4,
            string identifier5,
            string identifier6)
        {
            return IsRefactoringEnabled(identifier)
                || IsRefactoringEnabled(identifier2)
                || IsRefactoringEnabled(identifier3)
                || IsRefactoringEnabled(identifier4)
                || IsRefactoringEnabled(identifier5)
                || IsRefactoringEnabled(identifier6);
        }

        public void DisableRefactoring(string identifier)
        {
            DisabledRefactorings.Add(identifier);
        }

        public void EnableRefactoring(string identifier)
        {
            DisabledRefactorings.Remove(identifier);
        }

        public void SetRefactoring(string identifier, bool isEnabled)
        {
            if (isEnabled)
            {
                EnableRefactoring(identifier);
            }
            else
            {
                DisableRefactoring(identifier);
            }
        }
    }
}
