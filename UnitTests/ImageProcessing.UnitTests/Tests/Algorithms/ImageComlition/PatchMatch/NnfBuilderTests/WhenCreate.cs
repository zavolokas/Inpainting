//#if DEBUG
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using Zavolokas.ImageProcessing.Algorithms;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PatchMatch;

//namespace Zavolokas.ImageProcessing.UnitTests.NnfBuilderTests
//{
//    [TestFixture]
//    public class WhenCreate
//    {
//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void ShouldThrowExceptionWhenParametrsNull()
//        {
//            INnfBuilder builder = new NnfBuilder(null, null);
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void ShouldThrowExceptionWhenExecControlNull()
//        {
//            PatchMatchSettings setting = new PatchMatchSettings();
//            INnfBuilder builder = new NnfBuilder(setting, null);
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void ShouldThrowExceptionWhenSettingsNull()
//        {
//            var execControl = new ProcessExecutionControl();
//            INnfBuilder builder = new NnfBuilder(null, execControl);
//        }

//        //[Test]
//        //public void ShouldSetSettings()
//        //{
//        //    var setting = new PatchMatchSettings();
//        //    var execControl = new ProcessExecutionControl();
//        //    INnfBuilder builder = new NnfBuilder(setting, execControl);
//        //    Assert.That(builder.Settings, Is.EqualTo(setting));
//        //    Assert.That(builder.ExecutionControl, Is.EqualTo(execControl));
//        //}
//    }
//}
//#endif