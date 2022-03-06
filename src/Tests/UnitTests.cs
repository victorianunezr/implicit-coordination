using ImplicitCoordination.DEL;
using ImplicitCoordination.DEL.utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ImplicitCoordination.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //World w = new World(new BitVector32(0b110));

            //// Assert
            //Assert.IsTrue(w.valuation[1]);
            //Assert.IsFalse(w.valuation[0]);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void BitArrayTest()
        {
            var bb = new BitArray { data = 6 };

            Assert.IsTrue(bb.GetValue(1));
            Assert.IsFalse(bb.GetValue(0));
            Assert.IsTrue(bb.GetValue(2));

            bb.SetValue(4, true);
            Assert.IsTrue(bb.GetValue(4));

            bb.SetValue(4, false);
            Assert.IsFalse(bb.GetValue(4));
        }

        [TestMethod]
        public void PropositionalLogicFormulaEvaluationInWorldTest()
        {
            // Evaluating formulas in a world. The setup is:
            // p = false (idx = 0), q = true (idx = 1), r = true (idx = 1)

            var p = new Proposition(name: "p", arity: 0);
            var q = new Proposition(name: "q", arity: 0);
            var r = new Proposition(name: "r", arity: 0);

            var valuation = new BitArray { data = 6 };
            World w = new World(valuation);

            var atomP = Formula.Atom(p);
            var atomQ = Formula.Atom(q);
            var atomR = Formula.Atom(r);

            Assert.IsFalse(atomP.Evaluate(s: null, w: w));
            Assert.IsTrue(atomQ.Evaluate(s: null, w: w));
            Assert.IsTrue(atomR.Evaluate(s: null, w: w));

            var f1 = Formula.And(atomP, atomQ);
            // P AND Q is false
            Assert.IsFalse(f1.Evaluate(s: null, w: w));

            var f2 = Formula.Or(atomP, atomQ);
            // P OR Q is false
            Assert.IsTrue(f2.Evaluate(s: null, w: w));

            var f3 = Formula.Not(atomP);
            // NOT P is true
            Assert.IsTrue(f3.Evaluate(s: null, w: w));

            var f4 = Formula.And(atomQ, atomR);
            // Q AND R is true
            Assert.IsTrue(f4.Evaluate(s: null, w: w));

            var f5 = Formula.And(Formula.Not(atomP), atomR);
            // (NOT P) AND R is true
            Assert.IsTrue(f5.Evaluate(s: null, w: w));

            var f6 = Formula.Not(Formula.And(atomP, atomQ));
            // NOT (P AND Q) is true
            Assert.IsTrue(f6.Evaluate(s: null, w: w));
        }
    }
}
