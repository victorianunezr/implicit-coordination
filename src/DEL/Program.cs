using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ImplicitCoordination.DEL;
using ImplicitCoordination.DEL.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImplicitCoordination
{
    class Program
    {
        static void Main(string[] args)
        {
            // Testing accessibility relation graph
            var graph = new HashSet<(World, World)>();

            var w = new World();
            var v = new World();
            graph.Add((w, v));
            var edge = (w, v);
            Assert.IsTrue(graph.Contains(edge));
            graph.Add(edge);

            graph.Remove(edge);

            graph.Remove(edge);
            // Assert
            //Assert.IsTrue(w.valuation[1]);
            Console.WriteLine("hello");
        }
    }
}
