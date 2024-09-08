﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NuExt.System.Tests
{
    public class NotifyPropertyChangedTests
    {
        [Test]
        public void PropertyChangedTTest()
        {
            int cnt = 0;
            var c = new TestClass();
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                cnt++;
            }
            c.PropertyChanged += OnPropertyChanged;
            c.Uint16 = 1;
            c.Uint16 = 2;
            c.Uint16++;

            Assert.That(cnt, Is.EqualTo(3));
            Assert.Pass();
        }

        private class TestClass : NotifyPropertyChanged
        {
            private ushort _uint16;
            public ushort Uint16
            {
                get => _uint16;
                set => SetProperty(ref _uint16, value);
            }
        }
    }
}