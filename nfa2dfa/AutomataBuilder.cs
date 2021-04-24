using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace nfa2dfa
{
    public class AutomataBuilder
    {
        private Automata instance;
        public AutomataBuilder()
        {
            this.instance = new Automata();
            this.instance.FinalStates = new SortedSet<State>();
            this.instance.States = new SortedSet<State>();
        }
        public AutomataBuilder SetStates(string[] stateNames)
        {
            this.instance.States.Clear();
            foreach (var name in stateNames)
            {
                this.instance.States.Add(new State(name));
            }
            return this;
        }
        public AutomataBuilder SetStates(string stateNamesString)
        {
            var stateNames = stateNamesString.Replace("{", "").Replace("}", "").Split(",");
            this.instance.States.Clear();
            foreach (var name in stateNames)
            {
                this.instance.States.Add(new State(name.Trim()));
            }
            return this;
        }
        public AutomataBuilder SetInputSymbols(char[] symbols)
        {
            this.instance.InputSymbols = new SortedSet<char>();
            foreach (var symbol in symbols)
            {
                this.instance.InputSymbols.Add(symbol);

            }
            return this;
        }
        public AutomataBuilder SetInputSymbols(string symbolsString)
        {
            var symbols = symbolsString.Replace("{", "").Replace("}", "").Split(",");
            this.instance.InputSymbols = new SortedSet<char>();
            foreach (var symbol in symbols)
            {
                this.instance.InputSymbols.Add(symbol[0]);
            }
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionExpressions">形如：f(a,0)={1,2}</param>
        /// <returns></returns>
        public AutomataBuilder SetTransitionFunction(string[] functionExpressions)
        {
            var list = new HashSet<TransitionFunction>();
            foreach (var functionExpression in functionExpressions)
            {
                var lr = functionExpression.Split('='); // 分割后：[0]=f(a,0)  [1]={1,2}                
                var tmpl = lr[0].Split('(')[1].Split(')')[0].Split(',');//a,0
                var presentStateName = tmpl[0];
                var presentState = this.instance.GetStateByName(presentStateName);
                if (null == presentState)
                {
                    throw new Exception("Present state not found by name: " + presentStateName);
                }
                var inputChar = tmpl[1][0];

                var nextStateNames = lr[1].Replace("{", "").Replace("}", "").Split(',');
                var nextStates = new SortedSet<State>();
                foreach (var name in nextStateNames)
                {
                    var state = this.instance.GetStateByName(name);
                    if (null == state)
                    {
                        throw new Exception("State not found by name: " + name);
                    }
                    nextStates.Add(state);
                }
                // 先寻找 presentState 和 inputChar 相同的，找到就合并
                var foundOld = false;
                foreach (var tf in list)
                {
                    if (tf.PresentStates.First() == presentState && tf.Input == inputChar)
                    {
                        foundOld = true;
                        foreach (var nextState in nextStates)
                        {
                            tf.NextStates.Add(nextState);
                        }
                    }
                }
                if (!foundOld)
                {
                    list.Add(new TransitionFunction(presentState, inputChar, nextStates));
                }
            }
            this.instance.TransitionFunctions = list;
            return this;
        }

        public static Automata FromFile(string fileName)
        {
            var str = System.IO.File.ReadAllText(fileName);
            return FromString(str);
        }

        public static Automata FromString(string input)
        {
            string pattern = @".*NFA(.*)where(.*)and(.*)by(.*)";
            RegexOptions options = RegexOptions.Singleline;
            Match m = Regex.Match(input, pattern, options);
            // Console.WriteLine("'{0}' found at index {1}", m.Value, m.Index);
            // Group 2: where ... and
            var g2 = m.Groups[2];
            var raw1 = g2.Value.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("{", "").Replace("}", "");
            var lines1 = raw1.Split('\n');

            var dict = new Dictionary<string, string>();
            foreach (var line in lines1)
            {
                if (line.Trim().Length == 0) continue;
                var kv = line.Split("=");
                if (kv.Length != 2) {
                    throw new Exception("Invalid line: " + line);
                }
                var k = kv[0].ToLower().Trim();
                dict.Add(k, kv[1]);
            }
            foreach (var key in new string[] {"q","t","q0","f" })
            {
                if (!dict.ContainsKey(key))
                {
                    throw new Exception("Missing automata argument: " + key);
                }
            }
          
            // Group 4: by ...
            var g4 = m.Groups[4];
            var raw2 = g4.Value.Replace(" ", "").Replace("\t", "").Replace("\r", "").Replace("{", "").Replace("}", "");
            var lines2 = raw2.Split('\n');
            var exps = new List<string>();
            foreach (var line in lines2)
            {
                if (line.Trim().Length == 0) continue;
                exps.Add(line.Trim());
            }

            // build
            var builder = new AutomataBuilder();

            var automata = builder
                .SetStates(dict["q"])
                .SetInputSymbols(dict["t"])
                .SetInitialState(dict["q0"])
                .SetFinalState(dict["f"])
                .SetTransitionFunction(exps.ToArray())
                .Build();
            return automata;
        }

        public AutomataBuilder SetInitialState(string name)
        {
            var state = this.instance.GetStateByName(name);
            if (null == state)
            {
                throw new Exception("Initial State not found by name: " + name);
            }
            this.instance.InitialState = state;
            return this;
        }
        public AutomataBuilder SetFinalState(string name)
        {
            this.instance.FinalStates.Clear();
            var state = this.instance.GetStateByName(name);
            if (null == state)
            {
                throw new Exception("Final State not found by name: " + name);
            }
            this.instance.FinalStates.Add(state);
            return this;
        }
        public Automata Build()
        {
            return instance;
        }
    }
}
