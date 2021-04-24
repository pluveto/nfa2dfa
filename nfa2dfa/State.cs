using System;

namespace nfa2dfa
{
    public class State : IComparable
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 别名（用于幂集重命名）
        /// </summary>
        public string Alias { get; set; }

        public State(string name)
        {
            this.Name = name;
        }
        public State(string name, string alias)
        {
            this.Name = name;
            this.Alias = alias;
        }
        public override string ToString()
        {
            var str = this.Name;
            if (Alias != default)
            {
                str += "({" + Alias + "})";
            }
            return str;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.ToString() == obj.ToString();
        }

        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }

        internal bool AliasContains(State state)
        {
            if (default == Alias) return false;
            var spl = Alias.Split(",");
            foreach (var str in spl)
            {
                if (str == state.Name) { return true; }
            }
            return false;
        }
    }
}