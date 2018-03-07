
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole1
{
    public class TrieTree
    {
        public TrieNode mainRoot;

        //Tree constructor to when a new tree is declared
        public TrieTree()
        {
            mainRoot = new TrieNode();
        }

        public void Insert(string word)
        {
            TrieNode curr = mainRoot;
            for (int i = 0; i < word.Length; i++)
            {
                if (curr.Children.ContainsKey(word[i]))
                {
                    curr = curr.Children[word[i]];
                }
                else
                {
                    BuildTrie(word.Substring(i), curr);
                    break;
                }
            }
        }

        private void BuildTrie(string word, TrieNode current)
        {

            if (current.Children == null)
            {
                current.Children = new Dictionary<char, TrieNode>();
            }
            if (word.Length == 1)
            {
                //current.EndOfWord = true;
                current.Children.Add(word[0], new TrieNode());
                current = current.Children[word[0]];
                current.EndOfWord = true;
            }
            else
            {
                current.Children.Add(word[0], new TrieNode());
                BuildTrie(word.Substring(1), current.Children[word[0]]);
            }
        }

        public List<string> Search(string word)
        {
            List<string> result = new List<string>();
            TrieNode current = mainRoot;
            for (int i = 0; i < word.Length; i++)
            {
                if (current.Children.ContainsKey(word[i]))
                {
                    current = current.Children[word[i]];
                }
                else
                {
                    return new List<string>();
                }
            }
            result = SearchHelper(current, result, word);
            return result;
        }

        private List<string> SearchHelper(TrieNode current, List<string> result, string pathTerm)
        {
            if (result.Count() >= 10)
            {
                return result;
            }
            else if (current.Children != null)
            {
                if (current.EndOfWord)
                {
                    if (!result.Contains(pathTerm))
                    {
                        result.Add(pathTerm);
                    }
                }
                foreach (var key in current.Children.Keys)
                {
                    SearchHelper(current.Children[key], result, pathTerm + key.ToString());
                }
                return result;
            }
            else
            {
                if (current.EndOfWord)
                {
                    if (!result.Contains(pathTerm))
                    {
                        result.Add(pathTerm);
                    }
                }
                return result;
            }
        }
    }
}
