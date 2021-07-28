using System;
using System.Reflection;
using NUnit.Framework;

namespace ElectronFlex.Test
{
    class A
    {
        public static int Get<T>(int a) => a;
        public static int Get<T, T2>(int a) => a+1;
        public static int Get<T, T2, T3>(int a) => a+2;
    }    
    class B<T> {}
    class C<T1, T2> {}

    public class TestTypeExtension
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGetGenericType()
        {
            var type = TypeExtensions.ToType("C<B<A>, A>");
            Assert.AreEqual(typeof(C<B<A>, A>), type);
            
            type = TypeExtensions.ToType("Tuple<B<A>, A>");
            Assert.AreEqual(typeof(Tuple<B<A>, A>), type);
        }

        [Test]
        public void TestGetGenericMethod()
        {
            var type = typeof(A);
            var method = type.GetMethod("Get<int>", BindingFlags.Public | BindingFlags.Static, new object[] {100});
            var result = method.Invoke(null, new object[]{100});
            Assert.AreEqual(100, result);
            
            method = type.GetMethod("Get<int, int>", BindingFlags.Public | BindingFlags.Static, new object[] {100});
            result = method.Invoke(null, new object[]{100});
            Assert.AreEqual(101, result);
            
            method = type.GetMethod("Get<int, int, int>", BindingFlags.Public | BindingFlags.Static, new object[] {100});
            result = method.Invoke(null, new object[]{100});
            Assert.AreEqual(102, result);
        }
    }
}