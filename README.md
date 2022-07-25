# Implicit Coordination with Forward Induction

This repository contains an implementation of a planner for multi-agent cooperative tasks under implicit coordination. What is novel about this planner is the incorporation of forward induction reasoning, which allows agents to infer additional knowledge from observing how other agents act and the assumption of their optimality. The planner builds upon Dynamic Epistemic Logic to find *implicitly coordinated* plans, that is, plans where the agents reason about the knowledge and capabilities of the other agents in the planning task, thus reducing the need for negotiating plans.

## Code overview

The source code consists of the following two main modules:

- `DEL` - Contains the models of the concepts from Dynamic Epistemic Logic, the theoretical framework upon which we build our solution
- `Planning` - Contains models for the necessary components for plannning with forward induction: generating the search tree, computing costs and pruning. These components are used by the `ForwardInductionPlanner` class. Moreover, it contains an implementation of the planner described in the paper *DEL-based Epistemic Planning for Human-Robot Collaboration: Theory and Implementation* (without ordered partition refinement), used as a baseline in our experiments. This module contains the `LeverTaskInitializer` class, which generates lever planning tasks with custom number of positions and starting position of the lever. This planning task is presented in *Token-Based execution Semantics for Multi-Agent Epistemic Planning*.
