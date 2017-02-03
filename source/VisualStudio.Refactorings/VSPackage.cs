// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Roslynator.CSharp.Refactorings;
using Roslynator.VisualStudio.Settings;

namespace Roslynator.VisualStudio
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid("F8A97C1B-B204-42B9-A881-816D250F2074")]
    [ProvideOptionPage(typeof(GeneralOptionsPage), "Roslynator", "General", 0, 0, true)]
    [ProvideOptionPage(typeof(RefactoringsOptionsPage), "Roslynator", "Refactorings", 0, 0, true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string)]
    public sealed class VSPackage : Package, IVsSolutionEvents
    {
        private uint _cookie;
        private FileSystemWatcher _watcher;

        public VSPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            IVsSolution solution = GetService(typeof(SVsSolution)) as IVsSolution;

            if (solution != null)
                solution.AdviseSolutionEvents(this, out _cookie);
        }

        private void ReloadSettings()
        {
            RefactoringSettings settings = RefactoringSettings.Current;

            settings.Reset();

            settings.DisableRefactoring(RefactoringIdentifiers.IntroduceConstructor);
            settings.DisableRefactoring(RefactoringIdentifiers.RemoveAllDocumentationComments);
            settings.DisableRefactoring(RefactoringIdentifiers.ReplaceEmptyStringLiteralWithStringEmpty);
            settings.DisableRefactoring(RefactoringIdentifiers.ReplaceMethodWithProperty);

            var generalOptionsPage = (GeneralOptionsPage)GetDialogPage(typeof(GeneralOptionsPage));
            generalOptionsPage.Apply();

            var refactoringsOptionsPage = (RefactoringsOptionsPage)GetDialogPage(typeof(RefactoringsOptionsPage));
            refactoringsOptionsPage.Apply();

            ApplicationSettings appSettings = LoadApplicationSettings(settings);

            if (appSettings != null)
            {
                settings.PrefixFieldIdentifierWithUnderscore = appSettings.PrefixFieldIdentifierWithUnderscore;

                foreach (KeyValuePair<string, bool> kvp in appSettings.Refactorings)
                    settings.SetRefactoring(kvp.Key, kvp.Value);
            }
        }

        private ApplicationSettings LoadApplicationSettings(RefactoringSettings settings)
        {
            var dte = GetService(typeof(DTE)) as DTE;

            if (dte != null)
            {
                string path = dte.Solution.FullName;

                if (!string.IsNullOrEmpty(path))
                {
                    string directoryPath = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        path = Path.Combine(directoryPath, ApplicationSettings.FileName);

                        if (File.Exists(path))
                        {
                            try
                            {
                                return ApplicationSettings.Load(path);
                            }
                            catch (IOException)
                            {
                            }
                            catch (UnauthorizedAccessException)
                            {
                            }
                            catch (SecurityException)
                            {
                            }
                        }
                    }
                }
            }

            return default(ApplicationSettings);
        }

        private void WatchConfigFile()
        {
            var dte = GetService(typeof(DTE)) as DTE;

            if (dte != null)
            {
                string path = dte.Solution.FullName;

                if (!string.IsNullOrEmpty(path))
                {
                    string directoryPath = Path.GetDirectoryName(path);

                    if (!string.IsNullOrEmpty(directoryPath))
                    {
                        _watcher = new FileSystemWatcher(directoryPath, ApplicationSettings.FileName)
                        {
                            EnableRaisingEvents = true,
                            IncludeSubdirectories = false
                        };

                        _watcher.Changed += (object sender, FileSystemEventArgs e) => ReloadSettings();
                        _watcher.Created += (object sender, FileSystemEventArgs e) => ReloadSettings();
                        _watcher.Deleted += (object sender, FileSystemEventArgs e) => ReloadSettings();
                    }
                }
            }
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            ReloadSettings();
            WatchConfigFile();

            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
                _watcher = null;
            }

            return VSConstants.S_OK;
        }
    }
}
