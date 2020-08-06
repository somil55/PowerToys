using Microsoft.Plugin.Program.Programs;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using Wox.Infrastructure.Storage;
using Wox.Plugin;

namespace Microsoft.Plugin.Program.UnitTests
{
    using Win32 = Microsoft.Plugin.Program.Programs.Win32;

    [TestFixture]
    class MainTests
    {
        [Test]
        public void Contains_ShouldReturnTrue_WhenListIsInitializedWithItem()
        {
            //Arrange
            var maxScore = 4;
            var plugin = new Main();
            var UWPProgramA = new Mock<UWP.Application>();
            var UWPProgramB = new Mock<UWP.Application>();
            var Win32ProgramA = new Mock<Win32>();
            var PackageCatalog = new Mock<IPackageCatalog>();
            var UWPStorage = new Mock<IStorage<IList<UWP.Application>>>();
            UWPProgramA.Setup(x => x.Result(It.IsAny<string>(), It.IsAny<IPublicAPI>())).Returns(new Result() { Score = maxScore});
            UWPProgramB.Setup(x => x.Result(It.IsAny<string>(), It.IsAny<IPublicAPI>())).Returns(new Result() { Score = (int)(maxScore * .8)});
            Win32ProgramA.Setup(x => x.Result(It.IsAny<string>(), It.IsAny<IPublicAPI>())).Returns(new Result() { Score = (int)(maxScore * .7) });


            plugin._packageRepository = new Program.Storage.PackageRepository(PackageCatalog.Object, UWPStorage.Object);
            



            //Act
            //var x = plugin.Query(string.Empty);

            //Assert

        }
    }
}
