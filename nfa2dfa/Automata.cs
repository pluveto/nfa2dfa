using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nfa2dfa
{
    /// <summary>
    /// 状态机
    /// </summary>
    public class Automata
    {
        /// <summary>
        /// 所有状态
        /// </summary>
        public SortedSet<State> States { get; set; }
        /// <summary>
        /// 字母表
        /// </summary>
        public SortedSet<char> InputSymbols { get; set; }
        /// <summary>
        /// 转移函数表
        /// </summary>
        public HashSet<TransitionFunction> TransitionFunctions { get; set; }
        /// <summary>
        /// 初始状态
        /// </summary>
        public State InitialState { get; set; }
        /// <summary>
        /// 结束状态
        /// </summary>
        public SortedSet<State> FinalStates { get; set; }

        public string About()
        {
            var sb = new StringBuilder();
            sb.AppendLine("All States:    " + string.Join(',', this.States));
            sb.AppendLine("Input Symbols: " + string.Join(',', this.InputSymbols));
            sb.AppendLine("Transition Function:\n               " + string.Join("\n               ", this.TransitionFunctions));
            sb.AppendLine("Initial State: " + this.InitialState);
            sb.AppendLine("Final State:   " + string.Join(',',this.FinalStates));
            return sb.ToString();
        }


        /// <summary>
        /// 寻找状态的 epsilon 闭包
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <returns></returns>
        public SortedSet<State> GetEpsilonClosureOf(State state, bool includeSelf = true)
        {
            var list = new SortedSet<State>();
            if (includeSelf)
            {
                list.Add(state);
            }
            var nextStates = GetNextStatesOf(state, '$');
            foreach (var nextState in nextStates)
            {
                list.Add(nextState);
                var enclosureStates = GetEpsilonClosureOf(nextState, false);
                foreach (var enclosureState in enclosureStates)
                {
                    list.Add(enclosureState);
                }
            }
            return list;
        }

        public State GetStateByName(string name)
        {
            foreach (var state in this.States)
            {
                if(state.Name == name)
                {
                    return state;
                }
            }
            return null;
        }

        public List<char> GetAcceptableInputOf(State eclosureState, bool excludeEpsilon)
        {
            var ret = new List<char>();
            foreach (var func in TransitionFunctions)
            {
                if(func.PresentStates.First() == eclosureState)
                {
                    if (excludeEpsilon)
                    {
                        if (func.Input != '$')
                        {
                            ret.Add(func.Input);
                        }
                    }
                    else
                    {
                        ret.Add(func.Input);
                    }
                    
                }
            }
            return ret;
        }

        /// <summary>
        /// 寻找单个状态的下一个状态  
        /// </summary>
        /// <param name="state">当前状态</param>
        /// <param name="input">如果填写，则下一个状态必须是输入为此字符时的</param>
        /// <returns></returns>
        private List<State> GetNextStatesOf(State state, char input = '\0')
        {
            var list = new List<State>();

            foreach (var func in TransitionFunctions)
            {
                // 如果不限定输入，则返回所有下一个状态
                if ('\0' == input)
                {
                    if (func.PresentStates.First() == state)
                    {
                        list.AddRange(func.NextStates);
                    }
                }
                // 否则限定返回输入为 input 的下一个状态
                else
                {
                    if (func.PresentStates.First() == state && func.Input == input)
                    {
                        list.AddRange(func.NextStates);
                    }
                }
            }
            return list;
        }
        /// <summary>
        /// 寻找多个状态的下一个状态。将会对当前状态遍历，每个当前状态的下一所有状态都会添加到返回值中
        /// </summary>
        /// <param name="presentStates">当前的状态列表</param>
        /// <param name="input">输入限制</param>
        /// <returns></returns>
        public SortedSet<State> GetNextStatesOf(SortedSet<State> presentStates, char input)
        {
            var list = new SortedSet<State>();
            foreach (var func in TransitionFunctions)
            {
                foreach (var presentState in presentStates)
                {
                    if (func.PresentStates.First() == presentState && func.Input == input)
                    {
                        foreach (var nextState in func.NextStates)
                        {
                            list.Add(nextState);
                        }
                    }
                }
            }
            
            return list;
        }
    }
}
