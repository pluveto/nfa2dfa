using System;
using System.Collections.Generic;
using System.Linq;

namespace nfa2dfa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // 该例子选自：https://www.youtube.com/watch?v=xs6AOzseZjM&ab_channel=KuhelikaPuja
            // Example1();

            // 该例子来自：https://hritikbhandari.github.io/NFA-to-DFA-Converter/
            // Example2();

            // 该例子来自：课堂 PPT 子集构造法举例
            if(args.Count() == 1)
            {
                LoadFile(args[0]);
            }
            else
            {
                Console.WriteLine("There is no file specified, so this program will take the PPT's example.");
                Example3();
            }
            Console.ReadKey();
        }

        private static void LoadFile(string fileName)
        {
            Console.WriteLine("Loading from file: {0}", fileName);
            if (!System.IO.File.Exists(fileName))
            {
                Console.WriteLine("No such file!");
            }
            var eNfa = AutomataBuilder.FromFile(fileName);
            Console.WriteLine("Loaded.");
            Console.WriteLine("=== NFA Primitive Infomation ===");
            Console.WriteLine(eNfa.About());
            Console.WriteLine("Epsilon Closures: ");
            foreach (var state in eNfa.States)
            {
                var eclosure = eNfa.GetEpsilonClosureOf(state);
                Console.WriteLine("{0}: {{{1}}}", state, string.Join(',', eclosure));
            }
            Console.WriteLine("\nConverted.\n");

            Console.WriteLine("=== DFA Infomation ===");
            var dfa = AutomataConverter.EpsilonNfaToDFA(eNfa);
            Console.WriteLine(dfa.About());
        }

        private static void Example1()
        {
            var nfa = new AutomataBuilder()
                            .SetStates(new string[] { "A", "B", "C", "D", "E" })
                            .SetInputSymbols(new char[] { '0', '1', '$' })
                            .SetTransitionFunction(new string[] {
                                        "f(A,0)=B",
                                        "f(A,$)=C",
                                        "f(B,$)=D",
                                        "f(C,$)=D",
                                        "f(C,1)=E",
                                        "f(D,0)=D",
                                        "f(D,1)=E",
                                                  })
                            .SetInitialState("A")
                            .SetFinalState("E")
                            .Build();
            Console.WriteLine("=== NFA Primitive Infomation ===");
            Console.WriteLine(nfa.About());
            Console.WriteLine("Epsilon Closures: ");
            foreach (var state in nfa.States)
            {
                var eclosure = nfa.GetEpsilonClosureOf(state);
                Console.WriteLine("{0}: {{{1}}}", state, string.Join(',', eclosure));
            }

            var transitionFunctions = new SortedSet<TransitionFunction>();
            foreach (var state in nfa.States)
            {
                var eclosure = nfa.GetEpsilonClosureOf(state);
                foreach (var eclosureState in eclosure)
                {
                    var acceptableInputs = nfa.GetAcceptableInputOf(eclosureState, true);
                    foreach (var input in acceptableInputs)
                    {
                        var func = (new TransitionFunction(eclosure, input, nfa.GetNextStatesOf(eclosure, input)));
                        transitionFunctions.Add(func);
                    }
                }
            }
            Console.WriteLine("New transition functions:");
            Console.WriteLine(string.Join("\n", transitionFunctions));
        }
        private static void Example2()
        {
            var nfa = new AutomataBuilder()
                            .SetStates(new string[] { "q0", "q1", "q2" })
                            .SetInputSymbols(new char[] { 'a', 'b', '$' })
                            .SetTransitionFunction(new string[] {
                                                    "f(q0,a)=q1",
                                                    "f(q0,$)=q1",
                                                    "f(q1,a)=q0",
                                                    "f(q1,b)=q1",
                                                    "f(q1,a)=q2",
                                                    "f(q1,b)=q2",
                                                    "f(q2,a)=q2",
                                                    "f(q2,b)=q1",
                                                    "f(q2,b)=q1",
                                                  })
                            .SetInitialState("q0")
                            .SetFinalState("q1")
                            .Build();
            Console.WriteLine("=== NFA Primitive Infomation ===");
            Console.WriteLine(nfa.About());
            Console.WriteLine("Epsilon Closures: ");
            foreach (var state in nfa.States)
            {
                var eclosure = nfa.GetEpsilonClosureOf(state);
                Console.WriteLine("{0}: {{{1}}}", state, string.Join(',', eclosure));
            }

            var transitionFunctions = new HashSet<TransitionFunction>();
            foreach (var state in nfa.States)
            {
                var eclosure = nfa.GetEpsilonClosureOf(state);
                foreach (var eclosureState in eclosure)
                {
                    var acceptableInputs = nfa.GetAcceptableInputOf(eclosureState, true);
                    foreach (var input in acceptableInputs)
                    {
                        var func = (new TransitionFunction(eclosure, input, nfa.GetNextStatesOf(eclosure, input)));
                        transitionFunctions.Add(func);
                    }
                }
            }
            Console.WriteLine("New transition functions:");
            Console.WriteLine(string.Join("\n", transitionFunctions));
        }

        private static void Example3()
        {
            var eNfa = new AutomataBuilder()
                   .SetStates(new string[] { "p", "q", "r" })
                   .SetInputSymbols(new char[] { '0', '1', '$' })
                   .SetTransitionFunction(new string[] {
                                                    "f(p,0)=p",
                                                    "f(p,1)={p,q}",
                                                    "f(q,0)=r",
                                                    "f(q,1)=r",
                                         })
                   .SetInitialState("p")
                   .SetFinalState("r")
                   .Build();
            Console.WriteLine("=== NFA Primitive Infomation ===");
            Console.WriteLine(eNfa.About());
            Console.WriteLine("Epsilon Closures: ");
            foreach (var state in eNfa.States)
            {
                var eclosure = eNfa.GetEpsilonClosureOf(state);
                Console.WriteLine("{0}: {{{1}}}", state, string.Join(',', eclosure));
            }
            Console.WriteLine("\nConverted.\n");

            Console.WriteLine("=== DFA Infomation ===");
            var dfa = AutomataConverter.EpsilonNfaToDFA(eNfa);
            Console.WriteLine(dfa.About());
        }


    }
}
