﻿using Dynamo.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Threading;

namespace Dynamo.Tests.Logging
{
    [TestFixture]
     class FeatureFlagTests
    {
        [SetUp]
        public void Setup()
        {
            eventCounter = 0;
            log = string.Empty;
        }

        int eventCounter=0;
        string log = string.Empty;
        [Test]
        public void FeatureFlagsShouldReturnRealDataAfterCache()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.CacheAllFlags();
            Assert.IsTrue(testflagsManager.CheckFeatureFlag<bool>("TestFlag1", false));
        }

        [Test]
        public void FeatureFlagsShouldReturnDefaultDataBeforeCache()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            Assert.False(testflagsManager.CheckFeatureFlag<bool>("TestFlag1", false));

        }
        [Test]
        public void FeatureFlagsShouldReturnValidStringData()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.CacheAllFlags();
            Assert.AreEqual("I am a string", testflagsManager.CheckFeatureFlag<string>("TestFlag2", "NA"));

        }

        [Test]
        public void FeatureFlagsShouldTriggerEventAfterCacheFlags()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved += DynamoFeatureFlagsManager_FlagsRetrieved;
            testflagsManager.CacheAllFlags();

            DynamoUtilities.DynamoFeatureFlagsManager.FlagsRetrieved -= DynamoFeatureFlagsManager_FlagsRetrieved;
            Assert.AreEqual(1, eventCounter);
        }
        [Test]
        public void FeatureFlagsShouldMessageLoggedShouldContainAllLogs()
        {
            var testflagsManager = new DynamoUtilities.DynamoFeatureFlagsManager("testkey", new SynchronizationContext(), true);
            testflagsManager.MessageLogged += TestflagsManager_MessageLogged;
            testflagsManager.CacheAllFlags();

            testflagsManager.MessageLogged -= TestflagsManager_MessageLogged;
            Assert.AreEqual("LD startup: testmode true, no LD connection. LD startup time: 0 <<<<<InitDone>>>>>feature flag exe starting<<<<<Sod>>>>>{\"TestFlag1\":true,\"TestFlag2\":\"I am a string\",\"graphics-primitive-instancing\":true}<<<<<Eod>>>>>", log);
        }

        private void DynamoFeatureFlagsManager_FlagsRetrieved()
        {
            eventCounter++;
        }
        private void TestflagsManager_MessageLogged(string message)
        {
            log = log + message;
        }

    }
}
