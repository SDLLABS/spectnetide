﻿using GalaSoft.MvvmLight.Command;
using Spect.Net.Assembler.Assembler;
using Spect.Net.Wpf.Mvvm;

namespace Spect.Net.VsPackage.Z80Programs
{
    /// <summary>
    /// This class represents the view model of the 
    /// Export Z80 Program command's UI
    /// </summary>
    public class ExportZ80ProgramViewModel: EnhancedViewModelBase
    {
        private ExportFormat _format;
        private string _name;
        private string _filename;
        private bool _autoStart;
        private bool _applyClear;
        private bool _singleBlock;
        private bool _addToProject;
        private bool _isValid;

        /// <summary>
        /// Gets or sets the tape format of the export
        /// </summary>
        public ExportFormat Format
        {
            get => _format;
            set => Set(ref _format, value);
        }

        /// <summary>
        /// Gets or sets the name of the program to export
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        /// <summary>
        /// Gets or sets the name of the file (with full path) to save the code
        /// </summary>
        public string Filename
        {
            get => _filename;
            set => Set(ref _filename, value);
        }

        /// <summary>
        /// Indicates if autostart block should be saved
        /// </summary>
        public bool AutoStart
        {
            get => _autoStart;
            set => Set(ref _autoStart, value);
        }

        /// <summary>
        /// Indicates if a clear commands should be applied in
        /// the auto start block
        /// </summary>
        public bool ApplyClear
        {
            get => _applyClear;
            set => Set(ref _applyClear, value);
        }

        /// <summary>
        /// Indicates if multiple segments should be saved into a single block
        /// </summary>
        public bool SingleBlock
        {
            get => _singleBlock;
            set => Set(ref _singleBlock, value);
        }

        /// <summary>
        /// Indicates if the saved project file should be added to the project
        /// </summary>
        public bool AddToProject
        {
            get => _addToProject;
            set => Set(ref _addToProject, value);
        }

        /// <summary>
        /// Signs if the dialog content is valid
        /// </summary>
        public bool IsValid
        {
            get => _isValid;
            set => Set(ref _isValid, value);
        }

        /// <summary>
        /// Cancels the dialog
        /// </summary>
        public RelayCommand CancelCommand { get; }

        /// <summary>
        /// Completes the dialog by signing Export
        /// </summary>
        public RelayCommand ExportCommand { get; }

        /// <summary>
        /// The assembler output to save
        /// </summary>
        public AssemblerOutput AssemblerOutput { get; set; }

        public ExportZ80ProgramViewModel()
        {
        }
    }
}