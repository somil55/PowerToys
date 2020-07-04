﻿using System.Windows.Input;
using NUnit.Framework;
using Wox.Plugin;
using Wox.ViewModel;
using System.Runtime.CompilerServices;

namespace Wox.Test
{
    [TestFixture]
    public class Wox
    {
        // A Dummy class to test that OnPropertyChanged() is called while we set the variable
        public class DummyTestClass : BaseModel
        {
            public bool isFunctionCalled = false;
            private ICommand _item;

            public ICommand Item
            {
                get
                {
                    return this._item;
                }

                set
                {
                    if (value != this._item)
                    {
                        this._item = value;
                        OnPropertyChanged();
                    }
                }
            }

            // Overriding the OnPropertyChanged() function to test if it is being called
            protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                isFunctionCalled = true;
            }
        }


        [Test]
        public void AnyVariable_MustCallOnPropertyChanged_WhenSet()
        {
            // Arrange
            DummyTestClass testClass = new DummyTestClass();

            // Act
            testClass.Item = new RelayCommand(null);

            // Assert
            Assert.IsTrue(testClass.isFunctionCalled);

        }

    }
}
