using System;
using System.Collections.Generic;
using System.Linq;

namespace nfa2dfa
{
    /// <summary>
    /// 转移函数
    /// </summary>
    public class TransitionFunction
    {
        public SortedSet<State> PresentStates { get; set; }
        public char Input { get; set; }
        public SortedSet<State> NextStates { get; set; }

        public TransitionFunction(SortedSet<State> presentStates, char input, SortedSet<State> nextStates)
        {
            this.PresentStates = presentStates;
            this.Input = input;
            this.NextStates = nextStates;
        }
        public TransitionFunction(State presentState, char input, SortedSet<State> nextStates)
        {
            this.PresentStates = new SortedSet<State>();
            this.PresentStates.Add(presentState);
            this.Input = input;
            this.NextStates = nextStates;
        }
        public TransitionFunction(State presentState, char input, State nextState)
        {
            this.PresentStates = new SortedSet<State>();
            this.PresentStates.Add(presentState);
            this.Input = input;
            this.NextStates = new SortedSet<State>();
            this.NextStates.Add(nextState);
        }
        /// <summary>
        /// 以字符串形式显示转移函数
        /// </summary>
        /// <returns>形如 delta({a}, '0') = {b, c} 的列表项</returns>
        public override string ToString()
        {
            var input = Input.ToString();
            if (input == "$")
            {
                input = "epsilon";
            }
            return string.Format("delta({{{0}}}, '{1}') = {{{1}}}", StatesToString(PresentStates), input, StatesToString(NextStates));
        }

        public static Dictionary<string, State> MergeStates(HashSet<TransitionFunction> functions)
        {
            var num = 0;
            var aliasDictionary = new Dictionary<string, State>();
            foreach (var func in functions)
            {
                var presentName = StatesToString(func.PresentStates);
                if (!aliasDictionary.ContainsKey(presentName))
                {
                    aliasDictionary.Add(presentName, new State("q" + num));
                    num++;
                }
                var nextName = StatesToString(func.NextStates);
                if (!aliasDictionary.ContainsKey(nextName))
                {
                    aliasDictionary.Add(nextName, new State("q" + num));
                    num++;
                }
            }
            return aliasDictionary;
        }

        public static HashSet<TransitionFunction> MergeFunctions(HashSet<TransitionFunction> dfaTransitionFunctions, Dictionary<string, State> stateMap)
        {
            var set = new HashSet<TransitionFunction>();
            foreach (var func in dfaTransitionFunctions)
            {
                var presentName = StatesToString(func.PresentStates);
                var nextName = StatesToString(func.NextStates);
                set.Add(new TransitionFunction(stateMap[presentName], func.Input, stateMap[nextName]));
            }
            return set;
        }

        private static string StatesToString(SortedSet<State> states)
        {
            return string.Join(",", states);
        }

        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
        /// <summary>
        /// 判断转移函数是否重复
        /// </summary>
        /// <param name="dfaTransitionFunctions"></param>
        /// <param name="newFunc"></param>
        /// <returns></returns>

        public static bool HasOld(HashSet<TransitionFunction> dfaTransitionFunctions, TransitionFunction newFunc)
        {
            foreach (var func in dfaTransitionFunctions)
            {
                if (func.Input == newFunc.Input && func.PresentStates.SetEquals(newFunc.PresentStates))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 获取转移函数在某个字符输入下所有可以抵达的状态构成的集合
        /// </summary>
        /// <param name="nfaTransitionFunctions"></param>
        /// <param name="input"></param>
        /// <param name="currentStates"></param>
        /// <returns></returns>
        public static SortedSet<State> GetNextStatesOf(HashSet<TransitionFunction> nfaTransitionFunctions, char input, SortedSet<State> currentStates)
        {
            var list = new SortedSet<State>();
            foreach (var func in nfaTransitionFunctions)
            {
                if (func.Input != input)
                {
                    continue;
                }
                if (func.PresentStates.Count == 1 && currentStates.Contains(func.PresentStates.First()))
                {
                    foreach (var state in func.NextStates)
                    {
                        list.Add(state);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 获取初始状态对应的每隔字符的转移
        /// </summary>
        /// <param name="transitionFunctions"></param>
        /// <param name="initialState"></param>
        /// <returns></returns>
        public static HashSet<TransitionFunction> GetInitalTransitionFunction(HashSet<TransitionFunction> transitionFunctions, State initialState)
        {
            var list = new HashSet<TransitionFunction>();
            foreach (var func in transitionFunctions)
            {
                if (func.PresentStates.Count == 1 && func.PresentStates.First() == initialState)
                {
                    list.Add(func);
                }
            }
            return list;
        }

    }
}