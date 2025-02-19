using System;
using System.Collections.Generic;
using System.Linq;

namespace ImplicitCoordination.DEL
{
    public class Formula
    {
        private FormulaType type;
        private Predicate predicate;         // schematic
        private GroundPredicate groundPredicate;

        private Formula child;
        private Formula leftChild;
        private Formula rightChild;
        private ICollection<Formula> operands;
        private Agent agent;

        public FormulaType GetFormulaType()
        {
            return type;
        }

        /// <summary>
        // "Normal" Evaluate for ground formulas in a pointed world model (i.e. truth of a formula in a given world).
        /// </summary>
        public bool Evaluate(State s, World w)
        {
            switch (type)
            {
                case FormulaType.Top: return true;
                case FormulaType.Bottom: return false;

                case FormulaType.Atom:
                    // If groundPredicate is set, check it
                    if (groundPredicate != null)
                        return w.IsTrue(groundPredicate);
                    // If predicate is set, this is schematic => not handled here
                    return false;

                case FormulaType.Not:
                    return !child.Evaluate(s, w);
                case FormulaType.And:
                    return leftChild.Evaluate(s, w) && rightChild.Evaluate(s, w);
                case FormulaType.Or:
                    return leftChild.Evaluate(s, w) || rightChild.Evaluate(s, w);
                case FormulaType.Implies:
                    return !leftChild.Evaluate(s, w) || rightChild.Evaluate(s, w);

                case FormulaType.Disjunction:
                    foreach (var f in operands)
                        if (f.Evaluate(s, w)) return true;
                    return false;

                case FormulaType.Conjunction:
                    foreach (var f in operands)
                        if (!f.Evaluate(s, w)) return false;
                    return true;

                case FormulaType.Knows:
                    foreach (World v in s.accessibility.GetAccessibleWorlds(agent, w))
                        if (!child.Evaluate(s, v)) return false;
                    return true;

                case FormulaType.CommonKnow:
                    // not implemented
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Evaluates validity of formula in a model (state). Formula is valid if it is valid in every designated world
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool Evaluate(State s)
        {
            foreach (World w in s.designatedWorlds)
            {
                if (!Evaluate(s, w))
                {
                    return false;
                }
            }

            return true;
        }
        
        // Schematic-aware Evaluate that tries all variable assignments if needed
        public EvaluationResult EvaluateSchematic(State s, World w, IEnumerable<Object> allObjects)
        {
            // If there are no variables in this formula, do normal Evaluate
            if (!HasVariables(this))
                return new EvaluationResult { Satisfied = Evaluate(s, w), Assignment = new Dictionary<string, Object>() };

            // Otherwise, gather variables, attempt all assignments
            var parameters = new HashSet<Parameter>();
            CollectParameters(this, parameters);

            foreach (var assignment in GenerateAssignments(parameters.ToList(), allObjects.ToList()))
            {
                var groundFormula = SubstituteVariables(this, assignment);
                if (groundFormula.Evaluate(s, w))
                    return new EvaluationResult { Satisfied = true, Assignment = assignment };
            }
            return new EvaluationResult { Satisfied = false, Assignment = null };
        }

        // Helper to check if there's at least one variable in the formula
        private bool HasVariables(Formula f)
        {
            var parameters = new HashSet<Parameter>();
            CollectParameters(f, parameters);
            return parameters.Any();
        }

        private void CollectParameters(Formula f, HashSet<Parameter> parameters)
        {
            switch (f.type)
            {
                case FormulaType.Atom:
                    if (f.predicate != null)
                    {
                        foreach (var param in f.predicate.Parameters)
                        {
                            if (IsVariable(param.Name))
                                parameters.Add(param);
                        }
                    }
                    break;
                case FormulaType.Not:
                    CollectParameters(f.child, parameters);
                    break;
                case FormulaType.And:
                case FormulaType.Or:
                case FormulaType.Implies:
                    CollectParameters(f.leftChild, parameters);
                    CollectParameters(f.rightChild, parameters);
                    break;
                case FormulaType.Disjunction:
                case FormulaType.Conjunction:
                    foreach (var subFormula in f.operands)
                        CollectParameters(subFormula, parameters);
                    break;
                case FormulaType.Knows:
                case FormulaType.CommonKnow:
                    CollectParameters(f.child, parameters);
                    break;
            }
        }

        private bool IsVariable(string name)
        {
            return name.StartsWith("?");
        }

        // Generate all variable->object assignments (Cartesian product)
        private IEnumerable<Dictionary<string, Object>> GenerateAssignments(
            List<Parameter> parameters,
            List<Object> allObjs)
        {
            if (!parameters.Any())
            {
                yield return new Dictionary<string, Object>();
                yield break;
            }

            var firstParam = parameters[0];
            var restparameters = parameters.Skip(1).ToList();

            List<Object> candidateObjs;
            if (firstParam.Name.StartsWith("?"))
            {
                // If a type is specified and not "unknown", filter objects by type.
                if (!string.IsNullOrEmpty(firstParam.Type) && !firstParam.Type.Equals("unknown", StringComparison.OrdinalIgnoreCase))
                {
                    candidateObjs = allObjs
                        .Where(o => string.Equals(o.Type, firstParam.Type, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (!candidateObjs.Any())
                        candidateObjs = allObjs; // fallback if no object of the expected type exists
                }
                else
                {
                    candidateObjs = allObjs;
                }
            }
            else
            {
                // If not a variable, treat the parameter as a constant.
                candidateObjs = allObjs
                    .Where(o => string.Equals(o.Name, firstParam.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                if (!candidateObjs.Any())
                    candidateObjs = allObjs;
            }

            foreach (var obj in candidateObjs)
            {
                foreach (var partial in GenerateAssignments(restparameters, allObjs))
                {
                    var newMap = new Dictionary<string, Object>(partial);
                    if (firstParam.Name.StartsWith("?"))
                        newMap[firstParam.Name] = obj;
                    yield return newMap;
                }
            }
        }

        // Substitute variables in a formula => produce a ground formula
        private Formula SubstituteVariables(Formula original, Dictionary<string, Object> assignment)
        {
            switch (original.type)
            {
                case FormulaType.Atom:
                    if (original.groundPredicate != null)
                        // Already ground
                        return original;
                    if (original.predicate != null)
                    {
                        // Build a new ground predicate
                        var gpName = original.predicate.name;
                        var gpObjs = new List<Object>();
                        foreach (var p in original.predicate.Parameters)
                        {
                            if (IsVariable(p.Name) && assignment.ContainsKey(p.Name))
                                gpObjs.Add(assignment[p.Name]);
                            else
                                gpObjs.Add(new Object(p.Name, p.Type));
                        }
                        return new Formula {
                            type = FormulaType.Atom,
                            groundPredicate = new GroundPredicate(gpName, gpObjs)
                        };
                    }
                    return original;

                case FormulaType.Not:
                    return new Formula {
                        type = FormulaType.Not,
                        child = SubstituteVariables(original.child, assignment)
                    };
                case FormulaType.And:
                case FormulaType.Or:
                case FormulaType.Implies:
                    return new Formula {
                        type = original.type,
                        leftChild = SubstituteVariables(original.leftChild, assignment),
                        rightChild = SubstituteVariables(original.rightChild, assignment)
                    };
                case FormulaType.Disjunction:
                case FormulaType.Conjunction:
                    var subs = new List<Formula>();
                    foreach (var sf in original.operands)
                        subs.Add(SubstituteVariables(sf, assignment));
                    return new Formula {
                        type = original.type,
                        operands = subs
                    };
                case FormulaType.Knows:
                case FormulaType.CommonKnow:
                    return new Formula {
                        type = original.type,
                        agent = original.agent,
                        child = SubstituteVariables(original.child, assignment)
                    };
                default:
                    return original;
            }
        }


        public static Formula Top()
        {
            return new Formula { type = FormulaType.Top };
        }

        public static Formula Bottom()
        {
            return new Formula { type = FormulaType.Bottom };
        }

        public static Formula Not(Formula f)
        {
            return new Formula { type = FormulaType.Not, child = f };
        }

        public static Formula Atom(Predicate p)
        {
            return new Formula { type = FormulaType.Atom, predicate = p };
        }

        public static Formula Atom(GroundPredicate gp)
        {
            return new Formula { type = FormulaType.Atom, groundPredicate = gp };
        }

        public static Formula And(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.And, leftChild = f1, rightChild = f2 };
        }

        public static Formula Or(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.Or, leftChild = f1, rightChild = f2 };
        }

        public static Formula Implies(Formula f1, Formula f2)
        {
            return new Formula { type = FormulaType.Implies, leftChild = f1, rightChild = f2 };
        }

        public static Formula Disjunction(ICollection<Formula> disjuncts)
        {
            return new Formula { type = FormulaType.Disjunction, operands = disjuncts };
        }

        public static Formula Conjunction(ICollection<Formula> conjuncts)
        {
            return new Formula { type = FormulaType.Conjunction, operands = conjuncts };
        }

        public static Formula Knows(Agent a, Formula f)
        {
            return new Formula { type = FormulaType.Knows, child = f, agent = a };
        }

        public static Formula CommonKnow(Formula f)
        {
            return new Formula { type = FormulaType.CommonKnow, child = f };
        }

        public static bool AreFormulasEqual(Formula f1, Formula f2)
        {
            if (f1.GetFormulaType() != f2.GetFormulaType())
                return false;

            switch (f1.GetFormulaType())
            {
                case FormulaType.Top:
                case FormulaType.Bottom:
                    return true;

                case FormulaType.Atom:
                    if (f1.groundPredicate != null && f2.groundPredicate != null)
                    {
                        return f1.groundPredicate.Equals(f2.groundPredicate);
                    }
                    return f1.predicate.Equals(f2.predicate);

                case FormulaType.Not:
                    return AreFormulasEqual(f1.child, f2.child);

                case FormulaType.And:
                case FormulaType.Or:
                case FormulaType.Implies:
                    return AreFormulasEqual(f1.leftChild, f2.leftChild) &&
                            AreFormulasEqual(f1.rightChild, f2.rightChild);

                case FormulaType.Disjunction:
                case FormulaType.Conjunction:
                    return f1.operands.Count == f2.operands.Count &&
                            f1.operands.Zip(f2.operands, AreFormulasEqual).All(equal => equal);

                case FormulaType.Knows:
                    return f1.agent == f2.agent &&
                            AreFormulasEqual(f1.child, f2.child);

                case FormulaType.CommonKnow:
                    return AreFormulasEqual(f1.child, f2.child);

                default:
                    return false;
            }
        }

        public override string ToString()
        {
            switch (this.type)
            {
                case FormulaType.Top:
                    return "⊤";
                case FormulaType.Bottom:
                    return "⊥";
                case FormulaType.Atom:
                    if (this.groundPredicate != null)
                        return this.groundPredicate.ToString();
                    else if (this.predicate != null)
                        return this.predicate.ToString();
                    else
                        return "Atom(?)";
                case FormulaType.Not:
                    return "¬(" + (child != null ? child.ToString() : "null") + ")";
                case FormulaType.And:
                    return "(" + (leftChild != null ? leftChild.ToString() : "null") + " ∧ " + (rightChild != null ? rightChild.ToString() : "null") + ")";
                case FormulaType.Or:
                    return "(" + (leftChild != null ? leftChild.ToString() : "null") + " ∨ " + (rightChild != null ? rightChild.ToString() : "null") + ")";
                case FormulaType.Implies:
                    return "(" + (leftChild != null ? leftChild.ToString() : "null") + " → " + (rightChild != null ? rightChild.ToString() : "null") + ")";
                case FormulaType.Disjunction:
                    return "(" + string.Join(" ∨ ", (operands ?? new List<Formula>()).Select(o => o.ToString())) + ")";
                case FormulaType.Conjunction:
                    return "(" + string.Join(" ∧ ", (operands ?? new List<Formula>()).Select(o => o.ToString())) + ")";
                case FormulaType.Knows:
                    return "K(" + (agent != null ? agent.ToString() : "null") + ", " + (child != null ? child.ToString() : "null") + ")";
                case FormulaType.CommonKnow:
                    return "C(" + (child != null ? child.ToString() : "null") + ")";
                default:
                    return base.ToString();
            }
        }
    }

    public class EvaluationResult
    {
        public bool Satisfied { get; set; }
        public Dictionary<string, Object> Assignment { get; set; }
    }
}
