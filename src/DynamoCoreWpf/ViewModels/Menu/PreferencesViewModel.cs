﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using Dynamo.Configuration;
using Dynamo.Core;
using Dynamo.Events;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.Models;
using Dynamo.PackageManager;
using Dynamo.PythonServices;
using Dynamo.Utilities;
using Dynamo.Wpf.Properties;
using Dynamo.Wpf.ViewModels.Core.Converters;
using Res = Dynamo.Wpf.Properties.Resources;

namespace Dynamo.ViewModels
{
    /// <summary>
    /// The next enum will contain the posible values for Scaling (Visual Settings -> Geometry Scaling section)
    /// </summary>
    public enum GeometryScaleSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }

    public class PreferencesViewModel : ViewModelBase, INotifyPropertyChanged
    {
        #region Private Properties
        private string savedChangesLabel;
        private string savedChangesTooltip;
        private string currentWarningMessage;
        private string selectedPackagePathForInstall;

        private string selectedLanguage;
        private string selectedFontSize;
        private string selectedNumberFormat;
        private string selectedPythonEngine;

        private ObservableCollection<string> languagesList;
        private ObservableCollection<string> packagePathsForInstall;
        private ObservableCollection<string> fontSizeList;
        private ObservableCollection<string> numberFormatList;
        private StyleItem addStyleControl;
        private ObservableCollection<string> pythonEngineList;

        private RunType runSettingsIsChecked;
        private Dictionary<string, TabSettings> preferencesTabs;

        private readonly PreferenceSettings preferenceSettings;
        private readonly DynamoPythonScriptEditorTextOptions pythonScriptEditorTextOptions;
        private readonly DynamoViewModel dynamoViewModel;
        private readonly InstalledPackagesViewModel installedPackagesViewModel;

        private bool isWarningEnabled;
        private bool isSaveButtonEnabled = true;
        private bool isVisibleAddStyleBorder;
        private bool isEnabledAddStyleButton;
        private GeometryScalingOptions optionsGeometryScale = null;

        #endregion Private Properties

        public GeometryScaleSize ScaleSize { get; set; }

        public Tuple<string, string, string> ScaleRange
        {
            get
            {
                return scaleRanges[ScaleSize];
            }
        }

        private Dictionary<GeometryScaleSize, Tuple<string, string, string>> scaleRanges = new Dictionary<GeometryScaleSize, Tuple<string, string, string>>
        {
            {GeometryScaleSize.Medium, new Tuple<string, string, string>("medium", "0.0001", "10,000")},
            {GeometryScaleSize.Small, new Tuple<string, string, string>("small", "0.000,001", "100")},
            {GeometryScaleSize.Large, new Tuple<string, string, string>("large", "0.01", "1,000,000")},
            {GeometryScaleSize.ExtraLarge, new Tuple<string, string, string>("extra large", "1", "100,000,000")}
        };

        /// <summary>
        /// This property will be used by the Preferences screen to store and retrieve all the settings from the expanders
        /// </summary>
        public Dictionary<string, TabSettings> PreferencesTabs
        {
            get
            {
                return preferencesTabs;
            }
            set
            {
                preferencesTabs = value;
                RaisePropertyChanged(nameof(PreferencesTabs));
            }
        }

        /// <summary>
        /// Controls what the SavedChanges label will display
        /// </summary>
        public string SavedChangesLabel
        {
            get
            {
                return savedChangesLabel;
            }
            set
            {
                savedChangesLabel = value;
                RaisePropertyChanged(nameof(SavedChangesLabel));
            }
        }

        /// <summary>
        /// Controls what SavedChanges label's tooltip will display
        /// </summary>
        public string SavedChangesTooltip
        {
            get
            {
                return savedChangesTooltip;
            }
            set
            {
                savedChangesTooltip = value;
                RaisePropertyChanged(nameof(SavedChangesTooltip));

            }
        }

        /// <summary>
        /// Returns all installed packages
        /// </summary>
        public ObservableCollection<PackageViewModel> LocalPackages => installedPackagesViewModel.LocalPackages;

        /// <summary>
        /// Returns all available filters
        /// </summary>
        public ObservableCollection<PackageFilter> Filters => installedPackagesViewModel.Filters;

        //This includes all the properties that can be set on the General tab
        #region General Properties
        /// <summary>
        /// Controls the Selected option in Language ComboBox
        /// </summary>
        public string SelectedLanguage
        {
            get
            {
                return selectedLanguage;
            }
            set
            {
                selectedLanguage = value;
                RaisePropertyChanged(nameof(SelectedLanguage));
            }
        }

        /// <summary>
        /// Controls the Selected option in Node Font Size ComboBox
        /// </summary>
        public string SelectedFontSize
        {
            get
            {
                return selectedFontSize;
            }
            set
            {
                selectedFontSize = value;
                RaisePropertyChanged(nameof(SelectedFontSize));
            }
        }

        /// <summary>
        /// Controls the Selected option in Number Format ComboBox
        /// </summary>
        public string SelectedNumberFormat
        {
            get
            {
                return preferenceSettings.NumberFormat;
            }
            set
            {
                selectedNumberFormat = value;
                preferenceSettings.NumberFormat = value;
                RaisePropertyChanged(nameof(SelectedNumberFormat));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the RunSettings radio button
        /// </summary>
        public bool RunSettingsIsChecked
        {
            get
            {
                return runSettingsIsChecked == RunType.Manual;
            }
            set
            {
                if (value)
                {
                    preferenceSettings.DefaultRunType = RunType.Manual;
                    runSettingsIsChecked = RunType.Manual;
                }
                else
                {
                    preferenceSettings.DefaultRunType = RunType.Automatic;
                    runSettingsIsChecked = RunType.Automatic;
                }
                RaisePropertyChanged(nameof(RunSettingsIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewIsChecked
        {
            get
            {
                return preferenceSettings.ShowRunPreview;
            }
            set
            {
                preferenceSettings.ShowRunPreview = value;
                dynamoViewModel.ShowRunPreview = value;
                RaisePropertyChanged(nameof(RunPreviewIsChecked));
            }
        }

        /// <summary>
        /// Controls the Enabled property in the Show Run Preview toogle button
        /// </summary>
        public bool RunPreviewEnabled
        {
            get
            {
                return dynamoViewModel.HomeSpaceViewModel.RunSettingsViewModel.RunButtonEnabled;
            }
        }

        /// <summary>
        /// LanguagesList property containt the list of all the languages listed in: https://wiki.autodesk.com/display/LOCGD/Dynamo+Languages
        /// </summary>
        public ObservableCollection<string> LanguagesList
        {
            get
            {
                return languagesList;
            }
            set
            {
                languagesList = value;
                RaisePropertyChanged(nameof(LanguagesList));
            }
        }

        /// <summary>
        /// PackagePathsForInstall contains the list of all package paths where
        /// packages can be installed.
        /// </summary>
        public ObservableCollection<string> PackagePathsForInstall
        {
            get
            {
                var allowedFileExtensions = new string[] { ".dll", ".ds" };
                if (packagePathsForInstall == null || !packagePathsForInstall.Any())
                {
                    var programDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    // Filter Builtin Packages and ProgramData paths from list of paths for download
                    var customPaths = preferenceSettings.CustomPackageFolders.Where(
                        x => x != DynamoModel.BuiltInPackagesToken && !x.StartsWith(programDataPath));
                    //filter out paths that have extensions ending in .dll or .ds
                    var directoryPaths = customPaths.Where(path => !(Path.HasExtension(path) && allowedFileExtensions.Contains(Path.GetExtension(path).ToLower())));

                    packagePathsForInstall = new ObservableCollection<string>();
                    foreach (var path in directoryPaths)
                    {
                            packagePathsForInstall.Add(path);
                    }
                }
                return packagePathsForInstall;
            }
            set
            {
                packagePathsForInstall = value;
                RaisePropertyChanged(nameof(PackagePathsForInstall));
            }
        }

        /// <summary>
        /// Currently selected package path where new packages will be downloaded.
        /// </summary>
        public string SelectedPackagePathForInstall
        {
            get
            {
                return selectedPackagePathForInstall;
            }
            set
            {
                if (selectedPackagePathForInstall != value)
                {
                    selectedPackagePathForInstall = value;
                    RaisePropertyChanged(nameof(SelectedPackagePathForInstall));
                }
            }
        }

        /// <summary>
        /// Flag specifying whether loading built-in packages
        /// is disabled, if true, or enabled, if false.
        /// </summary>
        public bool DisableBuiltInPackages 
        { 
            get 
            {
                return preferenceSettings.DisableBuiltinPackages;
            } 
            set 
            {
                preferenceSettings.DisableBuiltinPackages = value;
                PackagePathsViewModel.SetPackagesScheduledState(PathManager.BuiltinPackagesDirectory, value);
                RaisePropertyChanged(nameof(DisableBuiltInPackages));
            }
        }

        /// <summary>
        /// Flag specifying whether loading custom packages
        /// is disabled, if true, or enabled, if false.
        /// </summary>
        public bool DisableCustomPackages 
        { 
            get
            {
                return preferenceSettings.DisableCustomPackageLocations;
            }
            set
            {
                preferenceSettings.DisableCustomPackageLocations = value;
                foreach(var path in preferenceSettings.CustomPackageFolders.Where(x => x != DynamoModel.BuiltInPackagesToken))
                {
                    PackagePathsViewModel.SetPackagesScheduledState(path, value);
                }
                RaisePropertyChanged(nameof(DisableCustomPackages));
            } 
        }

        /// <summary>
        /// Flag specifying whether trust warnings should be shown
        /// when opening .dyn files from unstrusted locations.
        /// </summary>
        public bool DisableTrustWarnings
        {
            get
            {
                return preferenceSettings.DisableTrustWarnings;
            }
            // We keep this setter private to avoid view extensions calling it directly.
            // Access modifiers are not intended for security, but it's simple enough to hook a toggle to the UI
            // without binding, and this makes it clear it's not an API.
            internal set
            {
                preferenceSettings.SetTrustWarningsDisabled(value);
            }
        }
        /// <summary>
        /// FontSizesList contains the list of sizes for fonts defined (the ones defined are Small, Medium, Large, Extra Large)
        /// </summary>
        public ObservableCollection<string> FontSizeList
        {
            get
            {
                return fontSizeList;
            }
            set
            {
                fontSizeList = value;
                RaisePropertyChanged(nameof(FontSizeList));
            }
        }

        /// <summary>
        /// NumberFormatList contains the list of the format for numbers, right now in Dynamo has the next formats: 0, 0.0, 0.00, 0.000, 0.0000
        /// </summary>
        public ObservableCollection<string> NumberFormatList
        {
            get
            {
                return numberFormatList;
            }
            set
            {
                numberFormatList = value;
                RaisePropertyChanged(nameof(NumberFormatList));
            }
        }
        #endregion

        //This includes all the properties that can be set on the Visual Settings tab
        #region VisualSettings Properties
        /// <summary>
        /// This will contain a list of all the Styles created by the user in the Styles list ( Visual Settings -> Group Styles section)
        /// </summary>
        public ObservableCollection<GroupStyleItem> StyleItemsList
        {
            get { return preferenceSettings.GroupStyleItemsList.ToObservableCollection(); }
            set
            {
                preferenceSettings.GroupStyleItemsList = value.ToList<GroupStyleItem>();
                RaisePropertyChanged(nameof(StyleItemsList));
            }
        }

        /// <summary>
        /// Used to add styles to the StyleItemsListe while also update the saved changes label
        /// </summary>
        /// <param name="style">style to be added</param>
        public void AddStyle(StyleItem style)
        {
            preferenceSettings.GroupStyleItemsList.Add(new GroupStyleItem { 
                HexColorString = style.HexColorString, 
                Name = style.Name, 
                IsDefault = style.IsDefault
            });
            RaisePropertyChanged(nameof(StyleItemsList));
        }

      

        /// <summary>
        /// This flag will be in true when the Style that user is trying to add already exists (otherwise will be false - Default)
        /// </summary>
        public bool IsWarningEnabled
        {
            get
            {
                return isWarningEnabled;
            }
            set
            {
                isWarningEnabled = value;
                RaisePropertyChanged(nameof(IsWarningEnabled));
            }
        }

        /// <summary>
        /// This property will hold the warning message that has to be shown in the warning icon next to the TextBox
        /// </summary>
        public string CurrentWarningMessage
        {
            get
            {
                return currentWarningMessage;
            }
            set
            {
                currentWarningMessage = value;
                RaisePropertyChanged(nameof(CurrentWarningMessage));
            }
        }

        /// <summary>
        /// This property describes if the SaveButton will be enabled or not (when trying to save a new Style).
        /// </summary>
        public bool IsSaveButtonEnabled
        {
            get
            {
                return isSaveButtonEnabled;
            }
            set
            {
                isSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(IsSaveButtonEnabled));
            }
        }

        /// <summary>
        /// This property was created just a container for default information when the user is adding a new Style
        /// When users press the Add Style button some controls are shown so the user can populate them, this property will contain default values shown
        /// </summary>
        public StyleItem AddStyleControl
        {
            get
            {
                return addStyleControl;
            }
            set
            {
                addStyleControl = value;
                RaisePropertyChanged(nameof(AddStyleControl));
            }
        }

        /// <summary>
        /// This property is used as a container for the description text (GeometryScalingOptions.DescriptionScaleRange) for each radio button (Visual Settings -> Geometry Scaling section)
        /// </summary>
        public GeometryScalingOptions OptionsGeometryScale
        {
            get
            {
                return optionsGeometryScale;
            }
            set
            {
                optionsGeometryScale = value;
                RaisePropertyChanged(nameof(OptionsGeometryScale));
            }
        }

        /// <summary>
        /// Controls the binding for the ShowEdges toogle in the Preferences->Visual Settings->Display Settings section
        /// </summary>
        public bool ShowEdges
        {
            get
            {
                return dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges;
            }
            set
            {
                dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges = value;
                RaisePropertyChanged(nameof(ShowEdges));
            }
        }

        /// <summary>
        /// Controls the binding for the IsolateSelectedGeometry toogle in the Preferences->Visual Settings->Display Settings section
        /// </summary>
        public bool IsolateSelectedGeometry
        {
            get
            {
                return dynamoViewModel.BackgroundPreviewViewModel.IsolationMode;
            }
            set
            {
                dynamoViewModel.BackgroundPreviewViewModel.IsolationMode = value;
                RaisePropertyChanged(nameof(IsolateSelectedGeometry));
            }
        }

        /// <summary>
        /// This property is bind to the Render Precision Slider and control the amount of tessellation applied to objects in background preview
        /// </summary>
        public int TessellationDivisions
        {
            get
            {
                return dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions;
            }
            set
            {
                dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = value;
                RaisePropertyChanged(nameof(TessellationDivisions));
            }
        }

        /// <summary>
        /// Indicates if preview bubbles should be displayed on nodes.
        /// </summary>
        public bool ShowPreviewBubbles
        {
            get
            {
                return preferenceSettings.ShowPreviewBubbles;
            }
            set
            {
                preferenceSettings.ShowPreviewBubbles = value;
                RaisePropertyChanged(nameof(ShowPreviewBubbles));
            }
        }

        /// <summary>
        /// Indicates if line numbers should be displayed on code block nodes.
        /// </summary>
        public bool ShowCodeBlockLineNumber
        {
            get
            {
                return preferenceSettings.ShowCodeBlockLineNumber;
            }
            set
            {
                preferenceSettings.ShowCodeBlockLineNumber = value;
                RaisePropertyChanged(nameof(ShowCodeBlockLineNumber));
            }
        }

        /// <summary>
        /// This property will make Visible or Collapse the AddStyle Border defined in the GroupStyles section
        /// </summary>
        public bool IsVisibleAddStyleBorder 
        {
            get
            {
                return isVisibleAddStyleBorder;
            } 
            set
            {
                isVisibleAddStyleBorder = value;
                RaisePropertyChanged(nameof(IsVisibleAddStyleBorder));
            }
        }

        /// <summary>
        /// This property will Enable or Disable the AddStyle button defined in the GroupStyles section
        /// </summary>
        public bool IsEnabledAddStyleButton 
        {
            get
            {
                return isEnabledAddStyleButton;
            }
            set
            {
                isEnabledAddStyleButton = value;
                RaisePropertyChanged(nameof(IsEnabledAddStyleButton));
            }
        }
        #endregion

        //This includes all the properties that can be set on the Features tab
        #region Features Properties
        /// <summary>
        /// PythonEnginesList contains the list of Python engines available
        /// </summary>
        public ObservableCollection<string> PythonEnginesList
        {
            get
            {
                return pythonEngineList;
            }
            set
            {
                pythonEngineList = value;
                RaisePropertyChanged(nameof(PythonEnginesList));
            }
        }

        /// <summary>
        /// Controls the Selected option in Python Engine combobox
        /// </summary>
        public string SelectedPythonEngine
        {
            get
            {
                return selectedPythonEngine;
            }
            set
            {
                if (value != selectedPythonEngine)
                {
                    selectedPythonEngine = value;
                    if(value != Res.DefaultPythonEngineNone)
                    {
                        preferenceSettings.DefaultPythonEngine = value;
                    }
                    else{
                        preferenceSettings.DefaultPythonEngine = string.Empty;
                    }

                    RaisePropertyChanged(nameof(SelectedPythonEngine));
                }
            }
        }
        
        /// <summary>
        /// Controls the IsChecked property in the "Hide IronPython alerts" toogle button
        /// </summary>
        public bool HideIronPythonAlertsIsChecked
        {
            get
            {
                return preferenceSettings.IsIronPythonDialogDisabled;
            }
            set
            {
                preferenceSettings.IsIronPythonDialogDisabled = value;
                RaisePropertyChanged(nameof(HideIronPythonAlertsIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Show Whitespace in Python editor" toogle button
        /// </summary>
        public bool ShowWhitespaceIsChecked
        {
            get
            {
                return preferenceSettings.ShowTabsAndSpacesInScriptEditor;
            }
            set
            {
                pythonScriptEditorTextOptions.ShowWhiteSpaceCharacters(value);
                preferenceSettings.ShowTabsAndSpacesInScriptEditor = value;
                RaisePropertyChanged(nameof(ShowWhitespaceIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Node autocomplete" toogle button
        /// </summary>
        public bool NodeAutocompleteIsChecked
        {
            get
            {
                return preferenceSettings.EnableNodeAutoComplete;
            }
            set
            {
                preferenceSettings.EnableNodeAutoComplete = value;
                RaisePropertyChanged(nameof(NodeAutocompleteIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Notification Center" toogle button
        /// </summary>
        public bool NotificationCenterIsChecked
        {
            get
            {
                return preferenceSettings.EnableNotificationCenter;
            }
            set
            {
                preferenceSettings.EnableNotificationCenter = value;
                RaisePropertyChanged(nameof(NotificationCenterIsChecked));
            }
        }

        /// <summary>
        /// Controls the IsChecked property in the "Enable T-spline nodes" toogle button
        /// </summary>
        public bool EnableTSplineIsChecked
        {
            get
            {
                return !preferenceSettings.NamespacesToExcludeFromLibrary.Contains(
                    "ProtoGeometry.dll:Autodesk.DesignScript.Geometry.TSpline");
            }
            set
            {
                HideUnhideNamespace(!value, "ProtoGeometry.dll", "Autodesk.DesignScript.Geometry.TSpline");
                RaisePropertyChanged(nameof(EnableTSplineIsChecked));
            }
        }

        /// <summary>
        /// This method updates the node search library to either hide or unhide nodes that belong
        /// to a specified assembly name and namespace. These nodes will be hidden from the node
        /// library sidebar and from the node search.
        /// </summary>
        /// <param name="hide">Set to true to hide, set to false to unhide.</param>
        /// <param name="library">The assembly name of the library.</param>
        /// <param name="namespc">The namespace of the nodes to be hidden.</param>
        internal void HideUnhideNamespace(bool hide, string library, string namespc)
        {
            var str = library + ':' + namespc;
            var namespaces = preferenceSettings.NamespacesToExcludeFromLibrary;

            if (hide)
            {
                if (!namespaces.Contains(str))
                {
                    namespaces.Add(str);
                }
            }
            else // unhide
            {
                namespaces.Remove(str);
            }
        }

        private void AddPythonEnginesOptions()
        {
            var options = new ObservableCollection<string> { Res.DefaultPythonEngineNone };
            foreach (var item in PythonEngineManager.Instance.AvailableEngines)
            {
                options.Add(item.Name);
            }
            PythonEnginesList = options;
        }
        #endregion

        /// <summary>
        /// Package Search Paths view model.
        /// </summary>
        public PackagePathViewModel PackagePathsViewModel { get; set; }

        /// <summary>
        /// Trusted Paths view model.
        /// </summary>
        public TrustedPathViewModel TrustedPathsViewModel { get; set; }

        /// <summary>
        /// Import Settings
        /// </summary>
        /// <param name="filePath"></param>
        public void importSettings(string filePath)
        {
            var newPreferences = PreferenceSettings.Load(filePath);
            newPreferences.CopyProperties(preferenceSettings);

            // Explicit copy
            preferenceSettings.SetTrustWarningsDisabled(newPreferences.DisableTrustWarnings);
            preferenceSettings.SetTrustedLocations(newPreferences.TrustedLocations);
            TrustedPathsViewModel?.InitializeTrustedLocations();

            // Set the not explicit Binding
            runSettingsIsChecked = preferenceSettings.DefaultRunType;
            var engine = PythonEnginesList.FirstOrDefault(x => x.Equals(preferenceSettings.DefaultPythonEngine));
            SelectedPythonEngine = string.IsNullOrEmpty(engine) ? Res.DefaultPythonEngineNone : preferenceSettings.DefaultPythonEngine;
            dynamoViewModel.RenderPackageFactoryViewModel.MaxTessellationDivisions = preferenceSettings.RenderPrecision;
            dynamoViewModel.RenderPackageFactoryViewModel.ShowEdges = preferenceSettings.ShowEdges;
            PackagePathsForInstall = null;
            PackagePathsViewModel?.InitializeRootLocations();

            dynamoViewModel.IsShowingConnectors = preferenceSettings.ShowConnector;
            dynamoViewModel.IsShowingConnectorTooltip = preferenceSettings.ShowConnectorToolTip;
            foreach (var item in dynamoViewModel.Watch3DViewModels)
            {
                var preferenceItem = preferenceSettings.BackgroundPreviews.Where(i => i.Name == item.PreferenceWatchName).FirstOrDefault();
                if (preferenceItem != null)
                {
                    item.Active = preferenceItem.IsActive;
                }                
            }

            RaisePropertyChanged(string.Empty);
        }

        /// <summary>
        /// The PreferencesViewModel constructor basically initialize all the ItemsSource for the corresponding ComboBox in the View (PreferencesView.xaml)
        /// </summary>
        public PreferencesViewModel(DynamoViewModel dynamoViewModel)
        {
            this.preferenceSettings = dynamoViewModel.PreferenceSettings;
            this.pythonScriptEditorTextOptions = dynamoViewModel.PythonScriptEditorTextOptions;
            this.dynamoViewModel = dynamoViewModel;
            this.installedPackagesViewModel = new InstalledPackagesViewModel(dynamoViewModel, 
                dynamoViewModel.PackageManagerClientViewModel.PackageManagerExtension.PackageLoader);

            // Scan for engines
            AddPythonEnginesOptions();

            PythonEngineManager.Instance.AvailableEngines.CollectionChanged += PythonEnginesChanged;

            //Sets SelectedPythonEngine.
            //If the setting is empty it corresponds to the default python engine
            var engine = PythonEnginesList.FirstOrDefault(x => x.Equals(preferenceSettings.DefaultPythonEngine));
            SelectedPythonEngine  = string.IsNullOrEmpty(engine) ? Res.DefaultPythonEngineNone : preferenceSettings.DefaultPythonEngine;

            string languages = Wpf.Properties.Resources.PreferencesWindowLanguages;
            LanguagesList = new ObservableCollection<string>(languages.Split(','));
            SelectedLanguage = languages.Split(',').First();

            FontSizeList = new ObservableCollection<string>
            {
                Wpf.Properties.Resources.ScalingSmallButton,
                Wpf.Properties.Resources.ScalingMediumButton,
                Wpf.Properties.Resources.ScalingLargeButton,
                Wpf.Properties.Resources.ScalingExtraLargeButton
            };
            SelectedFontSize = Wpf.Properties.Resources.ScalingMediumButton;

            // Number format settings
            NumberFormatList = new ObservableCollection<string>
            {
                Wpf.Properties.Resources.DynamoViewSettingMenuNumber0,
                Wpf.Properties.Resources.DynamoViewSettingMenuNumber00,
                Wpf.Properties.Resources.DynamoViewSettingMenuNumber000,
                Wpf.Properties.Resources.DynamoViewSettingMenuNumber0000,
                Wpf.Properties.Resources.DynamoViewSettingMenuNumber00000
            };
            SelectedNumberFormat = preferenceSettings.NumberFormat;

            runSettingsIsChecked = preferenceSettings.DefaultRunType;
            RunPreviewIsChecked = preferenceSettings.ShowRunPreview;

            //By Default the warning state of the Visual Settings tab (Group Styles section) will be disabled
            isWarningEnabled = false;

            // Initialize group styles with default and non-default GroupStyleItems
            StyleItemsList = GroupStyleItem.DefaultGroupStyleItems.AddRange(preferenceSettings.GroupStyleItemsList.Where(style => style.IsDefault != true)).ToObservableCollection();

            //When pressing the "Add Style" button some controls will be shown with some values by default so later they can be populated by the user
            AddStyleControl = new StyleItem() { Name = string.Empty, HexColorString = GetRandomHexStringColor() };

            //This piece of code will populate all the description text for the RadioButtons in the Geometry Scaling section.
            optionsGeometryScale = new GeometryScalingOptions();

            UpdateGeoScaleRadioButtonSelected(dynamoViewModel.ScaleFactorLog);

            optionsGeometryScale.DescriptionScaleRange = new ObservableCollection<string>();
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Small].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Small].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Medium].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Medium].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.Large].Item2,
                                                                                              scaleRanges[GeometryScaleSize.Large].Item3));
            optionsGeometryScale.DescriptionScaleRange.Add(string.Format(Res.ChangeScaleFactorPromptDescriptionContent, scaleRanges[GeometryScaleSize.ExtraLarge].Item2,
                                                                                              scaleRanges[GeometryScaleSize.ExtraLarge].Item3));

            SavedChangesLabel = string.Empty;
            SavedChangesTooltip = string.Empty;

            // Add tabs
            preferencesTabs = new Dictionary<string, TabSettings>();
            preferencesTabs.Add("General", new TabSettings() { Name = "General", ExpanderActive = string.Empty });
            preferencesTabs.Add("Features",new TabSettings() { Name = "Features", ExpanderActive = string.Empty });
            preferencesTabs.Add("VisualSettings",new TabSettings() { Name = "VisualSettings", ExpanderActive = string.Empty });
            preferencesTabs.Add("Package Manager", new TabSettings() { Name = "Package Manager", ExpanderActive = string.Empty });

            //create a packagePathsViewModel we'll use to interact with the package search paths list.
            var loadPackagesParams = new LoadPackageParams
            {
                Preferences = preferenceSettings
            };
            var customNodeManager = dynamoViewModel.Model.CustomNodeManager;
            var packageLoader = dynamoViewModel.Model.GetPackageManagerExtension()?.PackageLoader;            
            PackagePathsViewModel = new PackagePathViewModel(packageLoader, loadPackagesParams, customNodeManager);
            TrustedPathsViewModel = new TrustedPathViewModel(this.preferenceSettings, this.dynamoViewModel?.Model?.Logger);

            WorkspaceEvents.WorkspaceSettingsChanged += PreferencesViewModel_WorkspaceSettingsChanged;

            PropertyChanged += Model_PropertyChanged;

        }

        /// <summary>
        /// This method will be executed every time the WorkspaceModel.ScaleFactor value is updated.
        /// </summary>
        /// <param name="args"></param>
        private void PreferencesViewModel_WorkspaceSettingsChanged(WorkspacesSettingsChangedEventArgs args)
        {
            //The WorkspaceSettingsChanged event is refering to the double ScaleFactor, then we need to make the conversion to Logarithm scale factor ScaleFactorLog       
            UpdateGeoScaleRadioButtonSelected(Convert.ToInt32(Math.Log10(args.ScaleFactor)));
        }

        /// <summary>
        /// This method will update the currently selected Radio Button in the Geometry Scaling section.
        /// </summary>
        /// <param name="scaleFactor"></param>
        private void UpdateGeoScaleRadioButtonSelected(int scaleFactor)
        {
            ScaleSize = (GeometryScaleSize)GeometryScalingOptions.ConvertScaleFactorToUI(scaleFactor);
        }

        /// <summary>
        /// Called from DynamoViewModel::UnsubscribeAllEvents()
        /// </summary>
        internal virtual void UnsubscribeAllEvents()
        {
            PropertyChanged -= Model_PropertyChanged;
            WorkspaceEvents.WorkspaceSettingsChanged -= PreferencesViewModel_WorkspaceSettingsChanged;
            PythonEngineManager.Instance.AvailableEngines.CollectionChanged -= PythonEnginesChanged;
        }

        /// <summary>
        /// Listen for changes to the custom package paths and update package paths for install accordingly
        /// </summary>
        private void PackagePathsViewModel_RootLocations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // New path was added
                    var newPath = e.NewItems[0] as string;
                    PackagePathsForInstall.Add(newPath);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    // Path was removed
                    var removedPath = e.OldItems[0] as string;
                    var updateSelection = SelectedPackagePathForInstall == removedPath;
                    if (PackagePathsForInstall.Remove(removedPath) && updateSelection && PackagePathsForInstall.Count > 0)
                    {
                        // Path selected was removed
                        // Select first path in list
                        SelectedPackagePathForInstall = PackagePathsForInstall[0];
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    // Path was updated
                    newPath = e.NewItems[0] as string;
                    removedPath = e.OldItems[0] as string;
                    updateSelection = SelectedPackagePathForInstall == removedPath;
                    var index = PackagePathsForInstall.IndexOf(removedPath);
                    if (index != -1)
                    {
                        PackagePathsForInstall[index] = newPath;
                    }

                    if (updateSelection)
                    {
                        // Update selection of the updated path was selected
                        SelectedPackagePathForInstall = newPath;
                    }
                    break;
                default:
                    throw new NotSupportedException("Operation not supported");
            }
        }

        /// <summary>
        /// Store selection to preferences
        /// </summary>
        internal void CommitPackagePathsForInstall()
        {
            PackagePathsViewModel.RootLocations.CollectionChanged -= PackagePathsViewModel_RootLocations_CollectionChanged;
            preferenceSettings.SelectedPackagePathForInstall = SelectedPackagePathForInstall;
        }

        /// <summary>
        /// Force reload of paths and get current selection from preferences
        /// </summary>
        internal void InitPackagePathsForInstall()
        {
            PackagePathsForInstall = null;
            SelectedPackagePathForInstall = preferenceSettings.SelectedPackagePathForInstall;
            PackagePathsViewModel.RootLocations.CollectionChanged += PackagePathsViewModel_RootLocations_CollectionChanged;
        }

        /// <summary>
        /// Init all package filters
        /// </summary>
        internal void InitPackageListFilters()
        {
            installedPackagesViewModel.PopulateFilters();
        }

        /// <summary>
        /// Listen for the PropertyChanged event and updates the saved changes label accordingly
        /// </summary>
        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string description = string.Empty;

            // C# does not support going through all cases when one of the case is true
            switch (e.PropertyName)
            {
                case nameof(SelectedLanguage):
                    // Do nothing for now
                    break;
                case nameof(SelectedFontSize):
                    // Do nothing for now
                    break;
                case nameof(SelectedNumberFormat):
                    description = Resources.ResourceManager.GetString(nameof(Res.DynamoViewSettingMenuNumberFormat), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(SelectedPackagePathForInstall):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewSelectedPackagePathForDownload), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableBuiltInPackages):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewDisableBuiltInPackages), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableCustomPackages):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewDisableCustomPackages), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(RunSettingsIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewRunSettingsLabel), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(RunPreviewIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.DynamoViewSettingShowRunPreview), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(StyleItemsList):
                    // Do nothing for now
                    break;
                case nameof(OptionsGeometryScale):
                    description = Resources.ResourceManager.GetString(nameof(Res.DynamoViewSettingsMenuChangeScaleFactor), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowEdges):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingShowEdges), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(IsolateSelectedGeometry):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingsIsolateSelectedGeo), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(TessellationDivisions):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewVisualSettingsRenderPrecision), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(SelectedPythonEngine):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewDefaultPythonEngine), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(HideIronPythonAlertsIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewIsIronPythonDialogDisabled), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowWhitespaceIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewShowWhitespaceInPythonEditor), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(NodeAutocompleteIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewEnableNodeAutoComplete), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(EnableTSplineIsChecked):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewEnableTSplineNodes), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowPreviewBubbles):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewShowPreviewBubbles), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(ShowCodeBlockLineNumber):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewShowCodeBlockNodeLineNumber), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                case nameof(DisableTrustWarnings):
                    description = Resources.ResourceManager.GetString(nameof(Res.PreferencesViewTrustWarningHeader), System.Globalization.CultureInfo.InvariantCulture);
                    goto default;
                default:
                    if (!string.IsNullOrEmpty(description))
                    {
                        // Log switch on each setting and use description equals to label name
                        Dynamo.Logging.Analytics.TrackEvent(
                            Actions.Switch,
                            Categories.Preferences,
                            description);
                        UpdateSavedChangesLabel();
                    }
                    break;
            }
        }

        /// <summary>
        /// Updates the contents to display by the SavedChanges label and its tooltip
        /// </summary>
        internal void UpdateSavedChangesLabel()
        {
            SavedChangesLabel = Res.PreferencesViewSavedChangesLabel;
            //Sets the last saved time in the en-US format
            SavedChangesTooltip = Res.PreferencesViewSavedChangesTooltip + " " + DateTime.Now.ToString(@"HH:mm");
        }

        /// <summary>
        /// This method will remove the current Style selected from the Styles list by name
        /// </summary>
        /// <param name="styleName"></param>
        internal void RemoveStyleEntry(string styleName)
        {
            GroupStyleItem itemToRemovePreferences = preferenceSettings.GroupStyleItemsList.FirstOrDefault(x => x.Name.Equals(styleName));
            preferenceSettings.GroupStyleItemsList.Remove(itemToRemovePreferences);
            RaisePropertyChanged(nameof(StyleItemsList));
            UpdateSavedChangesLabel();
        }

        /// <summary>
        /// This method will check if the name of Style that is being created already exists in the Styles list
        /// </summary>
        /// <param name="item">target style item to check</param>
        /// <returns></returns>
        internal bool IsStyleNameValid(StyleItem item)
        {
            return StyleItemsList.Where(x => x.Name.Equals(item.Name)).Any();
        }

        /// <summary>
        /// This method will check if the name of Style that is being created already exists in the Styles list
        /// </summary>
        /// <param name="item">target style to be checked</param>
        /// <returns></returns>
        internal bool ValidateStyleGuid(StyleItem item)
        {
            return StyleItemsList.Where(x => x.Name.Equals(item.Name)).Any();
        }

        /// <summary>
        /// This method will remove a specific style control from the Styles list
        /// </summary>
        internal void ResetAddStyleControl()
        {
            IsEnabledAddStyleButton = true;
            IsSaveButtonEnabled = true;
            AddStyleControl.Name = String.Empty;
            AddStyleControl.HexColorString = GetRandomHexStringColor();
            IsWarningEnabled = false;
            IsVisibleAddStyleBorder = false;          
        }

        /// <summary>
        /// This method will enable the warning icon next to the GroupName TextBox and other buttons needed
        /// </summary>
        /// <param name="warningMessage">Message that will be displayed when the mouse is over the warning</param>
        internal void EnableGroupStyleWarningState(string warningMessage)
        {
            CurrentWarningMessage = warningMessage;
            IsWarningEnabled = true;
            IsSaveButtonEnabled = false;
        }

        /// <summary>
        /// This Method will generate a random color string in a Hexadecimal format
        /// </summary>
        /// <returns></returns>
        internal string GetRandomHexStringColor()
        {
            Random r = new Random();
            Color color = Color.FromArgb(255, (byte)r.Next(), (byte)r.Next(), (byte)r.Next());
            return ColorTranslator.ToHtml(color).Replace("#", "");
        }

        private void PythonEnginesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddPythonEnginesOptions();
            }
        }
    }

    /// <summary>
    /// This class will contain the Enum value and the corresponding description for each radio button in the Visual Settings -> Geometry Scaling section
    /// </summary>
    public class GeometryScalingOptions
    {
        //The Enum values can be Small, Medium, Large or Extra Large
        [Obsolete("This property is deprecated and will be removed in a future version of Dynamo")]
        public GeometryScaleSize EnumProperty { get; set; }

        /// <summary>
        /// This property will contain the description of each of the radio buttons in the Visual Settings -> Geometry Scaling section
        /// </summary>
        public ObservableCollection<string> DescriptionScaleRange { get; set; }

        /// <summary>
        /// This method is used to convert a index (representing a RadioButton in the UI) to a ScaleFactor
        /// </summary>
        /// <param name="index">This value is the index for the RadioButton in the Geometry Scaling section. 
        /// It can have the values:
        ///   0 - Small
        ///   1 - Medium (Default)
        ///   2 - Large
        ///   3 - Extra Large
        /// </param>
        /// <returns>The Scale Factor (-2, 0, 2, 4)</returns>
        public static int ConvertUIToScaleFactor (int index)
        {
            return (index - 1) * 2;
        }

        /// <summary>
        /// This method is used to conver a Scale Factor to a index so we can Check a Radio Button in the UI
        /// </summary>
        /// <param name="scaleValue">This values is the Scale that we need to convert to index (representing a RadioButton)
        /// It can have the values:
        /// - 2 - Small
        ///   0 - Medium (Default)
        ///   2 - Large
        ///   4 - Extra Large
        /// </param>
        /// <returns>The radiobutton index (0,1,2,3)</returns>
        public static int ConvertScaleFactorToUI(int scaleValue)
        {
           return (scaleValue / 2) + 1;
        }
    }

    /// <summary>
    /// This class represent a Tab and is used for store just one Expander info(due that just one Expander can be expanded at one time)
    /// </summary>
    public class TabSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Tab Name (e.g. Features or Visual Settings)
        /// </summary>
        public string Name;
        private string expanderActive;

        /// <summary>
        /// This property hold the name for the current Expander expanded
        /// </summary>
        public string ExpanderActive
        {
            get
            {
                return expanderActive;
            }
            set
            {
                if(value != null)
                {
                    expanderActive = value;
                    OnPropertyChanged(nameof(ExpanderActive));
                }
                else
                {
                    expanderActive = string.Empty;
                }
            }
        }
    }
}
