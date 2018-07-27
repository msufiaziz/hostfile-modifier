using System;
using System.IO;
using System.Linq;
using System.Text;
using HostFile.Libs.Contracts.DataContracts;
using HostFile.Libs.Contracts.Interfaces;
using HostFile.Libs.Updater.DataFiles;
using HostFile.Libs.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HostFile.Tests.UnitTests
{
    [TestClass]
    public class FileUpdaterUnitTests
    {
        const string EnvironmentName = "SIT-G4";
        const string AppSettingPath = "appSettings.json";
        const string CleanHostFilePath = @"Data\hostfile_clean.txt";
        const string DirtyHostFilePath = @"Data\hostfile_dirty.txt";
        const string DataFilePath = @"Data\data.txt";
        
        IFileUpdater _fileUpdater;

        [TestInitialize]
        public void Initialize()
        {
            var appSetting = AppSetting.GetAppSettings(AppSettingPath);
            var mockLogger = new Mock<ILogger>();

            _fileUpdater = new FileUpdater(appSetting, mockLogger.Object);
        }

        [TestMethod]
        public void FileUpdaterUnitTests_IsTargetFileDirty_False()
        {
            bool isDirty = _fileUpdater.IsTargetFileDirty(CleanHostFilePath);

            Assert.IsFalse(isDirty);
        }

        [TestMethod]
        public void FileUpdaterUnitTests_IsTargetFileDirty_True()
        {
            bool isDirty = _fileUpdater.IsTargetFileDirty(DirtyHostFilePath);

            Assert.IsTrue(isDirty);
        }

        [TestMethod]
        public void FileUpdaterUnitTests_GetEnvironment_Exists()
        {
            string environment = _fileUpdater.GetExistingTargetFileEnvironment(DirtyHostFilePath);

            Assert.AreEqual(EnvironmentName, environment);
        }

        [TestMethod]
        public void FileUpdaterUnitTests_GetEnvironment_NotExists()
        {
            string environment = _fileUpdater.GetExistingTargetFileEnvironment(CleanHostFilePath);

            Assert.AreNotEqual(EnvironmentName, environment);
        }

        [TestMethod]
        public void FileUpdateUnitTests_UpdateTargetFile()
        {
            string testFile = CleanHostFilePath + ".test";
            string backupTestFile = testFile + ".bak";
            File.Copy(CleanHostFilePath, testFile);

            _fileUpdater.UpdateTargetFile(EnvironmentName, testFile);

            Assert.IsTrue(File.Exists(backupTestFile));

            // Clean up.
            File.Delete(testFile);
            File.Delete(backupTestFile);
        }

        [TestMethod]
        public void FileUpdateUnitTests_RestoreTargetFile()
        {
            string testFile = DirtyHostFilePath + ".test";
            File.Copy(DirtyHostFilePath, testFile);

            bool result = _fileUpdater.RestoreTargetFile(testFile);
            var content = File.ReadAllText(testFile);

            Assert.IsTrue(result);
            Assert.IsTrue(!content.Contains("hostname6"));

            // Clean up.
            File.Delete(testFile);
        }
    }
}
