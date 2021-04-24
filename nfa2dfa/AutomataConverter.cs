using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace nfa2dfa
{
    public class AutomataConverter
    {
        /// <summary>
        /// 将 NFA(可带空转) 转换为 DFA
        /// </summary>
        /// <param name="eNfa">NFA</param>
        /// <returns></returns>
        public static Automata EpsilonNfaToDFA(Automata eNfa)
        {
            // 首先将 eNFA 转换为 NFA
            var nfaTransitionFunctions = RemoveEpsilonTransitions(eNfa);
            Debug.WriteLine("New transition functions:");
            Debug.WriteLine(string.Join("\n", nfaTransitionFunctions));

            var dfa = new AutomataBuilder()
                .SetInputSymbols(eNfa.InputSymbols.ToArray())
                .Build();

            // 获取初始转移函数
            var initalTransitionFunction = TransitionFunction.GetInitalTransitionFunction(nfaTransitionFunctions, eNfa.InitialState);
            if (0 == initalTransitionFunction.Count)
            {
                throw new Exception("Initial transition function not found!");
            }
            var dfaTransitionFunctions = NfaToDfaTransitionFunctions(nfaTransitionFunctions, dfa.InputSymbols, initalTransitionFunction);
            var stateMap = TransitionFunction.MergeStates(dfaTransitionFunctions);
            dfa.States = new SortedSet<State>();
            foreach (var kv in stateMap)
            {
                dfa.States.Add(new State(kv.Value.Name, kv.Key));
            }
            SortedSet<State> finalStates = GetFinialStates(eNfa, dfa);
            dfa.FinalStates = finalStates;
            dfa.InitialState = stateMap[eNfa.InitialState.Name];

            var mergedFunctions = TransitionFunction.MergeFunctions(dfaTransitionFunctions, stateMap);
            dfa.TransitionFunctions = mergedFunctions;

            Debug.WriteLine("DFA transition functions:");
            Debug.WriteLine(string.Join("\n", dfaTransitionFunctions));

            return dfa;
        }

        private static SortedSet<State> GetFinialStates(Automata eNfa, Automata dfa)
        {
            var finalStates = new SortedSet<State>();
            foreach (var finalState in eNfa.FinalStates)
            {
                foreach (var state in dfa.States)
                {
                    if (state.AliasContains(finalState))
                    {
                        finalStates.Add(state);
                    }
                }
            }

            return finalStates;
        }

        /// <summary>
        /// 使用一个队列辅助进行幂集构造，求出 DFA 的转移函数
        /// </summary>
        /// <param name="nfaTransitionFunctions">NFA 的转移函数</param>
        /// <param name="inputs">字母表</param>
        /// <param name="initalTransitionFunction">初始转移函数</param>
        /// <returns></returns>
        private static HashSet<TransitionFunction> NfaToDfaTransitionFunctions(
            HashSet<TransitionFunction> nfaTransitionFunctions,
            SortedSet<char> inputs,
            HashSet<TransitionFunction> initalTransitionFunction)
        {
            var dfaTransitionFunctionsQueue = new Queue<TransitionFunction>();
            var dfaTransitionFunctions = new HashSet<TransitionFunction>();


            foreach (var func in initalTransitionFunction)
            {
                dfaTransitionFunctionsQueue.Enqueue(func);
            }
            while (dfaTransitionFunctionsQueue.Count != 0)
            {
                var func = dfaTransitionFunctionsQueue.Dequeue();
                dfaTransitionFunctions.Add(func);
                foreach (var input in inputs)
                {
                    if (func.Input != input) continue;

                    var nextStates = func.NextStates;
                    foreach (var enumInput in inputs)
                    {
                        if (enumInput == '$') continue;
                        var nextNext = TransitionFunction.GetNextStatesOf(
                            nfaTransitionFunctions,
                            enumInput,
                            nextStates
                        );
                        var newFunc = new TransitionFunction(
                            nextStates,
                            enumInput,
                            nextNext
                        );

                        if (TransitionFunction.HasOld(dfaTransitionFunctions, newFunc)) continue;
                        dfaTransitionFunctionsQueue.Enqueue(newFunc);
                    }

                }
            }

            return dfaTransitionFunctions;
        }

        /// <summary>
        /// 将 eNfa 转换为 Nfa
        /// </summary>
        /// <param name="eNfa"></param>
        /// <returns></returns>
        private static HashSet<TransitionFunction> RemoveEpsilonTransitions(Automata eNfa)
        {
            var nfaTransitionFunctions = new HashSet<TransitionFunction>();
            foreach (var state in eNfa.States)
            {
                var eclosure = eNfa.GetEpsilonClosureOf(state);
                foreach (var eclosureState in eclosure)
                {
                    var acceptableInputs = eNfa.GetAcceptableInputOf(eclosureState, true);
                    foreach (var input in acceptableInputs)
                    {
                        var func = new TransitionFunction(eclosure, input, eNfa.GetNextStatesOf(eclosure, input));
                        nfaTransitionFunctions.Add(func);
                    }
                }
            }

            return nfaTransitionFunctions;
        }
    }
}