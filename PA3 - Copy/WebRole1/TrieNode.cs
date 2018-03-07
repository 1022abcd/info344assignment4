using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class TrieNode
    {
        public bool EndOfWord
        {
            get;
            set;
        }
        public Dictionary<char, TrieNode> Children
        {
            get;
            set;
        }
        public TrieNode()
        {
            EndOfWord = false;
            Children = new Dictionary<char, TrieNode>();
        }

    }
}

