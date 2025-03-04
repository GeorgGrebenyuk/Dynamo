﻿using System.Collections.Generic;
using System.IO;
using Dynamo.Configuration;
using Dynamo.Models;
using NUnit.Framework;
using System.Linq;
using System.Xml;
using System;
using Dynamo.Interfaces;
using System.Reflection;

namespace Dynamo.Tests.Configuration
{
    [TestFixture]
    class PreferenceSettingsTests : UnitTestBase
    {
        [Test]
        [Category("UnitTests")]
        public void TestLoad()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));

            var settings = PreferenceSettings.Load(settingsFilePath);

            Assert.IsNotNull(settings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestGetPythonTemplateFilePath()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-PythonTemplate-initial.xml");
            string initialPyFilePath = Path.Combine(settingDirectory, @"PythonTemplate-initial.py");

            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            Assert.IsTrue(File.Exists(initialPyFilePath));

            PreferenceSettings.Load(settingsFilePath);

            string pythonTemplateFilePath = Path.Combine(settingDirectory, PreferenceSettings.GetPythonTemplateFilePath());

            Assert.AreEqual(pythonTemplateFilePath, initialPyFilePath);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSettingsSerialization()
        {
            string tempPath = System.IO.Path.GetTempPath();
            tempPath = Path.Combine(tempPath, "userPreference.xml");

            PreferenceSettings settings = new PreferenceSettings();

            // Assert defaults
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), true);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, true);
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, false);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);

            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same when saved with defaults
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), true);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, true);
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, false);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, false);
            Assert.AreEqual(settings.EnableNodeAutoComplete, true);
            Assert.AreEqual(settings.EnableNotificationCenter, true);
            Assert.AreEqual(settings.DefaultPythonEngine, string.Empty);
            Assert.AreEqual(settings.MaxNumRecentFiles, PreferenceSettings.DefaultMaxNumRecentFiles);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 0);
            Assert.AreEqual(settings.DefaultRunType, RunType.Automatic);

            // Change setting values
            settings.SetIsBackgroundPreviewActive("MyBackgroundPreview", false);
            settings.ShowCodeBlockLineNumber = false;
            settings.IsIronPythonDialogDisabled = true;
            settings.ShowTabsAndSpacesInScriptEditor = true;
            settings.DefaultPythonEngine = "CP3";
            settings.MaxNumRecentFiles = 24;
            settings.EnableNodeAutoComplete = false;
            settings.EnableNotificationCenter = false;
            settings.DefaultRunType = RunType.Manual;
            settings.ViewExtensionSettings.Add(new ViewExtensionSettings()
            {
                Name = "MyExtension",
                UniqueId = "1234",
                DisplayMode = ViewExtensionDisplayMode.FloatingWindow,
                WindowSettings = new WindowSettings()
                {
                    Left = 123,
                    Top = 456,
                    Height = 321,
                    Width = 654,
                    Status = WindowStatus.Maximized
                }
            });
            settings.GroupStyleItemsList.Add(new GroupStyleItem 
            {
                Name = "TestGroup", 
                HexColorString = "000000" 
            });

            // Save
            settings.Save(tempPath);
            settings = PreferenceSettings.Load(tempPath);

            // Assert deserialized values are same as last changed
            Assert.AreEqual(settings.GetIsBackgroundPreviewActive("MyBackgroundPreview"), false);
            Assert.AreEqual(settings.ShowCodeBlockLineNumber, false);
            Assert.AreEqual(settings.IsIronPythonDialogDisabled, true);
            Assert.AreEqual(settings.ShowTabsAndSpacesInScriptEditor, true);
            Assert.AreEqual(settings.DefaultPythonEngine, "CP3");
            Assert.AreEqual(settings.MaxNumRecentFiles, 24);
            Assert.AreEqual(settings.EnableNodeAutoComplete, false);
            Assert.AreEqual(settings.EnableNotificationCenter, false);
            Assert.AreEqual(settings.ViewExtensionSettings.Count, 1);
            var extensionSettings = settings.ViewExtensionSettings[0];
            Assert.AreEqual(settings.DefaultRunType, RunType.Manual);
            Assert.AreEqual(extensionSettings.Name, "MyExtension");
            Assert.AreEqual(extensionSettings.UniqueId, "1234");
            Assert.AreEqual(extensionSettings.DisplayMode, ViewExtensionDisplayMode.FloatingWindow);
            Assert.IsNotNull(extensionSettings.WindowSettings);
            var windowSettings = extensionSettings.WindowSettings;
            Assert.AreEqual(windowSettings.Left, 123);
            Assert.AreEqual(windowSettings.Top, 456);
            Assert.AreEqual(windowSettings.Height, 321);
            Assert.AreEqual(windowSettings.Width, 654);
            Assert.AreEqual(windowSettings.Status, WindowStatus.Maximized);
            // Load function will only deserialize the customized style
            Assert.AreEqual(settings.GroupStyleItemsList.Count, 1);
            var styleItemsList = settings.GroupStyleItemsList[0];
            Assert.AreEqual(styleItemsList.Name, "TestGroup");
            Assert.AreEqual(styleItemsList.HexColorString, "000000");
        }

        [Test]
        [Category("UnitTests")]
        public void TestMigrateStdLibTokenToBuiltInToken()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string settingsFilePath = Path.Combine(settingDirectory, "DynamoSettings-stdlibtoken.xml");
            Assert.IsTrue(File.ReadAllText(settingsFilePath).Contains(DynamoModel.StandardLibraryToken));
            // Assert files required for test exist
            Assert.IsTrue(File.Exists(settingsFilePath));
            var settings = PreferenceSettings.Load(settingsFilePath);

            var token = settings.CustomPackageFolders[1];

            Assert.AreEqual(DynamoModel.BuiltInPackagesToken,token);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSerializationDisableTrustWarnings()
        {
            //create new prefs
            var prefs = new PreferenceSettings();
            //assert default.
            Assert.IsFalse(prefs.DisableTrustWarnings);
            prefs.SetTrustWarningsDisabled(true);
            Assert.True(prefs.DisableTrustWarnings);
            //save
            var tempPath = GetNewFileNameOnTempPath(".xml");
            prefs.Save(tempPath);

            //load
            var settingsLoaded = PreferenceSettings.Load(tempPath);
            Assert.IsTrue(settingsLoaded.DisableTrustWarnings);
        }

        [Test]
        [Category("UnitTests")]
        public void TestSerializationTrustedLocations()
        {
            //create new prefs
            var prefs = new PreferenceSettings();
            //assert default.
            Assert.AreEqual(0, prefs.TrustedLocations.Count);
            prefs.SetTrustedLocations(new List<string>() { Path.GetTempPath() });
            Assert.AreEqual(1, prefs.TrustedLocations.Count);
            //save
            var tempPath = GetNewFileNameOnTempPath(".xml");
            prefs.Save(tempPath);

            //load
            var settingsLoaded = PreferenceSettings.Load(tempPath);
            Assert.AreEqual(1, settingsLoaded.TrustedLocations.Count);

            Assert.IsTrue(settingsLoaded.IsTrustedLocation(Path.GetTempPath()));
        }        

        /// <summary>
        /// Struct to support the comparison between two PreferenceSettings instances
        /// </summary>
        struct PreferencesComparison
        {
            public List<string> Properties { get; set; }
            public List<String> SamePropertyValues { get; set; }
            public List<String> DifferentPropertyValues { get; set; }          
        }

        /// <summary>
        /// Compare the property values of two PreferenceSettings instances
        /// </summary>
        /// <param name="defaultSettings"></param>
        /// <param name="newGeneralSettings"></param>
        /// <returns>3 List of Properties, the properties that have been evaluated, the properties that have the same values and the properties that have different values</returns>
        PreferencesComparison comparePrefenceSettings(PreferenceSettings defaultSettings, PreferenceSettings newGeneralSettings)
        {
            var result = new PreferencesComparison();
            var propertiesWithSameValue = new List<string>();
            var propertiesWithDifferentValue = new List<string>();
            var evaluatedProperties = new List<string>();

            var destinationProperties = defaultSettings.GetType().GetProperties();

            foreach (var destinationPi in destinationProperties)
            {
                var sourcePi = newGeneralSettings.GetType().GetProperty(destinationPi.Name);

                if (destinationPi.GetCustomAttributes(typeof(System.ObsoleteAttribute), true).Length == 0 && !defaultSettings.StaticFields().ConvertAll(fieldName => fieldName.ToUpper()).Contains(destinationPi.Name.ToUpper()))
                {
                    evaluatedProperties.Add(destinationPi.Name);
                    var newValue = sourcePi.GetValue(newGeneralSettings, null);
                    var oldValue = destinationPi.GetValue(defaultSettings, null);

                    if (destinationPi.PropertyType == typeof(List<string>))
                    {
                        var newList = (List<string>)sourcePi.GetValue(newGeneralSettings, null);
                        var oldList = (List<string>)destinationPi.GetValue(defaultSettings, null);
                        if (newList.Except(oldList).ToList().Count == 0)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<GroupStyleItem>))
                    {
                        if (((List<GroupStyleItem>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<GroupStyleItem>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<ViewExtensionSettings>))
                    {
                        if (((List<ViewExtensionSettings>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<ViewExtensionSettings>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else if (destinationPi.PropertyType == typeof(List<BackgroundPreviewActiveState>))
                    {
                        if (((List<BackgroundPreviewActiveState>)sourcePi.GetValue(newGeneralSettings, null)).Count ==
                            ((List<BackgroundPreviewActiveState>)destinationPi.GetValue(defaultSettings, null)).Count)
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                    else
                    {
                        if (newValue?.ToString() == oldValue?.ToString())
                        {
                            propertiesWithSameValue.Add(destinationPi.Name);
                        }
                        else
                        {
                            propertiesWithDifferentValue.Add(destinationPi.Name);
                        }
                    }
                }
            }
            
            result.SamePropertyValues = propertiesWithSameValue;
            result.DifferentPropertyValues = propertiesWithDifferentValue;
            result.Properties = evaluatedProperties;
            return result;
        }

        [Test]
        [Category("UnitTests")]
        public void TestImportCopySettings()
        {
            string settingDirectory = Path.Combine(TestDirectory, "settings");
            string newSettingslFilePath = Path.Combine(settingDirectory, "DynamoSettings-NewSettings.xml");

            var defaultSettings = new PreferenceSettings();
            var newSettings = PreferenceSettings.Load(newSettingslFilePath);

            // validation
            bool newSettingsExist = File.Exists(newSettingslFilePath);
            var checkDifference = comparePrefenceSettings(defaultSettings, newSettings);
            int diffProps = checkDifference.DifferentPropertyValues.Count;
            int totProps = checkDifference.Properties.Count;
            string firstPropertyWithSameValue = checkDifference.Properties.Except(checkDifference.DifferentPropertyValues).ToList().FirstOrDefault();
            string defSettNumberFormat = defaultSettings.NumberFormat;
            string newSettNumberFormat = newSettings.NumberFormat;
            string failMessage = $"The file {newSettingslFilePath} exist: {newSettingsExist.ToString()} | DiffProps: {diffProps.ToString()} | TotProps: {totProps.ToString()} | Default Sett NumberFormat: {defSettNumberFormat} | New Sett NumberFormat: {newSettNumberFormat} | First Property with the same value {firstPropertyWithSameValue}";

            // checking if the new Setting are completely different from the Default
            Assert.IsTrue(checkDifference.DifferentPropertyValues.Count == checkDifference.Properties.Count, failMessage);

            newSettings.CopyProperties(defaultSettings);
            // Explicit copy
            defaultSettings.SetTrustWarningsDisabled(newSettings.DisableTrustWarnings);
            defaultSettings.SetTrustedLocations(newSettings.TrustedLocations);

            // checking if the default Setting instance has the same property values of the new one
            var checkEquality = comparePrefenceSettings(defaultSettings, newSettings);            
            Assert.IsTrue(checkEquality.SamePropertyValues.Count == checkEquality.Properties.Count);
        }
    }
}
